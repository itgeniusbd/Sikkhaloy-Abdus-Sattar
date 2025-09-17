<%@ Page Language="C#" AutoEventWireup="true"  MasterPageFile="~/BASIC.Master" CodeBehind="Student_Card.aspx.cs" Inherits="EDUCATION.COM.ID_Cards.Student_Card" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css" />--%>
    <link href="https://fonts.maateen.me/siyam-rupali/font.css" rel="stylesheet">
    <style>
        #wrapper {
    display: grid;
    grid-gap: 63px 54px;
    grid-template-columns: repeat(3, 1fr);
   /* font-family: 'Kalpurush', serif;*/
   font-family: 'siyam-rupali', sans-serif;
    justify-content: space-evenly;
    align-items: center;
        }

            #wrapper > div {
                position: relative;
                width: 204px;
                height: 314px;

                 position: relative;
 border: 2px solid #0075d2; 

 background-image: url("CSS/image/Student_Card.png");

 background-position: center; /* Center the image */
 background-repeat: no-repeat; /* Do not repeat the image */
 background-size: cover; /* Resize the background image to cover the entire container */
            }

        #grid_Header {
            margin-bottom: 2px;
            /*background-color: #0075d2;*/
            /*border-bottom: 2px solid #006bc8;*/
            color: #fff;
            text-align: center;
            /*display: grid;*/
            grid-template-columns: 40px 1fr;
            position: relative;
            /*background-color: #02283D;*/
            border-bottom-left-radius: 50%;
            border-bottom-right-radius: 50%;
            padding-bottom: 19px;
            padding-left: 5px;
            padding-right: 5px;

        }

        /*#grid_Header img {
                height: 30px;
                border-radius: 3px;
            }*/

        .Hidden_Ins_Name {
            position: absolute;
            visibility: hidden;
            height: auto;
            width: auto;
            white-space: nowrap;
            font-size: 14px;
            color:gainsboro;
        }

        .Institution_Dialog {
            font-size: 8px;
            letter-spacing: 2.3px;
            line-height: 14px;
            text-align: center;
            color:gainsboro;
        }

        .iCard-title {
            margin: auto;
            background-color: #0075d2;
            color:#fbd701;
            /*font-size: min(3vw, 1.em);*/
             font-size: 45px;
            padding: 1px 10px;
            text-align: center;
            width: 100%;
        }
