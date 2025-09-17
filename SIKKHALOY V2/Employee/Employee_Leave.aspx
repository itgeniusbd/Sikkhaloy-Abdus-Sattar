<%@ Page Title="Employee Leave" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_Leave.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_Leave" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <h3>Employee Leave</h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:RadioButtonList ID="EmpTypeRadioButtonList" CssClass="form-control" runat="server" AutoPostBack="True" RepeatDirection="Horizontal">
                <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
                <asp:ListItem>Teacher</asp:ListItem>
                <asp:ListItem>Staff</asp:ListItem>
            </asp:RadioButtonList>
        </div>
        <div class="form-group">
            <asp:TextBox ID="FindTextBox" runat="server" placeholder="Enter Search Keyword" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find"/>
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
               <asp:CheckBox ID="AddCheckBox" runat="server" Text=" " />
            </ItemTemplate>
         </asp:TemplateField>
         <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
         <asp:BoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName" />
         <asp:BoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName" />
         <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
         <asp:BoundField DataField="EmployeeType" HeaderText="Emp.Type" SortExpression="EmployeeType" />
         <asp:BoundField DataField="Salary" HeaderText="Salary" SortExpression="Salary" />
         <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
      </Columns>
   </asp:GridView>
   <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any employee from employee list." ForeColor="Red" ValidationGroup="A"> </asp:CustomValidator>
   <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EmployeeID, ID, EmployeeType, Permanent_Temporary, Salary, LastName, FirstName, Designation, Phone, EmployeeType  FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Active')  AND (EmployeeType LIKE @EmployeeType)"
      FilterExpression="ID LIKE '{0}%' or FirstName LIKE '{0}%' or LastName LIKE '{0}%' or Designation LIKE '{0}%' or Phone LIKE '{0}%'">
       <FilterParameters>
           <asp:ControlParameter ControlID="FindTextBox" Name="find" PropertyName="Text" />
       </FilterParameters>
      <SelectParameters>
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
      </SelectParameters>
   </asp:SqlDataSource>
</div>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:TextBox ID="StartDateTextBox" placeholder="From Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="StartDateTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="EndDateTextBox" placeholder="To Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="EndDateTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="LeaveReasonTextBox" TextMode="MultiLine" Height="35px" placeholder="Leave Reason" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="LeaveReasonTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="A"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Submit" ValidationGroup="A" />
            <asp:SqlDataSource ID="EmployyeLeaveSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [Employee_Leave] ([SchoolID], [RegistrationID], [EducationYearID], [EmployeeID], [LeaveStartDate], [LeaveEndDate], [LeaveReason], [ApproveStatus], [ApprovedBy_RegistrationID]) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @EmployeeID, @LeaveStartDate, @LeaveEndDate, @LeaveReason, @ApproveStatus, @ApprovedBy_RegistrationID)" SelectCommand="SELECT * FROM [Employee_Leave]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                    <asp:Parameter Name="EmployeeID" Type="Int32" />
                    <asp:ControlParameter ControlID="StartDateTextBox" DbType="Date" Name="LeaveStartDate" PropertyName="Text" />
                    <asp:ControlParameter ControlID="EndDateTextBox" DbType="Date" Name="LeaveEndDate" PropertyName="Text" />
                    <asp:ControlParameter ControlID="LeaveReasonTextBox" Name="LeaveReason" PropertyName="Text" Type="String" />
                    <asp:Parameter DefaultValue="Approved" Name="ApproveStatus" Type="String" />
                    <asp:SessionParameter DefaultValue="" Name="ApprovedBy_RegistrationID" SessionField="RegistrationID" Type="Int32" />
                </InsertParameters>
            </asp:SqlDataSource>
        </div>
    </div>
 
   <script>
       $(function () {
           $('.Datetime').datepicker({
               format: 'dd M yyyy',
               todayBtn: "linked",
               todayHighlight: true,
               autoclose: true
           });

           //Select All Checkbox
           $("[id*=AllCheckBox]").on("click", function () {
               var a = $(this), b = $(this).closest("table");
               $("input[type=checkbox]", b).each(function () {
                   a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
               });
           });

           $("[id*=AddCheckBox]").on("click", function () {
               var a = $(this).closest("table"), b = $("[id*=AllCheckBox]", a);
               $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=AddCheckBox]", a).length == $("[id*=AddCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
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

      function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
   </script>
</asp:Content>
