using System;
using System.Collections.Generic;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils.Extensions;

public class DictionaryExtensions_Tests
{
    private enum MyEnum { None, One, Two }

    [Fact]
    public void GetOrDefault_And_GetOrAdd_Work()
    {
        var d = new Dictionary<string, int>();

        Assert.Equal(42, d.GetOrDefault("missing", 42));

        int factoryCalls = 0;
        var v1 = d.GetOrAdd("a", k => { factoryCalls++; return 1; });
        var v2 = d.GetOrAdd("a", k => { factoryCalls++; return 2; });

        Assert.Equal(1, v1);
        Assert.Equal(1, v2);
        Assert.Equal(1, factoryCalls); // factory called only once
    }

    [Fact]
    public void ParseKeyValue_Parses_Quoted_And_Escaped()
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cs = "Key1=Value1; Key2=\"Quoted value\"; Key3='single '; Key4 = spaced ;";

        dict.ParseKeyValue(cs);

        Assert.Equal("Value1", dict["Key1"]);
        Assert.Equal("Quoted value", dict["Key2"]);
        Assert.Equal("single ", dict["Key3"]);
        Assert.Equal("spaced", dict["Key4"]);
    }

    [Fact]
    public void GetValue_Converts_Types_And_Enum_And_TimeSpans()
    {
        var dict = new Dictionary<string, string>
        {
            ["i"] = "123",
            ["b"] = "true",
            ["e"] = "Two",
            ["ts1"] = "15",
            ["ts2"] = "00:00:05"
        };

        Assert.Equal(123, dict.GetValue("i", 0));
        Assert.True(dict.GetValue("b", false));
        Assert.Equal(MyEnum.Two, dict.GetValue("e", MyEnum.None));
        Assert.Equal(TimeSpan.FromSeconds(15), dict.GetValue("ts1", TimeSpan.Zero));
        Assert.Equal(TimeSpan.FromSeconds(5), dict.GetValue("ts2", TimeSpan.Zero));
    }

    [Fact]
    public void GetValue_Invalid_Throws_LiteException()
    {
        var dict = new Dictionary<string, string>
        {
            ["i"] = "abc",
        };

        Assert.Throws<LiteException>(() => dict.GetValue("i", 0));
    }

    [Theory]
    [InlineData("10", 10)]
    [InlineData("10k", 10L * 1024)]
    [InlineData("2m", 2L * 1024 * 1024)]
    [InlineData("1g", 1L * 1024 * 1024 * 1024)]
    public void GetFileSize_Parses_Sizes(string size, long expected)
    {
        var dict = new Dictionary<string, string> { ["s"] = size };
        Assert.Equal(expected, dict.GetFileSize("s", -1));
    }

    [Fact]
    public void GetFileSize_Invalid_Returns_Zero_And_Default_When_Missing()
    {
        var dict = new Dictionary<string, string>();
        Assert.Equal(123, dict.GetFileSize("missing", 123));

        dict["bad"] = "xyz";
        Assert.Equal(0, dict.GetFileSize("bad", 123));
    }
}
