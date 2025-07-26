from fastapi import FastAPI, HTTPException
import pandas as pd
import pickle
import torch
import torch.nn as nn
import torch.optim as optim
from sqlalchemy import create_engine
from sklearn.neighbors import NearestNeighbors
import os

# Initialize FastAPI
app = FastAPI()

# Database connection
engine = create_engine(
    'mssql+pyodbc://localhost\\SQLEXPRESS/ShopAI?trusted_connection=yes&trustservercertificate=yes&driver=ODBC+Driver+17+for+SQL+Server'
)

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

# Helper function: Retrain models
def retrain_models():
    print("Retraining models...")

    # Fetch data with SQL joins to include categories
    query = """
        SELECT o.CustomerId, oi.ProductId, p.CategoryId, c.Name AS CategoryName, oi.Quantity
        FROM Orders o
        INNER JOIN OrderItems oi ON o.Id = oi.OrderId
        INNER JOIN Products p ON oi.ProductId = p.Id
        INNER JOIN Categories c ON p.CategoryId = c.Id
    """
    orders = pd.read_sql(query, engine)
    print("Fetched Orders, OrderItems, and Categories Data.")
    print(orders.head())

    # Create User-Product Matrix
    user_product_data = orders[['CustomerId', 'ProductId', 'Quantity']]
    user_product_matrix = user_product_data.pivot_table(
        index='CustomerId', columns='ProductId', values='Quantity', fill_value=0
    )
    print("Created User-Product Matrix:")
    print(user_product_matrix.head())

    # Train Collaborative Filtering (KNN)
    knn_model = NearestNeighbors(metric='cosine', algorithm='brute')
    knn_model.fit(user_product_matrix)

    # Save the KNN model
    knn_model_path = 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/knn_model.pkl'
    with open(knn_model_path, 'wb') as file:
        pickle.dump(knn_model, file)

    # Save User-Product Matrix
    user_product_matrix_path = 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/user_product_matrix.csv'
    user_product_matrix.to_csv(user_product_matrix_path)
    print(f"User-Product Matrix saved at: {user_product_matrix_path}")
    columns_path = 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/columns.pkl'
    with open(columns_path, 'wb') as f:
        pickle.dump(user_product_matrix.columns.tolist(), f)


    # Save column order (product IDs)
    with open('F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/columns.pkl', 'wb') as f:
        pickle.dump(user_product_matrix.columns.tolist(), f)


    # Train Deep Learning Model (PyTorch)
    unique_customers = user_product_data['CustomerId'].max() + 1
    unique_products = user_product_data['ProductId'].max() + 1
    X = torch.tensor(user_product_data[['CustomerId', 'ProductId']].values, dtype=torch.long)
    y = torch.tensor(user_product_data['Quantity'].values, dtype=torch.float)

    embed_dim = 50
    model = RecommendationModel(unique_customers, unique_products, embed_dim)
    criterion = nn.MSELoss()
    optimizer = optim.Adam(model.parameters(), lr=0.001)

    epochs = 5
    batch_size = 32
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

    # Save the Deep Learning model
    dl_model_path = 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/recommendation_model.pth'
    torch.save(model.state_dict(), dl_model_path)
    print("Models retrained successfully.")
    return knn_model, model

# API Endpoint: Provide Recommendations
@app.post("/recommend")
def get_recommendations(customer_id: int):
    print(f"Fetching recommendations for Customer ID: {customer_id}")
    knn_model_path = 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/knn_model.pkl'

    try:
        query = """
            SELECT o.CustomerId, oi.ProductId, p.CategoryId, c.Name AS CategoryName, oi.Quantity
            FROM Orders o
            INNER JOIN OrderItems oi ON o.Id = oi.OrderId
            INNER JOIN Products p ON oi.ProductId = p.Id
            INNER JOIN Categories c ON p.CategoryId = c.Id
        """
        orders = pd.read_sql(query, engine)

        if orders.empty:
            raise HTTPException(status_code=500, detail="Orders data is empty.")

        user_product_matrix = orders.pivot_table(
            index='CustomerId', columns='ProductId', values='Quantity', fill_value=0
        )

        with open('F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/columns.pkl', 'rb') as f:
            expected_columns = pickle.load(f)

# Reindex matrix to match the trained model
        user_product_matrix = user_product_matrix.reindex(columns=expected_columns, fill_value=0)



        if customer_id not in user_product_matrix.index:
            raise HTTPException(status_code=404, detail="Customer ID not found.")

        if not os.path.exists(knn_model_path):
            raise HTTPException(status_code=500, detail="KNN model file not found.")

        with open(knn_model_path, 'rb') as file:
            knn_model = pickle.load(file)

        distances, indices = knn_model.kneighbors(
            user_product_matrix.loc[[customer_id]], n_neighbors=20
        )
        initial_recommendations = indices.flatten().tolist()

        customer_orders = orders[orders['CustomerId'] == customer_id]
        category_scores = customer_orders['CategoryId'].value_counts(normalize=True)
        favorite_categories = category_scores.index.tolist()

        category_filtered_recommendations = [
            pid for pid in initial_recommendations
            if pid in orders['ProductId'].values and
            orders[orders['ProductId'] == pid]['CategoryId'].values[0] in favorite_categories
        ]

        sorted_recommendations = sorted(
            category_filtered_recommendations,
            key=lambda pid: category_scores.get(orders[orders['ProductId'] == pid]['CategoryId'].values[0], 0),
            reverse=True
        )

        final_recommendations = sorted_recommendations[:10]

        # Fallback logic if recommendations are less than 3
        if len(final_recommendations) < 3:
            top_products = (
                orders.groupby('ProductId')['Quantity']
                .sum()
                .sort_values(ascending=False)
                .head(5)
                .index.tolist()
            )
            for product_id in top_products:
                if product_id not in final_recommendations:
                    final_recommendations.append(product_id)

        product_ids = final_recommendations[:10]

        query_products = f"""
            SELECT p.Id AS ProductId, p.Name, p.Price, p.ProductImage, p.Description, p.CategoryId, c.Name AS CategoryName
            FROM Products p
            LEFT JOIN Categories c ON p.CategoryId = c.Id
            WHERE p.Id IN ({','.join(map(str, product_ids))})
            """

        products_df = pd.read_sql(query_products, engine)

        # Convert to list of dicts for JSON response
        products_list = products_df.to_dict(orient='records')

        return {
        "recommended_products": products_list,
        "message": "Product Recommendations generated successfully!"
        }

    except Exception as e:
        print(f"Error: {e}")
        raise HTTPException(status_code=500, detail="An internal server error occurred.")


    if __name__ == "__main__":
        retrain_models()


