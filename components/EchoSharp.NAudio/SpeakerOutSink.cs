// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;
using EchoSharp.Audio;
using EchoSharp.Config;
using NAudio.Wave;

namespace EchoSharp.NAudio;

public sealed class SpeakerOutSink(int deviceNumber = -1, float volume = 1) : IAudioSink
{
    private WaveOutEvent? waveOut;
    private PooledBufferedSampleProvider? bufferedSampleProvider;

    public Task Initialize(AudioHeader audioHeader, double? duration)
    {
        var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(
            (int)audioHeader.SampleRate,
            audioHeader.Channels);

        bufferedSampleProvider = new PooledBufferedSampleProvider(waveFormat);
        waveOut = new WaveOutEvent
        {
            DeviceNumber = deviceNumber,
            Volume = volume
        };
        waveOut.Init(bufferedSampleProvider);
        waveOut.Play();

        return Task.CompletedTask;
    }

    public Task WriteAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default)
    {
        if (bufferedSampleProvider == null)
        {
            throw new InvalidOperationException("SpeakerOutSink not initialized. Call Initialize first.");
        }

        bufferedSampleProvider.AddSamples(samples);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        waveOut?.Dispose();
        waveOut = null;
        bufferedSampleProvider = null;
    }

    /// <summary>
    /// A buffered sample provider that uses fixed–size buffers from the ArrayPool.
    /// Each added chunk is split into segments, and when segments are consumed,
    /// their buffers are returned to the pool.
    /// </summary>
    /// <remarks>
    /// Creates a new instance using the specified wave format and chunk size.
    /// </remarks>
    /// <param name="waveFormat">The audio format.</param>
    /// <param name="chunkSize">
    /// The fixed size for each rented buffer (in number of floats). Default is 8192.
    /// </param>
    public class PooledBufferedSampleProvider(WaveFormat waveFormat, int chunkSize = 8192) : ISampleProvider
    {
        private readonly WaveFormat waveFormat = waveFormat;
        private readonly Queue<BufferSegment> segments = [];
#if NET9_0_OR_GREATER
        private readonly Lock syncRoot = new();
#else
        private readonly object syncRoot = new();
#endif

        private readonly int chunkSize = chunkSize;

        private struct BufferSegment
        {
            public float[] Buffer;
            public int ReadOffset;
            public int Length;
        }

        public WaveFormat WaveFormat => waveFormat;

        /// <summary>
        /// Reads up to <paramref name="count"/> samples from the pooled buffers.
        /// When a segment is fully consumed, its buffer is returned to the ArrayPool.
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            var samplesRead = 0;
            lock (syncRoot)
            {
                while (count > 0 && segments.Count > 0)
                {
                    var segment = segments.Peek();
                    var available = segment.Length - segment.ReadOffset;
                    var toCopy = Math.Min(count, available);

                    // Use block copy.
                    Array.Copy(segment.Buffer, segment.ReadOffset, buffer, offset, toCopy);

                    segment.ReadOffset += toCopy;
                    offset += toCopy;
                    count -= toCopy;
                    samplesRead += toCopy;

                    // When the segment is fully read, return its buffer.
                    if (segment.ReadOffset >= segment.Length)
                    {
                        segments.Dequeue();
                        ArrayPool<float>.Shared.Return(segment.Buffer, ArrayPoolConfig.ClearOnReturn);
                    }
                }
            }
            return samplesRead;
        }

        /// <summary>
        /// Adds samples to the provider by splitting them into fixed–size chunks
        /// and renting arrays from the ArrayPool.
        /// </summary>
        public void AddSamples(ReadOnlyMemory<float> samples)
        {
            lock (syncRoot)
            {
                var remaining = samples.Length;
                var sampleOffset = 0;
                while (remaining > 0)
                {
                    var toCopy = Math.Min(remaining, chunkSize);
                    // Rent a buffer that is at least as large as needed.
                    var buffer = ArrayPool<float>.Shared.Rent(toCopy);

                    // Copy the block of samples into the rented buffer.
                    samples.Slice(sampleOffset, toCopy)
                           .CopyTo(buffer.AsMemory(0, toCopy));

                    var segment = new BufferSegment
                    {
                        Buffer = buffer,
                        ReadOffset = 0,
                        Length = toCopy
                    };
                    segments.Enqueue(segment);
                    sampleOffset += toCopy;
                    remaining -= toCopy;
                }
            }
        }
    }
}
