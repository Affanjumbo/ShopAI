import pandas as pd
import pickle
from sklearn.neighbors import NearestNeighbors
import torch
import torch.nn as nn
import torch.optim as optim
import os

output_dir = 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models'
os.makedirs(output_dir, exist_ok=True)
# Load preprocessed data
data_path = 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/data/processed_data/user_product_data.csv'
data = pd.read_csv(data_path)

# Prepare data for Collaborative Filtering (Machine Learning Model)
user_product_matrix = data.pivot_table(index='CustomerId', columns='ProductId', values='Quantity', fill_value=0)
knn_model = NearestNeighbors(metric='cosine', algorithm='brute')
knn_model.fit(user_product_matrix)

# Save Collaborative Filtering model (KNN)
with open('F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/knn_model.pkl', 'wb') as file:
    pickle.dump(knn_model, file)


with open(os.path.join(output_dir, 'columns.pkl'), 'wb') as f:
    pickle.dump(user_product_matrix.columns.tolist(), f)

# Prepare data for Deep Learning Model
unique_customers = data['CustomerId'].max() + 1
unique_products = data['ProductId'].max() + 1
X = torch.tensor(data[['CustomerId', 'ProductId']].values, dtype=torch.long)
y = torch.tensor(data['Quantity'].values, dtype=torch.float)

# Build the Deep Learning Model (PyTorch)
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

# Initialize the model
embed_dim = 50
model = RecommendationModel(unique_customers, unique_products, embed_dim)
criterion = nn.MSELoss()
optimizer = optim.Adam(model.parameters(), lr=0.001)

# Train the Deep Learning Model
epochs = 10
batch_size = 32
for epoch in range(epochs):
    model.train()
    total_loss = 0
    for i in range(0, len(X), batch_size):
        X_batch = X[i:i + batch_size]
        y_batch = y[i:i + batch_size]
        optimizer.zero_grad()
        predictions = model(X_batch).squeeze()
        loss = criterion(predictions, y_batch)
        loss.backward()
        optimizer.step()
        total_loss += loss.item()
    print(f'Epoch {epoch + 1}/{epochs}, Loss: {total_loss:.4f}')

# Save the trained Deep Learning Model
torch.save(model.state_dict(), 'F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/models/recommendation_model.pth')