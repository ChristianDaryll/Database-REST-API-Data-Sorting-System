# Database-REST-API-Data-Sorting-System
A robust, production-grade backend web service built with **ASP.NET Core**, **Entity Framework Core**, and a **normalized SQL Server database**. This system is designed to handle high-integrity transactional order processing, advanced server-side data sorting, dynamic filtering, pagination, and real-time business analytics.

---

## Key Features

* **Normalized Relational Schema:** Designed with referential integrity across Customers, Products, Orders, and OrderItems (junction table) with explicit foreign key constraints and optimized indexes.
* **ACID Transactions:** Order creation wraps inventory stock validation and decrementing inside an explicit SQL transaction (`BeginTransactionAsync`), ensuring zero risk of race conditions or out-of-sync inventory.
* **Dynamic Querying & Sorting:** Supports flexible REST query parameters for status filtering, ascending/descending ordering by date or monetary amount, and server-side pagination (`Skip`/`Take`).
* **High-Performance Projection:** Leverages Entity Framework Core LINQ projections (`.Select()`) to fetch only necessary columns directly at the database level, preventing over-fetching and circular reference loops.
* **Request Validation:** Integrated with **FluentValidation** to ensure strict input hygiene and guard against malformed data before payloads hit the database.
* **Business Analytics Engine:** Aggregates live operational metrics including total revenue, total order counts, average order value, and top-performing products ranked by sales volume.

---

## 🛠️ Tech Stack

* **Backend Framework:** ASP.NET Core Web API (.NET 8 / .NET 9)
* **Data Access:** Entity Framework Core (EF Core) & LINQ
* **Database:** Microsoft SQL Server / LocalDB / SQL Server Management Studio (SSMS)
* **Validation:** FluentValidation
* **Documentation & Testing:** OpenAPI / Swagger UI & Visual Studio `.http` files

---
