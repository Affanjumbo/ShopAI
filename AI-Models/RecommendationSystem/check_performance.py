import pandas as pd
import matplotlib.pyplot as plt
import os

# File paths
metrics_csv_path = 'F:/AI-Models/RecommendationSystem/models/performance_metrics.csv'
performance_graph_path = 'F:/AI-Models/RecommendationSystem/models/performance_graph.png'
user_product_matrix_path = 'F:/AI-Models/RecommendationSystem/models/user_product_matrix.csv'

def check_latest_performance():
    # Check if metrics CSV file exists
    if not os.path.exists(metrics_csv_path):
        print("Performance metrics file not found. Run retraining first.")
        return
    
    print("Loading performance metrics...")
    performance_data = pd.read_csv(metrics_csv_path)
    print("\nPerformance Metrics:")
    print(performance_data.tail())  # Display the latest metrics

    # Generate Graphs
    plt.figure(figsize=(10, 5))
    for model_name in performance_data['ModelName'].unique():
        model_data = performance_data[performance_data['ModelName'] == model_name]
        plt.plot(model_data['TrainingDate'], model_data['MSE'], label=f'{model_name} MSE')

    plt.xlabel('Training Date')
    plt.ylabel('MSE (Loss)')
    plt.title('Model Performance Over Time')
    plt.legend()
    plt.grid()
    plt.savefig(performance_graph_path)
    plt.show()
    print(f"Performance graph saved at: {performance_graph_path}")

# Call the function
if __name__ == "__main__":
    check_latest_performance()