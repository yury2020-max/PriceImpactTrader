using System;
using System.Collections.Generic;
using System.IO;

namespace PriceImpactTrader
{
    public class MarketSimulator
    {
        private readonly StrategyConfig _config;
        public decimal CurrentPrice { get; set; }
        public decimal LastTrapSellStartPrice { get; set; }
        public decimal LastPassiveBuyPrice { get; set; }
        public decimal VWAP => _vwapVolume > 0 ? _vwapTotal / _vwapVolume : 0m;
        private readonly List<string> _log = new();
        private readonly List<string> _priceHistory = new();
        private int _timeStep = 0; // Add a time counter
        #pragma warning disable CS0414
        private bool _stopTriggered = false;
        #pragma warning restore CS0414

        private decimal _vwapTotal = 0;
        private int _vwapVolume = 0;
        
        // Calculation of PnL
        private decimal _totalBuyAmount = 0;
        private decimal _totalSellAmount = 0;
        private int _totalSharesBought = 0;
        private int _totalSharesSold = 0;

        // Separate for stop orders
        private decimal _stopOrderAmount = 0;
        private int _stopOrderShares = 0;

        public MarketSimulator(StrategyConfig config)
        {
            _config = config;
            CurrentPrice = config.InitialPrice;
            LastPassiveBuyPrice = config.InitialPrice;
            _priceHistory.Add("TimeStep,Price,Volume,Phase,Action");
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
            _log.Add(message);
        }

        public void ExecutePassiveBuy(int volume, decimal price)
        {
            decimal amount = volume * price;
            _totalBuyAmount += amount;
            _totalSharesBought += volume;
            _vwapTotal += amount;
            _vwapVolume += volume;
            LastPassiveBuyPrice = price;
            CurrentPrice = price;
            Log($"BUY {volume} @ {price:F2}");

            // Phase 1: Quiet accumulation - price changes slowly
            _timeStep++;
            _priceHistory.Add($"{_timeStep},{price:F2},{volume},Phase1,PassiveBuy");
        }

        public void ExecuteBuy(int volume)
        {
            decimal priceImpact = _config.PriceImpactPerShare * volume;
            CurrentPrice += priceImpact;
            decimal amount = volume * CurrentPrice;
            _totalBuyAmount += amount;
            _totalSharesBought += volume;
            _vwapTotal += amount;
            _vwapVolume += volume;
            Log($"BUY {volume} @ {CurrentPrice:F2}");
            _priceHistory.Add($"{_priceHistory.Count},{CurrentPrice:F2}");            
        }

        public void ExecuteSell(int volume, out decimal avgSellPrice)
        {
            // Phase 4: Gradual liquidation with price drop
            decimal totalPriceImpact = _config.PriceImpactPerShare * volume;
            decimal startPrice = CurrentPrice;
            decimal endPrice = CurrentPrice - totalPriceImpact;
            
            avgSellPrice = startPrice - (totalPriceImpact / 2);
            CurrentPrice = endPrice;

            // Record price drop
            _timeStep++;
            _priceHistory.Add($"{_timeStep},{avgSellPrice:F2},{-volume},Phase4,Exit");
            
            decimal amount = volume * avgSellPrice;
            _totalSellAmount += amount;
            _totalSharesSold += volume;
            
            Log($"SELL {volume} @ {avgSellPrice:F2}");
        }

        public void ExecuteBuyAtPrice(int volume, decimal targetPrice)
        {
            // For Phase 3: Show price increase from purchases
            decimal startPrice = CurrentPrice;
            decimal totalPriceImpact = _config.PriceImpactPerShare * volume;
            decimal endPrice = targetPrice;

            // Record price increase
            _timeStep++;
            _priceHistory.Add($"{_timeStep},{endPrice:F2},{volume},Phase3,ImpulseBuy");
            
            CurrentPrice = endPrice;
            
            decimal amount = volume * targetPrice;
            _totalBuyAmount += amount;
            _totalSharesBought += volume;
            _vwapTotal += amount;
            _vwapVolume += volume;
            
            Log($"BUY {volume} @ {targetPrice:F2} (impulse)");
        }

        // Special method for Phase 2 Trap - shows real price movement
        public void ExecuteTrapSell(int volume, decimal startPrice, decimal endPrice, out decimal avgSellPrice)
        {
            avgSellPrice = (startPrice + endPrice) / 2;

            // Record several points to show price drop
            int steps = 5;
            decimal priceStep = (startPrice - endPrice) / steps;
            
            for (int i = 0; i <= steps; i++)
            {
                decimal stepPrice = startPrice - (priceStep * i);
                _timeStep++;
                _priceHistory.Add($"{_timeStep},{stepPrice:F2},{volume/steps},Phase2,TrapSell");
            }
            
            CurrentPrice = endPrice;
            
            decimal amount = volume * avgSellPrice;
            _totalSellAmount += amount;
            _totalSharesSold += volume;
            
            Log($"SELL {volume} @ {avgSellPrice:F2}");
        }

