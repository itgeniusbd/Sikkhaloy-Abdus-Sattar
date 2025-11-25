<%@ Page Title="Accounts by user" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="UserAccount.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.UserAccount" %>
<%@ OutputCache Duration="1" VaryByParam="*" Location="Client" NoStore="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
 <link href="CSS/Accounts_By_User.css?v=10" rel="stylesheet" />
 
 <style type="text/css">
 @media (min-width: 768px) {
 body #sidedrawer,
 body.hide-sidedrawer #sidedrawer {
 transform: translate(250px) !important;
 left: -250px !important;
 visibility: visible !important;
 display: block !important;
 }
 
 body #header,
 body #content-wrapper,
 body #footer,
 body.hide-sideddrawer #header,
 body.hide-sidedrawer #content-wrapper,
 body.hide-sidedrawer #footer {
 margin-left: 250px !important;
 }
 }
 </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

 <div class="d-flex justify-content-between align-items-center mb-3">
 <h3 class="mb-0">Accounts details by user
 <small class="Date"></small>
 </h3>
 <div class="NoPrint">
 <a href="SubmitBalance.aspx" class="btn btn-success">
 <i class="fa fa-plus"></i> জমা দিন
 </a>
 </div>
 </div>
 
 <div class="form-inline NoPrint">
 <div class="form-group">
 <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
 </div>
 <div class="form-group">
 <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="To Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
 <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
 </div>
 <div class="form-group">
 <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="SUBMIT" />
 </div>
 <div class="form-group pull-right Print">
 <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
 </div>
 </div>

 <asp:FormView ID="IncomeFormView" runat="server" DataSourceID="UserInExSQL" Width="100%">
 <ItemTemplate>
 <div class="row">
 <div class="col-md-2">
 <div class="user-grid user-bg">
 <div class="user-imge">
 <img alt="No Image" src="/Handeler/Admin_Photo.ashx?Img=<%#Eval("AdminID") %>" class="img-circle img-responsive" />
 </div>
 <div class="headline"><%#Eval("Name") %></div>
 <div class="value"><%#Eval("Designation") %></div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid income-bg">
 <i class="fa fa-money"></i>
 <div class="headline">INCOME</div>
 <div class="value"><%#Eval("Income","{0:N0}") %> TK</div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid expense-bg">
 <i class="fa fa-money"></i>
 <div class="headline">EXPENSE</div>
 <div class="value"><%#Eval("Expense","{0:N0}") %> TK</div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid balance-bg">
 <i class="fa fa-calculator"></i>
 <div class="headline">BALANCE</div>
 <div class="value"><%#Eval("Balance","{0:N0}") %> TK</div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid submitted-bg">
 <i class="fa fa-arrow-up"></i>
 <div class="headline">SUBMITTED</div>
 <div class="value"><%#Eval("Submitted","{0:N0}") %> TK</div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid remaining-bg">
 <i class="fa fa-wallet"></i>
 <div class="headline">REMAINING</div>
 <div class="value"><%#Eval("RemainingBalance","{0:N0}") %> TK</div>
 </div>
 </div>
 </div>
 </ItemTemplate>
 </asp:FormView>
 
 <asp:SqlDataSource ID="UserInExSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
 SelectCommand="SELECT RegistrationID, AdminID, Designation, Name, Income, Expense, (Income - Expense) AS Balance, Submitted, ((Income - Expense) - Submitted) AS RemainingBalance 
