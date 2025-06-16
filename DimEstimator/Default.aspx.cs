using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Http;
using DimEstimator.Class;
using Newtonsoft.Json;
using DotNetEnv;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace DimEstimator
{

    public partial class _Default : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            string envPath = Server.MapPath("~/.env");  

            if (System.IO.File.Exists(envPath))
            {
                DotNetEnv.Env.Load(envPath);

            }
            else
            {
                Response.Write($".env file NOT found at: {envPath}<br>");
            }

        }

        private async Task ProcessImageUrlAsync(string imageUrl)
        {
            try
            {
                UploadedImage.ImageUrl = imageUrl;
                UploadedImage.Visible = true;

                string explanation = await CallDimensionAPIAsync(imageUrl);

                if (string.IsNullOrEmpty(explanation))
                    throw new Exception("Dimension API returned null or empty response.");

                var estimate = JsonConvert.DeserializeObject<Estimate>(explanation);
                if (estimate == null)
                    throw new Exception("Failed to deserialize Estimate from response.");

                ResultLabel.Text = $@"
                <div class='card shadow-sm'>
                    <div class='card-body'>
                        <h5 class='card-title'>Estimated Dimensions</h5>
                        <ul class='list-group list-group-flush'>
                            <li class='list-group-item'><strong>Length:</strong> {estimate.length} inches</li>
                            <li class='list-group-item'><strong>Width:</strong> {estimate.width} inches</li>
                            <li class='list-group-item'><strong>Height:</strong> {estimate.height} inches</li>
                        </ul>
                    </div>
                    <a href='DimensionUpdate.aspx?length={estimate.length}&&width={estimate.width}&&height={estimate.height}' class='btn btn-danger'>Tag dimensions tracking number</a>
                </div>";

                ResultLabel.Visible = true;
            }
            catch (Exception ex)
            {
                ResultLabel.Text = "An error occurred: " + ex.Message;
                ResultLabel.Visible = true;
            }
        }






        protected async void UploadButton_Click(object sender, EventArgs e)
        {
            string base64Image = CapturedImageData.Value;

            if (!string.IsNullOrEmpty(base64Image) && base64Image.StartsWith("data:image"))
            {
                try
                {
                    // Strip off the data:image prefix
                    string base64Data = base64Image.Substring(base64Image.IndexOf(",") + 1);
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    // Generate a temporary file path
                    string uploadFolder = Server.MapPath("~/Uploads/");
                    Directory.CreateDirectory(uploadFolder);
                    string fileName = $"snapshot_{Guid.NewGuid():N}.jpg";
                    string filePath = Path.Combine(uploadFolder, fileName);

                    // Save the image temporarily
                    File.WriteAllBytes(filePath, imageBytes);

                    // Upload to Firebase
                    var firebaseHelper = new FirebaseStorageHelper();
                    string firebaseUrl = await firebaseHelper.UploadFileAsync(filePath);

                    // Delete the temporary local file
                    File.Delete(filePath);

                    // Show the image
                    UploadedImage.ImageUrl = firebaseUrl;
                    UploadedImage.Visible = true;

                    // Call your external API for dimension estimation
                    string explanation = await CallDimensionAPIAsync(firebaseUrl);
                    var estimate = JsonConvert.DeserializeObject<Estimate>(explanation);

                    ResultLabel.Text = $@"
                <div class='card shadow-sm'>
                    <div class='card-body'>
                        <h5 class='card-title'>Estimated Dimensions</h5>
                        <ul class='list-group list-group-flush'>
                            <li class='list-group-item'><strong>Length:</strong> {estimate.length} inches</li>
                            <li class='list-group-item'><strong>Width:</strong> {estimate.width} inches</li>
                            <li class='list-group-item'><strong>Height:</strong> {estimate.height} inches</li>
                        </ul>
                    </div>
                    <a href='DimensionUpdate.aspx?length={estimate.length}&&width={estimate.width}&&height={estimate.height}' class='btn btn-danger'>Tag dimensions tracking number</a>
                </div>";

                    ResultLabel.Visible = true;
                }
                catch (Exception ex)
                {
                    ResultLabel.Text = "An error occurred: " + ex.Message;
                    ResultLabel.Visible = true;
                }
            }
            else
            {
                ResultLabel.Text = "Please take a snapshot before uploading.";
                ResultLabel.Visible = true;
            }
        }



        private async Task<string> CallDimensionAPIAsync(string imagePath)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                UseProxy = false // Disable system proxy if not needed
            };

            using (HttpClient client = new HttpClient(handler))
            {
                client.Timeout = TimeSpan.FromSeconds(60); // Adjust as needed

                string authToken = Environment.GetEnvironmentVariable("DIM_API_AUTH_TOKEN");
                string serverName = Environment.GetEnvironmentVariable("DIM_API_SERVER_NAME");

                if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(serverName))
                    throw new InvalidOperationException("Authorization token or server name environment variables are missing.");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", authToken);
                client.DefaultRequestHeaders.Add("ServerName", serverName);

                var payload = new { ImageUrl = imagePath }; 
                string json = System.Text.Json.JsonSerializer.Serialize(payload);

                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    HttpResponseMessage response = await client.PostAsync("https://test.xpl.ph/warehousex-v2/GPT/EstimateBoxDimensions2", content);
                    stopwatch.Stop();

                    Console.WriteLine($"Dimension API call took {stopwatch.ElapsedMilliseconds} ms");

                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (TaskCanceledException ex)
                {
                    throw new TimeoutException("The request timed out.", ex);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error calling dimension API.", ex);
                }
            }
        }




    }
}