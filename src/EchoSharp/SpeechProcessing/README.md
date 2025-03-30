# Speech Processing Components

This directory contains the speech processing components of the EchoSharp library.

## ChunkedSpeechProcessor

The `ChunkedSpeechProcessor` is designed to efficiently process long audio streams by:

1. Segmenting the audio using a Voice Activity Detection (VAD) component
2. Processing each segment in parallel using an underlying speech processor
3. Combining the results while preserving the correct order and timing information

### Key Features

- **Parallel Processing**: Configurable number of parallel processing tasks
- **Segment Padding**: Option to add padding around each segment for better recognition
- **Incomplete Segment Handling**: Option to include or exclude incomplete segments
- **Timing Preservation**: Maintains accurate timing information in the results

### Usage

```csharp
// Create a speech processor factory (e.g., Whisper, Azure, etc.)
var speechProcessorFactory = new WhisperSpeechProcessorFactory(...);

// Create a VAD detector factory
var vadFactory = new SileroVadDetectorFactory(...);

// Create a VAD detector
var vadDetector = vadFactory.CreateVadDetector(new VadDetectorOptions());

// Create options for the chunked processor
var options = new ChunkedSpeechProcessorOptions
{
    MaxParallelism = Environment.ProcessorCount,
    SegmentPadding = TimeSpan.FromMilliseconds(100),
    ProcessIncompleteSegments = false
};

// Method 1: Create the factory and processor directly
var chunkedFactory = new ChunkedSpeechProcessorFactory(speechProcessorFactory, vadDetector, options);
var processor = chunkedFactory.Create(new SpeechProcessorOptions());

// Method 2: Use the extension method
var processor = speechProcessorFactory
    .WithChunking(vadDetector, options)
    .Create(new SpeechProcessorOptions());

// Use the processor to transcribe audio
await foreach (var segment in processor.TranscribeAsync(audioSource, cancellationToken))
{
    Console.WriteLine($"[{segment.StartTime}] {segment.Text}");
}
``` 