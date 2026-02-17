using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpPost]
    [Authorize(Roles = "VenueOwner")]
    public async Task<ActionResult<VendorVenueResponseDto>> CreateVenue(CreateVenueRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null) 
            {
                return Unauthorized(new { message = "User ID claim (sub/nameid) not found in token." });
            }

            var ownerId = Guid.Parse(userIdClaim.Value);
            var venue = await _venueService.CreateVenueAsync(request, ownerId);
            return CreatedAtAction(nameof(GetVenues), new { id = venue.VenueId }, venue);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorVenueResponseDto>>> GetVenues()
    {
        var venues = await _venueService.GetAllVenuesAsync();
        return Ok(venues);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveVenue(Guid id)
    {
        try
        {
            await _venueService.ApproveVenueAsync(id);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
