using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using SportsBookingPlatform.API.Data;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.Services;

public class BookingService : IBookingService
{
    private readonly ISlotRepository _slotRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly AppDbContext _context;
    private readonly IDiscountService _discountService;
    private readonly ISlotService _slotService; // Added

    public BookingService(ISlotRepository slotRepository, IMemoryCache memoryCache, AppDbContext context, IDiscountService discountService, ISlotService slotService)
    {
        _slotRepository = slotRepository;
        _memoryCache = memoryCache;
        _context = context;
        _discountService = discountService;
        _slotService = slotService;
    }

    public async Task<LockSlotResponseDto> LockSlotAsync(LockSlotRequestDto request, Guid userId)
    {
        // 1. Get Slot
        var slot = await _slotRepository.GetSlotByIdAsync(request.SlotId);
        if (slot == null) throw new DomainException("Slot not found.");
        if (slot.Status != SlotStatus.Available) throw new DomainException("Slot is not available.");

        // Explicitly load Court if missing (Robustness)
        if (slot.Court == null)
        {
            await _context.Entry(slot).Reference(s => s.Court).LoadAsync();
        }

        // 2. Check if already locked in MemoryCache
        string cacheKey = $"SlotLock:{request.SlotId}";
        if (_memoryCache.TryGetValue(cacheKey, out _))
        {
            throw new DomainException("Slot is arguably locked by another user (concurrency check)."); 
        }

        // 3. Calculate Final Price
        // Use the centralized dynamic pricing logic from SlotService
        decimal dynamicPrice = await _slotService.CalculateDynamicPrice(slot);

        // 4. Create Lock Object
        var lockData = new SlotLockData
        {
            SlotId = slot.SlotId,
            UserId = userId,
            LockedPrice = dynamicPrice,
            ExpiryTime = DateTime.UtcNow.AddMinutes(5)
        };

        // 5. Store in MemoryCache
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = lockData.ExpiryTime,
            Priority = CacheItemPriority.High
        };

        _memoryCache.Set(cacheKey, lockData, cacheOptions);

