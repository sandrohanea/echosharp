// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Internals;
using EchoSharp.Utils;

namespace EchoSharp.Audio.Sink;

/// <summary>
/// Base class for audio sinks that handles WAV format parsing and sample conversion.
/// </summary>
public abstract class WaveDecoderSink : IAudioSink
{
    private MergedMemoryChunks? pendingData;
    private bool isInitialized;
    private AudioHeader? audioHeader;
    private bool isDisposed;

    /// <summary>
    /// Initializes the audio sink with the audio header and the duration of the audio stream.
    /// </summary>
    /// <remarks>
    /// The duration is optional and can be null.
    /// This method can be called directly to initialize the sink, or it will be called automatically
    /// when the first WAV data with header is written.
    /// </remarks>
    public async Task Initialize(AudioHeader audioHeader, double? duration)
    {
        ThrowIfDisposed();

        if (isInitialized)
        {
            if (!audioHeader.Equals(this.audioHeader))
            {
                throw new InvalidOperationException("Cannot initialize with different audio header");
            }
            return;
        }

        this.audioHeader = audioHeader;
        await InitializeInternalAsync(audioHeader, duration);
        isInitialized = true;
    }

    /// <summary>
    /// When overridden in a derived class, performs the actual initialization.
    /// </summary>
    protected abstract Task InitializeInternalAsync(AudioHeader audioHeader, double? duration);

    /// <summary>
    /// Writes audio data to the sink (append only).
    /// </summary>
    /// <remarks>
    /// The samples are written as a sequence of 32-bit floating-point values in the range [-1.0, 1.0].
    /// The number of samples must be a multiple of the number of channels.
    /// In order to use this method, the stream must be initialized first.
    /// </remarks>
    public Task WriteAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (!isInitialized)
        {
            throw new InvalidOperationException("Sink must be initialized before writing samples");
        }
        return WriteInternalAsync(samples, cancellationToken);
    }

    /// <summary>
    /// When overridden in a derived class, performs the actual writing of samples.
    /// </summary>
    protected abstract Task WriteInternalAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken);

    /// <summary>
    /// Writes raw audio data to the sink (append only) (wav format)
    /// </summary>
    /// <remarks>
    /// The data is written as a sequence of bytes.
    /// If the sink is not initialized, the first write must include a valid WAV header.
    /// If the sink is already initialized, the data is treated as raw PCM data.
    /// </remarks>
    public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!isInitialized)
        {
            // If we haven't initialized yet, we need to parse the WAV header
            if (pendingData == null)
            {
                pendingData = new MergedMemoryChunks(buffer);
            }
            else
            {
                pendingData.AddChunk(buffer);
            }

            // Try to parse the WAV header if we have enough data
            if (pendingData.Length >= 44) // Minimum WAV header size
            {
                var parseResult = WaveFileUtils.ParseHeader(pendingData);
                if (parseResult.IsSuccess && parseResult.Header.HasValue)
                {
                    // Calculate duration if we have the data chunk size
                    double? duration = null;
                    if (parseResult.DataChunkSize > 0)
                    {
                        var bytesPerSecond = parseResult.Header.Value.SampleRate *
                            parseResult.Header.Value.Channels *
                            (parseResult.Header.Value.BitsPerSample / 8.0);
                        duration = parseResult.DataChunkSize / bytesPerSecond;
                    }

                    // Initialize using the parsed header
                    await Initialize(parseResult.Header.Value, duration);

                    // Process any remaining data after the header
                    if (pendingData.Length > parseResult.DataOffset)
                    {
                        // Skip to the data offset
                        pendingData.TrySkip((uint)parseResult.DataOffset);

                        // Process the remaining data in chunks
                        while (pendingData.Position < pendingData.Length)
                        {
                            var chunkSize = (int)Math.Min(pendingData.Length - pendingData.Position, 4096);
                            var chunk = pendingData.GetChunk(chunkSize);
                            await ProcessPcmData(chunk, cancellationToken);
                        }
                        pendingData = null;
                    }
                    else
                    {
                        pendingData = null;
                    }
                }
                else if (parseResult.IsCorrupt || parseResult.IsNotSupported)
                {
                    throw new InvalidDataException(parseResult.ErrorMessage ?? "Invalid WAV file");
                }
            }
        }
        else
        {
            // Already initialized, process as PCM data
            await ProcessPcmData(buffer, cancellationToken);
        }
    }

    private async Task ProcessPcmData(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        var samples = SampleSerializer.Deserialize(buffer, audioHeader!.Value.BitsPerSample);
        await WriteInternalAsync(samples, cancellationToken);
    }

    /// <summary>
    /// Throws if the object has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(isDisposed, this);
#else
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(WaveDecoderSink));
        }
#endif
    }

    /// <summary>
    /// Disposes the audio sink and releases any pending data.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (isDisposed)
        {
            return;
        }

        if (pendingData != null)
        {
            // If we have pending data that hasn't been processed, we should log or handle it
            // This could be an error condition or just incomplete data
            pendingData = null;
        }

        await DisposeInternalAsync();
        isDisposed = true;
    }

    /// <summary>
    /// When overridden in a derived class, releases the unmanaged resources used by the audio sink.
    /// </summary>
    protected virtual ValueTask DisposeInternalAsync()
    {
        return EchoValueTask.Completed;
    }
}
