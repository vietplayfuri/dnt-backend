
namespace costs.net.plugins.tests.PG.Extensions
{
    using dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins.PG.Extensions;
    using plugins.PG.Form;

    [TestFixture]
    public class FormExtensionsTests
    {
        [Test]
        public void EmptyForm_GetContentType()
        {
            //Arrange
            var target = new PgStageDetailsForm();

            //Act
            var result = target.GetContentType();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void CostType_Buyout_GetContentType()
        {
            //Arrange
            string expectedContentType = string.Empty;
            var target = new PgStageDetailsForm
            {
                CostType = CostType.Buyout.ToString()
            };

            //Act
            var result = target.GetContentType();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedContentType);
        }

        [Test]
        public void CostType_Production_GetContentType()
        {
            //Arrange
            const string expectedContentType = "Abc";
            var target = new PgStageDetailsForm
            {
                CostType = CostType.Production.ToString(),
                ContentType = new core.Builders.DictionaryValue
                {
                    Key = expectedContentType,
                    Value = "A Value"
                }
            };

            //Act
            var result = target.GetContentType();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedContentType);
        }

        [Test]
        public void CostType_Trafficking_GetContentType()
        {
            //Arrange
            var target = new PgStageDetailsForm { CostType = CostType.Trafficking.ToString() };

            //Act
            var result = target.GetContentType();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void EmptyForm_GetProductionType()
        {
            //Arrange
            var target = new PgStageDetailsForm();

            //Act
            var result = target.GetProductionType();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void CostType_Buyout_GetProductionType()
        {
            //Arrange
            const string expectedProductionType = "Abc";
            var target = new PgStageDetailsForm
            {
                CostType = CostType.Buyout.ToString(),
                UsageBuyoutType = new core.Builders.DictionaryValue
                {
                    Key = expectedProductionType
                }
            };

            //Act
            var result = target.GetProductionType();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedProductionType);
        }

        [Test]
        public void CostType_Production_GetProductionType()
        {
            //Arrange
            const string expectedProductionType = "Abc";
            var target = new PgStageDetailsForm
            {
                CostType = CostType.Production.ToString(),
                ProductionType = new core.Builders.DictionaryValue
                {
                    Key = expectedProductionType
                }
            };

            //Act
            var result = target.GetProductionType();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedProductionType);
        }

        [Test]
        public void CostType_Trafficking_GetProductionType()
        {
            //Arrange
            var target = new PgStageDetailsForm
            {
                CostType = CostType.Trafficking.ToString()
            };

            //Act
            var result = target.GetProductionType();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public void ContentType_Digital_GetProductionType()
        {
            //Arrange
            const string expected = "N/A";
            var target = new PgStageDetailsForm
            {
                CostType = CostType.Trafficking.ToString(),
                ContentType = new core.Builders.DictionaryValue
                {
                    Key = Constants.ContentType.Digital
                }
            };

            //Act
            var result = target.GetProductionType();

            //Assert
            result.Should().NotBeNull();
            result.Should().Be(expected);
        }
    }
}
