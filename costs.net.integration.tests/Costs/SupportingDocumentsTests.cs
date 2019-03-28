namespace costs.net.integration.tests.Costs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using core.Models.Costs;
    using core.Models.Response;
    using E = dataAccess.Entity;
    using FluentAssertions;
    using NUnit.Framework;
    using plugins;

    public class SupportingDocumentsTests
    {
        public abstract class SupportingDocumentsTest : BaseCostIntegrationTest
        {
            protected async Task<T> CreateSupportingDocument<T>(string url, string fileName = "file.txt", Guid? documentId = null)
            {
                var registerRequest = new SupportingDocumentRegisterRequest
                {
                    FileName = fileName,
                    FileSize = 1024
                };

                var registerUploadResult = Deserialize<SupportingDocumentRegisterResult>(await Browser.Post($"{url}/registerUpload", w =>
                {
                    w.User(User);
                    w.JsonBody(registerRequest);
                }), HttpStatusCode.Created);

                var completeUploadUrl = documentId.HasValue 
                    ? $"{url}/{documentId.Value}/completeUpload" 
                    : $"{url}/completeUpload";

                var response = Deserialize<T>(await Browser.Post(completeUploadUrl, w =>
                {
                    w.User(User);
                    w.JsonBody(registerUploadResult);
                }), HttpStatusCode.Created);

                return response;
            }
        }

        [TestFixture]
        public class UploadSupportingDocumentShould : SupportingDocumentsTest
        {
            [Test]
            public async Task UploadSupportingDocument()
            {
                var cost = await CreateCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                var url = SupportingDocumentsUrl(cost.Id, latestStage.Id, latestRevision.Id);
                var supportingDocument = await CreateSupportingDocument<E.SupportingDocument>(url);

                url += "/" + supportingDocument.Id + "/revision/latest";
                var latestSupportingDocumentRevision = Deserialize<E.SupportingDocumentRevision>(await Browser.Get(url, w => w.User(User)), HttpStatusCode.OK);

                latestSupportingDocumentRevision.FileName.Should().Be("file.txt");
            }
        }

        [TestFixture]
        public class DeleteSupportingDocumentShould : SupportingDocumentsTest
        {
            [Test]
            public async Task DeleteSupportingDocument()
            {
                var cost = await CreateCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                var url = SupportingDocumentsUrl(cost.Id, latestStage.Id, latestRevision.Id);
                var supportingDocument = await CreateSupportingDocument<E.SupportingDocument>(url);

                var supportingDocuments = Deserialize<IEnumerable<E.SupportingDocument>>(await Browser.Get(url, w => w.User(User)), HttpStatusCode.OK);
                supportingDocuments.Count().Should().BeGreaterOrEqualTo(1);

                var deleteResponse = Deserialize<OperationResponse>(await Browser.Delete(url + "/" + supportingDocument.Id, w => w.User(User)), HttpStatusCode.OK);
                deleteResponse.Success.Should().BeTrue();

                supportingDocuments = Deserialize<IEnumerable<E.SupportingDocument>>(await Browser.Get(url, w => w.User(User)), HttpStatusCode.OK);
                supportingDocuments.Where(d => d.Id == supportingDocument.Id).Should().BeEmpty();
            }

            [Test]
            public async Task NotDeleteGeneratedDocuments()
            {
                await CreateCostEntity(User);

                var cost = Deserialize<E.Cost>(await CreateCost(User, new CreateCostModel
                {
                    TemplateId = CostTemplate.Id,
                    StageDetails = new StageDetails
                    {
                        Data = new Dictionary<string, dynamic>
                        {
                            { "contentType", new { id = Guid.NewGuid(), value = Constants.ContentType.Video } },
                            { "productionType", new { id = Guid.NewGuid(), value = Constants.ProductionType.FullProduction } },
                            { "projectId", "123456789" },
//                            { "costNumber", "AC123456789123" },
                            { "approvalStage", "OriginalEstimate" },
                            { "agency", new 
                                {
                                    id = User.Agency.Id,
                                    abstractTypeId = User.Agency.AbstractTypes.FirstOrDefault().Id
                                }
                            }
                        }
                    }
                }), HttpStatusCode.Created);

                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                var url = SupportingDocumentsUrl(cost.Id, latestStage.Id, latestRevision.Id);
                var supportingDocuments = Deserialize<IEnumerable<E.SupportingDocument>>(await Browser.Get(url, w => w.User(User)), HttpStatusCode.OK);
                var generatedDocuments = supportingDocuments.Where(d => d.Generated).ToArray();
                generatedDocuments.Count().Should().BeGreaterOrEqualTo(1);

                var deleteUrl = url + "/" + generatedDocuments.Last().Id;
                var deleteResponse = Deserialize<OperationResponse>(await Browser.Delete(deleteUrl, w => w.User(User)), HttpStatusCode.BadRequest);
                deleteResponse.Success.Should().BeFalse();
            }
        }

        [TestFixture]
        public class UploadSupportingDocumentRevisionShould : SupportingDocumentsTest
        {
            [Test]
            public async Task UploadSupportingDocumentRevision()
            {
                var cost = await CreateCostEntity(User);
                var latestStage = await GetCostLatestStage(cost.Id, User);
                var latestRevision = await GetCostLatestRevision(cost.Id, latestStage.Id, User);

                var url = SupportingDocumentsUrl(cost.Id, latestStage.Id, latestRevision.Id);
                var supportingDocument = await CreateSupportingDocument<E.SupportingDocument>(url);

                await CreateSupportingDocument<E.SupportingDocumentRevision>(url, "filev2.txt", supportingDocument.Id);

                url += "/" + supportingDocument.Id + "/revision/latest";
                var latestSupportingDocumentRevision = Deserialize<E.SupportingDocumentRevision>(await Browser.Get(url, w => w.User(User)), HttpStatusCode.OK);

                latestSupportingDocumentRevision.FileName.Should().Be("filev2.txt");
            }
        }
    }
}
