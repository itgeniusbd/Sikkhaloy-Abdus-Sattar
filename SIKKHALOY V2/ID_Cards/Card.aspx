<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Card.aspx.cs" Inherits="EDUCATION.COM.ID_Cards.Card" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <style>
          
#wrapper {
    display: grid;
    grid-gap: 63px 54px;
    grid-template-columns: repeat(3, 1fr);
    font-family: 'Kalpurush', serif;
    justify-content: space-evenly;
    align-items: center;
}

            #wrapper > div {
                position: relative;
                border: 2px solid #0075d2; 
               width: 323.52px; height: 204px;
                background-image: url("CSS/image/card.PNG");

                background-position: center; /* Center the image */
                background-repeat: no-repeat; /* Do not repeat the image */
                background-size: cover; /* Resize the background image to cover the entire container */
               
            }

        #grid_Header {
            margin-bottom: 8px;
            color: #fff;
            text-align: center;
            display: grid;
            grid-template-columns: 40px 1fr;
        }

#grid_Header img {
    height: 50px;
    border-radius: 50px;
    margin: 4px 0 0 11px;
    width: 50px;
}

        .Hidden_Ins_Name {
            position: absolute;
            visibility: hidden;
            height: auto;
            width: auto;
            white-space: nowrap;
        }

        .Institution_Dialog {
            font-size: 11px;
            letter-spacing: 3.3px;
            line-height: 14px;
            text-align: center;
        }

.iCard-title {
    margin: -27px 0px 0px 109px;
    background-color: #99d59b;
    border-radius: 7px;
    color: #8b0a0a;
    font-size: 12px;
    padding: 2px 10px;
    text-align: center;
    width: 120px;
    border: solid 1px green;
    font-weight: bold;
}

        #user-info {
            margin-bottom: 20px;
            display: grid;
            grid-template-columns: 90px 1fr;
        }

            #user-info img {
                height: 50px;
                width:50px;
                margin: 24px 0px 0px 4px;
            }

            #user-info ul {
                margin: 5px 0 0 0;
                padding-left: 17px;
            }

#user-info ul li {
    list-style: none;
    font-size: 10px;
    line-height: 14px;
    color: #000;
}

        .c-user-name {
font-size: 15px;
    font-weight: bolder;
    color: maroon;
    margin-bottom: -10px;
    padding-left: 16px;
    margin-top: -6px;
        }

        .c-address {
            position: absolute;
            bottom: 0;
            font-size: 12px;
            text-align: center;
            color: #fff;
            width: 100%;
        }

        .sign {
            position: absolute;
            right: 5px;
            bottom: 18px;
            font-weight: normal;
            margin-bottom: 0;
            font-size: 8.5pt;
        }

        @page {
            margin: 15px;
            font-family: 'Bangla';
        }

        @media print {
            #header, h3 {
                display: none;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="row">
        <div class="col-sm-12">
            <h3>Students ID Cards  <a style="float: right" href="../ID_Cards/Student_ID_Cards.aspx">Student Custom ID Card</a></h3>
        </div>
    </div>
    

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
    </div>

    <div class="alert alert-info NoPrint">print orientation landscape</div>


    <div id="wrapper">
        <asp:Repeater ID="IDCardRepeater" runat="server">
            <ItemTemplate>
                <div>
                    <div id="grid_Header">
                        <div style="padding: 5px 0;">
                            <img alt="No Logo" src="/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>" />
                        </div>
                        <div>
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
                    </div>
                    <div class="iCard-title"> অভিভাবক কার্ড </div>

                    <div id="user-info">
                        <div style="text-align: center;">
                           
                            <strong  class="d-block" style="color:maroon;font-weight:700;margin-top:20px; font-family:Arial">ID : <%#Eval("ID") %></strong>
                            <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" /><br />
                        </div>
                        <div>
                       <p style="font-size: 14px;font-weight:800;color: maroon;margin-bottom: -6px;padding-left: 16px;margin-top: 1px;">  <%# Eval("StudentsName")%></p>
                            <ul style="font-size:13px">
                                <li>  </li>
                                <li> পিতা : <%# Eval("FathersName")%>  </li>
                                <li> শ্রেণি : <%# Eval("Class") %></li>
                                 <li> ঠিকানা : <%# Eval("StudentPermanentAddress") %></li>
                                <li> মোবাইল : <%# Eval("SMSPhoneNo") %></li>
                                <li> জন্ম তারিখ : <%# Eval("DateofBirth","{0:d MMM yyyy}") %></li>
                            </ul>
                        </div>
                    </div>

                  <%--  <div class="sign">Principal signature</div>--%>
                    <div class="c-address">
                     <%--   <%# Eval("Address") %>--%>
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
    <asp:SqlDataSource ID="IDsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, SchoolInfo.Address, CreateShift.Shift, ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup, StudentsClass.RollNo, StudentsClass.StudentID, SchoolInfo.SchoolID, Student.SMSPhoneNo, Student.StudentPermanentAddress, SchoolInfo.Institution_Dialog, Student.DateofBirth FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status) AND (SchoolInfo.SchoolID = @SchoolID) AND (Student.ID IN(SELECT  id from [dbo].[In_Function_Parameter] (@IDs)))">
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
                $(".iCard-title").text("Student ID Card");
            }
        });
    </script>
</asp:Content>
