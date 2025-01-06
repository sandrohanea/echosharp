// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Hasher;

[Serializable]
public class HasherException : Exception
{
    public HasherException(string? message) : base(message)
    {
    }

    public HasherException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
