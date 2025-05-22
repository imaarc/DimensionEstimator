<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DimEstimator._Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dimension Estimator</title>

    <!-- Local Bootstrap -->
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <style>
        body, html {
            height: 100%;
            background-color: #f8f9fa;
        }

        .app-container {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            flex-direction: column;
            padding: 20px;
        }

        #loadingSpinner {
            display: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
        <div class="container app-container text-center">
            <div class="w-100" style="max-width: 500px;">

                <!-- File Upload -->
                <div class="mb-3">
                    <asp:FileUpload ID="ImageUpload" runat="server" CssClass="form-control" />
                </div>

                <!-- Submit Button -->
                <div class="mb-3">
                    <asp:Button ID="UploadButton" runat="server"
                        Text="Upload and Estimate"
                        CssClass="btn btn-primary w-100"
                        OnClientClick="showSpinner();"
                        OnClick="UploadButton_Click" />
                </div>

                <!-- Loading Spinner -->
                <div id="loadingSpinner" class="mb-3">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-2 text-muted">Estimating dimensions...</p>
                </div>

                <!-- Image Preview -->
                <div class="mb-3">
                    <asp:Image ID="UploadedImage" runat="server"
                        Visible="false"
                        CssClass="img-fluid rounded shadow-sm" />
                </div>

                <!-- Result Label -->
               <asp:Label ID="ResultLabel" runat="server"
                CssClass="d-block mt-3"
                Font-Bold="True"
                EnableViewState="true" />

            </div>
        </div>
    </form>

    <!-- Bootstrap JS -->
    <script src="~/Scripts/bootstrap.bundle.min.js"></script>

    <!-- Spinner Script -->
    <script type="text/javascript">
        function showSpinner() {
            document.getElementById("loadingSpinner").style.display = "block";
        }
    </script>
</body>
</html>
