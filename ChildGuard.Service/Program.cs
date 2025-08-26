using ChildGuard.Service;

var builder = Host.CreateApplicationBuilder(args);

// Run as Windows Service with a clear service name
builder.Services.AddWindowsService(options => options.ServiceName = "ChildGuardService");

// Log to Windows Event Log when available
builder.Services.AddLogging(logging =>
{
    logging.AddEventLog();
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
