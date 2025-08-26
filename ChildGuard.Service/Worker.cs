namespace ChildGuard.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ChildGuardService started at: {time}", DateTimeOffset.Now);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Health heartbeat; later this will orchestrate user-session agent and sinks
                _logger.LogDebug("Heartbeat {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // expected on stop
        }
        finally
        {
            _logger.LogInformation("ChildGuardService stopping at: {time}", DateTimeOffset.Now);
        }
    }
}
