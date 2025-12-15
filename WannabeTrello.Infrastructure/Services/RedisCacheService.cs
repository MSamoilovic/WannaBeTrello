using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using WannabeTrello.Application.Common.Interfaces;
using WannabeTrello.Infrastructure.Options;

namespace WannabeTrello.Infrastructure.Services
{
    public class RedisCacheService : ICachingService
    {

        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly RedisOptions _options;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly string _instanceName;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis, IOptions<RedisOptions> options, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _redis = redis;
            _options = options.Value;
            _logger = logger;
            _instanceName = _options.InstanceName;
        }


        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled) return false;

            var fullKey = GetFullKey(key);

            try
            {
                var value = await _cache.GetAsync(fullKey, cancellationToken);
                return value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache key existence: {Key}", key);
                return false;
            }
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled) return default;

            var fullKey = GetFullKey(key);

            try
            {
                var cachedValue = await _cache.GetStringAsync(fullKey, cancellationToken);

                if (cachedValue == null)
                    return default;

                return JsonSerializer.Deserialize<T>(cachedValue, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                return default;
            }
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                return await factory();
            }

            var fullKey = GetFullKey(key);

            try
            {
                var cachedValue = await _cache.GetStringAsync(fullKey, cancellationToken);

                if (cachedValue != null)
                {
                    _logger.LogDebug("Cache HIT for key: {Key}", key);
                    return JsonSerializer.Deserialize<T>(cachedValue, JsonOptions);
                }

                _logger.LogDebug("Cache MISS for key: {Key}", key);

                var value = await factory();

                if (value != null)
                {
                    await SetAsync(key, value, expiration, cancellationToken);
                }

                return value;
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Redis connection failed, falling back to database for key: {Key}", key);
                return await factory();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache error for key: {Key}", key);
                return await factory();
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled) return;

            var fullKey = GetFullKey(key);

            try
            {
                await _cache.RemoveAsync(fullKey, cancellationToken);
                _logger.LogDebug("Cache REMOVE for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled) return;

            var fullPrefix = GetFullKey(prefix);

            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{fullPrefix}*").ToArray();

                if (keys.Length > 0)
                {
                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cache REMOVE BY PREFIX: {Prefix}, removed {Count} keys", prefix, keys.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache keys by prefix: {Prefix}", prefix);
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled) return;

            var fullKey = GetFullKey(key);
            var exp = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);

            try
            {
                var serialized = JsonSerializer.Serialize(value, JsonOptions);

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = exp
                };

                await _cache.SetStringAsync(fullKey, serialized, options, cancellationToken);
                _logger.LogDebug("Cache SET for key: {Key}, expiration: {Expiration}", key, exp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
            }
        }

        private string GetFullKey(string key) => $"{_instanceName}:{key}";
    }
}
