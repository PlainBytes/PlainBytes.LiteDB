using System;
using System.Linq;
using System.Text;
using Xunit;

namespace LiteDB.Tests.Utils.Extensions;

public class BufferExtensions_Tests
{
    [Fact]
    public void BinaryCompareTo_Works_With_Nulls_And_Content()
    {
        byte[] a = null;
        byte[] b = new byte[] { };
        Assert.Equal(-1, a.BinaryCompareTo(b));
        Assert.Equal(1, b.BinaryCompareTo(null));

        var x = new byte[] { 1, 2, 3 };
        var y = new byte[] { 1, 2, 4 };
        Assert.Equal(-1, x.BinaryCompareTo(y));
        Assert.Equal(1, y.BinaryCompareTo(x));
        Assert.Equal(0, x.BinaryCompareTo(new byte[] {1,2,3}));
    }

    [Fact]
    public void IsFullZero_And_Fill()
    {
        var arr = new byte[35];
        Assert.True(arr.IsFullZero());
        arr.Fill(0xFF, 5, 10);
        Assert.False(arr.IsFullZero());
        Assert.True(arr.Skip(5).Take(10).All(b => b == 0xFF));
    }

    [Fact]
    public void ReadCString_Reads_Until_Null()
    {
        var s = "hello";
        var bytes = Encoding.UTF8.GetBytes(s + "\0 world");
        var str = bytes.ReadCString(0, out var len);
        Assert.Equal("hello", str);
        Assert.Equal(5, len);
    }

    [Fact]
    public void ToBytes_Primitives_Match_BitConverter()
    {
        var buf = new byte[32];
        short i16 = -1234; i16.ToBytes(buf, 0);
        int i32 = 0x12345678; i32.ToBytes(buf, 2);
        long i64 = 0x0102030405060708; i64.ToBytes(buf, 6);
        float f = 123.5f; f.ToBytes(buf, 14);
        double d = Math.PI; d.ToBytes(buf, 18);

        Assert.Equal(BitConverter.GetBytes(i16), buf.Take(2).ToArray());
        Assert.Equal(BitConverter.GetBytes(i32), buf.Skip(2).Take(4).ToArray());
        Assert.Equal(BitConverter.GetBytes(i64), buf.Skip(6).Take(8).ToArray());
        Assert.Equal(BitConverter.GetBytes(f), buf.Skip(14).Take(4).ToArray());
        Assert.Equal(BitConverter.GetBytes(d), buf.Skip(18).Take(8).ToArray());
    }
}
