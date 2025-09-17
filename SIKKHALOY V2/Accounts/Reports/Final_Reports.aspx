<%@ Page Title="Accounts Summary" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Final_Reports.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.Final_Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Final_Reports.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3><i class="fa fa-th-list" aria-hidden="true"></i>&nbsp ACCOUNTS SUMMARY</h3>

    <div class="row">
        <div class="col-xl-3">
            <div class="Category-Section z-depth-1">
                <div class="box User-box" style="margin-top: 0;">
                    <i class="fa fa-users" aria-hidden="true"></i>&nbsp User Accounts Details
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="UserGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RegistrationID" DataSourceID="UserSQL" CssClass="mGrid">
                        <Columns>
                            <asp:TemplateField HeaderText="Name" SortExpression="Name">
                                <ItemTemplate>
                                    <a href="UserAccount.aspx?RegID=<%# Eval("RegistrationID") %>"><%#Eval("Name") %></a>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Income" HeaderText="Income" ReadOnly="True" SortExpression="Income" DataFormatString="{0:N0}" />
                            <asp:BoundField DataField="Expense" HeaderText="Expense" ReadOnly="True" SortExpression="Expense" DataFormatString="{0:N0}" />
                        </Columns>
                        <EmptyDataTemplate>
                            Empty
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <asp:SqlDataSource ID="UserSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * from(SELECT User_T.RegistrationID,User_T.AdminID, User_T.Name,ISNULL(EX_In_T.Other_Income,0) +ISNULL(Com_In_T.CommitteeDonation,0)+ ISNULL(Stu_P_T.Student_Income,0) AS Income,ISNULL(Ex_T.Expenditure,0) + ISNULL(Emp_P_T.Employee_Paid,0) AS Expense from (SELECT DISTINCT Registration.RegistrationID,Admin.AdminID, ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS Name
FROM            Registration INNER JOIN
							Admin ON Registration.RegistrationID = Admin.RegistrationID LEFT OUTER JOIN
							Extra_Income ON Registration.RegistrationID = Extra_Income.RegistrationID LEFT OUTER JOIN
							Income_PaymentRecord ON Registration.RegistrationID = Income_PaymentRecord.RegistrationID LEFT OUTER JOIN
							Expenditure ON Registration.RegistrationID = Expenditure.RegistrationID LEFT OUTER JOIN
							CommitteeMoneyReceipt ON Registration.RegistrationID = CommitteeMoneyReceipt.RegistrationID LEFT OUTER JOIN
							Employee_Payorder_Records ON Registration.RegistrationID = Employee_Payorder_Records.RegistrationID
WHERE        (Registration.SchoolID = @SchoolID)) as User_T
LEFT OUTER JOIN
(SELECT RegistrationID , ISNULL(SUM(Extra_IncomeAmount), 0) AS Other_Income FROM Extra_Income WHERE (SchoolID = @SchoolID) GROUP BY RegistrationID) AS EX_In_T on User_T.RegistrationID = EX_In_T.RegistrationID
LEFT OUTER JOIN
(SELECT RegistrationID , ISNULL(SUM(TotalAmount), 0) AS CommitteeDonation FROM CommitteeMoneyReceipt WHERE (SchoolID = @SchoolID) GROUP BY RegistrationID) AS Com_In_T on User_T.RegistrationID = Com_In_T.RegistrationID
LEFT OUTER JOIN
(SELECT RegistrationID,ISNULL(SUM(PaidAmount), 0) AS Student_Income FROM Income_PaymentRecord WHERE (SchoolID = @SchoolID) GROUP BY RegistrationID) AS Stu_P_T on User_T.RegistrationID = Stu_P_T.RegistrationID
LEFT OUTER JOIN
(SELECT  RegistrationID,ISNULL(SUM(Amount),0) AS Expenditure FROM Expenditure WHERE (SchoolID = @SchoolID) GROUP BY RegistrationID) AS Ex_T on User_T.RegistrationID =  Ex_T.RegistrationID
LEFT OUTER JOIN
(SELECT RegistrationID, ISNULL(SUM(Amount),0)AS Employee_Paid FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID)GROUP BY RegistrationID) AS Emp_P_T on User_T.RegistrationID = Emp_P_T.RegistrationID) as T where T.Income &lt;&gt; 0 or T.Expense &lt;&gt; 0">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                   <h3 style="font-size:14px; color:orangered"> <i class="fa fa-search" aria-hidden="true"></i> <a href="UserWise_Account_Report.aspx"> Income Expense Summary by user</a></h3>

                </div>

                <div class="box Account-box"><i class="fa fa-list-alt" aria-hidden="true"></i>&nbsp Account List</div>
                <asp:GridView ID="AccountGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="AccountID" DataSourceID="AccountSQL" CssClass="mGrid" ShowFooter="True">
                    <Columns>
                        <asp:TemplateField HeaderText="Account" SortExpression="AccountName">
                            <ItemTemplate>
                                <a href="AccountDetails.aspx?acc=<%#Eval("AccountID") %>"><%#Eval("AccountName") %></a>
                            </ItemTemplate>
                            <FooterTemplate>
                                Total
                            </FooterTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Current Balance" SortExpression="AccountBalance">
                            <FooterTemplate>
                                <asp:FormView ID="AllAccountFormView" runat="server" DataSourceID="AllAccountSQL" Width="100%">
                                    <ItemTemplate>
                                        <%#Eval("Total", "{0:N0}") %> Tk.
                                    </ItemTemplate>
                                </asp:FormView>
                                <asp:SqlDataSource ID="AllAccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SUM(AccountBalance) AS Total
