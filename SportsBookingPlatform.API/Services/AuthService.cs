using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;

namespace SportsBookingPlatform.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Check if email exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new DomainException("Email is already registered.");
        }

        if (request.Role == UserRole.Admin)
        {
            throw new DomainException("Registration as Admin is not allowed.");
        }

        // 2. Hash password
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Create User
        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Name = request.Name,
            Role = request.Role
        };

        // 4. Create User Profile (Empty for now)
        var profile = new UserProfile
        {
            UserId = user.Id
        };
        
        // 5. Create User Wallet (Empty for now)
        var wallet = new Wallet
        {
            UserId = user.Id
        };

        _context.Users.Add(user);
        _context.UserProfiles.Add(profile);
        _context.Wallets.Add(wallet);
        
        await _context.SaveChangesAsync();

        // 6. Generate Token - REMOVED for Register
        // var token = GenerateJwtToken(user);

        return new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            Name = user.Name,
            Message = "Registration successful. Please login to get access token."
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        // 1. Find User
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
        {
            throw new DomainException("Invalid email or password.");
        }

        // 2. Verify Password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new DomainException("Invalid email or password.");
        }

        // 3. Generate Token
        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            Name = user.Name
        };
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        
        // Fallback for development if config is missing (though should be enforced)
        if (string.IsNullOrEmpty(secretKey))
        {
             // Usually throw or have a default default only for dev
             throw new Exception("JwtSettings:SecretKey is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("Name", user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
