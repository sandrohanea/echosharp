// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Tests.Utils;

public class FloatComparer(float tolerance) : IEqualityComparer<float>
{
    public const float DefaultTolerance = 0.001f;

    public bool Equals(float x, float y)
    {
        return Math.Abs(x - y) <= tolerance;
    }

    public int GetHashCode(float obj)
    {
        return obj.GetHashCode();
    }
}
