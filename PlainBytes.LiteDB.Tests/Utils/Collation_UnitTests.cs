using System.Globalization;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils;

public class Collation_UnitTests
{
    [Fact]
    public void Collation_StringCtor_And_ToString_Roundtrip()
    {
        var col = new Collation("en-US/IgnoreCase");

        Assert.Equal("en-US", col.Culture.Name);
        Assert.Equal(CompareOptions.IgnoreCase, col.SortOptions);
        Assert.Equal("en-US/IgnoreCase", col.ToString());
    }

    [Fact]
    public void Collation_LcidCtor_Invariant_Ordinal()
    {
        // 127 is invariant culture in this codebase (see Collation.Binary)
        var col = new Collation(127, CompareOptions.Ordinal);

        Assert.Equal(127, col.LCID);
        Assert.Equal(CompareOptions.Ordinal, col.SortOptions);
        Assert.Equal("", col.Culture.Name); // invariant culture has empty name
    }

    [Fact]
    public void Compare_IgnoreCase_Treats_Different_Casing_As_Equal()
    {
        var col = new Collation("en-US/IgnoreCase");

        Assert.Equal(0, col.Compare("abc", "ABC"));
        Assert.Equal(0, col.Compare("Ánã", "ÁNÃ"));
    }

    [Fact]
    public void Compare_None_Treats_Different_Casing_As_Different()
    {
        var col = new Collation("en-US/None");

        Assert.NotEqual(0, col.Compare("abc", "ABC"));
    }

    [Fact]
    public void Equals_On_BsonValue_Respects_Collation()
    {
        var col = new Collation("en-US/IgnoreCase");

        var a = new BsonValue("ana");
        var b = new BsonValue("ANA");

        Assert.True(col.Equals(a, b));
    }

    [Fact]
    public void Defaults_Are_Stable()
    {
        // Default should be IgnoreCase and use current culture LCID
        var def = Collation.Default;
        Assert.Equal(CompareOptions.IgnoreCase, def.SortOptions);

        // Binary should be invariant/ordinal as defined in Collation
        var bin = Collation.Binary;
        Assert.Equal(127, bin.LCID);
        Assert.Equal(CompareOptions.Ordinal, bin.SortOptions);
    }
}
