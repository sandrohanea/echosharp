// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using EchoSharp.Audio;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.SpeechTranscription;

internal class EchoSharpRealtimeTranscriptor(
    ISpeechTranscriptorFactory speechTranscriptorFactory,
    IVadDetector vadDetector,
    ISpeechTranscriptorFactory? recognizingSpeechTranscriptorFactory,
    RealtimeSpeechTranscriptorOptions options,
    EchoSharpRealtimeOptions echoSharpRealtimeOptions)
    : IRealtimeSpeechTranscriptor
{
    public async IAsyncEnumerable<IRealtimeRecognitionEvent> TranscribeAsync(IAwaitableAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var promptBuilder = new StringBuilder(options.Prompt);
        CultureInfo? detectedLanguage = null;

        // Wait for the stream to be initialized
        await source.WaitForInitializationAsync(cancellationToken);

        var sessionId = Guid.NewGuid().ToString();
        yield return new RealtimeSessionStarted(sessionId);

        var processedDuration = TimeSpan.Zero;
        var lastDuration = TimeSpan.Zero;

        // We have enough samples to start processing
        while (!source.IsFlushed)
        {
            // Get samples
            var currentDuration = source.Duration;

            // If we couldn't read any samples, we wait for new samples
            if (currentDuration == lastDuration)
            {
                await source.WaitForNewSamplesAsync(lastDuration + echoSharpRealtimeOptions.ProcessingInterval, cancellationToken);
                continue;
            }

            lastDuration = currentDuration;

            var slicedSource = new SliceAudioSource(source, processedDuration, currentDuration - processedDuration);

            VadSegment? lastNonFinalSegment = null;
            VadSegment? recognizingSegment = null;
            await foreach (var segment in vadDetector.DetectSegmentsAsync(slicedSource, cancellationToken))
            {
                // This segment is the last one, we will process it as recognizing or when the source will be flushed (as new data might be written to it and it's not final)
                if (segment.IsIncomplete)
                {
                    recognizingSegment = segment;
                    continue;
                }

                var segmentEnd = segment.StartTime + segment.Duration;
                lastNonFinalSegment = segment;

                var transcribingEvents = TranscribeSegments(
                    speechTranscriptorFactory,
                    source,
                    processedDuration,
                    segment.StartTime,
                    segment.Duration,
                    promptBuilder,
                    detectedLanguage,
                    cancellationToken);

                await foreach (var segmentData in transcribingEvents)
                {
                    if (options.AutodetectLanguageOnce)
                    {
                        detectedLanguage = segmentData.Language;
                    }
                    if (echoSharpRealtimeOptions.ConcatenateSegmentsToPrompt)
                    {
                        promptBuilder.Append(segmentData.Text);
                    }
                    yield return new RealtimeSegmentRecognized(segmentData, sessionId);
                }
            }

            // We run the recognizing task if enabled (run the segment that was not fully recognized yet by vad)
            if (options.IncludeSpeechRecogizingEvents && recognizingSegment != null)
            {
                var transcribingEvents = TranscribeSegments(
                    recognizingSpeechTranscriptorFactory ?? speechTranscriptorFactory,
                    source,
                    processedDuration,
                    recognizingSegment.StartTime,
                    recognizingSegment.Duration,
                    promptBuilder,
                    detectedLanguage,
                    cancellationToken);

                await foreach (var segment in transcribingEvents)
                {
                    yield return new RealtimeSegmentRecognizing(segment, sessionId);
                }
            }

            if (lastNonFinalSegment != null)
            {
                var skippingDuration = lastNonFinalSegment.StartTime + lastNonFinalSegment.Duration;
                processedDuration += skippingDuration;

                // We can discard samples if the audio source supports it
                if (source is IDiscardableAudioSource discardableSource)
                {
                    var lastSegmentEndFrameIndex = (int)(skippingDuration.TotalMilliseconds * source.SampleRate / 1000d) - 1;
                    discardableSource.DiscardFrames(lastSegmentEndFrameIndex);
                }
            }
            else if (recognizingSegment == null)
            {
                // We are in a situation where we don't have any segments so we might have a long silence which we might discard
                if (lastDuration - processedDuration > echoSharpRealtimeOptions.SilenceDiscardInterval)
                {
                    // We can discard samples if the audio source supports it (we just discard half of the silence)
                    var silenceDurationToDiscard = TimeSpan.FromTicks(echoSharpRealtimeOptions.SilenceDiscardInterval.Ticks / 2);
                    processedDuration += silenceDurationToDiscard;

                    if (source is IDiscardableAudioSource discardableSource)
                    {
                        var halfSilenceIndex = (int)(silenceDurationToDiscard.TotalMilliseconds * source.SampleRate / 1000d) - 1;
                        discardableSource.DiscardFrames(halfSilenceIndex);
                    }
                }
            }
        }

        // Stream is complete, we need to process the last segments fully
        var lastEvents = TranscribeSegments(
            speechTranscriptorFactory,
            source,
            processedDuration,
            TimeSpan.Zero,
            source.Duration - processedDuration,
            promptBuilder,
            detectedLanguage,
            cancellationToken);

        await foreach (var segmentData in lastEvents)
        {
            if (echoSharpRealtimeOptions.ConcatenateSegmentsToPrompt)
            {
                promptBuilder.Append(segmentData.Text);
            }
            yield return new RealtimeSegmentRecognized(segmentData, sessionId);
        }

        yield return new RealtimeSessionStopped(sessionId);
    }

    private async IAsyncEnumerable<TranscriptSegment> TranscribeSegments(ISpeechTranscriptorFactory transcriptorFactory,
                                                                         IAudioSource source,
                                                                         TimeSpan processedDuration,
                                                                         TimeSpan startTime,
                                                                         TimeSpan duration,
                                                                         StringBuilder promptBuilder,
                                                                         CultureInfo? detectedLanguage,
                                                                         [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (duration < echoSharpRealtimeOptions.MinTranscriptDuration)
        {
            yield break;
        }

        // We need to add the processedDuration as the Vad was not aware of it.
        startTime += processedDuration;

        // We need to pad the segment with silence if it's too short (we pad to both sides)
        var paddedStart = startTime - echoSharpRealtimeOptions.PaddingDuration;

        // We don't want to pad the segment before the processed duration (as those samples can be already discarded)
        if (paddedStart < processedDuration)
        {
            paddedStart = processedDuration;
        }

        var paddedDuration = duration + echoSharpRealtimeOptions.PaddingDuration;

        // If the segment is too short, we need to pad it with silence (we do it to both sides)
        using IAudioSource paddedSource = paddedDuration < echoSharpRealtimeOptions.MinDurationWithPadding
            ? GetSilenceAddedSource(source, paddedStart, paddedDuration)
            : new SliceAudioSource(source, paddedStart, paddedDuration);

        var languageAutodetect = options.LanguageAutoDetect;
        var language = options.Language;

        // If we only detect language once, we reuse the detected language
        if (languageAutodetect && options.AutodetectLanguageOnce && detectedLanguage != null)
        {
            languageAutodetect = false;
            language = detectedLanguage;
        }

        var currentOptions = options with
        {
            Prompt = echoSharpRealtimeOptions.ConcatenateSegmentsToPrompt ? promptBuilder.ToString() : options.Prompt,
            LanguageAutoDetect = languageAutodetect,
            Language = language
        };
        using var trancriptor = transcriptorFactory.Create(currentOptions);

        await foreach (var segment in trancriptor.TranscribeAsync(paddedSource, cancellationToken))
        {
            // We need to adjust the start time of the segment based on the processed duration
            segment.StartTime += processedDuration;
            yield return segment;
        }
    }

    private ConcatAudioSource GetSilenceAddedSource(IAudioSource source, TimeSpan paddedStart, TimeSpan paddedDuration)
    {
        var silenceDuration = new TimeSpan((echoSharpRealtimeOptions.MinDurationWithPadding.Ticks - paddedDuration.Ticks) / 2);
        var preSilence = new SilenceAudioSource(silenceDuration, source.SampleRate, source.ChannelCount, source.BitsPerSample);
        var postSilence = new SilenceAudioSource(silenceDuration, source.SampleRate, source.ChannelCount, source.BitsPerSample);
        return new ConcatAudioSource([preSilence, new SliceAudioSource(source, paddedStart, paddedDuration), postSilence]);
    }

}
