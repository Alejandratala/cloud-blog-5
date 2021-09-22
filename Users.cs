using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using System.Net;
using System.Linq;
using Microsoft.Azure.Documents.Linq;

namespace Blog5Users
{
    public static class Users
    {
        [FunctionName("Users")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "user/{id?}")] HttpRequest req,
            [CosmosDB(
                databaseName: "Blog5Database",
                collectionName: "User",
                ConnectionStringSetting = "myCosmosDB")] DocumentClient client,
            ILogger log)
        {
            if (req.Method == "GET") {
                return await GetUser(req, client);
            }

            if (req.Method == "POST") {
                return await CreateUser(req, client);
            }

            return new NotFoundResult();
        }
        public static async Task<IActionResult> CreateUser(HttpRequest req, DocumentClient client) {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            Guid guid = Guid.NewGuid();

            var user = new User{
                Id = guid.ToString(),
                FirstName = data.firstName,
                LastName = data.lastName
            };

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Blog5Database", "User");

            var response = await client.CreateDocumentAsync(collectionUri, user);

            if (response.StatusCode == HttpStatusCode.Created) {
                return new OkObjectResult(response.Resource.Id);
            }

            return new NotFoundResult();
        }

        public static async Task<IActionResult> GetUser(HttpRequest req, DocumentClient client) {
            string id = req.Query["id"];

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Blog5Database", "User");

            var query = client.CreateDocumentQuery<User>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(p => p.Id == id)
                .AsDocumentQuery();

            if (query.HasMoreResults) {
                var queryResult = await query.ExecuteNextAsync();
                return new OkObjectResult((User)queryResult.FirstOrDefault());
            }

            return new NotFoundResult();
        }
    }
}
