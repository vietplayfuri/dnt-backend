
namespace costs.net.plugins.tests.PG.Services.Budget
{
    using System;
    using core.Models.Excel;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins.PG.Services.Budget;

    [TestFixture]
    public class BudgetFormPropertyValidatorTests
    {
        [Test]
        public void Null_ExcelProperties_Throw_Exception()
        {
            //Arrange
            ExcelProperties excelProperties = null;
            const string contentType = "Audio";
            const string production = "Full Production";

            var target = new BudgetFormPropertyValidator();

            //Act
            try
            {
                target.IsValid(excelProperties, contentType, production);
            }
            catch (ArgumentNullException)
            {
                return;
            }
            
            //Assert
            Assert.Fail(); //Should not reach here
        }

        [Test]
        public void Null_ContentType_Return_Failure()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Audio",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "Full Production"
            };

            const string contentType = null;
            const string production = Constants.ProductionType.FullProduction;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Empty_ContentType_Return_Failure()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Audio",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "Full Production"
            };

            var contentType = string.Empty;
            const string production = Constants.ProductionType.FullProduction;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void Unsupported_ContentType_Return_Failure()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Audio",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "Full Production"
            };

            var contentType = "Unsupported";
            const string production = Constants.ProductionType.FullProduction;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
        }

        [Test]
        public void AnyProduction_For_Audio_Return_Success()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Audio",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "Does not matter"
            };

            var contentType = Constants.ContentType.Audio;
            const string production = Constants.ProductionType.FullProduction;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void AnyProduction_For_Digital_Return_Success()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Digital",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "Does not matter"
            };

            var contentType = Constants.ContentType.Digital;
            const string production = Constants.ProductionType.FullProduction;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void AnyProduction_For_Photography_Return_Success()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Photography",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "Does not matter"
            };

            var contentType = Constants.ContentType.Photography;
            const string production = Constants.ProductionType.FullProduction;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void FullProduction_For_Video_Return_Success()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Video",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "Full Production"
            };

            var contentType = Constants.ContentType.Video;
            const string production = Constants.ProductionType.FullProduction;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void PostProduction_For_Video_Return_Success()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Video",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "post production only"
            };

            var contentType = Constants.ContentType.Video;
            const string production = Constants.ProductionType.PostProductionOnly;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }

        [Test]
        public void CGIAnimation_For_Video_Return_Success()
        {
            //Arrange
            var excelProperties = new ExcelProperties
            {
                [core.Constants.BudgetFormExcelPropertyNames.LookupGroup] = "LookupGroup",
                [core.Constants.BudgetFormExcelPropertyNames.ContentType] = "Video",
                [core.Constants.BudgetFormExcelPropertyNames.Production] = "post production only"
            };

            var contentType = Constants.ContentType.Video;
            const string production = Constants.ProductionType.CgiAnimation;

            var target = new BudgetFormPropertyValidator();

            //Act
            var result = target.IsValid(excelProperties, contentType, production);

            //Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
        }
    }
}
