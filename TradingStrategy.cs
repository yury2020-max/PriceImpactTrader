using System;

namespace PriceImpactTrader
{
    public class TradingStrategy
    {
        private readonly StrategyConfig _config;
        private readonly MarketSimulator _simulator;
        private readonly Random _random = new();

        public TradingStrategy(StrategyConfig config, MarketSimulator simulator)
        {
            _config = config;
            _simulator = simulator;
        }

        public void Execute()
        {
            _simulator.Log($"=== STARTING ALGORITHM FOR {_config.Instrument} ===");
            _simulator.Log($"Initial Price: {_config.InitialPrice:F2} EUR");
            _simulator.Log($"Stop-Loss Level: {(_config.InitialPrice * (1 - _config.StopLossPercent / 100)):F2} EUR ({_config.StopLossPercent}%)");
            _simulator.Log("");

            Phase1_Accumulate(); // 40% от целевого объема

            // Checking the stop loss after each phase
            if (_simulator.ShouldStopLoss())
            {
                _simulator.Log($"STOP-LOSS TRIGGERED after Phase 1 at {_simulator.CurrentPrice:F2}");
                EmergencyExit();
                return;
            }

            if (_config.EnableTrapPhase)
            {
                Phase2_Trap(); // Продаем ~50% от Фазы 1, затем быстро выкупаем

                if (_simulator.ShouldStopLoss())
                {
                    _simulator.Log($"STOP-LOSS TRIGGERED after Phase 2 at {_simulator.CurrentPrice:F2}");
                    EmergencyExit();
                    return;
                }
            }

            Phase3_ImpulseBuy(); // 60% от целевого объема

            if (_simulator.ShouldStopLoss())
            {
                _simulator.Log($"STOP-LOSS TRIGGERED after Phase 3 at {_simulator.CurrentPrice:F2}");
                EmergencyExit();
                return;
            }

            Phase4_Exit(); // Продаем 100% позиции
        }

        private void Phase1_Accumulate()
        {
            _simulator.Log("--- Phase 1: Accumulation (40% target volume) ---");
            _simulator.Log("Strategy: Build base position quietly");

            // 40% от целевого объема из конфига
            int phase1Volume = (int)(_config.TargetVolume * 0.40m);
            int ordersCount = phase1Volume / 1000; // По 1000 акций за ордер

            _simulator.Log($"Target volume for Phase 1: {phase1Volume:N0} shares ({ordersCount} orders)");

            for (int i = 0; i < ordersCount; i++)
            {
                decimal price = RandomPrice(23.21m, 23.50m);
                _simulator.ExecutePassiveBuy(1000, price);
            }

            _simulator.Log($"Phase 1 completed: Accumulated {phase1Volume:N0} shares");
        }

        private decimal RandomPrice(decimal min, decimal max)
        {
            double range = (double)(max - min);
            return min + (decimal)(_random.NextDouble() * range);
        }

        private void EmergencyExit()
        {
            _simulator.Log("--- EMERGENCY EXIT: Stop-Loss Triggered ---");

            decimal currentPrice = _simulator.CurrentPrice;
            decimal stopLossLevel = _config.InitialPrice * (1 - _config.StopLossPercent / 100);

            _simulator.Log($"Current Price: {currentPrice:F2} EUR");
            _simulator.Log($"Stop-Loss Level: {stopLossLevel:F2} EUR");
            _simulator.Log($"Loss: {((currentPrice - _config.InitialPrice) / _config.InitialPrice * 100):F2}%");

            // Emergency liquidation - базируется на Phase 1 volume
            int phase1Volume = (int)(_config.TargetVolume * 0.40m);

            if (phase1Volume > 0)
            {
                _simulator.Log($"Emergency liquidation of {phase1Volume:N0} shares");
                _simulator.ExecuteSell(phase1Volume, out decimal emergencyPrice);
                _simulator.Log($"Emergency liquidation completed at {emergencyPrice:F2}");
            }

            _simulator.Log("Emergency exit completed. Algorithm terminated.");
        }

