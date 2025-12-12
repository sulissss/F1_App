# F1 Racing Database Application

A comprehensive F1 data application powered by .NET 8 (ASP.NET Core MVC) and SQL Server.

## Features
- **Driver & Team Profiles**: Detailed stats, headshots, and team branding.
- **Race Results**: Qualifying and Race data processing.
- **Dynamic Stats**: Backend calculation of standings, fastest laps, and head-to-head comparisons.
- **3NF Database**: Normalized schema with centralized Country and Image storage.

## Prerequisites
- **.NET 8 SDK**
- **SQL Server** (LocalDB, Docker, or Express)
- **Git**

## Setup Guide

### 1. Database Setup
1. Open **SQL Server Management Studio (SSMS)** or **Azure Data Studio**.
2. Connect to your SQL Server instance.
3. Open and Execute **`group26_p2.sql`**.
   - This single script creates the database `F1Database`, tables, views, stored procedures, **AND** inserts all necessary data (including team logos and flags as BLOBs).

### 2. Configure Connection String
1. Open `F1App/appsettings.json`.
2. Update the `DefaultConnection` string to point to your SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=F1Database;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```
   *(Adjust `Server` and auth settings as needed for your environment)*

### 3. Run the Application
Open a terminal in the `F1App` directory:

```bash
cd F1App
dotnet build
dotnet run
```

Access the app at `http://localhost:5218`.

## Project Structure
- **/F1App**: ASP.NET Core Web Application
  - **/Controllers**: MVC Controllers (`HomeController.cs` handles all views).
  - **/Services**: Data Access Logic (`LinqF1Service.cs`).
  - **/Views**: Razor Views for UI.
- **group26_p2.sql**: Complete Database Setup Script (Schema + Data).
- **database_changes.sql**: Delta script containing only recent 3NF and Image modifications.
- **database_structure.md**: Detailed documentation of the database schema.
