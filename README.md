# EchoSharp

EchoSharp is an open-source library designed for near-real-time audio processing, orchestrating different AI models seamlessly for various audio analysis scopes. With an architecture that focuses on flexibility and performance, EchoSharp allows near-real-time Transcription and Translation by integrating components for Speech-to-Text and Voice Activity Detection.

## Key Features

- **Near-Real-Time Audio Processing:** Handle audio data with minimal latency, ensuring efficient near-real-time results.
- **Interchangeable Components**: Customize or extend the library by building your own components for speech-to-text or voice activity detection. EchoSharp exposes flexible interfaces, making integration straightforward.
- **Easy Orchestration**: Manage and coordinate different AI models effectively for specific audio analysis tasks, like transcribing and detecting speech in various environments.
- **Automatic Model Provisioning**: Download, verify, and initialize AI models with built-in integrity checking and efficient caching.

## Get Started

Get started with EchoSharp and explore how adaptable, near-real-time audio processing can transform your projects.

You can find the latest EchoSharp version on nuget at: [EchoSharp](https://www.nuget.org/packages/EchoSharp)

## Components

EchoSharp offers a variety of components for different audio processing needs:

- **Speech-to-Text (STT)** - Convert speech to text using various models and APIs
- **Voice Activity Detection (VAD)** - Detect when speech is occurring in audio
- **Audio Processing** - Handle audio input/output and processing

For detailed information about each component, see the [Components Directory](./components/README.md).

## Provisioning System

EchoSharp includes a powerful model provisioning system that handles:

1. **Automatic Model Downloads**: Download ML models from configured sources
2. **Integrity Verification**: Ensure models aren't corrupted during download with SHA512 hash verification
3. **Caching**: Avoid re-downloading models by checking if they already exist and verifying their integrity
4. **Memory or Disk Storage**: Choose between storing models on disk or loading them directly into memory
5. **Model Initialization**: Initialize models for inference with appropriate settings
6. **Warm-up**: Optional pre-inference to minimize latency for the first real request

Example of the provisioning process:

```csharp
// Create a provisioner that will download and set up the Whisper model
var provisioner = new WhisperSpeechProcessorProvisioner(new WhisperSpeechProcessorConfig
{
    ModelPath = "path/to/store/model",       // Optional: Store model on disk
    GgmlType = GgmlType.Base,         // Model size (Tiny, Base, Small, Medium, Large)
    QuantizationType = QuantizationType.Q5_1, // Model quantization level
    WarmUp = true // Perform a warm-up inference pass
});

// Provision the model - this will:
// 1. Download the model if it doesn't exist
// 2. Verify its integrity using cryptographic hashes
// 3. Initialize the model for inference
// 4. Perform a warm-up pass if configured
using var factory = await provisioner.ProvisionAsync();

// Create a speech transcriptor
using var transcriptor = factory.Create(new SpeechProcessorOptions()
{
    LanguageAutoDetect = false, // Flag to auto-detect the language
    Language = new CultureInfo("en-US") // Language to use for transcription
});
```

## Examples

To help you get started with EchoSharp, check out the [examples](./examples/) directory, which contains sample applications demonstrating common use cases:

- **File Speech Transcript** - Transcribe speech from audio files
- **Microphone Speech Transcript** - Real-time transcription of microphone input
- **Text to Speech** - Convert text to speech

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request or open an Issue if you have suggestions or find bugs.

## License

EchoSharp is licensed under the [MIT License](LICENSE).
