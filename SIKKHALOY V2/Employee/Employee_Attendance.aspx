<%@ Page Title="Manual Employee Attendance" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_Attendance.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_Attendance" %>



<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="../Attendances/TimePicker/jquery.timepicker.css" rel="stylesheet" />

    <style>

        .Show { display: none; }

        .hidden-employee-id { display: none; }

        .Diable_Rows, .mGrid tr.Diable_Rows td { background-color: #cdcdcd !important; color: #000 !important; }

        .mGrid td table td { border: none; }

    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3>Employee Attendance (Insert/Update)</h3>

    <div class="form-inline">

        <div class="form-group">

            <asp:DropDownList ID="ScheduleDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ScheduleSQL" DataTextField="ScheduleName" DataValueField="ScheduleID" OnSelectedIndexChanged="ScheduleDropDownList_SelectedIndexChanged">

                <asp:ListItem Value="0">[ SELECT SCHEDULE ]</asp:ListItem>

            </asp:DropDownList>

            <asp:SqlDataSource ID="ScheduleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ScheduleID, ScheduleName FROM Attendance_Schedule WHERE (SchoolID = @SchoolID) ORDER BY ScheduleName">

                <SelectParameters>

                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                </SelectParameters>

            </asp:SqlDataSource>

            <asp:RequiredFieldValidator ID="ScheduleRFV" runat="server" ControlToValidate="ScheduleDropDownList" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="EA" InitialValue="0"></asp:RequiredFieldValidator>

        </div>

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

            <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-primary" OnClick="FindButton_Click" ValidationGroup="EA" />

        </div>

    </div>



    <div class="table-responsive">

        <asp:GridView ID="EmployeeGridView" AllowSorting="false" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID" DataSourceID="EmployeeSQL" Visible="False" OnRowDataBound="EmployeeGridView_RowDataBound">

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

                <asp:TemplateField HeaderText="" ItemStyle-CssClass="hidden-employee-id">

                    <ItemTemplate>

                        <asp:HiddenField ID="EmployeeIDHidden" runat="server" Value='<%# Eval("EmployeeID") %>' />

                    </ItemTemplate>

                </asp:TemplateField>

                <asp:TemplateField HeaderText="Attendance">

                    <ItemTemplate>

                        <asp:RadioButtonList ID="AttendenceRadioButtonList" runat="server" RepeatDirection="Horizontal">

                            <asp:ListItem Value="Pre" Selected="True">Pre</asp:ListItem>

                            <asp:ListItem Value="Abs">Abs</asp:ListItem>

                            <asp:ListItem Value="Late">Late</asp:ListItem>

                            <asp:ListItem Value="Late Abs">Late Abs</asp:ListItem>

                            <asp:ListItem Value="Leave">Leave</asp:ListItem>

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

        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT VW_Emp_Info.EmployeeID, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, VW_Emp_Info.Permanent_Temporary, VW_Emp_Info.Salary, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.Phone, VW_Emp_Info.EmployeeType FROM VW_Emp_Info INNER JOIN Employee_Attendance_Schedule_Assign ON VW_Emp_Info.EmployeeID = Employee_Attendance_Schedule_Assign.EmployeeID AND VW_Emp_Info.SchoolID = Employee_Attendance_Schedule_Assign.SchoolID WHERE (VW_Emp_Info.SchoolID = @SchoolID) AND (VW_Emp_Info.Job_Status = N'Active') AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType) AND (Employee_Attendance_Schedule_Assign.ScheduleID = @ScheduleID) ORDER BY VW_Emp_Info.ID">

            <SelectParameters>

                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />

                <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" />

            </SelectParameters>

        </asp:SqlDataSource>

        <asp:CustomValidator ID="CV" runat="server" Enabled="False" ClientValidationFunction="Validate" ErrorMessage="You do not select any Employee from Employee list." ForeColor="Red" ValidationGroup="EA"> </asp:CustomValidator>

    </div>



    <asp:Button ID="AttendanceButton" runat="server" CssClass="btn btn-primary Show" Text="Submit" OnClick="AttendanceButton_Click" ValidationGroup="EA" />

    <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>



    <script src="../Attendances/TimePicker/jquery.timepicker.js"></script>

    <script>

        $(function () {

            $(".Time").timepicker();

            $(".Datetime").datepicker({

                format: 'dd M yyyy',

                todayBtn: "linked",

                todayHighlight: true,

                autoclose: true

            });

            showSubmitIfGridHasRows();

            bindEmployeeAttendanceHandlers();

        });



        function showSubmitIfGridHasRows() {

            if ($('[id*=EmployeeGridView] tr').length > 1) {

                $(".Show").show();

            }

        }



        function bindEmployeeAttendanceHandlers() {

            $("[id*=AllCheckBox]").off("click").on("click", function () {

                var a = $(this), b = $(this).closest("table");

                $("input[type=checkbox]", b).each(function () {

                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));

                });

            });



            $("[id*=Attendance_CheckBox]").off("click").on("click", function () {

                var a = $(this).closest("table"), b = $("[id*=chkHeader]", a);

                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=chkRow]", a).length == $("[id*=chkRow]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));

            });



            $("[id*=AttendenceRadioButtonList] input").off("click").on("click", function () {

                var td = $("td", $(this).closest("table").closest("tr"));

                if ($(this).val() == "Leave" || $(this).val() == "Abs") {

                    $("[id*=StartTimeTextBox]", td).val("").attr("disabled", true);

                    $("[id*=EndTimeTextBox]", td).val("").attr("disabled", true);

                }

                else {

                    $("[id*=StartTimeTextBox]", td).attr("disabled", false);

                    $("[id*=EndTimeTextBox]", td).attr("disabled", false);

                }

            });

        }



        function Validate(d, c) {

            if ($('[id*=EmployeeGridView] tr').length) {

                c.IsValid = !0;

            }

        }



        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

    </script>

</asp:Content>

