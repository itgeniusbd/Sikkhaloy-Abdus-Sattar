<%@ Page Title="Student Login Management" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Student_Username_password.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.Create_Student_Username_password" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/CreateStudentUserPassword.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Student Login Userid & Password.
      <label id="Class_Section"></label>
    </h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup"
                DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
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
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
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
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:TextBox ID="StuIdTextBox" autocomplete="off" placeholder="Enter ID" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="StuIdTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="IDFindButton" runat="server" CssClass="btn btn-primary" OnClick="IDFindButton_Click" Text="Find" ValidationGroup="F" />
        </div>
    </div>

    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item">
            <a class="nav-link active" data-toggle="tab" href="#panel2" role="tab" aria-expanded="true">Already Created Userid & Password</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" data-toggle="tab" href="#panel1" role="tab" aria-expanded="false">Create Userid & Password</a>
        </li>
    </ul>

    <div class="tab-content card">
        <div class="tab-pane fade in active show" id="panel2" role="tabpanel" aria-expanded="true">
            <div class="table-responsive">
                <asp:GridView ID="StudentUserGV" runat="server" AutoGenerateColumns="False" PagerStyle-CssClass="pgr" CssClass="mGrid" DataKeyNames="UserName,SMSPhoneNo,StudentsName,Password,StudentID">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllCheckBox" runat="server" Text="SMS" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <HeaderStyle CssClass="NoPrint" />
                            <ItemStyle Width="20px" CssClass="NoPrint" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone No" SortExpression="SMSPhoneNo" />
                        <asp:BoundField DataField="UserName" HeaderText="Userid" SortExpression="UserName" />
                        <asp:BoundField DataField="Password" HeaderText="Password" SortExpression="Password" />
                        <asp:BoundField DataField="CreateDate" HeaderText="Create Date" SortExpression="CreateDate" />
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                </asp:GridView>
                <asp:SqlDataSource ID="StudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, Registration.UserName, Student.SMSPhoneNo, AST.Password, Registration.CreateDate, Student.StudentID, StudentsClass.RollNo FROM aspnet_Users INNER JOIN Registration ON aspnet_Users.UserName = Registration.UserName INNER JOIN Student ON Registration.RegistrationID = Student.StudentRegistrationID INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID LEFT OUTER JOIN AST ON Student.StudentRegistrationID = AST.RegistrationID WHERE (Registration.Category = N'Student') AND (Registration.SchoolID = @SchoolID) AND (Student.Status = 'Active') AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = 'Active') AND (StudentsClass.EducationYearID = @EducationYearID) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo , '$' , '') , ',' , '') AS FLOAT) ELSE 0 END">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="Student_ID_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, Registration.UserName, Student.SMSPhoneNo, AST.Password, Registration.CreateDate, Student.StudentID, StudentsClass.RollNo FROM aspnet_Users INNER JOIN Registration ON aspnet_Users.UserName = Registration.UserName INNER JOIN Student ON Registration.RegistrationID = Student.StudentRegistrationID INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID LEFT OUTER JOIN AST ON Student.StudentRegistrationID = AST.RegistrationID WHERE (Registration.Category = N'Student') AND (Registration.SchoolID = @SchoolID) AND (Student.Status = 'Active') AND (Student.ID = @ID) AND (StudentsClass.EducationYearID = @EducationYearID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="StuIdTextBox" Name="ID" PropertyName="Text" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
                    <InsertParameters>
                        <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                        <asp:Parameter Name="SchoolID" />
                        <asp:Parameter Name="StudentID" />
                        <asp:Parameter Name="TeacherID" />
                        <asp:Parameter Name="EducationYearID" />
                    </InsertParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="School_InfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [Website] FROM [SchoolInfo] WHERE ([SchoolID] = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:CustomValidator ID="CV1" runat="server" ClientValidationFunction="Validate2" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="SP"></asp:CustomValidator>
            </div>

            <div class="form-inline" style="display: none;" id="sms">
                <div class="form-group">
                    <asp:Button ID="SMSButton" runat="server" Text="Send User &amp; Password" CssClass="btn btn-primary" OnClick="SMSButton_Click" ValidationGroup="SP" />
                    <input id="Submit1" class="btn btn-primary" type="button" value="Print" onclick='window.print();' />
                </div>
            </div>
        </div>
        <div class="tab-pane fade" id="panel1" role="tabpanel" aria-expanded="false">
            <div class="table-responsive">
                <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" PagerStyle-CssClass="pgr" DataKeyNames="StudentClassID,StudentID,ID" CssClass="mGrid">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllCheckBox" runat="server" Text="ALL" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <HeaderStyle CssClass="NoPrint" />
                            <ItemStyle Width="20px" CssClass="NoPrint" />
                        </asp:TemplateField>

                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                        <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                        <asp:BoundField DataField="StudentEmailAddress" HeaderText="Email" SortExpression="StudentEmailAddress" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                </asp:GridView>
                <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT Student.ID, Student.StudentsName, Student.StudentsLocalAddress, Student.FathersName, StudentsClass.RollNo, Student.SMSPhoneNo, Student.Gender, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, StudentsClass.StudentID, Student.StudentEmailAddress FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = 'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.StudentRegistrationID IS NULL) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="ShowIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, Student.StudentsLocalAddress, Student.FathersName, StudentsClass.RollNo, Student.SMSPhoneNo, Student.Gender, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, StudentsClass.StudentID, Student.StudentEmailAddress FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (Student.ID = @ID) AND (Student.Status = 'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (Student.StudentRegistrationID IS NULL) AND (StudentsClass.EducationYearID = @EducationYearID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="StuIdTextBox" Name="ID" PropertyName="Text" Type="String" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="ReSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Registration (SchoolID, UserName, Validation, Category, CreateDate) VALUES (@SchoolID,@UserName,'Valid','Student',GETDATE())
set @RegistrationID = SCOPE_IDENTITY() 
UPDATE   Student SET StudentRegistrationID = @RegistrationID  WHERE (StudentID = @StudentID)
INSERT INTO AST (RegistrationID, SchoolID, UserName, Category, Password) VALUES (@RegistrationID,@SchoolID,@UserName, 'Student',@Password)

INSERT INTO Education_Year_User (EducationYearID, SchoolID, RegistrationID) VALUES  (@EducationYear,@SchoolID, @RegistrationID)"
                    SelectCommand="SELECT * FROM [Student]">
                    <InsertParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYear" SessionField="Edu_Year" />
                        <asp:Parameter Name="UserName" />
                        <asp:Parameter Name="StudentID" />
                        <asp:Parameter Name="Password" />
                        <asp:Parameter Direction="Output" Name="RegistrationID" Size="100" />
                    </InsertParameters>
                </asp:SqlDataSource>
                <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="CU"></asp:CustomValidator>
            </div>

            <div class="form-inline" style="display: none;" id="Create">
                <div class="form-group">
                    <asp:Button ID="UserPasswordButton" runat="server" Text="Create User & Password" CssClass="btn btn-primary" OnClick="UserPasswordButton_Click" ValidationGroup="CU" />
                    <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                </div>
            </div>
        </div>
    </div>


    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>


    <script type="text/javascript">
        $(document).ready(function () {
            if ($('[id*=StudentsGridView] tr').length) {
                $("#Create").show();
            }
            if ($('[id*=StudentUserGV] tr').length) {
                $("#sms").show();
            }

            $('[id*=StuIdTextBox]').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "/Handeler/Student_IDs.asmx/GetStudentID",
                        data: JSON.stringify({ 'ids': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            result($.map(JSON.parse(response.d), function (item) {
                                return item;
                            }));
                        }
                    });
                }
            });

            var Class = "";
            if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                Class = " Class: " + $('[id*=ClassDropDownList] :selected').text() + ".";
            }

            var Section = "";
            if ($('[id*=SectionDropDownList] :selected').index() > 0) {
                Section = " Section: " + $('[id*=SectionDropDownList] :selected').text() + ".";
            }
            $("#Class_Section").text(Class + Section);


            $("[id*=AllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });

        /*--select at least one Checkbox Students GridView-----*/
        function Validate(d, c) {
            for (var b = document.getElementById("<%=StudentsGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };

        /*--select at least one Checkbox Students GridView-----*/
        function Validate2(d, c) {
            for (var b = document.getElementById("<%=StudentUserGV.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };
    </script>
</asp:Content>
