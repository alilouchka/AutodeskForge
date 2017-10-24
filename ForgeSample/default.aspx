<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="ForgeSample._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Forge Viewer Test</title>
<link rel="stylesheet" href="https://developer.api.autodesk.com/modelderivative/v2/viewers/style.min.css" type="text/css" />
     <script src="https://developer.api.autodesk.com/modelderivative/v2/viewers/three.min.js"></script>
    <script src="https://developer.api.autodesk.com/modelderivative/v2/viewers/viewer3D.min.js"></script>
    
    <script src="/Scripts/forgeViewer.js"></script>

</head>
<body>
     <!-- The Viewer JS -->
   
    <form id="form1" runat="server">
        <div>
            <asp:FileUpload ID="FileUpload1" runat="server" />
            <asp:Button ID="Button1" runat="server" OnClick="uploadAndTranslate" Text="Upload &amp; Translate" />
        </div>
    </form>
    <div id="forgeViewer" style="width:100%; height:500px">
    </div>
</body>
</html>
