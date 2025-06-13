import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.dates import DateFormatter
import matplotlib.dates as mdates

def create_price_volume_chart(csv_file_path='price_history.csv'):
    df = pd.read_csv(csv_file_path)

    # Создаем временные метки, если их нет
    if 'Time' not in df.columns:
        df['Time'] = pd.date_range(start='2024-01-01 09:30:00',
                                   periods=len(df),
                                   freq='1min')

    # График
    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(14, 10),
                                   gridspec_kw={'height_ratios': [3, 1]})

    # Цена
    ax1.plot(df['Time'], df['Price'],
             color='#1f77b4', linewidth=2, label='Price')
    ax1.set_ylabel('Price (EUR)', fontsize=12, fontweight='bold')
    ax1.set_title('PUM.DE - Price Impact Trading Algorithm Results',
                  fontsize=14, fontweight='bold')
    ax1.grid(True, alpha=0.3)
    ax1.legend()

    # Фазы
    highlight_trading_phases(ax1, df)

    # Объем
    colors = ['green' if vol > 0 else 'red' for vol in df['Volume']]
    ax2.bar(df['Time'], abs(df['Volume']),
            color=colors, alpha=0.7, width=0.0006)
    ax2.set_ylabel('Volume', fontsize=12, fontweight='bold')
    ax2.set_xlabel('Time', fontsize=12, fontweight='bold')
    ax2.grid(True, alpha=0.3)

    # Формат времени
    formatter = DateFormatter('%H:%M')
    ax1.xaxis.set_major_formatter(formatter)
    ax2.xaxis.set_major_formatter(formatter)
    plt.setp(ax2.xaxis.get_majorticklabels(), rotation=45)

    plt.tight_layout()
    plt.savefig('price_volume_analysis.png', dpi=300, bbox_inches='tight')
    plt.show()
    print("График сохранен как 'price_volume_analysis.png'")

def generate_volume_from_price_changes(prices):
    """
    Генерирует объемы на основе изменений цены
    """
    volumes = []
    
    for i in range(len(prices)):
        if i == 0:
            volumes.append(1000)  # Начальный объем
        else:
            price_change = abs(prices[i] - prices[i-1])
            
            # Большие изменения цены = больший объем
            if price_change > 0.05:  # Крупная сделка
                volume = np.random.randint(8000, 15000)
            elif price_change > 0.01:  # Средняя сделка
                volume = np.random.randint(3000, 8000)
            else:  # Мелкая сделка
                volume = np.random.randint(500, 3000)
            
            # Положительный объем для роста цены, отрицательный для падения
            if prices[i] > prices[i-1]:
                volumes.append(volume)  # Покупки
            else:
                volumes.append(-volume)  # Продажи
    
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
    Анализирует производительность торгового алгоритма
    """
    try:
        df = pd.read_csv(csv_file_path)
    except:
        df = pd.read_csv(csv_file_path, names=['Index', 'Price'])
    
    # Базовая статистика
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
    
    # Ищем файл в разных местах
    csv_paths = [
        'price_history.csv',           # В текущей папке
        '../price_history.csv',        # В родительской папке
        '../bin/Debug/net8.0/price_history.csv'  # В папке сборки
    ]
    
    csv_file = None
    for path in csv_paths:
        if os.path.exists(path):
            csv_file = path
            print(f"Найден файл: {path}")
            break
    
    if not csv_file:
        print("ОШИБКА: price_history.csv не найден!")
        print("Сначала запустите C# программу: dotnet run")
        print(f"Искал в: {csv_paths}")
        exit(1)
    
    print(f"Использую файл: {csv_file}")
    
    try:
        print("\nСоздание графика цены и объема...")
        create_price_volume_chart(csv_file)
        
        print("\nАнализ производительности...")
        analyze_trading_performance(csv_file)
        
        print("\n✅ Анализ завершен успешно!")
        
    except Exception as e:
        print(f"\n❌ Ошибка: {e}")
        print("Проверьте, что установлены все зависимости:")
        print("uv pip install pandas matplotlib numpy")