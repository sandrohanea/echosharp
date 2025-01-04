// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.SpeechTranscription;

namespace EchoSharp.Provisioning;

/// <summary>
/// Represents a provisioner for a speech transcriptor component.
/// </summary>
public interface ISpeechTranscriptorProvisioner
{
    /// <summary>
    /// Runs the provisioning for the specific speech transcriptor component.
    /// </summary>
    /// <remarks>
    /// The provisioning step might include: downloading the ML models, warm-up transcript, downloading specific resources, etc.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token to cancel the operation. </param>
    /// <returns>A task that represents the asynchronous provisioning operation. The task result contains the speech transcriptor factory.</returns>
    public Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default);
}
