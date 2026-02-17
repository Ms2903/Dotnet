using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.Services;

public class CourtService : ICourtService
{
    private readonly ICourtRepository _courtRepository;
    private readonly IVenueRepository _venueRepository;

    public CourtService(ICourtRepository courtRepository, IVenueRepository venueRepository)
    {
        _courtRepository = courtRepository;
        _venueRepository = venueRepository;
    }

    public async Task<CourtResponseDto> CreateCourtAsync(CreateCourtRequestDto request, Guid userId)
    {
        var venue = await _venueRepository.GetVenueByIdAsync(request.VenueId);
        if (venue == null)
        {
            throw new DomainException("Venue not found.");
        }

        if (venue.OwnerId != userId)
        {
            throw new DomainException("You are not the owner of this venue.");
        }

        var court = new Court
        {
            VenueId = request.VenueId,
            Name = request.Name,
            SportType = request.SportType,
            SlotDurationMinutes = request.SlotDurationMinutes,
            BasePrice = request.BasePrice,
            OperatingHours = request.OperatingHours.Select(oh => new OperatingHour
            {
                DayOfWeek = oh.DayOfWeek,
                OpenTime = oh.OpenTime,
                CloseTime = oh.CloseTime
            }).ToList()
        };

        var createdCourt = await _courtRepository.CreateCourtAsync(court);
        return MapToDto(createdCourt);
    }

    public async Task<CourtResponseDto> UpdateCourtAsync(Guid courtId, UpdateCourtRequestDto request, Guid userId)
    {
        var court = await _courtRepository.GetCourtByIdAsync(courtId);
        if (court == null) throw new DomainException("Court not found.");

        if (court.Venue?.OwnerId != userId)
        {
            throw new DomainException("You are not the owner of this venue.");
        }

        if (request.Name != null) court.Name = request.Name;
        if (request.BasePrice.HasValue) court.BasePrice = request.BasePrice.Value;
        if (request.IsActive.HasValue) court.IsActive = request.IsActive.Value;
        if (request.SlotDurationMinutes.HasValue) court.SlotDurationMinutes = request.SlotDurationMinutes.Value;

        await _courtRepository.UpdateCourtAsync(court);
        return MapToDto(court);
    }

    public async Task DeleteCourtAsync(Guid courtId, Guid userId)
    {
        var court = await _courtRepository.GetCourtByIdAsync(courtId);
        if (court == null) throw new DomainException("Court not found.");

        if (court.Venue?.OwnerId != userId)
        {
            throw new DomainException("You are not the owner of this venue.");
        }

        if (await _courtRepository.HasFutureBookingsAsync(courtId))
        {
            throw new DomainException("Cannot delete court with future bookings.");
        }

        await _courtRepository.DeleteCourtAsync(court);
    }

    public async Task<IEnumerable<CourtResponseDto>> GetCourtsByVenueIdAsync(Guid venueId)
    {
        var courts = await _courtRepository.GetCourtsByVenueIdAsync(venueId);
        return courts.Select(MapToDto);
    }

    public async Task<CourtResponseDto> GetCourtByIdAsync(Guid courtId)
    {
        var court = await _courtRepository.GetCourtByIdAsync(courtId);
        if (court == null) throw new DomainException("Court not found.");
        return MapToDto(court);
    }

    private static CourtResponseDto MapToDto(Court court)
    {
        return new CourtResponseDto
        {
            CourtId = court.CourtId,
            VenueId = court.VenueId,
            Name = court.Name,
            SportType = court.SportType,
            SlotDurationMinutes = court.SlotDurationMinutes,
            BasePrice = court.BasePrice,
            IsActive = court.IsActive,
            OperatingHours = court.OperatingHours.Select(oh => new OperatingHourDto
            {
                DayOfWeek = oh.DayOfWeek,
                OpenTime = oh.OpenTime,
                CloseTime = oh.CloseTime
            }).ToList()
        };
    }
}
