using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly AppDbContext _context; // Needed for basic User info if not in Profile Service

    public UsersController(IUserProfileService userProfileService, AppDbContext context)
    {
        _userProfileService = userProfileService;
        _context = context;
    }

    [HttpGet("{id}/profile")]
    public async Task<ActionResult<UserProfileResponseDto>> GetUserProfile(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound("User not found.");

        var profile = await _userProfileService.GetProfileByUserIdAsync(id);
        
        // Deserialize PreferredSportsJson
        List<string> sports = new List<string>();
        if (profile != null && !string.IsNullOrEmpty(profile.PreferredSportsJson))
        {
            try 
            {
                sports = JsonSerializer.Deserialize<List<string>>(profile.PreferredSportsJson) ?? new List<string>();
            }
            catch {}
        }

        return new UserProfileResponseDto
        {
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email, // Maybe hide email for public profile? Requirement says "View player profile". Usually email is private. But for MVP...
            GamesPlayed = profile?.GamesPlayed ?? 0,
            AverageRating = profile?.AverageRating ?? 0,
            TotalRatings = profile?.TotalRatingsReceived ?? 0,
            PreferredSports = sports
        };
    }
}
