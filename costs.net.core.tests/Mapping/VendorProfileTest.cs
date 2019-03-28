namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using core.Mapping;
    using core.Models.Vendor;
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    public class VendorProfileTest
    {
        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(m => { m.AddProfile<VendorProfile>(); }));
        }

        private IMapper _mapper;

        [Test]
        public void Currency_To_VendorCategory_IsValid()
        {
            var expectedCurrencyId = Guid.NewGuid();
            const bool expectedDefaultCurrency = true;
            var expectedLabels = new[] { "words", "other words" };
            const string expectedCode = "code";
            const string expectedDescription = "Some words describing something";
            const string expectedSymbol = "$";
            var expectedVendorCategories = new List<VendorCategory>();

            var emptyGuid = Guid.Empty;

            var model = new Currency
            {
                Id = expectedCurrencyId,
                Labels = expectedLabels,
                Code = expectedCode,
                DefaultCurrency = expectedDefaultCurrency,
                Description = expectedDescription,
                Symbol = expectedSymbol,
                VendorCategories = expectedVendorCategories
            };
            var result = _mapper.Map<Currency, VendorCategory>(model);

            result.Should().NotBeNull();
            result.Id.Should().Be(emptyGuid);
            result.DefaultCurrencyId.Should().Be(result.DefaultCurrencyId);
            result.DefaultCurrencyId.Should().Be(expectedCurrencyId);
            result.Name.Should().BeNull();
            result.Currency.Should().BeNull();
        }

        [Test]
        public void Vendor_To_VendorModel_IsValid()
        {
            var expectedId = Guid.NewGuid();
            var expectedCreatedById = Guid.NewGuid();
            var expectedLabels = new[] { "words", "other words" };
            const string expectedName = "Name";
            const string expectedSapVendor = "SapVendor";
            var expectedModifiedDate = DateTime.Now;
            var expectedCreatedDate = DateTime.Now;
            var expectedVendorCategories = new List<VendorCategory>();

            var model = new Vendor
            {
                Id = expectedId,
                Labels = expectedLabels,
                Categories = expectedVendorCategories,
                Name = expectedName,
                SapVendor = expectedSapVendor,
                Modified = expectedModifiedDate,
                CreatedById = expectedCreatedById,
                Created = expectedCreatedDate

            };
            var result = _mapper.Map<Vendor, VendorModel>(model);

            result.Should().NotBeNull();
            result.Id.Should().Be(expectedId);
            result.Id.Should().Be(model.Id);
            result.Name.Should().Be(expectedName);
            result.Name.Should().Be(model.Name);
            result.VendorCategoryModels.Count.Should().Be(0);
        }

        [Test]
        public void Vendor_To_VendorModel_NoCategories_IsValid()
        {
            var expectedId = Guid.NewGuid();
            var expectedCreatedById = Guid.NewGuid();
            var expectedLabels = new[] { "words", "other words" };
            const string expectedName = "Name";
            const string expectedSapVendor = "SapVendor";
            var expectedModifiedDate = DateTime.Now;
            var expectedCreatedDate = DateTime.Now;

            var model = new Vendor
            {
                Id = expectedId,
                Labels = expectedLabels,
                Categories = null,
                Name = expectedName,
                SapVendor = expectedSapVendor,
                Modified = expectedModifiedDate,
                CreatedById = expectedCreatedById,
                Created = expectedCreatedDate
            };
            var result = _mapper.Map<Vendor, VendorModel>(model);

            result.Should().NotBeNull();
            result.Id.Should().Be(expectedId);
            result.Id.Should().Be(model.Id);
            result.Name.Should().Be(expectedName);
            result.Name.Should().Be(model.Name);
            result.VendorCategoryModels.Count.Should().Be(0);
        }

        /// <summary>
        ///     Saves the vendor model to vendor is valid.
        /// </summary>
        [Test]
        public void SaveVendorModel_To_Vendor_IsValid()
        {
            var expectedId = Guid.NewGuid();
            var expectedName = "Bradford";
            var expectedSapVendorCode = "SapCode";

            var model = new SaveVendorModel
            {
                Id = expectedId,
                Name = expectedName,
                SapVendorCode = expectedSapVendorCode
            };
            var result = _mapper.Map<SaveVendorModel, Vendor>(model);

            result.Should().NotBeNull();
            result.Id.Should().Be(model.Id);
            result.Id.Should().Be(expectedId);
            result.Name.Should().Be(expectedName);
            result.Name.Should().Be(model.Name);
        }

        [Test]
        public void VendorProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}