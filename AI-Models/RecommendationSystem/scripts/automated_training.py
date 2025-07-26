from fastapi import FastAPI
import pandas as pd
import pickle
import torch
import torch.nn as nn
import torch.optim as optim
from sqlalchemy import create_engine
from sklearn.neighbors import NearestNeighbors
from sklearn.metrics import mean_squared_error, precision_score, recall_score
import matplotlib.pyplot as plt

app = FastAPI()

# Database connection
engine = create_engine('mssql+pyodbc://localhost\\SQLEXPRESS/ShopAI?trusted_connection=yes&trustservercertificate=yes&driver=ODBC+Driver+17+for+SQL+Server')

# PyTorch Model Class
class RecommendationModel(nn.Module):
    def __init__(self, num_customers, num_products, embed_dim):
        super(RecommendationModel, self).__init__()
        self.customer_embedding = nn.Embedding(num_customers, embed_dim)
        self.product_embedding = nn.Embedding(num_products, embed_dim)
        self.fc = nn.Linear(embed_dim * 2, 1)

    def forward(self, x):
        customer_embeds = self.customer_embedding(x[:, 0])
        product_embeds = self.product_embedding(x[:, 1])
        x = torch.cat([customer_embeds, product_embeds], dim=1)
        return self.fc(x)

# Helper function to retrain models and log performance
def retrain_and_log_metrics():
    # Fetch data from SQL Server
    orders = pd.read_sql("SELECT * FROM Orders", engine)
    order_items = pd.read_sql("SELECT * FROM OrderItems", engine)
    user_history = pd.merge(orders, order_items, on="Id", how="inner")
    user_product_data = user_history[['CustomerId', 'ProductId', 'Quantity']]

    # Step 1: Train Collaborative Filtering (KNN)
    user_product_matrix = user_product_data.pivot_table(index='CustomerId', columns='ProductId', values='Quantity', fill_value=0)
    knn_model = NearestNeighbors(metric='cosine', algorithm='brute')
    knn_model.fit(user_product_matrix)

    # Evaluate KNN Model
    test_data = user_product_matrix.sample(frac=0.2)
    mse_knn = mean_squared_error(test_data.values.flatten(), knn_model.kneighbors(test_data, return_distance=True)[0].flatten())

    # Save KNN model
    with open('F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/knn_model.pkl', 'wb') as file:
        pickle.dump(knn_model, file)

    # Step 2: Train Deep Learning Model (PyTorch)
    unique_customers = user_product_data['CustomerId'].max() + 1
    unique_products = user_product_data['ProductId'].max() + 1
    X = torch.tensor(user_product_data[['CustomerId', 'ProductId']].values, dtype=torch.long)
    y = torch.tensor(user_product_data['Quantity'].values, dtype=torch.float)

    embed_dim = 50
    model = RecommendationModel(unique_customers, unique_products, embed_dim)
    criterion = nn.MSELoss()
    optimizer = optim.Adam(model.parameters(), lr=0.001)

    # Train the model
    epochs = 5
    batch_size = 32
    total_loss = 0
    for epoch in range(epochs):
        model.train()
        for i in range(0, len(X), batch_size):
            X_batch = X[i:i + batch_size]
            y_batch = y[i:i + batch_size]
            optimizer.zero_grad()
            predictions = model(X_batch).squeeze()
            loss = criterion(predictions, y_batch)
            loss.backward()
            optimizer.step()
            total_loss += loss.item()
    mse_dl = total_loss / epochs

    # Save Deep Learning model
    torch.save(model.state_dict(), 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/recommendation_model.pth')

    # Log Performance Metrics
    performance = {
        'ModelName': ['KNN', 'Deep Learning'],
        'TrainingDate': [pd.Timestamp.now()] * 2,
        'MSE': [mse_knn, mse_dl]
    }
    csv_path = 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/performance_metrics.csv'
    try:
        existing_data = pd.read_csv(csv_path)
        updated_data = pd.concat([existing_data, pd.DataFrame(performance)], ignore_index=True)
    except FileNotFoundError:
        updated_data = pd.DataFrame(performance)
    updated_data.to_csv(csv_path, index=False)

    # Generate Graphs
    plt.figure(figsize=(10, 5))
    for model_name in updated_data['ModelName'].unique():
        model_data = updated_data[updated_data['ModelName'] == model_name]
        plt.plot(model_data['TrainingDate'], model_data['MSE'], label=f'{model_name} MSE')

    plt.xlabel('Training Date')
    plt.ylabel('MSE (Loss)')
    plt.title('Model Performance Over Time')
    plt.legend()
    plt.grid()
    plt.savefig('F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/performance_graph.png')
    plt.show()

    return knn_model, model

# API Endpoint: Retrain and Provide Recommendations
@app.post("/recommend")
def get_recommendations(customer_id: int):
    knn_model, dl_model = retrain_and_log_metrics()

    # Generate KNN Recommendations
    user_product_matrix = pd.read_sql("SELECT * FROM Orders", engine).pivot_table(
        index='CustomerId', columns='ProductId', values='Quantity', fill_value=0
    )
    distances, indices = knn_model.kneighbors(
        user_product_matrix[user_product_matrix.index == customer_id], n_neighbors=5
    )
    knn_recommendations = indices.flatten().tolist()

    return {"knn_recommendations": knn_recommendations}