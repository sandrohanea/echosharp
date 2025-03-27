# EchoSharp.NAudio

**EchoSharp.NAudio** is an audio processing component that provides microphone input, audio file handling, and audio output capabilities for EchoSharp using the [NAudio library](https://github.com/naudio/NAudio).

## Overview

This component enables EchoSharp to capture audio from microphones, process audio files, and play audio through speakers. It serves as a bridge between the hardware audio capabilities of the system and the audio processing pipeline.

## Key Features

- **Microphone Input**: Capture audio from system microphones for real-time processing
- **Audio File Processing**: Read audio data from various file formats including WAV and MP3
- **Audio Output**: Send processed audio to system speakers or other audio output devices
- **Resampling**: Convert audio between different sample rates to ensure compatibility with processing components

## Windows-Only features

Many of the features in this component are Windows-only due to the underlying NAudio library's reliance on Windows-specific APIs for audio capture and playback.

## Usage

MicrophoneInputSource is a class that provides microphone input capabilities for EchoSharp. It can be used to capture audio from the system's default microphone or a specific device, to be used by a `IRealTimeTranscriptor` component for real-time speech recognition.

```csharp
// Example code showing how to use EchoSharp.NAudio for microphone input
using var realtimeSpeechProcessor = ...; // Create a speech realtime processor (e.g. a Transcriptor)

using var micAudioSource = new MicrophoneInputSource(deviceNumber: 1);

var microphoneTask = Task.Run(() =>
{
    micAudioSource.StartRecording();
    Console.WriteLine("Speak to recognize, press any key to stop...");
    Console.ReadKey();
    micAudioSource.StopRecording();
});

async Task ShowTranscriptAsync()
{
    await foreach (var transcription in realtimeSpeechProcessor.TranscribeAsync(micAudioSource))
    {
        var eventType = transcription.GetType().Name;
        Console.WriteLine(eventType);

        var textToWrite = transcription switch
        {
            RealtimeSegmentRecognized segmentRecognized => $"{segmentRecognized.Segment.StartTime}-{segmentRecognized.Segment.StartTime + segmentRecognized.Segment.Duration}:{segmentRecognized.Segment.Text}",
            RealtimeSegmentRecognizing segmentRecognizing => $"{segmentRecognizing.Segment.StartTime}-{segmentRecognizing.Segment.StartTime + segmentRecognizing.Segment.Duration}:{segmentRecognizing.Segment.Text}",
            RealtimeSessionStarted sessionStarted => $"SessionId: {sessionStarted.SessionId}",
            RealtimeSessionStopped sessionStopped => $"SessionId: {sessionStopped.SessionId}",
            _ => string.Empty
        };

        Console.WriteLine(textToWrite);
    }
};

await Task.WhenAll(microphoneTask, showTranscriptTask);

```

SpeakerOutSink is a class that provides audio output capabilities for EchoSharp. It can be used to play audio data through the system's default audio output device or a specific device.

```csharp

using var speechSynthesizer = ...; // Create a speech synthesizer

using var speakerOutSink = new SpeakerOutSink(deviceNumber: 1);

await speechSynthesizer.SpeakAsync("Hello, world!", speakerOutSink);

```

## Integration

This component provides the fundamental audio I/O capabilities needed by EchoSharp pipelines. It can be used in conjunction with any of the speech recognition or VAD components. See the [examples](../../examples/) directory for complete usage samples.
