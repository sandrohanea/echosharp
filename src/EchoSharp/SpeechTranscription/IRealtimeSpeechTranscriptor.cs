// Licensed under the MIT license: https://opensource.org/licenses/MIT


// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;

namespace EchoSharp.SpeechTranscription;

public interface IRealtimeSpeechTranscriptor
{
    IAsyncEnumerable<IRealtimeRecognitionEvent> TranscribeAsync(IAwaitableAudioSource source, CancellationToken cancellationToken = default);
}
