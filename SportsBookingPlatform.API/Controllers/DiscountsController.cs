using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountsController(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    [HttpPost]
    [Authorize(Roles = "VenueOwner")]
    public async Task<ActionResult<DiscountResponseDto>> CreateDiscount(CreateDiscountRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null) return Unauthorized();

            var ownerId = Guid.Parse(userIdClaim.Value);
            var discount = await _discountService.CreateDiscountAsync(request, request.VenueId, ownerId);
            
            return CreatedAtAction(nameof(GetDiscounts), new { venueId = request.VenueId }, discount);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DiscountResponseDto>>> GetDiscounts([FromQuery] Guid venueId)
    {
        try 
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            var userId = userIdClaim != null ? Guid.Parse(userIdClaim.Value) : Guid.Empty;

            var discounts = await _discountService.GetDiscountsForVenueAsync(venueId, userId);
            return Ok(discounts);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
