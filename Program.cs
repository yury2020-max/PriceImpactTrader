using System;
using System.IO;

namespace PriceImpactTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Price Impact Trading Algorithm ===");
            Console.WriteLine();

            try
            {
                // Creating a configuration
                StrategyConfig config;
                
                if (args.Length > 0 && File.Exists(args[0]))
                {
                    Console.WriteLine($"Loading configuration from: {args[0]}");
                    config = StrategyConfig.Load(args[0]);
                }
                else
                {
                    Console.WriteLine("Using default configuration");
                    config = StrategyConfig.CreateDefault();
                }
                
                // Creating a simulation
                var simulator = new MarketSimulator(config);
                
                // Creating a strategy
                var strategy = new TradingStrategy(config, simulator);
                
                // Starting the algorithm
                strategy.Execute();
                
                // Generating a report
                simulator.GenerateReport();
                
                Console.WriteLine();
                Console.WriteLine("Algorithm completed successfully!");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}