using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace SportsBookingPlatform.API.Services;

public class SlotService : ISlotService
{
    private readonly ISlotRepository _slotRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IDiscountService _discountService;
    private readonly IMemoryCache _memoryCache; // Added

    public SlotService(ISlotRepository slotRepository, ICourtRepository courtRepository, IDiscountService discountService, IMemoryCache memoryCache)
    {
        _slotRepository = slotRepository;
        _courtRepository = courtRepository;
        _discountService = discountService;
        _memoryCache = memoryCache;
    }

    public async Task GenerateSlotsAsync(GenerateSlotsRequestDto request)
    {
         // ... (existing code)
         // Wait, I should use valid existing code or just replace the constructor and add methods at the end?
         // ReplaceFileContent is better for small chunks.
         // But I need to add field, update constructor, add method, update CalculatePrice.
         // Effectively rewriting the class structure.
         // I'll do it in chunks.
    }
    
    // Chunk 1: Constructor and Field
    // StartLine 10, EndLine 19




    public async Task<IEnumerable<SlotDto>> GetAvailableSlotsAsync(SlotSearchRequestDto request)
    {
        var start = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);
        var end = start.AddDays(1).AddTicks(-1);

        var slots = await _slotRepository.GetAvailableSlotsAsync(request.VenueId, request.CourtId, start, end);

        if (!string.IsNullOrEmpty(request.SportType))
        {
            slots = slots.Where(s => s.Court?.SportType == request.SportType);
        }

        // We need async select, so we cannot use LINQ Select directly with async lambda easily without formatting
        var slotDtos = new List<SlotDto>();
        foreach (var s in slots)
        {
            var finalPrice = await CalculateDynamicPrice(s);
            slotDtos.Add(new SlotDto
            {
                SlotId = s.SlotId,
                CourtId = s.CourtId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BasePrice = s.BasePrice,
                FinalPrice = finalPrice, 
                Status = s.Status.ToString()
            });
        }

        return slotDtos;
    }

    public void RecordVenueSearch(Guid venueId)
    {
        string cacheKey = $"VenueSearches:{venueId}";
        _memoryCache.TryGetValue(cacheKey, out int currentCount);
        
        currentCount++;
        
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5)); // Spec: 5 min window
            
        _memoryCache.Set(cacheKey, currentCount, cacheOptions);
    }

    public async Task<decimal> CalculateDynamicPrice(Slot slot)
    {
        Guid venueId = slot.Court?.VenueId ?? Guid.Empty;

        // 1. Demand Multiplier
        // 1.0 (0-1), 1.2 (2-5), 1.5 (>5)
        decimal demandMultiplier = 1.0m;
        if (venueId != Guid.Empty)
        {
            if (_memoryCache.TryGetValue($"VenueSearches:{venueId}", out int searchCount))
            {
                if (searchCount > 5) demandMultiplier = 1.5m;
                else if (searchCount >= 2) demandMultiplier = 1.2m;
            }
        }

        // 2. Time-based Multiplier
        // 1.0 (>24h), 1.2 (6-24h), 1.5 (<6h)
        decimal timeMultiplier = 1.0m;
        var hoursUntilSlot = (slot.StartTime - DateTime.UtcNow).TotalHours;
        
        if (hoursUntilSlot < 6) timeMultiplier = 1.5m;
        else if (hoursUntilSlot < 24) timeMultiplier = 1.2m;

        // 3. Historical Popularity Multiplier
        // Computed via background job (VenuePopularity:{venueId})
        // 1.0 (Low), 1.2 (Med), 1.5 (High)
        decimal historicalMultiplier = 1.0m;
        if (venueId != Guid.Empty)
        {
            if (_memoryCache.TryGetValue($"VenuePopularity:{venueId}", out decimal cachedMultiplier))
            {
                historicalMultiplier = cachedMultiplier;
            }
        }

        // Calculate Final Price
        // Formula: Base * Demand * Time * Historical
        decimal priceAfterMultipliers = slot.BasePrice * demandMultiplier * timeMultiplier * historicalMultiplier;

        // 4. Discount Factor
        if (venueId != Guid.Empty)
        {
            return await _discountService.ApplyDiscountsAsync(venueId, slot.CourtId, priceAfterMultipliers, slot.StartTime);
        }

        return priceAfterMultipliers;
    }
}
