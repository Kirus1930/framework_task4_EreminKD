using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookingService.Health;

public class ReadinessHealthCheck : IHealthCheck
{
    private static int _failures = 0;

    public static void RegisterFailure()
    {
        _failures++;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_failures > 5)
            return Task.FromResult(HealthCheckResult.Unhealthy("Too many failures"));

        return Task.FromResult(HealthCheckResult.Healthy());
    }
}