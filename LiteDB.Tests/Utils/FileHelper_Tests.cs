using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace LiteDB.Tests.Utils;

public class FileHelper_Tests
{
    private static IOException CreateWithWin32Error(int win32)
    {
        // set HResult low word to desired Win32 code
        var ex = new IOException("test");
        typeof(Exception).GetField("_HResult", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(ex, unchecked((int)0x80070000) | win32);
        return ex;
    }

    [Fact]
    public void GetSuffixFile_Appends_And_Increments_When_Exists()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            var baseFile = Path.Combine(dir, "data.db");
            var temp1 = FileHelper.GetSuffixFile(baseFile, "-temp", checkIfExists: true);
            Assert.Equal(Path.Combine(dir, "data-temp.db"), temp1);

            // create the first to force increment
            File.WriteAllText(temp1, "x");
            var temp2 = FileHelper.GetSuffixFile(baseFile, "-temp", checkIfExists: true);
            Assert.Equal(Path.Combine(dir, "data-temp-1.db"), temp2);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void GetLog_And_Temp_File_Suffixes()
    {
        var baseFile = "/tmp/myfile.db";
        Assert.Equal("/tmp/myfile-log.db", FileHelper.GetLogFile(baseFile));
        Assert.Equal("/tmp/myfile-tmp.db", FileHelper.GetTempFile(baseFile));
    }

    [Fact]
    public void IsFileLocked_Detects_Lock_State()
    {
        var path = Path.GetTempFileName();
        try
        {
            // initially not locked
            Assert.False(FileHelper.IsFileLocked(path));

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                if (OperatingSystem.IsWindows())
                {
                    Assert.True(FileHelper.IsFileLocked(path));
                }
                else
                {
                    // On non-Windows platforms, file sharing may not prevent reopen, so just ensure call does not throw
                    _ = FileHelper.IsFileLocked(path);
                }
            }

            Assert.False(FileHelper.IsFileLocked(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TryExec_Retries_On_Locked_IOException_Then_Succeeds()
    {
        int attempts = 0;
        bool result = FileHelper.TryExec(1, () =>
        {
            attempts++;
            if (attempts < 3)
            {
                throw CreateWithWin32Error(32); // locked
            }
        });

        Assert.True(result);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public void TryExec_Returns_False_If_Timeout_Elapsed()
    {
        int attempts = 0;
        bool result = FileHelper.TryExec(0, () =>
        {
            attempts++;
            throw CreateWithWin32Error(32);
        });

        Assert.False(result);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public void Exec_Retries_Then_Succeeds()
    {
        int attempts = 0;
        FileHelper.Exec(1, () =>
        {
            attempts++;
            if (attempts < 2)
            {
                throw CreateWithWin32Error(33);
            }
        });

        Assert.Equal(2, attempts);
    }

    [Fact]
    public void Exec_Throws_Last_Exception_On_Timeout()
    {
        Assert.Throws<IOException>(() => FileHelper.Exec(0, () => throw CreateWithWin32Error(32)));
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("1k", 1024L)]
    [InlineData("2K", 2L * 1024)]
    [InlineData("1m", 1024L * 1024)]
    [InlineData("3 gB", 3L * 1024 * 1024 * 1024)]
    [InlineData("1t", 1L * 1024 * 1024 * 1024 * 1024)]
    [InlineData("80000", 80000L)]
    public void ParseFileSize_Parses_Units(string input, long expected)
    {
        Assert.Equal(expected, FileHelper.ParseFileSize(input));
    }

    [Fact]
    public void ParseFileSize_Invalid_Returns_Zero()
    {
        Assert.Equal(0, FileHelper.ParseFileSize("abc"));
    }

    [Theory]
    [InlineData(0, "0B")]
    [InlineData(1023, "1023B")]
    [InlineData(1024, "1KB")]
    [InlineData(1536, "1.5KB")]
    [InlineData(1048576, "1MB")]
    public void FormatFileSize_Formats(long bytes, string expected)
    {
        Assert.Equal(expected, FileHelper.FormatFileSize(bytes));
    }
}
