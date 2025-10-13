using Xunit;

namespace PlainBytes.LiteDB.Tests.Client;

public class LiteCollection_Tests
{
    [Fact]
    public void BsonDocument_Mode_Uses_Passed_Name_And_AutoId()
    {
        var col = new LiteCollection<BsonDocument>("col1", BsonAutoId.Guid, engine: null, mapper: null);
        Assert.Equal("col1", col.Name);
        Assert.Equal(BsonAutoId.Guid, col.AutoId);
        Assert.Null(col.EntityMapper);
    }
}