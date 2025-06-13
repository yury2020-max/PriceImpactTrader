Price Impact Trading Algorithm
A sophisticated C# implementation of a four-phase trading algorithm designed to minimize market impact while maximizing position accumulation for PUM.DE stock.
Project Overview
This project demonstrates advanced algorithmic trading concepts including:

Multi-phase execution strategy with dynamic volume allocation
Market impact simulation with realistic price movements
Order book modeling with depth and liquidity simulation
Risk management with configurable parameters
Performance visualization and analysis tools

Architecture
Core Components

TradingStrategy.cs - Main algorithm implementation with four distinct phases
MarketSimulator.cs - Realistic market environment simulation
OrderBook.cs - Full order book implementation with matching engine
StrategyConfig.cs - Configurable parameters and settings

Trading Algorithm Phases

Phase 1: Accumulation (30%) - Conservative volume with minimal market impact
Phase 2: Trap Phase (10%) - Strategic pause to confuse other algorithms
Phase 3: Impulse (30%) - Aggressive accumulation during momentum
Phase 4: Exit Preparation (30%) - Gradual position adjustment

Quick Start
Prerequisites

.NET 8.0 SDK
Docker (optional, for containerized deployment)
Python 3.8+ with uv (optional, for analysis)

Running the Algorithm
bash# Clone the repository
git clone https://github.com/yury2020-max/PriceImpactTrader.git
cd PriceImpactTrader

# Build and run
dotnet build
dotnet run

# Run with custom configuration
dotnet run config.json
Configuration
Modify config.json to adjust trading parameters:
json{
  "TargetVolume": 100000,
  "MaxPriceImpact": 0.02,
  "SimulationDuration": 300,
  "InitialPrice": 22.75,
  "BaseSpread": 0.01,
  "VolatilityFactor": 0.005
}
Analysis and Visualization
Python Analysis Tools
The project includes sophisticated Python analysis tools for performance evaluation:
bash# Setup Python environment
cd analysis
uv venv
source .venv/bin/activate  # On Windows: .venv\Scripts\activate
uv pip install -r requirements.txt

# Generate analysis charts
python chart_generator.py
Features

Price and Volume Charts - Visual representation of algorithm performance
Trading Phase Highlighting - Clear visualization of strategy phases
Performance Metrics - Comprehensive statistical analysis
Market Impact Assessment - Quantitative impact measurement

Docker Deployment
Build and Run Container
bash# Build Docker image
docker build -t price-trader .

# Run in container
docker run price-trader

# Run with custom config
docker run -v $(pwd)/config.json:/app/config.json price-trader config.json
Multi-stage Build
The Dockerfile uses multi-stage builds for optimized production images:

Build stage: Uses .NET SDK for compilation
Runtime stage: Lightweight runtime-only image

Order Book Implementation
Advanced order book features:

Real-time matching engine with price-time priority
Market and limit order execution
Price impact simulation
Liquidity depth modeling
Spread calculation and monitoring

csharpvar orderBook = new OrderBook(22.75m);
var (avgPrice, executed) = orderBook.ExecuteMarketBuy(10000);
orderBook.ApplyPriceImpact(0.01m);
Performance Metrics
The algorithm tracks and reports:

Total Volume Executed - Actual vs. target volume
Average Execution Price - VWAP analysis
Market Impact - Price movement attribution
Phase Performance - Individual phase effectiveness
Risk Metrics - Drawdown and volatility analysis

Development
Project Structure
PriceImpactTrader/
├── Program.cs              # Entry point and orchestration
├── TradingStrategy.cs      # Core algorithm logic
├── MarketSimulator.cs      # Market environment simulation
├── OrderBook.cs           # Order book and matching engine
├── StrategyConfig.cs      # Configuration management
├── config.json           # Default parameters
├── Dockerfile            # Container deployment
├── analysis/             # Python analysis tools
│   ├── chart_generator.py
│   ├── requirements.txt
│   └── .venv/
└── .github/workflows/    # CI/CD pipeline
Testing
bash# Run tests
dotnet test

# Build verification
dotnet build --configuration Release

# Docker build test
docker build -t test-image .
Sample Output
=== PRICE IMPACT TRADING ALGORITHM ===
Target Volume: 100,000 shares
Initial Price: €22.75

Phase 1 (Accumulation): Executed 30,000 shares at avg €22.76
Phase 2 (Trap): Executed 10,000 shares at avg €22.77  
Phase 3 (Impulse): Executed 30,000 shares at avg €22.79
Phase 4 (Exit Prep): Executed 30,000 shares at avg €22.81

Total Executed: 100,000 shares
VWAP: €22.78
Market Impact: +0.26% (+€0.06)
Algorithm Efficiency: 94.2%
Key Features

Realistic Market Simulation - Authentic price movements and liquidity
Advanced Order Book - Full depth-of-market implementation
Configurable Parameters - Flexible strategy customization
Performance Analytics - Comprehensive reporting and visualization
Docker Ready - Production deployment capabilities
Multi-language Analysis - C# execution + Python analytics

Algorithm Insights
Market Impact Minimization
The algorithm employs several techniques to reduce market impact:

Volume Fragmentation - Spreads large orders across time
Timing Diversification - Varies execution intervals
Adaptive Sizing - Adjusts to market conditions
Stealth Mode - Includes deceptive pauses

Risk Management

Position Limits - Configurable maximum position sizes
Price Boundaries - Stop-loss and take-profit levels
Volatility Monitoring - Real-time risk assessment
Liquidity Checks - Ensures adequate market depth

Future Enhancements

 Machine Learning Integration - Predictive market impact models
 Multi-asset Support - Portfolio-level optimization
 Real-time Data Feeds - Live market data integration
 Advanced Analytics - Additional performance metrics
 Web Dashboard - Real-time monitoring interface

License
This project is for educational and demonstration purposes. See LICENSE file for details.
Contributing
Contributions are welcome! Please feel free to submit issues and enhancement requests.

Built with modern C# and containerized for professional deployment. Includes comprehensive Python analysis tools for performance evaluation.