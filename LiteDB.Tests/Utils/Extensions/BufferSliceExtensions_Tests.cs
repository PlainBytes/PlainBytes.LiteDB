using System;
using System.Linq;
using System.Text;
using Xunit;

namespace LiteDB.Tests.Utils.Extensions;

public class BufferSliceExtensions_Tests
{
    [Fact]
    public void WriteRead_Int32_Guid_String()
    {
        var raw = new byte[256];
        var slice = new BufferSlice(raw, 10, 200);

        // Int32
        slice.Write(0x12345678, 0);
        Assert.Equal(0x12345678, slice.ReadInt32(0));

        // Guid
        var g = Guid.NewGuid();
        slice.Write(g, 8);
        Assert.Equal(g, slice.ReadGuid(8));

        // String
        var s = "héllo"; // include multibyte
        var bytes = Encoding.UTF8.GetByteCount(s);
        slice.Write(s, 40);
        Assert.Equal(s, slice.ReadString(40, bytes));
    }
}
