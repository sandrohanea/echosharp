# EchoSharp.Whisper.net

**EchoSharp.Whisper.net** is a Speech-to-Text (STT) component built on top of [Whisper.net](https://github.com/sandrohanea/whisper.net), providing high-quality transcription and translation capabilities in a near-real-time setting.

## Overview

This component leverages the state-of-the-art Whisper models from OpenAI to ensure robust performance for processing audio input with impressive accuracy across multiple languages. It's designed to be highly efficient and easily interchangeable, allowing developers to customize or extend it with alternative STT components if desired.

## Key Features

- **Multilingual Transcription**: Supports transcription in multiple languages, with automatic detection and translation capabilities
- **Customizable Integration**: Plug-and-play design that integrates seamlessly with EchoSharp's audio orchestration
- **Local Inference**: Perform inference locally, ensuring data privacy and reducing latency for near-real-time processing
- **Automatic Model Provisioning**: Built-in support for downloading, verifying, and initializing Whisper models

## Provisioning System

EchoSharp.Whisper.net includes a powerful provisioning system that handles model downloading, integrity verification, and initialization:

```csharp
// Create a provisioner that will download and set up the Whisper model
var provisioner = new WhisperSpeechTranscriptorProvisioner(new WhisperSpeechTranscriptorConfig
{
    ModelPath = "path/to/store/model",       // Optional: Store model on disk
    GgmlType = GgmlType.Base,         // Model size (Tiny, Base, Small, Medium, Large)
    QuantizationType = QuantizationType.Q5_1 // Model quantization level
});

// Provision the model - this will:
// 1. Download the model if it doesn't exist
// 2. Verify its integrity using cryptographic hashes
// 3. Initialize the model for inference
// 4. Perform a warm-up pass if configured
using var factory = await provisioner.ProvisionAsync();

// Create a speech transcriptor
using var transcriptor = factory.Create(new SpeechTranscriptorOptions()
{
    LanguageAutoDetect = false, // Flag to auto-detect the language
    Language = new CultureInfo("en-US") // Language to use for transcription
});
```

### Memory-Only Provisioning

You can provision models to memory only, which is useful for scenarios where you don't want to store the model on disk:

```csharp
var provisioner = new WhisperSpeechTranscriptorProvisioner(new WhisperSpeechTranscriptorConfig 
{
    // No ModelPath specified - will load to memory only
    GgmlType = GgmlType.Base,         // Model size (Tiny, Base, Small, Medium, Large)
    QuantizationType = QuantizationType.Q5_1 // Model quantization level
    WarmUp = true // Perform a warm-up inference pass
});

using var factory = await provisioner.ProvisionAsync();
```

### Accelerated Inference

The component supports additional acceleration methods like Cuda, OpenVINO or CoreML.

For the accelation methods that require additional encoders, you can specify the path where the encoder model will be stored:

```csharp
var provisioner = new WhisperSpeechTranscriptorProvisioner(new WhisperSpeechTranscriptorConfig 
{
    ModelPath = "path/to/store/model",
    GgmlType = WhisperGgmlType.Small,
    OpenVinoEncoderModelPath = "path/to/store/openvino", // Will download OpenVINO model
    // Or for macOS:
    // CoreMLEncoderModelPath = "path/to/store/coreml",
});
```

Ensure you have the necessary Whisper.net.Runtime for the target platform to use these acceleration methods.

## Custom Builder Configuration

You can customize the Whisper processor by providing a builder configuration (with all the options available in Whisper.net library):

```csharp
var provisioner = new WhisperSpeechTranscriptorProvisioner(
    config: new WhisperSpeechTranscriptorConfig { /* ... */ },
    builderConfig: builder => builder
        .WithLanguage("en")        // Set language
        .WithTranslate()       // Enable translation
        .WithSingleSegment()   // Process as single segment
        .WithPrintSpecialTokens());
```

## Integration

This component can be used on its own or as part of a full EchoSharp pipeline. See the [examples](../../examples/) directory for complete usage samples.
