using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Client.Collections
{
    public class Collections_Tests
    {
        private class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<string> Tags { get; set; } = new List<string>();
            public int Value { get; set; }
        }

        private ILiteCollection<Item> NewCollection(out LiteDatabase db)
        {
            db = new LiteDatabase(new MemoryStream());
            return db.GetCollection<Item>("items");
        }

        [Fact]
        public void Insert_Update_Upsert_Basic_Flows()
        {
            var col = NewCollection(out var db);
            using (db)
            {
                // Insert single returns id and sets entity id when auto id
                var a = new Item { Name = "A", Value = 1 };
                var idA = col.Insert(a);
                Assert.Equal(a.Id, idA.AsInt32);
                Assert.True(idA.IsInt32);
                Assert.Equal(1, col.Count());

                // Insert with explicit id
                var b = new Item { Name = "B", Value = 2 };
                col.Insert(10, b);
                Assert.Equal(2, col.Count());
                Assert.NotNull(col.FindById(10));

                // Insert many
                var items = new[]
                {
                    new Item { Name = "C", Value = 3 },
                    new Item { Name = "D", Value = 4 }
                };
                var inserted = col.Insert(items);
                Assert.Equal(2, inserted);
                Assert.Equal(4, col.Count());

                // Update by entity
                a.Value = 11;
                Assert.True(col.Update(a));
                Assert.Equal(11, col.FindById(a.Id).Value);

                // Update by id + entity
                b.Value = 22;
                Assert.True(col.Update(10, b));
                Assert.Equal(22, col.FindById(10).Value);

                // Upsert existing (returns false per engine semantics when only update)
                a.Value = 111;
                var upA = col.Upsert(a);
                Assert.False(upA);
                Assert.Equal(111, col.FindById(a.Id).Value);

                // Upsert new (returns true when inserted)
                var e = new Item { Name = "E", Value = 5 };
                var upE = col.Upsert(e);
                Assert.True(upE);
                Assert.NotNull(col.FindById(e.Id));

                // Upsert with id
                var f = new Item { Name = "F", Value = 6 };
                Assert.True(col.Upsert(77, f));
                Assert.Equal(6, col.FindById(77).Value);
            }
        }

        [Fact]
        public void Find_Query_FindOne_FindAll_Work()
        {
            var col = NewCollection(out var db);
            using (db)
            {
                col.Insert(new []{
                    new Item { Name = "Ann", Value = 1 },
                    new Item { Name = "Bob", Value = 2 },
                    new Item { Name = "Ana", Value = 3 },
                });

                // Ensure index to help query
                col.EnsureIndex(x => x.Name);

                // Find with expression
                var aNames = col.Find(x => x.Name.StartsWith("An")).ToList();
                Assert.Equal(2, aNames.Count);

                // Find with BsonExpression and skip/limit
                var some = col.Find(Query.StartsWith("Name", "A"), skip: 1, limit: 1).ToList();
                Assert.Single(some);

                // FindById
                var one = col.FindOne(x => x.Value == 2);
                Assert.Equal("Bob", one.Name);
                Assert.Equal(one.Id, col.FindById(one.Id).Id);

                // FindAll
                var all = col.FindAll().ToList();
                Assert.Equal(3, all.Count);
            }
        }

        [Fact]
        public void Aggregates_Count_LongCount_Exists_Min_Max()
        {
            var col = NewCollection(out var db);
            using (db)
            {
                col.Insert(new []{
                    new Item { Name = "A", Value = 5 },
                    new Item { Name = "B", Value = 2 },
                    new Item { Name = "C", Value = 9 },
                });

                Assert.Equal(3, col.Count());
                Assert.Equal(2, col.Count(x => x.Value < 6));
                Assert.Equal(3, col.LongCount());
                Assert.Equal(2, col.LongCount(x => x.Value < 6));
                Assert.True(col.Exists(x => x.Value == 2));
                Assert.False(col.Exists(x => x.Value == 7));

                // Min/Max using expressions
                Assert.Equal(2, col.Min(x => x.Value));
                Assert.Equal(9, col.Max(x => x.Value));

                // Min/Max using bson expressions
                Assert.Equal(2, col.Min("$.Value").AsInt32);
                Assert.Equal(9, col.Max("$.Value").AsInt32);
            }
        }

        [Fact]
        public void Indexes_Enumerable_Key_Expansion_And_Drop()
        {
            var col = NewCollection(out var db);
            using (db)
            {
                col.Insert(new []{
                    new Item { Name = "A", Tags = new List<string> { "x", "y" } },
                    new Item { Name = "B", Tags = new List<string> { "z" } },
                });

                // Ensure index over IEnumerable property - should expand to [*]
                Assert.True(col.EnsureIndex(x => x.Tags));

                // Query using expanded path
                var q1 = col.Count(Query.Any().EQ("Tags[*]", "y"));
                Assert.Equal(1, q1);

                // Ensure index by name based on expression
                Assert.True(col.EnsureIndex("NameLower", "LOWER($.Name)"));

                // Drop index
                Assert.True(col.DropIndex("NameLower"));
            }
        }

        [Fact]
        public void Include_Returns_New_Collection_And_Null_Guards()
        {
            var col = NewCollection(out var db);
            using (db)
            {
                // Include with expression returns a new instance
                var col2 = col.Include(x => x.Name);
                Assert.NotSame(col, col2);

                // Null guards
                Assert.Throws<ArgumentNullException>(() => col.Include<string>(null));
                Assert.Throws<NullReferenceException>(() => col.Include((BsonExpression)null));
            }
        }

        [Fact]
        public void Argument_Validation_Guards()
        {
            var col = NewCollection(out var db);
            using (db)
            {
                Assert.Throws<ArgumentNullException>(() => col.Insert((Item)null));
                Assert.Throws<ArgumentNullException>(() => col.Insert((IEnumerable<Item>)null));
                Assert.Throws<ArgumentNullException>(() => col.Upsert((Item)null));
                Assert.Throws<ArgumentNullException>(() => col.Upsert((IEnumerable<Item>)null));
                Assert.Throws<ArgumentNullException>(() => col.Update((Item)null));
                Assert.Throws<ArgumentNullException>(() => col.Update((IEnumerable<Item>)null));
                Assert.Throws<ArgumentNullException>(() => col.Update(0, null));
                Assert.Throws<ArgumentNullException>(() => col.Upsert(0, null));
                Assert.Throws<ArgumentNullException>(() => col.Find((BsonExpression)null));
                Assert.Throws<ArgumentNullException>(() => col.Find((Query)null));
                Assert.Throws<ArgumentNullException>(() => col.FindById(BsonValue.Null));
                Assert.Throws<ArgumentNullException>(() => col.FindOne((BsonExpression)null));
            }
        }

        [Fact]
        public void UpdateMany_With_Document_Transform_Works_And_Invalid_Throws()
        {
            var col = NewCollection(out var db);
            using (db)
            {
                col.Insert(new []{
                    new Item { Name = "john", Value = 1 },
                    new Item { Name = "ana", Value = 2 }
                });

                // Invalid transform (non-document) should throw
                Assert.Throws<ArgumentException>(() => col.UpdateMany("LOWER($.Name)", "_id > 0"));

                // Valid transform document using BsonExpression
                var transform = BsonExpression.Create("{ Name: UPPER($.Name), Value: $.Value }");
                var pred = BsonExpression.Create("_id > 0");
                var updated = col.UpdateMany(transform, pred);
                Assert.Equal(2, updated);
                Assert.All(col.FindAll(), x => Assert.Equal(x.Name.ToUpperInvariant(), x.Name));

                // Valid transform using LINQ expression returning an anonymous type (document)
                var updated2 = col.UpdateMany(x => new Item { Name = x.Name.ToLowerInvariant(), Value = x.Value }, x => x.Value > 0);
                Assert.Equal(2, updated2);
                Assert.All(col.FindAll(), x => Assert.Equal(x.Name.ToLowerInvariant(), x.Name));
            }
        }
    }
}
