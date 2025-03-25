# EchoSharp.WebRtc.WebRtcVadSharp

**EchoSharp.WebRtc.WebRtcVadSharp** is a Voice Activity Detection (VAD) component that leverages the WebRTC VAD library for detecting speech in audio streams. It is designed for real-time applications requiring efficient and accurate voice activity detection.

## Overview

This component provides voice activity detection capabilities using the WebRTC VAD library. It is optimized for real-time audio processing and can be integrated into EchoSharp's audio processing pipeline.

## Key Features

- **Real-Time Detection**: Detects voice activity in real-time audio streams with minimal latency.
- **Configurable Operating Modes**: Supports multiple operating modes for balancing accuracy and performance.
- **Customizable Integration**: Easily integrate with EchoSharp's audio processing components.
- **Warm-Up Support**: Optionally warm up the detector for improved initial performance.

## Configuration

The `WebRtcVadSharpConfig` class is used to configure the VAD detector. It supports the following properties:

- `OperatingMode`: The operating mode of the VAD detector. Defaults to `OperatingMode.HighQuality`.
- `WarmUp`: A flag to run an initial warmup of the detector. Defaults to `true`.

### Example Configuration

```csharp
var config = new WebRtcVadSharpConfig
{
    OperatingMode = OperatingMode.LowBitrate,
    WarmUp = true
};
```

## Provisioning System

The `WebRtcVadSharpDetectorProvisioner` class handles the provisioning of the VAD detector. It initializes the detector with the specified configuration and supports warm-up for improved performance.

### Example Provisioning

```csharp
var provisioner = new WebRtcVadSharpDetectorProvisioner(config);
using var factory = await provisioner.ProvisionAsync();
using var vadDetector = factory.CreateVadDetector(new VadDetectorOptions()
{
    MinSilenceDuration = TimeSpan.FromMilliseconds(150),
    MinSpeechDuration = TimeSpan.FromMilliseconds(150),
});

// Use the vadDetector for voice activity detection
```

## Options

The `WebRtcVadSharpOptions` class provides additional options for configuring the VAD detector. It supports the following property:

- `OperatingMode`: The operating mode of the VAD detector. Defaults to `OperatingMode.HighQuality`.

### Example Options

```csharp
var options = new WebRtcVadSharpOptions
{
    OperatingMode = OperatingMode.VeryLowBitrate
};
```

## Integration

This component is ideal for real-time audio processing applications where efficient and accurate voice activity detection is required. See the [examples](../../examples/) directory for complete usage samples.
