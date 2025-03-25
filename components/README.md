# EchoSharp Components

This directory contains the various components that make up the EchoSharp ecosystem. Each component is designed to be modular and interchangeable, allowing you to customize your audio processing pipeline according to your specific needs.

## Speech-to-Text Components

These components provide speech recognition capabilities:

- [**EchoSharp.Whisper.net**](./EchoSharp.Whisper.net/README.md) - Local speech recognition using Whisper models via Whisper.net
- [**EchoSharp.OpenAI.Whisper**](./EchoSharp.OpenAI.Whisper/README.md) - Cloud-based speech recognition using OpenAI's Whisper API
- [**EchoSharp.AzureAI.SpeechServices**](./EchoSharp.AzureAI.SpeechServices/README.md) - Cloud-based speech recognition using Azure Speech Services
- [**EchoSharp.Onnx.Whisper**](./EchoSharp.Onnx.Whisper/README.md) - Local speech recognition using Whisper ONNX models *(Experimental)*
- [**EchoSharp.Onnx.Sherpa**](./EchoSharp.Onnx.Sherpa/README.md) - Local speech recognition using Sherpa ONNX models

## Voice Activity Detection (VAD) Components

These components detect when speech is occurring in audio:

- [**EchoSharp.Onnx.SileroVad**](./EchoSharp.Onnx.SileroVad/README.md) - Voice activity detection using the Silero VAD model
- [**EchoSharp.WebRtc.WebRtcVadSharp**](./EchoSharp.WebRtc.WebRtcVadSharp/README.md) - Voice activity detection using WebRTC VAD

## Audio Processing Components

These components handle audio input/output and processing:

- [**EchoSharp.NAudio**](./EchoSharp.NAudio/README.md) - Audio capture and playback using NAudio
- [**EchoSharp.SharpZipLib**](./EchoSharp.SharpZipLib/README.md) - Utilities for handling compressed audio data

## Building Custom Components

EchoSharp is designed to be extensible. You can create your own components by implementing the appropriate interfaces from the `EchoSharp.Abstractions` namespace. See the [development guide](../docs/DevelopmentGuide.md) for more information on creating custom components.
