using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlainBytes.LiteDB;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Client;

public class LiteQueryable_Tests
{
    private class Doc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    private (LiteDatabase db, ILiteCollection<Doc> col) NewDb()
    {
        var db = new LiteDatabase(new MemoryStream());
        var col = db.GetCollection<Doc>("docs");
        return (db, col);
    }

    [Fact]
    public void Fluent_Where_Order_Select_Skip_Limit_Count_Exists()
    {
        var (db, col) = NewDb();
        using (db)
        {
            col.Insert(new[]
            {
                new Doc { Name = "ana", Score = 10 },
                new Doc { Name = "bob", Score = 20 },
                new Doc { Name = "ann", Score = 30 },
                new Doc { Name = "zoe", Score = 40 },
            });

            // ensure index to enable fast query order
            col.EnsureIndex(x => x.Name);

            var q = col.Query()
                .Where(x => x.Name.StartsWith("a"))
                .OrderBy(x => x.Score)
                .Skip(1)
                .Limit(1);

            var list = q.ToList();
            Assert.Single(list);
            Assert.Equal("ann", list[0].Name);

            // Count/Exists reflect the same filter
            Assert.Equal(2, col.Query().Where(x => x.Name.StartsWith("a")).Count());
            Assert.True(col.Query().Where(x => x.Score > 35).Exists());
            Assert.False(col.Query().Where(x => x.Score > 100).Exists());
        }
    }

    [Fact]
    public void Include_GroupBy_Having_Select_Scalar_And_Errors()
    {
        var (db, col) = NewDb();
        using (db)
        {
            col.Insert(new[]
            {
                new Doc { Name = "a", Score = 1 },
                new Doc { Name = "b", Score = 2 },
                new Doc { Name = "a", Score = 3 }
            });

            // Include accepts expressions and raw expressions; here just ensure it doesn't throw and returns same instance
            var q = col.Query();
            var q2 = q.Include(x => x.Name).Include("$.Score");
            Assert.Same(q, q2);

            // Selecting scalar type from non-group query returns simple enumeration
            var scores = col.Query().OrderByDescending(x => x.Score).Select(x => x.Score).ToList();
            Assert.Equal(new[] { 3, 2, 1 }, scores);

            // Duplicated OrderBy must throw
            Assert.Throws<ArgumentException>(() => col.Query().OrderBy(x => x.Name).OrderBy(x => x.Score));
            // Duplicate GroupBy must throw
            Assert.Throws<ArgumentException>(() => col.Query().GroupBy("$.Name").GroupBy("$.Score"));
            // Duplicate Having must throw
            Assert.Throws<ArgumentException>(() => col.Query().Having("$.Score > 0").Having("$.Score > 1"));
        }
    }

    [Fact]
    public void ForUpdate_Offset_Limit_Do_Not_Throw()
    {
        var (db, col) = NewDb();
        using (db)
        {
            col.Insert(new Doc { Name = "x", Score = 1 });
            var query = col.Query().ForUpdate().Offset(0).Limit(10);
            var plan = query.GetPlan();
            Assert.NotNull(plan);
            Assert.True(plan.ContainsKey("$plan") || plan.Keys.Count > 0); // engine returns some plan doc
        }
    }
}
