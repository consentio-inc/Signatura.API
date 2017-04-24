using System;
using System.Net.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using Signatura.API.Http;
using Signatura.API.Clients;
using Signatura.API.Helpers;

namespace Signatura.API
{
    public class SignaturaClient
    {
        public static string SignaturaApiUrl = "http://localhost:8000/v1/";
        private IConnection _connection;

        public SignaturaClient(string token)
        {
            var client = new RestClient(SignaturaApiUrl)
            {
                UserAgent = "signatura-api-dotnet",
                Authenticator = new JwtAuthenticator(token),
            };

            _connection = new Connection(client);
            Documents = new DocumentsClient(_connection);
        }


        #region SignaturaClient Members

        public DocumentsClient Documents { get; private set; }

        #endregion
    }
}