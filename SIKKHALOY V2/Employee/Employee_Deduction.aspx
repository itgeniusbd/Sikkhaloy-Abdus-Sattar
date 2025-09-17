<%@ Page Title="Deduction" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_Deduction.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_Deduction" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid td table td { border: none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Assign Deduction</h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="DeductionDropDownList" runat="server" CssClass="form-control" DataSourceID="DeductionNameSQL" DataTextField="DeductionName" DataValueField="DeductionID" AutoPostBack="True" OnDataBound="DeductionDropDownList_DataBound">
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
        <asp:GridView ID="EmployeeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID" DataSourceID="TeachersSQL">
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
                <asp:BoundField DataField="Salary" HeaderText="Salary" SortExpression="Salary" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:TemplateField HeaderText="Amount">
                    <HeaderTemplate>
                        <input id="SetAmountText" onkeypress="return isNumberKey(event)" autocomplete="off" type="text" placeholder="Set All Amount" class="form-control form-control-sm" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:TextBox ID="DeductionAmountTextBox" pattern="[1-9][0-9]*" ToolTip="0 Not Allow" placeholder="Deduction Amount" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" Text='<%# Bind("DeductionAmount") %>'></asp:TextBox>
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
        <asp:SqlDataSource ID="TeachersSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        VW_Emp_Info.EmployeeID, T_A.DeductionID, CAST(CASE WHEN T_A.DeductionID IS NOT NULL THEN 1 ELSE 0 END AS bit) AS IS_Checked, VW_Emp_Info.ID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, 
                         VW_Emp_Info.Designation, VW_Emp_Info.EmployeeType, VW_Emp_Info.Phone, VW_Emp_Info.Salary, ISNULL(T_A.Fixed_Percetage, 'Fixed') AS Fixed_Percetage, T_A.DeductionAmount
FROM            VW_Emp_Info LEFT OUTER JOIN
                             (SELECT        DeductionID, EmployeeID, DeductionAmount, Fixed_Percetage
                               FROM            Employee_Deduction_Assign
                               WHERE        (DeductionID = @DeductionID)) AS T_A ON VW_Emp_Info.EmployeeID = T_A.EmployeeID
WHERE        (VW_Emp_Info.SchoolID = @SchoolID) AND (VW_Emp_Info.Job_Status = N'Active') AND (VW_Emp_Info.EmployeeType LIKE @EmployeeType)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="DeductionDropDownList" Name="DeductionID" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <asp:Button ID="AssignButton" runat="server" CssClass="btn btn-primary" Text="Submt" ValidationGroup="A" OnClick="AssignButton_Click" />
    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="DeductionDropDownList" CssClass="EroorSummer" ErrorMessage="Select Deduction" InitialValue="0" ValidationGroup="A"></asp:RequiredFieldValidator>

    <asp:SqlDataSource ID="Deduction_AssignSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Employee_Deduction_Assign WHERE (EmployeeID = @EmployeeID) AND (DeductionID = @DeductionID)" InsertCommand="IF EXISTS(SELECT DeductionAssignID FROM Employee_Deduction_Assign WHERE (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (DeductionID = @DeductionID))
