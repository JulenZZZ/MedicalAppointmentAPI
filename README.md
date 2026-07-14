# 🏥 AppointmentAPI - Medical Appointment Management System

![.NET](https://img.shields.io/badge/.NET%20Core%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC292B?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF%20Core%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-black?style=for-the-badge&logo=JSON%20web%20tokens)

`AppointmentAPI` is a robust and secure Web API built with **.NET Core** and **Entity Framework Core**, utilizing **SQL Server** as its database engine. The system implements a clean architecture and strict **Role-Based Access Control (RBAC)** to efficiently manage doctors, schedules (`TimeSlots`), appointment bookings (`Appointments`), and a simulated checkout flow (`Payments`).

---

## 🚀 Key Features

- **Authentication & Authorization**: Secure access powered by **JWT (JSON Web Tokens)** with strict user roles (`Admin` and `Patient`).
- **Role-Based Access Control (RBAC)**: 
  - Only **Admins** can register doctors and generate available time slots.
  - Both **Patients** and Admins can query free schedules by doctor and date.
  - Only **Patients** can book appointments and trigger the simulated payment flow.
- **Smart Schedule Management**: A single-table `TimeSlot` design that prevents overbooking or double-booking via atomic database transactions.
- **Simulated Payment Gateway**: Atomic transactional flow that creates an appointment and its corresponding `Payment` record simultaneously, supporting refund states upon cancellation.
- **Core Business Validations**: Duplication prevention for schedules on the database side and security checks preventing patients from canceling other users' appointments.

---

## 🛠️ Tech Stack

- **Framework:** .NET 8.0 Web API
- **ORM:** Entity Framework Core 8.0 (Code First)
- **Database:** Microsoft SQL Server
- **Authentication:** JWT Bearer Authentication
- **Password Security:** BCrypt.Net

---

## 📂 Entity Architecture

The relational model is designed to be highly scalable and decoupled:

* **User**: Manages credentials (hashed securely using BCrypt) and roles (`Admin` / `Patient`).
* **Doctor**: Represents the medical professionals within the system.
* **TimeSlot**: Defines specific 30-minute intervals (or custom durations) assigned to a doctor for a specific date.
* **Appointment**: The central entity linking the patient, the doctor, and the selected time slot.
* **Payment**: Financial model tracking transactions associated with bookings, keeping physical processing decoupled from core business logic.

---

## 🏁 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher installed.
- [SQL Server](https://www.microsoft.com/sql-server/) running locally or in the cloud.

### Configuration

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/your-username/AppointmentAPI.git](https://github.com/your-username/AppointmentAPI.git)
   cd AppointmentAPI
   Configure Connection String:
Open appsettings.json and set your connection string to point to your SQL Server instance:

JSON
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=AppointmentDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
Run Database Migrations:
Open your terminal in the project root directory and execute:

Bash
dotnet ef database update
(Note: This will automatically build the schema and seed a default administrator account: admin@appointment.com).

Run the Application:

Bash
dotnet run
The API will start running at https://localhost:7108 (or the port defined in your launchSettings.json).

🔑 Seed / Testing Accounts
Applying migrations seeds the database with a default master administrator account:

Email: admin@appointment.com

Password: [The seed password configured in your AppDbContext]

Role: Admin

📍 Key Endpoints
🔓 Authentication
POST /api/auth/register - Registers a new patient.

POST /api/auth/login - Signs in and returns a JWT Bearer Token.

🩺 Doctors
POST /api/doctors - Creates a doctor profile (Admin only).

GET /api/doctors/{doctorId}/available-slots?date=YYYY-MM-DD - Retrieves free slots for a doctor on a specific date (Any authenticated user).

📅 Time Slots
POST /api/timeslots - Generates a schedule slot for a doctor (Admin only).

💳 Appointments & Bookings
POST /api/appointments - Books a slot and processes a simulated payment (Patient only).

PUT /api/appointments/{id}/cancel - Cancels an appointment, releases the time slot, and flags a simulated refund (Patients on their own bookings, or Admins globally).
📄 License
This project is licensed under the MIT License. See the LICENSE file for details.
