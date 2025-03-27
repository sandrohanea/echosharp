# EchoSharp.AzureAI.SpeechServices

**EchoSharp.AzureAI.SpeechServices** is a comprehensive Azure AI Speech Services integration component that provides Speech-to-Text (STT) and Text-to-Speech (TTS) capabilities through Azure's cloud services.

## Overview

This component offers three main types of functionality:

- Fast Speech-to-Text transcription (for pre-recorded audio)
- Real-time Speech-to-Text transcription (for live audio streams)
- Text-to-Speech synthesis

## Authentication Methods

The component supports multiple authentication methods through the `AzureSpeechServicesConfig` class:

1. **Subscription Key Authentication**

   ```csharp
   var config = new AzureSpeechServicesConfig
   {
       SubscriptionKey = "your-subscription-key",
       AzureRegion = "westeurope"  // or use Endpoint for custom endpoints
   };
   ```

2. **Azure Active Directory (AAD) Token Authentication**

    ```csharp
    var config = new AzureSpeechServicesConfig
    {
        TokenCredential = new DefaultAzureCredential(),
        ResourceId = "your-azure-resource-id",
        AzureRegion = "westeurope"
    };
    ```

3. **Environment Variables**

   - Set `AZURE_AI_SUBSCRIPTION_KEY` for the subscription key
   - Set `AZURE_AI_REGION` for the Azure region

The configuration will automatically check for environment variables if values are not explicitly set.

## Speech-to-Text Components

### 1. Fast Transcriptor

The Fast Transcriptor is optimized for transcribing pre-recorded audio files (or integration with `EchoSharpRealtimeProcessor` and a custom voice activity detection component)

```csharp
// Create configuration
var config = new AzureSpeechServicesConfig
{
    SubscriptionKey = "your-key",
    AzureRegion = "your-region"
};

// Create provisioner
var provisioner = new AzureAIFastTranscriptorProvisioner(config);

// Get factory and create transcriptor
using var factory = await provisioner.ProvisionAsync();
using var transcriptor = factory.Create(new SpeechProcessorOptions
{
    LanguageAutoDetect = false,
    RetrieveTokenDetails = true,
    Language = CultureInfo.GetCultureInfo("en-US")
});
```

### 2. Real-time Transcriptor

The Real-time Transcriptor is designed for live audio streams with minimal latency.

```csharp
// Create configuration
var config = new AzureSpeechServicesConfig
{
    SubscriptionKey = "your-key",
    AzureRegion = "your-region"
};

// Configure realtime options
var realtimeOptions = new AzureAIRealtimeTranscriptorOptions
{
    CandidateLanguages = [new CultureInfo("en-US"), new CultureInfo("es-ES")]
};

// Create provisioner
var provisioner = new AzureAIRealtimeTranscriptorProvisioner(config, realtimeOptions);

// Get factory and create transcriptor
using var factory = await provisioner.ProvisionAsync();
using var transcriptor = factory.Create(new RealtimeSpeechProcessorOptions());
```

## Text-to-Speech Component

The Text-to-Speech component provides high-quality speech synthesis capabilities.

```csharp
// Create configuration
var config = new AzureSpeechServicesConfig
{
    SubscriptionKey = "your-key",
    AzureRegion = "your-region"
};

// Configure synthesis options
var synthOptions = new AzureSpeechSynthesizerOptions
{
    SpeakingRate = 1.0,  // Default rate
    Pitch = 1.0,         // Default pitch
    Volume = 100         // Default volume
};

// Create provisioner
var provisioner = new AzureSpeechSynthesisProvisioner(config, synthOptions);

// Get factory and create synthesizer
using var factory = await provisioner.ProvisionAsync();
using var synthesizer = factory.Create(new SpeechSynthesizerOptions
{
    DefaultLanguage = CultureInfo.GetCultureInfo("en-US"),
    DefaultVoice = "en-US-JennyNeural"
});
```

### Speech Synthesis Features

The synthesizer supports various customization options:

- Voice selection
- Language selection
- Speaking rate adjustment
- Pitch modification
- Volume control

## Connection Options

For all components, you can configure the connection:

### **Using Azure Region**

```csharp
var config = new AzureSpeechServicesConfig
{
    AzureRegion = "westeurope",
    SubscriptionKey = "your-key",
};
```

### **Using Custom Endpoint**

```csharp
var config = new AzureSpeechServicesConfig
{
    Endpoint = new Uri("https://your-custom-endpoint.cognitiveservices.azure.com/"),
    SubscriptionKey = "your-key",
};
```

### **Warm-up Configuration**

```csharp
var config = new AzureSpeechServicesConfig
{
    // ... Other properties
    WarmUp = true  // Default is true, set to false to disable initial connection warm-up
};
```

### EntraID authentication

```csharp
var config = new AzureSpeechServicesConfig
{
    // ... Other properties
    TokenCredential = new DefaultAzureCredential(),
    AzureRegion = "your-azure-region", // Replace with your Azure region
    ResourceId = "your-azure-resource-id", // Replace with your Azure resource id
};
```
