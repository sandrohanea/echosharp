// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;

namespace EchoSharp.Tests.Utils;

/// <summary>
/// Extension methods for testing with IAsyncEnumerable.
/// </summary>
internal static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Converts an array to an IAsyncEnumerable.
    /// </summary>
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        foreach (var item in source)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    /// <summary>
    /// Converts an IAsyncEnumerable to a List.
    /// </summary>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            results.Add(item);
        }

        return results;
    }
}
