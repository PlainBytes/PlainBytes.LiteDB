using System;
using System.IO;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils.Extensions;

public class StreamExtensions_Tests
{
    [Fact]
    public void FlushToDisk_MemoryStream_Flushes()
    {
        using var ms = new MemoryStream();
        ms.WriteByte(1);
        ms.FlushToDisk();
        Assert.True(ms.Length > 0);
    }

    [Fact]
    public void FlushToDisk_FileStream_Calls_FlushTrue()
    {
        var path = Path.GetTempFileName();
        try
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.WriteByte(2);
                fs.FlushToDisk();
                Assert.True(fs.Length > 0);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }
}
