// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Xunit;

namespace EchoSharp.Tests.Utils;

public static class ArrayAssert
{
    public static void EqualApprox(IEnumerable<float> expected, IEnumerable<float> actual, float tolerance = 0.001f)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

        var expectedList = expected.ToList();
        var actualList = actual.ToList();

        Assert.Equal(expectedList.Count, actualList.Count);

        for (var i = 0; i < expectedList.Count; i++)
        {
            var diff = Math.Abs(actualList[i] - expectedList[i]);
            Assert.InRange(diff, 0f, tolerance);
        }
    }
}