FROM Account WHERE (SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </FooterTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("AccountBalance", "{0:N0}") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                    <FooterStyle CssClass="Acc_Footer" />
                </asp:GridView>
                <asp:SqlDataSource ID="AccountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AccountID, AccountName, AccountBalance, Total_IN AS Diposit, Total_OUT AS Withdraw FROM Account WHERE (SchoolID = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <div class="box Income-box"><i class="fa fa-arrow-circle-down" aria-hidden="true"></i>&nbsp Income Category</div>
                <asp:GridView ID="IncomeCategoryGridView" runat="server" AutoGenerateColumns="False" DataSourceID="IncomeCategorySQL" CssClass="mGrid">
                    <Columns>
                        <asp:TemplateField HeaderText="Category" SortExpression="Category">
                            <ItemTemplate>
                                <a href="Income.aspx?Category=<%# HttpUtility.UrlEncode(Eval("Category").ToString()) %>"><%# Eval("Category") %></a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Income" HeaderText="Income" ReadOnly="True" SortExpression="Income" DataFormatString="{0:N0}" />
                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                </asp:GridView>
                <asp:SqlDataSource ID="IncomeCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Total) AS Income from(SELECT Income_Roles.Role AS Category, SUM(Income_PaymentRecord.PaidAmount) AS Total
FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID)
GROUP BY Income_Roles.Role
Union
SELECT CommitteeDonationCategory.DonationCategory AS Category, SUM(CommitteePaymentRecord.PaidAmount) AS Total 
FROM CommitteePaymentRecord INNER JOIN CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId 
WHERE (CommitteePaymentRecord.SchoolId = @SchoolID) 
GROUP BY CommitteeDonationCategory.DonationCategory
Union 
SELECT Extra_IncomeCategory.Extra_Income_CategoryName AS Category , SUM(Extra_Income.Extra_IncomeAmount) AS Total
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE(Extra_Income.SchoolID = @SchoolID)
GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName)as t GROUP  BY Category">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <div class="box Expense-box"><i class="fa fa-arrow-circle-up" aria-hidden="true"></i>&nbsp Expense Category</div>
                <asp:GridView ID="Ex_CategoryGridView" runat="server" AutoGenerateColumns="False" DataSourceID="Ex_CategorySQL" CssClass="mGrid">
                    <Columns>
                        <asp:TemplateField HeaderText="Category" SortExpression="Category">
                            <ItemTemplate>
                                <a href="Expense.aspx?Category=<%#  HttpUtility.UrlEncode(Eval("Category").ToString()) %>"><%# Eval("Category") %></a>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Expense" HeaderText="Expense" ReadOnly="True" SortExpression="Expense" DataFormatString="{0:N0}" />
                    </Columns>
                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>
                </asp:GridView>
                <asp:SqlDataSource ID="Ex_CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Payorder_Name.Payorder_Name AS Category, SUM(Employee_Payorder_Records.Amount) AS Expense
FROM Employee_Payorder_Records INNER JOIN Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN
	Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
