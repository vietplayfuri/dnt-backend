namespace costs.net.core.tests.Mapping
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using core.Mapping;
    using dataAccess.Entity;
    using dataAccess.Extensions;
    using FluentAssertions;
    using NUnit.Framework;

    [TestFixture]
    internal class SupportingDocumentProfileTests
    {
        private IMapper _mapper;

        [SetUp]
        public void Init()
        {
            _mapper = new Mapper(new MapperConfiguration(c => c.AddProfile<SupportingDocumentProfile>()));
        }

        [Test]
        public void SupportingDocumentProfile_Configuration_IsValid()
        {
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Test]
        public void SupportingDocument_To_SupportingDocument_COPY_IsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const int expectedSupportingDocRevisions = 0;
            var supportingDocRevision = new SupportingDocumentRevision
            {
                Id = Guid.NewGuid(),
                FileName = "FileName",
                GdnId = "ID"
            };
            supportingDocRevision.SetCreatedNow(userId);
            var supportingDocument = new SupportingDocument
            {
                Id = Guid.NewGuid(),
                Key = "P&G",
                Name = "PG agency name",
                CostStageRevisionId = Guid.NewGuid(),
                Generated = true,
                Required = true,
                SupportingDocumentRevisions = new List<SupportingDocumentRevision>
                {
                    supportingDocRevision
                }
            };
            supportingDocument.SetCreatedNow(userId);
            // Act
            var model = _mapper.Map<SupportingDocument>(supportingDocument);

            // Assert
            model.Id.Should().Be(Guid.Empty);
            model.Key.Should().Be(model.Key);
            model.Name.Should().Be(model.Name);
            model.CanManuallyUpload.Should().Be(model.CanManuallyUpload);
            model.Generated.Should().Be(model.Generated);
            model.Required.Should().Be(model.Required);
            model.SupportingDocumentRevisions.Count.Should().Be(expectedSupportingDocRevisions);
        }  [Test]
        public void SupportingDocumentRevision_To_SupportingDocumentRevision_COPY_IsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var supportingDocRevision = new SupportingDocumentRevision
            {
                Id = Guid.NewGuid(),
                FileName = "FileName",
                GdnId = "ID"
            };
            supportingDocRevision.SetCreatedNow(userId);

            // Act
            var model = _mapper.Map<SupportingDocumentRevision>(supportingDocRevision);

            // Assert
            model.Id.Should().Be(Guid.Empty);
            model.FileName.Should().Be(supportingDocRevision.FileName);
            model.CreatedById.Should().Be(supportingDocRevision.CreatedById);
            model.Created.Should().Be(supportingDocRevision.Created);
            model.GdnId.Should().Be(supportingDocRevision.GdnId);

        }
    }
}