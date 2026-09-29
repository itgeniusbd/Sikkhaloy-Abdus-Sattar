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
                --card-theme: #167A40;
                --card-theme-light: #3A9A5C;
                --card-body: #F3F9F4;
                position: relative;
                width: 323.52px;
                height: 204px;
                overflow: hidden;
                border: 2px solid var(--card-theme);
                background-color: var(--card-body);
                background-image: none;
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
            }

            #wrapper > div::before {
                content: "";
                position: absolute;
                top: 0;
                left: 0;
                right: 0;
                height: 42px;
                background: linear-gradient(
                    to bottom,
                    var(--card-theme) 0,
                    var(--card-theme) 26px,
                    var(--card-theme-light) 26px,
                    var(--card-theme-light) 42px
                );
                z-index: 0;
            }

            #wrapper > div::after {
                content: "";
                position: absolute;
                left: 0;
                right: 0;
                bottom: 0;
                height: 20px;
                background: var(--card-theme);
                z-index: 0;
            }

        #grid_Header {
            position: relative;
            z-index: 1;
            margin-bottom: 0;
            color: #fff;
            text-align: center;
            display: grid;
            grid-template-columns: 58px 1fr;
            min-height: 42px;
            padding: 1px 8px 1px 0;
            align-items: center;
        }

#grid_Header img {
    position: relative;
    top: 0px;
    height: 40px;
    width: 40px;
    margin: 0 0 0 7px;
    border: 2px solid #ffffff;
    border-radius: 50%;
    box-shadow: 0 0 0 1px #0F5E30;
    background: #fff;
    object-fit: cover;
    z-index: 3;
}

        .gcard-logo-wrap {
            display: flex;
            align-items: flex-start;
            justify-content: flex-start;
            padding: 0;
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
    letter-spacing: 2.4px;
    line-height: 23px;
    text-align: center;
    color: #fff;
    padding-left: 4px;
    padding-top: 5px;
}

.Ins_Name {
    color: #fff;
    font-weight: 700;
    padding-left: 4px;
    line-height: 0.95;
    font-size:17px !important;
}

.iCard-title {
    position: relative;
    z-index: 2;
    margin: 10px 12px 4px 98px;
    background-color: var(--card-theme);
    border-radius: 7px;
    color: #fff;
    font-size: 12px;
    padding: 2px 10px;
    text-align: center;
    width: 120px;
    border: solid 1px var(--card-theme);
    font-weight: bold;
}

        #user-info {
            position: relative;
            z-index: 1;
            margin-bottom: 0;
            display: grid;
            grid-template-columns: 90px 1fr;
            align-items: start;
            padding-bottom: 16px;
        }

            #user-info .gcard-photo-col {
                text-align: center;
                min-height: 90px;
            }

#user-info .gcard-id {
    position: absolute;
    left: 0px;
    top: 77px;
    width: 82px;
    display: block;
    color: maroon;
    font-weight: 700;
    font-family: Arial, sans-serif;
    font-size: 15px;
    line-height: 1.2;
    margin: 0;
    text-align: center;
    z-index: 3;
}

