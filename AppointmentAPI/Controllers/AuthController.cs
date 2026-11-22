using AppointmentAPI.Data;
using AppointmentAPI.DTOs;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AppointmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var userExists = await _context.Users
                .AnyAsync(x => x.Email == request.Email);

            if (userExists)
                return BadRequest("User already exists");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = Roles.Patient,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = string.Empty // Satisface el miembro 'required' sin gastar CPU
            };

            // Genera el hash real una sola vez
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User registered successfully",
                user.Id,
                user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return BadRequest("Invalid credentials");
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return BadRequest("Invalid credentials");
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                Message = "Login successful",
                Token = token,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetPerfil()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                Message = "Acceso autorizado al perfil",
                Id = userId,
                Email = userEmail,
                Role = userRole
            });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(token);

            return tokenHandler.WriteToken(securityToken);
        }
    }
}