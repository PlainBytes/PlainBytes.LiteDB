using System;
using System.IO;
using System.Linq;
using LiteDB;
using Xunit;

namespace LiteDB.Tests.Client;

public class LiteDatabase_Tests
{
    [Fact]
    public void GetCollection_With_And_Without_AutoId_Works()
    {
        using var db = new LiteDatabase(new MemoryStream());

        var c1 = db.GetCollection("col1");
        Assert.Equal(BsonAutoId.ObjectId, c1.AutoId);
        Assert.Equal("col1", c1.Name);

        var c2 = db.GetCollection("col2", BsonAutoId.Int32);
        Assert.Equal(BsonAutoId.Int32, c2.AutoId);
        Assert.Equal("col2", c2.Name);

        // Strongly typed overloads
        var cs = db.GetCollection<LiteCollection_Tests>("typed");
        Assert.NotNull(cs);
    }

    [Fact]
    public void CollectionNames_Create_Exists_Rename_Drop()
    {
        using var db = new LiteDatabase(new MemoryStream());

        var col = db.GetCollection("A");
        col.Insert(new BsonDocument { ["n"] = 1 });

        // exists and names
        Assert.True(db.CollectionExists("A"));
        Assert.Contains("A", db.GetCollectionNames());

        // rename
        Assert.True(db.RenameCollection("A", "B"));
        Assert.False(db.CollectionExists("A"));
        Assert.True(db.CollectionExists("B"));

        // drop
        Assert.True(db.DropCollection("B"));
        Assert.False(db.CollectionExists("B"));
    }

    [Fact]
    public void Begin_Commit_Rollback_Are_Supported()
    {
        using var db = new LiteDatabase(new MemoryStream());
        Assert.True(db.BeginTrans());
        Assert.True(db.Commit());
        Assert.True(db.BeginTrans());
        Assert.True(db.Rollback());
    }

    [Fact]
    public void Execute_Command_Pragma_Checkpoint_Do_Not_Throw()
    {
        using var db = new LiteDatabase(new MemoryStream());
        // basic pragma
        var userVer = db.Pragma("USER_VERSION");
        Assert.True(userVer.IsInt32 || userVer.IsInt64);

        // simple execute (no-op) and checkpoint
        using var reader = db.Execute("SELECT 1");
        db.Checkpoint();
    }
}
