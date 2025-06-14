import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.dates import DateFormatter
import matplotlib.dates as mdates

def create_price_volume_chart(csv_file_path='price_history.csv'):
    df = pd.read_csv(csv_file_path)

    # Create timestamps if there are none
    if 'Time' not in df.columns:
        df['Time'] = pd.date_range(start='2024-01-01 09:30:00',
                                   periods=len(df),
                                   freq='1min')

    # Chart
    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(14, 10),
                                   gridspec_kw={'height_ratios': [3, 1]})

    # Price
    ax1.plot(df['Time'], df['Price'],
             color='#1f77b4', linewidth=2, label='Price')
    ax1.set_ylabel('Price (EUR)', fontsize=12, fontweight='bold')
    ax1.set_title('PUM.DE - Price Impact Trading Algorithm Results',
                  fontsize=14, fontweight='bold')
    ax1.grid(True, alpha=0.3)
    ax1.legend()

    # Phases
    highlight_trading_phases(ax1, df)

    # Volume
    colors = ['green' if vol > 0 else 'red' for vol in df['Volume']]
    ax2.bar(df['Time'], abs(df['Volume']),
            color=colors, alpha=0.7, width=0.0006)
    ax2.set_ylabel('Volume', fontsize=12, fontweight='bold')
    ax2.set_xlabel('Time', fontsize=12, fontweight='bold')
    ax2.grid(True, alpha=0.3)

    # Time format
    formatter = DateFormatter('%H:%M')
    ax1.xaxis.set_major_formatter(formatter)
    ax2.xaxis.set_major_formatter(formatter)
    plt.setp(ax2.xaxis.get_majorticklabels(), rotation=45)

    plt.tight_layout()
    plt.savefig('price_volume_analysis.png', dpi=300, bbox_inches='tight')
    plt.show()
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
            if price_change > 0.05:  # Major trade
                volume = np.random.randint(8000, 15000)
            elif price_change > 0.01:  # Average trade
                volume = np.random.randint(3000, 8000)
            else:  # Minor trade
                volume = np.random.randint(500, 3000)

            # Positive volume for price increases, negative for decreases
            if prices[i] > prices[i-1]:
                volumes.append(volume)  # Buys
            else:
                volumes.append(-volume)  # Sells

    return volumes

def highlight_trading_phases(ax, df):
    df['PhaseChange'] = df['Phase'] != df['Phase'].shift()
    phase_changes = df[df['PhaseChange']].copy()
    phase_changes = pd.concat([phase_changes, df.iloc[[-1]]])

    for i in range(1, len(phase_changes)):
        phase_name = phase_changes['Phase'].iloc[i-1]
        x_pos = df['Time'].iloc[phase_changes.index[i]]

        colors = {
            'Phase1': 'red',
            'Phase2': 'orange',
            'Phase3': 'purple',
            'Phase4': 'green'
        }

        color = colors.get(phase_name, 'gray')
        ax.axvline(x=x_pos, color=color, linestyle='--', alpha=0.7)

        start_idx = phase_changes.index[i-1]
        end_idx = phase_changes.index[i]
        mid_idx = start_idx + (end_idx - start_idx) // 2
        mid_time = df['Time'].iloc[mid_idx]

        ax.text(mid_time, df['Price'].max(), phase_name, ha='center', va='top',
                bbox=dict(boxstyle='round', facecolor='lightgray', alpha=0.5))

    df.drop(columns='PhaseChange', inplace=True)


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
    
    create_price_volume_chart('price_history.csv')
    
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

        print("\nAnalysis completed successfully!")

    except Exception as e:
        print(f"\nError: {e}")
        print("Please ensure all dependencies are installed:")
        print("uv pip install pandas matplotlib numpy")