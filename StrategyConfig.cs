using System;
using System.IO;
using System.Text.Json;

namespace PriceImpactTrader
{
    public class StrategyConfig
    {
        public string Instrument { get; set; } = "PUM.DE";
        public decimal InitialPrice { get; set; } = 22.75m;
        public int TargetVolume { get; set; } = 170000;
        public decimal CapitalLimit { get; set; } = 30000000m;

        public bool EnableTrapPhase { get; set; } = true;
        public int TrapSellVolume { get; set; } = 20000;
        public decimal TrapDropPercent { get; set; } = 0.5m; // Percentage of trapped price drop
        public decimal StopLossPercent { get; set; } = 3.0m;
        public decimal PriceImpactPerShare { get; set; } = 0.000011m;
        public static StrategyConfig Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<StrategyConfig>(json)
                   ?? throw new Exception("Failed to parse config.json");
        }

        public static StrategyConfig CreateDefault()
        {
            return new StrategyConfig();
        }
    }
}