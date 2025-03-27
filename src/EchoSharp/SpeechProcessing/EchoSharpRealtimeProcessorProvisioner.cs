// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;

namespace EchoSharp.SpeechProcessing;

public class EchoSharpRealtimeProcessorProvisioner(IVadDetectorProvisioner vadProvisioner, ISpeechProcessorProvisioner speechTranscriptorProvisioner, EchoSharpRealtimeProcessorConfig config, ISpeechProcessorProvisioner? speechTranscriptorRecognizingProvisioner = null) : IRealtimeSpeechProcessorProvisioner
{
    public async Task<IRealtimeSpeechProcessorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var vadFactory = await vadProvisioner.ProvisionAsync(cancellationToken);
        var speechTranscriptorFactory = await speechTranscriptorProvisioner.ProvisionAsync(cancellationToken);
        var speechTranscriptorRecognizingFactory = speechTranscriptorRecognizingProvisioner is not null
            ? await speechTranscriptorRecognizingProvisioner.ProvisionAsync(cancellationToken)
            : null;

        return new EchoSharpRealtimeProcessorFactory(speechTranscriptorFactory, vadFactory, speechTranscriptorRecognizingFactory, config.Options, config.VadOptions);
    }
}
