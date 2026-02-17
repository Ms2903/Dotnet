using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SportsBookingPlatform.API.Repositories;

namespace SportsBookingPlatform.API.BackgroundServices;

public class PopularityBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<PopularityBackgroundService> _logger;

    public PopularityBackgroundService(IServiceProvider serviceProvider, IMemoryCache memoryCache, ILogger<PopularityBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateVenuePopularityAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating venue popularity cache.");
            }

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task UpdateVenuePopularityAsync()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var ratingRepository = scope.ServiceProvider.GetRequiredService<IRatingRepository>();
            
            // Get Average Ratings for Venues
            // Assuming "Venue" is the target type string used in Ratings
            var venueRatings = await ratingRepository.GetAverageRatingsByTargetTypeAsync("Venue");

            foreach (var kvp in venueRatings)
            {
                Guid venueId = kvp.Key;
                double avgRating = kvp.Value;
                decimal multiplier = 1.0m;

                // Historical Popularity Multiplier Logic
                // 1.0 for Low Demand (Avg Rating 1-2)
                // 1.2 for Medium Demand (Avg Rating 3) -> 2.5 to 3.5? Let's say < 4.
                // 1.5 for High Demand (Avg Rating 4-5) -> >= 4.

                if (avgRating >= 4.0)
                {
                    multiplier = 1.5m;
                }
                else if (avgRating >= 2.5) // Assuming 3 starts from 2.5 to 3.5
                {
                    multiplier = 1.2m;
                }
                else
                {
                    multiplier = 1.0m;
                }

                var cacheKey = $"VenuePopularity:{venueId}";
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(2)); // Cache significantly longer than update interval

                _memoryCache.Set(cacheKey, multiplier, cacheOptions);
            }
            
            _logger.LogInformation($"Updated popularity cache for {venueRatings.Count} venues.");
        }
    }
}
