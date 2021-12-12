using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SimCompaniesOptimizer.Database;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models;

namespace SimCompaniesOptimizer.APIs;

public class SimCompaniesApi : ISimCompaniesApi
{
    private readonly IExchangeTrackerApi _exchangeTrackerApi;
    private readonly ILogger<SimCompaniesApi> _logger;
    private readonly ConcurrentDictionary<ResourceId, Resource> _inMemoryResourceCache = new();

    public SimCompaniesApi(ILogger<SimCompaniesApi> logger, IExchangeTrackerApi exchangeTrackerApi)
    {
        _logger = logger;
        _exchangeTrackerApi = exchangeTrackerApi;
    }

    public async Task<IEnumerable<Resource>> GetAllResourcesAsync(CancellationToken cancellationToken,
        TimeSpan retrievalDelay = default)
    {
        var result = new List<Resource>();
        foreach (var resourceId in Enum.GetValues<ResourceId>())
        {
            var resource = await GetResourceAsync(resourceId, cancellationToken);
            if (resource != null)
                result.Add(resource);
            else
                _logger.LogWarning($"Could not retrieve resource {resourceId}");

            if (retrievalDelay.TotalSeconds > 0) await Task.Delay(retrievalDelay, cancellationToken);
        }

        return result;
    }

    public async Task<Resource?> GetResourceAsync(ResourceId resourceId, CancellationToken cancellationToken,
        int quality = 0)
    {
        var cached = _inMemoryResourceCache.TryGetValue(resourceId, out var memCachedResource);
        if (cached)
        {
            return memCachedResource;
        }

        await using var db = new SimCompaniesDbContext();
        var cachedResource = db.Resources.FirstOrDefault(r => r.Id == resourceId);

        if (cachedResource != null)
        {
            _inMemoryResourceCache.TryAdd(resourceId, cachedResource);
            return cachedResource;
        }

        var uri = $"{SimCompaniesConstants.BaseUrl}{SimCompaniesConstants.Encyclopedia}{quality}/{(int)resourceId}/";
        using var client = new HttpClient();
        var getResponse = await client.GetAsync(uri, cancellationToken);
        getResponse.EnsureSuccessStatusCode();
        var contentStream = await getResponse.Content.ReadAsStreamAsync(cancellationToken);
        var retrievedResource =
            await JsonSerializer.DeserializeAsync<Resource>(contentStream, cancellationToken: cancellationToken);
        if (retrievedResource != null)
        {
            db.Resources.Add(retrievedResource);
            await db.SaveChangesAsync(cancellationToken);
        }

        return retrievedResource;
    }

    public async Task UpdateExchangePriceOfResource(ResourceId resourceId, CancellationToken cancellationToken)
    {
        var priceCard = await _exchangeTrackerApi.GetPriceDetails(resourceId, TimeSpan.FromDays(10), cancellationToken);

        await using var db = new SimCompaniesDbContext();
        var cachedResource = db.Resources.First(r => r.Id == resourceId);
        cachedResource.PriceCard = priceCard;
        if (priceCard.Current != null) cachedResource.CurrentExchangePrice = priceCard.Current.Value ?? 0;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateExchangePriceOfAllResources(CancellationToken cancellationToken)
    {
        foreach (var resourceId in Enum.GetValues<ResourceId>())
            await UpdateExchangePriceOfResource(resourceId, cancellationToken);
    }
}