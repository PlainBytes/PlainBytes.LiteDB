using System;
using System.Linq;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils.Extensions;

public class LinqExtensions_Tests
{
    [Fact]
    public void Batch_Splits_Sequence()
    {
        var input = Enumerable.Range(1, 7).ToArray();
        var batches = input.Batch(3).Select(b => b.ToArray()).ToArray();

        Assert.Equal(3, batches.Length);
        Assert.Equal(new[] {1,2,3}, batches[0]);
        Assert.Equal(new[] {4,5,6}, batches[1]);
        Assert.Equal(new[] {7}, batches[2]);
    }

    [Fact]
    public void DistinctBy_Uses_Key_Selector_And_Comparer()
    {
        var input = new[] {"a", "A", "b"};
        var res = input.DistinctBy(s => s, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.Equal(new[] {"a", "b"}, res);
    }

    [Fact]
    public void IsLast_Marks_Last_Element()
    {
        var input = new[] {"x", "y"};
        var res = input.IsLast().ToArray();
        Assert.Equal(2, res.Length);
        Assert.Equal("x", res[0].Item);
        Assert.False(res[0].IsLast);
        Assert.Equal("y", res[1].Item);
        Assert.True(res[1].IsLast);
    }
}
