// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Abstractions.Audio;

namespace EchoSharp.Abstractions.SpeechTranscription;

public interface IRealtimeSpeechTranscriptor
{
    IAsyncEnumerable<IRealtimeRecognitionEvent> TranscribeAsync(IAwaitableAudioSource source, CancellationToken cancellationToken = default);
}
