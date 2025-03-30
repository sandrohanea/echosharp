// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.SpeechProcessing;

/// <summary>
/// Factory for creating instances of <see cref="ChunkedSpeechProcessor"/>.
/// </summary>
public class ChunkedSpeechProcessorFactory : ISpeechProcessorFactory
{
    private readonly ISpeechProcessorFactory innerFactory;
    private readonly IVadDetector vadDetector;
    private readonly ChunkedSpeechProcessorOptions chunkedOptions;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkedSpeechProcessorFactory"/> class.
    /// </summary>
    /// <param name="innerFactory">The factory used to create the inner speech processor.</param>
    /// <param name="vadDetector">The VAD detector to use for segmenting the audio.</param>
    /// <param name="chunkedOptions">The options for the chunked speech processor.</param>
    public ChunkedSpeechProcessorFactory(
        ISpeechProcessorFactory innerFactory,
        IVadDetector vadDetector,
        ChunkedSpeechProcessorOptions? chunkedOptions = null)
    {
        this.innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
        this.vadDetector = vadDetector ?? throw new ArgumentNullException(nameof(vadDetector));
        this.chunkedOptions = chunkedOptions ?? new ChunkedSpeechProcessorOptions();
    }

    /// <inheritdoc/>
    public ISpeechProcessor Create(SpeechProcessorOptions options)
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(isDisposed, this);
#else
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(ChunkedSpeechProcessorFactory));
        }
#endif

        return new ChunkedSpeechProcessor(innerFactory, options, vadDetector, chunkedOptions);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        if (innerFactory is IDisposable disposableFactory)
        {
            disposableFactory.Dispose();
        }

        vadDetector.Dispose();
        isDisposed = true;
    }
}
