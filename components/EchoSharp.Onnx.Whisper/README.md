# EchoSharp.Onnx.Whisper

**EchoSharp.Onnx.Whisper** is a Speech-to-Text (STT) component that uses ONNX models for speech recognition. It is designed for applications requiring local inference with optimized ONNX runtime.

## Overview

This component enables speech recognition using ONNX-formatted Whisper models. It supports local inference, ensuring data privacy and low latency. The component is still in development and may undergo significant changes.

## Key Features

- **Customizable Speech Recognition**: Use your own Whisper ONNX model for speech-to-text conversion.
- **Local Inference**: Perform speech recognition locally, ensuring data privacy and reducing latency.
- **Flexible Integration**: Seamlessly integrate with EchoSharp's audio processing pipeline for efficient audio analysis.
- **Model Provisioning**: Automatic downloading, verification, and initialization of ONNX models.

## Configuration

The `WhisperOnnxSpeechProcessorConfig` class is used to configure the processor. It supports the following properties:

- `ModelPath`: The path where the model file will be downloaded and used for transcription. If not set, the model will be downloaded in memory and used without being persisted to disk.
- `ModelType`: The type of Whisper ONNX model to download (e.g., Tiny, Small, Medium). Defaults to `WhisperOnnxModelType.Small`.
- `WarmUp`: A flag to run an initial warmup of the model. Defaults to `true`.

### Example Configuration

```csharp
var config = new WhisperOnnxSpeechProcessorConfig
{
    ModelPath = "path/to/store/model",
    ModelType = WhisperOnnxModelType.Tiny,
    WarmUp = true
};
```

## Provisioning System

The `WhisperOnnxSpeechProcessorProvisioner` class handles model downloading, integrity verification, and initialization. It supports both disk-based and memory-only provisioning.

### Example Provisioning

```csharp
var provisioner = new WhisperOnnxSpeechProcessorProvisioner(config);
var factory = await provisioner.ProvisionAsync();
using var processor = factory.Create(new SpeechProcessorOptions
{
    Language = new CultureInfo("en-US"),
    LanguageAutoDetect = false,
    RetrieveTokenDetails = true
});
```

## Integration

This component is ideal for environments where you need the performance benefits of ONNX runtime while maintaining local inference capabilities. See the [examples](../../examples/) directory for complete usage samples.

⚠️ **Experimental Notice**: This component is still in development and may undergo significant changes. Use in production environments is not recommended at this time.
