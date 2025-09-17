<%@ Page Title="Payment Voucher" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Paid_Receipt.aspx.cs" Inherits="EDUCATION.COM.Employee.Payment.Paid_Receipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="CSS/Paid_Receipt.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <h3>Payment Voucher</h3>
   <a href="Salary_Payment.aspx">Back</a>
   <asp:FormView ID="InfoFormView" runat="server" DataSourceID="EmployeeSQL" Width="100%" DataKeyNames="Name">
      <ItemTemplate>
         <div class="Info">
            <ul>
               <li><strong>(<asp:Label ID="IDLabel" runat="server" Text='<%# Bind("ID") %>' />
                  )
                    <asp:Label ID="NameLabel" runat="server" Text='<%# Bind("Name") %>' />
               </strong></li>
               <li><b>Designation:</b>
                  <asp:Label ID="DesignationLabel" runat="server" Text='<%# Bind("Designation") %>' />
               </li>
               <li><b>Phone:</b>
                  <asp:Label ID="PhoneLabel" runat="server" Text='<%# Bind("Phone") %>' />
               </li>
            </ul>
         </div>
      </ItemTemplate>
   </asp:FormView>
   <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Permanent_Temporary, Salary, FirstName + ' ' + LastName AS Name, Designation, Phone, EmployeeType, Work_Time_Basis, Time_Basis_Type, ID FROM VW_Emp_Info WHERE (Job_Status = N'Active') AND (EmployeeID = @EmployeeID)">
      <SelectParameters>
         <asp:QueryStringParameter Name="EmployeeID" QueryStringField="EmployeeID" />
      </SelectParameters>
   </asp:SqlDataSource>

   <br />
   <asp:GridView ID="PaidRecordGridView" runat="server" CssClass="mGrid" DataSourceID="PaidRecordSQL" AutoGenerateColumns="False" DataKeyNames="Employee_Payorder_RecordID">
      <Columns>
         <asp:BoundField DataField="Paid_For" HeaderText="Paid For" SortExpression="Paid_For" />
         <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
         <asp:BoundField DataField="Paid_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Paid Date" SortExpression="Paid_Date" />
      </Columns>
   </asp:GridView>
   <asp:SqlDataSource ID="PaidRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Employee_Payorder_Records] WHERE ([EmployeeID] = @EmployeeID)">
      <SelectParameters>
         <asp:QueryStringParameter Name="EmployeeID" QueryStringField="EmployeeID" Type="Int32" />
      </SelectParameters>
   </asp:SqlDataSource>

   <br />
   <input id="Button1" type="button" value="Print" class="btn btn-primary d-print-none" onclick="window.print()" />
</asp:Content>