WHERE (Employee_Payorder_Records.SchoolID = @SchoolID) GROUP BY Employee_Payorder_Name.Payorder_Name
Union SELECT Expense_CategoryName.CategoryName AS Category , SUM(Expenditure.Amount) AS Total FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
WHERE (Expenditure.SchoolID = @SchoolID) GROUP BY Expense_CategoryName.CategoryName">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>

        <div class="col-xl-9">
            <!-- All Payment-->
            <div class="All_Payment-Section card">
                <div class="card-body">
                    <div class="All-session-Title">
                        <h2><i class="fa fa-pie-chart" aria-hidden="true"></i>&nbsp ALL SESSION PAYMENT</h2>
                    </div>

                    <asp:FormView ID="IncomeFormView" runat="server" DataSourceID="IncomeSQL" Width="100%">
                        <ItemTemplate>
                            <div class="row">
                                <div class="col-lg mb-md-4">
                                    <div class="Income-widget">
                                        <div class="P-Heading clearfix">
                                            <div class="pull-left">
                                                <i class="fa fa-line-chart" aria-hidden="true"></i>
                                            </div>
                                            <div class="pull-right">
                                                <h2>
                                                    <%#Eval("Total_Revenue","{0:N0}") %>
                                                    Tk</h2>
                                                <div class="Title-f-size">Income</div>
                                            </div>
                                        </div>
                                        <div class="P-Footer clearfix">
                                            <a href="Income.aspx?All=1">
                                                <div class="pull-left">View Details</div>
                                                <div class="pull-right"><i class="fa fa-arrow-circle-right" aria-hidden="true"></i></div>
                                            </a>
                                        </div>
                                    </div>
                                </div>

                                <div class="col-lg mb-md-4">
                                    <div class="Expense-widget">
                                        <div class="P-Heading clearfix">
                                            <div class="pull-left">
                                                <i class="fa fa-area-chart" aria-hidden="true"></i>
                                            </div>
                                            <div class="pull-right">
                                                <h2>
                                                    <%#Eval("Total_Expense","{0:N0}") %>
                                                    Tk</h2>
                                                <div class="Title-f-size">Expense</div>
                                            </div>
                                        </div>
                                        <div class="P-Footer clearfix">
                                            <a href="Expense.aspx?All=1">
                                                <div class="pull-left">View Details</div>
                                                <div class="pull-right"><i class="fa fa-arrow-circle-right" aria-hidden="true"></i></div>
                                                <div class="pull-right"></div>
                                            </a>
                                        </div>
                                    </div>
                                </div>

                                <div class="col-lg mb-md-4">
                                    <div class="Net-widget">
                                        <div class="P-Heading clearfix">
                                            <div class="pull-left">
                                                <i class="fa fa-bar-chart" aria-hidden="true"></i>
                                            </div>
                                            <div class="pull-right">
                                                <h2>
                                                    <%# Eval("Net","{0:N0}") %>
                                                    Tk</h2>
                                                <div class="Title-f-size">Net Amount</div>
                                            </div>
                                        </div>
                                        <div class="P-Footer clearfix">
                                            <a href="Net.aspx?All=1">
                                                <div class="pull-left">View Details</div>
                                                <div class="pull-right"><i class="fa fa-arrow-circle-right" aria-hidden="true"></i></div>
                                            </a>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="IncomeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="select Other_Income + Studnet_Paid + CommitteeDonation as Total_Revenue,Expenditure + Employee_Paid as Total_Expense , Other_Income + Studnet_Paid +CommitteeDonation - Expenditure - Employee_Paid as Net from ( SELECT 
(SELECT ISNULL(SUM(Extra_IncomeAmount),0)  FROM Extra_Income WHERE (SchoolID = @SchoolID))AS Other_Income,
(SELECT ISNULL(SUM(TotalAmount), 0) FROM CommitteeMoneyReceipt WHERE        (SchoolId = @SchoolID)) AS CommitteeDonation,
(SELECT ISNULL(SUM(PaidAmount),0) FROM Income_PaymentRecord WHERE (SchoolID = @SchoolID))AS Studnet_Paid,
(SELECT ISNULL(SUM(Amount),0)  FROM Expenditure WHERE (SchoolID = @SchoolID))AS Expenditure,
(SELECT ISNULL(SUM(Amount),0) FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID))AS Employee_Paid) as t">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <!-- Student Payment-->
                    <div class="Student-Section">
                        <div class="Student-Title mt-4 py-1">
                            <h2><i class="fa fa-users" aria-hidden="true"></i>&nbsp STUDENT PAYMENT</h2>
                        </div>

                        <asp:FormView ID="Stu_PD_FormView" runat="server" DataSourceID="Stu_PD_SQL" Width="100%">
                            <ItemTemplate>
                                <div class="row no-gutters">
                                    <div class="col-lg">
                                        <div class="Payorder-bg mb-md-3">
                                            <div class="Fees-section">
                                                <small>Payorder</small>
                                                <h2>
                                                    <asp:Label ID="TotalFeeLabel" runat="server" Text='<%# Bind("TotalFee","{0:N0}") %>' />
                                                    Tk</h2>
                                            </div>
                                            <div class="P-Footer">
                                                <a href="PayOrderReport.aspx">
                                                    <small>Details <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg">
                                        <div class="Latefee-bg mb-md-3">
                                            <div class="Fees-section">
                                                <small>Late Fee</small>
                                                <h2>
                                                    <asp:Label ID="TotalLateFeeLabel" runat="server" Text='<%# Bind("TotalLateFee","{0:N0}") %>' />
                                                    Tk</h2>
                                            </div>
                                            <div class="P-Footer">
                                                <a href="PayOrderReport.aspx">
                                                    <small>Details <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg">
                                        <div class="Concession-bg mb-md-3">
                                            <div class="Fees-section">
                                                <small>Concession</small>
                                                <h2>
                                                    <asp:Label ID="TotalDiscountLabel" runat="server" Text='<%# Bind("TotalDiscount","{0:N0}") %>' />
                                                    Tk</h2>
                                            </div>
                                            <div class="P-Footer">
                                                <a href="PayOrderReport.aspx">
                                                    <small>Details <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg Paid-link">
                                        <a href="PaidReport.aspx">
                                            <div class="Paid-bg mb-md-3">
                                                <div class="Fees-section">
                                                    <small>Paid</small>
                                                    <h2><%# Eval("Paid","{0:N0}") %> Tk</h2>
                                                </div>
                                                <div class="P-Footer">
                                                    <small>Adv. Paid <%# Eval("Advance","{0:N0}") %> 
                                           Tk <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                </div>
                                            </div>
                                        </a>
                                    </div>
                                    <div class="col-lg">
                                        <div class="Unpaid-bg mb-md-3">
                                            <div class="Fees-section">
                                                <small>Unpaid</small>
                                                <h2>
                                                    <%# Eval("Unpaid","{0:N0}") %>
                                                    Tk</h2>
                                            </div>
                                            <div class="P-Footer">
                                                <a href="../AccountsAnalysis/Present_Due.aspx">
                                                    <small>Curr. Due <%# Eval("PresentDue","{0:N0}") %>
                                          Tk <i class="fa fa-arrow-circle-right" aria-hidden="true"></i>
                                                    </small>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:FormView>
                        <asp:SqlDataSource ID="Stu_PD_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SUM(Amount) AS TotalFee,
