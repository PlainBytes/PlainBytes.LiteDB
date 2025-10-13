using System.Text.RegularExpressions;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Client;

public class RegexResolver_Tests
{
    [Fact]
    public void ResolveMethod_Maps_Supported_Regex_Methods()
    {
        var rr = new RegexResolver();
        var split = typeof(Regex).GetMethod(nameof(Regex.Split), new[] { typeof(string), typeof(string) })!;
        var isMatch = typeof(Regex).GetMethod(nameof(Regex.IsMatch), new[] { typeof(string), typeof(string) })!;

        Assert.Equal("SPLIT(@0, @1, true)", rr.ResolveMethod(split));
        Assert.Equal("IS_MATCH(@0, @1)", rr.ResolveMethod(isMatch));

        // an unsupported method should return null
        var escape = typeof(Regex).GetMethod(nameof(Regex.Escape), new[] { typeof(string) })!;
        Assert.Null(rr.ResolveMethod(escape));
    }
}