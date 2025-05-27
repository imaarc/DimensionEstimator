<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DimEstimator._Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dimension Estimator</title>

    <!-- Local Bootstrap -->
    <link href="~/Content/bootstrap.min.css" rel="stylesheet" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link rel="manifest" href="/manifest.json">
    <meta name="theme-color" content="#dc3545" />
    <link rel="apple-touch-icon" href="Images/xplRulerLogo1.png">

    <style>
        body, html {
            height: 100%;
            background-color: #f8f9fa;
        }

        .app-container {
            min-height: 100vh;
            display: flex;
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
        <div class="container app-container">
           
            <p class="text-danger mb-3"> <img src ="Images/xplRulerLogo1.png" width="75" height="75" class="me-3"/>Dimension Estimator</p>
            <div class="mx-auto w-100 text-center">

                <!-- Webcam Preview -->
                <div id="my_camera" class="mb-3 border rounded shadow-sm w-100"></div>

                <!-- Hidden Field to store image base64 -->
                <asp:HiddenField ID="CapturedImageData" runat="server" />

                <!-- Snapshot Preview -->
                <div id="results" class="mb-3"></div>

                <!-- Action Buttons -->
                <div id="cameraActions">
                    <button type="button" class="btn btn-danger w-100 mb-3" onclick="take_snapshot()">Take Picture</button>
                    <p>Ensure your device camera is enabled to take a picture.</p>
                </div>

                <div id="snapshotActions" style="display: none;">
                    <div class="d-flex justify-content-between gap-2 mb-3">
                        <button type="button" class="btn btn-light w-50 border rounded shadow-sm" onclick="retake_snapshot()">Retake</button>
                        <asp:Button ID="UploadButton" runat="server"
                                    Text="Upload and Estimate"
                                    CssClass="btn btn-danger w-50"
                                    OnClientClick="return prepareAndUpload();"
                                    OnClick="UploadButton_Click" />
                    </div>
                </div>

                <!-- Loading Spinner -->
                <div id="loadingSpinner" class="mb-3" style="display: none;">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-2 text-muted">Estimating dimensions...</p>
                </div>

                <!-- Uploaded Image Preview -->
                <div class="mb-3">
                    <asp:Image ID="UploadedImage" runat="server"
                               Visible="false"
                               CssClass="img-fluid rounded shadow-sm" />
                </div>

                <!-- Result Label -->
                <asp:Label ID="ResultLabel" runat="server"
                           CssClass="d-block mt-3 fw-bold text-success" />
            </div>
        </div>
    </form>

    <!-- Scripts -->
    <script src="Scripts/jquery-3.7.0.min.js"></script>
    <script src="Scripts/bootstrap.bundle.min.js"></script>
    <script src="Scripts/webcamjs/webcam.min.js"></script>

    <script type="text/javascript">
        Webcam.set({
            width: 320,
            height: 240,
            image_format: 'jpeg',
            jpeg_quality: 90
        });
        Webcam.attach('#my_camera');

        let lastCapturedImage = "";

        function take_snapshot() {
            Webcam.snap(function (data_uri) {
                lastCapturedImage = data_uri;

                // Store in hidden field
                document.getElementById('<%= CapturedImageData.ClientID %>').value = data_uri;

                // Hide camera and show snapshot
                document.getElementById('my_camera').style.display = 'none';
                document.getElementById('results').innerHTML = `<img src="${data_uri}" class="img-fluid rounded shadow-sm"/>`;

                // Toggle buttons
                document.getElementById('cameraActions').style.display = 'none';
                document.getElementById('snapshotActions').style.display = 'block';
            });
        }

        function retake_snapshot() {
            // Clear previous image
            document.getElementById('results').innerHTML = '';
            document.getElementById('<%= CapturedImageData.ClientID %>').value = '';
            lastCapturedImage = "";

            // Show camera again
            document.getElementById('my_camera').style.display = 'block';

            // Toggle buttons
            document.getElementById('cameraActions').style.display = 'block';
            document.getElementById('snapshotActions').style.display = 'none';
        }

        function prepareAndUpload() {
            if (!lastCapturedImage) {
                alert("Please take a snapshot before uploading.");
                return false;
            }

            showSpinner();
            return true; // allow server postback
        }

        function showSpinner() {
            document.getElementById("loadingSpinner").style.display = "block";
            document.getElementById("UploadedImage").style.display = "none";
        }
    </script>
</body>

</html>
