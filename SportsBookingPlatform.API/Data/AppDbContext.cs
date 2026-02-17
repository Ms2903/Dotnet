using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Entities;

namespace SportsBookingPlatform.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Venue> Venues { get; set; }
    public DbSet<Court> Courts { get; set; }
    public DbSet<Slot> Slots { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameParticipant> GameParticipants { get; set; }
    public DbSet<GameWaitlist> GameWaitlists { get; set; }
    // public DbSet<Waitlist> Waitlists { get; set; } // Removed
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Rating> Ratings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Always call base

        // Configure relationships and constraints here if needed

        // Example: User Email Unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        // One-to-One User <-> Profile
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);

        // One-to-One User <-> Wallet
        modelBuilder.Entity<User>()
            .HasOne(u => u.Wallet)
            .WithOne(w => w.User)
            .HasForeignKey<Wallet>(w => w.UserId);

        // Precision for decimals
        modelBuilder.Entity<Court>()
            .Property(c => c.BasePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Slot>()
            .Property(s => s.BasePrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.LockedPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Wallet>()
            .Property(w => w.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<WalletTransaction>()
            .Property(w => w.Amount)
            .HasPrecision(18, 2);
            
        modelBuilder.Entity<Discount>()
            .Property(d => d.PercentOff)
            .HasPrecision(18, 2);
    }
}
