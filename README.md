# HospitalityHub API

A production-quality hotel management system REST API built with ASP.NET Core, C#, JWT Authentication, Entity Framework Core, and SQL Server.

## Features

- JWT Authentication with role-based access control (Admin, Receptionist, Guest)
- Full CRUD for Rooms, Guests and Bookings
- Double-booking prevention with date overlap detection
- Auto-calculation of total booking amount based on nights and room price
- Room status management — Available, Occupied, Maintenance
- Check-in / Check-out workflow with automatic room status updates
- Dashboard reporting — occupancy rate, revenue, daily activity
- Swagger UI documentation for all endpoints
- CORS configured for Angular frontend integration

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| Language | C# |
| Database | SQL Server with Entity Framework Core |
| Authentication | JWT Bearer Tokens |
| Documentation | Swagger / Swashbuckle |
| Architecture | Controller → Service → Repository pattern |

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022

### Setup

1. Clone the repository

git clone https://github.com/OdwaDynty/HospitalityHub-API.git

2. Update the connection string in `appsettings.json`
```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HospitalityHubDB;Trusted_Connection=True;TrustServerCertificate=True"
```

3. Run database migrations

Update-Database

4. Run the project

dotnet run

5. Open Swagger at `http://localhost:5058/swagger`

## API Endpoints

### Authentication
| Method | Endpoint | Description | Access |
|---|---|---|---|
| POST | /api/auth/register | Register new user | Public |
| POST | /api/auth/login | Login and get JWT token | Public |

### Rooms
| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | /api/rooms | Get all rooms with filters | Public |
| GET | /api/rooms/{id} | Get room by ID | Public |
| POST | /api/rooms | Create new room | Admin |
| PUT | /api/rooms/{id} | Update room | Admin |
| DELETE | /api/rooms/{id} | Delete room | Admin |

### Guests
| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | /api/guests | Get all guests with search | Admin, Receptionist |
| GET | /api/guests/{id} | Get guest with booking history | Admin, Receptionist |
| POST | /api/guests | Create new guest | Admin, Receptionist |
| PUT | /api/guests/{id} | Update guest | Admin, Receptionist |
| DELETE | /api/guests/{id} | Delete guest | Admin |

### Bookings
| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | /api/bookings | Get all bookings with filters | Admin, Receptionist |
| GET | /api/bookings/{id} | Get booking by ID | Admin, Receptionist |
| POST | /api/bookings | Create booking | Admin, Receptionist |
| PUT | /api/bookings/{id}/status | Update booking status | Admin, Receptionist |
| DELETE | /api/bookings/{id} | Cancel booking | Admin |

### Dashboard
| Method | Endpoint | Description | Access |
|---|---|---|---|
| GET | /api/dashboard/summary | Occupancy and revenue summary | Admin, Receptionist |
| GET | /api/dashboard/today | Today's check-ins and check-outs | Admin, Receptionist |
| GET | /api/dashboard/revenue | Revenue by room type and month | Admin, Receptionist |

## Frontend

The Angular frontend for this project is available at:
https://github.com/OdwaDynty/HospitalityHub-Client

## Author

Odwa Dyantyi — [github.com/OdwaDynty](https://github.com/OdwaDynty)