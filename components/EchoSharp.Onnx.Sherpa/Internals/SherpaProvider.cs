// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;

namespace EchoSharp.Onnx.Sherpa.Internals;

internal sealed class SherpaProvider<T>(Func<int, T> factory) : IDisposable
    where T : IDisposable
{
    private readonly ConcurrentDictionary<int, T> entities = [];

    public void Dispose()
    {
        foreach (var recognizer in entities.Values)
        {
            recognizer.Dispose();
        }
    }

    /// <summary>
    /// Get the dependency based on the sample rate.
    /// </summary>
    /// <returns></returns>
    public T Get(int sampleRate)
    {
        return entities.GetOrAdd(sampleRate, factory);
    }
}
