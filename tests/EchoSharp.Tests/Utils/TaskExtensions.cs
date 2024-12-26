// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Xunit;

namespace EchoSharp.Tests.Utils;

public static class TaskExtensions
{
    public static Task WaitWithTimeout(this Task task, TimeSpan timeout)
    {
        var finished = Task.WhenAny(task, Task.Delay(timeout));
        if (finished != task)
        {
            Assert.Fail("Task did not complete within the timeout.");
        }

        return task;
    }
}
