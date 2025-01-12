// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.SpeechTranscription;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxSpeechTranscriptorProvisioner(SherpaOnnxSpeechTranscriptorConfig config) : ISpeechTranscriptorProvisioner
{
    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Create proper unarchiver for tar.gz
        return null!;
    }

}
