using Xunit;

namespace PlainBytes.LiteDB.Tests.Client;

public class QueryAny_Tests
{
    [Fact]
    public void Any_Comparison_Expressions()
    {
        var qa = new QueryAny();
        Assert.Equal("$.tags[*] ANY=\"a\"", qa.EQ("tags", "a").Source);
        Assert.Equal("$.nums[*] ANY<5", qa.LT("nums", 5).Source);
        Assert.Equal("$.nums[*] ANY<=5", qa.LTE("nums", 5).Source);
        Assert.Equal("$.nums[*] ANY>5", qa.GT("nums", 5).Source);
        Assert.Equal("$.nums[*] ANY>=5", qa.GTE("nums", 5).Source);
        Assert.Equal("$.nums[*] ANY BETWEEN 1 AND 9", qa.Between("nums", 1, 9).Source);
        Assert.Equal("$.tags[*] ANY LIKE \"pre%\"", qa.StartsWith("tags", "pre").Source);
        Assert.Equal("$.nums[*] ANY!=0", qa.Not("nums", 0).Source);
    }
}