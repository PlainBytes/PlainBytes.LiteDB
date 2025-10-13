using System;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils;

public class Result_Tests
{
    [Fact]
    public void Ok_Result_Holds_Value_And_Implicit_Conversion_Works()
    {
        Result<string> r = "hello"; // implicit
        Assert.True(r.Ok);
        Assert.False(r.Fail);
        Assert.Equal("hello", r.Value);

        string s = r; // implicit to value
        Assert.Equal("hello", s);
        Assert.Equal("hello", r.GetValue());
    }

    [Fact]
    public void Fail_Result_Throws_On_GetValue()
    {
        var ex = new InvalidOperationException("bad");
        var r = new Result<string>(null, ex);
        Assert.False(r.Ok);
        Assert.True(r.Fail);
        Assert.Throws<InvalidOperationException>(() => r.GetValue());
    }
}
