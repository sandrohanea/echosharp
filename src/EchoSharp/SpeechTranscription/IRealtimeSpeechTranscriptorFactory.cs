// Licensed under the MIT license: https://opensource.org/licenses/MIT
namespace EchoSharp.SpeechTranscription;

public interface IRealtimeSpeechTranscriptorFactory : IDisposable
{
    IRealtimeSpeechTranscriptor Create(RealtimeSpeechTranscriptorOptions options);
}