FROM (
 SELECT User_T.RegistrationID, User_T.AdminID, User_T.Designation, User_T.Name, 
 ISNULL(EX_In_T.Other_Income,0) + ISNULL(Stu_P_T.Student_Income,0) + ISNULL(Com_In_T.CommitteeDonation,0) AS Income, 
 ISNULL(Ex_T.Expenditure,0) + ISNULL(Emp_P_T.Employee_Paid,0) AS Expense,
 ISNULL(Sub_T.TotalSubmitted,0) AS Submitted
FROM (
 SELECT Registration.RegistrationID, Admin.Designation, Admin.AdminID, 
 ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS Name 
 FROM Registration 
 LEFT OUTER JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID 
 WHERE (Registration.SchoolID = @SchoolID) AND (Registration.RegistrationID = @RegistrationID)
 ) AS User_T 
 LEFT OUTER JOIN (
 SELECT RegistrationID, ISNULL(SUM(Extra_IncomeAmount),0) AS Other_Income 
 FROM Extra_Income 
 WHERE (SchoolID = @SchoolID) 
 AND (Extra_IncomeDate >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (Extra_IncomeDate <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 GROUP BY RegistrationID
 ) AS EX_In_T ON User_T.RegistrationID = EX_In_T.RegistrationID 
 LEFT OUTER JOIN (
 SELECT RegistrationId, ISNULL(SUM(TotalAmount),0) AS CommitteeDonation 
 FROM CommitteeMoneyReceipt 
 WHERE (SchoolId = @SchoolID) 
 AND (CAST(PaidDate AS Date) >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (CAST(PaidDate AS Date) <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 GROUP BY RegistrationId
 ) AS Com_In_T ON User_T.RegistrationID = Com_In_T.RegistrationId 
 LEFT OUTER JOIN (
 SELECT RegistrationID, ISNULL(SUM(PaidAmount),0) AS Student_Income 
 FROM Income_PaymentRecord 
 WHERE (SchoolID = @SchoolID) 
 AND (CAST(PaidDate AS Date) >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (CAST(PaidDate AS Date) <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 GROUP BY RegistrationID
 ) AS Stu_P_T ON User_T.RegistrationID = Stu_P_T.RegistrationID 
 LEFT OUTER JOIN (
 SELECT RegistrationID, ISNULL(SUM(Amount),0) AS Expenditure 
 FROM Expenditure 
 WHERE (SchoolID = @SchoolID) 
 AND (ExpenseDate >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (ExpenseDate <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 GROUP BY RegistrationID
 ) AS Ex_T ON User_T.RegistrationID = Ex_T.RegistrationID 
 LEFT OUTER JOIN (
 SELECT RegistrationID, ISNULL(SUM(Amount),0) AS Employee_Paid 
 FROM Employee_Payorder_Records 
 WHERE (SchoolID = @SchoolID) 
 AND (Paid_date >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (Paid_date <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 GROUP BY RegistrationID
 ) AS Emp_P_T ON User_T.RegistrationID = Emp_P_T.RegistrationID
 LEFT OUTER JOIN (
 SELECT RegistrationID, ISNULL(SUM(SubmissionAmount),0) AS TotalSubmitted 
 FROM User_Balance_Submission 
 WHERE (SchoolID = @SchoolID) 
 AND (SubmissionDate >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (SubmissionDate <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 GROUP BY RegistrationID
 ) AS Sub_T ON User_T.RegistrationID = Sub_T.RegistrationID
) AS T"
 CancelSelectOnNullParameter="False">
 <SelectParameters>
 <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
 <asp:QueryStringParameter Name="RegistrationID" QueryStringField="RegID" />
 <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" Type="String" />
 <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" Type="String" />
 </SelectParameters>
 </asp:SqlDataSource>
 
 <div class="clearfix"></div>
 
 <div class="row">
 <div id="Income_gv" class="col-md-6">
 <div class="box Income-box"><i class="fa fa-arrow-circle-down"></i>&nbsp Income Category</div>
 <asp:Repeater ID="IncomeRepeater" runat="server" DataSourceID="IncomeDetailsSQL">
 <ItemTemplate>
 <div class="pull-left">
 <asp:Label ID="CategoryLabel" runat="server" Text='<%# Eval("Category") %>' />
 </div>
 <div class="pull-right">
 <%# Eval("Income","{0:N0}") %> TK
 </div>

 <div class="table-responsive mb-3">
 <asp:GridView ID="IncomeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="DetailsSQL" AllowSorting="True" AllowPaging="True" PageSize="20" EnableViewState="false">
 <Columns>
 <asp:BoundField DataField="AccountName" HeaderText="Account" ReadOnly="True" SortExpression="AccountName" />
 <asp:BoundField DataField="Details" HeaderText="Details" ReadOnly="True" SortExpression="Details" />
 <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" DataFormatString="{0:N0}" />
 <asp:BoundField DataField="Date" HeaderText="Date" ReadOnly="True" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
 </Columns>
 <PagerStyle CssClass="pgr" />
 </asp:GridView>
 <asp:SqlDataSource ID="DetailsSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT 
ISNULL(Account.AccountName, 'N/A') AS AccountName,
Income_Roles.Role as Category, 
'Class : ' + CreateClass.Class + ' (ID: ' + Student.ID + ') ' + ' ' + Income_PaymentRecord.PayFor AS Details, 
Income_PaymentRecord.PaidAmount AS Amount, 
cast(Income_PaymentRecord.PaidDate as Date) as [Date]
FROM Income_PaymentRecord INNER JOIN
 Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN
 Student ON Income_PaymentRecord.StudentID = Student.StudentID INNER JOIN
 StudentsClass ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID INNER JOIN 
 CreateClass ON StudentsClass.ClassID = CreateClass.ClassID LEFT OUTER JOIN
 Account ON Income_PaymentRecord.AccountID = Account.AccountID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND Income_PaymentRecord.RegistrationID = @RegistrationID 
 AND (CAST(Income_PaymentRecord.PaidDate AS Date) >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (CAST(Income_PaymentRecord.PaidDate AS Date) <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 AND Income_Roles.Role = @Category
Union 
SELECT 
ISNULL(Account.AccountName, 'N/A') AS AccountName, 
Extra_IncomeCategory.Extra_Income_CategoryName AS Category, 
Extra_Income.Extra_IncomeFor AS Details, 
Extra_Income.Extra_IncomeAmount AS Amount, 
Extra_Income.Extra_IncomeDate AS [Date] 
FROM Extra_Income INNER JOIN
 Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID LEFT OUTER JOIN
 Account ON Extra_Income.AccountID = Account.AccountID 
WHERE(Extra_Income.SchoolID = @SchoolID) AND Extra_Income.RegistrationID = @RegistrationID 
 AND (Extra_Income.Extra_IncomeDate >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (Extra_Income.Extra_IncomeDate <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 AND Extra_IncomeCategory.Extra_Income_CategoryName = @Category 

Union 
SELECT 
ISNULL(Account.AccountName, 'N/A') AS AccountName, 
CommitteeDonationCategory.DonationCategory AS Category, 
CommitteeDonation.Description AS Details, 
CommitteePaymentRecord.PaidAmount AS Amount, 
CommitteeMoneyReceipt.PaidDate AS [Date] 
FROM CommitteeMoneyReceipt INNER JOIN
 CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId INNER JOIN
 CommitteeDonation INNER JOIN
 CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId ON 
 CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId LEFT OUTER JOIN 
 Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID 
WHERE(CommitteeMoneyReceipt.SchoolID = @SchoolID) AND CommitteeMoneyReceipt.RegistrationID = @RegistrationID 
 AND (CAST(CommitteeMoneyReceipt.PaidDate AS Date) >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (CAST(CommitteeMoneyReceipt.PaidDate AS Date) <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 AND CommitteeDonationCategory.DonationCategory = @Category
order by [Date]">
 <SelectParameters>
 <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
 <asp:QueryStringParameter Name="RegistrationID" QueryStringField="RegID" />
 <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
 <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
 <asp:ControlParameter ControlID="CategoryLabel" Name="Category" PropertyName="Text" />
 </SelectParameters>
 </asp:SqlDataSource>
 </div>
 </ItemTemplate>
 </asp:Repeater>
 <asp:SqlDataSource ID="IncomeDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Income) AS Income from
(SELECT Income_Roles.Role AS Category, SUM(Income_PaymentRecord.PaidAmount) AS Income FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND Income_PaymentRecord.RegistrationID = @RegistrationID 
 AND (CAST(Income_PaymentRecord.PaidDate AS Date) >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (CAST(Income_PaymentRecord.PaidDate AS Date) <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
GROUP BY Income_Roles.Role
Union
SELECT CommitteeDonationCategory.DonationCategory AS Category, SUM(CommitteePaymentRecord.PaidAmount) AS Income FROM CommitteeMoneyReceipt 
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId 
INNER JOIN CommitteeDonation 
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId 
ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId 
WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolID) AND CommitteeMoneyReceipt.RegistrationID = @RegistrationID 
 AND (CAST(CommitteeMoneyReceipt.PaidDate AS Date) >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (CAST(CommitteeMoneyReceipt.PaidDate AS Date) <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
GROUP BY CommitteeDonationCategory.DonationCategory

Union 
SELECT Extra_IncomeCategory.Extra_Income_CategoryName AS Category , SUM(Extra_Income.Extra_IncomeAmount) AS Income
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID) AND Extra_Income.RegistrationID = @RegistrationID 
 AND (Extra_Income.Extra_IncomeDate >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (Extra_Income.Extra_IncomeDate <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName)as t GROUP BY Category
"
 CancelSelectOnNullParameter="False">
 <SelectParameters>
 <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
 <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
 <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
 <asp:QueryStringParameter Name="RegistrationID" QueryStringField="RegID" />
 </SelectParameters>
 </asp:SqlDataSource>
 </div>

 <div id="Expense_gv" class="col-md-6">
 <div class="box Expense-box"><i class="fa fa-arrow-circle-up"></i>&nbsp Expense Category</div>
 <asp:Repeater ID="ExpenseRepeater" runat="server" DataSourceID="ExpenseCategorySQL">
 <ItemTemplate>
 <div class="pull-left">
 <asp:Label ID="CategoryLabel" runat="server" Text='<%# Eval("Category") %>' />
 </div>
 <div class="pull-right">
 <%# Eval("Total","{0:N0}") %> TK
 </div>

 <div class="table-responsive mb-3">
 <asp:GridView ID="ExpenseGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="DetailsSQL" AllowSorting="True" AllowPaging="True" PageSize="20" EnableViewState="false">
 <Columns>
 <asp:BoundField DataField="AccountName" HeaderText="Account" ReadOnly="True" SortExpression="AccountName" />
 <asp:BoundField DataField="Details" HeaderText="Details" ReadOnly="True" SortExpression="Details" />
 <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" DataFormatString="{0:N0}" />
 <asp:BoundField DataField="Date" HeaderText="Date" ReadOnly="True" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
 </Columns>
 <PagerStyle CssClass="pgr" />
 </asp:GridView>
 <asp:SqlDataSource ID="DetailsSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(Account.AccountName, 'N/A') AS AccountName,Expense_CategoryName.CategoryName as Category, 
 Expenditure.ExpenseFor AS Details, Expenditure.Amount, Expenditure.ExpenseDate as [Date]
FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID= Expense_CategoryName.ExpenseCategoryID LEFT OUTER JOIN
 Account ON Expenditure.AccountID = Account.AccountID
WHERE (Expenditure.SchoolID = @SchoolID)AND Expenditure.RegistrationID = @RegistrationID 
 AND (Expenditure.ExpenseDate >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (Expenditure.ExpenseDate <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 AND Expense_CategoryName.CategoryName = @Category
Union
SELECT ISNULL(Account.AccountName, 'N/A') AS AccountName,Employee_Payorder_Name.Payorder_Name AS Category, Employee_Payorder_Records.Paid_For AS Details, Employee_Payorder_Records.Amount, Employee_Payorder_Records.Paid_date AS [Date]
FROM Employee_Payorder_Records INNER JOIN
 Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN
 Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID LEFT OUTER JOIN
 Account ON Employee_Payorder_Records.AccountID = Account.AccountID
WHERE (Employee_Payorder_Records.SchoolID = @SchoolID) AND Employee_Payorder_Records.RegistrationID = @RegistrationID 
 AND (Employee_Payorder_Records.Paid_date >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (Employee_Payorder_Records.Paid_date <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
 AND Employee_Payorder_Name.Payorder_Name = @Category
order by [Date]">
 <SelectParameters>
 <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
 <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
 <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
 <asp:ControlParameter ControlID="CategoryLabel" Name="Category" PropertyName="Text" />
 <asp:QueryStringParameter Name="RegistrationID" QueryStringField="RegID" />
 </SelectParameters>
 </asp:SqlDataSource>
 </div>
 </ItemTemplate>
 </asp:Repeater>
 <asp:SqlDataSource ID="ExpenseCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Amount) AS Total from(SELECT Employee_Payorder_Name.Payorder_Name AS Category, SUM(Employee_Payorder_Records.Amount) AS Amount
FROM Employee_Payorder_Records INNER JOIN
 Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN
 Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
WHERE (Employee_Payorder_Records.SchoolID = @SchoolID) AND Employee_Payorder_Records.RegistrationID = @RegistrationID 
 AND (Employee_Payorder_Records.Paid_date >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (Employee_Payorder_Records.Paid_date <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
GROUP BY Employee_Payorder_Name.Payorder_Name
Union 
SELECT Expense_CategoryName.CategoryName AS Category , SUM(Expenditure.Amount) AS Amount
FROM Expenditure INNER JOIN
 Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
WHERE (Expenditure.SchoolID = @SchoolID) AND Expenditure.RegistrationID = @RegistrationID 
 AND (Expenditure.ExpenseDate >= CASE WHEN NULLIF(@From_Date, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @From_Date,103) END)
 AND (Expenditure.ExpenseDate <= CASE WHEN NULLIF(@To_Date, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @To_Date,103) END)
GROUP BY Expense_CategoryName.CategoryName)as t GROUP BY Category" CancelSelectOnNullParameter="False">
 <SelectParameters>
 <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
 <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
 <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
 <asp:QueryStringParameter Name="RegistrationID" QueryStringField="RegID" />
 </SelectParameters>
 </asp:SqlDataSource>
 </div>
 </div>

    <script>
        // =================== SIDEBAR FIX ===================
(function() {
 'use strict';
 
 window.preventSidebarHide = true;
 window.forceLockSidebar = true;
 
 function fixSidebar() {
 if (document.body) {
 document.body.classList.remove('hide-sidedrawer');
 document.body.classList.add('lock-sidebar');
 }
 var sidebar = document.getElementById('sidedrawer');
 if (sidebar) {
 sidebar.style.transform = 'translate(250px)';
 sidebar.style.visibility = 'visible';
 sidebar.style.display = 'block';
 sidebar.style.left = '-250px';
 sidebar.style.opacity = '1';
 }
 }
 
 fixSidebar();
 
 if (document.readyState === 'loading') {
 document.addEventListener('DOMContentLoaded', fixSidebar);
 } else {
 fixSidebar();
 }
 
 setInterval(fixSidebar, 200);
})();

// =================== FORCE CLEAR CACHE ON LOAD ===================
(function() {
 // Clear any cached date values
 if (window.performance && window.performance.navigation.type === 1) {
 // Page was reloaded/refreshed
 console.log('Page reloaded - clearing date cache');
 }
 
 // Clear browser autocomplete for date fields
 setTimeout(function() {
 $('[id*=From_Date_TextBox]').attr('autocomplete', 'off').val($('[id*=From_Date_TextBox]').val());
 $('[id*=To_Date_TextBox]').attr('autocomplete', 'off').val($('[id*=To_Date_TextBox]').val());
 }, 100);
})();

// =================== Date Label Update Function ===================
function updateDateLabel() {
 // Force fresh read from textboxes
 var from = $("[id*=From_Date_TextBox]").val() || "";
 var To = $("[id*=To_Date_TextBox]").val() || "";
 
 var tt = "", Brases1 = "", Brases2 = "", A = "", B = "", TODate = "";

 if (To == "" || from == "") {
 tt = ""; 
 A = ""; 
 B = "";
 } else {
 tt = " To ";
 Brases1 = "(";
 Brases2 = ")";
 }

 if (To == "" && from == "") { 
 Brases1 = ""; 
 }
 
 if (To == from) {
 TODate = "";
 tt = "";
 Brases1 = "";
 Brases2 = "";
 } else {
 TODate = To;
 }

 if (from == "" && To != "") B = " Before ";
 if (To == "" && from != "") A = " After ";
 if (from != "" && To != "") { 
 A = ""; 
 B = ""; 
 }

 var dateText = Brases1 + B + A + from + tt + TODate + Brases2;
 
 // Force update with fade effect to show change
 $(".Date").fadeOut(100, function() {
 $(this).text(dateText).fadeIn(100);
 });
 
 console.log('Date label updated:', dateText, '| From:', from, '| To:', To);
}

// =================== jQuery DOM Ready ===================
$(function () {
 console.log('Page loaded - jQuery ready');
 
 // Hide empty sections
 if (!$('[id*=IncomeGridView] tr').length) {
 $('#Income_gv').hide().removeClass('col-md-6');
 $('#Expense_gv').removeClass('col-md-6').addClass('col-md-12');
 }

 if (!$('[id*=ExpenseGridView] tr').length) {
 $('#Expense_gv').hide().removeClass('col-md-6');
 $('#Income_gv').removeClass('col-md-6').addClass('col-md-12');
 }

 // Individual date picker
 $('.datepicker').datepicker({
 format: 'dd/mm/yyyy',
 todayBtn: "linked",
 todayHighlight: true,
 autoclose: true
 }).on('changeDate', function() {
 // Update label when date changed via datepicker
 setTimeout(updateDateLabel, 100);
 });

 // Date range picker configuration
 var dateRangePickerOptions = {
 autoApply: true,
 opens: 'left',
 locale: {
 format: 'DD/MM/YYYY'
 },
 ranges: {
 'Today': [moment(), moment()],
 'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
 'Last 7 Days': [moment().subtract(6, 'days'), moment()],
 'Last 30 Days': [moment().subtract(29, 'days'), moment().endOf('day')],
 'This Month': [moment().startOf('month'), moment().endOf('month')],
 'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
 'This Year': [moment().startOf('year'), moment().endOf('year')]
 }
 };

 // Set initial date range from CURRENT textbox values
 var fromDate = $('[id*=From_Date_TextBox]').val();
 var toDate = $('[id*=To_Date_TextBox]').val();
 
 console.log('Initial dates from textboxes - From:', fromDate, '| To:', toDate);
 
 if (fromDate && toDate) {
 try {
 var startMoment = moment(fromDate, 'DD/MM/YYYY');
 var endMoment = moment(toDate, 'DD/MM/YYYY');
 
 if (startMoment.isValid() && endMoment.isValid()) {
 dateRangePickerOptions.startDate = startMoment;
 dateRangePickerOptions.endDate = endMoment;
 }
 } catch(e) {
 console.log('Initial date parse error:', e);
 }
 }

 // Initialize date range picker
 $('#PickDate').daterangepicker(dateRangePickerOptions);

 // When date range is selected
 $('#PickDate').on('apply.daterangepicker', function(ev, picker) {
 var newFrom = picker.startDate.format('DD/MM/YYYY');
 var newTo = picker.endDate.format('DD/MM/YYYY');
 
 console.log('Date range selected - From:', newFrom, '| To:', newTo);
 
 $('[id*=From_Date_TextBox]').val(newFrom);
 $('[id*=To_Date_TextBox]').val(newTo);
 
 // Update label immediately
 updateDateLabel();
 
 // Then submit
 setTimeout(function() {
 $("[id*=Find_Button]").trigger("click");
 }, 100);
 });
 
 // Calendar icon click event
 $('#PickDate').on('click', function(e) {
 e.preventDefault();
 e.stopPropagation();
 
 var currentFrom = $('[id*=From_Date_TextBox]').val();
 var currentTo = $('[id*=To_Date_TextBox]').val();
 
 console.log('Calendar clicked - Current From:', currentFrom, '| To:', currentTo);
 
 if (currentFrom && currentTo) {
 try {
 var startMoment = moment(currentFrom, 'DD/MM/YYYY');
 var endMoment = moment(currentTo, 'DD/MM/YYYY');
 
 if (startMoment.isValid() && endMoment.isValid()) {
 $(this).data('daterangepicker').setStartDate(startMoment);
 $(this).data('daterangepicker').setEndDate(endMoment);
 }
 } catch(e) {
 console.log('Date update error:', e);
 }
 }
 
 $(this).data('daterangepicker').show();
 return false;
 });
 
 // Initial date label update
 setTimeout(updateDateLabel, 200);
});

// =================== Window Load Event ===================
window.addEventListener('load', function() {
 setTimeout(function() {
 console.log('Window fully loaded - updating date label');
 updateDateLabel();
 }, 300);
});

// =================== MutationObserver ===================
if (window.MutationObserver) {
 var observer = new MutationObserver(function(mutations) {
 clearTimeout(window.dateUpdateTimeout);
 window.dateUpdateTimeout = setTimeout(function() {
 var fromVal = $("[id*=From_Date_TextBox]").val();
 var toVal = $("[id*=To_Date_TextBox]").val();
 
 if (fromVal || toVal) {
 updateDateLabel();
 }
 }, 150);
 });
 
 setTimeout(function() {
 var targetNode = document.body;
 if (targetNode) {
 observer.observe(targetNode, {
 childList: true,
 subtree: true,
 attributes: false
 });
 }
 }, 500);
}

// Number key validation
function isNumberKey(a) { 
 a = a.which ? a.which : event.keyCode; 
 return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 
}
 </script>
</asp:Content>
