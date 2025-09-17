<%@ Page Title="Allowance" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_Allowance.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_Allowance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid td table td { border: none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Assign Allowance</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="AllowanceDropDownList" runat="server" CssClass="form-control" DataSourceID="AllowanceNameSQL" DataTextField="AllowanceName" DataValueField="AllowanceID" AutoPostBack="True" OnDataBound="AllowanceDropDownList_DataBound">
            </asp:DropDownList>
            <button type="button" data-toggle="modal" data-target="#myModal" class="btn btn-warning"><i class="fa fa-plus-square mr-1"></i>Add New</button>
        </div>
        <div class="form-group">
            <asp:RadioButtonList ID="EmpTypeRadioButtonList" runat="server" AutoPostBack="True" CssClass="form-control" RepeatDirection="Horizontal">
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
                        <asp:CheckBox ID="AllCheckBox" runat="server" Text=" " />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="AddCheckBox" runat="server" Text=" " Checked='<%# Bind("IS_Checked") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                <asp:BoundField DataField="EmployeeType" HeaderText="Emp.Type" SortExpression="EmployeeType" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Salary" HeaderText="Salary" SortExpression="Salary" />
                <asp:TemplateField HeaderText="Amount">
                    <HeaderTemplate>
                        <input id="SetAmountText" onkeypress="return isNumberKey(event)" autocomplete="off" type="text" placeholder="Set All Amount" class="form-control form-control-sm" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:TextBox ID="AllowanceAmountTextBox" pattern="[1-9][0-9]*" ToolTip="0 Not Allow" placeholder="Allowance Amount" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" Text='<%# Bind("AllowanceAmount") %>'></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Fixed/Percetage">
                    <ItemTemplate>
                        <asp:RadioButtonList ID="Fixed_PercetageRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("Fixed_Percetage") %>'>
                            <asp:ListItem>Fixed</asp:ListItem>
                            <asp:ListItem>Percentage</asp:ListItem>
                        </asp:RadioButtonList>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT VW_Emp_Info.EmployeeID, T_A.AllowanceID, CAST(CASE WHEN T_A.AllowanceID IS NOT NULL THEN 1 ELSE 0 END AS bit) AS IS_Checked, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, 
VW_Emp_Info.Designation, VW_Emp_Info.EmployeeType, VW_Emp_Info.Phone, VW_Emp_Info.Salary, ISNULL(T_A.Fixed_Percetage, 'Fixed') AS Fixed_Percetage, T_A.AllowanceAmount
FROM   VW_Emp_Info LEFT OUTER JOIN (SELECT AllowanceID, EmployeeID, Fixed_Percetage, AllowanceAmount FROM  Employee_Allowance_Assign WHERE  (AllowanceID = @AllowanceID)) AS T_A ON VW_Emp_Info.EmployeeID = T_A.EmployeeID
WHERE  (VW_Emp_Info.SchoolID = @SchoolID) AND (VW_Emp_Info.Job_Status = N'Active') AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType)">
            <SelectParameters>
                <asp:ControlParameter ControlID="AllowanceDropDownList" Name="AllowanceID" PropertyName="SelectedValue" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <div class="Show" style="display: none">
        <asp:Button ID="AssignButton" runat="server" CssClass="btn btn-primary" Text="Submt" ValidationGroup="A" OnClick="AssignButton_Click" />
        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="AllowanceDropDownList" CssClass="EroorSummer" ErrorMessage="Select Allowance" InitialValue="0" ValidationGroup="A"></asp:RequiredFieldValidator>
    </div>

    <asp:SqlDataSource ID="Allowance_AssignSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Employee_Allowance_Assign WHERE (EmployeeID = @EmployeeID) AND (AllowanceID = @AllowanceID)" InsertCommand="IF EXISTS(SELECT AllowanceAssignID FROM Employee_Allowance_Assign WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AllowanceID = @AllowanceID)) BEGIN UPDATE Employee_Allowance_Assign SET RegistrationID = @RegistrationID, AllowanceAmount = @AllowanceAmount, Fixed_Percetage = @Fixed_Percetage WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (AllowanceID = @AllowanceID) END ELSE BEGIN INSERT INTO Employee_Allowance_Assign (SchoolID, RegistrationID, AllowanceID, EmployeeID, AllowanceAmount, Fixed_Percetage) VALUES (@SchoolID,@RegistrationID,@AllowanceID,@EmployeeID,@AllowanceAmount,@Fixed_Percetage) END" SelectCommand="SELECT * FROM Employee_Allowance_Assign WHERE (EmployeeID = @EmployeeID) AND (AllowanceID = @AllowanceID) AND (SchoolID = @SchoolID)">
        <DeleteParameters>
            <asp:Parameter Name="EmployeeID" />
            <asp:ControlParameter ControlID="AllowanceDropDownList" Name="AllowanceID" PropertyName="SelectedValue" />
        </DeleteParameters>
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:Parameter Name="EmployeeID" Type="Int32" />
            <asp:ControlParameter ControlID="AllowanceDropDownList" Name="AllowanceID" PropertyName="SelectedValue" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:Parameter Name="AllowanceAmount"/>
            <asp:Parameter Name="Fixed_Percetage"/>
        </InsertParameters>
        <SelectParameters>
            <asp:Parameter Name="EmployeeID" />
            <asp:ControlParameter ControlID="AllowanceDropDownList" Name="AllowanceID" PropertyName="SelectedValue" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>



    <!-- Modal -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog cascading-modal" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="title"><i class="fa fa-plus"></i>Allowance Name</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body mb-0">
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox ID="AllowanceNameTextBox" runat="server" placeholder="Allowance Name" CssClass="form-control"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="AllowanceNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AA"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="AddAllowanceButton" runat="server" CssClass="btn btn-primary" Text="Add" ValidationGroup="AA" OnClick="AddAllowanceButton_Click" />
                            <asp:SqlDataSource ID="AllowanceNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="IF NOT EXISTS(SELECT TOP (1) AllowanceAssignID FROM Employee_Allowance_Assign WHERE (SchoolID = @SchoolID) AND (AllowanceID = @AllowanceID))
