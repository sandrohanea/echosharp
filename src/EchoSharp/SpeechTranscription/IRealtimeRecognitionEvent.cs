// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.SpeechTranscription;

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

public class RealtimeSegmentRecognizing(TranscriptSegment segment, string sessionId) : IRealtimeRecognitionEvent
{
    public TranscriptSegment Segment { get; } = segment;
    public string SessionId { get; } = sessionId;
}

public class RealtimeSegmentRecognized(TranscriptSegment segment, string sessionId) : IRealtimeRecognitionEvent
{
    public TranscriptSegment Segment { get; } = segment;
    public string SessionId { get; } = sessionId;
}