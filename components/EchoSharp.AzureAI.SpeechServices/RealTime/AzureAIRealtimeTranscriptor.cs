// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using EchoSharp.Abstractions.Audio;
using EchoSharp.Abstractions.Config;
using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.AzureAI.SpeechServices.Internals;

namespace EchoSharp.AzureAI.SpeechServices.RealTime;

internal class AzureAIRealtimeTranscriptor(SpeechConfig speechConfig, RealtimeSpeechTranscriptorOptions options, AzureAIRealtimeTranscriptorOptions azureOptions, RecognizerAuthTokenHandler? recognizerAuthTokenHandler) : IRealtimeSpeechTranscriptor
{
    const int bytesPerSample = 4;

    public async IAsyncEnumerable<IRealtimeRecognitionEvent> TranscribeAsync(IAwaitableAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await source.WaitForInitializationAsync(cancellationToken);

        using var pushStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormat(source.SampleRate, bytesPerSample * 8, (byte)source.ChannelCount, AudioStreamWaveFormat.PCM));
        var audioConfig = AudioConfig.FromStreamInput(pushStream);

        using var recognizer = await GetRecognizerTokenLoaderAsync(audioConfig, cancellationToken);

        var channel = Channel.CreateUnbounded<IRealtimeRecognitionEvent>();

        void RecognizingEventHandler(object? sender, SpeechRecognitionEventArgs e)
        {
            var language = options.Language;
            if (options.LanguageAutoDetect)
            {
                var result = AutoDetectSourceLanguageResult.FromResult(e.Result);
                language = new CultureInfo(result.Language);
            }

            channel.Writer.TryWrite(
                   new RealtimeSegmentRecognizing(
                       new TranscriptSegment()
                       {
                           StartTime = TimeSpan.FromTicks(e.Result.OffsetInTicks),
                           Duration = e.Result.Duration,
                           ConfidenceLevel = (float?)(e.Result.Best().FirstOrDefault()?.Confidence),
                           Text = e.Result.Text,
                           Tokens = e.Result.Best().FirstOrDefault()?.DisplayWords.Select(w => new TranscriptToken()
                           {
                               Id = w.Word,
                               Confidence = (float)w.Confidence,
                               Text = w.Word,
                               StartTime = TimeSpan.FromTicks(w.Offset),
                               Duration = TimeSpan.FromTicks(w.Duration)
                           }).ToList(),
                           Language = language,
                       },
                       e.SessionId));
        }

        if (options.IncludeSpeechRecogizingEvents)
        {
            recognizer.SpeechRecognizer.Recognizing += RecognizingEventHandler;
        }

        void RecognizedEventHandler(object? sender, SpeechRecognitionEventArgs e)
        {
            var language = options.Language;
            if (options.LanguageAutoDetect)
            {
                var result = AutoDetectSourceLanguageResult.FromResult(e.Result);
                language = new CultureInfo(result.Language);
            }

            channel.Writer.TryWrite(
                   new RealtimeSegmentRecognized(
                       new TranscriptSegment()
                       {
                           StartTime = TimeSpan.FromTicks(e.Result.OffsetInTicks),
                           Duration = TimeSpan.FromTicks(e.Result.Duration.Ticks),
                           ConfidenceLevel = (float?)(e.Result.Best().FirstOrDefault()?.Confidence),
                           Text = e.Result.Text,
                           Tokens = e.Result.Best().FirstOrDefault()?.DisplayWords?.Select(w => new TranscriptToken()
                           {
                               Id = w.Word,
                               Confidence = (float)w.Confidence,
                               Text = w.Word,
                               StartTime = TimeSpan.FromTicks(w.Offset),
                               Duration = TimeSpan.FromTicks(w.Duration)
                           }).ToList(),
                           Language = language
                       },
                       e.SessionId));
        }

        void RecognitionSessionStarted(object? sender, SessionEventArgs e)
        {
            channel.Writer.TryWrite(new RealtimeSessionStarted(e.SessionId));
        }

