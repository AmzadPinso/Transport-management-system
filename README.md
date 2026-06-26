# 🚍 Smart Transport Management System (STMS) — TMS PRO

> **Academic Project** | ASP.NET Core MVC (.NET 8) | Full-Stack Enterprise Logistics Platform

---

## 📋 Project Overview

The **Smart Transport Management System (STMS)** is a comprehensive, full-stack enterprise logistics management platform developed as an academic capstone project. It provides end-to-end management of transport operations, including fleet management, driver operations, route planning, passenger booking, maintenance tracking, expense management, and advanced analytics.

The system is designed with a **role-based architecture** supporting two primary user roles:
- **Admin** — Full system access including fleet, drivers, bookings, reports, and analytics.
- **User (Passenger)** — Can search trips, book tickets, and manage personal reservations.

---

## 🛠 Technology Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core MVC (.NET 8) |
| Language | C# 12 |
| ORM | Entity Framework Core 8 |
| Database | Microsoft SQL Server (LocalDB / SQL Express) |
| Authentication | Cookie-Based Auth + ASP.NET Identity Password Hasher |
| Frontend | Bootstrap 5 + Vanilla CSS |
| Charts | ApexCharts.js |
| Icons | Font Awesome 6 |
| Fonts | Google Fonts (Inter, Poppins) |
| Architecture | Repository Pattern + Dependency Injection + Service Layer |

---

## 🏗 System Architecture

```
STMS Solution
├── Controllers/          # MVC Controllers (Admin & User routes)
├── Models/               # Domain Models + Enums + ViewModels
├── Data/
│   └── AppDbContext.cs   # EF Core DbContext with all entity configurations
├── Repository/
│   ├── Interface/        # Repository contracts (IBaseRepository<T>, IBookingRepo, etc.)
│   └── Application/      # Concrete implementations (EF Core queries)
├── Services/             # Business Logic layer (MaintenanceService, SeatService, ReportsService, SearchService)
├── Views/                # Razor Views (strongly-typed MVC views)
│   └── Shared/           # _Layout, _Sidebar, _Topbar partials
└── wwwroot/              # Static assets (CSS, JS, Fonts, Lib)
```

---

## 📦 Core Modules

### 1. 🔐 Authentication & Authorization System
- Cookie-based login with secure password hashing (BCrypt via ASP.NET Identity PasswordHasher)
- **Email Verification System** — account activation via token link sent to inbox
- **OTP-based Password Reset** — 6-digit numeric OTP with 15-minute expiry and 5-attempt brute-force protection
- **Role-Based Access Control (RBAC)** — Admin / User role-gating on controllers and views
- **Audit trail** — `LastLogIN`, `CreatedAt`, `UpdatedAt` tracked on all user accounts

### 2. 👥 User Management Module
- Admin can view, create, edit, delete user accounts
- Role assignment (Admin / User)
- Email verification status visibility
- Password management and account locking

### 3. 🚗 Vehicle Management Module
- Full CRUD for fleet vehicles (Bus, MiniBus, Truck, Car)
- Vehicle status tracking: `Active`, `InMaintenance`, `OutOfService`
- Seat capacity configuration per vehicle
- Driver assignment linkage

### 4. 🧑‍✈️ Driver Management Module
- Full CRUD for driver records
- License number, expiry tracking with **expiring-soon** alerts (30-day window)
- Availability status: `Available`, `OnTrip`, `OnLeave`, `Unavailable`
- Vehicle assignment

### 5. 🗺 Route & Station Management Module
- Multi-station route definitions with Origin ↔ Destination
- Station metadata (City, District, Address, Active status)
- Intermediate stops, pickup/drop-off point support
- Distance (km) and estimated duration tracking

### 6. 📅 Trip Scheduling & Dispatch Module
- Trip creation with vehicle, driver, route, and departure date/time assignment
- Trip statuses: `Scheduled`, `ReadyForDispatch`, `Ongoing`, `Delayed`, `Completed`, `Cancelled`
- Ticket price per trip, total seat capacity tracking
- Admin dispatch panel with status management

### 7. 🎫 Booking & Ticketing System
- Passenger trip search with filters (origin → destination, date)
- Seat selection with real-time availability
- Booking confirmation with unique alphanumeric **Booking Reference**
- Payment status: `Pending`, `Paid`, `Refunded`
- Admin booking management panel with status overrides
- Passenger booking history & ticket view

### 8. 🪑 Smart Seat Recommendation System
- Algorithm-based seat suggestions based on:
  - **Window preference** detection (seat positions A/D for 4-seat rows)
  - **Group booking** detection (adjacent seats for 2+ passengers)
  - **Standard first-available** fallback

