# app/cart/cart_service.py

from sqlalchemy import create_engine
from fastapi import HTTPException
import pandas as pd

# Database connection
engine = create_engine(
    'mssql+pyodbc://localhost\\SQLEXPRESS/ShopAI?trusted_connection=yes&trustservercertificate=yes&driver=ODBC+Driver+17+for+SQL+Server'
)

# Helper function to add products to the cart
def add_to_cart(customer_id: int, product_ids: list):
    """
    Add products to a customer's cart.
    """
    try:
        query = f"""
        SELECT * FROM CartItems
        WHERE CustomerId = {customer_id} AND ProductId IN ({','.join(map(str, product_ids))})
        """
        existing_items = pd.read_sql(query, engine)

        if existing_items.empty:
            for product_id in product_ids:
                insert_query = f"""
                INSERT INTO CartItems (CustomerId, ProductId, Quantity, CreatedAt)
                VALUES ({customer_id}, {product_id}, 1, GETDATE())
                """
                engine.execute(insert_query)
            return {"message": "Products added to cart successfully!"}
        else:
            return {"message": "Some products are already in the cart."}
    except Exception as e:
        print(f"Error adding products to cart: {e}")
        raise HTTPException(status_code=500, detail="Error adding products to cart.")
