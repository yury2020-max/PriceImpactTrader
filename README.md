# Price Impact Trading Algorithm

A sophisticated C# implementation of a four-phase trading algorithm designed to minimize market impact while maximizing position accumulation for PUM.DE stock.

## Project Overview

This project demonstrates advanced algorithmic trading concepts including:

- Multi-phase execution strategy with dynamic volume allocation
- Market impact simulation with realistic price movements
- Order book modeling with depth and liquidity simulation
- Risk management with configurable parameters
- Performance visualization and analysis tools

## Architecture

### Core Components

- **TradingStrategy.cs** - Main algorithm implementation with four distinct phases
- **MarketSimulator.cs** - Realistic market environment simulation
- **OrderBook.cs** - Full order book implementation with matching engine
- **StrategyConfig.cs** - Configurable parameters and settings

### Trading Algorithm Phases

1. **Phase 1: Accumulation (30-40%)** - Conservative volume with minimal market impact
2. **Phase 2: Stop Hunt** - A quick sale of approximately 50% of the accumulated Phase 1 volume and then a quick buyback of shares, i.e. "unloading" retail traders by triggering their close stop orders.
3. **Phase 3: Impulse (60-70%)** - Aggressive position gain until mass stop orders are triggered.
4. **Phase 4: Exit from position (100%)** - the fastest possible exit during the triggering of mass stop orders, followed by a fairly quick closing of the remainder of the position when the market "slides" down.

## Prerequisites

- .NET 9.0 SDK
- Python 3.13+ with uv (for analysis)
- Docker (optional, for containerized deployment)
- Git

## Quick Start

### Local Development (Windows)

1. **Clone and Build**
```cmd
git clone https://github.com/yury2020-max/PriceImpactTrader.git
cd PriceImpactTrader
dotnet restore
dotnet build
```

2. **Setup Python Environment**
```cmd
cd analysis
python -m venv .venv
.venv\Scripts\activate
uv pip install -r requirements.txt
cd ..
```

3. **Run Analysis**
```cmd
# Run .NET application first
dotnet run

# Generate charts with activated Python environment
analysis\.venv\Scripts\activate
python analysis\chart_generator.py
```

**Result:** Chart saved in `charts/price_volume_analysis.png`

### Docker Deployment

1. **Build and Run .NET Application**
```cmd
docker build -t priceimpacttrader .
docker run -it --rm priceimpacttrader
```

2. **Generate Charts**
```cmd
mkdir charts
docker rm trader
docker run --name trader --entrypoint python priceimpacttrader analysis/chart_generator.py
docker cp trader:/app/charts/price_volume_analysis.png ./charts/price_volume_analysis.png
docker rm trader
```

### GitHub Codespaces

```bash
# Setup .NET
dotnet restore && dotnet run

# Setup Python
cd analysis && python3 -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt && cd ..

# Run analysis
python analysis/chart_generator.py
```

## Configuration

Modify `config.json` to adjust trading parameters:

```json
{
  "Instrument": "PUM.DE",
  "InitialPrice": 23.23,
  "TargetVolume": 170000,
  "CapitalLimit": 30000000,
  "EnableTrapPhase": true,
  "TrapSellVolume": 20000,
  "TrapDropPercent": 0.5,
  "StopLossPercent": 3.0,
  "PriceImpactPerShare": 0.000015
}
```

## Analysis and Visualization

The project includes sophisticated Python analysis tools for performance evaluation:

### Features
- **Price and Volume Charts** - Visual representation of algorithm performance
- **Trading Phase Highlighting** - Clear visualization of strategy phases
- **Performance Metrics** - Comprehensive statistical analysis
- **Market Impact Assessment** - Quantitative impact measurement

## Project Structure

```
PriceImpactTrader/
├── Program.cs              # Entry point and orchestration
├── TradingStrategy.cs      # Core algorithm logic
├── MarketSimulator.cs      # Market environment simulation
├── OrderBook.cs            # Order book and matching engine
├── StrategyConfig.cs       # Configuration management
├── config.json             # Default parameters
├── Dockerfile              # Container deployment
├── analysis/               # Python analysis tools
│   ├── chart_generator.py
│   ├── requirements.txt
│   └── .venv/
└── charts/                 # Generated analysis charts
```

## Sample Output

```
=== PRICE IMPACT TRADING ALGORITHM ===
Target Volume: 170,000 shares
Initial Price: €23.23

Phase 1 (Accumulation): Executed 68,000 shares at avg €23.36
Phase 2 (Trap): SELL 34,000 shares at avg €23.14  and BUY 34000 shares at avg €22.96
Phase 3 (Impulse): Executed 100,000 shares at avg €24.14
Phase 4 (Exit Prep): Executed 168,000 shares at avg €24.67

=== TRADING SUMMARY ===
Total Shares Bought: 202,000
Total Shares Sold (regular): 152,000
Stop Order Sales: 50,000
Total Shares Sold (all): 202,000
Net Position: 0 shares
Average Buy Price: 23.7069
Average Sell Price: 24.6363
Total Money Spent: 4788797.22 EUR
Total Money Received: 4976523.41 EUR
  - Regular Sales: 3698454.64 EUR
  - Stop Order Sales: 1278068.76 EUR
Net P&L: 187726.19 EUR
Final VWAP: 23.71
Position fully liquidated - No unrealized P&L
```

## Algorithm Insights

The algorithm employs several techniques to reduce market impact:

- **Volume Fragmentation** - Spreads large orders across time
- **Timing Diversification** - Varies execution intervals
- **Adaptive Sizing** - Adjusts to market conditions
- **Stealth Mode** - Includes deceptive pauses
- **Create false moves**

### Risk Management

- **Position Limits** - Configurable maximum position sizes
- **Price Boundaries** - Stop-loss and take-profit levels
- **Volatility Monitoring** - Real-time risk assessment
- **Liquidity Checks** - Ensures adequate market depth

## Key Features

- Realistic market modeling — authentic price movements and liquidity
- Customizable parameters — flexible strategy customization
- Performance analytics — comprehensive reporting and visualization
- Docker Ready — production deployment capabilities
- Multilingual analysis — C# execution + Python analytics

## Future Enhancements

- [ ] Machine Learning Integration - Predictive market impact models
- [ ] Multi-asset Support - Portfolio-level optimization
- [ ] Real-time Data Feeds - Live market data integration
- [ ] Advanced Analytics - Additional performance metrics
- [ ] Web Dashboard - Real-time monitoring interface

## Testing

```bash
# Run tests
dotnet test

# Build verification
dotnet build --configuration Release

# Docker build test
docker build -t test-image .
```

## License

This project is for educational and demonstration purposes. See LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit issues and enhancement requests.

---

Built with modern C# and containerized for professional deployment. Includes comprehensive Python analysis tools for performance evaluation.