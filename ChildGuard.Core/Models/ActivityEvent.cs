namespace ChildGuard.Core.Models;

public enum ActivityEventType
{
    Keyboard,
    Mouse,
    ActiveWindow,
    ProcessStart,
    ProcessStop,
    UsbDeviceChange,
    SessionSwitch,
}

public record ActivityEvent(
    DateTimeOffset Timestamp,
    ActivityEventType Type,
    object? Data
);

public record ActiveWindowInfo(string Title, string ProcessName, int ProcessId);
public record InputActivitySummary(long KeyPressCount, long MouseEventCount);
