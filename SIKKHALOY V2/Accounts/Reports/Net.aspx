<%@ Page Title="Income & Expense Details" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Net.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.Net" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Income_Expense_Net.css?v=1.1" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a href="Final_Reports.aspx" class="NoPrint">
        <i class="fa fa-hand-o-left" aria-hidden="true"></i>
        Back to Accounts Summary</a> .....  <i class="fa fa-search" aria-hidden="true"></i> <a href="UserWise_Account_Report.aspx"> Income Expense Summary by user</a>
    <h3>
        <label class="Date"></label>
        INCOME & EXPENSE
    </h3>

    <div class="form-inline head-area NoPrint">
        <div class="form-group">
            <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="Submit" />
        </div>
        <div class="form-group pull-right Print">
            <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
        </div>
    </div>

    <asp:FormView ID="IncomeFormView" runat="server" DataSourceID="NetSQL" RenderOuterTable="false">
        <ItemTemplate>
            <div class="row">
                <div class="col-md-3 col-sm-3">
                    <div class="user-grid">
                        <i class="fa fa-line-chart"></i>
                        <span class="headline">INCOME</span>
                        <span class="value"><%#Eval("Total_Revenue","{0:N0}") %> TK</span>
                    </div>
                </div>
                <div class="col-md-3 col-sm-3">
                    <div class="user-grid">
                        <i class="fa fa-area-chart"></i>
                        <span class="headline">EXPENSE</span>
                        <span class="value"><%#Eval("Total_Expense","{0:N0}") %> TK</span>
                    </div>
                </div>
                <div class="col-md-3 col-sm-3">
                    <div class="user-grid">
                        <i class="fa fa-area-chart"></i>
                        <span class="headline">ONLINE PAYMENT</span>
                        <span class="value"><%#Eval("Online_Payment","{0:N0}") %> TK</span>
                    </div>
                </div>
                <div class="col-md-3 col-sm-3">
                    <div class="user-grid">
                        <i class="fa fa-bar-chart"></i>
                        <span class="headline">CASH IN HAND</span>
                        <span class="value"><%#Eval("Net","{0:N0}") %> TK</span>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="NetSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Other_Income + Studnet_Paid + CommitteeDonation AS Total_Revenue, 
