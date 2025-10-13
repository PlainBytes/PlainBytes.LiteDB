using System;
using System.Text;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils;

public class StringEncoding_Tests
{
    [Fact]
    public void UTF8_Throws_On_Unpaired_Surrogate()
    {
        // Create a string with a lone high surrogate
        string s = "\uD800"; // high surrogate without low pair
        Assert.Throws<EncoderFallbackException>(() => StringEncoding.UTF8.GetBytes(s));
    }

    [Theory]
    [InlineData("")]
    [InlineData("hello")] 
    [InlineData("héllö 世界")] 
    public void UTF8_Roundtrip_Valid_Strings(string input)
    {
        var bytes = StringEncoding.UTF8.GetBytes(input);
        var back = StringEncoding.UTF8.GetString(bytes);
        Assert.Equal(input, back);
    }
}
