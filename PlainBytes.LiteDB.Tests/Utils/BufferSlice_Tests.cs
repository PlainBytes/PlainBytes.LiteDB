using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils;

public class BufferSlice_Tests
{
    [Fact]
    public void Indexer_Get_Set_And_ToArray()
    {
        var arr = new byte[10];
        var slice = new BufferSlice(arr, 2, 5); // covers indices 2..6

        slice[0] = 10;
        slice[4] = 20;

        Assert.Equal(10, arr[2]);
        Assert.Equal(20, arr[6]);
        Assert.Equal(new byte[] { 10, 0, 0, 0, 20 }, slice.ToArray());
    }

    [Fact]
    public void Clear_And_Clear_Range()
    {
        var arr = new byte[8];
        var slice = new BufferSlice(arr, 1, 6);
        slice.Fill(0x7F);
        Assert.True(slice.All(0x7F));

        slice.Clear();
        Assert.True(slice.All(0));

        slice.Fill(1);
        slice.Clear(2, 3); // clear middle part
        Assert.Equal(new byte[] { 1, 1, 0, 0, 0, 1 }, slice.ToArray());
    }

    [Fact]
    public void Clear_Range_Out_Of_Bounds_Throws_LiteException()
    {
        var arr = new byte[4];
        var slice = new BufferSlice(arr, 0, 4);
        Assert.Throws<LiteException>(() => slice.Clear(3, 2)); // 3+2 > 4
    }

    [Fact]
    public void Slice_Creates_New_View()
    {
        var arr = new byte[] { 0, 1, 2, 3, 4, 5 };
        var slice = new BufferSlice(arr, 1, 4); // [1,2,3,4]
        var sub = slice.Slice(1, 2); // [2,3]
        Assert.Equal(new byte[] { 2, 3 }, sub.ToArray());

        sub[1] = 9;
        Assert.Equal(9, arr[1 + 1 + 1]); // original backing updated at index 3
    }

    [Fact]
    public void ToHex_Formats_Bytes_In_Lines()
    {
        var arr = new byte[] { 0x00, 0x01, 0x0A };
        var slice = new BufferSlice(arr, 0, 3);
        var hex = slice.ToHex();
        // expected line with trailing space and newline
        Assert.Equal("00 01 0A \n", hex.Replace("\r\n", "\n"));
    }

    [Fact]
    public void ToString_Shows_Offset_And_Count()
    {
        var slice = new BufferSlice(new byte[5], 2, 3);
        Assert.Contains("Offset: 2", slice.ToString());
        Assert.Contains("Count: 3", slice.ToString());
    }
}
