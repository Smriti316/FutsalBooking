# PlayZone Futsal Booking System

This is my internship project. A web-based futsal court booking system built using ASP.NET Core MVC.

## About the Project

The company (PlayZone Futsal, Baneshwor) was managing all bookings through phone calls and a physical register. This was causing problems like double bookings, no booking history, and difficulty for customers to check availability.

My task was to build a simple online booking system to solve these problems.

## Features I Built

- Customer registration and login
- View available courts
- Book a court by selecting date and time
- Conflict detection (won't let you book an already booked slot)
- View your booking history
- Cancel a booking
- Admin panel to manage courts, bookings and users

## Features NOT yet done (plan to finish)

- Khalti/eSewa payment integration (sir said to do later)
- Email confirmation after booking
- SMS notification
- Search/filter courts by type
- Booking report export (PDF)

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server (LocalDB for development)
- ASP.NET Identity for auth
- Bootstrap 5 for UI

## How to Run

1. Open the solution in Visual Studio 2022
2. Make sure SQL Server is installed (LocalDB is fine)
3. Open Package Manager Console and run:
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
4. Press F5 to run

Default admin account is created automatically:
- Email: admin@playzone.com
- Password: Admin@123

## Database

Main tables:
- AspNetUsers - stores user accounts
- Courts - the futsal courts
- Bookings - all booking records

## Notes

- Soft delete is used for courts (IsActive flag) because we don't want to lose booking history
- Booking conflict is checked server-side in BookingController
- TimeSpan is used for storing time (StartTime, EndTime) since we only need HH:mm

## Problems I faced

- At first I was getting error with migrations because I forgot to add connection string in appsettings.json
- The time overlap logic was confusing at first, had to draw it on paper to understand
- Role-based authorization took time to set up correctly

---
Developed by: [Your Name]
Internship at: [Company Name]
Duration: [Start] - [End]
Supervisor: [Supervisor Name]
