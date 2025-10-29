using FluentAssertions;
using Xunit;

namespace Test.Integration;

public class PlaceholderTest
{
    [Fact]
    public void Test()
    {
        int result = 5 * 3;
        result.Should().Be(15);
    }
}