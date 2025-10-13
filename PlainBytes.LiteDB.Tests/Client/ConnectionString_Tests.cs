using Xunit;

namespace PlainBytes.LiteDB.Tests.Client;

public class ConnectionString_Tests
{
    [Fact]
    public void Parses_KeyValue_Pairs_And_Defaults()
    {
        var cs = new ConnectionString("filename=my.db;readonly=true;initial size=1mb;password=;upgrade=true;auto-rebuild=true;connection=Direct");

        Assert.Equal("my.db", cs.Filename);
        Assert.True(cs.ReadOnly);
        Assert.Equal(1024L * 1024, cs.InitialSize);
        Assert.Null(cs.Password); // empty string becomes null
        Assert.True(cs.Upgrade);
        Assert.True(cs.AutoRebuild);
        Assert.Equal(ConnectionType.Direct, cs.Connection);

        // indexer exposes parsed values (case-insensitive)
        Assert.Equal("my.db", cs["FILENAME"]);
        Assert.Equal("true", cs["readonly"]);
    }

    [Fact]
    public void Parses_Filename_Short_Form()
    {
        var cs = new ConnectionString("short.db");
        Assert.Equal("short.db", cs.Filename);
        Assert.Equal(ConnectionType.Direct, cs.Connection);
    }

    [Fact]
    public void Parses_Collation_String_When_Provided()
    {
        var cs = new ConnectionString("filename=f.db;collation=pt-BR/IgnoreCase");
        Assert.NotNull(cs.Collation);
        Assert.Equal("pt-BR/IgnoreCase", cs["collation"]);
    }
}