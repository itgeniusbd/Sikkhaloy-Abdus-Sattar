<%@ Page Title="Student Attendance Scheduling" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Schedule_AssignStudent.aspx.cs" Inherits="EDUCATION.COM.Attendances.Schedule_AssignStudent" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .No_Schedule { background-color: #f78693; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Assign Student Attendance Schedule
        <asp:Label ID="CGSSLabel" runat="server"></asp:Label>
    </h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ScheduleDropDownList" runat="server" AppendDataBoundItems="True" DataSourceID="ScheduleSQL" DataTextField="ScheduleName" DataValueField="ScheduleID" CssClass="form-control" AutoPostBack="True">
                <asp:ListItem Value="0">[ SELECT SCHEDULE ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ScheduleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ScheduleName, CONVERT (varchar(15), LateEntryTime, 100) AS LateEntryTime, CONVERT (varchar(15), StartTime, 100) AS StartTime, CONVERT (varchar(15), EndTime, 100) AS EndTime, ScheduleID FROM Attendance_Schedule WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>

            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="ScheduleDropDownList" CssClass="EroorStar" ErrorMessage="Select Schedule" ValidationGroup="1" InitialValue="0">*</asp:RequiredFieldValidator>
        </div>

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
            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
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
    </div>

    <div class="table-responsive">
        <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" PagerStyle-CssClass="pgr"
            DataKeyNames="StudentClassID,StudentID" CssClass="mGrid" DataSourceID="ShowStudentClassSQL" AllowSorting="True">
            <Columns>
                <asp:TemplateField HeaderText="Add">
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox1" runat="server" Text=" " />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="AddSch_SelectCheckBox" Checked='<%#Bind("IsSchedule") %>' runat="server" Text=" " />
                        <input class="IsNotAssign" type="hidden" value='<%# Eval("Schedule_AssignStuID") %>' />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="50px" CssClass="NoPrint" />
                </asp:TemplateField>
                <asp:BoundField DataField="DeviceID" HeaderText="Device ID" SortExpression="DeviceID" />
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="StudentsName" SortExpression="StudentsName" HeaderText="Name" />
                <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                <asp:TemplateField HeaderText="RFID Card No.">
                    <ItemTemplate>
                        <asp:TextBox ID="RFIDTextBox" runat="server" Text='<%#Bind("RFID") %>' CssClass="form-control"></asp:TextBox>
                    </ItemTemplate>
                    <ItemStyle Width="150px" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Pre">
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox2" runat="server" Text="Pre" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="Entry_SelectCheckBox" Checked='<%#Bind("Entry_Confirmation") %>' runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="50px" CssClass="NoPrint" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Late">
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox5" runat="server" Text="Late" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="Late_SelectCheckBox" Checked='<%#Bind("Is_Late_SMS") %>' runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="50px" CssClass="NoPrint" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Abs">
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox4" runat="server" Text="Abs" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="Abs_SelectCheckBox" Checked='<%#Bind("Is_Abs_SMS") %>' runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="50px" CssClass="NoPrint" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Exit">
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox3" runat="server" Text="Exit" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="Exit_SelectCheckBox" Checked='<%#Bind("Exit_Confirmation") %>' runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="50px" CssClass="NoPrint" />
                </asp:TemplateField>
            </Columns>

            <EmptyDataTemplate>
                Empty
            </EmptyDataTemplate>

            <PagerStyle CssClass="pgr" />
            <SelectedRowStyle CssClass="Selected" />
        </asp:GridView>

        <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        Student.StudentID, StudentsClass.StudentClassID, Student.DeviceID, Student.ID, Student.RFID, Student.StudentsName, StudentsClass.RollNo, ISNULL(Sche_T.Entry_Confirmation, 0) AS Entry_Confirmation, 
                         ISNULL(Sche_T.Exit_Confirmation, 0) AS Exit_Confirmation, ISNULL(Sche_T.Is_Abs_SMS, 0) AS Is_Abs_SMS, ISNULL(Sche_T.Is_Late_SMS, 0) AS Is_Late_SMS, CAST(CASE WHEN Sche_T.StudentID IS NULL 
                         THEN 0 ELSE 1 END AS BIT) AS IsSchedule, ST.Schedule_AssignStuID
FROM            StudentsClass INNER JOIN
                         Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN
                             (SELECT        StudentID, Schedule_AssignStuID
                               FROM            Attendance_Schedule_AssignStudent
                               WHERE        (SchoolID = @SchoolID)) AS ST ON Student.StudentID = ST.StudentID LEFT OUTER JOIN
                             (SELECT        StudentID, Entry_Confirmation, Exit_Confirmation, Is_Abs_SMS, Is_Late_SMS
                               FROM            Attendance_Schedule_AssignStudent AS Attendance_Schedule_AssignStudent_1
                               WHERE        (SchoolID = @SchoolID) AND (ScheduleID = @ScheduleID)) AS Sche_T ON Student.StudentID = Sche_T.StudentID
WHERE        (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND
                          (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
            <SelectParameters>
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                <asp:Parameter DefaultValue="Active" Name="Status" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>

        <asp:SqlDataSource ID="ScheduleAssignSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="DELETE FROM Attendance_Schedule_AssignStudent WHERE (SchoolID = @SchoolID) AND (StudentID = @StudentID)

INSERT INTO [Attendance_Schedule_AssignStudent] ([SchoolID], [RegistrationID], [ScheduleID], [StudentID], [Entry_Confirmation], [Exit_Confirmation],Is_Abs_SMS,Is_Late_SMS, [Date]) VALUES (@SchoolID, @RegistrationID, @ScheduleID, @StudentID, @Entry_Confirmation, @Exit_Confirmation, @Is_Abs_SMS,@Is_Late_SMS,GETDATE())"
            SelectCommand="SELECT * FROM [Attendance_Schedule_AssignStudent] WHERE (SchoolID = @SchoolID) AND (ScheduleID = @ScheduleID) AND (StudentID = @StudentID)" DeleteCommand="DELETE FROM Attendance_Schedule_AssignStudent WHERE (SchoolID = @SchoolID) AND (StudentID = @StudentID) AND (ScheduleID = @ScheduleID)" UpdateCommand="UPDATE Student SET RFID = @RFID WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID)">
            <DeleteParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:Parameter Name="StudentID" />
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" />
            </DeleteParameters>
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:Parameter Name="StudentID" Type="Int32" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" Type="Int32" />
                <asp:Parameter Name="Entry_Confirmation" Type="Boolean" />
                <asp:Parameter Name="Exit_Confirmation" Type="Boolean" />
                <asp:Parameter Name="Is_Abs_SMS" />
                <asp:Parameter Name="Is_Late_SMS" />
            </InsertParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" />
                <asp:Parameter Name="StudentID" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="RFID" />
                <asp:Parameter Name="StudentID" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </UpdateParameters>
        </asp:SqlDataSource>

        <asp:SqlDataSource ID="ConfSMS_UpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Attendance_Schedule_AssignStudent]" UpdateCommand="UPDATE Attendance_Schedule_AssignStudent SET Is_Abs_SMS = @Is_Abs_SMS, Is_Late_SMS = @Is_Late_SMS, Exit_Confirmation = @Exit_Confirmation, Entry_Confirmation = @Entry_Confirmation WHERE (ScheduleID = @ScheduleID) AND (StudentID = @StudentID)">
            <UpdateParameters>
                <asp:Parameter Name="Is_Abs_SMS" />
                <asp:Parameter Name="Is_Late_SMS" />
                <asp:Parameter Name="Exit_Confirmation" />
                <asp:Parameter Name="Entry_Confirmation" />
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" />
                <asp:Parameter Name="StudentID" />
            </UpdateParameters>
        </asp:SqlDataSource>

            <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END" SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                    <asp:Parameter DefaultValue="Student RFID/Schedule" Name="UpdateType" Type="String" />
                    <asp:Parameter DefaultValue="Change Student RFID or Schedule" Name="UpdateDescription" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>

        <%if (StudentsGridView.Rows.Count > 0)
            {%>
        <br />
        <asp:Button ID="AssignButton" runat="server" CssClass="btn btn-primary" OnClick="AssignButton_Click" Text="Assign/Update" ValidationGroup="1" />
        <%}%>
        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="EroorSummer" DisplayMode="List" ShowMessageBox="True" ValidationGroup="1" />
    </div>


    <script>
        $(function () {
            $("[id*=AllCheckBox1]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=AddSch_SelectCheckBox]", b).each(function () {
                    a.is(":checked") ? ($(this).prop('checked', true).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).prop('checked', false).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=AllCheckBox2]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=Entry_SelectCheckBox]", b).each(function () {
                    a.is(":checked") ? $(this).prop('checked', true).attr("checked", "checked") : $(this).prop('checked', false).removeAttr("checked");
                });
            });

            $("[id*=AllCheckBox3]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=Exit_SelectCheckBox]", b).each(function () {
                    a.is(":checked") ? $(this).prop('checked', true).attr("checked", "checked") : $(this).prop('checked', false).removeAttr("checked");
                });
            });

            $("[id*=AllCheckBox4]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=Abs_SelectCheckBox]", b).each(function () {
                    a.is(":checked") ? $(this).prop('checked', true).attr("checked", "checked") : $(this).prop('checked', false).removeAttr("checked");
                });
            });

            $("[id*=AllCheckBox5]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=Late_SelectCheckBox]", b).each(function () {
                    a.is(":checked") ? $(this).prop('checked', true).attr("checked", "checked") : $(this).prop('checked', false).removeAttr("checked");
                });
            });
            //For Color change
            $("[id*=AddSch_SelectCheckBox]").on("click", function () {
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected")) : ($("td", $(this).closest("tr")).removeClass("selected"));
            });

            //Is Schedule
            $("[id*=AddSch_SelectCheckBox]").each(function () {
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected")) : ($("td", $(this).closest("tr")).removeClass("selected"));
            });

            //No Assign in schedule
            $(".IsNotAssign").each(function () {
                var IsAssign = $(this).val();
                if (IsAssign === "") {
                    $("td", $(this).closest("tr")).addClass("No_Schedule");
                }
            });
        });
    </script>
</asp:Content>
