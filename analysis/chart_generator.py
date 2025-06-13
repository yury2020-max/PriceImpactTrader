import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.dates import DateFormatter
import matplotlib.dates as mdates

def create_price_volume_chart(csv_file_path='price_history.csv'):
    """
    Creates a price and volume chart from the price_history.csv file
    """

    # Read data from the CSV file created by the C# program
    try:
        # If the file has headers
        df = pd.read_csv(csv_file_path)
    except:
        # If the file has no headers (only index and price)
        df = pd.read_csv(csv_file_path, names=['Index', 'Price'])

    # Generate volumes based on price changes (simulation)
    df['Volume'] = generate_volume_from_price_changes(df['Price'])

    # Create timestamps
    df['Time'] = pd.date_range(start='2024-01-01 09:30:00',
                               periods=len(df),
                               freq='1min')

    # Create a chart with two Y-axes
    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(14, 10),
                                   gridspec_kw={'height_ratios': [3, 1]})

    # Price chart (upper)
    ax1.plot(df['Time'], df['Price'],
             color='#1f77b4', linewidth=2, label='Price')
    ax1.set_ylabel('Price (EUR)', fontsize=12, fontweight='bold')
    ax1.set_title('PUM.DE - Price Impact Trading Algorithm Results', 
                  fontsize=14, fontweight='bold')
    ax1.grid(True, alpha=0.3)
    ax1.legend()

    # Highlight trading phases
    highlight_trading_phases(ax1, df)
    
    # Volume graph (bottom)
    colors = ['green' if vol > 0 else 'red' for vol in df['Volume']]
    ax2.bar(df['Time'], abs(df['Volume']), 
            color=colors, alpha=0.7, width=0.0006)
    ax2.set_ylabel('Volume', fontsize=12, fontweight='bold')
    ax2.set_xlabel('Time', fontsize=12, fontweight='bold')
    ax2.grid(True, alpha=0.3)

    # Time formatting
    formatter = DateFormatter('%H:%M')
    ax1.xaxis.set_major_formatter(formatter)
    ax2.xaxis.set_major_formatter(formatter)

    # Rotate time labels
    plt.setp(ax2.xaxis.get_majorticklabels(), rotation=45)
    
    plt.tight_layout()
    plt.show()

    # Save the chart
    plt.savefig('price_volume_analysis.png', dpi=300, bbox_inches='tight')
    print("Chart saved as 'price_volume_analysis.png'")

def generate_volume_from_price_changes(prices):
    """
    Generates volumes based on price changes
    """
    volumes = []
    
    for i in range(len(prices)):
        if i == 0:
            volumes.append(1000)  # Initial volume
        else:
            price_change = abs(prices[i] - prices[i-1])

            # Large price changes = higher volume
            if price_change > 0.05:  # Large trade
                volume = np.random.randint(8000, 15000)
            elif price_change > 0.01:  # Medium trade
                volume = np.random.randint(3000, 8000)
            else:  # Small trade
                volume = np.random.randint(500, 3000)

            # Positive volume for price increases, negative for decreases
            if prices[i] > prices[i-1]:
                volumes.append(volume)  # Buys
            else:
                volumes.append(-volume)  # Sells

    return volumes

def highlight_trading_phases(ax, df):
    """
    Highlights trading phases on the chart
    """
    total_points = len(df)

    # Approximate phase boundaries (can be adjusted)
    phase1_end = int(total_points * 0.3)    # Phase 1: 30%
    phase2_end = int(total_points * 0.4)    # Phase 2: 10%
    phase3_end = int(total_points * 0.7)    # Phase 3: 30%
    # Phase 4: remaining 30%

    # Add vertical lines to separate phases
    ax.axvline(x=df['Time'].iloc[phase1_end], color='red', linestyle='--', alpha=0.7)
    ax.axvline(x=df['Time'].iloc[phase2_end], color='orange', linestyle='--', alpha=0.7)
    ax.axvline(x=df['Time'].iloc[phase3_end], color='purple', linestyle='--', alpha=0.7)

    # Add phase labels
    ax.text(df['Time'].iloc[int(phase1_end/2)], df['Price'].max(),
            'Phase 1\nAccumulation', ha='center', va='top',
            bbox=dict(boxstyle='round', facecolor='lightblue', alpha=0.7))
    
    ax.text(df['Time'].iloc[phase1_end + int((phase2_end-phase1_end)/2)], 
            df['Price'].max(), 'Phase 2\nTrap', ha='center', va='top',
            bbox=dict(boxstyle='round', facecolor='lightcoral', alpha=0.7))
    
    ax.text(df['Time'].iloc[phase2_end + int((phase3_end-phase2_end)/2)], 
            df['Price'].max(), 'Phase 3\nImpulse', ha='center', va='top',
            bbox=dict(boxstyle='round', facecolor='lightgreen', alpha=0.7))
    
    ax.text(df['Time'].iloc[phase3_end + int((total_points-phase3_end)/2)], 
            df['Price'].max(), 'Phase 4\nExit', ha='center', va='top',
            bbox=dict(boxstyle='round', facecolor='lightyellow', alpha=0.7))

def analyze_trading_performance(csv_file_path='price_history.csv'):
    """
    Analyzes the performance of the trading algorithm
    """
    try:
        df = pd.read_csv(csv_file_path)
    except:
        df = pd.read_csv(csv_file_path, names=['Index', 'Price'])

    # Basic statistics
    start_price = df['Price'].iloc[0]
    end_price = df['Price'].iloc[-1]
    max_price = df['Price'].max()
    min_price = df['Price'].min()
    
    print("\n=== TRADING PERFORMANCE ANALYSIS ===")
    print(f"Start Price: {start_price:.2f} EUR")
    print(f"End Price: {end_price:.2f} EUR")
    print(f"Max Price: {max_price:.2f} EUR")
    print(f"Min Price: {min_price:.2f} EUR")
    print(f"Total Price Change: {((end_price - start_price) / start_price * 100):.2f}%")
    print(f"Max Drawdown: {((min_price - max_price) / max_price * 100):.2f}%")
    print(f"Volatility: {df['Price'].std():.4f}")

if __name__ == "__main__":
    import os
    
    print("=== Диагностика ===")
    print(f"Текущая папка: {os.getcwd()}")
    
    # Looking for the file in different places
    csv_paths = [
        'price_history.csv',           # In the current folder
        '../price_history.csv',        # In the parent folder
        '../bin/Debug/net8.0/price_history.csv'  # In the build folder
    ]
    
    csv_file = None
    for path in csv_paths:
        if os.path.exists(path):
            csv_file = path
            print(f"Found file: {path}")
            break
    
    if not csv_file:
        print("ERROR: price_history.csv not found!")
        print("Please run the C# program first: dotnet run")
        print(f"Looked in: {csv_paths}")
        exit(1)

    print(f"Using file: {csv_file}")

    try:
        print("\nCreating price and volume chart...")
        create_price_volume_chart(csv_file)

        print("\nAnalyzing performance...")
        analyze_trading_performance(csv_file)

        print("\n Analysis completed successfully!")

    except Exception as e:
        print(f"\n Error: {e}")
        print("Please ensure all dependencies are installed:")
        print("uv pip install pandas matplotlib numpy")