Expenditure + Employee_Paid AS Total_Expense, 
Online_Payment,
Other_Income + Studnet_Paid + CommitteeDonation - Expenditure - Employee_Paid - Online_Payment AS Net
FROM            
(SELECT        
	(SELECT        ISNULL(SUM(Extra_IncomeAmount), 0) AS Expr1
	 FROM            Extra_Income
	 WHERE        (SchoolID = @SchoolID) AND (Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
	 ) AS Other_Income,
	(SELECT        ISNULL(SUM(TotalAmount), 0) AS Expr1
	  FROM            CommitteeMoneyReceipt
	  WHERE        (SchoolId = @SchoolID) AND (PaidDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
	) AS CommitteeDonation,
	(SELECT        ISNULL(SUM(PaidAmount), 0) AS Expr1
	  FROM            Income_PaymentRecord
	  WHERE        (SchoolID = @SchoolID) AND (CAST(PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
	  ) AS Studnet_Paid,
	(SELECT ISNULL(SUM(t.TotalAmount), 0) as Expr1
	FROM
	(SELECT a.PaidAmount as TotalAmount
	  FROM Income_PaymentRecord a, Account b
	  WHERE a.AccountID = b.AccountID 
	  AND a.SchoolID = b.SchoolID
	  AND b.AccountName = 'Online Payment'
      AND (a.SchoolID = @SchoolID)
	  AND (CAST(PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
	)t
	)AS Online_Payment ,
	(SELECT        ISNULL(SUM(Amount), 0) AS Expr1
	  FROM            Expenditure
	  WHERE        (SchoolID = @SchoolID) AND (ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
	  ) AS Expenditure,
	(SELECT        ISNULL(SUM(Amount), 0) AS Expr1
	  FROM            Employee_Payorder_Records
	  WHERE        (SchoolID = @SchoolID) AND (Paid_date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
	  ) AS Employee_Paid
) AS t"
        CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>


    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item"><a class="nav-link active" href="#tab1" data-toggle="tab" role="tab" aria-expanded="true"><i class="fa fa-bar-chart"></i>&nbsp Income & Expense Category</a></li>
        <li class="nav-item"><a class="nav-link" href="#tab2" data-toggle="tab" role="tab" aria-expanded="false"><i class="fa fa-bar-chart"></i>&nbsp Income</a></li>
        <li class="nav-item"><a class="nav-link" href="#tab3" data-toggle="tab" role="tab" aria-expanded="false"><i class="fa fa-area-chart"></i>&nbsp Expense</a></li>
    </ul>


    <div class="tab-content card">
        <div id="tab1" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
            <div class="w-100">
                <div class="Acc-income">
                    <%if (IncomeCategoryGridView.Rows.Count > 0)
                        {%>
                    <div class="box Income-box"><i class="fa fa-arrow-circle-down" aria-hidden="true"></i>&nbsp Category wise Income</div>
                    <asp:GridView ID="IncomeCategoryGridView" runat="server" AutoGenerateColumns="False" DataSourceID="IncomeCategorySQL" CssClass="mGrid" AllowSorting="True" OnRowDataBound="IncomeCategoryGridView_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="Category" HeaderText="Category" ReadOnly="True" SortExpression="Category" />
                            <asp:BoundField DataField="Income" HeaderText="Income" ReadOnly="True" SortExpression="Income" DataFormatString="{0:N0}">
                                <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="IncomeCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Income) AS Income from(SELECT Income_Roles.Role AS Category, SUM(Income_PaymentRecord.PaidAmount) AS Income FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) and cast(Income_PaymentRecord.PaidDate as Date)   BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY Income_Roles.Role
Union 
SELECT Extra_IncomeCategory.Extra_Income_CategoryName AS Category , SUM(Extra_Income.Extra_IncomeAmount) AS Income
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID) and Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName)as t  GROUP  BY Category"
                        CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <br />
                    <%} %>

                    <%if (ClassGridView.Rows.Count > 0)
                        {%>
                    <div class="box Income-box"><i class="fa fa-arrow-circle-down" aria-hidden="true"></i>&nbsp Class wise Income</div>
                    <asp:GridView ID="ClassGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="ClassIncomeSQL" OnRowDataBound="ClassGridView_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" DataFormatString="{0:N0}">
                                <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="ClassIncomeSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        StudentsClass.ClassID, CreateClass.Class, SUM(Income_PaymentRecord.PaidAmount) AS Amount
FROM            Income_PaymentRecord INNER JOIN
                         StudentsClass ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                         CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE        (Income_PaymentRecord.SchoolID = @SchoolID) AND (CAST(Income_PaymentRecord.PaidDate AS DATE) BETWEEN ISNULL(@From_Date, 
                         '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
GROUP BY StudentsClass.ClassID, CreateClass.Class
ORDER BY StudentsClass.ClassID">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <br />
                    <%} %>

                    <%if (CommitteeGridView.Rows.Count > 0)
                        {%>
                    <div class="box Income-box"><i class="fa fa-arrow-circle-down" aria-hidden="true"></i>&nbsp Donation wise Income</div>
                    <asp:GridView ID="CommitteeGridView" runat="server" AutoGenerateColumns="False" DataSourceID="CommitteeGridViewSQL" CssClass="mGrid" AllowSorting="True" OnRowDataBound="IncomeCategoryGridView_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="DonationCategory" HeaderText="Category" ReadOnly="True" SortExpression="DonationCategory" />
                            <asp:BoundField DataField="Donation" HeaderText="Donation" ReadOnly="True" SortExpression="Donation" DataFormatString="{0:N0}">
                                <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="CommitteeGridViewSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeDonationCategory.DonationCategory, SUM(CommitteePaymentRecord.PaidAmount) AS Donation FROM CommitteeMoneyReceipt INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId INNER JOIN CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolID) AND (CAST(CommitteeMoneyReceipt.PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY CommitteeDonationCategory.DonationCategory"
                        CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <br />
                    <%} %>
                </div>

                <div class="Acc-expense">
                    <%if (Ex_CategoryGridView.Rows.Count > 0)
                        {%>
                    <div class="box Expense-box"><i class="fa fa-arrow-circle-up" aria-hidden="true"></i>&nbsp Expense</div>
                    <asp:GridView ID="Ex_CategoryGridView" runat="server" AutoGenerateColumns="False" DataSourceID="Ex_CategorySQL" CssClass="mGrid" AllowSorting="True" OnRowDataBound="Ex_CategoryGridView_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="Category" HeaderText="Category" ReadOnly="True" SortExpression="Category" />
                            <asp:BoundField DataField="Total" HeaderText="Total" ReadOnly="True" SortExpression="Total" DataFormatString="{0:N0}">
                                <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="Ex_CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Amount) AS Total from(SELECT Employee_Payorder_Name.Payorder_Name AS Category, SUM(Employee_Payorder_Records.Amount) AS Amount
FROM            Employee_Payorder_Records INNER JOIN
                         Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN
                         Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
WHERE        (Employee_Payorder_Records.SchoolID = @SchoolID) AND (Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From_Date, N'1-1-1000') AND ISNULL(@To_Date, N'1-1-3000'))  
GROUP BY Employee_Payorder_Name.Payorder_Name
Union 
SELECT      Expense_CategoryName.CategoryName AS Category , SUM(Expenditure.Amount) AS Amount
FROM            Expenditure INNER JOIN
                         Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
WHERE        (Expenditure.SchoolID = @SchoolID)  and Expenditure.ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')
GROUP BY Expense_CategoryName.CategoryName)as t  GROUP  BY Category"
                        CancelSelectOnNullParameter="False">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <%} %>
                </div>
            </div>
        </div>

        <div id="tab2" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <div class="box Income-box"><i class="fa fa-arrow-circle-down" aria-hidden="true"></i>&nbsp Income</div>

            <asp:Repeater ID="IncomeRepeater" runat="server" DataSourceID="IncomeDetailsSQL">
                <ItemTemplate>
                    <div class="table-responsive mb-3">
                        <div class="pull-left">
                            <asp:Label ID="CategoryLabel" runat="server" Text='<%# Eval("Category") %>' />
                        </div>
                        <div class="pull-right">
                            ৳<%# Eval("Income","{0:N0}") %>
                        </div>

                        <asp:GridView ID="DetailsGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="DetailsSQL" AllowSorting="True" AllowPaging="True" PageSize="150" OnRowDataBound="DetailsGridView_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="UserName" HeaderText="User Name" ReadOnly="True" SortExpression="UserName" />
                                <asp:BoundField DataField="AccountName" HeaderText="Account" ReadOnly="True" SortExpression="AccountName" />
                                <asp:BoundField DataField="Details" HeaderText="Details" ReadOnly="True" SortExpression="Details">
                                    <ItemStyle HorizontalAlign="Left" />
                                </asp:BoundField>
                                <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" DataFormatString="{0:N0}">
                                    <ItemStyle HorizontalAlign="Right" />
                                </asp:BoundField>
                                <asp:BoundField DataField="Date" HeaderText="Date" ReadOnly="True" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="DetailsSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, ISNULL(Account.AccountName, 'N/A') AS AccountName,Income_Roles.Role as Category, 
                       '['+Student.ID+'] ' + Student.StudentsName + ', Class: ' + CreateClass.Class + ', For: ' + Income_PaymentRecord.PayFor AS Details, Income_PaymentRecord.PaidAmount AS Amount, cast(Income_PaymentRecord.PaidDate as Date) as [Date] FROM Admin INNER JOIN Income_PaymentRecord INNER JOIN
                         Registration ON Income_PaymentRecord.RegistrationID = Registration.RegistrationID INNER JOIN
                         Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN
                         Student ON Income_PaymentRecord.StudentID = Student.StudentID INNER JOIN
                         StudentsClass INNER JOIN
                         CreateClass ON StudentsClass.ClassID = CreateClass.ClassID ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID ON 
                         Admin.RegistrationID = Registration.RegistrationID LEFT OUTER JOIN
                         Account ON Income_PaymentRecord.AccountID = Account.AccountID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) and cast(Income_PaymentRecord.PaidDate as Date)   BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND Income_Roles.Role = @Category
Union 
SELECT ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, ISNULL(Account.AccountName, 'N/A') AS AccountName, 
                         Extra_IncomeCategory.Extra_Income_CategoryName AS Category, Extra_Income.Extra_IncomeFor AS Details, Extra_Income.Extra_IncomeAmount AS Amount, Extra_Income.Extra_IncomeDate AS [Date] FROM Extra_Income INNER JOIN
                         Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID INNER JOIN
                         Registration ON Extra_Income.RegistrationID = Registration.RegistrationID INNER JOIN
                         Admin ON Registration.RegistrationID = Admin.RegistrationID LEFT OUTER JOIN
                         Account ON Extra_Income.AccountID = Account.AccountID WHERE(Extra_Income.SchoolID = @SchoolID) and Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND Extra_IncomeCategory.Extra_Income_CategoryName = @Category
Union
SELECT        ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, ISNULL(Account.AccountName, 'N/A') AS AccountName, 
                         CommitteeDonationCategory.DonationCategory AS Category, CommitteeDonation.Description AS Details, CommitteeMoneyReceipt.TotalAmount AS Amount, CommitteeMoneyReceipt.PaidDate AS Date
FROM            CommitteePaymentRecord INNER JOIN
                         CommitteeMoneyReceipt ON CommitteePaymentRecord.CommitteeMoneyReceiptId = CommitteeMoneyReceipt.CommitteeMoneyReceiptId INNER JOIN
                         CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN
                         CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN
                         Registration ON CommitteeMoneyReceipt.RegistrationId = Registration.RegistrationID INNER JOIN
                         Admin ON Registration.RegistrationID = Admin.RegistrationID LEFT OUTER JOIN
                         Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID
WHERE        (CommitteeMoneyReceipt.SchoolId = @SchoolID) AND (CommitteeMoneyReceipt.PaidDate BETWEEN ISNULL(@From_Date, N'1-1-1000') AND ISNULL(@To_Date, N'1-1-3000')) AND 
                         (CommitteeDonationCategory.DonationCategory = @Category)
ORDER BY Date
                            ">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                                <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                                <asp:ControlParameter ControlID="CategoryLabel" Name="Category" PropertyName="Text" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <asp:SqlDataSource ID="IncomeDetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Income) AS Income from(SELECT Income_Roles.Role AS Category, SUM(Income_PaymentRecord.PaidAmount) AS Income FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) and cast(Income_PaymentRecord.PaidDate as Date)   BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY Income_Roles.Role
Union 
SELECT Extra_IncomeCategory.Extra_Income_CategoryName AS Category , SUM(Extra_Income.Extra_IncomeAmount) AS Income
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID) and Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName
Union 
SELECT CommitteeDonationCategory.DonationCategory AS Category, SUM(CommitteePaymentRecord.PaidAmount) AS Income FROM CommitteePaymentRecord INNER JOIN CommitteeDonation ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN CommitteeDonationCategory ON CommitteeDonationCategory.CommitteeDonationCategoryId = CommitteeDonation.CommitteeDonationCategoryId INNER JOIN CommitteeMoneyReceipt ON CommitteePaymentRecord.CommitteeMoneyReceiptId = CommitteeMoneyReceipt.CommitteeMoneyReceiptId
WHERE (CommitteePaymentRecord.SchoolID = @SchoolID) and cast(CommitteeMoneyReceipt.PaidDate as Date)   BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY CommitteeDonationCategory.DonationCategory)as t  GROUP  BY Category order by t.Category"
                CancelSelectOnNullParameter="False">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                    <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div id="tab3" class="tab-pane fade" role="tabpanel" aria-expanded="false">
        <div class="box Expense-box"><i class="fa fa-arrow-circle-up" aria-hidden="true">&nbsp Expense</i></div>
     <asp:Repeater ID="ExpenseRepeater" runat="server" DataSourceID="ExpenseCategorySQL">
<ItemTemplate>
       <div class="table-responsive mb-3">
                <div class="pull-left">
              <asp:Label ID="CategoryLabel" runat="server" Text='<%# Eval("Category") %>' />
        </div>
      <div class="pull-right">
             ৳<%# Eval("Total","{0:N0}") %>
         </div>

       <asp:GridView ID="DetailsGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="DetailsSQL" AllowSorting="True" AllowPaging="True" PageSize="150" OnRowDataBound="DetailsGridView_RowDataBound">
       <Columns>
  <asp:BoundField DataField="UserName" HeaderText="User Name" ReadOnly="True" SortExpression="UserName" />
   <asp:BoundField DataField="AccountName" HeaderText="Account" ReadOnly="True" SortExpression="AccountName" />
                <asp:BoundField DataField="Details" HeaderText="Details" ReadOnly="True" SortExpression="Details">
            <ItemStyle HorizontalAlign="Left" />
           </asp:BoundField>
<asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" DataFormatString="{0:N0}">
          <ItemStyle HorizontalAlign="Right" />
     </asp:BoundField>
<asp:BoundField DataField="Date" HeaderText="Date" ReadOnly="True" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
        </Columns>
        <PagerStyle CssClass="pgr" />
         </asp:GridView>
     <asp:SqlDataSource ID="DetailsSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, ISNULL(Account.AccountName, 'N/A') AS AccountName,Expense_CategoryName.CategoryName as Category, 
              Expenditure.ExpenseFor AS Details, Expenditure.Amount, Expenditure.ExpenseDate as [Date]
FROM   Expenditure INNER JOIN
      Registration ON Expenditure.RegistrationID = Registration.RegistrationID INNER JOIN
           Expense_CategoryName ON Expenditure.ExpenseCategoryID= Expense_CategoryName.ExpenseCategoryID INNER JOIN
               Admin ON Admin.RegistrationID = Registration.RegistrationID LEFT OUTER JOIN
            Account ON Expenditure.AccountID = Account.AccountID
WHERE        (Expenditure.SchoolID = @SchoolID) and Expenditure.ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND Expense_CategoryName.CategoryName = @Category
Union ALL
SELECT   ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, ISNULL(Account.AccountName, 'N/A') AS AccountName, 
        Employee_Payorder_Name.Payorder_Name AS Category, Employee_Payorder_Records.Paid_For AS Details, Employee_Payorder_Records.Amount, Employee_Payorder_Records.Paid_date AS [Date]
FROM       Registration INNER JOIN
         Admin ON Admin.RegistrationID = Registration.RegistrationID INNER JOIN
           Employee_Payorder_Records ON Registration.RegistrationID = Employee_Payorder_Records.RegistrationID INNER JOIN
      Employee_Payorder ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Records.Employee_PayorderID INNER JOIN
     Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID LEFT OUTER JOIN
        Account ON Employee_Payorder_Records.AccountID = Account.AccountID
WHERE        (Employee_Payorder_Records.SchoolID = @SchoolID) AND (Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From_Date, N'1-1-1000') AND ISNULL(@To_Date, N'1-1-3000')) AND Employee_Payorder_Name.Payorder_Name = @Category
order by [Date]">
           <SelectParameters>
      <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
       <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
         <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
          <asp:ControlParameter ControlID="CategoryLabel" Name="Category" PropertyName="Text" />
       </SelectParameters>
     </asp:SqlDataSource>
  </div>
     </ItemTemplate>
            </asp:Repeater>
            <asp:SqlDataSource ID="ExpenseCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Amount) AS Total from(SELECT Employee_Payorder_Name.Payorder_Name AS Category, SUM(Employee_Payorder_Records.Amount) AS Amount
FROM    Employee_Payorder_Records INNER JOIN
  Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN
     Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
WHERE        (Employee_Payorder_Records.SchoolID = @SchoolID) AND (Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From_Date, N'1-1-1000') AND ISNULL(@To_Date, N'1-1-3000'))  
GROUP BY Employee_Payorder_Name.Payorder_Name
Union 
SELECT      Expense_CategoryName.CategoryName AS Category , SUM(Expenditure.Amount) AS Amount
FROM         Expenditure INNER JOIN
        Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
