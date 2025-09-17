<%@ Page Title="Employee Attendance Schedule" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Attendance_Schedule.aspx.cs" Inherits="EDUCATION.COM.Employee.Attendance_Schedule" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Attendances/TimePicker/jquery.timepicker.css" rel="stylesheet" />
    <style>
        .No_Schedule { background-color: #f78693; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Assign Employee Attendance Schedule</h3>
    <a class="blue-text" href="../Attendances/Absence_Fee_Manage.aspx">
        <i class="fa fa-plus-circle" aria-hidden="true"></i>
        Add/Change Schedule
    </a>

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
            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ScheduleDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="A"></asp:RequiredFieldValidator>
        </div>

        <div class="form-group">
            <asp:RadioButtonList ID="EmpTypeRadioButtonList" CssClass="form-control" runat="server" AutoPostBack="True" RepeatDirection="Horizontal">
                <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
                <asp:ListItem>Teacher</asp:ListItem>
                <asp:ListItem>Staff</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="EmployeeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID" DataSourceID="EmployeeSQL">
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox1" runat="server" Text=" " />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="AddCheckBox" Checked='<%#Bind("IsSchedule") %>' runat="server" Text=" " />
                        <input class="IsNotAssign" type="hidden" value='<%# Eval("Employee_Schedule_AssignID") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="DeviceID" HeaderText="Device ID" SortExpression="DeviceID" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />

                <asp:TemplateField HeaderText="RFID Card No.">
                    <ItemTemplate>
                        <asp:TextBox ID="RFIDTextBox" runat="server" Text='<%# Bind("RFID") %>' CssClass="form-control"></asp:TextBox>
                    </ItemTemplate>
                    <ItemStyle Width="200px" />
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Late">
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox2" runat="server" Text="Late" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="LateCheckBox" Checked='<%#Bind("Is_Late_SMS") %>' runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="50px" CssClass="NoPrint" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Abs">
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox3" runat="server" Text="Abs" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="AbsCheckBox" Checked='<%#Bind("Is_Abs_SMS") %>' runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle CssClass="NoPrint" />
                    <ItemStyle Width="50px" CssClass="NoPrint" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT VW_Emp_Info.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.DeviceID, VW_Emp_Info.EmployeeType, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.Phone, VW_Emp_Info.RFID, ISNULL(S_T.Is_Abs_SMS, 0) AS Is_Abs_SMS, ISNULL(S_T.Is_Late_SMS, 0) AS Is_Late_SMS, CAST(CASE WHEN S_T.EmployeeID IS NULL THEN 0 ELSE 1 END AS BIT) AS IsSchedule, Employee_Attendance_Schedule_Assign_1.Employee_Schedule_AssignID FROM VW_Emp_Info LEFT OUTER JOIN Employee_Attendance_Schedule_Assign AS Employee_Attendance_Schedule_Assign_1 ON VW_Emp_Info.EmployeeID = Employee_Attendance_Schedule_Assign_1.EmployeeID LEFT OUTER JOIN (SELECT EmployeeID, Is_Abs_SMS, Is_Late_SMS FROM Employee_Attendance_Schedule_Assign WHERE (ScheduleID = @ScheduleID)) AS S_T ON VW_Emp_Info.EmployeeID = S_T.EmployeeID WHERE (VW_Emp_Info.SchoolID = @SchoolID) AND (VW_Emp_Info.Job_Status = N'Active') AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType)" UpdateCommand="UPDATE Employee_Info SET RFID = @RFID WHERE (EmployeeID = @EmployeeID) AND (SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="RFID" />
                <asp:Parameter Name="EmployeeID" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </UpdateParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="Schedule_AssignSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Employee_Attendance_Schedule_Assign WHERE (EmployeeID = @EmployeeID) AND (ScheduleID = @ScheduleID)" InsertCommand="DELETE FROM Employee_Attendance_Schedule_Assign WHERE (EmployeeID = @EmployeeID) 

INSERT INTO Employee_Attendance_Schedule_Assign
                         (SchoolID, RegistrationID, EmployeeID, ScheduleID, Is_Abs_SMS, Is_Late_SMS)
VALUES        (@SchoolID,@RegistrationID,@EmployeeID,@ScheduleID,@Is_Abs_SMS,@Is_Late_SMS)"
            SelectCommand="SELECT Employee_Schedule_AssignID, SchoolID, RegistrationID, EmployeeID, ScheduleID, CreateDate FROM Employee_Attendance_Schedule_Assign WHERE (SchoolID = @SchoolID) AND (ScheduleID = @ScheduleID) AND (EmployeeID = @EmployeeID)">
            <DeleteParameters>
                <asp:Parameter Name="EmployeeID" />
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" />
            </DeleteParameters>
            <InsertParameters>
                <asp:Parameter Name="EmployeeID" Type="Int32" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" Type="Int32" />
                <asp:Parameter Name="Is_Abs_SMS" />
                <asp:Parameter Name="Is_Late_SMS" />
            </InsertParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" />
                <asp:Parameter Name="EmployeeID" />
            </SelectParameters>
        </asp:SqlDataSource>

            <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END" SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                    <asp:Parameter DefaultValue="Employee RFID/Schedule" Name="UpdateType" Type="String" />
                    <asp:Parameter DefaultValue="Change Employee RFID or Schedule" Name="UpdateDescription" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>

        <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any employee from employee list." ForeColor="Red" ValidationGroup="A"> </asp:CustomValidator>
    </div>
    <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Submit" ValidationGroup="A" />

    <script>
        $(function () {
            //Select All Checkbox
            $("[id*=AllCheckBox1]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=AddCheckBox]", b).each(function () {
                    a.is(":checked") ? ($(this).prop('checked', true).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).prop('checked', false).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=AllCheckBox2]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=LateCheckBox]", b).each(function () {
                    a.is(":checked") ? $(this).prop('checked', true).attr("checked", "checked") : $(this).prop('checked', false).removeAttr("checked");
                });
            });

            $("[id*=AllCheckBox3]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=AbsCheckBox]", b).each(function () {
                    a.is(":checked") ? $(this).prop('checked', true).attr("checked", "checked") : $(this).prop('checked', false).removeAttr("checked");
                });
            });

            //For Color change
            $("[id*=AddCheckBox]").on("click", function () {
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected")) : ($("td", $(this).closest("tr")).removeClass("selected"));
            });

            //Is Schedule
            $("[id*=AddCheckBox]").each(function () {
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

        //Select at least one Checkbox Students GridView
        function Validate(d, c) {
            if ($('[id*=EmployeeGridView] tr').length) {
                for (var b = document.getElementById("<%=EmployeeGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                    if ("checkbox" == b[a].type && b[a].checked) {
                        c.IsValid = !0;
                        return;
                    }
                }
                c.IsValid = !1;
            }
        }
    </script>
</asp:Content>
