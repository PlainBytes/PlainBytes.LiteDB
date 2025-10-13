using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils.Extensions;

public class IOExceptionExtensions_Tests
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
    public void IsLocked_Detects_Sharing_And_Lock_Violation()
    {
        Assert.True(CreateWithWin32Error(32).IsLocked());
        Assert.True(CreateWithWin32Error(33).IsLocked());
        Assert.False(new IOException("x").IsLocked());
    }

    [Fact]
    public void WaitIfLocked_Waits_On_Lock_Else_Throws()
    {
        var locked = CreateWithWin32Error(32);
        locked.WaitIfLocked(1); // should not throw

        var notLocked = new IOException("x");
        Assert.Throws<IOException>(() => notLocked.WaitIfLocked(1));
    }
}
