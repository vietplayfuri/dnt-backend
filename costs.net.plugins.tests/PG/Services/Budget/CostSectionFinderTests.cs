
namespace costs.net.plugins.tests.PG.Services.Budget
{
    using System;
    using System.Collections.Generic;
    using core.Models.CostTemplate;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins.PG.Services.Budget;

    [TestFixture]
    public class CostSectionFinderTests
    {
        [Test]
        public void Null_Template_Throws_Argument_Error()
        {
            //Arrange
            CostTemplateVersionModel templateModel = null;
            var contentType = Constants.ContentType.Audio;
            var production = Constants.ProductionType.FullProduction;

            var target = new CostSectionFinder();
            
            //Act
            try
            {
                target.GetCostSection(templateModel, contentType, production);
            }
            catch (ArgumentNullException)
            {
                return;
            }

            //Assert
            Assert.Fail();
        }

        [Test]
        public void Null_ProductionDetails_Throws_Argument_Error()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = Constants.ContentType.Audio;
            var production = Constants.ProductionType.FullProduction;
            templateModel.ProductionDetails = null;

            var target = new CostSectionFinder();

            //Act
            try
            {
                target.GetCostSection(templateModel, contentType, production);
            }
            catch (ArgumentNullException)
            {
                return;
            }

            //Assert
            Assert.Fail();
        }

        [Test]
        public void Empty_ContentType_Return_Failure()
        {
            //Arrange
            var templateModel = new CostTemplateVersionModel
            {
                ProductionDetails = new List<ProductionDetailsTemplateModel>()
            };
            var contentType = string.Empty;
            const string production = Constants.ProductionType.FullProduction;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Unsupported_ContentType_Return_Failure()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = "Unsupported";
            const string production = Constants.ProductionType.FullProduction;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void AnyProduction_For_Audio_Return_Success()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = Constants.ContentType.Audio;
            const string production = Constants.ProductionType.FullProduction;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void FullProduction_For_Digital_Return_Success()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = Constants.ContentType.Digital;
            const string production = Constants.ProductionType.FullProduction;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void NotApplicableProduction_For_Digital_Return_Success()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = Constants.ContentType.Digital;
            const string production = Constants.Miscellaneous.NotApplicable;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void AnyProduction_For_Photography_Return_Success()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = Constants.ContentType.Photography;
            const string production = Constants.ProductionType.FullProduction;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void FullProduction_For_Video_Return_Success()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = Constants.ContentType.Video;
            const string production = Constants.ProductionType.FullProduction;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void PostProduction_For_Video_Return_Success()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = Constants.ContentType.Video;
            const string production = Constants.ProductionType.PostProductionOnly;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void CGIAnimation_For_Video_Return_Success()
        {
            //Arrange
            var templateModel = CostFormTestHelper.CreateTemplateModel();
            var contentType = Constants.ContentType.Video;
            const string production = Constants.ProductionType.CgiAnimation;

            var target = new CostSectionFinder();

            //Act
            var result = target.GetCostSection(templateModel, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }
        
    }
}
