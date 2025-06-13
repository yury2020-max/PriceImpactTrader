using System;
using System.Collections.Generic;
using System.Linq;

namespace PriceImpactTrader
{
    public class OrderBook
    {
        public List<(decimal Price, int Volume)> Bids { get; private set; }
        public List<(decimal Price, int Volume)> Asks { get; private set; }

        public OrderBook(decimal midPrice)
        {
            Bids = new List<(decimal, int)>();
            Asks = new List<(decimal, int)>();
            GenerateDepth(midPrice);
        }

        private void GenerateDepth(decimal mid)
        {
            // Generate 10 levels in each direction
            var random = new Random();
            
            for (int i = 1; i <= 10; i++)
            {
                // Bids (purchases) - prices are decreasing from the midpoint
                decimal bidPrice = mid - 0.01m * i;
                int bidVolume = 5000 + random.Next(0, 10000); // Random volume
                Bids.Add((bidPrice, bidVolume));

                // Asks (sales) - prices are increasing from the midpoint
                decimal askPrice = mid + 0.01m * i;
                int askVolume = 5000 + random.Next(0, 10000);
                Asks.Add((askPrice, askVolume));
            }

            // Sort: Bids by descending price, Asks by ascending
            Bids = Bids.OrderByDescending(x => x.Price).ToList();
            Asks = Asks.OrderBy(x => x.Price).ToList();
        }

        public decimal GetBestBid() => Bids.FirstOrDefault().Price;
        public decimal GetBestAsk() => Asks.FirstOrDefault().Price;
        public decimal GetSpread() => GetBestAsk() - GetBestBid();
        public decimal GetMidPrice() => (GetBestBid() + GetBestAsk()) / 2;

        // Execution of market buy order
        public (decimal avgPrice, int executedVolume) ExecuteMarketBuy(int targetVolume)
        {
            decimal totalCost = 0;
            int remainingVolume = targetVolume;
            int executedVolume = 0;
            
            for (int i = 0; i < Asks.Count && remainingVolume > 0; i++)
            {
                var ask = Asks[i];
                int volumeToTake = Math.Min(remainingVolume, ask.Volume);
                
                totalCost += ask.Price * volumeToTake;
                executedVolume += volumeToTake;
                remainingVolume -= volumeToTake;

                // Update the volume in the order book
                if (volumeToTake == ask.Volume)
                {
                    Asks.RemoveAt(i);
                    i--; // Compensate for the removed element
                }
                else
                {
                    Asks[i] = (ask.Price, ask.Volume - volumeToTake);
                }
            }
            
            decimal avgPrice = executedVolume > 0 ? totalCost / executedVolume : 0;
            return (avgPrice, executedVolume);
        }

        // Execution of market sell order
        public (decimal avgPrice, int executedVolume) ExecuteMarketSell(int targetVolume)
        {
            decimal totalReceived = 0;
            int remainingVolume = targetVolume;
            int executedVolume = 0;
            
            for (int i = 0; i < Bids.Count && remainingVolume > 0; i++)
            {
                var bid = Bids[i];
                int volumeToTake = Math.Min(remainingVolume, bid.Volume);
                
                totalReceived += bid.Price * volumeToTake;
                executedVolume += volumeToTake;
                remainingVolume -= volumeToTake;

                // Update the volume in the orderbook
                if (volumeToTake == bid.Volume)
                {
                    Bids.RemoveAt(i);
                    i--; // Compensate for the removed element
                }
                else
                {
                    Bids[i] = (bid.Price, bid.Volume - volumeToTake);
                }
            }
            
            decimal avgPrice = executedVolume > 0 ? totalReceived / executedVolume : 0;
            return (avgPrice, executedVolume);
        }

        // Adding a limit order
        public void AddLimitOrder(decimal price, int volume, bool isBuy)
        {
            if (isBuy)
            {
                Bids.Add((price, volume));
                Bids = Bids.OrderByDescending(x => x.Price).ToList();
            }
            else
            {
                Asks.Add((price, volume));
                Asks = Asks.OrderBy(x => x.Price).ToList();
            }
        }

        // Applying price impact (shifts the entire order book)
        public void ApplyPriceImpact(decimal priceShift)
        {
            // Shift all prices in the order book
            for (int i = 0; i < Bids.Count; i++)
            {
                Bids[i] = (Bids[i].Price + priceShift, Bids[i].Volume);
            }
            
            for (int i = 0; i < Asks.Count; i++)
            {
                Asks[i] = (Asks[i].Price + priceShift, Asks[i].Volume);
            }
        }

        public void Print()
        {
            Console.WriteLine("\n=== ORDER BOOK SNAPSHOT ===");
            Console.WriteLine($"Spread: {GetSpread():F4} EUR");
            Console.WriteLine($"Mid Price: {GetMidPrice():F2} EUR");
            Console.WriteLine();
            
            Console.WriteLine("ASKS (Sales):");
            for (int i = Math.Min(5, Asks.Count) - 1; i >= 0; i--)
            {
                var ask = Asks[i];
                Console.WriteLine($"  {ask.Price:F2} x {ask.Volume:N0}");
            }
            
            Console.WriteLine("  --------");
            Console.WriteLine($"  {GetMidPrice():F2} (Mid)");
            Console.WriteLine("  --------");
            
            Console.WriteLine("BIDS (Purchases):");
            for (int i = 0; i < Math.Min(5, Bids.Count); i++)
            {
                var bid = Bids[i];
                Console.WriteLine($"  {bid.Price:F2} x {bid.Volume:N0}");
            }
            Console.WriteLine();
        }

        // Calculation of order execution cost (without actual execution)
        public decimal CalculateMarketImpact(int volume, bool isBuy)
        {
            decimal totalCost = 0;
            int remainingVolume = volume;
            
            if (isBuy)
            {
                foreach (var ask in Asks)
                {
                    if (remainingVolume <= 0) break;
                    
                    int volumeToTake = Math.Min(remainingVolume, ask.Volume);
                    totalCost += ask.Price * volumeToTake;
                    remainingVolume -= volumeToTake;
                }
            }
            else
            {
                foreach (var bid in Bids)
                {
                    if (remainingVolume <= 0) break;
                    
                    int volumeToTake = Math.Min(remainingVolume, bid.Volume);
                    totalCost += bid.Price * volumeToTake;
                    remainingVolume -= volumeToTake;
                }
            }
            
            return volume > 0 ? totalCost / volume : 0;
        }
    }

    // Example of use in MarketSimulator
    public static class OrderBookExample
    {
        public static void TestOrderBook()
        {
            var orderBook = new OrderBook(22.75m);

            Console.WriteLine("Initial order book:");
            orderBook.Print();

            // Testing market orders
            var (avgPrice, executedVolume) = orderBook.ExecuteMarketBuy(15000);
            Console.WriteLine($"Средняя цена: {avgPrice:F4}");
            Console.WriteLine($"Исполнено: {executedVolume}");

            orderBook.Print();

            Console.WriteLine("Executing market sell for 8,000 shares:");
            var sellResult = orderBook.ExecuteMarketSell(8000);
            Console.WriteLine($"Average price: {sellResult.avgPrice:F4}, Executed: {sellResult.executedVolume}");

            orderBook.Print();

            // Testing price impact
            Console.WriteLine("Applying positive price impact +0.05:");
            orderBook.ApplyPriceImpact(0.05m);
            orderBook.Print();
        }
    }
}