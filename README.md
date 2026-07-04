# AurHER

Production-ready e-commerce platform built with ASP.NET Core for the Nigerian fashion market.

AurHER was designed to provide a seamless online shopping experience while demonstrating modern backend engineering practices, scalable architecture, and production-ready application development.

**Live Demo:** https://aurher.onrender.com

---

## Overview

AurHER is a full-stack e-commerce application that enables customers to browse products, place orders, and complete secure payments online. Alongside the customer experience, the platform includes a comprehensive administration dashboard for managing products, inventory, customers, and orders.

The project emphasizes clean architecture, maintainability, and real-world backend development using ASP.NET Core.

---

## Features

### Customer Experience

- Browse products by category
- Product search and filtering
- Shopping cart
- Secure checkout
- Paystack payment integration
- Order history
- Email notifications

### Administration

- Product management
- Category management
- Inventory management
- Order management
- Customer management
- Dashboard analytics

### Backend

- Layered architecture
- RESTful design principles
- Entity Framework Core
- Authentication & Authorization
- Background email processing
- Image optimization
- PostgreSQL database

---

## Architecture

AurHER follows a layered architecture that separates business logic, data access, and presentation to improve maintainability and scalability.

```
Presentation (MVC)
        │
Business Logic
        │
Entity Framework Core
        │
PostgreSQL
```

---

## Tech Stack

### Backend

- ASP.NET Core MVC
- C#
- Entity Framework Core

### Database

- PostgreSQL

### Frontend

- HTML
- CSS
- JavaScript

### Integrations

- Paystack
- MailKit

---

## Project Structure

```
├── Controllers/               # HTTP request handlers
│   ├── SessionController/     # BaseController with SessionId property
│   ├── AdminController.cs     # Login, Logout, Dashboard
│   ├── CartController.cs      # Add, Update, Remove, Clear, Count
│   ├── CategoryController.cs  # Admin CRUD + Public listing
│   ├── CheckoutController.cs  # Checkout form, PlaceOrder
│   ├── CollectionController.cs# Admin CRUD + add/remove products
│   ├── DeliveryLocationController.cs
│   ├── HomeController.cs      # Storefront homepage
│   ├── InventoryController.cs # Admin inventory view
│   ├── OrderController.cs     # Admin order management
│   ├── PaymentController.cs   # Initialize, Callback, Webhook, Retry
│   ├── ProductController.cs   # Admin product CRUD + variants + images
│   ├── ShopController.cs      # Public product listing + detail
│   └── TrackController.cs     # Customer order tracking
├── DTOs/
│   ├── Admin/                 # Dashboard, Login, Category, Collection, Product DTOs
│   ├── Payment/               # PaymentInitResult, PaymentCallbackResult
│   ├── Paystack/              # Paystack API request/response DTOs
│   └── Store/                 # Cart, Checkout, Shop, Home page DTOs
├── Models/
│   ├── Enums/                 # OrderStatus, PaymentStatus
│   └── *.cs                   # 13 entity models
├── Repositories/
│   ├── Interfaces/            # IRepository<T> + 7 specific interfaces
│   └── *.cs                   # 7 concrete repository classes
├── Services/
│   ├── Interface/             # 15 service interfaces
│   └── *.cs                   # 15 concrete service classes
├── Data/
│   └── AppDbContext.cs        # EF Core DbContext, 13 DbSets
├── Migrations/                # EF Core migration files
├── Views/                     # Razor views per controller
├── wwwroot/
│   ├── css/                   # Per-feature CSS files
│   ├── js/                    # Per-feature JS/TS files
│   ├── fonts/                 # DancingScript + system fonts
│   └── images/products/{id}/  # Product image storage
├── Program.cs                 # DI registrations + middleware pipeline
└── appsettings.json           # Config: DB, admin creds, email, SMS, Paystack
 

```

---

## Running Locally

```bash
git clone https://github.com/khaleed-x/AurHER.git

cd AurHER
```

Configure the application settings:

- PostgreSQL connection string
- Paystack API keys
- Email configuration

Run the application:

```bash
dotnet run
```

---

## Future Improvements

- Docker support
- Product recommendations
- Redis caching
- Advanced search
- Order tracking
- Cloud deployment

---

## Author

**Khalid Ayomide Oyekunle**

Backend Software Engineer

Portfolio:
https://khaleed-portfolio.vercel.app
