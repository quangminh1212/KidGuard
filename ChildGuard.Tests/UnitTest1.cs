using System.Text.Json;
using ChildGuard.Core.Models;
using ChildGuard.Core.Sinks;

namespace ChildGuard.Tests;

public class CoreModelsTests
{
    [Fact]
    public async Task JsonlSink_Writes_Line()
    {
        var tmp = Path.Combine(Path.GetTempPath(), "childguard_test", Guid.NewGuid() + ".log");
        var sink = new JsonlFileEventSink(tmp);
        var evt = new ActivityEvent(DateTimeOffset.UnixEpoch, ActivityEventType.ActiveWindow, new ActiveWindowInfo("Title", "proc", 123));
        await sink.WriteAsync(evt);
        await sink.DisposeAsync();

        Assert.True(File.Exists(tmp));
        var content = await File.ReadAllTextAsync(tmp);
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        Assert.Equal("ActiveWindow", root.GetProperty("type").GetString());
        Assert.Equal("Title", root.GetProperty("data").GetProperty("title").GetString());
    }
}
