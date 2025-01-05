// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.ProvisioningModelUtility;

internal class UnarchiverDiscard : IUnarchiver
{
    public IUnarchiverSession CreateSession(UnarchiverOptions unarchiverOptions)
    {
        return new UnarchiverDiscardSession();
    }
}
