# Vehicle Rental Management System

A web app for a car rental company to manage its fleet, customers and reservations, with a dashboard and reports. Built with ASP.NET Core MVC and deployed to Microsoft Azure.

**Live demo:** https://vehiclerentalmanagementsys-d6cpbnb8b4d8h6e5.canadacentral-01.azurewebsites.net/ (register an account to log in)

**Note: This app was deployed to Azure for the course. The hosted version is no longer running, but the full source, database migration and design documents are in this repo.**

Final project for the ASP.NET course. This was a team project.

## Features

- **Accounts:** register, log in and log out using ASP.NET Core Identity with cookie authentication.
- **Dashboard:** total vehicles and how many are rented, available or in maintenance; customer counts; new customers this month; upcoming reservations; reservations this month compared to last month; the five most recent customers; and the four most-rented makes.
- **Vehicles:** add, edit, delete and search the fleet.
- **Customers:** add, edit, delete and search, including contact details and payment type.
- **Reservations:** create, edit, delete and search. A reservation's status moves automatically from Upcoming to Confirmed to Completed based on its dates, and the car and customer statuses are updated to match (for example Available to Rented, Active to Renting). The car picker only offers vehicles that aren't under maintenance or already booked.
- **Reports:** filter by date range to see the most rented model and its share of reservations, average rental length in days, the peak month, the most active renter, active reservations, and the active vs. inactive customer split.

## Tech stack

| Area | What was used |
|---|---|
| Framework | ASP.NET Core MVC on .NET 8 |
| Data access | Entity Framework Core 9 (code-first with migrations) |
| Database | SQL Server, hosted on Azure SQL |
| Authentication | ASP.NET Core Identity + cookie auth |
| Hosting | Azure App Service (Canada Central) |
| Front end | Razor views with HTML, CSS and JavaScript |

## Data model

Three main tables plus the Identity tables:

- **Car:** make, model, year, colour, status
- **Customer:** name, email, phone number, payment type, status
- **Reservation:** start and end dates, status, and a foreign key to a Car and to a Customer (the renter)

One customer can have many reservations, and each reservation is for one car. The original ER diagram and design documents are in the `Project Design/` folder.

## Project layout

```
Code/asp.net-final-assignment/asp.net-final-assignment/
  Controllers/    Account, Home (dashboard + reports), Vehicle, Customer, Reservation
  Models/         Car, Customer, Reservation, AppUser
  ViewModels/     Page-specific data shapes (login, register, reports, index pages)
  Views/          Razor pages for each controller
  Data/           VehicleDbContext (EF Core + Identity)
  Migrations/     Database schema history
  Program.cs      Service setup: EF Core, Identity, cookie auth
Project Design/   Project outline, design document and ER diagram
```

## Running it locally

You need the .NET 8 SDK and a SQL Server instance.

1. Set your own connection string. The app reads the one named `Azure` (see `Program.cs`). Keep it out of source control by using user secrets:
   ```bash
   cd Code/asp.net-final-assignment/asp.net-final-assignment
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:Azure" "<your SQL Server connection string>"
   ```
2. Create the database from the included migration:
   ```bash
   dotnet ef database update
   ```
3. Start the app:
   ```bash
   dotnet run
   ```
4. Open the URL shown in the terminal and register an account.

## What I'd improve

- Add the billing and tax calculation that was in the original project outline.
- Check for overlapping bookings on the server when a reservation is created, not just in the car dropdown.
- Move the automatic status updates out of the page-load request into a background job.
- Add automated tests for the status transitions and report calculations.
