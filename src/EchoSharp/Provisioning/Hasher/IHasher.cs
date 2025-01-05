// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Hasher;

public interface IHasher
{
    public HasherStream CreateStream(Stream source);
}
