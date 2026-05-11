using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartStorage.Infrastructure.Data;
using SmartStorage.Core.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartStorage.Services
{
    public class ContractExpiryService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ContractExpiryService> _logger;

        public ContractExpiryService(IServiceProvider services, ILogger<ContractExpiryService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckExpiringContracts(stoppingToken);

                // Check every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CheckExpiringContracts(CancellationToken stoppingToken)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var today = DateTime.Now;
            var sevenDaysFromNow = today.AddDays(7);
            var thirtyDaysFromNow = today.AddDays(30);

            // Find contracts expiring soon
            var expiringIn7Days = await context.Contracts
                .Include(c => c.Client)
                .Where(c => c.EndDate > today && c.EndDate <= sevenDaysFromNow && c.Status != ContractStatus.Expired)
                .ToListAsync(stoppingToken);

            var expiringIn30Days = await context.Contracts
                .Include(c => c.Client)
                .Where(c => c.EndDate > sevenDaysFromNow && c.EndDate <= thirtyDaysFromNow && c.Status != ContractStatus.Expired)
                .ToListAsync(stoppingToken);

            // Mark expired contracts
            var expiredContracts = await context.Contracts
                .Where(c => c.EndDate <= today && c.Status != ContractStatus.Expired)
                .ToListAsync(stoppingToken);

            foreach (var contract in expiredContracts)
            {
                contract.Status = ContractStatus.Expired;
                _logger.LogWarning($"Contract {contract.ContractNumber} has EXPIRED!");
            }

            await context.SaveChangesAsync(stoppingToken);

            // Log alerts (can be extended to send emails)
            foreach (var contract in expiringIn7Days)
            {
                _logger.LogCritical($"URGENT: Contract {contract.ContractNumber} for customer {contract.Client?.Email} expires in {(contract.EndDate - today).Days} days!");
            }

            foreach (var contract in expiringIn30Days)
            {
                _logger.LogWarning($"Contract {contract.ContractNumber} for customer {contract.Client?.Email} expires in {(contract.EndDate - today).Days} days");
            }
        }
    }
}