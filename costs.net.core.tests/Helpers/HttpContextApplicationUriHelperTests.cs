using costs.net.core.Helpers;
using costs.net.core.Models.Utils;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;

namespace costs.net.core.tests.Helpers
{
    [TestFixture]
    public class HttpContextApplicationUriHelperTests
    {
        private const string FrontEndUrl = "http://adstream.com:8882/";
        private Mock<IOptions<AppSettings>> _appSettingsMock;

        [SetUp]
        public void Setup()
        {
            _appSettingsMock = new Mock<IOptions<AppSettings>>();
            _appSettingsMock.Setup(s => s.Value).Returns(new AppSettings { FrontendUrl = FrontEndUrl });
        }
        
        [Test]
        public void Null_Id_GetLink_Succeeds()
        {
            //Arrange
            ApplicationUriName resourceName = ApplicationUriName.Cost;
            string id = null;
            var target = new HttpContextApplicationUriHelper();

            //Act
            string result = target.GetLink(resourceName, id);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void Cost_With_Id_GetLink()
        {
            //Arrange
            ApplicationUriName resourceName = ApplicationUriName.Cost;
            const string id = "cost-id";
            var target = new HttpContextApplicationUriHelper();
            const string expected = "/costs/#/costs/cost-details/cost-id";

            //Act
            string result = target.GetLink(resourceName, id);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CostRevisionReview_With_RevisionId_And_CostId_GetLink()
        {
            //Arrange
            ApplicationUriName resourceName = ApplicationUriName.CostRevisionReview;
            const string costId = "cost-id";
            const string revisionId = "revision-id";
            var target = new HttpContextApplicationUriHelper();
            const string expected = "/costs/#/costs/items/review?revisionId=revision-id&costId=cost-id";

            //Act
            string result = target.GetLink(resourceName, revisionId, costId);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result);
        }
    }
}
