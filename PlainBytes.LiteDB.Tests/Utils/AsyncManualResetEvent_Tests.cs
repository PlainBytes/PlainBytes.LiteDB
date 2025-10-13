using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PlainBytes.LiteDB.Tests.Utils;

public class AsyncManualResetEvent_Tests
{
    [Fact]
    public async Task WaitAsync_Blocks_Until_Set()
    {
        var ev = new AsyncManualResetEvent();
        var waitTask = ev.WaitAsync();

        // should not complete within short timeout
        var completed = await Task.WhenAny(waitTask, Task.Delay(50)) == waitTask;
        Assert.False(completed);

        ev.Set();
        await waitTask; // now completes
    }

    [Fact]
    public async Task Reset_Reblocks_After_Set()
    {
        var ev = new AsyncManualResetEvent();
        ev.Set();
        await ev.WaitAsync(); // already set, should complete

        ev.Reset();
        var waitTask = ev.WaitAsync();
        var completed = await Task.WhenAny(waitTask, Task.Delay(50)) == waitTask;
        Assert.False(completed);

        ev.Set();
        await waitTask;
    }

    [Fact]
    public async Task Multiple_Waiters_Release_On_Set()
    {
        var ev = new AsyncManualResetEvent();
        var tasks = Enumerable.Range(0, 5).Select(_ => ev.WaitAsync()).ToArray();

        // none should complete yet (the delay must win)
        var delay = Task.Delay(50);
        var winner = await Task.WhenAny(Task.WhenAll(tasks), delay);
        Assert.Same(delay, winner);

        ev.Set();
        await Task.WhenAll(tasks);
    }
}
