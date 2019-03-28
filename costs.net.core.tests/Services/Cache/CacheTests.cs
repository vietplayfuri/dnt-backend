namespace costs.net.core.tests.Services.Cache
{
    using System;
    using System.Threading.Tasks;
    using core.Models.Utils;
    using core.Services.Cache;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CacheTests
    {
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromSeconds(123);
        private CacheSettings _cacheSettings;
        private Mock<IOptions<CacheSettings>> _cacheSettingsMock;
        private Mock<IMemoryCache> _memoryCacheMock;
        private Cache _cache;

        [SetUp]
        public void Init()
        {
            _cacheSettingsMock = new Mock<IOptions<CacheSettings>>();
            _cacheSettings = new CacheSettings { DefaultExpiration = _defaultExpiration.ToString("G") };
            _cacheSettingsMock.Setup(s => s.Value).Returns(_cacheSettings);

            _memoryCacheMock = new Mock<IMemoryCache>();

            _cache = new Cache(_cacheSettingsMock.Object, _memoryCacheMock.Object);
        }

        [Test]
        public void TryGetValue_Always_ShouldTryGetValueMemoryCache()
        {
            // Arrange
            var key = new object();
            var value = new object();

            // Act
            _cache.TryGetValue(key, out value);

            // Assert
            _memoryCacheMock.Verify(mc => mc.TryGetValue(key, out value), Times.Once);
        }

        [Test]
        public async Task TryGetValueAsync_Always_ShouldTryGetValueMemoryCache()
        {
            // Arrange
            var key = new object();
            var value = new object();

            // Act
            await _cache.TryGetValueAsync(key, out value);

            // Assert
            _memoryCacheMock.Verify(mc => mc.TryGetValue(key, out value), Times.Once);
        }

        [Test]
        public async Task RemoveAsync_Always_ShouldRemoveFromMemoryCache()
        {
            // Arrange
            var key = new object();

            // Act
            await _cache.RemoveAsync(key);

            // Assert
            _memoryCacheMock.Verify(mc => mc.Remove(key), Times.Once);
        }

        [Test]
        public void Remove_Always_ShouldRemoveFromMemoryCache()
        {
            // Arrange
            var key = new object();

            // Act
            _cache.Remove(key);

            // Assert
            _memoryCacheMock.Verify(mc => mc.Remove(key), Times.Once);
        }

        [Test]
        public void Set_WhenCacheOptionsNotProvided_ShouldUseDefaultExpirationFromConfig()
        {
            // Arrange
            var key = new object();
            var value = new object();

            var cacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

            // Act
            _cache.Set(key, value);

            // Assert
            cacheEntryMock.VerifySet(e => e.SlidingExpiration = _defaultExpiration);
        }

        [Test]
        public async Task SetAsync_WhenCacheOptionsNotProvided_ShouldUseDefaultExpirationFromConfig()
        {
            // Arrange
            var key = new object();
            var value = new object();

            var cacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

            // Act
            await _cache.SetAsync(key, value);

            // Assert
            cacheEntryMock.VerifySet(e => e.SlidingExpiration = _defaultExpiration);
        }

        [Test]
        public void Set_WhenConfigExpirationIsNotProvided_ShouldUseDefaultExpiration()
        {
            // Arrange
            var key = new object();
            var value = new object();

            var cacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);
            _cacheSettings.DefaultExpiration = null;
            _cache = new Cache(_cacheSettingsMock.Object, _memoryCacheMock.Object);

            // Act
            _cache.Set(key, value);

            // Assert
            cacheEntryMock.VerifySet(e => e.SlidingExpiration = TimeSpan.FromDays(1));
        }

        [Test]
        public async Task SetAsync_WhenConfigExpirationIsNotProvided_ShouldUseDefaultExpiration()
        {
            // Arrange
            var key = new object();
            var value = new object();

            var cacheEntryMock = new Mock<ICacheEntry>();
            _memoryCacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);
            _cacheSettings.DefaultExpiration = null;
            _cache = new Cache(_cacheSettingsMock.Object, _memoryCacheMock.Object);

            // Act
            await _cache.SetAsync(key, value);

            // Assert
            cacheEntryMock.VerifySet(e => e.SlidingExpiration = TimeSpan.FromDays(1));
        }
    }
}
