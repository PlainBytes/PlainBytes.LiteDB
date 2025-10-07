using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Xunit;

namespace LiteDB.Tests.Mapper
{
    public class BsonMapper_Tests
    {
        private readonly BsonMapper _mapper = new BsonMapper();

        private class Address
        {
            public string StreetName { get; set; }
        }

        private class Person
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public Address HomeAddress { get; set; }
        }

        private enum Color
        {
            Red = 1,
            Green = 2
        }

        private class WithEnum
        {
            public int Id { get; set; }
            public Color Favorite { get; set; }
        }

        public class Customer
        {
            [BsonId]
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Order
        {
            [BsonId]
            public int OrderId { get; set; }

            [BsonRef("customers")]
            public Customer Customer { get; set; }

            [BsonRef("customers")]
            public List<Customer> Customers { get; set; }
        }

        [Fact]
        public void UseCamelCase_Should_Apply_To_Field_Names()
        {
            var p = new Person
            {
                Id = 1,
                FirstName = "John",
                HomeAddress = new Address { StreetName = "Main" }
            };

            _mapper.UseCamelCase();

            var doc = _mapper.ToDocument(p);
            // Id becomes _id
            doc.Keys.Should().Contain("_id");
            doc.Keys.Should().Contain("firstName");
            doc.Keys.Should().Contain("homeAddress");

            var addr = doc["homeAddress"].AsDocument;
            addr.Keys.Should().Contain("streetName");
        }

        [Fact]
        public void UseLowerCaseDelimiter_Should_Apply_To_Field_Names()
        {
            var p = new Person
            {
                Id = 2,
                FirstName = "Jane",
                HomeAddress = new Address { StreetName = "Second" }
            };

            _mapper.UseLowerCaseDelimiter('_');

            var doc = _mapper.ToDocument(p);
            doc.Keys.Should().Contain("_id");
            doc.Keys.Should().Contain("first_name");
            doc.Keys.Should().Contain("home_address");
            doc["home_address"].AsDocument.Keys.Should().Contain("street_name");
        }

        [Fact]
        public void TrimWhitespace_And_EmptyStringToNull_Should_Affect_String_Serialization()
        {
            // Defaults: TrimWhitespace=true, EmptyStringToNull=true
            _mapper.TrimWhitespace.Should().Be(true);
            _mapper.EmptyStringToNull.Should().Be(true);

            var v1 = _mapper.Serialize(typeof(string), "  hello  ");
            v1.Type.Should().Be(BsonType.String);
            v1.AsString.Should().Be("hello");

            var v2 = _mapper.Serialize(typeof(string), "   ");
            v2.IsNull.Should().Be(true);

            // Turn off both and re-check
            _mapper.TrimWhitespace = false;
            _mapper.EmptyStringToNull = false;

            var v3 = _mapper.Serialize(typeof(string), "  ok  ");
            v3.Type.Should().Be(BsonType.String);
            v3.AsString.Should().Be("  ok  ");

            var v4 = _mapper.Serialize(typeof(string), "");
            v4.Type.Should().Be(BsonType.String);
            v4.AsString.Should().Be("");
        }

        private class WithNulls
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void SerializeNullValues_Should_Include_Or_Exclude_Nulls()
        {
            var obj = new WithNulls { Id = 10, Name = null };

            // default SerializeNullValues=false -> omit nulls (except _id)
            var doc1 = _mapper.ToDocument(obj);
            doc1.Keys.Should().Contain("_id");
            doc1.ContainsKey("Name").Should().Be(false);

            // include
            _mapper.SerializeNullValues = true;
            var doc2 = _mapper.ToDocument(obj);
            doc2.Keys.Should().Contain("_id");
            doc2.ContainsKey("Name").Should().Be(true);
            doc2["Name"].IsNull.Should().Be(true);
        }

        [Fact]
        public void EnumAsInteger_Should_Toggle_Enum_Serialization()
        {
            var e = new WithEnum { Id = 1, Favorite = Color.Green };

            _mapper.EnumAsInteger = false;
            var d1 = _mapper.ToDocument(e);
            d1["Favorite"].Type.Should().Be(BsonType.String);
            d1["Favorite"].AsString.Should().Be(Color.Green.ToString());

            _mapper.EnumAsInteger = true;
            var d2 = _mapper.ToDocument(e);
            d2["Favorite"].Type.Should().Be(BsonType.Int32);
            d2["Favorite"].AsInt32.Should().Be((int)Color.Green);
        }

        private struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        [Fact]
        public void RegisterType_Should_Use_Custom_Serializer_And_Deserializer()
        {
            var mapper = new BsonMapper();
            mapper.RegisterType<Point>(
                p => new BsonValue($"{p.X},{p.Y}"),
                b => {
                    var parts = b.AsString.Split(',');
                    return new Point { X = int.Parse(parts[0]), Y = int.Parse(parts[1]) };
                });

            var p = new Point { X = 3, Y = 5 };
            var v = mapper.Serialize(typeof(Point), p);
            v.Type.Should().Be(BsonType.String);
            v.AsString.Should().Be("3,5");

            var back = (Point)mapper.Deserialize(typeof(Point), v);
            back.X.Should().Be(3);
            back.Y.Should().Be(5);
        }

        [Fact]
        public void ResolveCollectionName_Default_And_Custom()
        {
            // default: for enumerable, returns underlying type name
            var nameDefault = _mapper.ResolveCollectionName(typeof(List<Customer>));
            nameDefault.Should().Be("Customer");

            // custom
            _mapper.ResolveCollectionName = t => "prefix_" + (Reflection.IsEnumerable(t) ? Reflection.GetListItemType(t).Name : t.Name).ToLower();
            _mapper.ResolveCollectionName(typeof(List<Customer>)).Should().Be("prefix_customer");
            _mapper.ResolveCollectionName(typeof(Customer)).Should().Be("prefix_customer");
        }

        [Fact]
        public void DbRef_Single_And_List_Should_Serialize_And_Deserialize()
        {
            var order = new Order
            {
                OrderId = 99,
                Customer = new Customer { Id = 5, Name = "C5" },
                Customers = new List<Customer>
                {
                    new Customer { Id = 1, Name = "C1" },
                    new Customer { Id = 2, Name = "C2" }
                }
            };

            var doc = _mapper.ToDocument(order);

            // Single
            var custDoc = doc["Customer"].AsDocument;
            custDoc["$id"].AsInt32.Should().Be(5);
            custDoc["$ref"].AsString.Should().Be("customers");

            // List
            var arr = doc["Customers"].AsArray;
            arr.Count.Should().Be(2);
            arr[0]["$id"].AsInt32.Should().Be(1);
            arr[1]["$id"].AsInt32.Should().Be(2);
            arr[0]["$ref"].AsString.Should().Be("customers");

            // Roundtrip deserialize
            var order2 = _mapper.Deserialize<Order>(doc);
            order2.OrderId.Should().Be(99);
            Assert.NotNull(order2.Customer);
            order2.Customer.Id.Should().Be(5);
            Assert.NotNull(order2.Customers);
            order2.Customers.Count.Should().Be(2);
            var ids = order2.Customers.Select(x => x.Id).OrderBy(x => x).ToArray();
            ids.Length.Should().Be(2);
            ids[0].Should().Be(1);
            ids[1].Should().Be(2);
        }
    }
}
