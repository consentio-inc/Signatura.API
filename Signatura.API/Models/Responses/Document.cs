using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RestSharp.Deserializers;

namespace Signatura.API.Models.Responses
{
    public class Document
    {

        [DeserializeAs(Name = "id")]
        public string DocumentId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string MimeType { get; set; }

        public string Hash { get; set; }

        public string RawHash { get; set; }

        public DateTime Created { get; set; }

        public List<Signer> Signers { get; set; }
    }

    public class Signer
    {

        [DeserializeAs(Name = "id")]
        public string SignerId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsSigned { get; set; }
    }
}