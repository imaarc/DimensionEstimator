using DimEstimator.Class;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace DimEstimator
{
    public partial class DimensionUpdate : System.Web.UI.Page
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

            if (!IsPostBack)
            {
                string length = Request.QueryString["length"];
                string width = Request.QueryString["width"];
                string height = Request.QueryString["height"];

                if (!string.IsNullOrEmpty(length))
                    lblLength.Text = "Length: " + length;

                if (!string.IsNullOrEmpty(width))
                    lblWidth.Text = "Width: " + width;

                if (!string.IsNullOrEmpty(height))
                    lblHeight.Text = "Height: " + height;
            }

        }

        private List<ScannedItem> ScannedItems
        {
            get
            {
                return ViewState["ScannedItems"] as List<ScannedItem> ?? new List<ScannedItem>();
            }
            set
            {
                ViewState["ScannedItems"] = value;
            }
        }



        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            Page.RegisterAsyncTask(new PageAsyncTask(async () =>
            {
                string trackingNumber = txtQrResult.Text.Trim();
                int merchantId = 1;
                int transitpointId = 1;

                CheckTrackNumberResponse resp = await checkIfTrackingNumberExist(trackingNumber, merchantId, transitpointId);

                if (resp != null && resp.success && resp.data != null)
                {
                    var list = ScannedItems;

                    list.Add(new ScannedItem
                    {
                        Id = resp.data.id,
                        TrackingNumber = resp.data.trackingNumber,
                        Type = resp.data.isExist ? (resp.data.isChild ? "Item" : "Waybill") : "Not Existing",
                        Status = resp.data.isExist ? "Existing" : "Not Existing"
                    });

                    ScannedItems = list;

                    BindTable();
                }
            }));

            Page.ExecuteRegisteredAsyncTasks();
        }

        private void BindTable()
        {
            tblResults.Rows.Clear();

            // Add header row (optional if you already have a header in ASPX)
            TableHeaderRow headerRow = new TableHeaderRow();

            TableHeaderCell thId = new TableHeaderCell { Text = "ID" };
            thId.Style.Add("display", "none");

            TableHeaderCell thTracking = new TableHeaderCell { Text = "Tracking Number" };
            TableHeaderCell thType = new TableHeaderCell { Text = "Type" };
            TableHeaderCell thStatus = new TableHeaderCell { Text = "Status" };

            headerRow.Cells.Add(thId);
            headerRow.Cells.Add(thTracking);
            headerRow.Cells.Add(thType);
            headerRow.Cells.Add(thStatus);

            tblResults.Rows.Add(headerRow);

            // Add data rows
            foreach (var item in ScannedItems)
            {
                TableRow row = new TableRow();

                TableCell cellId = new TableCell();
                cellId.Text = item.Id.ToString();
                cellId.Style.Add("display", "none");

                TableCell cellTracking = new TableCell { Text = item.TrackingNumber };
                TableCell cellType = new TableCell { Text = item.Type };
                TableCell cellStatus = new TableCell { Text = item.Status };

                row.Cells.Add(cellId);
                row.Cells.Add(cellTracking);
                row.Cells.Add(cellType);
                row.Cells.Add(cellStatus);

                tblResults.Rows.Add(row);
            }
        }



        private async Task<CheckTrackNumberResponse> checkIfTrackingNumberExist(string trackingNumber, int merchantId, int transitpointId)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(120);

                string authToken = Environment.GetEnvironmentVariable("DIM_API_AUTH_TOKEN");
                string serverName = Environment.GetEnvironmentVariable("DIM_API_SERVER_NAME");

                if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(serverName))
                {
                    throw new InvalidOperationException("Authorization token or server name environment variables are missing.");
                }

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", authToken);
                client.DefaultRequestHeaders.Add("ServerName", serverName);

                var payload = new
                {
                    trackingNumber = trackingNumber,
                    merchantId = merchantId,
                    transitpointId = transitpointId
                };

                string json = JsonConvert.SerializeObject(payload);
                HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("https://test.xpl.ph/warehousex-v2/Logistics/checkTrackNumber", content);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<CheckTrackNumberResponse>(responseBody);


                    return result;
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