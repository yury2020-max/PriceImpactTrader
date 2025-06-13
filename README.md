# Price Impact Trading Algorithm

A trading algorithm simulator that models an accumulation strategy with subsequent price manipulation to trigger stop orders from other market participants.

## Strategy Overview

The algorithm implements a 4-phase trading strategy:

### Phase 1: Hidden Accumulation
- Passive purchases in small volumes (1000 shares)
- Random prices within a range to mask activity
- Goal: accumulate base position without market impact

### Phase 2: Trap Flush
- Sharp sell-off to drop price by specified percentage
- Immediate buyback at lower price
- Goal: create false impression of weakness and return to original level

### Phase 3: Impulse Buy
- Aggressive purchases in large lots (12 × 10000 shares)
- Sequential price increase with each purchase
- Activation of stop orders from other participants at peak price

### Phase 4: Liquidation (Exit)
- Gradual sale of accumulated position
- Block sales to minimize price impact
- Stop-loss protection against excessive losses

## Project Architecture

```
PriceImpactTrader/
├── Program.cs              # Application entry point
├── StrategyConfig.cs       # Strategy parameters configuration
├── MarketSimulator.cs      # Market operations simulator
├── TradingStrategy.cs      # Trading logic implementation
└── README.md               # Project documentation
```

## Key Parameters

| Parameter | Default Value | Description |
|-----------|---------------|-------------|
| `InitialPrice` | 22.75 EUR | Initial instrument price |
| `PriceImpactPerShare` | 0.000011 | Impact of one share on price |
| `TrapDropPercent` | 0.5% | Price drop percentage in trap phase |
| `StopLossPercent` | 3.0% | Stop-loss level from initial price |
| `EnableTrapPhase` | true | Enable/disable trap phase |
| `TargetVolume` | 170000 | Target volume for strategy |

## Installation and Usage

### Requirements
- .NET 6.0 or higher
- Operating System: Windows, macOS, Linux

### Running
```bash
# Clone repository
git clone [repository-url]
cd PriceImpactTrader

# Build project
dotnet build

# Run simulation with default settings
dotnet run

# Run with configuration file
dotnet run config.json
```

## Configuration Example

```json
{
  "Instrument": "PUM.DE",
  "InitialPrice": 22.75,
  "TargetVolume": 170000,
  "CapitalLimit": 30000000,
  "EnableTrapPhase": true,
  "TrapSellVolume": 20000,
  "TrapDropPercent": 0.5,
  "StopLossPercent": 3.0,
  "PriceImpactPerShare": 0.000011
}
```

## Output Data

After execution, the algorithm generates:

1. **Console log** - detailed information about each operation
2. **simulation_log.txt** - complete log of all trading operations
3. **price_history.csv** - price change history for analysis

### Sample Report
```
=== TRADING SUMMARY ===
Total Shares Bought: 170,000
Total Shares Sold: 170,000
Net Position: 0 shares
Average Buy Price: 23.4521
Average Sell Price: 24.1337
Total Money Spent: 3,986,857.00 EUR
Total Money Received: 4,102,729.00 EUR
Net P&L: +115,872.00 EUR
```

## Key Features

### Price Impact Model
- Linear dependency: `Δ Price = Volume × PriceImpactPerShare`
- Purchases increase price, sales decrease price
- Realistic market liquidity modeling

### P&L Calculation
- Accurate tracking of all trading operations
- Separate calculation of purchases and sales
- Stop orders counted as sales to other participants
- Volume Weighted Average Price (VWAP) calculation

### Risk Management
- **Automatic stop-loss** when loss limit is exceeded
- **Phase-by-phase monitoring** - stop-loss checked after each phase
- **Emergency liquidation** when stop-loss is triggered
- Position size control

## Technical Details

### Technologies Used
- **C# / .NET** - primary development language
- **System.Text.Json** - configuration serialization
- **File I/O** - results export

### Design Patterns
- **Strategy Pattern** - separation of trading strategy and simulator
- **Configuration Pattern** - external parameter configuration
- **Observer Pattern** - operation logging

## Limitations and Warnings

⚠️ **Educational Purposes Only**
This code is intended solely for demonstrating algorithmic trading approaches and should not be used in real markets without appropriate licenses and permissions.

⚠️ **Simplified Market Model**
The simulation uses a simplified pricing model and does not account for many real market factors.

## Possible Improvements

- [ ] Adding commissions and spreads
- [ ] Implementing more complex liquidity models
- [ ] Integration with real market data
- [ ] Graphical visualization of results
- [ ] Backtesting on historical data
- [ ] Strategy parameter optimization

## License

This project is provided for educational purposes. Commercial use or deployment on real trading platforms requires additional permissions and licenses.

---

*Developed to demonstrate algorithmic trading strategies*