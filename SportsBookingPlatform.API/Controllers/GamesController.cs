using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<GameResponseDto>> CreateGame(CreateGameRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var game = await _gameService.CreateGameAsync(request, userId);
            
            return CreatedAtAction(nameof(GetGames), new { id = game.GameId }, game);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameResponseDto>>> GetGames([FromQuery] GameSearchRequestDto request)
    {
        var games = await _gameService.GetGamesAsync(request);
        return Ok(games);
    }

    [HttpPut("{id}/join")]
    [Authorize]
    public async Task<IActionResult> JoinGame(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            await _gameService.JoinGameAsync(id, userId);
            return Ok(new { message = "Joined game successfully." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPut("{id}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveGame(Guid id)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            await _gameService.LeaveGameAsync(id, userId);
            return Ok(new { message = "Left game successfully." });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