BEGIN
UPDATE       Employee_Deduction_Assign
SET          RegistrationID = @RegistrationID, DeductionAmount = @DeductionAmount, Fixed_Percetage = @Fixed_Percetage
WHERE        (SchoolID = @SchoolID) AND (EmployeeID = @EmployeeID) AND (DeductionID = @DeductionID)
END
ELSE
BEGIN
INSERT INTO [Employee_Deduction_Assign] ([SchoolID], [RegistrationID], [DeductionID], [EmployeeID], [DeductionAmount], [Fixed_Percetage]) VALUES (@SchoolID, @RegistrationID, @DeductionID, @EmployeeID, @DeductionAmount, @Fixed_Percetage)
END" SelectCommand="SELECT * FROM [Employee_Deduction_Assign]">
        <DeleteParameters>
            <asp:Parameter Name="EmployeeID" />
            <asp:ControlParameter ControlID="DeductionDropDownList" Name="DeductionID" PropertyName="SelectedValue" />
        </DeleteParameters>
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="DeductionDropDownList" Name="DeductionID" PropertyName="SelectedValue" Type="Int32" />
            <asp:Parameter Name="EmployeeID" Type="Int32" />
            <asp:Parameter Name="DeductionAmount" />
            <asp:Parameter Name="Fixed_Percetage"/>
        </InsertParameters>
    </asp:SqlDataSource>


    <!-- Modal -->
    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog cascading-modal" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="title"><i class="fa fa-plus"></i>Deduction Name</h4>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body mb-0">
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:TextBox ID="DeductionNameTextBox" placeholder="Deduction Name" runat="server" CssClass="form-control"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="DeductionNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AA"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="AddDeductionButton" runat="server" CssClass="btn btn-primary" Text="Add" ValidationGroup="AA" OnClick="AddDeductionButton_Click" />
                            <asp:SqlDataSource ID="DeductionNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="IF NOT EXISTS(SELECT  TOP(1) DeductionID FROM  Employee_Deduction_Records WHERE (DeductionID = @DeductionID)) 
IF NOT EXISTS(SELECT  TOP(1) DeductionID FROM  Employee_Deduction_Assign WHERE (DeductionID = @DeductionID)) 
BEGIN
DELETE FROM [Employee_Deduction] WHERE [DeductionID] = @DeductionID
END" InsertCommand="IF NOT EXISTS (SELECT * FROM Employee_Deduction WHERE SchoolID = @SchoolID AND DeductionName = @DeductionName)
BEGIN
INSERT INTO [Employee_Deduction] ([SchoolID], [RegistrationID], [DeductionName]) VALUES (@SchoolID, @RegistrationID, @DeductionName)
END"
                                SelectCommand="SELECT DeductionID, SchoolID, RegistrationID, DeductionName, CreateDate FROM Employee_Deduction WHERE (SchoolID = @SchoolID)" UpdateCommand="IF NOT EXISTS (SELECT * FROM Employee_Deduction WHERE SchoolID = @SchoolID AND DeductionName = @DeductionName)
BEGIN
UPDATE Employee_Deduction SET DeductionName = @DeductionName WHERE (DeductionID = @DeductionID)
END">
                                <DeleteParameters>
                                    <asp:Parameter Name="DeductionID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                    <asp:ControlParameter ControlID="DeductionNameTextBox" Name="DeductionName" PropertyName="Text" Type="String" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:Parameter Name="DeductionName" />
                                    <asp:Parameter Name="DeductionID" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>
                    <asp:GridView ID="AllownceNameGridView" runat="server" AllowPaging="True" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="DeductionID" DataSourceID="DeductionNameSQL">
                        <Columns>
                            <asp:CommandField ShowEditButton="True" />
                            <asp:BoundField DataField="DeductionName" HeaderText="Deduction Name" SortExpression="DeductionName" />
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
                $("[id*=DeductionAmountTextBox]").val($.trim($(this).val()));
            });

            //checl Amount %
            $("[id*=DeductionAmountTextBox]").on("keyup", function () {
                var Value = $('[id*=Fixed_PercetageRadioButtonList] input[type=radio]:checked', $(this).closest("tr")).val();
                if (Value === "Percentage") {
                    if ($(this).val() > 100) {
                        $(this).val(100);
                    }
                }
            });

            $("[id*=Fixed_PercetageRadioButtonList] input").on("click", function () {
                if ($(this).val() === "Percentage") {
                    var Value = $('[id*=DeductionAmountTextBox]', $(this).parents("tr"));
                    if (Value.val() > 100) {
                        Value.val(100);
                    }
                }
            });
        });


        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
