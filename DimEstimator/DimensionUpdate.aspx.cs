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
                    txtLength.Text =  length;

                if (!string.IsNullOrEmpty(width))
                    txtWidth.Text =  width;

                if (!string.IsNullOrEmpty(height))
                    txtHeight.Text =  height;
            }

        }

        public List<ScannedItem> ScannedItems
        {
            get
            {
                if (Session["ScannedItems"] == null)
                    Session["ScannedItems"] = new List<ScannedItem>();
                return (List<ScannedItem>)Session["ScannedItems"];
            }
            set
            {
                Session["ScannedItems"] = value;
            }
        }




        protected void btnAdd_Click(object sender, EventArgs e)
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

                    // Check if tracking number already exists in the list
                    bool alreadyExists = list.Any(item => item.TrackingNumber == trackingNumber);

                    if (alreadyExists)
                    {
                        // Show alert that it already exists, do not add again
                        ScriptManager.RegisterStartupScript(this, GetType(), "alreadyExistsAlert",
                            $"alert('Tracking number \"{trackingNumber}\" already exists in the list.');", true);
                        return;
                    }

                    // Add new scanned item if not exists
                    list.Add(new ScannedItem
                    {
                        Id = resp.data.id,
                        TrackingNumber = resp.data.trackingNumber,
                        Type = resp.data.isExist ? (resp.data.isChild ? "Item" : "Waybill") : "Not Existing",
                        Status = resp.data.isExist ? "Existing" : "Not Existing"
                    });

                    ScannedItems = list;

                    BindTable();

                    // Add to client side array so future checks see it too
                    string script = $"scannedTrackingNumbers.push('{trackingNumber}');";
                    ScriptManager.RegisterStartupScript(this, GetType(), "updateClientList", script, true);
                }
            }));

            Page.ExecuteRegisteredAsyncTasks();
        }


        protected async void btnUpdate_Click(object sender, EventArgs e)
        {
            List<ScannedItem> allScanned = ScannedItems;

            // Parse actual values from textboxes
            if (!int.TryParse(txtLength.Text.Trim(), out int length) ||
                !int.TryParse(txtWidth.Text.Trim(), out int width) ||
                !int.TryParse(txtHeight.Text.Trim(), out int height))
            {
                // Optional: Show alert for invalid input
                ScriptManager.RegisterStartupScript(this, GetType(), "invalidInput", "alert('Please enter valid numeric dimensions.');", true);
                return;
            }

            // Perform updates
            foreach (var item in allScanned)
            {
                if (!item.Status.Equals("Not Existing"))
                {
                    await updateDimensions(item, length, width, height);
                }
            }

            // ✅ Clear scanned items
            ScannedItems.Clear(); // If ScannedItems is a property that references session or view state, this clears it

            // ✅ Clear the table if you use it to show scanned results
            tblResults.Rows.Clear();

            // ✅ Optionally clear the textbox fields
            txtLength.Text = "";
            txtWidth.Text = "";
            txtHeight.Text = "";

            // ✅ Show success alert
            ScriptManager.RegisterStartupScript(this, GetType(), "updateSuccess", "alert('Dimensions successfully updated.');", true);
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

        private async Task<bool> updateDimensions(ScannedItem item, int length, int width, int height)
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

                bool isChild = item.Type == "Item";

                var payload = new
                {
                    id = item.Id,
                    isChild = isChild,
                    length = length,  
                    width = width,
                    height = height    
                };

                string json = JsonConvert.SerializeObject(payload);
                HttpContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("https://test.xpl.ph/warehousex-v2/Logistics/updateDimensions", content);
                    response.EnsureSuccessStatusCode();
                    return true;
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