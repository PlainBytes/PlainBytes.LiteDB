using AwesomeAssertions;
using LiteDB.Engine;
using Xunit;

namespace LiteDB.Tests.Engine
{
    public class Update_Tests
    {
        [Fact]
        public void Update_IndexNodes()
        {
            using (var db = new LiteEngine())
            {
                var doc = new BsonDocument {["_id"] = 1, ["name"] = "Mauricio", ["phones"] = new BsonArray() {"51", "11"}};

                db.Insert("col1", doc);

                db.EnsureIndex("col1", "idx_name", "name", false);
                db.EnsureIndex("col1", "idx_phones", "phones[*]", false);

                doc["name"] = "David";
                doc["phones"] = new BsonArray() {"11", "25"};

                db.Update("col1", doc);

                doc["name"] = "John";

                db.Update("col1", doc);
            }
        }

        [Fact]
        public void Update_ExtendBlocks()
        {
            using (var db = new LiteEngine())
            {
                var doc = new BsonDocument {["_id"] = 1, ["d"] = new byte[1000]};

                db.Insert("col1", doc);

                // small (same page)
                doc["d"] = new byte[300];

                db.Update("col1", doc);

                var page3 = db.GetPageLog(3);

                page3["freeBytes"].AsInt32.Should().Be(7828);

                // big (same page)
                doc["d"] = new byte[2000];

                db.Update("col1", doc);

                page3 = db.GetPageLog(3);

                page3["freeBytes"].AsInt32.Should().Be(6128);

                // big (extend page)
                doc["d"] = new byte[20000];

                db.Update("col1", doc);

                page3 = db.GetPageLog(3);
                var page4 = db.GetPageLog(4);
                var page5 = db.GetPageLog(5);

                page3["freeBytes"].AsInt32.Should().Be(0);
                page4["freeBytes"].AsInt32.Should().Be(0);
                page5["freeBytes"].AsInt32.Should().Be(4428);

                // small (shrink page)
                doc["d"] = new byte[10000];

                db.Update("col1", doc);

                page3 = db.GetPageLog(3);
                page4 = db.GetPageLog(4);
                page5 = db.GetPageLog(5);

                page3["freeBytes"].AsInt32.Should().Be(0);
                page4["freeBytes"].AsInt32.Should().Be(6278);
                page5["pageType"].AsString.Should().Be("Empty");
            }
        }

        [Fact]
        public void Update_Empty_Collection()
        {
            using(var e = new LiteEngine())
            {
                var d = new BsonDocument { ["_id"] = 1, ["a"] = "demo" };
                var r = e.Update("col1", new BsonDocument[] { d });

                r.Should().Be(0);
            }
        }
        
        public class Doc
        {
            [BsonId]
            public int Id { get; set; }
            public string Name { get; set; }
        }
        
        [Fact]
        public void Sequence_Should_Update_With_Explicit_Numeric_Id()
        {
            using var db = new LiteDatabase(":memory:");
            var col = db.GetCollection<Doc>("items", BsonAutoId.Int32);

            // Insert an explicit higher numeric id
            col.Insert(new Doc { Id = 100, Name = "a" });

            // Now insert without id, engine should continue sequence and assign 101
            var d2 = new Doc { Name = "b" };
            col.Insert(d2);

            d2.Id.Should().Be(101);

            // And another one should be 102
            var d3 = new Doc { Name = "c" };
            col.Insert(d3);

            d3.Id.Should().Be(102);

            // Validate stored ids
            var ids = col.Query().OrderBy(x => x.Id).Select(x => x.Id).ToArray();
            ids.Length.Should().Be(3);
            ids[0].Should().Be(100);
            ids[1].Should().Be(101);
            ids[2].Should().Be(102);
        }
    }
}