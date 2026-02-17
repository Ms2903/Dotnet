using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.Services;

public class SlotService : ISlotService
{
    private readonly ISlotRepository _slotRepository;
    private readonly ICourtRepository _courtRepository;
    private readonly IDiscountService _discountService;

    public SlotService(ISlotRepository slotRepository, ICourtRepository courtRepository, IDiscountService discountService)
    {
        _slotRepository = slotRepository;
        _courtRepository = courtRepository;
        _discountService = discountService;
    }

    public async Task GenerateSlotsAsync(GenerateSlotsRequestDto request)
    {
        var court = await _courtRepository.GetCourtByIdAsync(request.CourtId);
        if (court == null) throw new DomainException("Court not found.");

        var slots = new List<Slot>();
        var currentDate = request.StartDate.Date;
        var endDate = request.EndDate.Date;

        while (currentDate <= endDate)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var operatingHour = court.OperatingHours.FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);

            if (operatingHour != null)
            {
                var startTime = currentDate.Add(operatingHour.OpenTime);
                var closeTime = currentDate.Add(operatingHour.CloseTime);

                while (startTime.AddMinutes(court.SlotDurationMinutes) <= closeTime)
                {
                    var slotEndTime = startTime.AddMinutes(court.SlotDurationMinutes);
                    
                    slots.Add(new Slot
                    {
                        CourtId = court.CourtId,
                        StartTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc),
                        EndTime = DateTime.SpecifyKind(slotEndTime, DateTimeKind.Utc),
                        Status = SlotStatus.Available,
                        BasePrice = court.BasePrice
                    });

                    startTime = slotEndTime;
                }
            }

            currentDate = currentDate.AddDays(1);
        }
        
        var existingSlots = await _slotRepository.GetSlotsByCourtAndDateRangeAsync(
            request.CourtId, 
            DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc), 
            DateTime.SpecifyKind(request.EndDate.AddDays(1), DateTimeKind.Utc));

        var newSlots = slots.Where(s => !existingSlots.Any(es => es.StartTime == s.StartTime)).ToList();

        if (newSlots.Any())
        {
            await _slotRepository.AddSlotsAsync(newSlots);
        }
    }

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

    private async Task<decimal> CalculateDynamicPrice(Slot slot)
    {
        // 1. Time-based Multiplier
        decimal timeMultiplier = 1.0m;
        var hoursUntilSlot = (slot.StartTime - DateTime.UtcNow).TotalHours;
        
        if (hoursUntilSlot < 6) timeMultiplier = 1.5m;
        else if (hoursUntilSlot < 24) timeMultiplier = 1.2m;
        
        decimal priceAfterMultipliers = slot.BasePrice * timeMultiplier;

        // 2. Apply Discounts
        // We need VenueId. Slot has Court, Court has VenueId? 
        // Slot -> CourtId. We need to fetch Court to get VenueId if not loaded.
        // The repository `GetAvailableSlotsAsync` usually includes Court. 
        // Let's assume s.Court is not null or we have VenueId from request (but slot might rely on its own data).
        // If s.Court is null, we can't easily get VenueId. Repos should Include(s => s.Court).
        
        Guid venueId = slot.Court?.VenueId ?? Guid.Empty; 
        // If empty, discount service might fail or return 0 discount.
        
        if (venueId != Guid.Empty)
        {
            return await _discountService.ApplyDiscountsAsync(venueId, slot.CourtId, priceAfterMultipliers, slot.StartTime);
        }

        return priceAfterMultipliers;
    }
}
