// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;

namespace EchoSharp.SpeechTranscription;

public class EchoSharpRealtimeTranscriptorProvisioner(IVadDetectorProvisioner vadProvisioner, ISpeechTranscriptorProvisioner speechTranscriptorProvisioner, EchoSharpRealtimeTranscriptorConfig config, ISpeechTranscriptorProvisioner? speechTranscriptorRecognizingProvisioner = null) : IRealtimeSpeechTranscriptorProvisioner
{
    public async Task<IRealtimeSpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var vadFactory = await vadProvisioner.ProvisionAsync(cancellationToken);
        var speechTranscriptorFactory = await speechTranscriptorProvisioner.ProvisionAsync(cancellationToken);
        var speechTranscriptorRecognizingFactory = speechTranscriptorRecognizingProvisioner is not null
            ? await speechTranscriptorRecognizingProvisioner.ProvisionAsync(cancellationToken)
            : null;

        return new EchoSharpRealtimeTranscriptorFactory(speechTranscriptorFactory, vadFactory, speechTranscriptorRecognizingFactory, config.Options, config.VadOptions);
    }
}
