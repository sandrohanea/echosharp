// Licensed under the MIT license: https://opensource.org/licenses/MIT

using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Execution;

namespace EchoSharp.Tests.Utils;

public static class GenericCollectionAssertionsExtensions
{
    public static AndConstraint<GenericCollectionAssertions<T>> ApproxEqual<T>(
        this GenericCollectionAssertions<T> assertions,
        IEnumerable<T> expected,
        Func<T, T, bool> comparison,
        string because = "",
        params object[] becauseArgs) where T : struct
    {
        var actualItems = assertions.Subject;
        actualItems.Should().NotBeNull(because, becauseArgs);
        var actualItemsList = actualItems.ToList();
        var expectedList = expected.ToList();

        if (actualItemsList.Count != expectedList.Count)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:collection} to have {0} item(s), but found {1}.", expectedList.Count, actualItemsList.Count);
        }

        for (var i = 0; i < actualItemsList.Count; i++)
        {
            var actual = actualItemsList[i];
            var expectedItem = expectedList[i];

            if (!comparison(actual, expectedItem))
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection}[{0}] to be {1}{reason}, but found {2}.", i, expectedItem, actual);
            }
        }

        return new AndConstraint<GenericCollectionAssertions<T>>(assertions);
    }

    public static AndConstraint<GenericCollectionAssertions<float>> BeApproxEqual(
        this GenericCollectionAssertions<float> assertions,
        IEnumerable<float> expected,
        float tolerance = 0.001f,
        string because = "",
        params object[] becauseArgs)
    {
        return assertions.ApproxEqual(
            expected,
            (actual, exp) => Math.Abs(actual - exp) <= tolerance,
            because,
            becauseArgs);
    }
}
