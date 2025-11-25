# SummerSplash Web Application

A comprehensive ASP.NET Core MVC web application for managing SummerSplash pool service operations, employee scheduling, time tracking, and reporting.

## Overview

SummerSplash Web is an administrative dashboard that streamlines pool service management operations including employee management, scheduling, time tracking, service reports, and location management.

## Features

### User Management
- User authentication and authorization
- Role-based access control (Admin, Manager, Employee)
- User profile management
- Secure password hashing with BCrypt

### Time Tracking (Clock System)
- Employee clock in/out functionality
- Real-time time tracking
- Clock history and reports
- Employee attendance monitoring

### Scheduling
- Employee work schedule management
- Four-week schedule creation and editing
- Individual and team schedule views
- Schedule conflict detection

### Service Reports
- Service technician report creation and management
- Site evaluation forms
- Report details and history
- PDF export capabilities

### Location Management
- Job location tracking
- Location-based assignment
- Geographic data management

### Dashboard Analytics
- Real-time operational statistics
- Employee activity monitoring
- Service report summaries
- Performance metrics

## Technology Stack

- **Framework**: ASP.NET Core 8.0 (MVC)
- **Database**: Microsoft SQL Server
- **ORM**: Dapper (lightweight ORM)
- **Authentication**: BCrypt.Net-Next for password hashing
- **Frontend**: Bootstrap 5, jQuery
- **API**: RESTful API for mobile integration

## Project Structure

```
SummerSplashWeb/
├── Controllers/          # MVC Controllers
│   ├── Api/             # API Controllers for mobile
│   ├── AuthController   # Authentication
│   ├── DashboardController
│   ├── UsersController
│   ├── ScheduleController
│   ├── ClockController
│   ├── ReportsController
│   └── LocationsController
├── Models/              # Data models
├── Services/            # Business logic services
│   ├── AuthService
│   ├── DatabaseService
│   ├── UserService
│   └── EmailService
├── Views/               # Razor views
│   ├── Auth/
│   ├── Dashboard/
│   ├── Users/
│   ├── Schedule/
│   ├── Clock/
│   ├── Reports/
│   └── Shared/
└── wwwroot/            # Static files (CSS, JS, images)
```

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server (Express or full version)
- Git

## Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/kopi111/SummerSplashWeb.git
   cd SummerSplashWeb
   ```

2. **Configure Database Connection**

   Update `appsettings.json` with your SQL Server credentials:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=SummerSplashDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
     }
   }
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the Application**
   ```bash
   dotnet run
   ```

5. **Access the Application**

   Open your browser and navigate to:
   ```
   http://localhost:5177
   ```

## Default Login

After setting up the database, use the admin credentials that were created during setup. Refer to the database setup scripts for default admin user information.

## Configuration

### Database Setup

The application requires a SQL Server database. Database scripts and setup instructions can be found in the parent project folder.

### Email Service

Configure SMTP settings in `appsettings.json` for email notifications:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "FromEmail": "noreply@summersplash.com",
    "FromName": "SummerSplash Admin"
  }
}
```

## API Endpoints

The application includes a mobile API for integration with mobile applications:

- `POST /api/mobile/clockin` - Employee clock in
- `POST /api/mobile/clockout` - Employee clock out
- `GET /api/mobile/schedule/{userId}` - Get employee schedule
- `POST /api/mobile/report` - Submit service report

## Development

### Running in Development Mode

```bash
dotnet run --environment Development
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## Security Features

- Secure password storage with BCrypt hashing
- Session-based authentication
- Role-based authorization
- HTTPS redirection support
- SQL injection protection via parameterized queries

## License

Proprietary - All rights reserved

## Support

For issues, questions, or contributions, please contact the development team or create an issue in the GitHub repository.

## Version History

- **v1.0.0** - Initial release with core features
  - User management
  - Clock system
  - Scheduling
  - Service reports
  - Location management
  - Dashboard analytics

---

Built with ASP.NET Core MVC | Database: SQL Server | ORM: Dapper