        // Special method for Phase 2 Trap buyback - shows price recovery
        public void ExecuteTrapBuyback(int volume, decimal triggerPrice, decimal endPrice, out decimal avgBuyPrice)
        {
            avgBuyPrice = (triggerPrice + endPrice) / 2;

            // Record several points to show price recovery
            int steps = 3;
            decimal priceStep = (endPrice - triggerPrice) / steps;
            
            for (int i = 0; i <= steps; i++)
            {
                decimal stepPrice = triggerPrice + (priceStep * i);
                _timeStep++;
                _priceHistory.Add($"{_timeStep},{stepPrice:F2},{volume/steps},Phase2,TrapBuy");
            }
            
            CurrentPrice = endPrice;
            
            decimal amount = volume * avgBuyPrice;
            _totalBuyAmount += amount;
            _totalSharesBought += volume;
            _vwapTotal += amount;
            _vwapVolume += volume;
            
            Log($"BUY {volume} @ {triggerPrice:F2} (triggered)");
        }

        public void ExecuteStopOrderSale(int volume, decimal price)
        {
            // Separate for stop orders
            decimal amount = volume * price;
            _stopOrderAmount += amount;
            _stopOrderShares += volume;

            // Record peak price
            _timeStep++;
            _priceHistory.Add($"{_timeStep},{price:F2},{volume},Phase3,StopOrders");
            
            CurrentPrice = price;
            
            Log($"STOP ORDER SALE: {volume} @ {price:F2} (others bought from us)");
        }

        private void TriggerStopOrders()
        {
            _stopTriggered = true;
            int volume = 50000;
            decimal priceImpact = _config.PriceImpactPerShare * volume;
            CurrentPrice += priceImpact;
            Log($"STOP ORDERS TRIGGERED: BUY {volume} @ {CurrentPrice:F2}");
            _priceHistory.Add($"{_priceHistory.Count},{CurrentPrice:F2}");
        }

        public bool ShouldStopLoss()
        {
            decimal threshold = _config.InitialPrice * (1 - _config.StopLossPercent / 100);
            return CurrentPrice <= threshold;
        }

        public void GenerateReport()
        {
            decimal totalRevenue = _totalSellAmount + _stopOrderAmount; // Include stop orders in revenue
            decimal totalCosts = _totalBuyAmount;
            decimal netPnL = totalRevenue - totalCosts;

            int netPosition = _totalSharesBought - _totalSharesSold - _stopOrderShares; // Subtract stop orders
            decimal avgBuyPrice = _totalSharesBought > 0 ? _totalBuyAmount / _totalSharesBought : 0;
            decimal avgSellPrice = (_totalSharesSold + _stopOrderShares) > 0 ? 
                                   (totalRevenue) / (_totalSharesSold + _stopOrderShares) : 0;
            
            Log($"=== TRADING SUMMARY ===");
            Log($"Total Shares Bought: {_totalSharesBought:N0}");
            Log($"Total Shares Sold (regular): {_totalSharesSold:N0}");
            Log($"Stop Order Sales: {_stopOrderShares:N0}");
            Log($"Total Shares Sold (all): {(_totalSharesSold + _stopOrderShares):N0}");
            Log($"Net Position: {netPosition:N0} shares");
            Log($"Average Buy Price: {avgBuyPrice:F4}");
            Log($"Average Sell Price: {avgSellPrice:F4}");
            Log($"Total Money Spent: {totalCosts:F2} EUR");
            Log($"Total Money Received: {totalRevenue:F2} EUR");
            Log($"  - Regular Sales: {_totalSellAmount:F2} EUR");
            Log($"  - Stop Order Sales: {_stopOrderAmount:F2} EUR");
            Log($"Net P&L: {netPnL:F2} EUR");
            Log($"Final VWAP: {VWAP:F2}");
            
            if (netPosition > 0)
            {
                decimal unrealizedValue = netPosition * CurrentPrice;
                decimal totalPnL = netPnL + unrealizedValue - (netPosition * avgBuyPrice);
                Log($"Unrealized Position Value: {unrealizedValue:F2} EUR");
                Log($"Total P&L (including unrealized): {totalPnL:F2} EUR");
            }
            else if (netPosition == 0)
            {
                Log($"Position fully liquidated - No unrealized P&L");
            }
            else
            {
                Log($"Negative position detected - Check calculations!");
            }
            
            File.WriteAllLines("simulation_log.txt", _log);
            File.WriteAllLines("price_history.csv", _priceHistory);
        }
    }
}