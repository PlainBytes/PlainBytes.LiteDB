using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LiteDB.Tests.Utils.Extensions;

public class ExpressionExtensions_Tests
{
    private class Foo
    {
        public string Name { get; set; } = string.Empty;
        public Bar Bar { get; set; } = new Bar();
        public List<Bar> Bars { get; set; } = new();
        public Bar[] Arr { get; set; } = Array.Empty<Bar>();
        public MyEnum E { get; set; }
    }
    private class Bar { public string Street { get; set; } = string.Empty; }
    private enum MyEnum { A = 1, B = 2 }

    [Fact]
    public void GetPath_Simple_Property()
    {
        Expression<Func<Foo, object>> e = x => x.Name;
        Assert.Equal("Name", e.GetPath());
    }

    [Fact]
    public void GetPath_List_Select_Property()
    {
        Expression<Func<Foo, object>> e = x => x.Bars.Select(z => z.Street);
        Assert.Equal("Bars.Street", e.GetPath());
    }

    [Fact]
    public void GetPath_Array_Index_Property()
    {
        Expression<Func<Foo, object>> e = x => x.Arr[0].Street;
        Assert.Equal("Arr.Street", e.GetPath());
    }

    [Fact]
    public void GetPath_Enum_With_Convert()
    {
        Expression<Func<Foo, object>> e = x => (int)x.E;
        Assert.Equal("E, Int32", e.GetPath());
    }
}