SUM(LateFeeCountable) AS TotalLateFee,
SUM(Total_Discount) AS TotalDiscount,
SUM(PaidAmount) AS Paid, 
SUM(Receivable_Amount) AS Unpaid,
(SELECT SUM(ISNULL(Receivable_Amount, 0)) AS PresentDue FROM Income_PayOrder WHERE (EndDate &lt; GETDATE()) AND (SchoolID = @SchoolID) AND (Is_Active = 1)) AS PresentDue,
(SELECT SUM(ISNULL(PaidAmount, 0)) FROM Income_PayOrder WHERE (StartDate &gt; GETDATE()) AND (SchoolID = @SchoolID)) AS Advance
FROM Income_PayOrder
WHERE (SchoolID = @SchoolID) AND (Is_Active = 1)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>
            <!-- Session Payment-->
            <div class="Session-Section">
                <asp:Repeater ID="SessionRepeater" runat="server" DataSourceID="SessionSQL">
                    <ItemTemplate>
                        <div class="card mt-3">
                            <div class="card-body">
                                <asp:HiddenField ID="Edu_Year_IDHF" runat="server" Value='<%# Eval("EducationYearID") %>' />
                                <div class="Session-Title">
                                    <h2><i class="fa fa-database" aria-hidden="true"></i>&nbsp SESSION <%# Eval("EducationYear") %></h2>
                                    <div class="session-date">
                                        <i class="fa fa-calendar" aria-hidden="true"></i>
                                        <asp:Label ID="StartDateLabel" runat="server" Text='<%# Eval("StartDate","{0:d MMM yyyy}") %>' />
                                        -
						<asp:Label ID="EndDateLabel" runat="server" Text='<%# Eval("EndDate","{0:d MMM yyyy}") %>' />
                                    </div>
                                </div>

                                <div class="row">
                                    <div class="col-lg-4 mb-md-3">
                                        <div class="Session-Total-amt">
                                            <div class="P-Heading clearfix">
                                                <div class="pull-left">
                                                    <h2>
                                                        <asp:Label ID="IncomeLabel" runat="server" Text='<%# Eval("Income","{0:N0}") %>' />
                                                        Tk</h2>
                                                    <div class="Title-f-size">Income</div>
                                                </div>
                                                <div class="pull-right">
                                                    <i class="fa fa-line-chart" aria-hidden="true"></i>
                                                </div>
                                            </div>
                                            <div class="P-Footer clearfix">
                                                <a href="Income.aspx?f=<%# Eval("StartDate","{0:d MMM yyyy}") %>&t=<%# Eval("EndDate","{0:d MMM yyyy}") %>">
                                                    <div class="pull-left">View Details</div>
                                                    <div class="pull-right"><i class="fa fa-arrow-circle-right" aria-hidden="true"></i></div>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-4 mb-md-3">
                                        <div class="Session-Total-amt">
                                            <div class="P-Heading clearfix">
                                                <div class="pull-left">
                                                    <h2>
                                                        <asp:Label ID="ExpenseLabel" runat="server" Text='<%# Eval("Expense","{0:N0}") %>' />
                                                        Tk</h2>
                                                    <div class="Title-f-size">Expense</div>
                                                </div>
                                                <div class="pull-right">
                                                    <i class="fa fa-area-chart" aria-hidden="true"></i>
                                                </div>
                                            </div>
                                            <div class="P-Footer clearfix">
                                                <a href="Expense.aspx?f=<%# Eval("StartDate","{0:d MMM yyyy}") %>&t=<%# Eval("EndDate","{0:d MMM yyyy}") %>">
                                                    <div class="pull-left">View Details</div>
                                                    <div class="pull-right"><i class="fa fa-arrow-circle-right" aria-hidden="true"></i></div>
                                                    <div class="pull-right"></div>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-4 mb-md-3">
                                        <div class="Session-Total-amt">
                                            <div class="P-Heading clearfix">
                                                <div class="pull-left">
                                                    <h2>
                                                        <asp:Label ID="NETLabel" runat="server" Text='<%# Eval("NET","{0:N0}") %>' />
                                                        Tk</h2>
                                                    <div class="Title-f-size">Net Amount</div>
                                                </div>
                                                <div class="pull-right">
                                                    <i class="fa fa-bar-chart" aria-hidden="true"></i>
                                                </div>
                                            </div>
                                            <div class="P-Footer clearfix">
                                                <a href="Net.aspx?f=<%# Eval("StartDate","{0:d MMM yyyy}") %>&t=<%# Eval("EndDate","{0:d MMM yyyy}") %>">
                                                    <div class="pull-left">View Details</div>
                                                    <div class="pull-right"><i class="fa fa-arrow-circle-right" aria-hidden="true"></i></div>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <asp:FormView ID="Sessi_Student_FormView" runat="server" DataSourceID="Stu_P_SQL" Width="100%">
                                    <ItemTemplate>
                                        <div class="Payorder-paid">
                                            <div class="Student-Title py-2">
                                                <h2><i class="fa fa-users" aria-hidden="true"></i>&nbsp STUDENT PAYMENT</h2>
                                            </div>

                                            <div class="row no-gutters">
                                                <div class="col-lg mb-md-3">
                                                    <div class="s-Payorder-bg">
                                                        <div class="Fees-section">
                                                            <small>Payorder</small>
                                                            <h2>
                                                                <asp:Label ID="TotalFeeLabel" runat="server" Text='<%# Bind("TotalFee","{0:N0}") %>' />
                                                                Tk</h2>
                                                        </div>
                                                        <div class="P-Footer">
                                                            <a href="../ReportsSession/SessionPayorderReport.aspx?EducationYearID=<%# Eval("EducationYearID") %>">
                                                                <small>Details <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                            </a>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-lg mb-md-3">
                                                    <div class="s-Latefee-bg">
                                                        <div class="Fees-section">
                                                            <small>Late Fee</small>
                                                            <h2>
                                                                <asp:Label ID="TotalLateFeeLabel" runat="server" Text='<%# Bind("TotalLateFee","{0:N0}") %>' />
                                                                Tk</h2>
                                                        </div>
                                                        <div class="P-Footer">
                                                            <small>Details <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-lg mb-md-3">
                                                    <div class="s-Concession-bg">
                                                        <div class="Fees-section">
                                                            <small>Concession</small>
                                                            <h2>
                                                                <asp:Label ID="TotalDiscountLabel" runat="server" Text='<%# Bind("TotalDiscount","{0:N0}") %>' />
                                                                Tk</h2>
                                                        </div>
                                                        <div class="P-Footer">
                                                            <a href="../ReportsSession/Concession_Report.aspx?EducationYearID=<%# Eval("EducationYearID") %>">
                                                                <small>Details <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                            </a>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div class="col-lg mb-md-3">
                                                    <div class="s-Paid-bg">
                                                        <div class="Fees-section">
                                                            <small>Paid</small>
                                                            <h2>
                                                                <asp:Label ID="TotalPaidLabel" runat="server" Text='<%# Bind("TotalPaid","{0:N0}") %>' />
                                                                Tk</h2>
                                                        </div>
                                                        <div class="P-Footer">
                                                            <a href="../ReportsSession/Session_Paid_Report.aspx?Year=<%# Eval("EducationYearID") %>">
                                                                <small>Details <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                            </a>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-lg mb-md-3">
                                                    <div class="s-Unpaid-bg">
                                                        <div class="Fees-section">
                                                            <small>Unpaid</small>
                                                            <h2>
                                                                <asp:Label ID="UnpaidLabel" runat="server" Text='<%# Bind("Unpaid","{0:N0}") %>' />
                                                                Tk</h2>
                                                        </div>
                                                        <div class="P-Footer">
                                                            <a href="../ReportsSession/Session_Due_Report.aspx?Year=<%# Eval("EducationYearID") %>">
                                                                <small>Details <i class="fa fa-arrow-circle-right" aria-hidden="true"></i></small>
                                                            </a>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                    </ItemTemplate>
                                </asp:FormView>
                                <asp:SqlDataSource ID="Stu_P_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT    EducationYearID, COUNT(DISTINCT StudentID) AS Total_Stu,   
														SUM(Amount) AS TotalFee, 
					SUM(LateFeeCountable) AS TotalLateFee, 
					SUM(Total_Discount) AS TotalDiscount, 
					SUM(ISNULL(PaidAmount, 0)) AS TotalPaid,
					SUM(Receivable_Amount) AS Unpaid
