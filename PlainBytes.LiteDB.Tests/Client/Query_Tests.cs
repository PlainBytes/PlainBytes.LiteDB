using Xunit;

namespace PlainBytes.LiteDB.Tests.Client;

public class Query_Tests
{
    [Fact]
    public void All_Defaults()
    {
        var q0 = Query.All();
        Assert.NotNull(q0);

        var q1 = Query.All();
        var q2 = Query.All("name", Query.Descending);
        Assert.Equal("$.name", q2.OrderBy.Source);
        Assert.Equal(Query.Descending, q2.Order);

        var q3 = Query.All(Query.Ascending);
        Assert.Equal("$._id", q3.OrderBy.Source);
        Assert.Equal(Query.Ascending, q3.Order);
    }

    [Fact]
    public void Comparison_Expressions_Render_Correct_Source()
    {
        Assert.Equal("$.age=10", Query.EQ("age", 10).Source);
        Assert.Equal("$.age<10", Query.LT("age", 10).Source);
        Assert.Equal("$.age<=10", Query.LTE("age", 10).Source);
        Assert.Equal("$.age>10", Query.GT("age", 10).Source);
        Assert.Equal("$.age>=10", Query.GTE("age", 10).Source);
        Assert.Equal("$.age BETWEEN 1 AND 10", Query.Between("age", 1, 10).Source);
    }

    [Fact]
    public void String_Expressions_Render_With_Like()
    {
        Assert.Equal("$.name LIKE \"Jo%\"", Query.StartsWith("name", "Jo").Source);
        Assert.Equal("$.name LIKE \"%abc%\"", Query.Contains("name", "abc").Source);
    }

    [Fact]
    public void In_And_Not_Expressions()
    {
        Assert.Equal("$.status!=null", Query.Not("status", null).Source);

        var arr = new BsonArray { 1, 2, 3 };
        Assert.Equal("$.age IN [1,2,3]", Query.In("age", arr).Source);

        var expr = Query.In("age", 1, 2, 3);
        Assert.Equal("$.age IN [1,2,3]", expr.Source);
    }

    [Fact]
    public void And_Or_Compose_Expressions()
    {
        var a = Query.EQ("age", 10);
        var b = Query.EQ("name", "Ana");

        var and = Query.And(a, b);
        Assert.Equal("($.age=10 AND $.name=\"Ana\")", and.Source);

        var or = Query.Or(a, b);
        Assert.Equal("($.age=10 OR $.name=\"Ana\")", or.Source);
    }
}