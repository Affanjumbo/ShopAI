# app/suggestions/suggestion_service.py

from fastapi import HTTPException
import pandas as pd
import random
from sqlalchemy import create_engine

# Database connection
engine = create_engine(
    'mssql+pyodbc://localhost\\SQLEXPRESS/ShopAI?trusted_connection=yes&trustservercertificate=yes&driver=ODBC+Driver+17+for+SQL+Server'
)

def get_smart_suggestions(customer_id: int):
    try:
        # Fetch products in the cart for the given customer
        query = f"""
        SELECT ci.ProductId, p.Name AS ProductName, p.Price, p.CategoryId, c.Name AS CategoryName
        FROM CartItems ci
        INNER JOIN Products p ON ci.ProductId = p.Id
        INNER JOIN Categories c ON p.CategoryId = c.Id
        WHERE ci.CustomerId = {customer_id} AND ci.Quantity > 0
        """
        cart_items = pd.read_sql(query, engine)
        
        if cart_items.empty:
            raise HTTPException(status_code=404, detail="No products in the cart.")

        # Identify categories in the cart
        cart_categories = cart_items['CategoryName'].value_counts().index.tolist()

        # Fetch available products from the same categories
        all_products_query = f"""
        SELECT p.Id AS ProductId, p.Name AS ProductName, p.Price, p.CategoryId, c.Name AS CategoryName
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        WHERE p.IsAvailable = 1
        """
        all_products = pd.read_sql(all_products_query, engine)
        
        # Filter products based on categories in the cart
        suggested_products = all_products[all_products['CategoryName'].isin(cart_categories)]

        # Randomly select between 5 and 10 products
        num_suggestions = random.randint(5, 10)
        suggested_products = suggested_products.sample(n=min(num_suggestions, len(suggested_products)))

        # Prepare the suggestions
        suggestions = suggested_products[['ProductId', 'ProductName', 'Price', 'CategoryName']].to_dict(orient='records')

        return {
            "message": "Smart product suggestions generated successfully!",
            "suggestions": suggestions
        }

    except Exception as e:
        print(f"Error: {e}")
        raise HTTPException(status_code=500, detail="Error generating smart suggestions.")
