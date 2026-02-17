using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("lock-slot")]
    [Authorize(Roles = "User,VenueOwner,Admin")] // Users should be able to lock
    public async Task<ActionResult<LockSlotResponseDto>> LockSlot(LockSlotRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _bookingService.LockSlotAsync(request, userId);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("confirm")]
    [Authorize]
    public async Task<ActionResult<BookingResponseDto>> ConfirmBooking(ConfirmBookingRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _bookingService.ConfirmBookingAsync(request, userId);
            return Ok(response);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetMyBookings()
    {
         var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
         if (userIdClaim == null) return Unauthorized();

         var userId = Guid.Parse(userIdClaim.Value);
         var bookings = await _bookingService.GetUserBookingsAsync(userId);
         return Ok(bookings);
    }

    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            await _bookingService.CancelBookingAsync(id, userId);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
