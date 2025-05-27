<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="DimensionUpdate.aspx.cs" Inherits="DimEstimator.DimensionUpdate" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
     <title>Dimension Estimator</title>

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

      #reader {
        width: 100%;
        max-width: 100%;
        height: auto;
        aspect-ratio: 1 / 1; /* Maintains square shape */
        margin-bottom: 1rem;
        border-radius: 0.5rem;
        overflow: hidden;
    }

    @media (max-width: 576px) {
        #reader {
            aspect-ratio: auto;
        }
    }

    </style>
</head>
<body>
    
<form id="form1" runat="server">

  <asp:ScriptManager ID="ScriptManager1" runat="server" />

    <div class="container mt-5">
        <div class="card p-4 shadow-sm">
            <h4 class="text-center mb-3">QR Code Scanner</h4>
            <div id="reader"></div>

            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>

                    <div class="form-group mt-3">
                        <label>Scanned Result</label>
                        <div class="d-flex justify-content-between">
                            <asp:TextBox 
                                ID="txtQrResult" 
                                runat="server" 
                                CssClass="textbox form-control textboxTrackingNumber" 
                                TextMode="MultiLine" 
                                Rows="2" 
                                Columns="20" />
                            <asp:Button ID="checkBtn" runat="server" CssClass="btn btn-danger ms-3" Text="Check" OnClick="btnUpdate_Click" />
                        </div>
                    </div>

                    <div class="d-flex justify-content-between w-100 mt-4">
                        <asp:Label ID="lblLength" runat="server" Text="Length: -" style="font-weight:bold;font-size:16px"/>
                        <asp:Label ID="lblWidth" runat="server" Text="Width: -"  style="font-weight:bold;font-size:16px"/>
                        <asp:Label ID="lblHeight" runat="server" Text="Height: -"  style="font-weight:bold;font-size:16px"/>
                    </div>

                    <asp:Button ID="btnUpdate" runat="server" CssClass="btn btn-danger mt-3" Text="Update Dimensions" />

                    <div class="mt-4">
                        <asp:Table ID="tblResults" runat="server" CssClass="table table-bordered">
                            <asp:TableHeaderRow>
                                <asp:TableHeaderCell style="display:none;">ID</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Tracking Number</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Type</asp:TableHeaderCell>
                                <asp:TableHeaderCell>Status</asp:TableHeaderCell>
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>

                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
</form>






    <script src="Scripts/jquery-3.7.0.min.js"></script>
    <script src="Scripts/bootstrap.bundle.min.js"></script>
    <script src="https://unpkg.com/html5-qrcode" type="text/javascript"></script>

    <script type="text/javascript">
        let html5QrCode;
        let isScanning = false;

        const startScanner = () => {
            html5QrCode = new Html5Qrcode("reader");

            const qrCodeConfig = {
                fps: 10,
                qrbox: function (viewfinderWidth, viewfinderHeight) {
                    const edge = Math.floor(Math.min(viewfinderWidth, viewfinderHeight) * 0.8);
                    return { width: edge, height: edge }; 
                }
            };



            html5QrCode.start(
                { facingMode: "environment" }, 
                qrCodeConfig,
                qrCodeSuccessCallback,
                qrCodeErrorCallback
            ).catch(err => {
                console.error("Camera start failed:", err);
            });
        };

        const qrCodeSuccessCallback = (decodedText, decodedResult) => {
            if (isScanning) return; 
            isScanning = true;

            console.log("Scanned:", decodedText);

            document.getElementById('<%= txtQrResult.ClientID %>').value = decodedText;


            html5QrCode.pause()
                .then(() => {
                    setTimeout(() => {
                        html5QrCode.resume()
                            .then(() => {
                                isScanning = false;
                            })
                            .catch(err => {
                                console.error("Resume failed:", err);
                                isScanning = false;
                            });
                    }, 1000); 
                })
                .catch(err => {
                    console.error("Pause failed:", err);
                    isScanning = false;
                });
        };

        const qrCodeErrorCallback = errorMessage => {
        };

        $(document).ready(function () {
            startScanner();
        });

    

       

    </script>


</body>
</html>
