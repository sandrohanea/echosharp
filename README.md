# EchoSharp
EchoSharp is an open-source library designed for near-real-time audio processing, orchestrating different AI models seamlessly for various audio analysis scopes. With an architecture that focuses on flexibility and performance, EchoSharp allows near-real-time Transcription and Translation by integrating components for Speech-to-Text and Voice Activity Detection.

## Key Features

- *Near-Real-Time Audio Processing:* Handle audio data with minimal latency, ensuring efficient near-real-time results.
- *Interchangeable Components*: Customize or extend the library by building your own components for speech-to-text or voice activity detection. EchoSharp exposes flexible interfaces, making integration straightforward.
- *Easy Orchestration*: Manage and coordinate different AI models effectively for specific audio analysis tasks, like transcribing and detecting speech in various environments.

## Get Started

Get started with EchoSharp and explore how adaptable, near-real-time audio processing can transform your projects.

You can find the latest EchoSharp version on nuget at: [EchoSharp](https://www.nuget.org/packages/Whisper.net)

## First-Party components

### EchoSharp.Whisper.net

**EchoSharp.Whisper.net** is a Speech-to-Text (STT) component built on top of [Whisper.net](https://github.com/sandrohanea/whisper.net), providing high-quality transcription and translation capabilities in a near-real-time setting. Leveraging the state-of-the-art Whisper models from OpenAI, this component ensures robust performance for processing audio input with impressive accuracy across multiple languages. It's designed to be highly efficient and easily interchangeable, allowing developers to customize or extend it with alternative STT components if desired.

**Key Features**:
- **Multilingual Transcription**: Supports transcription in multiple languages, with automatic detection and translation capabilities.
- **Customizable Integration**: Plug-and-play design that integrates seamlessly with EchoSharp's audio orchestration.
- **Local Inference**: Perform inference locally, ensuring data privacy and reducing latency for near-real-time processing.

---

### EchoSharp.Onnx.SileroVad

**EchoSharp.Onnx.SileroVad** is a Voice Activity Detection (VAD) component that uses [Silero VAD](https://github.com/snakers4/silero-vad) to distinguish between speech and non-speech segments in audio streams. By efficiently detecting voice activity, this component helps manage and optimize audio processing pipelines, activating transcription only when necessary to reduce overhead and improve overall performance.

**Key Features**:
- **Accurate Voice Detection**: Reliably identifies when speech is present, even in noisy environments.
- **Resource Efficiency**: Minimizes unnecessary processing by filtering out silent or irrelevant audio segments.
- **Flexible Configuration**: Easily adjustable settings to fine-tune voice detection thresholds based on specific use cases.

---

### EchoSharp.OpenAI.Whisper

**EchoSharp.OpenAI.Whisper** is a Speech-to-Text (STT) component that leverages the [OpenAI Whisper API](https://platform.openai.com/docs/guides/speech-to-text).

**Key Features**:
- **High-Quality Transcription**: Utilizes the OpenAI Whisper API to provide accurate and reliable speech-to-text conversion.
- **Azure or OpenAI APIs**: Choose between Azure or OpenAI APIs for transcription based on your requirements. (just provide the AudioClient from OpenAI SDK or Azure SDK)
- **Customizable Integration**: Easily integrate with EchoSharp's audio orchestration for seamless audio processing.

---

### EchoSharp.AzureAI.SpeechServices

**EchoSharp.AzureAI.SpeechServices** is a Speech-to-Text (STT) component that uses the [Azure Speech Services API](https://azure.microsoft.com/en-us/services/cognitive-services/speech-services/).

**Key Features**:
- **Azure Speech Services Integration**: Leverage the Azure Speech Services API for high-quality speech-to-text conversion.
- **Real-Time Transcription**: Process audio data in near-real-time with minimal latency.
- **Customizable Configuration**: Easily adjust settings and parameters to optimize transcription performance.

---

### EchoSharp.WebRtc.WebRtcVadSharp

**EchoSharp.WebRtc.WebRtcVadSharp** is a Voice Activity Detection (VAD) component that uses
the [WebRTC VAD](https://webrtc.org/) and [WebRtcVadSharp](https://github.com/ladenedge/WebRtcVadSharp)
algorithm to detect voice activity in audio streams.
By accurately identifying speech segments, this component helps optimize audio processing pipelines, reducing unnecessary processing and improving overall efficiency.

**Key Features**:
- **Efficient Voice Detection**: Detects voice activity with high accuracy, even in noisy environments.
- **Resource Optimization**: Filters out silent or irrelevant audio segments to minimize processing overhead.
- **Flexible Configuration**: Easily adjust settings to fine-tune voice detection [OperatingMode](https://github.com/ladenedge/WebRtcVadSharp/wiki/OperatingMode-Enum) based on specific use cases.

### EchoSharp.Onnx.Whisper

**Experimental** - This component is still in development and may not be suitable for production use.

**EchoSharp.Onnx.Whisper** is a Speech-to-Text (STT) component that uses an ONNX model for speech recognition.

**Key Features**:
- **Customizable Speech Recognition**: Utilize your own Whisper ONNX model for speech-to-text conversion.
- **Local Inference**: Perform speech recognition locally, ensuring data privacy and reducing latency.
- **Flexible Integration**: Seamlessly integrate with EchoSharp's audio processing pipeline for efficient audio analysis.

### EchoSharp.Onnx.Sherpa

**EchoSharp.Onnx.Sherpa** is a Speech-to-Text (STT) component that uses multiple ONNX models for speech recognition.
It integrates with this [sherpa-onnx](https://github.com/k2-fsa/sherpa-onnx) project and supports both OnlineModels and OfflineModels.
**Key Features**:
- **Customizable Speech Recognition**: Utilize your own ONNX models for speech-to-text conversion.
- **Local Inference**: Perform speech recognition locally, ensuring data privacy and reducing latency.
- **Flexible Integration**: Seamlessly integrate with EchoSharp's audio processing pipeline for efficient audio analysis.
