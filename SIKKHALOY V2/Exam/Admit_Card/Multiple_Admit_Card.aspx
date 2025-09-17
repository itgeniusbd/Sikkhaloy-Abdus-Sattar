<%@ Page Title="Admit Card" Language="C#" MasterPageFile="~/BASIC.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Multiple_Admit_Card.aspx.cs" Inherits="EDUCATION.COM.Exam.Admit_Card.Multiple_Admit_Card" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="css/Custom.css?v=14.3" rel="stylesheet" />
    <link href="css/skin1.css?v=6" id="DefaultCSS" rel="stylesheet" />


    <%--<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js"></script>
<script src="jscolor.js"></script>--%>

    <style>
        #canvas {
            width: 500px;
            height: 250px;
            background-color: #000;
        }

        input#colorpic {
            width: 100px;
            height: 40px;
            background-color: transparent;
            float: left;
        }

        .idcardborder {
            border: 2px solid #0075d2;
        }
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Admit Card</h3>
    <a class="d-print-none" href="Old_AdmitCard.aspx">Old Admit Card</a> 
    <div style="float:right"><h3 ><a class="d-print-none" href="AdmitCard_WithoutPhoto.aspx"> প্রবেশপত্র বাংলায়</a></h3> <h3 ><a class="d-print-none" href="Old_AdmitCardWithoutPhoto.aspx"> প্রবেশপত্র বাংলায় পুরাতন</a></h3></div>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
        </ContentTemplate>
    </asp:UpdatePanel>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" onchange="showMe(this);" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True" AutoPostBack="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ EXAM ]</asp:ListItem>
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorStar" ErrorMessage="*" InitialValue="0" ValidationGroup="F"></asp:RequiredFieldValidator>
            <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ExamID, ExamName FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <%if (SectionDropDownList.Items.Count > 1)
            {%>
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <%}%>
        <%if (GroupDropDownList.Items.Count > 1)
            { %>
        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <%}%>
        <div class="form-group">
            <asp:DropDownList ID="Paid_DropDownList" CssClass="form-control" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ ALL STUDENT ]</asp:ListItem>
                <asp:ListItem>Paid</asp:ListItem>
                <asp:ListItem>Due</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="form-group">
            <asp:TextBox ID="Find_ID_TextBox" runat="server" CssClass="form-control" placeholder="Multiple ID by comma"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*" CssClass="EroorStar" ControlToValidate="Find_ID_TextBox" ValidationGroup="F"></asp:RequiredFieldValidator>
        </div>

        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" Text="Find" class="btn btn-primary" OnClick="FindButton_Click" ValidationGroup="F" />
        </div>
        <div class="form-group">
            <button type="button" class="btn btn-primary hidden-print" onclick="window.print();"><i class="glyphicon glyphicon-print"></i>Print</button>
        </div>
    </div>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <input id="ExamNameTextBox" class="form-control" type="text" value="" placeholder="Exam Name" />
        </div>
        <div class="form-group">
            <input id="TeacherSign" class="form-control" type="text" value="Teacher signature" placeholder="Teacher Signature" />
            <label class="btn btn-grey btn-file">
                Browse
                <input id="Tfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
            <input id="PrincipalSign" class="form-control" type="text" value="Headmaster signature" placeholder="Principal Signature" />
            <label class="btn btn-grey btn-file">
                Browse
                <input id="Hfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
            <select id="PrintPage" class="form-control">
                <option value="1">2 Admit Card</option>
                <option value="2" selected="selected">4 Admit Card</option>
                <option value="3">6 Admit Card</option>
            </select>
        </div>
        <div class="form-group">
            <input id="Issue_d" placeholder="Issue date" autocomplete="off" type="text" class="form-control p-date" />
        </div>
        <div class="form-group">
            <%--<div id="colorPanel" class="colorPanel hidden-print">
                <ul></ul>
            </div>--%>

            <div class="dropdown">
                <button class="btn btn-primary " type="button" data-toggle="dropdown">
                    Choose Color
            <span class="caret"></span>
                </button>
                <ul class="dropdown-menu">
                    <li style="text-align: center"><a><b>Background Color</b></a></li>
                    <li class="divider"></li>
                    <asp:Table runat="server" CssClass="table">
                        <asp:TableRow>
                            <asp:TableCell>Head</asp:TableCell>
                            <asp:TableCell><li><input type="color" class="getColor" /></li></asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                    <li class="divider"></li>
                    <li style="text-align: center"><a><b>Font Color</b></a></li>
                    <li class="divider"></li>
                    <asp:Table runat="server" CssClass="table">
                        <asp:TableRow>
                            <asp:TableCell>Head</asp:TableCell>
                            <asp:TableCell><li><input type="color" class="getfontColor" /></li></asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </ul>
            </div>


        </div>

    </div>



    <div class="alert alert-success hidden-print">
        <asp:Label ID="TotalCardLabel" runat="server"></asp:Label>
        [Page orientation "landscape" prefer mozilla browser]
    </div>















    <div id="wrapper">
        <asp:Repeater ID="IDCardDL" runat="server">
            <ItemTemplate>
                <div class="idcardborder">
                    <div class="card-header color-output">
                        <div class="pl-1">
                            <img src='/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>' />
                        </div>
                        <div>
                            <h4 class="font-weight-bold"><%# Eval("SchoolName") %></h4>
                            <p class="font-weight-bold"><%# Eval("Address") %></p>
                        </div>
                    </div>

                    <div class="Card_Title">ADMIT CARD</div>

                    <div class="student-info">
                        <div class="s-Photo">
                            <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" class="img-thumbnail rounded-circle" />
                            <strong>ID: <%# Eval("ID") %></strong>
                        </div>
                        <div class="Info">
                            <table>
                                <tr>
                                    <td>Exam</td>
                                    <td>:</td>
                                    <td class="ExamName"></td>
                                </tr>
                                <tr>
                                    <td>Session</td>
                                    <td>:</td>
                                    <td><%# Eval("EducationYear")%></td>
                                </tr>
                                <tr>
                                    <td>Name</td>
                                    <td>:</td>
                                    <td><%# Eval("StudentsName")%></td>
                                </tr>
                                <tr>
                                    <td>Class</td>
                                    <td>:</td>
                                    <td><%# Eval("Class") %></td>
                                </tr>
                                <tr>
                                    <td>Roll No</td>
                                    <td>:</td>
                                    <td><%# Eval("RollNo") %></td>
                                </tr>

                                <tr class="Group" style="display: none;">
                                    <td>Group</td>
                                    <td>:</td>
                                    <td><%# Eval("SubjectGroup") %></td>
                                </tr>
                                <tr class="Section" style="display: none;">
                                    <td>Section</td>
                                    <td>:</td>
                                    <td><%# Eval("Section") %></td>
                                </tr>
                                <tr class="Shift" style="display: none;">
                                    <td>Shift</td>
                                    <td>:</td>
                                    <td><%# Eval("Shift") %></td>
                                </tr>
                                <tr>
                                    <td>Issue</td>
                                    <td>:</td>
                                    <td class="Issue"></td>
                                </tr>
                            </table>
                        </div>
                    </div>

                    <div class="Sign">
                        <div class="pull-left">
                            <div class="SignTeacher">
                                <img class="TeacherSign" src="/Handeler/Sign_Teacher.ashx?sign=<%# Eval("SchoolID") %>" />
                            </div>
                            <label class="Teacher">Teacher</label>
                        </div>
                        <div class="text-right pull-right">
                            <div class="SignHead">
                                <img class="HeadSign" src="/Handeler/Sign_Principal.ashx?sign=<%# Eval("SchoolID") %>" />
                            </div>
                            <label class="Head">Principal</label>
                        </div>
                        <div class="clearfix"></div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:SqlDataSource ID="ICardInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, SchoolInfo.SchoolID, StudentsClass.StudentID, SchoolInfo.SchoolName, Student.StudentsName, Student.FathersName, CreateClass.Class, CreateSection.Section, SchoolInfo.Address, CreateShift.Shift, StudentsClass.RollNo, Education_Year.EducationYear, CreateSubjectGroup.SubjectGroup, StudentsClass.SubjectGroupID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = 'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID)">
        <SelectParameters>
            <asp:SessionParameter DefaultValue="" Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="IDsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, SchoolInfo.SchoolID, StudentsClass.StudentID, SchoolInfo.SchoolName, Student.StudentsName, Student.FathersName, CreateClass.Class, CreateSection.Section, SchoolInfo.Address, CreateShift.Shift, StudentsClass.RollNo, Education_Year.EducationYear, CreateSubjectGroup.SubjectGroup FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = 'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (Student.ID IN (SELECT id FROM dbo.In_Function_Parameter(@IDs) AS In_Function_Parameter_1))">
        <SelectParameters>
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="Find_ID_TextBox" Name="IDs" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <%--<div class="color-picker">
        <canvas id="canvas"></canvas>
        <br />
        <input type="color" id="colorpic" />
        <div id="colorCode">#000</div>

    </div>




    <div id="canvas_1" style="width: 300px; height: 100px">
    </div>--%>




    <script src="/JS/jquery.colorpanel.js"></script>


    <script>

        //var canvas = document.getElementById("canvas_3");
        //var input = document.getElementById("colorpic");
        //var colorCode = document.getElementById("colorCode");

        //input.addEventListener("input", setColor);
        //function setColor() {
        //    canvas.style.backgroundColor = input.value;
        //    colorCode.innerHTML = input.value;
        //}
        //setColor();




        /*Sign Upload*/
        //Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
        $(function () {
            if ($('.Group td').eq(2).text() != "") {
                $('.Group').show();
            }
            if ($('.Section td').eq(2).text() != "") {
                $('.Section').show();
            }
            if ($('.Shift td').eq(2).text() != "") {
                $('.Shift').show();
            }

            $(".p-date").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            }).datepicker('setDate', new Date());;

            //Issue Date
            $(".Issue").text($('#Issue_d').val());
            $('#Issue_d').on('change', function () {
                $(".Issue").text($(this).val());
            });

            $("#Tfileupload").change(function (input) {
                var file = input.target.files[0];
                var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
                    alert('Please upload file having extensions .jpeg/.jpg/.png only');
                    return false;
                }
                else {
                    var fileReader = new FileReader();

                    fileReader.readAsDataURL(file);
                    fileReader.onload = function (event) {
                        var image = new Image();

                        image.src = event.target.result;
                        image.onload = function () {
                            var canvas = document.createElement("canvas");
                            var context = canvas.getContext("2d");
                            canvas.width = 130;//image.width / 4;
                            canvas.height = 40; //image.height / 4;
                            context.drawImage(image, 0, 0, image.width, image.height, 0, 0, canvas.width, canvas.height);

                            $('.TeacherSign').attr("src", canvas.toDataURL());

                            //Save to server
                            $.ajax({
                                url: "Multiple_Admit_Card.aspx/Teacher_Sign",
                                data: JSON.stringify({ 'Image': canvas.toDataURL().split(",")[1] }),
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                success: function (response) {
                                    console.log(response)
                                },
                                error: function (xhr) {
                                    var err = JSON.parse(xhr.responseText);
                                    console.log(err.message);
                                }
                            });
                        }
                    }
                }
            });

            $("#Hfileupload").change(function (input) {
                var file = input.target.files[0];
                var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
                    alert('Please upload file having extensions .jpeg/.jpg/.png only');
                    return false;
                }
                else {
                    var fileReader = new FileReader();

                    fileReader.readAsDataURL(file);
                    fileReader.onload = function (event) {
                        var image = new Image();

                        image.src = event.target.result;
                        image.onload = function () {
                            var canvas = document.createElement("canvas");
                            var context = canvas.getContext("2d");
                            canvas.width = 130;//image.width / 4;
                            canvas.height = 40; //image.height / 4;
                            context.drawImage(image, 0, 0, image.width, image.height, 0, 0, canvas.width, canvas.height);

                            $('.HeadSign').attr("src", canvas.toDataURL());

                            //Save to server
                            $.ajax({
                                url: "Multiple_Admit_Card.aspx/Principal_Sign",
                                data: JSON.stringify({ 'Image': canvas.toDataURL().split(",")[1] }),
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8",
                                success: function (response) {
                                    console.log(response)
                                },
                                error: function (xhr) {
                                    var err = JSON.parse(xhr.responseText);
                                    console.log(err.message);
                                }
                            });
                        }
                    }
                }
            });

            $('.ExamName').text($('[id*=ExamDropDownList] :selected').text());

            $('#ExamNameTextBox').val($('[id*=ExamDropDownList] :selected').text());

            $(".Teacher").text($("#TeacherSign").val());
            $("#TeacherSign").on('keyup', function () {
                $(".Teacher").text($(this).val());
            });


            $("#ExamNameTextBox").on('keyup', function () {
                $(".ExamName").text($(this).val());
            });


            $(".Head").text($("#PrincipalSign").val());
            $("#PrincipalSign").on('keyup', function () {
                $(".Head").text($(this).val());
            });

            $("#wrapper").css("grid-template-columns", "repeat(" + $('#PrintPage').val() + ", 1fr)");
            $("#PrintPage").change(function () {
                $("#wrapper").css("grid-template-columns", "repeat(" + this.value + ", 1fr)");

                if (this.value == 3) {
                    $(".card-header h4").css('font-size', '1rem !important');
                }


            });

            //Chane Color

            $('#colorPanel').ColorPanel({
                styleSheet: '#DefaultCSS',
                animateContainer: '#wrapper',
                colors: {
                    '#00a12a': 'css/skin1.css?v=12',
                    '#ff4444': 'css/skin2.css?v=10',
                    '#4285F4': 'css/skin3.css?v=4'
                }
            });
        });
    </script>

    <script type="text/javascript"> 
        function showMe(a) {
            $('#ExamNameTextBox').val($('[id*=ExamDropDownList] :selected').text());
        }

        // Background Color

        $(".getColor").on("change", function () {
            //Get Color
            var color = $(".getColor").val();
            //apply cuurent color to div
            $(".color-output").css("background", color);
            $(".idcardborder").css("border-color", color);
            $(".headcolor").css("background", color);
        })


        //  forcolor

        $(".getfontColor").on("change", function () {
            //Get Color
            var color = $(".getfontColor").val();
            //apply cuurent color to font
            $(".color-output").css("color", color);

        })

    </script>

</asp:Content>
