using System.Text.Json;
using SportsBookingPlatform.API.Dtos;
using SportsBookingPlatform.API.Entities;
using SportsBookingPlatform.API.Exceptions;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venueRepository;

    public VenueService(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }

    public async Task<VendorVenueResponseDto> CreateVenueAsync(CreateVenueRequestDto request, Guid ownerId)
    {
        var venue = new Venue
        {
            Name = request.Name,
            Address = request.Address,
            OwnerId = ownerId,
            SportsSupportedJson = JsonSerializer.Serialize(request.SportsSupported),
            ApprovalStatus = VenueApprovalStatus.Pending
        };

        var createdVenue = await _venueRepository.CreateVenueAsync(venue);

        return MapToDto(createdVenue);
    }

    public async Task<IEnumerable<VendorVenueResponseDto>> GetAllVenuesAsync()
    {
        var venues = await _venueRepository.GetAllVenuesAsync();
        return venues.Select(MapToDto);
    }

    public async Task ApproveVenueAsync(Guid venueId)
    {
        var venue = await _venueRepository.GetVenueByIdAsync(venueId);
        if (venue == null)
        {
            throw new DomainException("Venue not found.");
        }

        venue.ApprovalStatus = VenueApprovalStatus.Approved;
        await _venueRepository.UpdateVenueAsync(venue);
    }

    private static VendorVenueResponseDto MapToDto(Venue venue)
    {
        return new VendorVenueResponseDto
        {
            VenueId = venue.VenueId,
            Name = venue.Name,
            Address = venue.Address,
            SportsSupported = JsonSerializer.Deserialize<List<string>>(venue.SportsSupportedJson) ?? new List<string>(),
            ApprovalStatus = venue.ApprovalStatus.ToString()
        };
    }
}
