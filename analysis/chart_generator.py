import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from matplotlib.dates import DateFormatter
import matplotlib.dates as mdates

def create_price_volume_chart(csv_file_path='price_history.csv'):
    """
    Создает график цены и объема из файла price_history.csv
    """
    
    # Читаем данные из CSV файла, созданного C# программой
    try:
        # Если файл имеет заголовки
        df = pd.read_csv(csv_file_path)
    except:
        # Если файл без заголовков (только индекс и цена)
        df = pd.read_csv(csv_file_path, names=['Index', 'Price'])
    
    # Генерируем объемы на основе изменений цены (симуляция)
    df['Volume'] = generate_volume_from_price_changes(df['Price'])
    
    # Создаем временные метки
    df['Time'] = pd.date_range(start='2024-01-01 09:30:00', 
                               periods=len(df), 
                               freq='1min')
    
    # Создаем график с двумя осями Y
    fig, (ax1, ax2) = plt.subplots(2, 1, figsize=(14, 10), 
                                   gridspec_kw={'height_ratios': [3, 1]})
    
    # График цены (верхний)
    ax1.plot(df['Time'], df['Price'], 
             color='#1f77b4', linewidth=2, label='Price')
    ax1.set_ylabel('Price (EUR)', fontsize=12, fontweight='bold')
    ax1.set_title('PUM.DE - Price Impact Trading Algorithm Results', 
                  fontsize=14, fontweight='bold')
    ax1.grid(True, alpha=0.3)
    ax1.legend()
    
    # Выделяем фазы алгоритма цветом
    highlight_trading_phases(ax1, df)
    
    # График объема (нижний)
    colors = ['green' if vol > 0 else 'red' for vol in df['Volume']]
    ax2.bar(df['Time'], abs(df['Volume']), 
            color=colors, alpha=0.7, width=0.0006)
    ax2.set_ylabel('Volume', fontsize=12, fontweight='bold')
    ax2.set_xlabel('Time', fontsize=12, fontweight='bold')
    ax2.grid(True, alpha=0.3)
    
    # Форматирование времени
    formatter = DateFormatter('%H:%M')
    ax1.xaxis.set_major_formatter(formatter)
    ax2.xaxis.set_major_formatter(formatter)
    
    # Поворачиваем подписи времени
    plt.setp(ax2.xaxis.get_majorticklabels(), rotation=45)
    
    plt.tight_layout()
    plt.show()
    
    # Сохраняем график
    plt.savefig('price_volume_analysis.png', dpi=300, bbox_inches='tight')
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
    """
    Выделяет фазы торгового алгоритма на графике
    """
    total_points = len(df)
    
    # Примерные границы фаз (можно настроить)
    phase1_end = int(total_points * 0.3)    # Фаза 1: 30%
    phase2_end = int(total_points * 0.4)    # Фаза 2: 10%
    phase3_end = int(total_points * 0.7)    # Фаза 3: 30%
    # Фаза 4: оставшиеся 30%
    
    # Добавляем вертикальные линии для разделения фаз
    ax.axvline(x=df['Time'].iloc[phase1_end], color='red', linestyle='--', alpha=0.7)
    ax.axvline(x=df['Time'].iloc[phase2_end], color='orange', linestyle='--', alpha=0.7)
    ax.axvline(x=df['Time'].iloc[phase3_end], color='purple', linestyle='--', alpha=0.7)
    
    # Добавляем подписи фаз
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