        private void Phase2_Trap()
        {
            _simulator.Log("--- Phase 2: Trap Flush Phase ---");
            _simulator.Log("Strategy: Sell ~50% of Phase 1 position, then quickly buy back");

            decimal basePrice = _simulator.LastPassiveBuyPrice;

            int phase1Volume = (int)(_config.TargetVolume * 0.40m);
            int sharesToSellInTrap = phase1Volume / 2;

            _simulator.Log($"Phase 1 accumulated: {phase1Volume:N0} shares");
            _simulator.Log($"Selling {sharesToSellInTrap:N0} shares to create price drop");

            decimal trapPriceDrop = basePrice * (_config.TrapDropPercent / 100);
            decimal triggerPrice = basePrice - trapPriceDrop;

            _simulator.Log($"Base price: {basePrice:F2}, Drop: {trapPriceDrop:F2} ({_config.TrapDropPercent}%)");
            _simulator.Log($"Target trigger price: {triggerPrice:F2}");

            _simulator.LastTrapSellStartPrice = basePrice;

            // Используем специальные методы для правильного расчета средних цен
            _simulator.ExecuteTrapSell(sharesToSellInTrap, basePrice, triggerPrice, out decimal avgSellPrice);
            _simulator.Log($"Sold {sharesToSellInTrap:N0} shares at {avgSellPrice:F2}");

            _simulator.ExecuteTrapBuyback(sharesToSellInTrap, triggerPrice, basePrice, out decimal avgBuyPrice);
            _simulator.Log($"Bought back {sharesToSellInTrap:N0} shares at {triggerPrice:F2} (triggered)");
            _simulator.Log("Trap phase completed - position restored!");
        }

        private void Phase3_ImpulseBuy()
        {
            _simulator.Log("--- Phase 3: Impulse Buy Phase (60% target volume) ---");
            _simulator.Log("Strategy: Aggressive accumulation to trigger stop orders");

            decimal reboundPrice = _simulator.LastTrapSellStartPrice;
            _simulator.CurrentPrice = reboundPrice;
            _simulator.Log($"Starting impulse from price: {reboundPrice:F2}");

            // 60% от целевого объема из конфига
            int phase3Volume = (int)(_config.TargetVolume * 0.60m);
            int lotSize = 10000;
            int lotsToBuy = phase3Volume / lotSize; // Рассчитываем количество лотов

            _simulator.Log($"Phase 3 target: {phase3Volume:N0} shares");
            _simulator.Log($"Buying {lotsToBuy} lots of {lotSize:N0} shares each");

            for (int i = 0; i < lotsToBuy; i++)
            {
                decimal priceImpact = _config.PriceImpactPerShare * lotSize;
                decimal newPrice = _simulator.CurrentPrice + priceImpact;

                _simulator.Log($"Lot {i + 1}/{lotsToBuy}: Price impact = {priceImpact:F4}, Target price = {newPrice:F2}");

                // Используем ExecuteBuyAtPrice для Phase 3 (это создаст рост)
                _simulator.ExecuteBuyAtPrice(lotSize, newPrice);
                _simulator.CurrentPrice = newPrice;
            }

            _simulator.Log($"All {lotsToBuy} lots completed. Current price: {_simulator.CurrentPrice:F2}");
            _simulator.Log($"STOP ORDERS TRIGGERED: 50000 shares market buy activated!");

            decimal stopImpact = _config.PriceImpactPerShare * 50000;
            decimal peakPrice = _simulator.CurrentPrice + stopImpact;

            _simulator.ExecuteStopOrderSale(50000, peakPrice);
            _simulator.CurrentPrice = peakPrice;

            _simulator.Log($"PEAK PRICE REACHED: {peakPrice:F2}");
            _simulator.Log($"Phase 3 completed. Bought {phase3Volume:N0} shares total.");
        }

