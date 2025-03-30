// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.SpeechProcessing;

/// <summary>
/// Extension methods for <see cref="ISpeechProcessorFactory"/>.
/// </summary>
public static class SpeechProcessorFactoryExtensions
{
    /// <summary>
    /// Creates a new <see cref="ChunkedSpeechProcessorFactory"/> that wraps the current factory.
    /// </summary>
    /// <param name="factory">The speech processor factory to wrap.</param>
    /// <param name="vadDetector">The VAD detector to use for segmenting the audio.</param>
    /// <param name="options">The options for the chunked speech processor.</param>
    /// <returns>A new factory that creates chunked speech processors.</returns>
    public static ISpeechProcessorFactory WithChunking(
        this ISpeechProcessorFactory factory,
        IVadDetector vadDetector,
        ChunkedSpeechProcessorOptions? options = null)
    {
        return new ChunkedSpeechProcessorFactory(factory, vadDetector, options ?? new ChunkedSpeechProcessorOptions());
    }

    /// <summary>
    /// Creates a new <see cref="ChunkedSpeechProcessorFactory"/> that wraps the current factory.
    /// </summary>
    /// <param name="factory">The speech processor factory to wrap.</param>
    /// <param name="vadDetectorFactory">The factory to create VAD detectors.</param>
    /// <param name="vadDetectorOptions">The options for the VAD detector.</param>
    /// <param name="chunkedOptions">The options for the chunked speech processor.</param>
    /// <returns>A new factory that creates chunked speech processors.</returns>
    public static ISpeechProcessorFactory WithChunking(
        this ISpeechProcessorFactory factory,
        IVadDetectorFactory vadDetectorFactory,
        VadDetectorOptions? vadDetectorOptions = null,
        ChunkedSpeechProcessorOptions? chunkedOptions = null)
    {
        // Create a VAD detector with the provided options
        var vadDetector = vadDetectorFactory.CreateVadDetector(vadDetectorOptions ?? new VadDetectorOptions());
        
        return new ChunkedSpeechProcessorFactory(factory, vadDetector, chunkedOptions ?? new ChunkedSpeechProcessorOptions());
    }
} 