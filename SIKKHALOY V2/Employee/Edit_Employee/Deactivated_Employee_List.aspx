<%@ Page Title="Deactivated Employee List" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Deactivated_Employee_List.aspx.cs" Inherits="EDUCATION.COM.Employee.Edit_Employee.Deactivated_Employee_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../CSS/Emp_List.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Deactivated Employee List</h3>
   <a class="NoPrint" href="../Employee_List.aspx">Back To Employee List</a>

   <div class="form-inline form-group">
      <asp:RadioButtonList ID="EmpTypeRadioButtonList" runat="server" AutoPostBack="True" CssClass="form-control NoPrint" RepeatDirection="Horizontal">
         <asp:ListItem Selected="True" Value="%">All Employee</asp:ListItem>
         <asp:ListItem>Teacher</asp:ListItem>
         <asp:ListItem>Staff</asp:ListItem>
      </asp:RadioButtonList>
   </div>
   <div class="table-responsive">
      <asp:GridView ID="EmployeeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="EmployeeID,EmployeeType" DataSourceID="EmployeeSQL">
         <Columns>
            <asp:TemplateField HeaderText="Active">
               <ItemTemplate>
                  <asp:LinkButton ID="ActiveLinkButton" runat="server" CommandName='<%#Eval("EmployeeID") %>' OnCommand="ActiveLinkButton_Command">Re-Active</asp:LinkButton>
               </ItemTemplate>
               <HeaderStyle CssClass="NoPrint" />
               <ItemStyle CssClass="NoPrint" />
            </asp:TemplateField>
            <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
            <asp:BoundField DataField="DeviceID" HeaderText="Device ID" SortExpression="DeviceID" />
            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
            <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
            <asp:BoundField DataField="EmployeeType" HeaderText="Emp.Type" SortExpression="EmployeeType" />
            <asp:BoundField DataField="Salary" HeaderText="Salary" SortExpression="Salary" />
            <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
            <asp:TemplateField HeaderText="Image">
               <ItemTemplate>
                  <img alt="" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" class="Itm_Img img-thumbnail z-depth-1" />
               </ItemTemplate>
               <ItemStyle VerticalAlign="Middle" CssClass="Itm_Img" />
            </asp:TemplateField>
         </Columns>
      </asp:GridView>
      <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EmployeeID, ID, EmployeeType, Permanent_Temporary, Salary,  FirstName +' '+ LastName as Name, Designation, Phone, DeviceID FROM VW_Emp_Info WHERE (SchoolID = @SchoolID) AND (Job_Status = N'Deactivate') AND (EmployeeType LIKE @EmployeeType)" UpdateCommand="UPDATE Employee_Info SET Job_Status = 'Active' WHERE (EmployeeID = @EmployeeID)
UPDATE  Registration SET  Validation = 'Valid' FROM Teacher INNER JOIN Registration ON Teacher.TeacherRegistrationID = Registration.RegistrationID WHERE  (Teacher.SchoolID = @SchoolID) AND (Teacher.EmployeeID = @EmployeeID)

UPDATE aspnet_Membership SET  IsApproved = 1 FROM  aspnet_Membership INNER JOIN aspnet_Users ON aspnet_Membership.UserId = aspnet_Users.UserId INNER JOIN
 Teacher INNER JOIN  Registration ON Teacher.TeacherRegistrationID = Registration.RegistrationID ON aspnet_Users.UserName = Registration.UserName
WHERE  (Teacher.SchoolID = @SchoolID) AND (Teacher.EmployeeID = @EmployeeID)">
         <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="EmpTypeRadioButtonList" Name="EmployeeType" PropertyName="SelectedValue" />
         </SelectParameters>
         <UpdateParameters>
            <asp:Parameter Name="EmployeeID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         </UpdateParameters>
      </asp:SqlDataSource>
   </div>
</asp:Content>
