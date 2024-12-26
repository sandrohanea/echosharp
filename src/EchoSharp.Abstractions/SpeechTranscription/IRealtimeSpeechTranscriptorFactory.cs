// Licensed under the MIT license: https://opensource.org/licenses/MIT
namespace EchoSharp.Abstractions.SpeechTranscription;

public interface IRealtimeSpeechTranscriptorFactory
{
    IRealtimeSpeechTranscriptor Create(RealtimeSpeechTranscriptorOptions options);
}
