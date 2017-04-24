
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;
using Signatura.API.Helpers;
using Signatura.API.Exceptions;
using Signatura.API.Models.Responses;

namespace Signatura.API.Http {
    public class Connection : IConnection {
        public Connection(IRestClient client) {
            Client = client;
        }

        #region IConnection Members

        public IRestClient Client { get; private set; }

        public async Task<T> ExecuteRequest<T>(string endpoint, IList<Parameter> parameters,
            object data = null, Method method = Method.GET)
            where T : new()
        {
            var request = BuildRequest(endpoint, parameters);
            request.RootElement = "data";
            request.Method = method;

            if (data != null && method != Method.GET)
            {
                request.RequestFormat = DataFormat.Json;
                request.JsonSerializer = new JsonNetSerializer();
                request.AddBody(data);
            }

            var response = await Client.ExecuteTaskAsync<T>(request);

            Console.WriteLine(Client.BuildUri(request));

            if (response.ResponseStatus == ResponseStatus.Error)
            {
                throw response.ErrorException;
            }

            return response.Data;
        }

        #endregion

        private IRestRequest BuildRequest(string endpoint, IEnumerable<Parameter> parameters)
        {
            var request = new RestRequest(endpoint);

            if (parameters == null) {
                return request;
            }
            foreach (var parameter in parameters) {
                request.AddParameter(parameter);
            }

            return request;
        }
    }
}