FROM          Income_PayOrder
WHERE        (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (Is_Active = 1) 
GROUP BY EducationYearID">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:ControlParameter ControlID="Edu_Year_IDHF" Name="EducationYearID" PropertyName="Value" />
                                    </SelectParameters>
                                </asp:SqlDataSource>

                                <div class="Student-Title py-2">
                                    <h2><i class="fa fa-calendar-check-o" aria-hidden="true"></i>&nbsp MONTHLY STATEMENT</h2>
                                </div>

                                <div class="table-responsive">
                                    <asp:GridView ID="MonthsGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataSourceID="MonthsSQL" AllowSorting="True">
                                        <Columns>
                                            <asp:BoundField DataField="Months" HeaderText="Months" ReadOnly="True" SortExpression="Months" />
                                            <asp:BoundField DataField="Income" HeaderText="Income" ReadOnly="True" SortExpression="Income" DataFormatString="{0:N0}" />
                                            <asp:BoundField DataField="Expense" HeaderText="Expense" ReadOnly="True" SortExpression="Expense" DataFormatString="{0:N0}" />
                                            <asp:BoundField DataField="NET" HeaderText="NET" ReadOnly="True" SortExpression="NET" DataFormatString="{0:N0}" />
                                        </Columns>
                                    </asp:GridView>
                                    <asp:SqlDataSource ID="MonthsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT M_T.Months, ISNULL(Ex_In_T.Ex_In,0) + ISNULL(Stu_In_T.Stu_In,0)+ ISNULL(Com_In_T.Com_In,0) AS Income,ISNULL(Ex_T.Ex,0) + ISNULL(Emp_Ex_T.Emp_Ex,0) AS Expense,  ISNULL(Ex_In_T.Ex_In,0) + ISNULL(Stu_In_T.Stu_In,0)+ ISNULL(Com_In_T.Com_In,0) - ISNULL(Ex_T.Ex,0) - ISNULL(Emp_Ex_T.Emp_Ex,0) AS NET FROM 
(SELECT FORMAT(Extra_IncomeDate, 'MMM yyyy') AS Months FROM Extra_Income WHERE (SchoolID = @SchoolID) AND (Extra_IncomeDate BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(Extra_IncomeDate, 'MMM yyyy')
union
SELECT FORMAT(PaidDate, 'MMM yyyy') AS Months FROM Income_PaymentRecord WHERE (SchoolID = @SchoolID) AND (CAST(PaidDate AS DATE) BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(PaidDate, 'MMM yyyy')
union
SELECT FORMAT(ExpenseDate, 'MMM yyyy') AS Months FROM Expenditure WHERE (SchoolID = @SchoolID) AND (ExpenseDate BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(ExpenseDate, 'MMM yyyy')
union
SELECT FORMAT(Paid_date, 'MMM yyyy') AS Months FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID) AND (Paid_Date BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(Paid_date, 'MMM yyyy')
union
SELECT FORMAT(PaidDate, 'MMM yyyy') AS Months FROM CommitteeMoneyReceipt WHERE (SchoolID = @SchoolID) AND (PaidDate BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(PaidDate, 'MMM yyyy')
) M_T 

Left OUTER JOIN 
(SELECT ISNULL(SUM(Extra_IncomeAmount), 0) AS Ex_In, FORMAT(Extra_IncomeDate, 'MMM yyyy') AS Months FROM Extra_Income WHERE (SchoolID = @SchoolID) AND (Extra_IncomeDate BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(Extra_IncomeDate, 'MMM yyyy')) Ex_In_T on M_T.Months = Ex_In_T.Months
Left OUTER JOIN 
(SELECT ISNULL(SUM(TotalAmount), 0) AS Com_In, FORMAT(PaidDate, 'MMM yyyy') AS Months FROM CommitteeMoneyReceipt WHERE (SchoolID = @SchoolID) AND (CAST(PaidDate AS DATE) BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(PaidDate, 'MMM yyyy')) Com_In_T  on M_T.Months = Com_In_T.Months
Left OUTER JOIN 
(SELECT ISNULL(SUM(PaidAmount), 0) AS Stu_In, FORMAT(PaidDate, 'MMM yyyy') AS Months FROM Income_PaymentRecord WHERE (SchoolID = @SchoolID) AND (CAST(PaidDate AS DATE) BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(PaidDate, 'MMM yyyy')) Stu_In_T  on M_T.Months = Stu_In_T.Months
Left OUTER JOIN
(SELECT ISNULL(SUM(Amount), 0) AS Ex, FORMAT(ExpenseDate, 'MMM yyyy') AS Months FROM Expenditure WHERE (SchoolID = @SchoolID) AND (ExpenseDate BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(ExpenseDate, 'MMM yyyy')) Ex_T on M_T.Months = Ex_T.Months
Left OUTER JOIN
(SELECT ISNULL(SUM(Amount), 0) AS Emp_Ex, FORMAT(Paid_date, 'MMM yyyy') AS Months FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID) AND (Paid_Date BETWEEN @StartDate AND @EndDate) GROUP BY FORMAT(Paid_date, 'MMM yyyy')) Emp_Ex_T on M_T.Months = Emp_Ex_T.Months
order by  CONVERT(Date, M_T.Months)">
                                        <SelectParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                            <asp:ControlParameter ControlID="StartDateLabel" Name="StartDate" PropertyName="Text" />
                                            <asp:ControlParameter ControlID="EndDateLabel" Name="EndDate" PropertyName="Text" />
                                        </SelectParameters>
                                    </asp:SqlDataSource>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:SqlDataSource ID="SessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Edu_Year.EducationYearID,Education_Year.EducationYear, Education_Year.StartDate, Education_Year.EndDate, ISNULL(Ex_In_T.Ex_In,0) + ISNULL(Stu_In_T.Stu_In,0)+ ISNULL(Com_In_T.Com_In,0) AS Income,ISNULL(Ex_T.Ex,0) + ISNULL(Emp_Ex_T.Emp_Ex,0) AS Expense,ISNULL(Ex_In_T.Ex_In,0) + ISNULL(Stu_In_T.Stu_In,0) + ISNULL(Com_In_T.Com_In,0)- ISNULL(Ex_T.Ex,0) - ISNULL(Emp_Ex_T.Emp_Ex,0) AS NET FROM 

(SELECT DISTINCT EducationYearID FROM Extra_Income WHERE (SchoolID = @SchoolID)
union
SELECT DISTINCT EducationYearID FROM Income_PaymentRecord WHERE (SchoolID = @SchoolID)
union
SELECT DISTINCT EducationYearID FROM Expenditure WHERE (SchoolID = @SchoolID)
union
SELECT DISTINCT EducationYearID FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID)
union
SELECT DISTINCT EducationYearId FROM CommitteeMoneyReceipt WHERE (SchoolID = @SchoolID)
) AS Edu_Year
inner join
Education_Year on Edu_Year.EducationYearID = Education_Year.EducationYearID
Left OUTER JOIN 
(SELECT Education_Year.EducationYearID, Education_Year.EducationYear, ISNULL(SUM(A_T.Amount), 0) AS Ex
FROM Education_Year INNER JOIN Expenditure AS A_T ON Education_Year.SchoolID = A_T.SchoolID
WHERE (Education_Year.SchoolID = @SchoolID) AND (A_T.ExpenseDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate)
GROUP BY Education_Year.EducationYearID, Education_Year.EducationYear) as Ex_T on  Edu_Year.EducationYearID = Ex_T.EducationYearID 
Left OUTER JOIN
(SELECT Education_Year.EducationYearID, Education_Year.EducationYear, ISNULL(SUM(C_T.TotalAmount), 0) AS Com_In
FROM Education_Year INNER JOIN CommitteeMoneyReceipt AS C_T ON Education_Year.SchoolID = C_T.SchoolID
WHERE (Education_Year.SchoolID = @SchoolID) AND (C_T.PaidDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate)
GROUP BY Education_Year.EducationYearID, Education_Year.EducationYear) as  Com_In_T on Edu_Year.EducationYearID = Com_In_T.EducationYearID
Left OUTER JOIN
(SELECT Education_Year.EducationYearID, Education_Year.EducationYear, ISNULL(SUM(A_T.Extra_IncomeAmount), 0) AS Ex_In
FROM Education_Year INNER JOIN Extra_Income AS A_T ON Education_Year.SchoolID = A_T.SchoolID
WHERE (Education_Year.SchoolID = @SchoolID) AND (A_T.Extra_IncomeDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate)
GROUP BY Education_Year.EducationYearID, Education_Year.EducationYear) as  Ex_In_T on Edu_Year.EducationYearID = Ex_In_T.EducationYearID
Left OUTER JOIN
(SELECT Education_Year.EducationYearID, Education_Year.EducationYear, ISNULL(SUM(A_T.PaidAmount), 0) AS Stu_In
FROM Education_Year INNER JOIN Income_PaymentRecord AS A_T ON Education_Year.SchoolID = A_T.SchoolID
WHERE (Education_Year.SchoolID = @SchoolID) AND (A_T.PaidDate BETWEEN Education_Year.StartDate AND Education_Year.EndDate)
GROUP BY Education_Year.EducationYearID, Education_Year.EducationYear) AS Stu_In_T on Edu_Year.EducationYearID = Stu_In_T.EducationYearID
Left OUTER JOIN
(SELECT Education_Year.EducationYearID, Education_Year.EducationYear, ISNULL(SUM(A_T.Amount), 0) AS Emp_Ex
FROM Education_Year INNER JOIN Employee_Payorder_Records AS A_T ON Education_Year.SchoolID = A_T.SchoolID
WHERE (Education_Year.SchoolID = @SchoolID) AND (A_T.Paid_date BETWEEN Education_Year.StartDate AND Education_Year.EndDate)
GROUP BY Education_Year.EducationYearID, Education_Year.EducationYear) AS Emp_Ex_T on Edu_Year.EducationYearID = Emp_Ex_T.EducationYearID Order By StartDate DESC">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
    </div>

</asp:Content>
