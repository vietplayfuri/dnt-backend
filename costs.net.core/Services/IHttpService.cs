
namespace dnt.core.Services
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IHttpService
    {
        Task<HttpResponseMessage> GetAsync(Uri url);
        Task<HttpResponseMessage> PostAsync(Uri url, HttpContent content);
    }
}
