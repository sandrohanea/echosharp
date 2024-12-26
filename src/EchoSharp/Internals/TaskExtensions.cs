// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Internals;

internal static class TaskExtensions
{
    public static async Task<T> WaitWithCancellationAsync<T>(this Task<T> task, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        var tcs = new TaskCompletionSource<T>();
        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
        var completedTask = await Task.WhenAny(task, tcs.Task);
        if (completedTask == tcs.Task)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
        return await task;
#else
        return await task.WaitAsync(cancellationToken);
#endif
    }

    public static async Task WaitWithCancellationAsync(this Task task, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_0
        var tcs = new TaskCompletionSource<bool>();
        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
        var completedTask = await Task.WhenAny(task, tcs.Task);
        if (completedTask == tcs.Task)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
#else
        await task.WaitAsync(cancellationToken);
#endif
    }
}
