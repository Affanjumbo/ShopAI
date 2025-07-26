from sqlalchemy import create_engine
import pandas as pd

# Establish connection to SQL Server
engine = create_engine(
    'mssql+pyodbc://localhost\\SQLEXPRESS/ShopAI?trusted_connection=yes&trustservercertificate=yes&driver=ODBC+Driver+17+for+SQL+Server'
)

# Fetch required tables
orders = pd.read_sql("SELECT * FROM Orders", engine)
order_items = pd.read_sql("SELECT * FROM OrderItems", engine)
products = pd.read_sql("SELECT * FROM Products", engine)
feedbacks = pd.read_sql("SELECT * FROM Feedbacks", engine)

# Merge and preprocess data
user_history = pd.merge(orders, order_items, on="Id", how="inner")
user_product_data = user_history[['CustomerId', 'ProductId', 'Quantity']]

# Save preprocessed data
user_product_data.to_csv('F:/VisualStudio Data/ShopAI/ShopAI/AI-Models/RecommendationSystem/data/processed_data/user_product_data.csv', index=False)