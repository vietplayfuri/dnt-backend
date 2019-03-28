
namespace costs.net.core.tests.Extensions
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;
    using core.Extensions;
    using FluentAssertions;

    [TestFixture]
    public class JObjectExtensionsTests
    {
        private dynamic content = new JObject();

        [SetUp]
        public void Setup()
        {
            content.projectMediaType = new JArray();
            content.projectMediaType.Add("Broadcast");
            content.name = "My Second Saatchi Project";
            content.projectDescription = "My Project Description";
            content.published = false;
            content.campaignDates = new JObject();
            content.campaignDates.start = "2017-05-31T23:00:00Z";
            content.campaignDates.end = "2018-06-26T23:00:00Z";
        }

        [Test]
        public void GetPropertyValue_ExactMatchPropertyName_MatchFound()
        {
            const string propertyName = "published";
            const string expected = "False";

            var target = new JObject(content);
            var result = target.GetPropertyValue(propertyName, false);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyValue_ExactMatchPropertyName_MatchNotFound()
        {
            const string propertyName = "campaign";
            const string expected = "";

            var target = new JObject(content);
            var result = target.GetPropertyValue(propertyName, false);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyValue_StartsWithMatchPropertyName_MatchFound()
        {
            const string propertyName = "projectDesc";
            const string expected = "My Project Description";

            var target = new JObject(content);
            var result = target.GetPropertyValue(propertyName, true);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyValue_StartsWithMatchPropertyName_MatchNotFound()
        {
            const string propertyName = "projectE";
            const string expected = "";

            var target = new JObject(content);
            var result = target.GetPropertyValue(propertyName, true);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyValue_ExactMatchPropertyName_IsArray_MatchFound()
        {
            const string propertyName = "projectMediaType";
            const string expected = "Broadcast";

            var target = new JObject(content);
            var result = target.GetPropertyValue(propertyName, false, true);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyValue_ExactMatchPropertyName_IsArray_MatchNotFound()
        {
            const string propertyName = "campaign";
            const string expected = "";

            var target = new JObject(content);
            var result = target.GetPropertyValue(propertyName, false, true);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyValue_StartsWithMatchPropertyName_IsArray_MatchFound()
        {
            const string propertyName = "projectMedia";
            const string expected = "Broadcast";

            var target = new JObject(content);
            var result = target.GetPropertyValue(propertyName, true, true);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyValue_StartsWithMatchPropertyName_IsArray_MatchNotFound()
        {
            const string propertyName = "projectContent";
            const string expected = "";

            var target = new JObject(content);
            var result = target.GetPropertyValue(propertyName, true, true);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }



        [Test]
        public void GetPropertyObject_ExactMatchPropertyName_MatchFound()
        {
            const string propertyName = "published";
            const string expected = "False";

            var target = new JObject(content);
            var result = target.GetPropertyObject<string>(propertyName, false);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyObject_ExactMatchPropertyName_MatchNotFound()
        {
            const string propertyName = "campaign";

            var target = new JObject(content);
            var result = target.GetPropertyObject<string>(propertyName, false);

            result.Should().BeNull();
        }

        [Test]
        public void GetPropertyObject_StartsWithMatchPropertyName_MatchFound()
        {
            const string propertyName = "projectDesc";
            const string expected = "My Project Description";

            var target = new JObject(content);
            var result = target.GetPropertyObject<string>(propertyName, true);

            result.Should().NotBeNull();
            result.Should().Be(expected);
        }

        [Test]
        public void GetPropertyObject_StartsWithMatchPropertyName_MatchNotFound()
        {
            const string propertyName = "projectE";

            var target = new JObject(content);
            var result = target.GetPropertyObject<string>(propertyName, true);

            result.Should().BeNull();
        }

        [Test]
        public void GetPropertyObject_PropertyName_ReturnsObject()
        {
            const string propertyName = "campaignDates";
            const string startDateExpected = "2017-05-31T23:00:00Z";
            const string endDateExpected = "2018-06-26T23:00:00Z";

            var target = new JObject(content);
            var result = target.GetPropertyObject<Dictionary<string,object>>(propertyName, true);

            result.Should().NotBeNull();
            result["start"].Should().NotBeNull();
            result["start"].Should().Be(startDateExpected);
            result["end"].Should().NotBeNull();
            result["end"].Should().Be(endDateExpected);
        }
    }
}
