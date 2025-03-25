# EchoSharp.Onnx.SileroVad

**EchoSharp.Onnx.SileroVad** is a Voice Activity Detection (VAD) component that uses [Silero VAD](https://github.com/snakers4/silero-vad) to distinguish between speech and non-speech segments in audio streams.

## Overview

This component efficiently detects voice activity in audio streams, helping to manage and optimize audio processing pipelines. By activating transcription only when necessary, it reduces overhead and improves overall performance of speech processing applications.

## Key Features

- **Accurate Voice Detection**: Reliably identifies when speech is present, even in noisy environments
- **Resource Efficiency**: Minimizes unnecessary processing by filtering out silent or irrelevant audio segments
- **Flexible Configuration**: Easily adjustable settings to fine-tune voice detection thresholds based on specific use cases
- **Automatic Model Provisioning**: Built-in support for downloading, verifying, and initializing the Silero VAD model

## Provisioning System

EchoSharp.Onnx.SileroVad includes a provisioning system that handles model downloading, integrity verification, and initialization:

```csharp
// Create a provisioner that will download and set up the Silero VAD model
var provisioner = new SileroVadProvisioner(new SileroVadConfig
{
    ModelPath = "path/to/store/model",  // Optional: Store model on disk
    Model = SileroVadModels.Full,       // The model to be used (8000, 16000, etc.)
    ThresholdGap = 0.15f,                // Frame size in samples
    Threshold = 0.5f                    // VAD detection threshold
});

// Provision the model - this will:
// 1. Download the Silero VAD model if it doesn't exist
// 2. Verify its integrity using cryptographic hashes
// 3. Initialize the ONNX runtime for inference
using var factory = await provisioner.ProvisionAsync();

// Create a voice activity detector
using var vadDetector = factory.CreateVadDetector(new VadDetectorOptions()
{
    MinSilenceDuration = TimeSpan.FromMilliseconds(150), // Minimum silence duration in seconds
    MinSpeechDuration = TimeSpan.FromMilliseconds(150),  // Minimum speech duration in seconds
});
```

### Memory-Only Provisioning

You can provision the model to memory only, which is useful for containerized environments or when you don't want to persist the model on disk:

```csharp
var provisioner = new SileroVadProvisioner(new SileroVadConfig
{
    Model = SileroVadModels.Full,       // The model to be used (8000, 16000, etc.)
    ThresholdGap = 0.15f,                // Frame size in samples
    Threshold = 0.5f                    // VAD detection threshold
});

using var factory = await provisioner.ProvisionAsync();

```

### Custom ModelDownloader

You can provide a custom ModelDownloader for specialized download scenarios:

```csharp
// Create a custom model downloader with authentication
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
    "Bearer", "your-api-token");
var customDownloader = new ModelDownloader(httpClient);

var provisioner = new SileroVadProvisioner(
    config: new SileroVadConfig { /* ... */ },
    modelDownloader: customDownloader
);
```

## Integration

This component works well in conjunction with any of the Speech-to-Text components to create an efficient audio processing pipeline that only transcribes when speech is detected. See the [examples](../../examples/) directory for complete usage samples.
