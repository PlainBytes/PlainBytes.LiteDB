using System;
using PlainBytes.LiteDB.Utils;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils;

public class TryCatch_Tests
{
    [Fact]
    public void Catch_Adds_Exceptions_Without_Throwing()
    {
        var tc = new TryCatch();
        tc.Catch(() => throw new InvalidOperationException("boom"));
        tc.Catch(() => { /* no-op */ });

        Assert.Single(tc.Exceptions, ex => ex is InvalidOperationException);
    }

    [Fact]
    public void InvalidDatafileState_Detected_From_LiteException()
    {
        var tc = new TryCatch();
        tc.Catch(() => throw LiteException.InvalidDatafileState("bad"));
        Assert.True(tc.InvalidDatafileState);
    }

    [Fact]
    public void Ctor_With_Initial_Exception_Populates_List()
    {
        var ex = new Exception("x");
        var tc = new TryCatch(ex);
        Assert.Same(ex, Assert.Single(tc.Exceptions));
        Assert.False(tc.InvalidDatafileState);
    }
}
