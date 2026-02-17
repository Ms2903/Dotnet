using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    [HttpGet("claims")]
    [Authorize]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new 
        { 
            Message = "You are authorized!", 
            User = User.Identity?.Name, 
            IsAuthenticated = User.Identity?.IsAuthenticated,
            HasVenueOwnerRole = User.IsInRole("VenueOwner"),
            Claims = claims 
        });
    }

    [HttpGet("public")]
    public IActionResult GetPublic()
    {
        return Ok(new { Message = "Public endpoint works" });
    }
}
