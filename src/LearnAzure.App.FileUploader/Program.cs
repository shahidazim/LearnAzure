using System;
using System.IO;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace LearnAzure.App.FileUploader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("File path is missing.");
                return;
            }

            var storageConnectionString = "UseDevelopmentStorage=true;";

            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                var cloudBlobClient = storageAccount.CreateCloudBlobClient();

                var cloudBlobContainer = cloudBlobClient.GetContainerReference("images");

                if (!cloudBlobContainer.Exists())
                {
                    cloudBlobContainer.CreateAsync();
                    var permissions = new BlobContainerPermissions()
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    cloudBlobContainer.SetPermissions(permissions);
                }

                var sourceFile = args[0];

                if (!File.Exists(sourceFile))
                {
                    Console.WriteLine("File not found: " + sourceFile);
                }

                var blobName = new FileInfo(sourceFile).Name;

                Console.WriteLine("Uploading to Blob storage as blob '{0}'", sourceFile);

                var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);
                cloudBlockBlob.UploadFromFileAsync(sourceFile);

                Console.WriteLine("Blob uploaded successfully!");
            }
            else
            {
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add an environment variable named 'STORAGE_CONNECTION_STRING' with your storage " +
                    "connection string as a value.");
            }
            Console.WriteLine("Press any key to exit the application.");
            Console.ReadLine();
        }
    }
}
