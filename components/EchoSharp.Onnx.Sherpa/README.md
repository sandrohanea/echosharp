# EchoSharp.Onnx.Sherpa

**EchoSharp.Onnx.Sherpa** is a component of the EchoSharp library that provides Speech-to-Text (STT) transcription capabilities using Sherpa Onnx models. It supports both online and offline transcription configurations.

## Overview

This component offers:

- Offline Speech-to-Text transcription using Sherpa Onnx models.
- Online Speech-to-Text transcription for real-time audio streams.

Both configurations are implemented as `ISpeechTranscriptor` and can be used interchangeably in your applications.

## Sherpa Onnx Models

Sherpa Onnx supports multiple pre-trained models for transcription. These models are defined in the `SherpaOnnxModels` class. For example:

- **ZipFormerGigaSpeech**: A high-quality offline model.
- **ZipFormerGigaSpeechInt8**: An optimized version of the above model with int8 quantization.

You can add more models by creating new instances of the `SherpaOnnxOfflineModel` or `SherpaOnnxOnlineModel` classes.

## Configuration

The `SherpaOnnxSpeechTranscriptorConfig` class is used to configure the transcriptor. It supports the following properties:

- `Model`: The Sherpa Onnx model to use (e.g., `SherpaOnnxModels.ZipFormerGigaSpeech`).
- `Features`: The number of features to use for the model (default is 80).
- `ModelPath`: The path to download and store the model.
- `WarmUp`: A flag to run an initial warmup of the model (default is true).

### Example Configuration

```csharp
var config = new SherpaOnnxSpeechTranscriptorConfig
{
    Model = SherpaOnnxModels.ZipFormerGigaSpeechInt8,
    Features = 80,
    ModelPath = Path.Combine("models", "sherpa"),
    WarmUp = true
};
```

## Provisioning

The `SherpaOnnxSpeechTranscriptorProvisioner` class is responsible for provisioning the transcriptor. It downloads the model, unpacks it, and initializes the transcriptor based on the configuration.

### Example Provisioning

```csharp
var provisioner = new SherpaOnnxSpeechTranscriptorProvisioner(config);
var factory = await provisioner.ProvisionAsync();
using var transcriptor = factory.Create(new SpeechTranscriptorOptions()
{
    Language = new CultureInfo("en-US"),
    LanguageAutoDetect = false,
});
```
