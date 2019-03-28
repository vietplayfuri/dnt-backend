
namespace costs.net.core.tests.Services.CostTemplate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using core.Mapping;
    using core.Services.PerDiems;
    using costs.net.core.Models.CostTemplate;
    using costs.net.core.Services.CostTemplate;
    using dataAccess;
    using dataAccess.Entity;
    using FluentAssertions;
    using Moq;
    using net.tests.common.Stubs.EFContext;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;


    public class CostTemplateServiceTests
    {
        [TestFixture]
        public abstract class CostTemplateServiceTest
        {
            protected CostTemplateService _costTemplateService;
            protected Mock<EFContext> _efContextMock;
            protected Mock<ICostTemplateVersionService> _costTemplateVersionServiceMock;
            protected Guid _costTemplateVersionId;
            protected const string _productionCost = "Production Cost";

            /// <summary>
            /// This is data of template for stage detail - Production Cost
            /// select cf.* from custom_form_data cf
            /// join cost_template ct on cf.id = ct.field_definitions_id
            /// join cost_template_version ctv on ct.id = ctv.cost_template_id
            /// where ctv."name" = 'Production Cost';
            /// </summary>
            protected const string _productionTemplate = "[{\"name\": \"agencyName\", \"type\": \"string\", \"label\": \"Agency Name\"}, {\"name\": \"agencyLocation\", \"type\": \"string\", \"label\": \"Agency Location\"}, {\"name\": \"agencyProducerArtBuyer\", \"type\": \"string\", \"label\": \"Agency producer/art buyer\"}, {\"name\": \"budgetRegion\", \"type\": \"string\", \"label\": \"Budget Region\"}, {\"name\": \"brand\", \"type\": \"string\", \"label\": \"Campaign\"}, {\"name\": \"contentType\", \"type\": \"string\", \"label\": \"Content Type\"}, {\"name\": \"productionType\", \"type\": \"string\", \"label\": \"Production Type\"}, {\"name\": \"targetBudget\", \"type\": \"number\", \"label\": \"Target Budget\", \"mandatory\": true}, {\"name\": \"contentType\", \"type\": \"string\", \"label\": \"Agency Tracking Number\"}, {\"name\": \"organisation\", \"type\": \"string\", \"label\": \"Organisation\"}, {\"name\": \"agencyCurrency\", \"type\": \"string\", \"label\": \"Agency Payment Currency\"}]";

            /// <summary>
            /// This is data of template for production details - Buyout Cost
            /// select cf.* from custom_form_data cf
            /// join cost_template ct on cf.id = ct.field_definitions_id
            /// join cost_template_version ctv on ct.id = ctv.cost_template_id
            /// where ctv."name" = 'Buyout Cost';
            /// </summary>
            protected const string _buyoutTemplate = "[{\"name\": \"agencyName\", \"type\": \"string\", \"label\": \"Agency Name\"}, {\"name\": \"agencyLocation\", \"type\": \"string\", \"label\": \"Agency Location\"}, {\"name\": \"agencyProducerArtBuyer\", \"type\": \"string\", \"label\": \"Agency producer/art buyer\"}, {\"name\": \"budgetRegion\", \"type\": \"string\", \"label\": \"Budget Region\"}, {\"name\": \"brand\", \"type\": \"string\", \"label\": \"Campaign\"}, {\"name\": \"contentType\", \"type\": \"string\", \"label\": \"Content Type\"}, {\"name\": \"productionType\", \"type\": \"string\", \"label\": \"Production Type\"}, {\"name\": \"targetBudget\", \"type\": \"number\", \"label\": \"Target Budget\"}, {\"name\": \"contentType\", \"type\": \"string\", \"label\": \"Agency Tracking Number\"}, {\"name\": \"organisation\", \"type\": \"string\", \"label\": \"Organisation\"}, {\"name\": \"agencyCurrency\", \"type\": \"string\", \"label\": \"Agency Payment Currency\"}]";

            /// <summary>
            /// This is data of template for production details - Trafficking/Distribution Cost
            /// select cf.* from custom_form_data cf 
            /// join cost_template ct on cf.id = ct.field_definitions_id
            /// join cost_template_version ctv on ct.id = ctv.cost_template_id
            /// where ctv."name" = 'Trafficking/Distribution Cost';
            /// </summary>
            protected const string _traffickingTemplate = "[{\"name\": \"agencyName\", \"type\": \"string\", \"label\": \"Agency Name\"}, {\"name\": \"agencyLocation\", \"type\": \"string\", \"label\": \"Agency Location\"}, {\"name\": \"agencyProducerArtBuyer\", \"type\": \"string\", \"label\": \"Agency producer/art buyer\"}, {\"name\": \"budgetRegion\", \"type\": \"string\", \"label\": \"Budget Region\"}, {\"name\": \"targetBudget\", \"type\": \"number\", \"label\": \"Target Budget\"}, {\"name\": \"contentType\", \"type\": \"string\", \"label\": \"Agency Tracking Number\"}, {\"name\": \"organisation\", \"type\": \"string\", \"label\": \"Organisation\"}, {\"name\": \"agencyCurrency\", \"type\": \"string\", \"label\": \"Agency Currency\"}]";

            [SetUp]
            public void Init()
            {
                _efContextMock = new Mock<EFContext>();
                _costTemplateVersionServiceMock = new Mock<ICostTemplateVersionService>();
                _costTemplateService = new CostTemplateService(
                    _efContextMock.Object,
                    _costTemplateVersionServiceMock.Object);
            }
        }

        [TestFixture]
        public class ValidateCostTemplate : CostTemplateServiceTest
        {
            [Test]
            [TestCase("{\"initialBudget\": 0, \"agencyProducer\": []}", false)] /* Bad data of SPB-2051, SPB-2089, SPB-2100, SPB-2141 */
            [TestCase("{\"smoId\": null, \"title\": \"Lenor Unstoppables Beard\", \"isAIPE\": false, \"campaign\": \"Lenor Unstoppables Beard\", \"costType\": \"Production\", \"projectId\": \"5b9a8b0bb9fc661a9d780f45\", \"costNumber\": \"PRO0003465V0001\", \"contentType\": {\"id\": \"d64450c1-8a27-4b31-bb5f-1f9240597be9\", \"key\": \"Video\", \"value\": \"Video\", \"created\": \"2018-02-14T15:47:15.121647\", \"visible\": true, \"modified\": \"2018-02-14T15:47:15.121646\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"f2a40aba-5066-4a8b-b87a-7e830224dd0f\"}, \"description\": \"Lenor Unstoppables Beard TVC\", \"budgetRegion\": {\"id\": \"57eac257-a217-4660-be7f-53cca79f68f4\", \"key\": \"EUROPE AREA\", \"name\": \"Europe\"}, \"organisation\": {\"id\": \"8167dd41-671e-4d66-9949-c7a3d52c5e4a\", \"key\": \"RBU\", \"value\": \"RBU\", \"created\": \"2018-02-14T15:47:15.134188\", \"visible\": true, \"modified\": \"2018-02-14T15:47:15.134187\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"c81e485e-10ab-423f-9597-0959430d3be6\"}, \"initialBudget\": 221591, \"agencyCurrency\": \"GBP\", \"agencyProducer\": [\"Charlotte Wilson\"], \"productionType\": {\"id\": \"38f2c69d-d0d2-48a5-a8e6-96d52383b4fb\", \"key\": \"Full Production\", \"value\": \"Full Production\"}, \"IsCurrencyChanged\": false, \"agencyTrackingNumber\": \"105227836\"}", true)] /* Correct data of SPB-2089 */
            [TestCase("{\"campaign\": \"N/A\", \"projectId\": \"5ac389a3b9fc666ccf84f139\", \"costNumber\": \"PRO0383T0000015\", \"agencyProducer\": [\"Dennis Marchesiello\"], \"airInsertionDate\": \"2018-04-01T04:00:00Z\", \"agencyTrackingNumber\": \"B1192-004455-00\"}", false)] /* Bad data of SPB-2094 */
            [TestCase("{\"smoId\": \"cec14a6d-168b-463f-8a8f-32d0c0b4b15c\", \"title\": \"Q4 Tide Dupes\", \"isAIPE\": false, \"smoName\": \"UNITED STATES GROUP\", \"campaign\": \"N/A\", \"costType\": \"Trafficking\", \"projectId\": \"5ac389a3b9fc666ccf84f139\", \"description\": \"dupe materials fourth qtr, Tide\", \"budgetRegion\": {\"id\": \"af2fb04a-f1c0-49c9-903b-f4b70f3a8d41\", \"key\": \"NORTHERN AMERICA AREA\", \"name\": \"North America\"}, \"organisation\": {\"id\": \"ab55bbc4-81cc-4930-a909-47605a928a2e\", \"key\": \"SMO\", \"value\": \"SMO\", \"created\": \"2018-02-14T15:47:15.13419\", \"visible\": true, \"modified\": \"2018-02-14T15:47:15.134189\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"c81e485e-10ab-423f-9597-0959430d3be6\"}, \"approvalStage\": \"OriginalEstimate\", \"initialBudget\": 5000, \"agencyCurrency\": \"USD\", \"agencyProducer\": [\"Amy Salzman (Saatchi & Saatchi)\"], \"airInsertionDate\": \"2018-04-01T04:00:00Z\", \"IsCurrencyChanged\": false, \"agencyTrackingNumber\": \"B119B-000070-00\"}", true)]/* Correct data of SPB-2094 */
            public async Task IsValidateFormDetailData_StageDetails(string jsonData, bool expected)
            {
                //Cost Production - missing stage details
                var costId = Guid.NewGuid();

                var cost = new Cost
                {
                    Id = costId,
                    CostType = dataAccess.Entity.CostType.Production,
                    LatestCostStageRevision = new CostStageRevision
                    {
                        StageDetails = new CustomFormData
                        {
                            Id = Guid.NewGuid(),
                            Data = "{\"smoId\": \"747e9209-d22f-423c-9309-7d075edbeed5\", \"title\": \"Power of Softness Superheroes - 20'' Cut Down\", \"isAIPE\": false, \"smoName\": \"CENTRAL EUROPE\", \"campaign\": \"Power of Softness\", \"costType\": \"Production\", \"projectId\": \"5bb346adb9fc665cfaf3676a\", \"costNumber\": \"PRO0003756V0001\", \"contentType\": {\"id\": \"d64450c1-8a27-4b31-bb5f-1f9240597be9\", \"key\": \"Video\", \"value\": \"Video\", \"created\": \"2018-02-14T15:47:15.121647\", \"visible\": true, \"modified\": \"2018-02-14T15:47:15.121646\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"f2a40aba-5066-4a8b-b87a-7e830224dd0f\"}, \"description\": \"Power of Softness Superheroes - 20'' Cut Down from Original 30'' Film\", \"budgetRegion\": {\"id\": \"57eac257-a217-4660-be7f-53cca79f68f4\", \"key\": \"EUROPE AREA\", \"name\": \"Europe\"}, \"organisation\": {\"id\": \"ab55bbc4-81cc-4930-a909-47605a928a2e\", \"key\": \"SMO\", \"value\": \"SMO\", \"created\": \"2018-02-14T15:47:15.13419\", \"visible\": true, \"modified\": \"2018-02-14T15:47:15.134189\", \"projects\": null, \"createdById\": \"77681eb0-fc0d-44cf-83a0-36d51851e9ae\", \"dictionaryId\": \"c81e485e-10ab-423f-9597-0959430d3be6\"}, \"initialBudget\": 8000, \"agencyCurrency\": \"GBP\", \"agencyProducer\": [\"Savannah King\"], \"productionType\": {\"id\": \"a6134764-c8a7-42ba-acb6-5cbbb82d7477\", \"key\": \"Post Production Only\", \"value\": \"Post Production Only\"}, \"IsCurrencyChanged\": false, \"agencyTrackingNumber\": \"TBC\"}"
                        }
                    },
                    CostTemplateVersionId = _costTemplateVersionId
                };

                _efContextMock.MockAsyncQueryable(new[] { cost }.AsQueryable(), d => d.Cost)
                    .Setup(c => c.FindAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(cost);
                var costModel = new CostTemplateVersionModel { Cost = new CostTemplateModel { } };

                costModel.Cost.Fields = JsonConvert.DeserializeObject<List<FormFieldDefintionModel>>(_productionTemplate);

                _costTemplateVersionServiceMock.Setup(c => c.GetCostTemplateVersionModel(It.IsAny<Guid>()))
                    .ReturnsAsync(costModel);

                // Act
                var isValid = await _costTemplateService.IsValidateFormDetailData(costId, JObject.Parse(jsonData).ToObject<Dictionary<string, dynamic>>());

                // Assert
                isValid.Should().Be(expected);
            }
        }
    }
}