WHERE        (Expenditure.SchoolID = @SchoolID)  and Expenditure.ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')
GROUP BY Expense_CategoryName.CategoryName)as t  GROUP  BY Category"
            CancelSelectOnNullParameter="False">
    <SelectParameters>
              <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
          <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                  <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
</SelectParameters>
        </asp:SqlDataSource>
        </div>
    </div>

    <script>
        $(function () {
            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            if (!$("[id*=IncomeCategoryGridView] tr").length) {
                //$('.Acc-income').hide();
                $('.Acc-expense').addClass("w-100");
            }

            if (!$("[id*=CommitteeGridView] tr").length) {
                //$('.Acc-donations').hide();
                $('.Acc-expense').addClass("w-100");
            }

            if (!$("[id*=Ex_CategoryGridView] tr").length) {
                //$('.Acc-expense').hide();
                $('.Acc-income').addClass("w-100");
            }

            //get date in label
            var from = $("[id*=From_Date_TextBox]").val();
            var To = $("[id*=To_Date_TextBox]").val();

            var tt;
            var Brases1 = "";
            var Brases2 = "";
            var A = "";
            var B = "";
            var TODate = "";

            if (To == "" || from == "" || To == "" && from == "") {
                tt = "";
                A = "";
                B = "";
            }
            else {
                tt = " To ";
                Brases1 = "(";
                Brases2 = ")";
            }

            if (To == "" && from == "") { Brases1 = ""; }

            if (To == from) {
                TODate = "";
                tt = "";
                var Brases1 = "";
                var Brases2 = "";
            }
            else { TODate = To; }

            if (from == "" && To != "") {
                B = " Before ";
            }

            if (To == "" && from != "") {
                A = " After ";
            }

            if (from != "" && To != "") {
                A = "";
                B = "";
            }

            $(".Date").text(Brases1 + B + A + from + tt + TODate + Brases2);

            //Date range picker
            function cb(start, end) {
                $('[id*=From_Date_TextBox]').val(start.format('D MMMM YYYY'));
                $('[id*=To_Date_TextBox]').val(end.format('D MMMM YYYY'));

                $("[id*=Find_Button]").trigger("click");
            }

            $('#PickDate').daterangepicker({
                autoApply: true,
                showCustomRangeLabel: false,
                ranges: {
                    'Today': [moment(), moment()],
                    'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                    'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                    'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                    'This Month': [moment().startOf('month'), moment().endOf('month')],
                    'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
                    'This Year': [moment().startOf('year'), moment().endOf('year')]
                }
            }, cb);

            cb(start, end);
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