DELETE FROM [Employee_Allowance] WHERE [AllowanceID] = @AllowanceID" InsertCommand="IF NOT EXISTS(SELECT * FROM Employee_Allowance WHERE SchoolID = @SchoolID AND AllowanceName = @AllowanceName)
BEGIN
INSERT INTO [Employee_Allowance] ([SchoolID], [RegistrationID], [AllowanceName]) VALUES (@SchoolID, @RegistrationID, @AllowanceName)
END"
                                SelectCommand="SELECT AllowanceID, SchoolID, RegistrationID, AllowanceName, CreateDate FROM Employee_Allowance WHERE (SchoolID = @SchoolID)" UpdateCommand="IF NOT EXISTS(SELECT * FROM Employee_Allowance WHERE SchoolID = @SchoolID AND AllowanceName = @AllowanceName)
BEGIN
UPDATE Employee_Allowance SET AllowanceName = @AllowanceName WHERE (AllowanceID = @AllowanceID)
END">
                                <DeleteParameters>
                                    <asp:Parameter Name="AllowanceID" Type="Int32" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                    <asp:ControlParameter ControlID="AllowanceNameTextBox" Name="AllowanceName" PropertyName="Text" Type="String" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="AllowanceName" />
                                    <asp:Parameter Name="AllowanceID" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

                    <asp:GridView ID="AllownceNameGridView" runat="server" AllowPaging="True" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="AllowanceID" DataSourceID="AllowanceNameSQL">
                        <Columns>
                            <asp:CommandField ShowEditButton="True" />
                            <asp:BoundField DataField="AllowanceName" HeaderText="Allowance Name" SortExpression="AllowanceName" />
                            <asp:BoundField DataField="CreateDate" DataFormatString="{0:d MMM yyyy}" ReadOnly="true" HeaderText="Created Date" SortExpression="CreateDate" />
                            <asp:CommandField ShowDeleteButton="True" />
                        </Columns>
                        <PagerStyle CssClass="pgr" />
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

    <script>
        $(function () {
            if ($('[id*=EmployeeGridView] tr').length) {
                $(".Show").show();
            }

            //onload
            $("input[type=checkbox]").each(function () {
                if ($(this).is(":checked")) {
                    $('input[type="text"]', $(this).closest("tr")).attr('required', 'required');
                    ($(this).closest("tr")).addClass("selected");
                }
            });

            //Select All Checkbox
            $("[id*=AllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ?
                    ($(this).prop('checked', true).attr("checked", "checked"),
                    $('input[type="text"]', $(this).closest("tr")).attr('required', 'required'),
                    $($(this).closest("tr")).addClass("selected"))
                    :
                    ($(this).prop('checked', false).removeAttr("checked"),
                    $('input[type="text"]', $(this).closest("tr")).removeAttr('required', 'required').val(''),
                    $($(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=AddCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllCheckBox]", a);
                $(this).is(":checked") ?
                (($(this).closest("tr")).addClass("selected"),
                $('input[type="text"]', $(this).closest("tr")).attr('required', 'required'),
                $("[id*=AddCheckBox]", a).length == $("[id*=AddCheckBox]:checked", a).length && b.attr("checked", "checked"))
                :
                ($($(this).closest("tr")).removeClass("selected"),
                $('input[type="text"]', $(this).closest("tr")).removeAttr('required', 'required').val(''),
                b.removeAttr("checked"));
            });

            //Set Amount
            $("#SetAmountText").on("keyup", function () {
                $("[id*=AllowanceAmountTextBox]").val($.trim($(this).val()));
            });

            //checl Amount %
            $("[id*=AllowanceAmountTextBox]").on("keyup", function () {
                var Value = $('[id*=Fixed_PercetageRadioButtonList] input[type=radio]:checked', $(this).closest("tr")).val();
                if (Value === "Percentage") {
                    if ($(this).val() > 100) {
                        $(this).val(100);
                    }
                }
            });

            $("[id*=Fixed_PercetageRadioButtonList] input").on("click", function () {
                if ($(this).val() === "Percentage") {
                    var Value = $('[id*=AllowanceAmountTextBox]', $(this).parents("tr"));
                    if (Value.val() > 100) {
                        Value.val(100);
                    }
                }
            });
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
