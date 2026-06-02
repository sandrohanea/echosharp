// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Internals;

internal static class TaskExtensions
{
    public static async Task<T> WaitWithCancellationAsync<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        return await task.WaitAsync(cancellationToken);
    }

    public static async Task WaitWithCancellationAsync(this Task task, CancellationToken cancellationToken)
    {
        await task.WaitAsync(cancellationToken);
    }
}
