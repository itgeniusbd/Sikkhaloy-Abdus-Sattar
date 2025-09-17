<%@ Page Title="Manual Employee Attendance" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_Attendance.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_Attendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Attendances/TimePicker/jquery.timepicker.css" rel="stylesheet" />
    <style>
        .Show { display: none; }
        .Diable_Rows { background-color: #cdcdcd; color: #000; }
        .mGrid td table td { border: none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Employee Attendance (Insert/Update)</h3>
    <div class="form-inline">
        <div class="form-group">
            <asp:RadioButtonList ID="EmpTypeRadioButtonList" CssClass="form-control" runat="server" AutoPostBack="True" RepeatDirection="Horizontal" OnSelectedIndexChanged="EmpTypeRadioButtonList_SelectedIndexChanged">
                <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
                <asp:ListItem>Teacher</asp:ListItem>
                <asp:ListItem>Staff</asp:ListItem>
            </asp:RadioButtonList>
        </div>
        <div class="form-group">
            <asp:TextBox ID="AttendanceDateTextBox" autocomplete="off" placeholder="Attendance Date" runat="server" CssClass="form-control Datetime ml-1" onkeypress="return DisableAllKey()"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="AttendanceDateTextBox" CssClass="EroorSummer" ErrorMessage="*" SetFocusOnError="True" ValidationGroup="EA"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-primary" OnClick="FindButton_Click" />
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="EmployeeGridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID" DataSourceID="EmployeeSQL" Visible="False">
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ID="AllCheckBox" runat="server" Text="All" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="Attendance_CheckBox" runat="server" Text=" " />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                <asp:BoundField DataField="EmployeeType" HeaderText="Emp.Type" SortExpression="EmployeeType" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:TemplateField HeaderText="Attendance">
                    <ItemTemplate>
                        <asp:RadioButtonList ID="AttendenceRadioButtonList" runat="server" RepeatDirection="Horizontal">
                            <asp:ListItem Selected="True">Pre</asp:ListItem>
                            <asp:ListItem>Abs</asp:ListItem>
                            <asp:ListItem>Late</asp:ListItem>
                            <asp:ListItem>Late Abs</asp:ListItem>
                            <asp:ListItem>Leave</asp:ListItem>
                        </asp:RadioButtonList>
                        <asp:Label ID="AtDateLabel" runat="server" CssClass="EroorStar"></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Entry Time">
                    <ItemTemplate>
                        <asp:TextBox ID="StartTimeTextBox" runat="server" CssClass="form-control Time"></asp:TextBox>
                    </ItemTemplate>
                    <ItemStyle Width="165px" />
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Exit Time">
                    <ItemTemplate>
                        <asp:TextBox ID="EndTimeTextBox" runat="server" CssClass="form-control Time"></asp:TextBox>
                    </ItemTemplate>
                    <ItemStyle Width="165px" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EmployeeID, ID, EmployeeType, Permanent_Temporary, Salary,  FirstName +' '+ LastName as Name, Designation, Phone, EmployeeType  FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Active')  AND (EmployeeType LIKE @EmployeeType)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
        <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any Employee from Employee list." ForeColor="Red" ValidationGroup="EA"> </asp:CustomValidator>
    </div>

    <asp:Button ID="AttendanceButton" runat="server" CssClass="btn btn-primary Show" Text="Submit" OnClick="AttendanceButton_Click" ValidationGroup="EA" />
    <asp:SqlDataSource ID="Attendance_RecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT * FROM Employee_Attendance_Record WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceDate = @AttendanceDate))
BEGIN
INSERT INTO [Employee_Attendance_Record] ([SchoolID], [RegistrationID], [EmployeeID], [AttendanceStatus], [AttendanceDate], [EntryTime], [ExitTime]) VALUES (@SchoolID, @RegistrationID, @EmployeeID, @AttendanceStatus, @AttendanceDate, @EntryTime, @ExitTime)
END
ELSE
BEGIN
UPDATE  Employee_Attendance_Record SET ExitTime =@ExitTime, EntryTime =@EntryTime, AttendanceStatus =@AttendanceStatus WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AttendanceDate = @AttendanceDate)
END"
        SelectCommand="SELECT * FROM [Employee_Attendance_Record]">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:Parameter Name="EmployeeID" Type="Int32" />
            <asp:Parameter Name="AttendanceStatus" Type="String" />
            <asp:Parameter Name="AttendanceDate" Type="DateTime" />
            <asp:Parameter Name="EntryTime" Type="DateTime" />
            <asp:Parameter Name="ExitTime" Type="DateTime" />
        </InsertParameters>
    </asp:SqlDataSource>


    <script src="../Attendances/TimePicker/jquery.timepicker.js"></script>
    <script>
        //Time Picker
        $(function () {
            $(".Time").timepicker();

            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true

            });
            if ($('[id*=EmployeeGridView] tr').length) {
                $(".Show").show();
            }

            //Select All Checkbox
            $("[id*=AllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=Attendance_CheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=chkHeader]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=chkRow]", a).length == $("[id*=chkRow]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });

            $("[id*=AttendenceRadioButtonList]").click(function () {
                var td = $("td", $(this).parent().closest("table").closest("tr"));

                if ($(this).val() == "Leave" || $(this).val() == "Abs") {
                    $("[id*=StartTimeTextBox]", td).val("").attr("disabled", true);
                    $("[id*=EndTimeTextBox]", td).val("").attr("disabled", true);
                }
                else {
                    $("[id*=StartTimeTextBox]", td).val("").attr("disabled", false);
                    $("[id*=EndTimeTextBox]", td).val("").attr("disabled", false);
                }
            })
        });


        //Select at least one Checkbox from GridView
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

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
