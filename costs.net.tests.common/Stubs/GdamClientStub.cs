namespace costs.net.tests.common.Stubs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using core.ExternalResource.Gdam;
    using core.Models.AMQ;
    using core.Models.Common;
    using core.Models.Gdam;
    using dataAccess.Entity;
    using Microsoft.AspNetCore.Http;

    public class GdamClientStub : IGdamClient
    {
        private readonly List<string> _fileIds = new List<string>();

        public GdamClientStub()
        {
            Users = new List<GdamUser>();
        }

        private List<GdamUser> Users { get; }

        public Task<IEnumerable<dynamic>> FindAgencies(string userId, string name)
        {
            dynamic agencies = new { };
            return Task.FromResult(agencies);
        }

        public Task<A5ProjectSchema> FindProjectSchemaByAgencyId(string agencyId)
        {
            throw new NotImplementedException();
        }

        public Task<GdamListResponse<GdamProject>> FindProjects(string userId)
        {
            var response = new GdamListResponse<GdamProject>
            {
                List = new GdamProject[0]
            };

            return Task.FromResult(response);
        }

        public async Task<A5Agency> FindAgencyById(string buId)
        {
            var agency = new A5Agency();
            return await Task.FromResult(agency);
        }

        public async Task<IEnumerable<dynamic>> GetAgencyUsers(string userId, string agencyId)
        {
            dynamic users = new { };
            return await Task.FromResult(users);
        }

        public Task<RegisterFileUploadResponse> RegisterFileUpload(string userId, RegisterFileUploadRequest request)
        {
            var fileId = Guid.NewGuid().ToString();
            _fileIds.Add(fileId);

            return Task.FromResult(new RegisterFileUploadResponse
            {
                Files = new[]
                {
                    new RegisterFileUploadResponse.File
                    {
                        ExternalId = Guid.NewGuid().ToString(),
                        FileId = fileId,
                        FileUri = "http://fakegdn.co/getFile/blah",
                        Status = "accepted",
                        StorageId = "123"
                    }
                }
            });
        }


        public Task<HttpResponseMessage> UploadFile(string fileUrl, byte[] bytes)
        {
            Task.Delay(250);
            return Task.FromResult<HttpResponseMessage>(new HttpResponseMessage(HttpStatusCode.Created));
        }

        public Task CompleteFileUpload(string userId, string fileId)
        {
            if (_fileIds.Contains(fileId))
            {
                return Task.FromResult(true);
            }

            throw new Exception("Not registered");
        }

        public async Task<GdamUser> FindUser(string userId)
        {
            dynamic user = Users.FirstOrDefault(a => a._id == userId.Replace("id-", ""));
            return await Task.FromResult(user);
        }

        public Task<A5Project> FindProjectById(string projectId)
        {
            throw new NotImplementedException();
        }

        public Task<A5DictionaryResponse> GetDictionary(string agencyId, string dictionaryName)
        {
            throw new NotImplementedException();
        }

        public void CreateUser(GdamUser user)
        {
            Users.Add(user);
        }

        public Task UploadFile(string fileUrl, Stream bytes, IFormFile file)
        {
            Task.Delay(250);
            return Task.FromResult<dynamic>(null);
        }

        public Task<IEnumerable<Country>> FindCountries()
        {
            var countries = Enumerable.Empty<Country>();
            return Task.FromResult(countries);
        }
    }
}