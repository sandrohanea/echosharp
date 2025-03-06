// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.SpeechSynthesis;

namespace EchoSharp.Provisioning;

public interface ISpeechSynthesisProvisioner
{
    /// <summary>
    /// Runs the provisioning for the specific speech synthesis component.
    /// </summary>
    /// <remarks>
    /// The provisioning step might include: downloading the ML models, warm-up engines, downloading specific resources, etc.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token to cancel the operation. </param>
    /// <returns>A task that represents the asynchronous provisioning operation. The task result contains the speech synthesis factory.</returns>
    public Task<ISpeechSynthesizerFactory> ProvisionAsync(CancellationToken cancellationToken = default);
}
