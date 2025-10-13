using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Mapper
{
    public class LinqExpressionVisitor_Tests
    {
        private readonly BsonMapper _mapper = new BsonMapper();

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Active { get; set; }
            public List<int> Scores { get; set; }
        }

        [Fact]
        public void GetExpression_vs_GetIndexExpression_Should_Differ_On_Predicate()
        {
            // When resolving as predicate, visitor auto-appends "= true" for path/call/parameter
            var exprPredicate = _mapper.GetExpression<User, bool>(x => x.Active);
            exprPredicate.Source.Should().Be("($.Active=true)");

            // When resolving as index expression, predicate flag is false and no auto-append happens
            var exprIndex = _mapper.GetIndexExpression<User, bool>(x => x.Active);
            exprIndex.Source.Should().Be("$.Active");
        }

        [Fact]
        public void Predicate_Method_Call_Should_AutoAppend_True()
        {
            var expr = _mapper.GetExpression<User, bool>(x => x.Name.StartsWith("A"));
            expr.Source.Should().Be("$.Name LIKE (@p0+\"%\")");
            expr.Parameters["p0"].Should().Be("A");
        }

        [Fact]
        public void Any_All_Invalid_Left_Side_Should_Throw()
        {
            // n + 1 on the left side is not a simple parameter
            Assert.Throws<LiteException>(() => _mapper.GetExpression<User, bool>(x => x.Scores.Select(n => n).All(n => (n + 1) > 0)));
        }

        [Fact]
        public void Any_Method_Not_Supported_In_Predicate_Should_Throw()
        {
            // GetHashCode is not translatable inside Any/All according to visitor rules
            Assert.Throws<LiteException>(() => _mapper.GetExpression<User, bool>(x => x.Scores.Select(n => n).Any(n => n.GetHashCode() > 0)));
        }
    }
}
