// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.SpeechProcessing;

namespace EchoSharp.Provisioning;

public interface IRealtimeSpeechProcessorProvisioner
{
    /// <summary>
    /// Runs the provisioning for the specific realtime speech transcriptor component.
    /// </summary>
    /// <remarks>
    /// The provisioning step might include: downloading the ML models, warm-up transcript, downloading specific resources, etc.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token to cancel the operation. </param>
    /// <returns>A task that represents the asynchronous provisioning operation. The task result contains the realtime speech transcriptor factory.</returns>
    public Task<IRealtimeSpeechProcessorFactory> ProvisionAsync(CancellationToken cancellationToken = default);
}