        private void Phase4_Exit()
        {
            _simulator.Log("--- Phase 4: Complete Exit (100% position) ---");
            _simulator.Log("Strategy: Liquidate entire position");

            decimal startPrice = _simulator.CurrentPrice;
            _simulator.Log($"Starting complete liquidation from peak price: {startPrice:F2}");

            // calculation of actually purchased volume
            int phase1Volume = (int)(_config.TargetVolume * 0.40m);  // Phase 1: +40%
            int phase3Volume = (int)(_config.TargetVolume * 0.60m);
            int lotSize = 10000;
            int lotsBought = phase3Volume / lotSize;
            int actualPhase3Volume = lotsBought * lotSize;

            int stopOrderVolume = 50000;  // Stop orders: -50k
            int totalPortfolio = phase1Volume + actualPhase3Volume - stopOrderVolume;

            _simulator.Log($"=== PORTFOLIO CALCULATION DEBUG ===");
            _simulator.Log($"TargetVolume: {_config.TargetVolume:N0}");
            _simulator.Log($"Phase 1 (40%): +{phase1Volume:N0} shares");
            _simulator.Log($"Phase 2: +0 shares (neutral - sold and bought back)");
            _simulator.Log($"Phase 3 (60% target): requested +{phase3Volume:N0} shares, actually bought: {actualPhase3Volume:N0}");
            _simulator.Log($"Stop orders: -{stopOrderVolume:N0} shares");
            _simulator.Log($"Calculated portfolio: {phase1Volume:N0} + {actualPhase3Volume:N0} - {stopOrderVolume:N0} = {totalPortfolio:N0}");
            _simulator.Log($"=====================================");

            int sellStep = Math.Max(10000, totalPortfolio / 10);
            int remainingShares = totalPortfolio;
            int totalSoldInPhase4 = 0;

            _simulator.Log($"Liquidation strategy: ~{sellStep:N0} shares per step");

            int stepNumber = 1;

            while (remainingShares > 0)
            {
                int sharesToSell = Math.Min(sellStep, remainingShares);

                decimal priceImpact = _config.PriceImpactPerShare * sharesToSell;

                _simulator.Log($"Step {stepNumber}: Selling {sharesToSell:N0} shares, price impact: -{priceImpact:F4}");

                _simulator.ExecuteSell(sharesToSell, out decimal actualSellPrice);

                remainingShares -= sharesToSell;
                totalSoldInPhase4 += sharesToSell;
                stepNumber++;

                _simulator.Log($"Sold at {actualSellPrice:F2}, remaining shares: {remainingShares:N0}");

                if (_simulator.ShouldStopLoss())
                {
                    _simulator.Log($"STOP-LOSS TRIGGERED at {_simulator.CurrentPrice:F2}");
                    _simulator.Log($"Emergency liquidation of remaining {remainingShares:N0} shares");

                    if (remainingShares > 0)
                    {
                        _simulator.ExecuteSell(remainingShares, out decimal emergencyPrice);
                        totalSoldInPhase4 += remainingShares;
                        _simulator.Log($"Emergency sell completed at {emergencyPrice:F2}");
                        remainingShares = 0;
                    }
                    break;
                }
            }

            decimal endPrice = _simulator.CurrentPrice;
            decimal totalPriceMovement = endPrice - startPrice;

            _simulator.Log($"Phase 4 completed - POSITION FULLY LIQUIDATED!");
            _simulator.Log($"Expected to sell: {totalPortfolio:N0} shares");
            _simulator.Log($"Actually sold: {totalSoldInPhase4:N0} shares");
            _simulator.Log($"Difference: {totalSoldInPhase4 - totalPortfolio:N0} shares");
            _simulator.Log($"Price movement during liquidation: {startPrice:F2} -> {endPrice:F2} ({totalPriceMovement:F2})");
            _simulator.Log($"Final position: 0 shares");
        }
    }
}    
        