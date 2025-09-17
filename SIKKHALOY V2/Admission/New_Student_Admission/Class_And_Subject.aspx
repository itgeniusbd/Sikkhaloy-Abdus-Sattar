<%@ Page Title="Academic Information" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Class_And_Subject.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Class_And_Subject" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3 class="mb-2">Academic Information</h3>
            <h4 class="mb-3">
                <span id="id" class="badge pink"></span>
                <span id="name" class="badge pink"></span>
                <span id="Fname" class="badge pink"></span>
            </h4>

            <asp:FormView ID="RollNo_FormView" DataSourceID="RollShowSQL" runat="server" Width="100%">
                <ItemTemplate>
                    <div class="alert alert-primary font-weight-bold">Last Entry Roll No. <%# Eval("RollNo") %></div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="RollShowSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT TOP (1) StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.EducationYearID = @Education_YearID) AND (Student.Status = N'Active') ORDER BY StudentsClass.StudentClassID DESC">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:CookieParameter CookieName="Admission_Year" Name="Education_YearID" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="GroupDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%')">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="GRequiredFieldValidator" runat="server" ControlToValidate="GroupDropDownList" CssClass="EroorStar" ErrorMessage="Select group" InitialValue="%" ValidationGroup="1" Visible="False">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="SectionDropDownList" runat="server" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" AutoPostBack="True" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group mx-1">
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
                    <asp:TextBox ID="RollNumberTextBox" runat="server" CssClass="form-control" placeholder="Enter Roll Number"></asp:TextBox>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="GroupGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SubjectID,SubjectType" DataSourceID="SubjectGroupSQL">
                    <Columns>
                        <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName">
                            <HeaderStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Select" ShowHeader="False">
                            <ItemTemplate>
                                <asp:CheckBox ID="SubjectCheckBox" runat="server" Text=" " Checked='<%# Bind("Check") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" Width="30px" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="This Subject Is">
                            <ItemTemplate>
                                <asp:RadioButtonList ID="SubjectTypeRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("SubjectType") %>'>
                                    <asp:ListItem>Compulsory</asp:ListItem>
                                    <asp:ListItem>Optional</asp:ListItem>
                                </asp:RadioButtonList>
                            </ItemTemplate>
                            <ItemStyle Width="175px" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SubjectGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Subject.SubjectID, Subject.SubjectName, SubjectForGroup.SubjectType, CAST((CASE WHEN SubjectForGroup.SubjectType = 'Compulsory' THEN 1 ELSE 0 END)AS BIT) AS [Check] FROM Subject INNER JOIN SubjectForGroup ON Subject.SubjectID = SubjectForGroup.SubjectID WHERE (Subject.SchoolID = @SchoolID) AND (SubjectForGroup.ClassID = @ClassID) AND (SubjectForGroup.SubjectGroupID = (CASE WHEN @SubjectGroupID = '%' THEN 0 ELSE @SubjectGroupID END)) ORDER BY SubjectForGroup.SubjectType, Subject.SubjectName">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <asp:SqlDataSource ID="StudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS (SELECT * FROM  StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID))
BEGIN
INSERT INTO [StudentsClass] ([SchoolID], [RegistrationID], [StudentID], [ClassID], [SectionID], [ShiftID], [SubjectGroupID], [RollNo], [EducationYearID], [Date]) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, Getdate())
END"
                SelectCommand="SELECT * FROM StudentsClass ">
                <InsertParameters>
                    <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" Type="Int32" />
                    <asp:CookieParameter CookieName="Admission_Year" Name="EducationYearID" Type="String" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:SessionParameter Name="SectionID" SessionField="SectionID" Type="String" />
                    <asp:SessionParameter Name="ShiftID" SessionField="ShiftID" Type="String" />
                    <asp:SessionParameter Name="SubjectGroupID" SessionField="GroupID" Type="String" />
                    <asp:ControlParameter ControlID="RollNumberTextBox" Name="RollNo" PropertyName="Text" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="StudentRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                InsertCommand="IF NOT EXISTS (SELECT  * FROM  StudentRecord WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID) AND (SubjectID = @SubjectID))
BEGIN
INSERT INTO StudentRecord(SchoolID, RegistrationID, StudentID, StudentClassID, SubjectID, EducationYearID, Date, SubjectType) VALUES (@SchoolID, @RegistrationID, @StudentID, @StudentClassID, @SubjectID, @EducationYearID, GETDATE(), @SubjectType)
END"
                SelectCommand="SELECT * FROM [StudentRecord]">
                <InsertParameters>
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:CookieParameter CookieName="Admission_Year" Name="EducationYearID" />
                    <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" Type="Int32" />
                    <asp:Parameter Name="SubjectID" Type="Int32" />
                    <asp:Parameter Name="SubjectType" />
                </InsertParameters>
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

            <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END" SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                    <asp:Parameter DefaultValue="New Student" Name="UpdateType" Type="String" />
                    <asp:Parameter DefaultValue="Admission new student" Name="UpdateDescription" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>

            <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
            <asp:CheckBox ID="SMSCheckBox" runat="server" Text="Send admission confirmation SMS" />
            <asp:CheckBox ID="PrintCheckBox" runat="server" Text="Print Admission Form" />

            <div class="form-inline">
                <div class="form-group">
                    <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Save &amp; Compleate Without Payorder" ValidationGroup="1" />
                </div>
                <div class="form-group">
                    <asp:Button ID="GoPayorderButton" runat="server" CssClass="btn btn-primary" OnClick="GoPayorderButton_Click" Text="Save &amp; Continue to Payorder" ValidationGroup="1" />
                    <asp:ValidationSummary ID="ValidationSummary2" runat="server" CssClass="EroorSummer" DisplayMode="List" ShowMessageBox="True" ShowSummary="False" ValidationGroup="1" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

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

    <script type="text/javascript">
        //Query String
        function getQstring(name, url) {
            if (!url) url = window.location.href;
            name = name.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        }

        $("#id").text("ID: " + getQstring('ID'));
        $("#name").text("Name: " + getQstring('Name'));
        $("#Fname").text("Father's Name: " + getQstring('Fname'));

        //Disable the submit button after clicking
        $("form").submit(function () {
            $(".btn btn-primary").attr("disabled", true);
            setTimeout(function () {
                $(".btn btn-primary").prop('disabled', false);
            }, 2000); // 2 seconds
            return true;
        });

        function noBack() {
            window.history.forward();
        }
        noBack();
        window.onload = noBack;
        window.onpageshow = function (evt) {
            if (evt.persisted) noBack();
        }
        window.onunload = function () { void (0); }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $("#id").text("ID: " + getQstring('ID'));
            $("#name").text("Name: " + getQstring('Name'));
            $("#Fname").text("Father's Name: " + getQstring('Fname'));
        })
    </script>
</asp:Content>

