using AppointmentAPI.Controllers;
using AppointmentAPI.Data;
using AppointmentAPI.DTOs;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AppointmentAPI.Tests
{
    public class AuthControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
        private IConfiguration GetTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key", "ClaveSecretaSuperSeguraParaTestsUnitarios2026!" },
                { "Jwt:Issuer", "YourMedAPI" },
                { "Jwt:Audience", "YourMedClient" }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task Register_ShouldAssignPatientRoleByDefault_AndPersistUser()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context, GetTestConfiguration());

            var dto = new RegisterDto
            {
                Name = "Jane Doe",
                Email = "jane@yourmed.com",
                Password = "Password123!"
            };

            var result = await controller.Register(dto);

            Assert.IsType<OkObjectResult>(result);
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "jane@yourmed.com");
            Assert.NotNull(savedUser);
            Assert.Equal(Roles.Patient, savedUser!.Role);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            var context = GetInMemoryDbContext();
            context.Users.Add(new User
            {
                Name = "Existing User",
                Email = "duplicate@yourmed.com",
                Role = Roles.Patient,
                PasswordHash = "hash"
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context, GetTestConfiguration());
            var dto = new RegisterDto
            {
                Name = "Another User",
                Email = "duplicate@yourmed.com",
                Password = "Password123!"
            };

            var result = await controller.Register(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User already exists", badRequest.Value);
        }

        [Fact]
        public async Task Login_ShouldReturnJwtToken_WhenCredentialsAreValid()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context, GetTestConfiguration());

            await controller.Register(new RegisterDto
            {
                Name = "Valid Patient",
                Email = "patient@yourmed.com",
                Password = "Password123!"
            });

            var result = await controller.Login(new LoginDto
            {
                Email = "patient@yourmed.com",
                Password = "Password123!"
            });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.Contains("Token", okResult.Value!.ToString());
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenPasswordIsWrong()
        {
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context, GetTestConfiguration());

            await controller.Register(new RegisterDto
            {
                Name = "Valid Patient",
                Email = "patient@yourmed.com",
                Password = "Password123!"
            });

            var result = await controller.Login(new LoginDto
            {
                Email = "patient@yourmed.com",
                Password = "WrongPassword!"
            });

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid credentials", badRequest.Value);
        }
        [Fact]
        public void AuthController_ShouldNotExposePublicAdminResetEndpoint()
        {
            var resetMethod = typeof(AuthController).GetMethod("ResetAdminPassword");
            Assert.Null(resetMethod);
        }
        [Fact]
        public async Task Login_ValidCredentials_CompletesWithinAcceptableTime()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);
            await DbSeeder.SeedAsync(context); // Crea a sarah.jenkins@example.com

            // Configura tu AuthController con tu IConfiguration de prueba
            var inMemorySettings = new Dictionary<string, string?> {
        {"Jwt:Key", "ClaveSecretaDePruebaSuperLargaParaTestsUnitarios2026!"},
        {"Jwt:Issuer", "YourMedAPI"},
        {"Jwt:Audience", "YourMedClient"}
    };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var controller = new AuthController(context, configuration);
            var loginDto = new LoginDto
            {
                Email = "sarah.jenkins@example.com",
                Password = "Patient123!"
            };

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await controller.Login(loginDto);
            stopwatch.Stop();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.True(stopwatch.ElapsedMilliseconds < 1500,
                $"El login tardó demasiado: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}