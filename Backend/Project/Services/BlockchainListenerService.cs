using Application.DTOs.Subscription;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;

namespace WebApp.Services;

public class BlockchainListenerService : BackgroundService
{
    private readonly ILogger<BlockchainListenerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Web3 _web3;
    private readonly string _contractAddress;

    public BlockchainListenerService(
        ILogger<BlockchainListenerService> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var rpcUrl = configuration["Ethereum:Url"] ?? throw new ArgumentNullException("Ethereum:Url");
        _contractAddress = configuration["Ethereum:ContractAddress"] ?? throw new ArgumentNullException("Ethereum:ContractAddress");

        _web3 = new Web3(rpcUrl);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Запуск фонового слухача блокчейн-подій (HTTP Polling)...");

        var eventHandler = _web3.Eth.GetEvent<SubscriptionPurchasedEventDTO>(_contractAddress);

        HexBigInteger? lastProcessedBlock = null;

        try
        {
            lastProcessedBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не вдалося отримати початковий номер блоку.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var latestBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                if (lastProcessedBlock == null)
                {
                    lastProcessedBlock = latestBlock;
                }
                else if (latestBlock.Value > lastProcessedBlock.Value)
                {
                    var nextBlockHex = new HexBigInteger(lastProcessedBlock.Value + 1);

                    var filterInput = eventHandler.CreateFilterInput(
                        fromBlock: new BlockParameter(nextBlockHex),
                        toBlock: new BlockParameter(latestBlock)
                    );

                    var changes = await eventHandler.GetAllChangesAsync(filterInput);

                    foreach (var change in changes)
                    {
                        var log = change.Event;
                        _logger.LogInformation("Отримано івент! UserId (string): {UserId}, Plan: {Plan}", log.UserId, log.PlanType);

                        await ProcessSubscriptionAsync(log, stoppingToken);
                    }

                    lastProcessedBlock = latestBlock;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при зчитуванні івентів через HTTP.");
            }

            try
            {
                await Task.Delay(10000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessSubscriptionAsync(SubscriptionPurchasedEventDTO log, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<SonaraDbContext>();
        var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

        if (!Guid.TryParse(log.UserId, out var userId))
        {
            _logger.LogWarning("Не вдалося розпарсити UserId рядок у Guid: {UserId}", log.UserId);
            return;
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user != null)
        {
            await subscriptionService.ProcessBlockchainPurchaseAsync(user.Id, log.PlanType, ct);
            _logger.LogInformation("Користувача {UserId} успішно оновлено в БД!", userId);
        }
        else
        {
            _logger.LogWarning("Користувача з ID {UserId} не знайдено в БД.", userId);
        }
    }
}