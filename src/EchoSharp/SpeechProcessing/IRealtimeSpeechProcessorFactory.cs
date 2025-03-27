// Licensed under the MIT license: https://opensource.org/licenses/MIT
namespace EchoSharp.SpeechProcessing;

public interface IRealtimeSpeechProcessorFactory : IDisposable
{
    IRealtimeSpeechProcessor Create(RealtimeSpeechProcessorOptions options);
}
