using System.Collections.Generic;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils.Extensions;

public class HashSetExtensions_Tests
{
    [Fact]
    public void AddRange_Adds_All_And_Ignores_Null()
    {
        var set = new HashSet<int>();
        set.AddRange(null);
        Assert.Empty(set);

        set.AddRange(new[] { 1, 2, 2, 3 });
        Assert.Equal(3, set.Count);
        Assert.Contains(1, set);
        Assert.Contains(2, set);
        Assert.Contains(3, set);
    }
}
