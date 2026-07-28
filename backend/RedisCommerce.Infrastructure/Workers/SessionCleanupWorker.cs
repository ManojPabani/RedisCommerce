using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedisCommerce.Application.Interfaces;

namespace RedisCommerce.Infrastructure.Workers;

/// <summary>
/// Defensive reconciliation for the sessions:active Set: sessions that expired via TTL without the
/// RedisExpirationListener catching the event (e.g. keyspace notifications weren't permitted on the
/// server) leave a stale member behind. This worker periodically removes them.
/// </summary>
public class SessionCleanupWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCleanupWorker> _logger;

    public SessionCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<SessionCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanupOrphanedSessionsAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Session cleanup tick failed; will retry on the next interval");
            }
        }
    }

    private async Task CleanupOrphanedSessionsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

        var activeSessionIds = await repository.GetActiveSessionIdsAsync();

        var removed = 0;
        foreach (var sessionId in activeSessionIds)
        {
            var session = await repository.GetAsync(sessionId);
            if (session is null)
            {
                await repository.RemoveFromActiveSetAsync(sessionId);
                removed++;
            }
        }

        if (removed > 0)
        {
            _logger.LogInformation(
                "Session cleanup: removed {RemovedCount} orphaned session(s) from the active set ({RemainingCount} still active)",
                removed,
                activeSessionIds.Count - removed);
        }
    }
}
