using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace Signatura.API.Http
{
    public interface IConnection
    {
        IRestClient Client { get; }

        Task<T> ExecuteRequest<T>(string endpoint, IList<Parameter> parameters,
            object data = null, Method method = Method.GET) where T : new();
    }
}