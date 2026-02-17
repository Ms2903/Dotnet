using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RatingResponseDto>> SubmitRating(SubmitRatingRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var rating = await _ratingService.SubmitRatingAsync(request, userId);
            return Ok(rating);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("target/{targetId}")]
    public async Task<ActionResult<IEnumerable<RatingResponseDto>>> GetRatings(Guid targetId)
    {
        var ratings = await _ratingService.GetRatingsByTargetAsync(targetId);
        return Ok(ratings);
    }
}
