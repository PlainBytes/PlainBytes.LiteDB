using Xunit;

namespace LiteDB.Tests.Utils;

public class Randomizer_Tests
{
    [Fact]
    public void Next_Returns_Non_Negative_Int()
    {
        var value = Randomizer.Next();
        Assert.InRange(value, 0, int.MaxValue);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(-5, 5)]
    [InlineData(10, 11)]
    public void Next_With_Range_Respects_Bounds(int min, int max)
    {
        var v = Randomizer.Next(min, max);
        Assert.InRange(v, min, max - 1);
    }
}