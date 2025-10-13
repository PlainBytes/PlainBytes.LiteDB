using System;
using System.Globalization;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils.Extensions;

public class StringExtensions_Tests
{
    [Fact]
    public void IsNullOrWhiteSpace_And_IsNullOrEmpty()
    {
        Assert.True(((string)null).IsNullOrWhiteSpace());
        Assert.True(((string)null).IsNullOrEmpty());
        Assert.True(string.Empty.IsNullOrEmpty());
        Assert.True("   ".IsNullOrWhiteSpace());
        Assert.False("a".IsNullOrWhiteSpace());
        Assert.False("a".IsNullOrEmpty());
    }

    [Theory]
    [InlineData("Name", true)]
    [InlineData("_name", true)]
    [InlineData("$x", true)]
    [InlineData("name1", true)]
    [InlineData("1name", false)]
    [InlineData("spa ce", false)]
    [InlineData("", false)]
    public void IsWord_Checks_Valid_Identifiers(string value, bool expected)
    {
        Assert.Equal(expected, value.IsWord());
    }

    [Fact]
    public void Sha1_Known_Vector()
    {
        var sha = "abc".Sha1();
        Assert.Equal("A9993E364706816ABA3E25717850C26C9CD0D89D", sha);
    }

    [Theory]
    [InlineData("hello", "hello", true)]
    [InlineData("hello", "he%", true)]
    [InlineData("hello", "%lo", true)]
    [InlineData("hello", "h_llo", true)]
    [InlineData("hello", "h%o", true)]
    [InlineData("hello", "h%z", false)]
    public void SqlLike_Basic_Wildcards(string input, string pattern, bool expected)
    {
        var col = Collation.Default; // IgnoreCase current culture
        Assert.Equal(expected, input.SqlLike(pattern, col));
    }

    [Fact]
    public void SqlLikeStartsWith_Extracts_Prefix_And_HasMore()
    {
        bool hasMore;
        var p1 = "abc%def".SqlLikeStartsWith(out hasMore);
        Assert.Equal("abc", p1);
        Assert.True(hasMore);

        var p2 = "abc".SqlLikeStartsWith(out hasMore);
        Assert.Equal("abc", p2);
        Assert.False(hasMore);

        var p3 = "%anything".SqlLikeStartsWith(out hasMore);
        Assert.Equal("", p3);
        Assert.True(hasMore);
    }
}
