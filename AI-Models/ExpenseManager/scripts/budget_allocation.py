import pandas as pd

def generate_grocery_list(budget, products, spending_history=None):
    # Filter available products (already done in SQL but just in case)
    available_products = products.copy()
    
    # Sort by price ascending
    available_products = available_products.sort_values(by="Price")

    grocery_list = []
    current_total = 0

    for _, product in available_products.iterrows():
        price = product["Price"]
        if current_total + price <= budget:
            grocery_list.append({
                "SubCategory": product["SubCategoryName"],
                "ProductId": product["ProductId"],
                "ProductName": product["ProductName"],
                "Price": price,
                "ProductImage": product["ProductImage"]
            })
            current_total += price
        else:
            break

    return grocery_list
