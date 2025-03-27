# EchoSharp.OpenAI.Whisper

**EchoSharp.OpenAI.Whisper** is a Speech-to-Text (STT) component that leverages the [OpenAI Whisper API](https://platform.openai.com/docs/guides/speech-to-text) for high-quality speech recognition.

## Overview

This component provides cloud-based speech recognition capabilities through the OpenAI Whisper API. It's designed for applications that require high-accuracy transcription and don't need to perform inference locally.

## Key Features

- **High-Quality Transcription**: Utilizes the OpenAI Whisper API to provide accurate and reliable speech-to-text conversion.
- **Flexible API Support**: Choose between Azure or OpenAI APIs for transcription based on your requirements.
- **Customizable Integration**: Easily integrate with EchoSharp's audio processing components.
- **API Key Management**: Simple configuration for API credentials and deployment names.

## Configuration

The `OpenAiWhisperSpeechProcessorConfig` class is used to configure the transcriptor. It supports the following properties:

- `ApiKey`: The API key for the OpenAI Whisper API. If not set, the key is read from the `OPENAI_API_KEY` environment variable.
- `AudioClient`: An optional audio client for advanced configurations.
- `Temperature`: Controls the randomness of the transcription (0.0 to 1.0).
- `WarmUp`: A flag to run an initial warmup to establish the connection (default is `true`).

### Example Configuration

```csharp
var config = new OpenAiWhisperSpeechProcessorConfig
{
    ApiKey = "your-api-key",
    Temperature = 0.5f,
    WarmUp = true
};
```

For Azure OpenAI, you can use the `OpenAiWhisperSpeechProcessorConfig` class with the custom AudioClient:

```csharp
var azureClient = new Azure.AI.OpenAI.AzureOpenAIClient(
    new Uri("https://your-resource.openai.azure.com"),
    new Azure.AzureKeyCredential("your-api-key"));

var config = new OpenAiWhisperSpeechProcessorConfig
{
    AudioClient = azureClient.GetAudioClient("deployment-name"),
    Temperature = 0.5f,
    WarmUp = true
};
```

## Provisioning System

The `OpenAIWhisperSpeechTranscriporProvisioner` class handles the provisioning of the transcriptor. It supports multiple methods for API key management and initialization.

### Example Provisioning

```csharp
var provisioner = new OpenAIWhisperSpeechTranscriporProvisioner(config);
using var factory = await provisioner.ProvisionAsync();
using var transcriptor = factory.Create(new SpeechProcessorOptions
{
    Language = new CultureInfo("en-US"),
    LanguageAutoDetect = false,
    RetrieveTokenDetails = true
});
```

## Usage

### Using OpenAI Whisper API

```csharp
var openaiClient = new OpenAI.OpenAIClient("your-api-key");
using var factory = new OpenAIWhisperSpeechProcessorFactory(openaiClient.GetAudioClient("audio-model"));
using var transcriptor = factory.Create(new SpeechProcessorOptions()
{
    Language = CultureInfo.GetCultureInfo("en-US"),
    LanguageAutoDetect = false,
});
```

### Using Azure OpenAI

```csharp
var azureClient = new Azure.AI.OpenAI.AzureOpenAIClient(
    new Uri("https://your-resource.openai.azure.com"),
    new Azure.AzureKeyCredential("your-api-key"));

using var factory = new OpenAIWhisperSpeechProcessorFactory(azureClient.GetAudioClient("deployment-name"));
using var transcriptor = factory.Create(new SpeechProcessorOptions()
{
    Language = CultureInfo.GetCultureInfo("en-US"),
    LanguageAutoDetect = false,
});
// Use the transcriptor for speech-to-text processing
```

## Integration

This component can be used on its own or as part of a custom audio processing pipeline. See the [examples](../../examples/) directory for complete usage samples.
