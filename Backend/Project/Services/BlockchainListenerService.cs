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

    // скільки разів підряд опитування ноди може провалитись, перш ніж
    // ми почнемо чекати довше (backoff), а не молотити раз на 10с у нікуди
    private int _consecutiveErrors = 0;

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
            var startBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

            // ВИПРАВЛЕННЯ гонки: починаємо на один блок РАНІШЕ поточного,
            // а не рівно на поточному. Інакше, якщо покупка стається в тому
            // самому блоці, де ми щойно ініціалізувались (типово для
            // instant-mining Hardhat-ноди), lastProcessedBlock == latestBlock
            // на першій ітерації і подія з цього блоку НІКОЛИ не буде
            // прочитана, бо цикл чекає на latestBlock.Value > lastProcessedBlock.Value.
            var safeStart = startBlock.Value > 0 ? startBlock.Value - 1 : startBlock.Value;
            lastProcessedBlock = new HexBigInteger(safeStart);
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
                    // навіть тут краще відступити на 1 блок назад, а не
                    // "проковтнути" поточний блок мовчки
                    var safe = latestBlock.Value > 0 ? latestBlock.Value - 1 : latestBlock.Value;
                    lastProcessedBlock = new HexBigInteger(safe);
                }

                if (latestBlock.Value > lastProcessedBlock.Value)
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
                        _logger.LogInformation(
                            "Отримано івент! UserId (string): {UserId}, Plan: {Plan}, Buyer: {Buyer}",
                            log.UserId, log.PlanType, log.Buyer);

                        await ProcessSubscriptionAsync(log, stoppingToken);
                    }

                    lastProcessedBlock = latestBlock;
                }

                _consecutiveErrors = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _consecutiveErrors++;
                _logger.LogError(ex,
                    "Помилка при зчитуванні івентів через HTTP (спроба {Attempt} поспіль).",
                    _consecutiveErrors);
            }

            // Backoff: якщо нода недоступна кілька разів підряд, не молотимо
            // запити щосекунди — чекаємо довше, максимум 60с.
            var delayMs = _consecutiveErrors == 0
                ? 10000
                : Math.Min(10000 * (_consecutiveErrors + 1), 60000);

            try
            {
                await Task.Delay(delayMs, stoppingToken);
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