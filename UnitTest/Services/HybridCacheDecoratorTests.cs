using AppService.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace UnitTest.Services;

public class HybridCacheDecoratorTests
{
    private readonly Mock<IConnectionMultiplexer> _mockRedis;
    private readonly Mock<IDatabase> _mockRedisDb;
    private readonly Mock<ILogger<HybridCacheDecorator>> _mockLogger;
    private readonly IMemoryCache _memoryCache;
    private readonly HybridCacheDecorator _hybridCache;

    public HybridCacheDecoratorTests()
    {
        _mockRedis = new Mock<IConnectionMultiplexer>();
        _mockRedisDb = new Mock<IDatabase>();
        _mockLogger = new Mock<ILogger<HybridCacheDecorator>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());

        _mockRedis.Setup(x => x.IsConnected).Returns(true);
        _mockRedis.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockRedisDb.Object);

        _hybridCache = new HybridCacheDecorator(_memoryCache, _mockRedis.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_WhenDataInL1_ShouldReturnFromL1()
    {
        // Arrange
        var key = "test:key";
        var expectedValue = "cached-value";
        _memoryCache.Set(key, expectedValue);

        // Act
        var result = await _hybridCache.GetAsync<string>(key);

        // Assert
        Assert.Equal(expectedValue, result);
        
        // Verify Redis was NOT called (L1 hit)
        _mockRedisDb.Verify(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_WhenDataInL2_ShouldPromoteToL1()
    {
        // Arrange
        var key = "test:key";
        var expectedValue = "redis-value";
        var redisValue = System.Text.Json.JsonSerializer.Serialize(expectedValue);

        _mockRedisDb
            .Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)redisValue);

        // Act
        var result = await _hybridCache.GetAsync<string>(key);

        // Assert
        Assert.Equal(expectedValue, result);
        
        // Verify data was promoted to L1
        Assert.True(_memoryCache.TryGetValue(key, out string? cachedValue));
        Assert.Equal(expectedValue, cachedValue);
    }

    [Fact]
    public async Task GetAsync_WhenDataNotFound_ShouldReturnDefault()
    {
        // Arrange
        var key = "non-existent:key";

        _mockRedisDb
            .Setup(x => x.StringGetAsync(key, It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        var result = await _hybridCache.GetAsync<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ShouldWriteToBothL1AndL2()
    {
        // Arrange
        var key = "test:key";
        var value = "test-value";
        var ttl = TimeSpan.FromMinutes(10);

        // Act
        await _hybridCache.SetAsync(key, value, ttl);

        // Assert
        // Verify L1 cache
        Assert.True(_memoryCache.TryGetValue(key, out string? cachedValue));
        Assert.Equal(value, cachedValue);

        // Verify L2 cache was called
        _mockRedisDb.Verify(x => x.StringSetAsync(
            key,
            It.IsAny<RedisValue>(),
            It.IsAny<TimeSpan?>(),
            It.Is<When>(w => w == When.Always),
            It.IsAny<CommandFlags>()
        ), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveFromBothL1AndL2()
    {
        // Arrange
        var key = "test:key";
        var value = "test-value";
        _memoryCache.Set(key, value);

        // Act
        await _hybridCache.RemoveAsync(key);

        // Assert
        // Verify L1 removal
        Assert.False(_memoryCache.TryGetValue(key, out _));

        // Verify L2 removal was called
        _mockRedisDb.Verify(x => x.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task IsHealthyAsync_WhenRedisDown_ShouldStillReturnTrue()
    {
        // Arrange
        _mockRedisDb
            .Setup(x => x.PingAsync(It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Cannot connect"));

        // Act
        var result = await _hybridCache.IsHealthyAsync();

        // Assert
        Assert.True(result); // L1 still works, so should return true
    }
}
