using FluentAssertions;
using Xunit;

namespace Test.Unit;

public class PlaceholderTest
{
    [Fact]
    public void Test()
    {
        int result = 5 * 3;
        result.Should().Be(15);
    }
}