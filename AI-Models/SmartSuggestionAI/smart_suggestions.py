from fastapi import FastAPI, HTTPException
import pandas as pd
from sqlalchemy import create_engine
import random

app = FastAPI()

# Database connection
engine = create_engine(
    'mssql+pyodbc://localhost\\SQLEXPRESS/ShopAI?trusted_connection=yes&trustservercertificate=yes&driver=ODBC+Driver+17+for+SQL+Server'
)

@app.post("/smart-suggestions")
def get_smart_suggestions(cart_items: list):
    """
    Endpoint to provide smart product suggestions during checkout.
    
    cart_items: List of product IDs that the customer is adding to the cart
    """
    try:
        # Fetch all products from the database
        query = """
        SELECT p.Id AS ProductId, p.Name AS ProductName, p.Price, p.CategoryId, c.Name AS CategoryName
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        WHERE p.IsAvailable = 1
        """
        all_products = pd.read_sql(query, engine)
        print("Fetched All Products:")
        print(all_products.head())

        if all_products.empty:
            raise HTTPException(status_code=404, detail="No products available for suggestion.")

        # Get categories from the products already in the cart
        cart_product_ids = [item['ProductId'] for item in cart_items]
        cart_query = f"""
        SELECT p.Id AS ProductId, c.Name AS CategoryName
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        WHERE p.Id IN ({','.join(map(str, cart_product_ids))})
        """
        cart_product_info = pd.read_sql(cart_query, engine)
        
        if cart_product_info.empty:
            raise HTTPException(status_code=404, detail="No products found in the cart.")

        # Identify popular categories in the cart
        cart_categories = cart_product_info['CategoryName'].value_counts().index.tolist()

        # Suggest products based on the popular categories
        suggested_products = all_products[all_products['CategoryName'].isin(cart_categories)]
        
        # Randomly select between 5 and 10 products from the suggested category
        num_suggestions = random.randint(5, 10)
        suggested_products = suggested_products.sample(n=min(num_suggestions, len(suggested_products)))

        # Prepare the suggestion output
        suggestions = suggested_products[['ProductId', 'ProductName', 'Price', 'CategoryName']].to_dict(orient='records')
        
        return {
            "message": "Smart product suggestions generated successfully!",
            "suggestions": suggestions
        }

    except Exception as e:
        print(f"Error: {e}")
        raise HTTPException(status_code=500, detail="An error occurred while generating smart suggestions.")
