using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using Google.Api.Gax.ResourceNames;
using System.Drawing.Imaging;
using System.Drawing;

namespace DimEstimator.Class
{
    public class FirebaseStorageHelper
    {
        private readonly string bucketName = "xpl-app.appspot.com";

        public async Task<string> UploadFileAsync(string localFilePath)
        {
            var storage = StorageClient.Create();

            string folderName = "dimPicsEstimator";

            string extension = Path.GetExtension(localFilePath)?.ToLower();
            string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg"; // force JPEG for compression
            string destinationFileName = $"{folderName}/{fileName}";
            string contentType = "image/jpeg";

            // Compress image
            string compressedFilePath = CompressAndSaveTempImage(localFilePath);

            using (var fileStream = File.OpenRead(compressedFilePath))
            {
                await storage.UploadObjectAsync(bucketName, destinationFileName, contentType, fileStream);
            }

            return $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(destinationFileName)}?alt=media";
        }


        private string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".webp":
                    return "image/webp";
                default:
                    return "application/octet-stream";
            }
        }

        public string CompressAndSaveTempImage(string originalPath, int maxWidth = 800, long quality = 75L)
        {
            string extension = Path.GetExtension(originalPath)?.ToLower();
            string tempPath = Path.Combine(Path.GetTempPath(), $"compressed_{Path.GetFileName(originalPath)}");

            using (var image = System.Drawing.Image.FromFile(originalPath))
            {
                int newWidth = maxWidth;
                int newHeight = (int)((double)image.Height / image.Width * newWidth);

                using (var bitmap = new Bitmap(image, new Size(newWidth, newHeight)))
                {
                    var encoder = GetEncoder(ImageFormat.Jpeg);
                    var encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                    bitmap.Save(tempPath, encoder, encoderParams);
                }
            }

            return tempPath;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(c => c.FormatID == format.Guid);
        }




    }
}