import matplotlib.pyplot as plt
import pandas as pd

# Example model metrics - replace with your actual model metrics
metrics = {
    "precision": 0.85,
    "recall": 0.88,
    "accuracy": 0.87,
    "f1_score": 0.86
}

def generate_metrics_graph(metrics):
    # Plotting model metrics
    plt.figure(figsize=(10, 6))
    plt.bar(metrics.keys(), metrics.values(), color='skyblue')
    plt.title("Model Evaluation Metrics")
    plt.ylabel("Score")
    plt.xlabel("Metric")
    graph_path = "F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/ExpenseManager/graphs/model_metrics.png"
    plt.savefig(graph_path)
    plt.close()
    return graph_path

def generate_metrics_table(metrics):
    # Convert metrics into a DataFrame
    metrics_df = pd.DataFrame(list(metrics.items()), columns=['Metric', 'Value'])
    table_path = "F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/ExpenseManager/tables/model_metrics.csv"
    metrics_df.to_csv(table_path, index=False)
    return table_path

def get_last_accessed_customer_id():
    try:
        with open('last_accessed_customer_id.txt', 'r') as f:
            last_accessed_customer_id = f.read().strip()
        return last_accessed_customer_id
    except Exception as e:
        return f"Error reading last accessed customer ID: {str(e)}"

if __name__ == "__main__":
    # Generate and save the graph and table for model metrics
    graph_path = generate_metrics_graph(metrics)
    table_path = generate_metrics_table(metrics)

    print(f"Model Metrics Graph saved at: {graph_path}")
    print(f"Model Metrics Table saved at: {table_path}")
    print(f"Last accessed Customer ID: {get_last_accessed_customer_id()}")
