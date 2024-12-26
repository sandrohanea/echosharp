// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Tests.Utils;

internal static class TestData
{

    // Returns sample data in the interval (-1, 1)
    public static float[] GetTestSampleData(int count, float starting = 0, float dividePower = 2)
    {
        var data = new float[count];
        var divedeBy = Math.Pow(10, dividePower);
        for (var i = 0; i < count; i++)
        {
            data[i] = (float)(starting + i / divedeBy);
        }
        return data;
    }
}