#user-info .gcard-photo-box {
    position: absolute;
    left: 10px;
    top: -17px;
    width: 71px;
    height: 88px;
    overflow: hidden;
    border-radius: 5px;
    background: #fff;
    border: 2px solid var(--card-theme);
    box-sizing: border-box;
    z-index: 2;
}

            #user-info .gcard-photo-box img {
                width: 100% !important;
                height: 100% !important;
                max-width: none !important;
                max-height: none !important;
                margin: 0 !important;
                padding: 0 !important;
                display: block;
                object-fit: cover;
                object-position: center top;
            }

            #user-info ul {
                margin: 2px 0 0 0;
                padding-left: 8px;
            }

            .gcard-details {
                padding-top: 3px;
                padding-left: 4px;
            }

            .gcard-details .gcard-name {
                font-size: 14px;
                font-weight: 800;
                color: maroon;
                margin: 0 0 2px 0;
                padding-left: 8px;
                line-height: 1.2;
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
    left: 0;
    right: 0;
    bottom: 0;
    height: 20px;
    z-index: 2;
    font-size: 11px;
    line-height: 20px;
    text-align: center;
    color: #fff;
    width: 100%;
    padding: 0 8px;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
    box-sizing: border-box;
}

    .c-address i {
        margin-right: 4px;
        font-size: 11px;
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

            #wrapper > div {
                -webkit-print-color-adjust: exact;
                print-color-adjust: exact;
                background-image: none !important;
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
        <div class="form-group">
            <label class="mb-0 mr-1" style="font-size:.81rem">Theme</label>
            <input type="color" id="ThemeColor" value="#167A40" title="Header / footer color" style="width:42px;height:34px;padding:2px;border:1px solid #ccc;cursor:pointer;" />
        </div>
        <div class="form-group">
            <label class="mb-0 mr-1" style="font-size:.81rem">Body</label>
            <input type="color" id="BodyColor" value="#F3F9F4" title="Card background color" style="width:42px;height:34px;padding:2px;border:1px solid #ccc;cursor:pointer;" />
        </div>
        <div class="form-group">
            <button type="button" id="ResetCardColors" class="btn btn-white" style="font-size:.81rem">Reset Color</button>
        </div>
    </div>

    <div class="alert alert-info NoPrint">print orientation landscape</div>


    <div id="wrapper">
        <asp:Repeater ID="IDCardRepeater" runat="server">
            <ItemTemplate>
                <div>
                    <div id="grid_Header">
                        <div class="gcard-logo-wrap">
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
                        <div class="gcard-photo-col">
                            <strong class="gcard-id">ID : <%#Eval("ID") %></strong>
                            <div class="gcard-photo-box">
                                <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" alt="" />
                            </div>
                        </div>
                        <div class="gcard-details">
                       <p class="gcard-name"><%# Eval("StudentsName")%></p>
                            <ul style="font-size:13px">
                                <li>  </li>
                                <li> পিতা : <%# Eval("FathersName")%>  </li>
                               <%-- <li> শ্রেণি : <%# Eval("Class") %></li>--%>
                                 <li> ঠিকানা : <%# Eval("StudentPermanentAddress") %></li>
                                <li> মোবাইল : <%# Eval("SMSPhoneNo") %></li>
                                <li> জন্ম তারিখ : <%# Eval("DateofBirth","{0:d MMM yyyy}") %></li>
                            </ul>
                        </div>
                    </div>

                  <%--  <div class="sign">Principal signature</div>--%>
                    <div class="c-address"><i class="fa fa-map-marker fas fa-location-dot" aria-hidden="true"></i> <%# Eval("Address") %></div>
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

            var defaultTheme = "#167A40";
            var defaultBody = "#F3F9F4";

            function lightenHex(hex, amount) {
                hex = (hex || "").replace("#", "");
                if (hex.length === 3) {
                    hex = hex.split("").map(function (c) { return c + c; }).join("");
                }
                if (hex.length !== 6) return defaultTheme;
                var num = parseInt(hex, 16);
                var r = Math.min(255, Math.max(0, (num >> 16) + amount));
                var g = Math.min(255, Math.max(0, ((num >> 8) & 0xff) + amount));
                var b = Math.min(255, Math.max(0, (num & 0xff) + amount));
                return "#" + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
            }

            function applyCardColors(theme, body) {
                theme = theme || defaultTheme;
                body = body || defaultBody;
                var light = lightenHex(theme, 42);
                $("#wrapper > div").each(function () {
                    this.style.setProperty("--card-theme", theme);
                    this.style.setProperty("--card-theme-light", light);
                    this.style.setProperty("--card-body", body);
                });
            }

            var savedTheme = localStorage.gcardTheme || defaultTheme;
            var savedBody = localStorage.gcardBody || defaultBody;
            $("#ThemeColor").val(savedTheme);
            $("#BodyColor").val(savedBody);
            applyCardColors(savedTheme, savedBody);

            $("#ThemeColor").on("input change", function () {
                localStorage.gcardTheme = this.value;
                applyCardColors(this.value, $("#BodyColor").val());
            });

            $("#BodyColor").on("input change", function () {
                localStorage.gcardBody = this.value;
                applyCardColors($("#ThemeColor").val(), this.value);
            });

            $("#ResetCardColors").on("click", function () {
                localStorage.removeItem("gcardTheme");
                localStorage.removeItem("gcardBody");
                $("#ThemeColor").val(defaultTheme);
                $("#BodyColor").val(defaultBody);
                applyCardColors(defaultTheme, defaultBody);
            });
        });
    </script>
</asp:Content>
