using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;

    public SlotsController(ISlotService slotService)
    {
        _slotService = slotService;
    }

    [HttpPost("generate")]
    [Authorize(Roles = "VenueOwner")]
    public async Task<IActionResult> GenerateSlots(GenerateSlotsRequestDto request)
    {
        try
        {
            await _slotService.GenerateSlotsAsync(request);
            return Ok(new { message = "Slots generated successfully." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<SlotDto>>> GetAvailableSlots([FromQuery] SlotSearchRequestDto request)
    {
        if (request.VenueId != Guid.Empty)
        {
            _slotService.RecordVenueSearch(request.VenueId);
        }
        
        var slots = await _slotService.GetAvailableSlotsAsync(request);
        return Ok(slots);
    }
}
