using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using LearnAzure.App.ImageViewer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace LearnAzure.App.ImageViewer.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var imageUrls = new List<string>();

            var storageConnectionString = "UseDevelopmentStorage=true;";

            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                var cloudBlobClient = storageAccount.CreateCloudBlobClient();

                var cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem blobItem in results.Results)
                    {
                        imageUrls.Add(blobItem.Uri.AbsoluteUri);

                        if (blobItem is CloudBlob)
                        {
                            using (HttpClient client = new HttpClient())
                            {
                                var cloudBlobItem = (CloudBlob)blobItem;
                                await client.GetStringAsync($"http://localhost:7071/api/ImageAccess?name={cloudBlobItem.Name}");
                            }
                        }
                    }
                } while (blobContinuationToken != null);
            }

            return View(imageUrls);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
