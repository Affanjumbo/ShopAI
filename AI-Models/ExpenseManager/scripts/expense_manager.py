import sys
import os
import numpy as np
import pandas as pd
from collections import defaultdict
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from sqlalchemy import create_engine, text

# Add the scripts directory to path
sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

# Import updated grocery list function
from scripts.budget_allocation import generate_grocery_list

app = FastAPI()

# DB connection
engine = create_engine(
    'mssql+pyodbc://localhost\\SQLEXPRESS/ShopAI?trusted_connection=yes&trustservercertificate=yes&driver=ODBC+Driver+17+for+SQL+Server'
)

class BudgetRequest(BaseModel):
    CustomerId: int
    Budget: float

@app.post("/expense-manager")
def manage_budget(request: BudgetRequest):
    customer_id = request.CustomerId
    budget = request.Budget

    if budget < 2000:
        raise HTTPException(status_code=400, detail="Budget must be at least 2000 PKR.")

    try:
        history_query = """
        SELECT sc.Name AS SubCategoryName, SUM(oi.Quantity * oi.UnitPrice) AS TotalSpent
        FROM OrderItems oi
        INNER JOIN Products p ON oi.ProductId = p.Id
        INNER JOIN SubCategories sc ON p.SubCategoryId = sc.Id
        INNER JOIN Orders o ON oi.OrderId = o.Id
        WHERE o.CustomerId = :CustomerId
        GROUP BY sc.Name
        """
        user_history = pd.read_sql(text(history_query), engine, params={"CustomerId": customer_id})
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error fetching user history: {str(e)}")

    try:
        product_query = """
        SELECT p.Id AS ProductId, p.Name AS ProductName, p.Price, p.ProductImage, sc.Name AS SubCategoryName
        FROM Products p
        INNER JOIN SubCategories sc ON p.SubCategoryId = sc.Id
        WHERE p.CategoryId = 16 AND p.IsAvailable = 1
        """
        products = pd.read_sql(text(product_query), engine)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error fetching products: {str(e)}")

    if products.empty:
        return {
            "Message": "No grocery items available.",
            "Budget": budget,
            "UtilizedAmount": 0,
            "Balance": budget,
            "BudgetPlan": []
        }

    try:
        grocery_items = generate_grocery_list(budget, products)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error generating grocery list: {str(e)}")

    utilized_amount = sum(item['Price'] for item in grocery_items)
    balance = budget - utilized_amount

    grouped = defaultdict(list)
    for item in grocery_items:
        grouped[item["SubCategory"]].append({
            "ProductId": item["ProductId"],
            "ProductName": item["ProductName"],
            "Price": item["Price"],
            "ProductImage": item["ProductImage"]
        })

    grocery_list = [{"SubCategory": k, "Items": v} for k, v in grouped.items()]

    finalized = True
    if finalized:
        try:
            with engine.connect() as connection:
                for item in grocery_items:
                    history_update_query = text("""
                    INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice, Discount)
                    VALUES (:OrderId, :ProductId, :Quantity, :UnitPrice, :TotalPrice, :Discount)
                    """)
                    connection.execute(
                        history_update_query,
                        {
                            "OrderId": 1,
                            "ProductId": item["ProductId"],
                            "Quantity": 1,
                            "UnitPrice": item["Price"],
                            "TotalPrice": item["Price"],
                            "Discount": 0
                        }
                    )
        except Exception as e:
            raise HTTPException(status_code=500, detail=f"Error updating user history: {str(e)}")

    # Record the last accessed customer ID
    try:
        with open('last_accessed_customer_id.txt', 'w') as f:
            f.write(str(customer_id))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error writing last accessed customer ID: {str(e)}")

    return {
        "Message": f"Grocery list generated successfully for Customer ID: {customer_id}.",
        "Budget": budget,
        "UtilizedAmount": utilized_amount,
        "Balance": balance,
        "BudgetPlan": grocery_list
    }
