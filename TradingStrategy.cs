
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

            Phase1_Accumulate();

            // Checking the stop loss after each phase
            if (_simulator.ShouldStopLoss())
            {
                _simulator.Log($"STOP-LOSS TRIGGERED after Phase 1 at {_simulator.CurrentPrice:F2}");
                EmergencyExit();
                return;
            }

            if (_config.EnableTrapPhase)
            {
                Phase2_Trap();
                
                if (_simulator.ShouldStopLoss())
                {
                    _simulator.Log($"STOP-LOSS TRIGGERED after Phase 2 at {_simulator.CurrentPrice:F2}");
                    EmergencyExit();
                    return;
                }
            }

            Phase3_ImpulseBuy();

            if (_simulator.ShouldStopLoss())
            {
                _simulator.Log($"STOP-LOSS TRIGGERED after Phase 3 at {_simulator.CurrentPrice:F2}");
                EmergencyExit();
                return;
            }

            Phase4_Exit();
        }

        private void Phase1_Accumulate()
        {
            _simulator.Log("--- Phase 1: Hidden Accumulation ---");
            for (int i = 0; i < 50; i++)
            {
                decimal price = RandomPrice(23.21m, 23.50m);
                _simulator.ExecutePassiveBuy(1000, price);
            }
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
            
            // Emergency liquidation of all positions
            // Approximate calculation of current positions (simplified)
            int estimatedPosition = 50000; // Base position from Phase 1

            if (estimatedPosition > 0)
            {
                _simulator.Log($"Emergency liquidation of {estimatedPosition:N0} shares");
                _simulator.ExecuteSell(estimatedPosition, out decimal emergencyPrice);
                _simulator.Log($"Emergency liquidation completed at {emergencyPrice:F2}");
            }
            
            _simulator.Log("Emergency exit completed. Algorithm terminated.");
        }

        private void Phase2_Trap()
        {
            _simulator.Log("--- Phase 2: Trap Flush Phase ---");

            // Use the price of the last passive purchase from Phase 1
            decimal basePrice = _simulator.LastPassiveBuyPrice;

            // Calculate the drop size based on the percentage
            decimal trapPriceDrop = basePrice * (_config.TrapDropPercent / 100);
            decimal triggerPrice = basePrice - trapPriceDrop;
            
            _simulator.Log($"Base price: {basePrice:F2}, Drop: {trapPriceDrop:F2} ({_config.TrapDropPercent}%)");
            _simulator.Log($"Triggering trap buy orders at {triggerPrice:F2}");

            // Sell enough volume to reach the trigger price
            decimal priceImpactPerShare = _config.PriceImpactPerShare;
            decimal totalImpactNeeded = basePrice - triggerPrice;
            int totalSharesToSell = (int)Math.Ceiling(totalImpactNeeded / priceImpactPerShare);
            int adjustedVolume = (totalSharesToSell / 1000) * 1000; // round down to the nearest 1000

            if (adjustedVolume < 1000) adjustedVolume = 1000;

            // Save the correct starting price for Phase 3
            _simulator.LastTrapSellStartPrice = basePrice; // Price at the end of Phase 1
            _simulator.CurrentPrice = basePrice; // Set the current price

            _simulator.ExecuteSell(adjustedVolume, out decimal actualSellPrice);
            _simulator.ExecuteBuyAtPrice(adjustedVolume, triggerPrice);
        }

        private void Phase3_ImpulseBuy()
        {
            _simulator.Log("--- Phase 3: Impulse Buy Phase ---");

            // Start from the price where Phase 2 began (reverting to the starting point)
            decimal reboundPrice = _simulator.LastTrapSellStartPrice;
            _simulator.CurrentPrice = reboundPrice;
            _simulator.Log($"Resuming buys from price: {reboundPrice:F2}");

            int lotSize = 10000;
            int lotsToBuy = 12; // будем покупать 12 лотов по 10000 акций = 120000

            for (int i = 0; i < lotsToBuy; i++)
            {
                // Calculate the impact of volume on price
                decimal priceImpact = _config.PriceImpactPerShare * lotSize;
                decimal newPrice = _simulator.CurrentPrice + priceImpact;
                
                _simulator.Log($"Lot {i + 1}/12: Price impact = {priceImpact:F4}, Target price = {newPrice:F2}");

                // Execute buy
                _simulator.ExecuteBuyAtPrice(lotSize, newPrice);

                // Update the current price for the next iteration
                _simulator.CurrentPrice = newPrice;
            }

            // After the 12th lot, stop orders are triggered
            _simulator.Log($"All 12 lots completed. Current price: {_simulator.CurrentPrice:F2}");
            _simulator.Log($"STOP ORDERS TRIGGERED: 50000 shares market buy activated!");

            // Calculate the impact of stop orders on price
            decimal stopImpact = _config.PriceImpactPerShare * 50000;
            decimal peakPrice = _simulator.CurrentPrice + stopImpact;

            // IMPORTANT: Stop orders are a sale to US, not our purchase!
            // Other participants buy 50000 shares from us at the peak price
            _simulator.ExecuteStopOrderSale(50000, peakPrice);
            _simulator.CurrentPrice = peakPrice; // Update the price after stop orders

            _simulator.Log($"PEAK PRICE REACHED: {peakPrice:F2}");
            _simulator.Log($"Total price movement: {reboundPrice:F2} -> {peakPrice:F2} (+{(peakPrice - reboundPrice):F2})");
            _simulator.Log($"Phase 3 completed. Ready for Phase 4 liquidation.");
        }

        private void Phase4_Exit()
        {
            _simulator.Log("--- Phase 4: Exit Phase ---");
            
            decimal startPrice = _simulator.CurrentPrice;
            _simulator.Log($"Starting liquidation from peak price: {startPrice:F2}");
            
            // Liquidation portfolio:
            // Phase 1: +50000 shares (our purchases)
            // Phase 3: +120000 shares (our purchases)
            // Stop orders: -50000 shares (other participants bought from us)
            // Total to liquidate: 50000 + 120000 - 50000 = 120000 shares
            int totalPortfolio = 120000;
            int sellStep = 20000; // Selling in blocks of 20000
            int remainingShares = totalPortfolio;
            
            _simulator.Log($"Our portfolio to liquidate: {totalPortfolio:N0} shares");
            _simulator.Log($"(Phase 1: +50000, Phase 3: +120000, Stop orders: -50000)");
            _simulator.Log($"Liquidation strategy: {sellStep:N0} shares per step");
            
            int stepNumber = 1;
            
            while (remainingShares > 0)
            {
                int sharesToSell = Math.Min(sellStep, remainingShares);
                
                // Calculate the impact of the sale on the price (negative)
                decimal priceImpact = _config.PriceImpactPerShare * sharesToSell;
                
                _simulator.Log($"Step {stepNumber}: Selling {sharesToSell:N0} shares, price impact: -{priceImpact:F4}");

                // Execute sell
                _simulator.ExecuteSell(sharesToSell, out decimal actualSellPrice);
                
                remainingShares -= sharesToSell;
                stepNumber++;
                
                _simulator.Log($"Sold at {actualSellPrice:F2}, remaining shares: {remainingShares:N0}");

                // Checking for stop loss
                if (_simulator.ShouldStopLoss())
                {
                    _simulator.Log($"STOP-LOSS TRIGGERED at {_simulator.CurrentPrice:F2}");
                    _simulator.Log($"Emergency liquidation of remaining {remainingShares:N0} shares");
                    
                    if (remainingShares > 0)
                    {
                        // Emergency liquidation of the remainder
                        _simulator.ExecuteSell(remainingShares, out decimal emergencyPrice);
                        _simulator.Log($"Emergency sell completed at {emergencyPrice:F2}");
                    }
                    break;
                }
            }
            
            decimal endPrice = _simulator.CurrentPrice;
            decimal totalPriceMovement = endPrice - startPrice;
            
            _simulator.Log($"Phase 4 completed.");
            _simulator.Log($"Price movement during liquidation: {startPrice:F2} -> {endPrice:F2} ({totalPriceMovement:F2})");
            _simulator.Log($"Portfolio fully liquidated.");
        }
    }
}
    
