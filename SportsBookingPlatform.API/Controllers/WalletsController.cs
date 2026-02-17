using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Services;

namespace SportsBookingPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("balance")]
    [Authorize]
    public async Task<ActionResult<WalletResponseDto>> GetBalance()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var wallet = await _walletService.GetWalletByUserIdAsync(userId);
            return Ok(wallet);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("add-funds")]
    [Authorize]
    public async Task<ActionResult<WalletResponseDto>> AddFunds(AddFundsRequestDto request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var wallet = await _walletService.AddFundsAsync(userId, request);
            return Ok(wallet);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpGet("transactions")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<WalletTransactionResponseDto>>> GetTransactions()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            var userId = Guid.Parse(userIdClaim.Value);
            var transactions = await _walletService.GetTransactionsAsync(userId);
            return Ok(transactions);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
