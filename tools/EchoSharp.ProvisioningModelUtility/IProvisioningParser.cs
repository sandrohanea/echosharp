// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;

namespace EchoSharp.ProvisioningModelUtility;

public interface IProvisioningParser
{
    ProvisioningModel.ArchiveTypes GetArchiveType();
    IntegrityFile GetIntegrityFile();
    long GetLargestFileSize();
    Task RunAsync(CancellationToken cancellationToken);
}
