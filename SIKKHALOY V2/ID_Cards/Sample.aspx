<%@ Page Title="" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Sample.aspx.cs" Inherits="EDUCATION.COM.ID_Cards.Sample" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/All_ID_Cards.css?v=5" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="Contain">
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
                <label class="btn btn-default btn-file">
                    Signature Browse
                    <input id="Hfileupload" type="file" style="display: none;" />
                </label>
            </div>
        </div>

        <asp:DataList ID="IDCardDataList" CssClass="IdCard" runat="server" RepeatColumns="2" RepeatDirection="Horizontal" HeaderStyle-VerticalAlign="Top">
            <ItemTemplate>
                <div class="border col-sm-6">
                    <div class="row card-header">
                        <div class="col-md-2 col-sm-2 text-center" style="padding-left: 0; padding-right: 0">
                            <img src="/Handeler/SchoolLogo.ashx?SLogo=<%#Eval("SchoolID") %>" class="Ins_Logo" />
                        </div>
                        <div class="col-md-10 col-sm-10 Card-width" style="padding-left: 0; padding-right: 0">
                            <div class="Ins_Name">
                                <%# Eval("SchoolName") %>
                            </div>
                            <div class="Hidden_Ins_Name">
                                <%# Eval("SchoolName") %>
                            </div>

                            <div class="Institution_Dialog">
                                <asp:Label ID="Instit_Dialog" CssClass="Instit_Dialog" runat="server" Text='<%# Eval("Institution_Dialog") %>' />
                            </div>
                        </div>
                    </div>

                    <div class="Student_ID">
                        Student ID Card
                    </div>

                    <div class="row student-info">
                        <div class="col-md-3 col-sm-3 col-xs-12 text-center" style="padding-left: 8px; padding-right:0">
                            <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" class="S_Photo" /><br />
                        </div>

                        <div class="col-md-9 col-sm-9 col-xs-12 S_Info">
                            <ul>
                                <li style="font-weight:bold;">ID: <%#Eval("ID") %></li>
                                <li style="font-weight:bold; font-size:14px;"><%# Eval("StudentsName")%></li>
                                <li>Father's Name: <%# Eval("FathersName") %></li>
                                <li>Class: <%# Eval("Class") %></li>
                                <li>Session: <%# Eval("EducationYear") %></li>
                            </ul>
                        </div>
                    </div>

                    <div class="sign">authority signature</div>

                    <div class="row card-footer text-center">
                        <asp:Label ID="AddressLabel" runat="server" Text='<%# Eval("Address") %>' />
                    </div>
                </div>
            </ItemTemplate>
        </asp:DataList>


        <asp:SqlDataSource ID="ICardInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, SchoolInfo.Address, CreateShift.Shift, ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup, StudentsClass.RollNo, StudentsClass.StudentID, SchoolInfo.SchoolID, Student.SMSPhoneNo, Student.BloodGroup, SchoolInfo.Institution_Dialog, Student.DateofBirth, Education_Year.EducationYear FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status)">
            <SelectParameters>
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                <asp:Parameter DefaultValue="Active" Name="Status" />
            </SelectParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="IDsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, CreateSection.Section, CreateClass.Class, SchoolInfo.SchoolName, SchoolInfo.Address, CreateShift.Shift, ISNULL(CreateSubjectGroup.SubjectGroup, N'No Group') AS SubjectGroup, StudentsClass.RollNo, StudentsClass.StudentID, SchoolInfo.SchoolID, Student.SMSPhoneNo, Student.BloodGroup, SchoolInfo.Institution_Dialog, Student.DateofBirth, Education_Year.EducationYear FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN SchoolInfo ON StudentsClass.SchoolID = SchoolInfo.SchoolID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status) AND (SchoolInfo.SchoolID = @SchoolID) AND (Student.ID IN (SELECT id FROM dbo.In_Function_Parameter(@IDs) AS In_Function_Parameter_1))">
            <SelectParameters>
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:Parameter DefaultValue="Active" Name="Status" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="Find_ID_TextBox" Name="IDs" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>

        <input onclick="window.print()" type="submit" value="Print" class="btn btn-primary" />
    </div>


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
                $('.Ins_Name').css("margin-top", "8px");
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
                                dvPreview.append("authority signature");
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
        });
    </script>
</asp:Content>
