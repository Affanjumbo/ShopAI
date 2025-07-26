from fastapi import FastAPI, HTTPException, Query
import pandas as pd
from sqlalchemy import create_engine

# Initialize FastAPI
app = FastAPI()

# Database connection
engine = create_engine(
    'mssql+pyodbc://localhost\\SQLEXPRESS/ShopAI?trusted_connection=yes&trustservercertificate=yes&driver=ODBC+Driver+17+for+SQL+Server'
)

@app.post("/smart-suggestions")
def smart_suggestions(product_id: int = Query(..., description="Enter Product ID to generate smart suggestions")):
    try:
        # Log start of the request
        print(f"Starting smart suggestions for Product ID: {product_id}")

        # Step 1: Fetch product details
        print(f"Fetching details for Product ID: {product_id}")
        product_query = f"""
        SELECT 
            p.Id AS ProductId,
            p.Name AS ProductName,
            p.Price,
            p.ProductImage,
            p.CategoryId
        FROM Products p
        WHERE p.Id = {product_id}
        """
        selected_product = pd.read_sql(product_query, engine)

        if selected_product.empty:
            print(f"Product ID {product_id} not found in the database.")
            raise HTTPException(status_code=404, detail=f"Product ID {product_id} not found.")

        print(f"Product details found: {selected_product}")

        category_id = selected_product.loc[0, "CategoryId"]

        # Step 2: Fetch co-occurrence data within the same category (products bought together)
        print(f"Fetching related products within the same category for Product ID: {product_id}")
        association_query = f"""
        SELECT oi2.ProductId AS RelatedProductId, COUNT(*) AS Frequency
        FROM OrderItems oi1
        INNER JOIN OrderItems oi2 ON oi1.OrderId = oi2.OrderId
        INNER JOIN Products p ON oi2.ProductId = p.Id
        WHERE oi1.ProductId = {product_id} AND oi1.ProductId != oi2.ProductId AND p.CategoryId = {category_id}
        GROUP BY oi2.ProductId
        ORDER BY Frequency DESC
        """
        related_products = pd.read_sql(association_query, engine)

        if related_products.empty:
            print(f"No related products found for Product ID {product_id}. Fetching fallback suggestions.")

            # Step 3: Fallback to top trending products within the same category
            fallback_query = f"""
            SELECT 
                TOP 10 oi.ProductId, 
                COUNT(oi.ProductId) AS PurchaseCount, 
                p.Name AS ProductName, 
                p.Price, 
                p.ProductImage,
                p.CategoryId
            FROM OrderItems oi
            INNER JOIN Products p ON oi.ProductId = p.Id
            WHERE p.CategoryId = {category_id}
            GROUP BY oi.ProductId, p.Name, p.Price, p.CategoryId, p.ProductImage
            ORDER BY PurchaseCount DESC
            """
            trending_products = pd.read_sql(fallback_query, engine)

            if trending_products.empty:
                print("Fallback failed: No trending products available within the same category.")
                raise HTTPException(status_code=500, detail="No fallback suggestions available in the database.")

            print(f"Fallback suggestions: {trending_products}")

            # Format fallback suggestions
            suggestions = []
            for _, row in trending_products.iterrows():
                suggestions.append({
                    "ProductId": row['ProductId'],
                    "Name": row['ProductName'],
                    "Price": row['Price'],
                    "ProductImage": row['ProductImage']
                })

            return {
                "InputProductId": product_id,
                "Suggestions": suggestions,
                "Message": "Fallback suggestions based on trending products within the same category have been generated."
            }

        print(f"Related products found: {related_products}")

        # Step 4: Fetch details for related products
        print("Fetching details for related products...")
        related_product_ids = related_products['RelatedProductId'].tolist()
        product_details_query = f"""
        SELECT 
            p.Id AS ProductId,
            p.Name AS ProductName,
            p.Price,
            p.ProductImage,
            p.CategoryId
        FROM Products p
        WHERE p.Id IN ({','.join(map(str, related_product_ids))})
        """
        related_product_details = pd.read_sql(product_details_query, engine)

        if related_product_details.empty:
            print(f"No details found for related products of Product ID {product_id}.")
            raise HTTPException(status_code=500, detail="Related product details not available.")

        print(f"Details for related products: {related_product_details}")

        # Step 5: Merge frequency data with product details
        print("Merging frequency data with product details...")
        merged_data = pd.merge(
            related_products, 
            related_product_details, 
            left_on='RelatedProductId', 
            right_on='ProductId'
        )
        merged_data['Score'] = merged_data['Frequency']  # Assign frequency as the scoring metric

        # Step 6: Sort and limit suggestions
        print("Sorting and limiting suggestions...")
        merged_data = merged_data.sort_values(by='Score', ascending=False)
        top_suggestions = merged_data.head(10)

        # Step 7: Format suggestions for output
        suggestions = []
        for _, row in top_suggestions.iterrows():
            suggestions.append({
                "ProductId": row['ProductId'],
                "Name": row['ProductName'],
                "Price": row['Price'],
                "Score": row['Score'],
                "ProductImage": row['ProductImage']
            })

        print(f"Smart suggestions generated successfully for Product ID {product_id}.")
        return {
            "InputProductId": product_id,
            "Suggestions": suggestions,
            "Message": "Smart suggestions generated successfully!"
        }

    except HTTPException as e:
        raise e
    except Exception as e:
        print(f"Internal Server Error: {e}")
        raise HTTPException(status_code=500, detail="An internal server error occurred.")