using Genocs.Secrets.HashicorpKeyVault.Configurations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VaultSharp;

namespace Genocs.Secrets.HashicorpKeyVault.Internals;

internal sealed class VaultHostedService : BackgroundService
{
    private readonly IVaultClient _client;
    private readonly ILeaseService _leaseService;
    private readonly ICertificatesIssuer _certificatesIssuer;
    private readonly ICertificatesService _certificatesService;
    private readonly VaultOptions _settings;
    private readonly ILogger<VaultHostedService> _logger;
    private readonly int _interval;

    public VaultHostedService(
                                IVaultClient client,
                                ILeaseService leaseService,
                                ICertificatesIssuer certificatesIssuer,
                                ICertificatesService certificatesService,
                                VaultOptions settings,
                                ILogger<VaultHostedService> logger)
    {
        _client = client;
        _leaseService = leaseService;
        _certificatesIssuer = certificatesIssuer;
        _certificatesService = certificatesService;
        _settings = settings;
        _logger = logger;
        _interval = _settings.RenewalsInterval <= 0 ? 10 : _settings.RenewalsInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            return;
        }

        if ((_settings.Pki is null || !_settings.Pki.Enabled) &&
            (_settings.Lease is null || _settings.Lease.All(l => !l.Value.Enabled) ||
             !_settings.Lease.Any(l => l.Value.AutoRenewal)))
        {
            return;
        }

        _logger.LogInformation($"Vault lease renewals will be processed every {_interval} s.");
        var interval = TimeSpan.FromSeconds(_interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextIterationAt = now.AddSeconds(2 * _interval);

            if (_settings.Pki is not null && _settings.Pki.Enabled)
            {
                foreach (var (role, cert) in _certificatesService.All)
                {
                    if (cert.NotAfter > nextIterationAt)
                    {
                        continue;
                    }

                    _logger.LogInformation($"Issuing a certificate for: '{role}'.");
                    var certificate = await _certificatesIssuer.IssueAsync();
                    _certificatesService.Set(role, certificate);
                }
            }

            foreach (var (key, lease) in _leaseService.All.Where(l => l.Value.AutoRenewal))
            {
                if (lease.ExpiryAt > nextIterationAt)
                {
                    continue;
                }

                _logger.LogInformation($"Renewing a lease with ID: '{lease.Id}', for: '{key}', " +
                                       $"duration: {lease.Duration} s.");

                var beforeRenew = DateTime.UtcNow;
                var renewedLease = await _client.V1.System.RenewLeaseAsync(lease.Id, lease.Duration);
                lease.Refresh(renewedLease.LeaseDurationSeconds - (lease.ExpiryAt - beforeRenew).TotalSeconds);
            }

            await Task.Delay(interval.Subtract(DateTime.UtcNow - now), stoppingToken);
        }

        if (!_settings.RevokeLeaseOnShutdown)
        {
            return;
        }

        foreach (var (key, lease) in _leaseService.All)
        {
            _logger.LogInformation($"Revoking a lease with ID: '{lease.Id}', for: '{key}'.");
            await _client.V1.System.RevokeLeaseAsync(lease.Id);
        }
    }
}