// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;
using NAudio.Dsp;

namespace EchoSharp.NAudio;
public sealed class ResamplerAudioSource : IAudioSource
{
    private readonly IAudioSource source;
    private readonly uint targetSampleRate;

    private bool disposed;

    // The resampler that we reuse for sequential reads:
    private readonly WdlResampler _resampler;

    // Current frame positions:
    // - _currentFrameInResampledDomain is how many frames we've already *returned* to callers 
    //   in terms of the new sample rate domain.
    // - _currentFrameInSourceDomain is how many frames we've already consumed from _source 
    //   in terms of the original sample rate domain.
    private long _currentFrameInResampledDomain;
    private long _currentFrameInSourceDomain;

    private float[] resamplerInputBuffer = [];  // Storage for feeding the resampler
    private float[] resamplerOutputBuffer = []; // Temporary buffer for resampler’s output

    // We store the ratio once, to avoid recomputing:
    private readonly double ratio; // ratio = source.SampleRate / targetSampleRate

    /// <summary>
    /// Creates a new <see cref="ResamplerAudioSource"/>.
    /// </summary>
    public ResamplerAudioSource(IAudioSource source, uint targetSampleRate)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        if (targetSampleRate < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(targetSampleRate), "Must be > 0.");
        }

        this.targetSampleRate = targetSampleRate;
        _resampler = new WdlResampler();
        _resampler.SetFeedMode(true); // We'll do a "feed" approach

        // Precompute ratio for converting from new sample rate domain -> old domain
        //   new frames -> old frames
        // because outFrames * ratio = neededInFrames
        ratio = (double)this.source.SampleRate / this.targetSampleRate;

        // Let’s also set the resampler rates:
        _resampler.SetRates(this.source.SampleRate, this.targetSampleRate);

        // Optionally tweak resampler mode, filter, etc.
        // e.g.: _resampler.SetMode(true, 1, false);

        ResetResampler();
    }

    // IAudioSource boilerplate properties
    public TimeSpan Duration => source.Duration;
    public TimeSpan TotalDuration => source.TotalDuration;
    public uint SampleRate => targetSampleRate;
    public ushort ChannelCount => source.ChannelCount;
    public bool IsInitialized => source.IsInitialized;
    public ushort BitsPerSample => source.BitsPerSample;

    public long FramesCount => (long)Math.Floor(source.FramesCount / ratio);

    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            source.Dispose();
        }
    }

    /// <summary>
    /// The main entry point that either continues sequential reading at the current position
    /// or, if <paramref name="startFrame"/> is not the next sequential frame,
    /// seeks (reinitializes the resampler) to random position.
    /// </summary>
    public async Task<Memory<float>> GetSamplesAsync(
        long startFrame,
        int maxFrames = int.MaxValue,
        CancellationToken cancellationToken = default
    )
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(ResamplerAudioSource));
        }

        // If startFrame differs from the next expected sequential position
        // then we are in a "random access" scenario => reset and seek.
        if (startFrame != _currentFrameInResampledDomain)
        {
            Seek(startFrame);
        }

        if (startFrame >= FramesCount || maxFrames <= 0)
        {
            return Memory<float>.Empty;
        }

        var availableFrames = FramesCount - startFrame;
        var framesToGenerate = (int)Math.Min(maxFrames, availableFrames);

        // We'll produce up to framesToGenerate in the *resampled* domain
        // Let's accumulate them in a local buffer and feed from the source as needed.
        // Because WdlResampler in feed mode typically works like:
        //   1) We prepare an input buffer,
        //   2) We supply actual input frames,
        //   3) We read the output from the resampler,
        //   4) If we need more output frames, we feed more input from the source, etc.
        //
        // We'll do that in a while loop until we fill the entire framesToGenerate or we run out of source data.

        var output = new float[framesToGenerate * ChannelCount];
        var totalOutputProduced = 0;

        while (totalOutputProduced < framesToGenerate)
        {
            var framesLeft = framesToGenerate - totalOutputProduced;

            // Step 1: Prepare the resampler with how many output frames we want
            var requestedInputFrames = _resampler.ResamplePrepare(
                framesLeft,
                ChannelCount,
                out var inBuf,
                out var inOffset
            );

            // Step 2: We must fill up to requestedInputFrames from the source, if available
            //         but only if the resampler says it *needs* more input.
            if (requestedInputFrames > 0)
            {
                var sourceFramesLeft = source.FramesCount - _currentFrameInSourceDomain;
                var canRead = (int)Math.Min(requestedInputFrames, sourceFramesLeft);

                if (canRead <= 0)
                {
                    // No more source data
                    // We can "flush" the resampler by calling ResampleOut with nsamples_in=0
                    // but if there's no input, it will only produce leftover if feedmode expects that.
                    canRead = 0;
                }
                else
                {
                    // Read float samples from the underlying source
                    var inputSamples = await source.GetSamplesAsync(
                        _currentFrameInSourceDomain,
                        canRead,
                        cancellationToken
                    );

                    // Copy them into the resampler input buffer
                    inputSamples.CopyTo(inBuf.AsMemory(inOffset, canRead * ChannelCount));

                    // Advance the underlying source position
                    _currentFrameInSourceDomain += canRead;
                }

                // Step 3: Produce output from the resampler
                var outputProducedThisPass = _resampler.ResampleOut(
                    output,
                    totalOutputProduced * ChannelCount,
                    canRead,
                    framesLeft,
                    ChannelCount
                );

                totalOutputProduced += outputProducedThisPass;

                // If we produced 0 frames but still requested input frames, 
                // then the source might be exhausted.
                if (outputProducedThisPass == 0 && canRead == 0)
                {
                    break;
                }
            }
            else
            {
                // If the resampler doesn't need more input but we still want more output,
                // there's not much we can do -> break to avoid infinite loop
                break;
            }
        }

        // Now totalOutputProduced is how many frames we actually have
        _currentFrameInResampledDomain += totalOutputProduced;

        // If we generated fewer frames than requested, shrink the array
        if (totalOutputProduced < framesToGenerate)
        {
            var countFloats = totalOutputProduced * ChannelCount;
            var trimmed = new float[countFloats];
            Array.Copy(output, trimmed, countFloats);
            return trimmed;
        }

        return output;
    }

    public async Task<Memory<byte>> GetFramesAsync(
        long startFrame,
        int maxFrames = int.MaxValue,
        CancellationToken cancellationToken = default
    )
    {
        var floatSamples = await GetSamplesAsync(startFrame, maxFrames, cancellationToken);
        return SampleSerializer.Serialize(floatSamples, BitsPerSample);
    }

    public async Task<int> CopyFramesAsync(
        Memory<byte> destination,
        long startFrame,
        int maxFrames = int.MaxValue,
        CancellationToken cancellationToken = default
    )
    {
        var framesMemory = await GetFramesAsync(startFrame, maxFrames, cancellationToken);
        var framesSpan = framesMemory.Span;

        var bytesToCopy = Math.Min(destination.Length, framesSpan.Length);
        if (bytesToCopy > 0)
        {
            framesSpan.Slice(0, bytesToCopy).CopyTo(destination.Span);

            var bytesPerFrame = ChannelCount * (BitsPerSample / 8);
            return bytesToCopy / bytesPerFrame;
        }

        return 0;
    }

    /// <summary>
    /// Clear the internal state, reposition resampler, etc.
    /// </summary>
    private void ResetResampler()
    {
        _currentFrameInResampledDomain = 0;
        _currentFrameInSourceDomain = 0;
        _resampler.Reset();
        resamplerInputBuffer = [];
        resamplerOutputBuffer = [];
    }

    /// <summary>
    /// Tells our class to reset the existing resampler and jump to a new position.
    /// Typically done when random-access reads detect a jump in `startFrame`.
    /// </summary>
    private void Seek(long newResampledFramePosition)
    {
        // Convert the new target position (resampled domain) to the source domain
        var newSourceFramePos = (long)Math.Floor(newResampledFramePosition * ratio);

        // We can simply reset the entire resampler state
        ResetResampler();

        // Advance our counters to reflect the new position
        _currentFrameInResampledDomain = newResampledFramePosition;
        _currentFrameInSourceDomain = newSourceFramePos;
    }
}
