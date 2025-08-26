namespace ChildGuard.Core.Sinks;

using System.Text.Json;
using System.Text.Json.Serialization;
using ChildGuard.Core.Abstractions;
using ChildGuard.Core.Models;

public sealed class JsonlFileEventSink : IEventSink
{
    private readonly StreamWriter _writer;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public JsonlFileEventSink(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        _writer = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
        {
            AutoFlush = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task WriteAsync(ActivityEvent evt, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(evt, _jsonOptions);
        await _writer.WriteLineAsync(json);
    }

    public ValueTask DisposeAsync()
    {
        _writer.Dispose();
        return ValueTask.CompletedTask;
    }
}
