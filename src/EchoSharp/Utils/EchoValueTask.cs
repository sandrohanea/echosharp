// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Utils;

public static class EchoValueTask
{
    public static ValueTask Completed { get; } =
#if NET8_0_OR_GREATER
        ValueTask.CompletedTask;
#else
    new ValueTask(Task.CompletedTask);
#endif
}
