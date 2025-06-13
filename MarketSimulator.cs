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
        public decimal LastPassiveBuyPrice { get; set; } // Tracking your last passive purchase
        public decimal VWAP => _vwapVolume > 0 ? _vwapTotal / _vwapVolume : 0m;
        private readonly List<string> _log = new();
        private readonly List<string> _priceHistory = new();
        private bool _stopTriggered = false;

        private decimal _vwapTotal = 0;
        private int _vwapVolume = 0;
        
        // Новая система учета для правильного расчета PnL
        private decimal _totalBuyAmount = 0;
        private decimal _totalSellAmount = 0;
        private int _totalSharesBought = 0;
        private int _totalSharesSold = 0;

        public MarketSimulator(StrategyConfig config)
        {
            _config = config;
            CurrentPrice = config.InitialPrice;
            LastPassiveBuyPrice = config.InitialPrice; // Initialize
            _priceHistory.Add("TimeStep,Price");
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
            _priceHistory.Add($"{_priceHistory.Count},{price:F2}");
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
            decimal priceImpact = _config.PriceImpactPerShare * volume;
            CurrentPrice -= priceImpact;
            avgSellPrice = CurrentPrice;
            decimal amount = volume * CurrentPrice;
            _totalSellAmount += amount;
            _totalSharesSold += volume;
            Log($"SELL {volume} @ {CurrentPrice:F2}");
            _priceHistory.Add($"{_priceHistory.Count},{CurrentPrice:F2}");
        }

        public void ExecuteBuyAtPrice(int volume, decimal price)
        {
            CurrentPrice = price;
            decimal amount = volume * price;
            _totalBuyAmount += amount;
            _totalSharesBought += volume;
            _vwapTotal += amount;
            _vwapVolume += volume;
            Log($"BUY {volume} @ {price:F2} (triggered)");
            _priceHistory.Add($"{_priceHistory.Count},{price:F2}");
        }

        public void ExecuteStopOrderSale(int volume, decimal price)
        {
            decimal amount = volume * price;
            _totalSellAmount += amount;
            _totalSharesSold += volume;
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
            // Calculation of PnL
            decimal totalRevenue = _totalSellAmount;
            decimal totalCosts = _totalBuyAmount;
            decimal netPnL = totalRevenue - totalCosts;
            
            int netPosition = _totalSharesBought - _totalSharesSold;
            decimal avgBuyPrice = _totalSharesBought > 0 ? _totalBuyAmount / _totalSharesBought : 0;
            decimal avgSellPrice = _totalSharesSold > 0 ? _totalSellAmount / _totalSharesSold : 0;
            
            Log($"=== TRADING SUMMARY ===");
            Log($"Total Shares Bought: {_totalSharesBought:N0}");
            Log($"Total Shares Sold: {_totalSharesSold:N0}");
            Log($"Net Position: {netPosition:N0} shares");
            Log($"Average Buy Price: {avgBuyPrice:F4}");
            Log($"Average Sell Price: {avgSellPrice:F4}");
            Log($"Total Money Spent: {totalCosts:F2} EUR");
            Log($"Total Money Received: {totalRevenue:F2} EUR");
            Log($"Net P&L: {netPnL:F2} EUR");
            Log($"Final VWAP: {VWAP:F2}");
            
            // If there are any shares left, we count them at the current price
            if (netPosition > 0)
            {
                decimal unrealizedValue = netPosition * CurrentPrice;
                decimal totalPnL = netPnL + unrealizedValue - (netPosition * avgBuyPrice);
                Log($"Unrealized Position Value: {unrealizedValue:F2} EUR");
                Log($"Total P&L (including unrealized): {totalPnL:F2} EUR");
            }
            
            File.WriteAllLines("simulation_log.txt", _log);
            File.WriteAllLines("price_history.csv", _priceHistory);
        }
    }
}