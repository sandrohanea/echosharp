// Licensed under the MIT license: https://opensource.org/licenses/MIT


// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio.Source.Awaitable;

namespace EchoSharp.SpeechTranscription;

public interface IRealtimeSpeechTranscriptor : IDisposable
{
    IAsyncEnumerable<IRealtimeRecognitionEvent> TranscribeAsync(IAwaitableAudioSource source, CancellationToken cancellationToken = default);
}
