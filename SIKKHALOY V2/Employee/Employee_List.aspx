<%@ Page Title="Employee List" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Employee_List.aspx.cs" Inherits="EDUCATION.COM.Employee.Employee_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Emp_List.css" rel="stylesheet" />
    <style>
        .avatar-upload { position: relative; display: inline-block; }
        .avatar-upload .avatar-edit { position: absolute; right: 1px; z-index: 1; bottom: 1px; }
        .avatar-upload .avatar-edit input { display: none; }
        .avatar-upload .avatar-edit input + label { display: inline-block; width: 20px; height: 20px; padding-top: 1px; margin-bottom: 0; border-radius: 50%; background: #FFFFFF; box-shadow: 0px 1px 3px 0px rgba(0, 0, 0, 0.15); cursor: pointer; font-weight: normal; transition: all 0.2s ease-in-out; text-align: center; border: 1px solid #E6E6E6; font-size: 10px; }
        .avatar-upload .avatar-edit input + label:hover { background: #f1f1f1; border-color: #d6d6d6; }
        .avatar-upload .avatar-edit label::after { content: "\f040"; font-family: 'FontAwesome'; color: #757575; }
        .employee-img { object-fit: cover;border-radius:8px; }
        .success_message { display:none; font-size: 80%; margin:0; color: green; font-weight: bold; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Employee List</h3>
    <a class="NoPrint" href="Edit_Employee/Deactivated_Employee_List.aspx">Deactivated Employee List</a>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:RadioButtonList CssClass="form-control" ID="EmpTypeRadioButtonList" runat="server" AutoPostBack="True" RepeatLayout="Flow" RepeatDirection="Horizontal" OnSelectedIndexChanged="EmpTypeRadioButtonList_SelectedIndexChanged">
   <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
      <asp:ListItem>Teacher</asp:ListItem>
        <asp:ListItem>Staff</asp:ListItem>
     </asp:RadioButtonList>
        </div>
        <div class="form-group mx-2">
          <asp:TextBox ID="FindTextBox" runat="server" placeholder="Enter Search Keyword" CssClass="form-control"></asp:TextBox>
        </div>
   <div class="form-group">
      <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" OnClick="FindButton_Click" />
 <input type="button" value="Print" class="btn btn-dark-green" onclick="window.print();" />
        </div>
    </div>

    <div class="alert alert-info">
<asp:Label ID="CountLabel" runat="server"></asp:Label>
    </div>

 <div class="table-responsive">
        <asp:GridView ID="EmployeeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID,EmployeeType" DataSourceID="EmployeeSQL" AllowSorting="True">
    <Columns>
 <asp:TemplateField HeaderText="Edit/Deactivate">
              <ItemTemplate>
     <asp:LinkButton ID="EditLinkButton" runat="server" OnCommand="EditLinkButton_Command" CommandName='<%#Eval("EmployeeID") %>' CommandArgument='<%#Eval("EmployeeType") %>'>Edit/Deactivate</asp:LinkButton>
           </ItemTemplate>
     <HeaderStyle CssClass="d-print-none" />
                <ItemStyle CssClass="d-print-none" />
     </asp:TemplateField>
     <asp:TemplateField HeaderText="ID" SortExpression="ID">
   <ItemTemplate>
     <asp:TextBox ID="Emp_ID_TextBox" CssClass="form-control d-print-none" runat="server" Text='<%# Bind("ID") %>'></asp:TextBox>
  <span class="d-print-block d-none"><%#Eval("ID") %></span>
        </ItemTemplate>
              </asp:TemplateField>
              <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
        <asp:BoundField DataField="FatherName" HeaderText="Father's Name" SortExpression="FatherName" />
      <asp:BoundField DataField="Phone" HeaderText="Mobile No." SortExpression="Phone" />
           <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
       <asp:TemplateField HeaderText="Emp.Type" SortExpression="EmployeeType">
            <ItemTemplate>
   <asp:TextBox ID="EmployeeTypeTextBox" CssClass="form-control" runat="server" Text='<%# Bind("EmployeeType") %>'></asp:TextBox>
        </ItemTemplate>
   <HeaderStyle CssClass="d-print-none" />
         <ItemStyle CssClass="d-print-none" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Salary" SortExpression="Salary">
     <ItemTemplate>
  <asp:TextBox ID="SalaryTextBox" CssClass="form-control" runat="server" Text='<%# Bind("Salary") %>'></asp:TextBox>
    </ItemTemplate>
  <HeaderStyle CssClass="d-print-none" />
<ItemStyle CssClass="d-print-none" />
       </asp:TemplateField>
            <asp:TemplateField HeaderText="Bank Acc. No.">
    <ItemTemplate>
 <asp:TextBox ID="AccNoTextBox" CssClass="form-control d-print-none" runat="server" Text='<%# Bind("Bank_AccNo") %>'></asp:TextBox>
              <span class="d-print-block d-none"><%#Eval("Bank_AccNo") %></span>
        </ItemTemplate>
  </asp:TemplateField>
         <asp:TemplateField HeaderText="Image">
         <ItemTemplate>
   <div class="avatar-upload">
    <div class="avatar-edit d-print-none">
              <input name="Employee_Photo" id="emp_<%# Container.DataItemIndex %>" type="file" accept="image/x-png,image/jpeg" />
  <label for="emp_<%# Container.DataItemIndex %>"></label>
       </div>
       <img alt="" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" class="employee-img z-depth-1 img-thumbnail" />
 <input class="EmployeeID" value="<%# Eval("EmployeeID") %>" type="hidden" />
          <input class="EmployeeType" value="<%# Eval("EmployeeType") %>" type="hidden" />
    <p class="text-center success_message">Upload Success!</p>
     </div>
         </ItemTemplate>
          <ItemStyle VerticalAlign="Middle" CssClass="Itm_Img" />
    </asp:TemplateField>
 </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
       SelectCommand="SELECT EmployeeID, ID,Bank_AccNo, EmployeeType, Permanent_Temporary, Salary,  FirstName +' '+ LastName as Name,FatherName, Designation, Phone, DeviceID FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Active') AND (EmployeeType LIKE @EmployeeType) order by ID"
            FilterExpression="ID LIKE '{0}%' or Name LIKE '{0}%' or Designation LIKE '{0}%' or Phone LIKE '{0}%'" UpdateCommand="IF NOT EXISTS (SELECT * FROM Employee_Info WHERE ID = @ID AND SchoolID = @SchoolID) 
UPDATE Employee_Info SET ID = @ID WHERE (EmployeeID = @EmployeeID)"
InsertCommand="UPDATE Employee_Info SET EmployeeType = @EmployeeType WHERE (EmployeeID = @EmployeeID)">
      <FilterParameters>
      <asp:ControlParameter ControlID="FindTextBox" Name="Find" PropertyName="Text" />
 </FilterParameters>
<InsertParameters>
     <asp:Parameter Name="EmployeeType" />
      <asp:Parameter Name="EmployeeID" />
            </InsertParameters>
  <SelectParameters>
      <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
    <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
         </SelectParameters>
        <UpdateParameters>
    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
      <asp:Parameter Name="ID" />
      <asp:Parameter Name="EmployeeID" />
  </UpdateParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="SalaryUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Employee_Info]" UpdateCommand="UPDATE Employee_Info SET Salary = @Salary WHERE (EmployeeID = @EmployeeID)">
          <UpdateParameters>
   <asp:Parameter Name="Salary" />
          <asp:Parameter Name="EmployeeID" />
            </UpdateParameters>
      </asp:SqlDataSource>
     <asp:SqlDataSource ID="Bank_AccNoUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Employee_Info]" UpdateCommand="UPDATE Employee_Info SET Bank_AccNo = @Bank_AccNo WHERE (EmployeeID = @EmployeeID)">
   <UpdateParameters>
 <asp:Parameter Name="Bank_AccNo" />
   <asp:Parameter Name="EmployeeID" />
            </UpdateParameters>
 </asp:SqlDataSource>

            <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END" SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
          <InsertParameters>
   <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
   <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
  <asp:Parameter DefaultValue="Employee ID Change" Name="UpdateType" Type="String" />
         <asp:Parameter DefaultValue="Employee ID chnage" Name="UpdateDescription" Type="String" />
      </InsertParameters>
            </asp:SqlDataSource>

    </div>

    <%if (EmployeeGridView.Rows.Count > 0)
        {%>
    <br />
    <asp:Button ID="UploadButton" runat="server" CssClass="btn btn-primary d-print-none" OnClick="UploadButton_Click" Text="Update ID, Salary, Bank A/C & Type" />
    <%}%>

    <script src="/JS/Resize_Img/canvasResize.js"></script>
    <script type="text/javascript">
$(document).ready(function () {
            //upload image
       $('input[name=Employee_Photo]').change(function (input) {
    var file = input.target.files[0];
     var prev = $(this).closest('.avatar-upload').find('.employee-img');
            var empId = $(this).closest('.avatar-upload').find('.EmployeeID');
        var empType = $(this).closest('.avatar-upload').find('.EmployeeType');
          var success_msg = $(this).closest('.avatar-upload').find('.success_message');

  var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
     alert('Please upload file having extensions .jpeg/.jpg/.png only');
       return false;
             }
             else {
          canvasResize(file, {
   width: 300,
               height: 330,
         quality: 70,
     callback: function (idata) {
                  $(prev).attr('src', idata);

   $.ajax({
           url: "Employee_List.aspx/UpdateEmployeeImage",
     data: JSON.stringify({ 'EmployeeID': empId.val(), 'EmployeeType': empType.val(), 'Image': idata.split(",")[1] }),
       dataType: "json",
             type: "POST",
 contentType: "application/json; charset=utf-8",
              success: function (response) {
    success_msg.fadeIn();
      setTimeout(function () { success_msg.fadeOut("slow") }, 2000);
   },
               error: function (xhr) {
     var err = JSON.parse(xhr.responseText);
               alert(err.Message || 'Error uploading image');
 }
      });
         }
       });
              }
 });
   });
    </script>
</asp:Content>