.IDCard-title {
    margin: auto;
    color:#fbd701;
    font-size: 19px;
    padding: 0px;
    text-align: center;
    width: 100%;
    height: 20px;
}

        #user-info {
            margin-bottom: 20px;
            /*display: grid;*/
            grid-template-columns: 90px 1fr;
            margin-left: 9px;
        }

        .simg {
            /*height: 85px;
                width: 85px;*/
            height: 70px;
            width: 70px;
            margin-top: -19px;
            z-index: 1;
            position: relative;
        }

        #user-info ul {
            margin: 5px 0 0 0;
            padding-left: 0px;
        }

            #user-info ul li {
                list-style: none;
                font-size: 12px;
                line-height: 16px;
                color: #000;
            }
            .img-thumbnail {
    padding: 0.25rem;
    background-color: #dc3545;
    border: 1px solid #dee2e6;
    border-radius: 0.25rem;
    max-width: 100%;
    height: auto;
}

        .c-user-name {
            font-weight: bold;
        }

        .c-address {
            position: absolute;
            bottom: 0;
            
            font-size: 8px;
            text-align: center;
            color: #fff;
            width: 100%;
        }

     .simg {
    height: 70px;
    width: 70px;
    margin-top: -16px;
    z-index: 1;
    position: relative;
}

        .headcolor {
            background-color: #8000ff !important;
        }

        @page {
            margin: 15px;
        }

        @media print {
            #header, h3 {
                display: none;
            }
        }

        /*.color-output{height:200px; width:200px; border:5px solid #ccc}

        .output span{    background: orange;    padding: 4px 17px;    display: inline-block;    color: #000;}*/


        .color-output {
           /* background-color: #0075d2;*/
        }

        .name-color-output {
            /*background-color: #0075d2;*/
        }

        .add-color-output {
           /* background-color: #0075d2;*/
        }

        .idcardborder {
          /*  border: 2px solid #0075d2;*/
        }

        /*li {
            padding-left: 10px;
        }

            li a {
                display: block;*/
        /*color: white;*/
        /*text-align: center;
                padding: 16px;
                text-decoration: none;
            }

        ul li input {
            margin-left: 10px;
        }*/
        .table tbody tr td {
            padding-left: 10px;
            width: 100%;
            text-align: right;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Students ID Cards</h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:TextBox ID="Find_ID_TextBox" runat="server" CssClass="form-control" placeholder="Separate the ID by comma"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" Text="Find" class="btn btn-primary" OnClick="FindButton_Click" />
        </div>
        <div class="form-group">
            <label class="btn btn-white" style="font-size: .81rem">
                Signature Browse
                    <input id="Hfileupload" type="file" style="display: none;" />
            </label>
        </div>
        <div class="form-group">
            <input onclick="window.print()" type="button" value="Print" class="btn btn-primary" />
        </div>
        <div class="form-group">
            <input id="HeadlineText" type="text" placeholder="Change Title" class="form-control" />
        </div>
        <div class="form-group">
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
                        <asp:TableRow>
                            <asp:TableCell>Name</asp:TableCell>
                            <asp:TableCell><li><input type="color" class="getnameColor" /></li></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>Address</asp:TableCell>
                            <asp:TableCell><li><input type="color" class="getaddressColor" /></li></asp:TableCell>
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
                        <asp:TableRow>
                            <asp:TableCell>Name</asp:TableCell>
                            <asp:TableCell><li><input type="color" class="getfontnameColor" /></li></asp:TableCell>
                        </asp:TableRow>
                        <asp:TableRow>
                            <asp:TableCell>Address</asp:TableCell>
                            <asp:TableCell><li><input type="color" class="getfontaddressColor" /></li></asp:TableCell>
                        </asp:TableRow>
                    </asp:Table>
                </ul>
            </div>
        </div>

    </div>

    <br />
    <div class="alert alert-info NoPrint">print orientation landscape</div>


    <div id="wrapper">
        <asp:Repeater ID="IDCardRepeater" runat="server">
            <ItemTemplate>
                <div class="idcardborder">
                    <div id="grid_Header" class="color-output getfontColor">
                        <img style="text-align: center; height: 30px" alt="No Logo" src="/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>" />
                        <div class="Ins_Name">
                            <%# Eval("SchoolName") %>
                        </div>
                        <div class="Hidden_Ins_Name">
                            <%# Eval("SchoolName") %>
                        </div>
                        <div class="Institution_Dialog">
                            <asp:Label ID="Label1" CssClass="Instit_Dialog" runat="server" Text='<%# Eval("Institution_Dialog") %>' />
                        </div>
                    </div>

                    <div style="text-align: center;">
                        <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" class="rounded-circle img-thumbnail simg" /><br />

                    </div>
                    <div class="IDCard-title name-color-output"><strong class=""> Student ID <%#Eval("ID") %></strong></div>

                    <div id="user-info">

                        <div>
                       <p style="font-size: 15px;font-weight: bolder;color: maroon;margin-bottom: 0px;margin-top: 10px;">  <%# Eval("StudentsName")%></p>
                            <ul>
                                <li>  </li>
                                <li> পিতা : <%# Eval("FathersName")%>  </li>
                                <li> শ্রেণি : <%# Eval("Class") %></li>
                                 <li> ঠিকানা : <%# Eval("StudentPermanentAddress") %></li>
                                <li> মোবাইল : <%# Eval("SMSPhoneNo") %></li>
                                <li> জন্ম তারিখ : <%# Eval("DateofBirth","{0:d MMM yyyy}") %></li>
                            </ul>
                        </div>
                    </div>

                    <%--<div class="sign">Principal signature</div>--%>
                    <div class="c-address add-color-output">
                      <%--  <%# Eval("Address") %>--%>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:SqlDataSource ID="ICardInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, SchoolInfo.Address, CreateShift.Shift, ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup, StudentsClass.RollNo, StudentsClass.StudentID, SchoolInfo.SchoolID, Student.SMSPhoneNo, Student.StudentPermanentAddress, SchoolInfo.Institution_Dialog, Student.DateofBirth FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status)">
        <SelectParameters>
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
        </SelectParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="IDsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, SchoolInfo.Address, CreateShift.Shift, ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup, StudentsClass.RollNo, StudentsClass.StudentID, SchoolInfo.SchoolID, Student.SMSPhoneNo, Student.BloodGroup, SchoolInfo.Institution_Dialog, Student.DateofBirth FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status) AND (SchoolInfo.SchoolID = @SchoolID) AND (Student.ID IN(SELECT  id from [dbo].[In_Function_Parameter] (@IDs)))">
        <SelectParameters>
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="Find_ID_TextBox" Name="IDs" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            var Default_fontSize = 13;
            var Max_fontSize = 20;

            var test = document.getElementsByClassName("Hidden_Ins_Name")[0];
            var Show = document.getElementsByClassName("Ins_Name")[0];

            var New_fontSize = Math.round(((Default_fontSize * parseFloat(Show.clientWidth)) / parseFloat(test.clientWidth)));
            if (New_fontSize > Max_fontSize) {
                New_fontSize = Max_fontSize;
            }
            var width = (test.clientWidth) + "px";

            $('.Ins_Name').css('font-size', New_fontSize);


            if (!$('.Instit_Dialog').text()) {
                $('.Institution_Dialog').hide();
                $('.Hidden_Ins_Name').hide();
            }

            // Sign upload
            $("#Hfileupload").change(function () {
                if (typeof (FileReader) != "undefined") {
                    var dvPreview = $(".sign");
                    dvPreview.html("");
                    var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.jpg|.jpeg|.gif|.png|.bmp)$/;
                    $($(this)[0].files).each(function () {
                        var file = $(this);
                        if (regex.test(file[0].name.toLowerCase())) {
                            var reader = new FileReader();
                            reader.onload = function (e) {
                                var img = $("<img />");
                                img.attr("style", "height: 24px;width: 75px;position: absolute;right: 0;bottom: 15px;");
                                img.attr("src", e.target.result);
                                dvPreview.append(img);
                                dvPreview.append("Principal signature");
                            }
                            reader.readAsDataURL(file[0]);
                        } else {
                            alert(file[0].name + " is not a valid image file.");
                            dvPreview.html("");
                            return false;
                        }
                    });
                } else {
                    alert("This browser does not support HTML5 FileReader.");
                }
            });

            //save headline
            $("#HeadlineText").on("keyup", function () {
                $(".iCard-title").text($(this).val());
                localStorage.Headline = $(this).val();
            });

            //read headline
            if (localStorage.Headline) {
                $(".iCard-title").text(localStorage.Headline);
            }
            else {
                $(".iCard-title").text("Student ID :");
            }
        });

        // Background Color

        $(".getColor").on("change", function () {
            //Get Color
            var color = $(".getColor").val();
            //apply cuurent color to div
            $(".color-output").css("background", color);
            $(".idcardborder").css("border-color", color);
            $(".headcolor").css("background", color);
        })
        $(".getnameColor").on("change", function () {
            //Get Color
            var color = $(".getnameColor").val();
            //apply cuurent color to div
            $(".name-color-output").css("background", color);
        })
        $(".getaddressColor").on("change", function () {
            //Get Color
            var color = $(".getaddressColor").val();
            //apply cuurent color to div
            $(".add-color-output").css("background", color);
        })

        //  forcolor

        $(".getfontColor").on("change", function () {
            //Get Color
            var color = $(".getfontColor").val();
            //apply cuurent color to font
            $(".color-output").css("color", color);

        })
        $(".getfontnameColor").on("change", function () {
            //Get Color
            var color = $(".getfontnameColor").val();
            //apply cuurent color to student Name
            $(".name-color-output").css("color", color);
        })
        $(".getfontaddressColor").on("change", function () {
            //Get Color
            var color = $(".getfontaddressColor").val();
            //apply cuurent color to Address
            $(".add-color-output").css("color", color);
        })



    </script>
</asp:Content>