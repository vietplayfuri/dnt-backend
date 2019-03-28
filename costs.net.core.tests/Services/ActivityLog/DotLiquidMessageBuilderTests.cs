
namespace costs.net.core.tests.Services.ActivityLog
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Builders.ActivityLog;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class DotLiquidMessageBuilderTests
    {
        private readonly Mock<EFContext> _efContextMock = new Mock<EFContext>();
        private readonly List<ActivityLogMessageTemplate> _templates = new List<ActivityLogMessageTemplate>();
        private DotLiquidMessageBuilder _target;

        [SetUp]
        public void Init()
        {
            _efContextMock.MockAsyncQueryable(_templates.AsQueryable(), d => d.ActivityLogMessageTemplate);

            _target = new DotLiquidMessageBuilder(_efContextMock.Object);
        }

        [Test]
        public void Null_Log_Returns_Empty_Message()
        {
            //Arrange
            ActivityLog log = null;

            //Act
            var result = _target.Build(log);

            //Assert
            result.Should().NotBeNull();
            result.Message.Should().BeNull();
        }

        [Test]
        public void Log_WithMissingTemplate_Returns_Empty_Message()
        {
            //Arrange
            ActivityLog log = new ActivityLog
            {
                ActivityLogType = ActivityLogType.CostCreated
            };
            _templates.Clear(); //Clear the templates provided by EFContext

            //Act
            var result = _target.Build(log);

            //Assert
            result.Should().NotBeNull();
            result.Message.Should().BeNull();
        }

        [Test]
        public void Log_WithInvalidTemplate_Returns_Empty_Message()
        {
            //Arrange
            ActivityLog log = new ActivityLog
            {
                ActivityLogType = ActivityLogType.CostCreated
            };
            _templates.Clear(); //Clear the templates provided by EFContext
            _templates.Add(new ActivityLogMessageTemplate
            {
                ActivityLogType = ActivityLogType.CostCreated,
                Id = 1,
                Template = "Not a valid {{ template."
            });

            //Act
            var result = _target.Build(log);

            //Assert
            result.Should().NotBeNull();
            result.Message.Should().BeNull();
        }

        [Test]
        public void Log_WithValidTemplate_Returns_Json_Message()
        {
            //Arrange
            var costUserId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var costUser = new CostUser
            {
                Email = "costs.admin@adstream.com",
                Id = costUserId
            };
            var data = new Dictionary<string, object>();
            data[Constants.ActivityLogData.CostId] = costId;
            ActivityLog log = new ActivityLog
            {
                ActivityLogType = ActivityLogType.CostCreated,
                IpAddress = "127.0.0.1",
                Data = JsonConvert.SerializeObject(data),
                Timestamp = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CostUserId = costUserId,
                CostUser = costUser
            };
            _templates.Clear(); //Clear the templates provided by EFContext
            _templates.Add(new ActivityLogMessageTemplate
            {
                ActivityLogType = ActivityLogType.CostCreated,
                Id = 1,
                Template = "{\"timestamp\":\"{{ timestamp | date: \"yyyy-MM-ddTHH:mm:ss.fffZ\" }}\",\"id\":\"{{ messageId }}\",\"type\":\"costCreated\",\"object\":{\"id\":\"{{ objectId }}\",\"type\":\"activity\", \"message\":\"cost ''{{ costId | escape }}'' has been created\", \"costId\":\"{{ costId || escape }}\"},\"subject\":{\"id\":\"{{ subjectId }}\",\"application\":\"adcosts\"}}"
            });

            //Act
            var result = _target.Build(log);

            //Assert
            result.Should().NotBeNull();
            result.Message.Should().NotBeNull();
            var resultJson = JsonConvert.DeserializeObject(result.Message);
            resultJson.Should().NotBeNull();
        }

        /// <summary>
        /// This test case is cover SPB-2527
        /// </summary>
        [Test]
        public void BuildLog_CostRejectedWithReason_WithDoubleQuote_Returns_Json_WithSingleQuote()
        {
            //Arrange
            var costUserId = Guid.NewGuid();
            var costId = Guid.NewGuid();
            var approvalUsername = "zermeno.ma@pg.com";
            var costUser = new CostUser
            {
                Email = "costs.admin@adstream.com",
                Id = costUserId
            };
            var data = new Dictionary<string, object>();
            data[Constants.ActivityLogData.CostId] = costId;
            data[Constants.ActivityLogData.RejectionComment] = "Please list under \"Expected Assets/Deliverable\" all executions included in this production,  Right now, the AdCost lists only ONE deliverable, which is incomplete.";
            data[Constants.ActivityLogData.ApprovalUsername] = approvalUsername;

            ActivityLog log = new ActivityLog
            {
                ActivityLogType = ActivityLogType.CostRejectedWithReason,
                IpAddress = "127.0.0.1",
                Data = JsonConvert.SerializeObject(data),
                Timestamp = DateTime.UtcNow,
                Created = DateTime.UtcNow,
                CostUserId = costUserId,
                CostUser = costUser
            };
            _templates.Clear(); //Clear the templates provided by EFContext
            _templates.Add(new ActivityLogMessageTemplate
            {
                ActivityLogType = ActivityLogType.CostRejectedWithReason,
                Id = 1,
                Template = "{\"timestamp\":\"{{ timestamp | date: \"yyyy-MM-ddTHH:mm:ss.fffZ\" }}\",\"id\":\"{{ messageId }}\", \"type\":\"costRejectedWithReason\",\"object\":{\"id\":\"{{ objectId }}\",\"type\":\"activity\", \"message\":\"Cost '{{ costId }}' rejected by '{{ approvalUsername | escape }}' for the following reason: {{ rejectionComment}}\" {% for kv in data %},\"{{ kv.key }}\": {% if kv.objectType == 'string' %} \"{{ kv.value | escape }}\" {% elsif kv.objectType == 'datetime' %} \"{{ kv.value | date: \"yyyy-MM-ddTHH:mm:ss.fffZ\" }}\" {% else %} \"{{ kv.value }}\" {% endif %} {% endfor %}},\"subject\":{ \"id\":\"{{ subjectId }}\",\"application\":\"adcosts\"}}"
            });

            //Act
            var result = _target.Build(log);

            //Assert
            result.Should().NotBeNull();
            result.Message.Should().NotBeNull();
            var resultJson = JsonConvert.DeserializeObject(result.Message);
            resultJson.Should().NotBeNull(); //This line will be error if we don't convert from double quote to single quote

            var jObject = JObject.Parse(resultJson.ToString());
            var t1 = jObject.Root["object"]["message"];
            jObject.Root["object"]["message"].ToString().Should().Equals($"Cost '{costId}' rejected by '{approvalUsername}' for the following reason: Please list under 'Expected Assets/Deliverable' all executions included in this production,  Right now, the AdCost lists only ONE deliverable, which is incomplete.");
        }
    }
}
