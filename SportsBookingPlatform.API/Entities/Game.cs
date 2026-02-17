using System.ComponentModel.DataAnnotations;

namespace SportsBookingPlatform.API.Entities;

public enum GameStatus
{
    Open,
    Full,
    Cancelled,
    Completed
}

public class Game
{
    [Key]
    public Guid GameId { get; set; } = Guid.NewGuid();

    public Guid SlotId { get; set; }
    public Slot? Slot { get; set; }

    public Guid GameOwnerId { get; set; }
    public User? GameOwner { get; set; }

    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }

    public bool IsPrivate { get; set; } = false;

    public GameStatus Status { get; set; } = GameStatus.Open;

    public ICollection<GameParticipant> Participants { get; set; } = new List<GameParticipant>();
    public ICollection<GameWaitlist> Waitlist { get; set; } = new List<GameWaitlist>();
}

public class GameParticipant
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GameId { get; set; }
    public Game? Game { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}

public class GameWaitlist
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GameId { get; set; }
    public Game? Game { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool IsNotified { get; set; } = false;
}
