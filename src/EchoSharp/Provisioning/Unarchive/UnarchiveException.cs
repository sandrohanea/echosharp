// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Unarchive;

[Serializable]
public class UnarchiveException : Exception
{
    public UnarchiveException()
    {
    }

    public UnarchiveException(string? message) : base(message)
    {
    }

    public UnarchiveException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