        void RecognitionSessionStopped(object? sender, SessionEventArgs e)
        {
            channel.Writer.TryWrite(new RealtimeSessionStopped(e.SessionId));
            channel.Writer.TryComplete();
        }

        void RecognitionCancelled(object? sender, SpeechRecognitionCanceledEventArgs e)
        {
            channel.Writer.TryWrite(new RealtimeSessionCanceled(e.SessionId));
            channel.Writer.TryComplete();
        }

        recognizer.SpeechRecognizer.SessionStarted += RecognitionSessionStarted;
        recognizer.SpeechRecognizer.SessionStopped += RecognitionSessionStopped;
        recognizer.SpeechRecognizer.Canceled += RecognitionCancelled;

        recognizer.SpeechRecognizer.Recognized += RecognizedEventHandler;

        await recognizer.SpeechRecognizer.StartContinuousRecognitionAsync();

        var readEnumerable = channel.Reader.ReadAllAsync(cancellationToken);

        var consumedIndex = 0L;
        var writeTask = WriteToPushStreamAsync(source, consumedIndex, pushStream, cancellationToken);
        var asyncEnumerator = readEnumerable.GetAsyncEnumerator(cancellationToken);
        var readTask = asyncEnumerator.MoveNextAsync().AsTask();
        do
        {
            var completedTask = await Task.WhenAny(readTask, writeTask);
            if (completedTask == readTask)
            {
                var haveNext = await readTask;
                if (haveNext)
                {
                    yield return asyncEnumerator.Current;
                }
                readTask = asyncEnumerator.MoveNextAsync().AsTask();
                continue;
            }

            consumedIndex = await writeTask;
            writeTask = WriteToPushStreamAsync(source, consumedIndex, pushStream, cancellationToken);
        } while (!source.IsFlushed && !cancellationToken.IsCancellationRequested);

        // After the source is flushed, we want to finish all the remaining recognition events
        await recognizer.SpeechRecognizer.StopContinuousRecognitionAsync();

        while (!cancellationToken.IsCancellationRequested)
        {
            var haveAdditionalElement = await readTask;
            if (!haveAdditionalElement)
            {
                break;
            }
            yield return asyncEnumerator.Current;
            readTask = asyncEnumerator.MoveNextAsync().AsTask();
        }
    }

    public static async Task<long> WriteToPushStreamAsync(IAwaitableAudioSource audioSource, long startIndex, PushAudioInputStream pushStream, CancellationToken cancellationToken)
    {
        await audioSource.WaitForNewSamplesAsync(startIndex, cancellationToken);
        var currentFrames = audioSource.FramesCount;
        if (currentFrames == 0)
        {
            return startIndex;
        }

        var bufferLength = (int)(currentFrames * audioSource.ChannelCount * audioSource.BitsPerSample / 8);
        var dataBuffer = ArrayPool<byte>.Shared.Rent(bufferLength);

        try
        {
            var frames = await audioSource.CopyFramesAsync(dataBuffer, startIndex, (int)currentFrames, cancellationToken: cancellationToken);
            if (frames == 0)
            {
                return startIndex;
            }

            pushStream.Write(dataBuffer, bufferLength);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(dataBuffer, ArrayPoolConfig.ClearOnReturn);
        }

        if (audioSource is IDiscardableAudioSource discardableSource)
        {
            discardableSource.DiscardFrames((int)currentFrames);
        }
        return startIndex + currentFrames;
    }

    private async Task<RecognizerTokenLoader> GetRecognizerTokenLoaderAsync(AudioConfig audioConfig, CancellationToken cancellationToken)
    {
        var recognizer = options.LanguageAutoDetect
            ? new SpeechRecognizer(speechConfig, AutoDetectSourceLanguageConfig.FromLanguages(azureOptions.CandidateLanguages.Select(c => c.ToString()).ToArray()), audioConfig)
            : new SpeechRecognizer(speechConfig, language: options.Language.ToString(), audioConfig);

        if (recognizerAuthTokenHandler != null)
        {
            return await recognizerAuthTokenHandler.GetLoaderAsync(recognizer, cancellationToken);
        }

        return new RecognizerTokenLoader(recognizer);
    }
}
