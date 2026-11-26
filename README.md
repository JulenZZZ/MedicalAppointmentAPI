# 🏥 YourMed (`AppointmentAPI`) — Medical Appointment Management System

![.NET](https://img.shields.io/badge/.NET%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/Azure%20SQL%20/%20SQL%20Server-CC292B?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Entity Framework](https://img.shields.io/badge/EF%20Core%208.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-black?style=for-the-badge&logo=JSON%20web%20tokens)

`AppointmentAPI` is a secure, cloud-ready RESTful Web API built with **.NET** and **Entity Framework Core**, utilizing **Azure SQL / SQL Server** as its database engine. It powers the **YourMed** platform with strict **Role-Based Access Control (RBAC)**, atomic transactional workflows, self-healing schedule seeding, and automated unit testing.

🔗 **Live Frontend Demo:** [yourmed-phi.vercel.app](https://yourmed-phi.vercel.app)  
☁️ **Cloud Hosting:** Azure App Service & Azure SQL Database

---

## 🚀 Key Features (v1.0)

### ✅ Core & Cloud Engineering Highlights
- **Authentication & Role-Based Access Control (RBAC):** Secure access powered by **JWT Bearer Tokens** and `ASP.NET Core Identity` password hashing (`PasswordHasher<User>`), enforcing strict separation between `Patient` and `Admin` roles.
- **Atomic Cancellation & Azure SQL Resiliency:** Cancelling an appointment (`PUT /api/appointments/{id}/cancel`) updates the booking status (`Cancelled`), flags a simulated payment refund (`Refunded`), and releases the underlying time slot (`IsAvailable = true`) inside an explicit database transaction wrapped in `CreateExecutionStrategy()` to ensure compatibility with Azure SQL transient fault retries (`EnableRetryOnFailure`).
- **Self-Healing Rolling Agenda (`DbSeeder`):** Automatically runs on application startup to deactivate expired free time slots (without breaking past appointment history) and generate a rolling 4-day future schedule for active doctors so the live demo never runs out of availability.
- **Real-Time Expired Slot Filtering:** Query endpoints filter out past dates and elapsed hours on the current day, returning cleanly formatted `TimeSlotDto` payloads.
- **Simulated Payment Gateway:** Atomic transactional flow that books a slot and records its corresponding `Payment` entity simultaneously, preventing double-booking.
- **Cold-Start Pre-Warming Endpoint:** Exposes a lightweight public health-check root endpoint (`GET /`) with configured CORS policies (`AllowFrontend`) so the client app can wake up the Azure free-tier server silently in the background.

### 🔮 Upcoming Roadmap (v2.0)
- **Stripe Sandbox Webhooks:** Replace simulated payment records with real Stripe Checkout session verification.
- **Dedicated Practitioner Endpoints:** Granular endpoints for doctors to manage their own availability and consultation notes.
- **Automated Email & `.ics` Invites:** Transactional email confirmations upon booking and cancellation.

---

## 🛠️ Tech Stack

- **Framework:** .NET 8.0 Web API
- **ORM:** Entity Framework Core 8.0 (Code First + Execution Strategies)
- **Database:** Azure SQL Database / Microsoft SQL Server
- **Authentication & Security:** JWT Bearer Authentication, `PasswordHasher<User>`, CORS Policies
- **Testing & CI:** xUnit & EF Core InMemory automated via GitHub Actions
- **Cloud Deployment:** Microsoft Azure App Service

---

## 📂 Entity Architecture

The relational model is designed to keep scheduling, clinical bookings, and financial records decoupled:

- **User:** Manages user credentials (salted and hashed via `PasswordHasher<User>`) and roles (`Admin` / `Patient`).
- **Doctor:** Represents medical specialists, their specializations, and active status (`IsActive`).
- **TimeSlot:** Defines specific 30-minute intervals assigned to a doctor on a given date, preventing overbooking through atomic availability flags.
- **Appointment:** Central entity linking the patient, doctor, and selected time slot, tracking lifecycle statuses (`Scheduled`, `Completed`, `Cancelled`).
- **Payment:** Financial record tracking transaction amounts, payment methods, and statuses (`Paid`, `Pending`, `Refunded`).

---

## 🏁 Getting Started Locally

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher installed.
- [SQL Server](https://www.microsoft.com/sql-server/) or SQL Server LocalDB running locally.

### Configuration & Run

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/your-username/AppointmentAPI.git](https://github.com/your-username/AppointmentAPI.git)
   cd AppointmentAPI
   ```
   2. **Configure Connection String & JWT:**
   Open `appsettings.json` (or configure User Secrets) with your SQL Server connection string and JWT settings:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=AppointmentDB;Trusted_Connection=True;TrustServerCertificate=True;"
   },
   "Jwt": {
     "Key": "YourSuperSecretLongSigningKeyForLocalDevelopment2026!",
     "Issuer": "AppointmentAPI",
     "Audience": "AppointmentClient"
   }
   ```

3. **Apply Database Migrations:**
   ```bash
   dotnet ef database update
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```
   The API will start at `https://localhost:7108` and automatically execute `DbSeeder.SeedAsync()` to populate demo doctors, patients, and rolling future time slots.

### Running Automated Tests
```bash
dotnet test
```
The xUnit test suite validates authentication performance, JWT generation, expired slot filtering, and rolling database seeding using EF Core InMemory.

---

## 🔑 Seed / Demo Accounts

On initial startup, `DbSeeder` populates test patients (used by the frontend's **Demo Patient Access** button) alongside active medical specialists:

- **Demo Patient Email:** `sarah.jenkins@example.com`
- **Password:** `Patient123!`
- **Role:** `Patient`

---

## 📍 Key Endpoints

### 🩺 Health Check
- `GET /` — Public root endpoint returning `"Appointment API running!"` for server health checks and cold-start pre-warming.

### 🔓 Authentication
- `POST /api/auth/register` — Registers a new patient account.
- `POST /api/auth/login` — Authenticates credentials and returns a JWT Bearer Token.

### 👨‍⚕️ Doctors & Availability
- `GET /api/doctors` — Retrieves the directory of doctors.
- `POST /api/doctors` — Creates a new doctor profile (`Admin` only).
- `GET /api/doctors/available-timeslots` — Retrieves all upcoming available time slots across active doctors (supports optional specialty/date filtering).
- `GET /api/doctors/{doctorId}/available-timeslots` — Retrieves upcoming free slots for a specific doctor, excluding past dates and elapsed hours.

### 📅 Time Slots
- `POST /api/timeslots` — Generates custom availability slots for a doctor (`Admin` only).

### 💳 Appointments & Bookings
- `GET /api/appointments/my-appointments` — Retrieves the authenticated patient's booking history and payment statuses (`Patient` only).
- `POST /api/appointments` — Books an available slot and processes a simulated payment atomically (`Patient` only).
- `PUT /api/appointments/{id}/cancel` — Cancels an appointment, releases the time slot, and flags a simulated refund inside a resilient transaction (`Patient` on own bookings, or `Admin` globally).

---

## 📄 License

This project is open-source and available under the [MIT License](LICENSE). See the `LICENSE` file for details.
