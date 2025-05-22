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

namespace DimEstimator
{

    public partial class _Default : System.Web.UI.Page
    {
       
        protected void Page_Load(object sender, EventArgs e)
        {
            Env.Load();
        }

        protected async void UploadButton_Click(object sender, EventArgs e)
        {
            if (ImageUpload.HasFile)
            {
                string fileName = Path.GetFileName(ImageUpload.FileName);

                // Save file temporarily on server
                string uploadFolder = Server.MapPath("~/Uploads/");
                Directory.CreateDirectory(uploadFolder);
                string filePath = Path.Combine(uploadFolder, fileName);
                ImageUpload.SaveAs(filePath);

                var firebaseHelper = new FirebaseStorageHelper();
                string firebaseUrl = await firebaseHelper.UploadFileAsync(filePath);

                File.Delete(filePath);

                UploadedImage.ImageUrl = firebaseUrl;
                UploadedImage.Visible = true;

                string explanation = await CallDimensionAPIAsync(firebaseUrl);
                var root = JsonConvert.DeserializeObject<DimObj>(explanation);
                var estimate = JsonConvert.DeserializeObject<Estimate>(root.DimensionsEstimate);

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
                </div>";

                ResultLabel.Visible = true;
            }
            else
            {
                ResultLabel.Text = "Please upload an image first.";
            }
        }


        private async Task<string> CallDimensionAPIAsync(string imagePath)
        {
            using (HttpClient client = new HttpClient())
            {
                string authToken = Environment.GetEnvironmentVariable("DIM_API_AUTH_TOKEN");
                string serverName = Environment.GetEnvironmentVariable("DIM_API_SERVER_NAME");

                client.DefaultRequestHeaders.Add("Authorization", authToken);
                client.DefaultRequestHeaders.Add("ServerName", serverName);

                var payload = new { ImageUrl = imagePath };
                string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(payload);

                HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("https://test.xpl.ph/warehousex-v2/GPT/EstimateBoxDimensions", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }


    }
}