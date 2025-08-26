namespace ChildGuard.Core.Abstractions;

using ChildGuard.Core.Models;

public interface IEventSink : IAsyncDisposable
{
    Task WriteAsync(ActivityEvent evt, CancellationToken ct = default);
}
