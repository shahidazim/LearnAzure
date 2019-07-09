using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LearnAzure.Function.ImageAccess
{
    public static class ImageAccess
    {
        [FunctionName("ImageAccess")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            if (name == null)
            {
                return new BadRequestObjectResult("Please pass a name on the query string or in the request body");
            }

            var storageConnectionString = "UseDevelopmentStorage=true;";

            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                var cloudBlobClient = storageAccount.CreateCloudBlobClient();

                var cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

                var cloudBlob = cloudBlobContainer.GetBlobReference(name);
                
                if (!cloudBlob.Exists())
                {
                    return new BadRequestObjectResult("Blob not found");
                }

                if (!cloudBlob.Metadata.ContainsKey("AccessCount"))
                {
                    cloudBlob.Metadata.Add("AccessCount", "1");
                }
                else
                {
                    cloudBlob.Metadata["AccessCount"] = (int.Parse(cloudBlob.Metadata["AccessCount"]) + 1).ToString();
                }

                await cloudBlob.SetMetadataAsync();
            }

            return new OkObjectResult($"Done, {name}");
        }
    }
}
