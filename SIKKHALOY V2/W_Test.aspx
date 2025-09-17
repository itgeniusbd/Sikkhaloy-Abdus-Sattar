<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="W_Test.aspx.cs" Inherits="EDUCATION.COM.W_Test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <!-- Bootstrap core CSS -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.1.3/css/bootstrap.min.css" rel="stylesheet" />
    <!-- Material Design Bootstrap -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/mdbootstrap/4.5.15/css/mdb.min.css" rel="stylesheet" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <img src="CSS/loading.gif" id="loading" style="display: none;" />

        <div class="progress">
            <div id="dynamic" class="progress-bar progress-bar-success progress-bar-striped active" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="0" style="width: 0%">
                <span id="current-progress"></span>
            </div>
        </div>
    </form>

    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.4/umd/popper.min.js"></script>
    <!-- Bootstrap core JavaScript -->
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/4.1.3/js/bootstrap.min.js"></script>
    <!-- MDB core JavaScript -->
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/mdbootstrap/4.5.15/js/mdb.min.js"></script>

    <script src="JS/Zip_File/FileServer.js"></script>
    <script src="JS/Zip_File/jszip.min.js"></script>
    <script>
        function download() {
            $.ajax({
                type: "POST",
                url: "W_Test.aspx/Get_Image_file",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                beforeSend: function () {
                    $("#loading").show();
                },
                complete: function () {
                    console.log("Request finished.");
                },
                success: function (r) {
                    var response = JSON.parse(r.d);
                    zip(response);
                },
                error: function (err) {
                    console.log(err);
                }
            });
        };

        function zip(response) {
            var zip = new JSZip();

            response.forEach(function (data, i) {
                var name =  data.ID + ".jpg";
                var photo = b64toFile(data.Image);
                zip.file(name, photo, { base64: true });
            });

            zip.generateAsync({ type: "blob" }).then(function (content) {
                saveAs(content, "Attendance_Image.zip");
                $("#loading").hide();
            });
        }

        function b64toFile(b64Data) {
            var sliceSize = 512;
            var byteCharacters = atob(b64Data);
            var byteArrays = [];

            for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
                var slice = byteCharacters.slice(offset, offset + sliceSize);

                var byteNumbers = new Array(slice.length);
                for (var i = 0; i < slice.length; i++) {
                    byteNumbers[i] = slice.charCodeAt(i);
                }

                var byteArray = new Uint8Array(byteNumbers);

                byteArrays.push(byteArray);
            }

            return new Blob(byteArrays, { type: 'image/png' });
        }
    </script>
</body>
</html>
