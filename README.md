# ShopAI

**ShopAI** is an intelligent, multi-vendor e-commerce platform developed as a Final Year Project. Built using the **ASP.NET MVC framework**, **Microsoft SQL Server**, and integrated with **AI modules via FastAPI**, ShopAI delivers a personalized, scalable, and user-friendly shopping experience across web and mobile platforms.

## Table of Contents
- [Overview](#overview)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [System Architecture](#system-architecture)
- [AI Modules](#ai-modules)
- [Setup Instructions](#setup-instructions)
- [Screenshots](#screenshots)
- [Contributors](#contributors)
- [License](#license)

---

## Overview

ShopAI aims to modernize the e-commerce experience using artificial intelligence. It supports multiple user roles, including **Admin**, **Vendor**, and **Customer**, and integrates intelligent features like **product recommendations**, **checkout suggestions**, and **expense tracking** using Python-based FastAPI services.

## Key Features

- 🛍️ Multi-vendor product listing and management  
- 📦 Full shopping cart and order workflow  
- 🤖 AI-powered product recommendations and smart checkout  
- 💸 Expense manager for user spending insights  
- 📱 Mobile version built with Flutter  
- 🔒 Secure user authentication with token-based access  
- 📊 Admin dashboard for full data and vendor control  
- 🌐 Fully responsive front-end with subtle animations

## Tech Stack

### Backend
- **ASP.NET MVC** (C#)
- **Microsoft SQL Server**
- **Entity Framework**

### AI & Microservices
- **FastAPI** (Python)
- **Swagger** for API documentation
- **Trained ML Models** for recommendation and expense analysis

### Frontend
- **HTML**, **CSS**, **JavaScript**, **jQuery**
- **Bootstrap** for responsive design
- **Flutter** (for mobile application)

### Tools & Utilities
- **Visual Studio**
- **VS Code**
- **Git & GitHub**
- **Postman** (for API testing)
- **FileZilla** (for deployment)
- **Swagger UI** (for API exploration)

## System Architecture

- ASP.NET MVC handles web routing, business logic, and role-based authorization.
- SQL Server manages structured e-commerce data with stored procedures.
- FastAPI services run independently and are consumed via HTTP REST APIs.
- Frontend (HTML/CSS/JS) dynamically interacts with APIs using AJAX.
- Mobile app communicates with backend via RESTful APIs.

## AI Modules

1. **Product Recommendation** – Personalized suggestions based on user behavior.
2. **Smart Checkout** – Intelligent item suggestion during the checkout phase.
3. **Expense Manager** – Categorizes and visualizes spending patterns.
4. **AI Chatbot** – Assists users with queries (available in web version only).

## Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone https://github.com/AffanJumbo/shopai.git
   ```

2. **ASP.NET Setup**
   - Open solution in **Visual Studio**.
   - Set connection string to your local **SQL Server** instance.
   - Run database migrations or use provided `.sql` script to generate schema.

3. **AI Models**
   - Navigate to `AI-Models/` directory.
   - Install Python dependencies:
     ```bash
     pip install -r requirements.txt
     ```
   - Run each service:
     ```bash
     uvicorn model_service.py --reload
     ```

<img width="975" height="773" alt="image" src="https://github.com/user-attachments/assets/83cfb16e-14f7-4b20-b1e2-bb95ff8f7534" />

<img width="758" height="403" alt="image" src="https://github.com/user-attachments/assets/c061be4b-132f-4096-b19c-689d9601af2f" />


4. **Frontend**
   - Frontend is served via the ASP.NET project.
   - Assets are located in `/Views`, `/Scripts`, and `/Content` folders.

5. **Flutter App**
   - Open in **Android Studio** or **VS Code**.
   - Set up emulator or physical device.
   - Run:
     ```bash
     flutter pub get
     flutter run
     ```


## Contributors

- **Affan Ahmed** – Full-Stack Developer, AI Integration, System Architecture  
- **Aima Khan** – Front-End Web Development, App Developer

## License

This project is developed for academic purposes and is not licensed for commercial distribution. For learning or demo use only.
