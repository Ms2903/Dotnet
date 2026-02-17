using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api")] 
public class CourtsController : ControllerBase
{
    private readonly ICourtService _courtService;

    public CourtsController(ICourtService courtService)
    {
        _courtService = courtService;
    }

    [HttpPost("venues/{venueId}/courts")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<ActionResult<CourtResponseDto>> CreateCourt(Guid venueId, CreateCourtRequestDto request)
    {
        try
        {
            if (venueId != request.VenueId) return BadRequest("VenueId mismatch");
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var court = await _courtService.CreateCourtAsync(request, userId);
            
            return CreatedAtAction(nameof(GetCourt), new { id = court.CourtId }, court);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("courts/{id}")]
    public async Task<ActionResult<CourtResponseDto>> GetCourt(Guid id)
    {
        try
        {
            var court = await _courtService.GetCourtByIdAsync(id);
            return Ok(court);
        }
        catch (DomainException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    [HttpPut("courts/{id}")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<ActionResult<CourtResponseDto>> UpdateCourt(Guid id, UpdateCourtRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var court = await _courtService.UpdateCourtAsync(id, request, userId);
            return Ok(court);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("courts/{id}")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> DeleteCourt(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            await _courtService.DeleteCourtAsync(id, userId);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
