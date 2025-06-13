// OrderBook.cs
using System;
using System.Collections.Generic;

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
            for (int i = 0; i < 5; i++)
            {
                Bids.Add((mid - 0.01m * (i + 1), 10000 + i * 500));
                Asks.Add((mid + 0.01m * (i + 1), 10000 + i * 500));
            }
        }

        public void Print()
        {
            Console.WriteLine("Order Book Snapshot:");
            for (int i = 0; i < Bids.Count; i++)
            {
                Console.WriteLine($"Bid {i + 1}: {Bids[i].Price:F2} x {Bids[i].Volume}");
            }
            for (int i = 0; i < Asks.Count; i++)
            {
                Console.WriteLine($"Ask {i + 1}: {Asks[i].Price:F2} x {Asks[i].Volume}");
            }
        }
    }
}