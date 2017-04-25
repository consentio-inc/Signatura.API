using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using Org.BouncyCastle.Utilities.Encoders;
using RestSharp;
using Signatura.API.Http;
using Signatura.API.Models;
using Signatura.API.Models.Responses;

namespace Signatura.API.Clients
{
    public class DocumentsClient
    {
        private readonly IConnection _connection;

        public DocumentsClient(IConnection connection)
        {
            _connection = connection;
        }

        public async Task<Document> Get(string documentId)
        {
            var parameters = new List<Parameter>
            {
                new Parameter { Name = "id", Value = documentId, Type = ParameterType.UrlSegment }
            };
            return await _connection.ExecuteRequest<Document>("documents/{id}", parameters);
        }

        public async Task<Document> Create(Models.Requests.Document document)
        {
            return await _connection.ExecuteRequest<Document>(
                "documents/add", null, document, method: Method.POST);
        }

        public async Task<Document> Sign(Document document, PrivateKey privateKey)
        {
            // sign document content

            Transaction tx = Transaction.Parse();
            var payload = Base64.Encode(privateKey.Sign(Base64.Decode(document.Hash)));
            var rawPayload = Base64.Encode(privateKey.Sign(Base64.Decode(document.RawHash)));

            var parameters = new List<Parameter>()
            {
                new Parameter { Name = "id", Value = document.DocumentId, Type = ParameterType.UrlSegment },
                new Parameter { Name = "payload", Value = payload, Type = ParameterType.RequestBody },
                new Parameter { Name = "raw_payload", Value = rawPayload, Type = ParameterType.RequestBody }
            };
            return await _connection.ExecuteRequest<Document>(
                "documents/{id}/sign", parameters, method: Method.POST);
        }
    }
}