namespace costs.net.core.tests.Services.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using core.Models;
    using core.Models.Cache;
    using core.Models.Rule;
    using core.Models.User;
    using core.Models.Utils;
    using core.Services.Cache;
    using core.Services.Rules;
    using dataAccess.Entity;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using NUnit.Framework;
    using Rule = dataAccess.Entity.Rule;

    public class CacheableRuleServiceTests
    {
        private CacheSettings _cacheSettings;
        private Mock<IRuleService> _innerRuleServiceMock;
        private Mock<ICache> _cacheMock;
        private CacheableRuleService _ruleService;
        private readonly string _cacheKeysKey = CacheEntryType.BusinessRules.ToString();

        private class TestRule
        {

        }

        [SetUp]
        public void Init()
        {
            _cacheSettings = new CacheSettings();
            var appSettingsMock = new Mock<IOptions<CacheSettings>>();
            appSettingsMock.Setup(s => s.Value).Returns(_cacheSettings);
            _innerRuleServiceMock = new Mock<IRuleService>();
            _cacheMock = new Mock<ICache>();

            _ruleService = new CacheableRuleService(_innerRuleServiceMock.Object, _cacheMock.Object, appSettingsMock.Object);
        }

        [Test]
        public async Task GetCompiledByRuleType_WhenInCache_ShouldGetRulesFromCache()
        {
            // Arrange
            const RuleType ruleType = RuleType.Action;
            var cacheKey = $"{_cacheKeysKey}_{ruleType}_{nameof(TestRule)}";
            var cacheKeysKey =_cacheKeysKey;

            object expectedRules = new List<CompiledRule<TestRule>>();
            _cacheMock.Setup(c => c.TryGetValue(cacheKey, out expectedRules)).Returns(true);

            object cacheKeys;
            _cacheMock.Setup(c => c.TryGetValue(cacheKeysKey, out cacheKeys)).Returns(false);

            // Act
            var compiledRules = await _ruleService.GetCompiledByRuleType<TestRule>(RuleType.Action);

            // Assert
            _innerRuleServiceMock.Verify(r => r.GetCompiledByRuleType<TestRule>(ruleType), Times.Never);
            compiledRules.Should().BeSameAs((List<CompiledRule<TestRule>>)expectedRules);
        }

        [Test]
        public async Task GetCompiledByRuleType_WhenNotInCache_ShouldGetRulesFromRuleService()
        {
            // Arrange
            const RuleType ruleType = RuleType.Action;
            var cacheKey = $"{_cacheKeysKey}_{ruleType}_{nameof(TestRule)}";
            var cacheKeysKey =_cacheKeysKey;

            var expectedRules = new List<CompiledRule<TestRule>>();
            _innerRuleServiceMock.Setup(r => r.GetCompiledByRuleType<TestRule>(ruleType)).ReturnsAsync(expectedRules);

            object cacheRules;
            _cacheMock.Setup(c => c.TryGetValue(cacheKey, out cacheRules)).Returns(false);

            object cacheKeys;
            _cacheMock.Setup(c => c.TryGetValue(cacheKeysKey, out cacheKeys)).Returns(false);

            // Act
            var compiledRules = await _ruleService.GetCompiledByRuleType<TestRule>(RuleType.Action);

            // Assert
            _innerRuleServiceMock.Verify(r => r.GetCompiledByRuleType<TestRule>(ruleType), Times.Once);
            compiledRules.Should().BeSameAs(expectedRules);
        }
        [Test]
        public async Task GetCompiledByRuleType_WhenNotInCache_ShouldAddCompiledRulesToCache()
        {
            // Arrange
            const RuleType ruleType = RuleType.Action;
            var cacheKey = $"{_cacheKeysKey}_{ruleType}_{nameof(TestRule)}";
            var cacheKeysKey =_cacheKeysKey;
            var expectedRules = new List<CompiledRule<TestRule>>();
            _innerRuleServiceMock.Setup(r => r.GetCompiledByRuleType<TestRule>(ruleType)).ReturnsAsync(expectedRules);

            object cacheRules;
            _cacheMock.Setup(c => c.TryGetValue(cacheKey, out cacheRules)).Returns(false);

            object cacheKeys;
            _cacheMock.Setup(c => c.TryGetValue(cacheKeysKey, out cacheKeys)).Returns(false);

            // Act
            var compiledRules = await _ruleService.GetCompiledByRuleType<TestRule>(RuleType.Action);

            // Assert
            _cacheMock.Verify(e => e.SetAsync(cacheKey, compiledRules, It.IsAny<CacheEntryOptions>()), Times.Once);
        }

        [Test]
        public async Task Ctor_WhenRuleExpirationIsValid_ShouldUseProvidedExpiration()
        {
            // Arrange
            const RuleType ruleType = RuleType.Action;
            var cacheKey = $"{_cacheKeysKey}_{ruleType}_{nameof(TestRule)}";
            var cacheKeysKey =_cacheKeysKey;

            var expectedRules = new List<CompiledRule<TestRule>>();
            _innerRuleServiceMock.Setup(r => r.GetCompiledByRuleType<TestRule>(ruleType)).ReturnsAsync(expectedRules);

            object cacheRules;
            _cacheMock.Setup(c => c.TryGetValue(cacheKey, out cacheRules)).Returns(false);

            object cacheKeys;
            _cacheMock.Setup(c => c.TryGetValue(cacheKeysKey, out cacheKeys)).Returns(false);

            _cacheSettings = new CacheSettings { RuleExpiration = "2:00:00:00.000" }; // 2 days
            var appSettingsMock = new Mock<IOptions<CacheSettings>>();
            appSettingsMock.Setup(s => s.Value).Returns(_cacheSettings);

            _ruleService = new CacheableRuleService(_innerRuleServiceMock.Object, _cacheMock.Object, appSettingsMock.Object);

            // Act
            await _ruleService.GetCompiledByRuleType<TestRule>(RuleType.Action);

            // Assert
            _cacheMock.Verify(c => c.SetAsync(cacheKey, expectedRules, It.Is<CacheEntryOptions>(o => o.Expiration.Equals(TimeSpan.FromDays(2)))));
        }

        [Test]
        public async Task Ctor_WhenRuleExpirationIsInvalidOrEmpty_ShouldUseDefaultExpiration()
        {
            // Arrange
            const RuleType ruleType = RuleType.Action;
            var cacheKey = $"{_cacheKeysKey}_{ruleType}_{nameof(TestRule)}";
            var cacheKeysKey = _cacheKeysKey;
            var expectedRules = new List<CompiledRule<TestRule>>();
            _innerRuleServiceMock.Setup(r => r.GetCompiledByRuleType<TestRule>(ruleType)).ReturnsAsync(expectedRules);

            object cacheRules;
            _cacheMock.Setup(c => c.TryGetValue(cacheKey, out cacheRules)).Returns(false);

            object cacheKeys;
            _cacheMock.Setup(c => c.TryGetValue(cacheKeysKey, out cacheKeys)).Returns(false);

            _cacheSettings = new CacheSettings { RuleExpiration = "invalid" };
            var appSettingsMock = new Mock<IOptions<CacheSettings>>();
            appSettingsMock.Setup(s => s.Value).Returns(_cacheSettings);

            _ruleService = new CacheableRuleService(_innerRuleServiceMock.Object, _cacheMock.Object, appSettingsMock.Object);

            // Act
            await _ruleService.GetCompiledByRuleType<TestRule>(RuleType.Action);

            // Assert
            _cacheMock.Verify(c => c.SetAsync(cacheKey, expectedRules, It.Is<CacheEntryOptions>(o => !o.Expiration.HasValue)));
        }

        [Test]
        public async Task CheckAipeApplicability_Always_ShouldInvokeRuleService()
        {
            // Arrange
            var region = It.IsAny<string>();
            var targetBadget = It.IsAny<decimal>();
            var contentType = It.IsAny<string>();
            var productionType = It.IsAny<string>();
            var buType = It.IsAny<BuType>();

            // Act
            await _ruleService.CheckAipeApplicability(region, targetBadget, contentType, productionType, buType);

            // Assert
            _innerRuleServiceMock.Verify(r => r.CheckAipeApplicability(region, targetBadget, contentType, productionType, buType));
        }

        [Test]
        public async Task GetVendorCriteria_Always_ShouldInvokeRuleService()
        {
            // Arrange
            var buType = It.IsAny<BuType>();

            // Act
            await _ruleService.GetVendorCriteria(buType);

            // Assert
            _innerRuleServiceMock.Verify(r => r.GetVendorCriteria(buType));
        }

        [Test]
        public void TryMatchRule_Always_ShouldInvokeRuleService()
        {
            // Arrange
            var rules = Enumerable.Empty<CompiledRule<TestRule>>().ToArray();
            var test = It.IsAny<TestRule>();
            var func = It.IsAny<Func<TestRule, Rule, TestRule>>();

            // Act
            _ruleService.TryMatchRule(rules, test, func, out var result);

            // Assert
            _innerRuleServiceMock.Verify(r => r.TryMatchRule(rules, test, func, out result));
        }
        [Test]
        public void TryMatchRuleWithAggregator_Always_ShouldInvokeRuleService()
        {
            // Arrange
            var rules = Enumerable.Empty<CompiledRule<TestRule>>().ToArray();
            var test = It.IsAny<TestRule>();
            var func = It.IsAny<Func<TestRule, Rule, TestRule>>();
            var aggregator = It.IsAny<Func<TestRule, TestRule, TestRule>>();

            // Act
            _ruleService.TryMatchRule(rules, test, func, aggregator, out var result);

            // Assert
            _innerRuleServiceMock.Verify(r => r.TryMatchRule(rules, test, func, aggregator, out result));
        }

        [Test]
        public async Task GetCompiledByVendorId_Always_ShouldInvokeRuleService()
        {
            // Arrange
            var vendorId = Guid.NewGuid();

            // Act
            await _ruleService.GetCompiledByVendorId<TestRule>(vendorId, RuleType.VendorPayment);

            // Assert
            _innerRuleServiceMock.Verify(r => r.GetCompiledByVendorId<TestRule>(vendorId, RuleType.VendorPayment, null));
        }

        [Test]
        public async Task CanEditIONumber_Always_ShouldInvokeRuleService()
        {
            // Arrange
            var userIdentity = new UserIdentity();
            var costId = Guid.NewGuid();

            // Act
            await _ruleService.CanEditIONumber(userIdentity, costId);

            // Assert
            _innerRuleServiceMock.Verify(r => r.CanEditIONumber(userIdentity, costId));
        }
    }
}