### 9. 🔧 Maintenance Monitoring & Alerting System
- Full CRUD for maintenance records with service type, cost, provider, and next service date
- Maintenance statuses: `Scheduled`, `InProgress`, `Completed`, `Overdue`
- **Smart Alerts**: overdue and upcoming (within 7 days) maintenance alerts
- Dashboard KPIs for overdue, in-progress, and upcoming maintenance counts
- Fleet health audit report

### 10. 📋 Driver Issue Logbook
- Drivers / admins can log mechanical or operational issues
- Issue categories, severity priorities (Low / Medium / High / Critical)
- Issue statuses: `Open`, `InProgress`, `Resolved`, `Closed`
- Resolution notes and resolved timestamp tracking

### 11. 💰 Expense Tracking System
- Operational expense logging with categories: Fuel, Toll, Maintenance, Salary, Meals, etc.
- Vehicle and user linkage per expense voucher
- Monthly and total expense aggregation
- Expense audit report with category breakdown charts

### 12. 📊 Reports & Analytics Module *(Sprint 7 — Final)*
- **Revenue & Financial Reports**: income vs. expenses trend, route performance, vehicle-type revenue distribution
- **Vehicle Utilization & Fleet Analytics**: trip frequency, utilization rate, maintenance cost per vehicle
- **Passenger Booking Audits**: filterable booking register with status and date filters
- **Operating Expense Reports**: category pie chart, ledger voucher view
- **Maintenance Health Audits**: overdue detection, service log history, cost audit
- All reports support **date range filters**, **print-to-PDF**, and per-page KPI cards

### 13. 🔍 Global Search System *(Sprint 7 — Final)*
- Unified search bar in the topbar with **live autocomplete suggestions** (debounced fetch)
- Full-text search across: Users, Drivers, Vehicles, Stations, Routes, Trips, Bookings, Maintenance Records
- Categorized search results page with jump links per category
- Admin-only access

### 14. 🖥 Executive Dashboard
- Real-time KPI widgets (Vehicles, Drivers, Routes, Bookings, Revenue, Expenses)
- Weekly bookings + income trend chart (ApexCharts dual-axis area/column)
- Fleet status distribution donut chart (Active / InMaintenance / OutOfService)
- Tabbed recent activity panel: Bookings, Trips, Maintenance, Driver Issues

---

## 🚀 Setup & Running Locally

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB or SQL Express)
- Visual Studio 2022 or VS Code

### Steps

```bash
# 1. Clone the repository
git clone <repo-url>

# 2. Update connection string in appsettings.json
#    Server=<YourSqlServer>;Database=<YourDatabase>;...

# 3. Apply EF Core migrations
dotnet ef database update

# 4. Run the application
dotnet run
```

The app starts at `http://localhost:5000` (or configured port).

### Default Roles (seeded)
| Role | ID | Description |
|------|----|-------------|
| Admin | 1 | Full system access |
| User | 2 | Passenger access |

> ⚠️ No default admin user is seeded. Register a user manually then update their `RoleId` to `1` in the database, or use the **User Management** module after your first admin is created.

---

## 📁 Project File Structure (Key Files)

```
Controllers/
  AccountController.cs     — Login, Register, OTP, Email Verification
  DashboardController.cs   — Executive Dashboard KPIs + Charts
  ReportsController.cs     — Analytics & Reporting endpoints
  SearchController.cs      — Global search + suggestions API
  BookingsController.cs    — Passenger booking workflow
  AdminBookingsController  — Admin booking management
  MaintenanceController    — Fleet maintenance CRUD

Services/
  IReportsService.cs       — Reports service contract
  ReportsService.cs        — EF Core analytics queries
  ISearchService.cs        — Search contract
  SearchService.cs         — Full-text search across entities
  MaintenanceService.cs    — Maintenance alert engine
  SeatService.cs           — Smart seat recommendation algorithm

Views/
  Dashboard/Index.cshtml   — Executive dashboard
  Reports/                 — Revenue, VehicleUtilization, Bookings, Expenses, Maintenance
  Search/Index.cshtml      — Global search results
  Shared/_Layout.cshtml    — Main layout with print CSS
  Shared/_Sidebar.cshtml   — Navigation sidebar
  Shared/_Topbar.cshtml    — Topbar with live global search
```

---

## 🎓 Academic Context

This project was developed as a **7-sprint agile simulation** for academic assessment:

| Sprint | Focus Area |
|--------|-----------|
| 1–2 | Authentication, Authorization, Email Verification, OTP Reset |
| 3 | User, Vehicle, Driver, Route, Station Management |
| 4 | Trip Scheduling & Dispatch |
| 5 | Booking & Ticketing, Smart Seat Recommendation |
| 6 | Maintenance Monitoring, Driver Issue Logbook, Expense Tracking, Dashboard |
| 7 | Reports & Analytics, Global Search, Final Polishing, Documentation |

---

*© 2026 Smart Transport Management System — TMS PRO. Academic Project.*
