using System;
using LiteDB;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace LiteDB.Tests.Client;

public class LiteRepository_Tests
{
    private class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    private LiteRepository NewRepo(out LiteDatabase db)
    {
        db = new LiteDatabase(new MemoryStream());
        return new LiteRepository(db);
    }

    [Fact]
    public void Insert_Update_Upsert_Delete_And_Query_Wrappers_Work()
    {
        var repo = NewRepo(out var db);
        using (repo)
        using (db)
        {
            // Implicit collection name by type
            var id = repo.Insert(new Person { Name = "A", Age = 10 });
            Assert.True(id.IsInt32);

            // Explicit collection name
            var id2 = repo.Insert(new Person { Name = "B", Age = 20 }, collectionName: "people");
            Assert.True(id2.IsInt32);

            // Update (by entity)
            var a = repo.First<Person>(x => x.Name == "A");
            a.Age = 11;
            Assert.True(repo.Update(a));
            Assert.Equal(11, repo.Single<Person>(x => x.Name == "A").Age);

            // Upsert existing returns false
            a.Age = 12;
            Assert.False(repo.Upsert(a));
            Assert.Equal(12, repo.Single<Person>(x => x.Name == "A").Age);

            // Upsert new returns true
            var c = new Person { Name = "C", Age = 30 };
            Assert.True(repo.Upsert(c));
            Assert.NotNull(repo.Single<Person>(x => x.Id == c.Id));

            // Upsert many
            var inserted = repo.Upsert((IEnumerable<Person>)new[] { new Person { Name = "D", Age = 40 }, new Person { Name = "E", Age = 50 } });
            Assert.Equal(2, inserted);

            // Delete by id (explicit collection name)
            var b = repo.Single<Person>(x => x.Name == "B", collectionName: "people");
            Assert.True(repo.Delete<Person>(b.Id, collectionName: "people"));
            Assert.Null(repo.SingleOrDefault<Person>(x => x.Name == "B", collectionName: "people"));

            // Query wrapper should enumerate
            var adults = repo.Query<Person>().Where(x => x.Age >= 30).ToList();
            Assert.Equal(3, adults.Count);
        }
    }

    [Fact]
    public void Fetch_First_Single_Variants_Work()
    {
        var repo = NewRepo(out var db);
        using (repo)
        using (db)
        {
            repo.Insert((IEnumerable<Person>)new[]
            {
                new Person { Name = "A", Age = 10 },
                new Person { Name = "B", Age = 20 },
                new Person { Name = "C", Age = 30 }
            });

            var list = repo.Fetch<Person>(x => x.Age >= 20);
            Assert.Equal(2, list.Count);

            Assert.Equal("B", repo.First<Person>(x => x.Age >= 20).Name);
            Assert.Equal("B", repo.FirstOrDefault<Person>(x => x.Age >= 20).Name);

            Assert.Equal("C", repo.Single<Person>(x => x.Age == 30).Name);
            Assert.Null(repo.SingleOrDefault<Person>(x => x.Age == 99));
        }
    }

    [Fact]
    public void EnsureIndex_Delegates_To_Collection()
    {
        var repo = NewRepo(out var db);
        using (repo)
        using (db)
        {
            var created = repo.EnsureIndex<Person, string>(x => x.Name);
            Assert.True(created);

            // by name and expression
            var created2 = repo.EnsureIndex<Person>("AgeIdx", BsonExpression.Create("$.Age"), unique: false);
            Assert.True(created2);
        }
    }

    [Fact]
    public void SingleById_Uses_Collection()
    {
        var repo = NewRepo(out var db);
        using (repo)
        using (db)
        {
            var p = new Person { Name = "A", Age = 1 };
            repo.Insert(p);
            var loaded = repo.SingleById<Person>(p.Id);
            Assert.Equal(p.Name, loaded.Name);
        }
    }
}
