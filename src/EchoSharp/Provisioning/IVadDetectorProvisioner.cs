// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.Provisioning;

/// <summary>
/// Represents a provisioner for a voice activity detector component.
/// </summary>
public interface IVadDetectorProvisioner
{
    /// <summary>
    /// Runs the provisioning for the specific voice activity detector component.
    /// </summary>
    /// <remarks>
    /// The provisioning step might include: downloading the ML models, warm-up transcript, downloading specific resources, etc.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token to cancel the operation. </param>
    /// <returns>A task that represents the asynchronous provisioning operation. The task result contains the voice activity detector factory.</returns>
    public Task<IVadDetectorFactory> ProvisionAsync(CancellationToken cancellationToken = default);
}
