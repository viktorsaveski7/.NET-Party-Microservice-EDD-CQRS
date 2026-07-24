using Microsoft.Extensions.Caching.Distributed;
using PartyService.Application.Interfaces;
using PartyService.Domain.Entities;
using System.Text.Json;

namespace PartyService.Infrastructure.Repositories;

public class CachedPartyRepository : IPartyRepository
{
    private readonly IPartyRepository _inner;
    private readonly IDistributedCache _cache;
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    private const string PartyKeyPrefix = "party:";
    private const string AllPartiesKey = "parties:all";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CachedPartyRepository(IPartyRepository inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Party> CreateAsync(Party party)
    {
        var result = await _inner.CreateAsync(party);
        await _cache.RemoveAsync(AllPartiesKey);
        return result;
    }

    public async Task<Party?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"{PartyKeyPrefix}{id}";

        var cached = await GetFromCacheAsync(cacheKey);
        if (cached is not null)
            return cached;

        var party = await _inner.GetByIdAsync(id);

        if (party is not null)
            await SetCacheAsync(cacheKey, party);

        return party;
    }

    public async Task<IEnumerable<Party>> GetAllAsync()
    {
        var cached = await GetListFromCacheAsync();
        if (cached is not null)
            return cached;

        var parties = await _inner.GetAllAsync();

        var partyList = parties.ToList();
        await SetCacheAsync(AllPartiesKey, partyList);

        return partyList;
    }

    public async Task<Party> UpdateAsync(Party party)
    {
        var result = await _inner.UpdateAsync(party);
        await InvalidatePartyCacheAsync(result.Id);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _inner.DeleteAsync(id);
        await InvalidatePartyCacheAsync(id);
        return result;
    }

    private async Task InvalidatePartyCacheAsync(Guid id)
    {
        await _cache.RemoveAsync($"{PartyKeyPrefix}{id}");
        await _cache.RemoveAsync(AllPartiesKey);
    }

    private async Task<Party?> GetFromCacheAsync(string key)
    {
        try
        {
            var data = await _cache.GetAsync(key);
            if (data is null)
                return null;

            var json = System.Text.Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<Party>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<Party>?> GetListFromCacheAsync()
    {
        try
        {
            var data = await _cache.GetAsync(AllPartiesKey);
            if (data is null)
                return null;

            var json = System.Text.Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<List<Party>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private async Task SetCacheAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            var data = System.Text.Encoding.UTF8.GetBytes(json);
            await _cache.SetAsync(key, data, CacheOptions);
        }
        catch
        {
        }
    }
}