        return new LockSlotResponseDto
        {
            SlotId = slot.SlotId,
            LockedPrice = dynamicPrice,
            ExpiryTime = lockData.ExpiryTime
        };
    }

    public async Task<BookingResponseDto> ConfirmBookingAsync(ConfirmBookingRequestDto request, Guid userId)
    {
        // 1. Validate Lock
        string cacheKey = $"SlotLock:{request.SlotId}";
        if (!_memoryCache.TryGetValue(cacheKey, out SlotLockData? lockData) || lockData == null)
        {
            throw new DomainException("Slot lock expired or not found. Please lock the slot again.");
        }

        // Variable lockData is now known to be not null here by flow analysis, but to be safe/explicit:
        var validLockData = lockData!;

        if (validLockData.UserId != userId)
        {
            throw new DomainException("This slot is locked by another user.");
        }

        // 2. Start Transaction
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 3. Re-verify Slot Availability (DB check)
            var slot = await _slotRepository.GetSlotByIdAsync(request.SlotId);
            if (slot == null || slot.Status != SlotStatus.Available)
            {
                throw new DomainException("Slot is no longer available.");
            }

            // 4. Debit Wallet (User)
            var walletService = new WalletService(_context); 
            // Ideally inject IWalletService, but manual instantiation preserves context for transaction within this scope logic if not using UnitOfWork.
            
            bool debitSuccess = await walletService.DebitFundsAsync(userId, validLockData.LockedPrice, $"Booking:{request.SlotId}");
            if (!debitSuccess)
            {
                throw new DomainException("Insufficient wallet balance.");
            }

            // 5. Credit Venue Owner
            // specific fetching of venue owner
            if (slot.Court == null) 
            {
                 // Should be loaded by GetSlotByIdAsync
                 await _context.Entry(slot).Reference(s => s.Court).LoadAsync();
            }
            
            if (slot.Court != null)
            {
                // We need Venue to get OwnerId
                var venue = await _context.Venues.FindAsync(slot.Court.VenueId);
                if (venue != null)
                {
                    // Credit Owner
                    await walletService.AddFundsAsync(venue.OwnerId, new AddFundsRequestDto
                    {
                        Amount = validLockData.LockedPrice,
                        ReferenceId = $"BookingCredit:{request.SlotId}"
                    });
                }
            }

            // 6. Update Slot Status
            slot.Status = SlotStatus.Booked;
            await _slotRepository.UpdateSlotAsync(slot);

            // 7. Create Booking
            var booking = new Booking
            {
                UserId = userId,
                SlotId = request.SlotId,
                LockedPrice = validLockData.LockedPrice,
                Status = BookingStatus.Confirmed,
                LockExpiryTime = validLockData.ExpiryTime 
            };
            
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // 7. Remove Lock
            _memoryCache.Remove(cacheKey);

            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                SlotId = booking.SlotId,
                UserId = booking.UserId,
                Price = booking.LockedPrice,
                Status = booking.Status.ToString(),
                CreatedAt = booking.CreatedAt
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task CancelBookingAsync(Guid bookingId, Guid userId)
    {
         var booking =await _context.Bookings.Include(b => b.Slot).FirstOrDefaultAsync(b => b.BookingId == bookingId);
         if (booking == null) throw new DomainException("Booking not found.");
         
         if (booking.UserId != userId && userId != Guid.Empty) // Admin specific logic might pass empty/admin ID, but here strict.
         {
             // If Admin needs to cancel, we'd check roles. For now, assuming User context.
             throw new DomainException("Not authorized to cancel this booking.");
         }
         
         if (booking.Status == BookingStatus.Cancelled) throw new DomainException("Booking already cancelled.");
         
         // Refund Logic
         // 24h -> 100%, 6-24h -> 50%, <6h -> 0%
         // Need Slot Start Time
         if (booking.Slot == null) throw new DomainException("Invalid booking state.");
         
         var timeUntilSlot = booking.Slot.StartTime - DateTime.UtcNow;
         decimal refundPercentage = 0;
         
         if (timeUntilSlot.TotalHours >= 24) refundPercentage = 1.0m;
         else if (timeUntilSlot.TotalHours >= 6) refundPercentage = 0.5m;
         else refundPercentage = 0m;
         
         decimal refundAmount = booking.LockedPrice * refundPercentage;
         
         using var transaction = await _context.Database.BeginTransactionAsync();
         try
         {
             if (refundAmount > 0)
             {
                 var walletService = new WalletService(_context);
                 // We need a "Credit" method in WalletService similar to AddFunds but maybe internal or repurposed.
                 // AddFundsRequestDto requires ReferenceId. 
                 await walletService.AddFundsAsync(booking.UserId, new AddFundsRequestDto 
                 { 
                     Amount = refundAmount, 
                     ReferenceId = $"Refund:{booking.BookingId}" 
                 });
             }
             
             booking.Status = BookingStatus.Cancelled;
             booking.Slot.Status = SlotStatus.Available; // Free up slot?
             // Requirements say "Cancel booking; triggers refund rules". Usually frees up slot.
             
             await _context.SaveChangesAsync();
             await transaction.CommitAsync();
         }
         catch
         {
             await transaction.RollbackAsync();
             throw;
         }
    }

    public async Task<IEnumerable<BookingResponseDto>> GetUserBookingsAsync(Guid userId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
            
        return bookings.Select(b => new BookingResponseDto
        {
            BookingId = b.BookingId,
            SlotId = b.SlotId,
            UserId = b.UserId,
            Price = b.LockedPrice,
            Status = b.Status.ToString(),
            CreatedAt = b.CreatedAt
        });
    }
}

public class SlotLockData
{
    public Guid SlotId { get; set; }
    public Guid UserId { get; set; }
    public decimal LockedPrice { get; set; }
    public DateTime ExpiryTime { get; set; }
}
