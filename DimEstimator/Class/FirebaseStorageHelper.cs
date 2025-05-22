using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using Google.Api.Gax.ResourceNames;

namespace DimEstimator.Class
{
    public class FirebaseStorageHelper
    {
        private readonly string bucketName = "xpl-app.appspot.com";

        public async Task<string> UploadFileAsync(string localFilePath)
        {
            var storage = StorageClient.Create();

            string folderName = "dimPicsEstimator";

            string extension = Path.GetExtension(localFilePath); // keep original extension
            string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + extension;

            string destinationFileName = $"{folderName}/{fileName}";

            using (var fileStream = File.OpenRead(localFilePath))
            {
                await storage.UploadObjectAsync(bucketName, destinationFileName, null, fileStream);
            }

            return $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(destinationFileName)}?alt=media";
        }

    }
}