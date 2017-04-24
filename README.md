Signatura API
=============

Signatura API Client for .NET


### Usage

		var client = new SignaturaClient(token);

		// Fetch existing document

		var task = client.Documents.Get("a121f98c-d50b-403a-b24a-f3dc2a4773ca");
		task.Wait();
		var doc = task.GetAwaiter().GetResult();

		Console.WriteLine(doc.Title);

		// Create new document

		var privateKey = PrivateKey.FromWIF("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");

		var req = new Document(privateKey)
		{
			Title = "Hello World",
						Description = "Created via API",
						FileName = "hello.txt",
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


### Copyright

2017 Consentio Inc.
