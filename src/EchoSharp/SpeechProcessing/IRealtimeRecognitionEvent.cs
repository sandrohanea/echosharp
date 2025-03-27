// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.SpeechProcessing;

public interface IRealtimeRecognitionEvent
{
}

public class RealtimeSessionStarted(string sessionId) : IRealtimeRecognitionEvent
{
    public string SessionId { get; } = sessionId;
}

public class RealtimeSessionStopped(string sessionId) : IRealtimeRecognitionEvent
{
    public object SessionId { get; } = sessionId;
}

public class RealtimeSessionCanceled(string sessionId) : IRealtimeRecognitionEvent
{
    public object SessionId { get; } = sessionId;
}

public class RealtimeSegmentRecognizing(ProcessedSpeechSegment segment, string sessionId) : IRealtimeRecognitionEvent
{
    public ProcessedSpeechSegment Segment { get; } = segment;
    public string SessionId { get; } = sessionId;
}

public class RealtimeSegmentRecognized(ProcessedSpeechSegment segment, string sessionId) : IRealtimeRecognitionEvent
{
    public ProcessedSpeechSegment Segment { get; } = segment;
    public string SessionId { get; } = sessionId;
}
