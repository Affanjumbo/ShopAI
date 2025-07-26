import pandas as pd
from sklearn.cluster import KMeans
import pickle
import torch
import torch.nn as nn
import torch.optim as optim

# Paths
SPENDING_HISTORY_PATH = "F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/ExpenseManager/data/customer_spending.csv"
KMEANS_MODEL_PATH = "F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/ExpenseManager/models/kmeans_model.pkl"
EXPENSE_MODEL_PATH = "F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/ExpenseManager/models/expense_recommendation.pth"

# K-Means Training
def train_kmeans():
    data = pd.read_csv(SPENDING_HISTORY_PATH)
    kmeans = KMeans(n_clusters=5, random_state=42)
    data['Cluster'] = kmeans.fit_predict(data[['SubCategory', 'TotalSpent']])
    with open(KMEANS_MODEL_PATH, 'wb') as file:
        pickle.dump(kmeans, file)

# Deep Learning Model
class ExpenseRecommendationModel(nn.Module):
    def __init__(self, num_customers, num_products, embed_dim):
        super(ExpenseRecommendationModel, self).__init__()
        self.customer_embedding = nn.Embedding(num_customers, embed_dim)
        self.product_embedding = nn.Embedding(num_products, embed_dim)
        self.fc = nn.Linear(embed_dim * 2, 1)

    def forward(self, x):
        customer_embeds = self.customer_embedding(x[:, 0])
        product_embeds = self.product_embedding(x[:, 1])
        x = torch.cat([customer_embeds, product_embeds], dim=1)
        return self.fc(x)

def train_expense_model():
    data = pd.read_csv(SPENDING_HISTORY_PATH)
    num_customers = data['CustomerId'].nunique()
    num_products = data['ProductId'].nunique()
    X = torch.tensor(data[['CustomerId', 'ProductId']].values, dtype=torch.long)
    y = torch.tensor(data['Interaction'].values, dtype=torch.float)

    model = ExpenseRecommendationModel(num_customers, num_products, embed_dim=50)
    criterion = nn.BCEWithLogitsLoss()
    optimizer = optim.Adam(model.parameters(), lr=0.001)

    for epoch in range(10):
        optimizer.zero_grad()
        predictions = model(X).squeeze()
        loss = criterion(predictions, y)
        loss.backward()
        optimizer.step()

    torch.save(model.state_dict(), EXPENSE_MODEL_PATH)