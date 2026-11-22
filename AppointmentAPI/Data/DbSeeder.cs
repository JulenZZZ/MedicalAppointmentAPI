using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppointmentAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var today = DateTime.UtcNow.Date;

            var shiftHours = new[]
            {
                (new TimeSpan(9, 0, 0), new TimeSpan(9, 30, 0)),
                (new TimeSpan(9, 30, 0), new TimeSpan(10, 0, 0)),
                (new TimeSpan(10, 30, 0), new TimeSpan(11, 0, 0)),
                (new TimeSpan(14, 0, 0), new TimeSpan(14, 30, 0)),
                (new TimeSpan(15, 0, 0), new TimeSpan(15, 30, 0)),
                (new TimeSpan(16, 0, 0), new TimeSpan(16, 30, 0))
            };

            // 1. CREACIÓN INICIAL (Solo se ejecuta si aún no hay doctores en la BD)
            if (!await context.Doctors.AnyAsync())
            {
                var passwordHasher = new PasswordHasher<User>();

                // Pacientes de prueba (Contraseña para todos: Patient123!)
                var patients = new List<User>
                {
                    new User
                    {
                        Name = "Sarah Jenkins",
                        Email = "sarah.jenkins@example.com",
                        Role = Roles.Patient,
                        CreatedAt = today.AddDays(-30),
                        PasswordHash = string.Empty
                    },
                    new User
                    {
                        Name = "David Miller",
                        Email = "david.miller@example.com",
                        Role = Roles.Patient,
                        CreatedAt = today.AddDays(-20),
                        PasswordHash = string.Empty
                    },
                    new User
                    {
                        Name = "Elena Rostova",
                        Email = "elena.rostova@example.com",
                        Role = Roles.Patient,
                        CreatedAt = today.AddDays(-10),
                        PasswordHash = string.Empty
                    }
                };

                foreach (var patient in patients)
                {
                    patient.PasswordHash = passwordHasher.HashPassword(patient, "Patient123!");
                }

                context.Users.AddRange(patients);

                // Doctores en inglés
                var doctors = new List<Doctor>
                {
                    new Doctor
                    {
                        Name = "Dr. Emily Carter",
                        Specialization = "Cardiology",
                        Email = "emily.carter@yourmed.com",
                        IsActive = true
                    },
                    new Doctor
                    {
                        Name = "Dr. Marcus Vance",
                        Specialization = "Dermatology",
                        Email = "marcus.vance@yourmed.com",
                        IsActive = true
                    },
                    new Doctor
                    {
                        Name = "Dr. Sophia Bennett",
                        Specialization = "Pediatrics",
                        Email = "sophia.bennett@yourmed.com",
                        IsActive = true
                    },
                    new Doctor
                    {
                        Name = "Dr. Liam O'Connor",
                        Specialization = "Orthopedics",
                        Email = "liam.oconnor@yourmed.com",
                        IsActive = true
                    },
                    new Doctor
                    {
                        Name = "Dr. Hannah Lin",
                        Specialization = "Neurology",
                        Email = "hannah.lin@yourmed.com",
                        IsActive = true
                    },
                    new Doctor
                    {
                        Name = "Dr. Robert Hayes",
                        Specialization = "General Medicine",
                        Email = "robert.hayes@yourmed.com",
                        IsActive = false
                    }
                };

                context.Doctors.AddRange(doctors);
                await context.SaveChangesAsync();

                // Horarios pasados y futuros iniciales
                var initialTimeSlots = new List<TimeSlot>();

                var pastSlot1 = new TimeSlot
                {
                    DoctorId = doctors[0].Id,
                    Date = today.AddDays(-5),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(9, 30, 0),
                    IsAvailable = false
                };
                var pastSlot2 = new TimeSlot
                {
                    DoctorId = doctors[1].Id,
                    Date = today.AddDays(-2),
                    StartTime = new TimeSpan(11, 0, 0),
                    EndTime = new TimeSpan(11, 30, 0),
                    IsAvailable = false
                };
                initialTimeSlots.AddRange(new[] { pastSlot1, pastSlot2 });

                for (int docIndex = 0; docIndex < 5; docIndex++)
                {
                    for (int dayOffset = 1; dayOffset <= 4; dayOffset++)
                    {
                        foreach (var (start, end) in shiftHours)
                        {
                            initialTimeSlots.Add(new TimeSlot
                            {
                                DoctorId = doctors[docIndex].Id,
                                Date = today.AddDays(dayOffset),
                                StartTime = start,
                                EndTime = end,
                                IsAvailable = true
                            });
                        }
                    }
                }

                context.TimeSlots.AddRange(initialTimeSlots);
                await context.SaveChangesAsync();

                // Selección de bloques futuros para reservar citas iniciales
                var futureSlotScheduled1 = initialTimeSlots.First(ts => ts.DoctorId == doctors[0].Id && ts.Date == today.AddDays(1) && ts.StartTime == new TimeSpan(9, 0, 0));
                var futureSlotScheduled2 = initialTimeSlots.First(ts => ts.DoctorId == doctors[2].Id && ts.Date == today.AddDays(1) && ts.StartTime == new TimeSpan(10, 30, 0));
                var futureSlotPending = initialTimeSlots.First(ts => ts.DoctorId == doctors[3].Id && ts.Date == today.AddDays(2) && ts.StartTime == new TimeSpan(14, 0, 0));
                var futureSlotCancelled = initialTimeSlots.First(ts => ts.DoctorId == doctors[4].Id && ts.Date == today.AddDays(2) && ts.StartTime == new TimeSpan(15, 0, 0));

                futureSlotScheduled1.IsAvailable = false;
                futureSlotScheduled2.IsAvailable = false;
                futureSlotPending.IsAvailable = false;
                futureSlotCancelled.IsAvailable = true;

                // Citas iniciales
                var appointments = new List<Appointment>
                {
                    new Appointment
                    {
                        TimeSlotId = pastSlot1.Id,
                        DoctorId = doctors[0].Id,
                        PatientId = patients[0].Id,
                        CreatedAt = today.AddDays(-8),
                        Status = "Completed",
                        PaymentStatus = "Paid"
                    },
                    new Appointment
                    {
                        TimeSlotId = pastSlot2.Id,
                        DoctorId = doctors[1].Id,
                        PatientId = patients[1].Id,
                        CreatedAt = today.AddDays(-5),
                        Status = "Completed",
                        PaymentStatus = "Paid"
                    },
                    new Appointment
                    {
                        TimeSlotId = futureSlotScheduled1.Id,
                        DoctorId = doctors[0].Id,
                        PatientId = patients[0].Id,
                        CreatedAt = today.AddHours(-12),
                        Status = "Scheduled",
                        PaymentStatus = "Paid"
                    },
                    new Appointment
                    {
                        TimeSlotId = futureSlotScheduled2.Id,
                        DoctorId = doctors[2].Id,
                        PatientId = patients[2].Id,
                        CreatedAt = today.AddHours(-6),
                        Status = "Scheduled",
                        PaymentStatus = "Paid"
                    },
                    new Appointment
                    {
                        TimeSlotId = futureSlotPending.Id,
                        DoctorId = doctors[3].Id,
                        PatientId = patients[1].Id,
                        CreatedAt = today.AddHours(-2),
                        Status = "Scheduled",
                        PaymentStatus = "Pending"
                    },
                    new Appointment
                    {
                        TimeSlotId = futureSlotCancelled.Id,
                        DoctorId = doctors[4].Id,
                        PatientId = patients[2].Id,
                        CreatedAt = today.AddDays(-1),
                        Status = "Cancelled",
                        PaymentStatus = "Refunded"
                    }
                };

                context.Appointments.AddRange(appointments);
                await context.SaveChangesAsync();

                // Pagos iniciales
                var payments = new List<Payment>
                {
                    new Payment
                    {
                        AppointmentId = appointments[0].Id,
                        Amount = 120.00m,
                        Status = "Paid",
                        PaymentMethod = "Credit Card",
                        CreatedAt = appointments[0].CreatedAt,
                        PaidAt = appointments[0].CreatedAt.AddMinutes(2)
                    },
                    new Payment
                    {
                        AppointmentId = appointments[1].Id,
                        Amount = 95.00m,
                        Status = "Paid",
                        PaymentMethod = "Simulated",
                        CreatedAt = appointments[1].CreatedAt,
                        PaidAt = appointments[1].CreatedAt.AddMinutes(1)
                    },
                    new Payment
                    {
                        AppointmentId = appointments[2].Id,
                        Amount = 120.00m,
                        Status = "Paid",
                        PaymentMethod = "Credit Card",
                        CreatedAt = appointments[2].CreatedAt,
                        PaidAt = appointments[2].CreatedAt.AddMinutes(3)
                    },
                    new Payment
                    {
                        AppointmentId = appointments[3].Id,
                        Amount = 85.00m,
                        Status = "Paid",
                        PaymentMethod = "Debit Card",
                        CreatedAt = appointments[3].CreatedAt,
                        PaidAt = appointments[3].CreatedAt.AddMinutes(1)
                    },
                    new Payment
                    {
                        AppointmentId = appointments[4].Id,
                        Amount = 150.00m,
                        Status = "Pending",
                        PaymentMethod = "Simulated",
                        CreatedAt = appointments[4].CreatedAt,
                        PaidAt = null
                    },
                    new Payment
                    {
                        AppointmentId = appointments[5].Id,
                        Amount = 135.00m,
                        Status = "Refunded",
                        PaymentMethod = "Credit Card",
                        CreatedAt = appointments[5].CreatedAt,
                        PaidAt = appointments[5].CreatedAt.AddMinutes(2)
                    }
                };

                context.Payments.AddRange(payments);
                await context.SaveChangesAsync();
            }

            // 2. AGENDA RODANTE (Rellena horarios futuros para los próximos 4 días si no existen)
            var activeDoctors = await context.Doctors.Where(d => d.IsActive).ToListAsync();
            var newSlots = new List<TimeSlot>();

            foreach (var doctor in activeDoctors)
            {
                for (int dayOffset = 1; dayOffset <= 4; dayOffset++)
                {
                    var targetDate = today.AddDays(dayOffset);
                    bool hasSlotsForDay = await context.TimeSlots
                        .AnyAsync(ts => ts.DoctorId == doctor.Id && ts.Date == targetDate);

                    if (!hasSlotsForDay)
                    {
                        foreach (var (start, end) in shiftHours)
                        {
                            newSlots.Add(new TimeSlot
                            {
                                DoctorId = doctor.Id,
                                Date = targetDate,
                                StartTime = start,
                                EndTime = end,
                                IsAvailable = true
                            });
                        }
                    }
                }
            }

            if (newSlots.Any())
            {
                context.TimeSlots.AddRange(newSlots);
                await context.SaveChangesAsync();
            }
        }
    }
}