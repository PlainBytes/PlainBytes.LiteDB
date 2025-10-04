using System;
using Xunit;

namespace LiteDB.Tests.Utils.Extensions;

public class DateExtensions_Tests
{
    [Fact]
    public void Truncate_Preserves_Milliseconds_And_Kind()
    {
        var dt = new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc).AddTicks(1234);
        var t = dt.Truncate();
        Assert.Equal(new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc), t);
    }

    [Fact]
    public void MonthDifference_Basic()
    {
        var a = new DateTime(2020, 1, 31);
        var b = new DateTime(2020, 3, 1);
        Assert.Equal(1, a.MonthDifference(b));
    }

    [Fact]
    public void YearDifference_Complete_Years()
    {
        var a = new DateTime(2000, 5, 10);
        var b = new DateTime(2003, 5, 9);
        Assert.Equal(2, a.YearDifference(b));

        b = new DateTime(2003, 5, 10);
        Assert.Equal(3, a.YearDifference(b));
    }
}
