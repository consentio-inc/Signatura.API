using System;
using System.Text;
using Signatura.API;
using Signatura.API.Models;
using Signatura.API.Models.Requests;

namespace Signatura.API
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE0OTIxMTIyNDQsImp0aSI6Ik4yTU5MWkxTIiwic3ViIjoiZmVkZXJpY29ib25kIn0.7aCKb4k56HeLZMQis44KnN_aOGis54F9N69ZrB607ho";
            var client = new SignaturaClient(token);

            Console.WriteLine("Fetching existing document...");
            var task = client.Documents.Get("a121f98c-d50b-403a-b24a-f3dc2a4773ca");
            task.Wait();
            var doc = task.GetAwaiter().GetResult();

            Console.WriteLine(doc.Title);

            Console.WriteLine("Creating new document...");

            var privateKey = PrivateKey.FromWIF("KyD2koJnit82bAFBM4KfscrXW8DA8X5ETzhatGc1VhPW3NUxg5Fv");

            var req = new Document(privateKey)
            {
                Title = "hello world",
                Description = "created via API",
                FileName = "myfile.txt",
                MimeType = "text/plain",
                Content = Encoding.UTF8.GetBytes("hello world"),

            };
            req.AddParticipant(new Signer
                {
                    SignerId = "6460c89e-9e84-433e-8eb3-dad7fd03b848",
                    PublicKey = privateKey.PublicKey
                }
            );

            task = client.Documents.Create(req);
            task.Wait();

            var newDoc = task.GetAwaiter().GetResult();

            Console.WriteLine("Signing new document");

            task = client.Documents.Sign(newDoc, privateKey);
            task.Wait();

            doc = task.GetAwaiter().GetResult();
        }
    }
}
