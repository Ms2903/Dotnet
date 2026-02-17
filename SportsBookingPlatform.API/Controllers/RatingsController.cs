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

    // Specific endpoints as requested
    [HttpGet("venue/{id}")]
    public async Task<ActionResult<IEnumerable<RatingResponseDto>>> GetVenueRatings(Guid id)
    {
        return await GetRatings(id);
    }

    [HttpGet("court/{id}")]
    public async Task<ActionResult<IEnumerable<RatingResponseDto>>> GetCourtRatings(Guid id)
    {
        return await GetRatings(id);
    }

    [HttpGet("player/{id}")]
    public async Task<ActionResult<IEnumerable<RatingResponseDto>>> GetPlayerRatings(Guid id)
    {
        return await GetRatings(id);
    }

    // POST specific endpoints routing to Submit
    [HttpPost("venue")]
    [Authorize]
    public async Task<ActionResult<RatingResponseDto>> RateVenue(SubmitRatingRequestDto request)
    {
        // Force Type? Or trust request?
        // Requirement: POST /api/ratings/venue
        // DTO has TargetType.
        // Good practice to enforce it here.
        request.TargetType = "Venue";
        return await SubmitRating(request);
    }

    [HttpPost("court")]
    [Authorize]
    public async Task<ActionResult<RatingResponseDto>> RateCourt(SubmitRatingRequestDto request)
    {
        request.TargetType = "Court";
        return await SubmitRating(request);
    }

    [HttpPost("player")]
    [Authorize]
    public async Task<ActionResult<RatingResponseDto>> RatePlayer(SubmitRatingRequestDto request)
    {
        request.TargetType = "Player";
        return await SubmitRating(request);
    }
}
