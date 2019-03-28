namespace costs.net.integration.tests.CostTemplate
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.ACL;
    using core.Models.CostTemplate;
    using FluentAssertions;
    using NUnit.Framework;

    public class CostTemplateIntegrationTests
    {
        public abstract class CostTemplateIntegrationTest : BaseIntegrationTest
        {
            protected CreateCostTemplateModel GetCreateCostTemplateModel()
            {
                var model = new CreateCostTemplateModel
                {
                    Name = "Production Cost",
                    CostDetails = new CostTemplateModel
                    {
                        Name = "cost",
                        Label = "Cost Details",
                        Fields = new List<FormFieldDefintionModel>
                        {
                            new FormFieldDefintionModel
                            {
                                Label = "Cost Number",
                                Name = "costNumber",
                                Type = "string"
                            }
                        }
                    },
                    ProductionDetails =
                        new[]
                        {
                        new ProductionDetailsTemplateModel
                        {
                            Type = "video",
                            Forms = new List<ProductionDetailsFormDefinitionModel>
                            {
                                new ProductionDetailsFormDefinitionModel
                                {
                                    Name = "fullProductionWithShoot",
                                    Label = "Full Production",
                                    Fields = new List<FormFieldDefintionModel>
                                    {
                                        new FormFieldDefintionModel
                                        {
                                            Label = "Shoot Date",
                                            Name = "shootDate",
                                            Type = "string"
                                        }
                                    },
                                    CostLineItemSections = new List<CostLineItemSectionTemplateModel>
                                    {
                                        new CostLineItemSectionTemplateModel
                                        {
                                            Name = "production",
                                            Label = "Production",
                                            Items = new List<CostLineItemSectionTemplateItemModel>
                                            {
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Art Department (Location /studio)",
                                                    Name = "artDepartmentStudio"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Art Department (set dressing)",
                                                    Name = "artDepartmentSetDressing"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Casting",
                                                    Name = "casting"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Crew costs",
                                                    Name = "crewCost"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Equipment",
                                                    Name = "equiment"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Photographer",
                                                    Name = "photographer"
                                                }
                                            }
                                        },
                                        new CostLineItemSectionTemplateModel
                                        {
                                            Name = "postProduction",
                                            Label = "Post Production",
                                            Items = new List<CostLineItemSectionTemplateItemModel>
                                            {
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Retouching",
                                                    Name = "retouching"
                                                }
                                            }
                                        },
                                        new CostLineItemSectionTemplateModel
                                        {
                                            Name = "agencyCosts",
                                            Label = "Agency Costs",
                                            Items = new List<CostLineItemSectionTemplateItemModel>
                                            {
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Foreign exchange loss/gain",
                                                    Name = "foreignExchangeLossGain"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Insurance",
                                                    Name = "insurance"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Offline costs",
                                                    Name = "offlineCosts"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Online costs",
                                                    Name = "offlineCosts"
                                                },
                                                new CostLineItemSectionTemplateItemModel
                                                {
                                                    Label = "Tax/importation tax",
                                                    Name = "taxImportationTax"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        }
                };
                return model;
            }
        }

        [TestFixture]
        public class CreateCostTemplateShould : CostTemplateIntegrationTest
        {
            [Test]
            public async Task CreateCostTemplate()
            {
                var user = await CreateUser($"{Guid.NewGuid()}User1", Roles.ClientAdmin);

                var model = GetCreateCostTemplateModel();

                var createResult = await Browser.Post("/v1/costtemplate/create", with =>
                {
                    with.User(user);
                    with.JsonBody(model);
                });

                var costTemplate = Deserialize<CostTemplateDetailsModel>(createResult, HttpStatusCode.Created);

                costTemplate.Id.Should().NotBe(Guid.Empty);

                var getResult = await Browser.Get($"/v1/costtemplate/{costTemplate.Id}/versions/latest", w => w.User(user));

                var latestVersion = Deserialize<CostTemplateVersionModel>(getResult, HttpStatusCode.OK);

                latestVersion.Should().NotBeNull();

                var getFullResult = await Browser.Get($"/v1/costtemplate/{costTemplate.Id}", w => w.User(user));

                var fullTemplate = Deserialize<CostTemplateDetailsModel>(getFullResult, HttpStatusCode.OK);

                fullTemplate.LatestVersionId.Should().Be(latestVersion.Id);

                fullTemplate.Versions[0].Name.Should().Be(model.Name);
            }

            [Test]
            public async Task CreateCostTemplateWithCustomForms()
            {
                var user = await CreateUser($"{Guid.NewGuid()}User1", Roles.ClientAdmin);

                var model = GetCreateCostTemplateModel();
                var formModel = new FormDefinitionModel
                {
                    Name = "Test",
                    Label = "test",
                    Fields = new List<FormFieldDefintionModel>
                    {
                        new FormFieldDefintionModel
                        {
                            Label = "Test field one",
                            Name = "testFieldOne",
                            Type = "string"
                        }
                    },
                    Sections = new List<FormSectionDefinitionModel>
                    {
                        new FormSectionDefinitionModel
                        {
                            Name = "Section",
                            Label = "section",
                            Fields = new List<FormFieldDefintionModel>
                            {
                                new FormFieldDefintionModel
                                {
                                    Label = "Section field one",
                                    Name = "sectionFieldOne",
                                    Type = "string"
                                },
                                new FormFieldDefintionModel
                                {
                                    Label = "Section field two",
                                    Name = "sectionFieldTwo",
                                    Type = "number"
                                }
                            }
                        }
                    }
                };
                model.Forms = new List<FormDefinitionModel> { formModel };

                var costTemplate = Deserialize<CostTemplateDetailsModel>(await Browser.Post("/v1/costtemplate/create", with =>
                {
                    with.User(user);
                    with.JsonBody(model);
                }), HttpStatusCode.Created);

                costTemplate.Id.Should().NotBe(Guid.Empty);

                costTemplate = Deserialize<CostTemplateDetailsModel>(
                    await Browser.Get($"/v1/costtemplate/{costTemplate.Id}", w => w.User(user)), 
                    HttpStatusCode.OK);

                var version = costTemplate.GetLatestVersion();

                version.Should().NotBeNull();
                version.Forms.Should().NotBeNull();
                version.Forms.Should().HaveCount(1);

                var form = version.Forms.First();
                form.Name.Should().Be(formModel.Name);
                form.Label.Should().Be(formModel.Label);

                form.Fields.Should().NotBeNull();
                form.Fields.Should().HaveCount(1);
                form.Sections.Should().NotBeNull();
                form.Sections.Should().HaveCount(1);

                var field = form.Fields.First();
                field.Name.Should().Be(formModel.Fields[0].Name);
                field.Label.Should().Be(formModel.Fields[0].Label);
                field.Type.Should().Be(formModel.Fields[0].Type);

                var sectionModel = formModel.Sections[0];
                var section = form.Sections.First();
                section.Name.Should().Be(sectionModel.Name);
                section.Label.Should().Be(sectionModel.Label);
                section.Fields.Should().NotBeNull();
                section.Fields.Should().HaveCount(sectionModel.Fields.Count);

                var i = 0;
                foreach (var sectionField in section.Fields)
                {
                    var sectionFieldModel = sectionModel.Fields[i++];
                    sectionField.Name.Should().Be(sectionFieldModel.Name);
                    sectionField.Label.Should().Be(sectionFieldModel.Label);
                    sectionField.Type.Should().Be(sectionFieldModel.Type);
                }
            }
        }

        [TestFixture]
        public class UpdateCostTemplateShould : CostTemplateIntegrationTest
        {
            [Test]
            public async Task UpdateCostTemplate()
            {
                var user = await CreateUser($"{Guid.NewGuid()}User1", Roles.ClientAdmin);

                var model = GetCreateCostTemplateModel();

                var createResult = await Browser.Post("/v1/costtemplate/create", with =>
                {
                    with.User(user);
                    with.JsonBody(model);
                });

                var createdCost = Deserialize<CostTemplateDetailsModel>(createResult, HttpStatusCode.Created);

                var updateModel = model;

                updateModel.Name = "NewName";
                updateModel.Type = CostType.Buyout;

                updateModel.CostDetails.Name = "costDetails2";
                updateModel.CostDetails.Label = "Cost Details 2";
                updateModel.CostDetails.Fields = new List<FormFieldDefintionModel>
                {
                    new FormFieldDefintionModel
                    {
                        Label = "Cost Number 2",
                        Name = "costNumber2",
                        Type = "string"
                    }
                };

                var updateResult = await Browser.Put($"/v1/costtemplate/{createdCost.Id}", with =>
                {
                    with.User(user);
                    with.JsonBody(updateModel);
                });

                ValidateStatusCode(updateResult, HttpStatusCode.OK);

                var getFullResult = await Browser.Get($"/v1/costtemplate/{createdCost.Id}", w => w.User(user));

                var fullTemplate = Deserialize<CostTemplateDetailsModel>(getFullResult, HttpStatusCode.OK);

                fullTemplate.CostType.Should().Be(updateModel.Type);

                var latest = fullTemplate.GetLatestVersion();

                latest.Name.Should().Be(updateModel.Name);

                latest.Cost.Name.Should().Be(updateModel.CostDetails.Name);
                latest.Cost.Label.Should().Be(updateModel.CostDetails.Label);

                latest.Cost.Fields.Count.Should().Be(updateModel.CostDetails.Fields.Count);
                latest.Cost.Fields[0].Name.Should().Be(updateModel.CostDetails.Fields[0].Name);
                latest.Cost.Fields[0].Label.Should().Be(updateModel.CostDetails.Fields[0].Label);
                latest.Cost.Fields[0].Type.Should().Be(updateModel.CostDetails.Fields[0].Type);
            }
        }

        [TestFixture]
        public class ListCostTemplatesShould : CostTemplateIntegrationTest
        {
            [Test]
            public async Task ListCostTemplates()
            {
                var user = await CreateUser($"{Guid.NewGuid()}User1", Roles.ClientAdmin);

                var model = GetCreateCostTemplateModel();

                var createResult = await Browser.Post("/v1/costtemplate/create", with =>
                {
                    with.User(user);
                    with.JsonBody(model);
                });

                var createdTemplate = Deserialize<CostTemplateDetailsModel>(createResult, HttpStatusCode.Created);

                var listResult = Deserialize<IEnumerable<CostTemplateDetailsModel>>(await Browser.Get("/v1/costtemplate", w => w.User(user)), HttpStatusCode.OK);

                listResult.Should().Contain(x => x.Id == createdTemplate.Id);
            }
        }
    }
}