# TechWORLD - Product Management System (PMS)

A modern, responsive Product Management System built with **ASP.NET Core 8.0 Razor Pages**, **C#**, and **Microsoft SQL Server** using clean **Repository Pattern with ADO.NET**.

---

## 🌟 Key Features

- **Role-Based Access**:
  - **Admin & Stock Manager**: Full inventory management, category management, real-time KPI metrics, order processing.
  - **Customer**: Clean storefront, dynamic product filtering, shopping cart, checkout with house address, payment options, and order history.
- **Clean Architecture & ADO.NET**:
  - Separation of concerns: Models, Repository Interfaces, ADO.NET Implementations (`SqlConnection`, `SqlCommand`, `SqlDataReader`, `SqlTransaction`).
  - Automatic database creation and schema migrations on first launch.
- **Security**:
  - Secure PBKDF2 password hashing with cryptographically random salt.
  - Cookie authentication with role claims.
  - Optional `SECRET-CODE: ANYONEUSEIT` for instant Admin/Stock Manager privilege elevation.
- **Responsive UI**:
  - Dark-mode authentication cards.
  - Clean tech-store layout with vector hardware assets.
  - Client-side and server-side validation.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB or SQL Server instance (default connection: `(localdb)\MSSQLLocalDB`)

### Run Locally
```bash
# Clone the repository
git clone <your-github-repo-url>
cd "SMT-PMS AW"

# Restore & Run
dotnet run
```
Open your browser to: **`http://localhost:5000`**

---

## 📁 Project Structure

```
├── Data/                 # Connection factory & Auto-database initializer
├── Models/               # Domain entities (Product, Category, Order, Cart, User)
├── Repositories/         # Repository Pattern interfaces & ADO.NET data access
├── Services/             # Auth service & image handling
├── Pages/
│   ├── Account/          # Dark theme Login & Register with SECRET-CODE
│   ├── Admin/            # Admin dashboard & customer orders management
│   ├── Products/         # Product catalog CRUD (Add, Edit, Details, Delete)
│   ├── Categories/       # Category management
│   ├── Customer/         # Storefront, Cart, Checkout & Profile Address
│   └── About.cshtml      # Architecture diagrams & overview
└── wwwroot/              # CSS, JS, and vector hardware images
```

