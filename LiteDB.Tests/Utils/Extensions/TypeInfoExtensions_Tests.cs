using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace LiteDB.Tests.Utils.Extensions;

public class TypeInfoExtensions_Tests
{
    [Fact]
    public void IsAnonymousType_And_IsEnumerable()
    {
        var anon = new { A = 1 };
        Assert.True(anon.GetType().IsAnonymousType());

        Assert.True(typeof(int[]).IsEnumerable());
        Assert.True(typeof(List<string>).IsEnumerable());
        Assert.False(typeof(string).IsEnumerable());
        Assert.False(typeof(int).IsEnumerable());
    }
}
