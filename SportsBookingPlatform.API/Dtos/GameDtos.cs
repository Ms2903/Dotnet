using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Dtos;

public class CreateGameRequestDto
{
    [Required]
    public Guid SlotId { get; set; }

    [Required]
    [Range(1, 100)] // Reasonable limits
    public int MinPlayers { get; set; }

    [Required]
    [Range(1, 100)]
    public int MaxPlayers { get; set; }

    public bool IsPrivate { get; set; }
}

public class GameResponseDto
{
    public Guid GameId { get; set; }
    public Guid SlotId { get; set; }
    public Guid GameOwnerId { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
    public bool IsPrivate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class GameSearchRequestDto
{
    public DateTime? Date { get; set; }
    public Guid? VenueId { get; set; }
}
