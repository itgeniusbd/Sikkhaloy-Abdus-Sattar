<%@ Page Title="Accounts by user" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="UserAccount.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.UserAccount" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Accounts_By_User.css?v=3" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Accounts details by user
      <small class="Date"></small>
    </h3>
    <div class="form-inline NoPrint">
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

    <asp:FormView ID="IncomeFormView" runat="server" DataSourceID="UserInExSQL" Width="100%">
        <ItemTemplate>
            <div class="row">
                <div class="col-md-4">
                    <div class="user-grid user-bg">
                        <div class="user-imge">
                            <img alt="No Image" src="/Handeler/Admin_Photo.ashx?Img=<%#Eval("AdminID") %>" class="img-circle img-responsive" />
                        </div>
                        <div class="headline"><%#Eval("Name") %></div>
                        <div class="value"><%#Eval("Designation") %></div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="user-grid income-bg">
                        <i class="fa fa-money"></i>
                        <div class="headline">INCOME</div>
                        <div class="value"><%#Eval("Income","{0:N0}") %> TK</div>
                    </div>
                </div>

                <div class="col-md-4">
                    <div class="user-grid expense-bg">
                        <i class="fa fa-money"></i>
                        <div class="headline">EXPENSE</div>
                        <div class="value"><%#Eval("Expense","{0:N0}") %> TK</div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="UserInExSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT RegistrationID, AdminID, Designation, Name, Income, Expense FROM (SELECT User_T.RegistrationID, User_T.AdminID, User_T.Designation, User_T.Name, ISNULL(EX_In_T.Other_Income, 0) + ISNULL(Stu_P_T.Student_Income, 0) + ISNULL(Com_In_T.CommitteeDonation, 0) AS Income, ISNULL(Ex_T.Expenditure, 0) + ISNULL(Emp_P_T.Employee_Paid, 0) AS Expense FROM (SELECT Registration.RegistrationID, Admin.Designation, Admin.AdminID, ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS Name FROM Registration LEFT OUTER JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID WHERE (Registration.SchoolID = @SchoolID) AND (Registration.RegistrationID = @RegistrationID)) AS User_T LEFT OUTER JOIN (SELECT RegistrationID, ISNULL(SUM(Extra_IncomeAmount), 0) AS Other_Income FROM Extra_Income WHERE (SchoolID = @SchoolID) AND (Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationID) AS EX_In_T ON User_T.RegistrationID = EX_In_T.RegistrationID LEFT OUTER JOIN (SELECT RegistrationId, ISNULL(SUM(TotalAmount), 0) AS CommitteeDonation FROM CommitteeMoneyReceipt WHERE (SchoolId = @SchoolID) AND (CAST(PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationId) AS Com_In_T ON User_T.RegistrationID = Com_In_T.RegistrationId LEFT OUTER JOIN (SELECT RegistrationID, ISNULL(SUM(PaidAmount), 0) AS Student_Income FROM Income_PaymentRecord WHERE (SchoolID = @SchoolID) AND (CAST(PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationID) AS Stu_P_T ON User_T.RegistrationID = Stu_P_T.RegistrationID LEFT OUTER JOIN (SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Expenditure FROM Expenditure WHERE (SchoolID = @SchoolID) AND (ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationID) AS Ex_T ON User_T.RegistrationID = Ex_T.RegistrationID LEFT OUTER JOIN (SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Employee_Paid FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID) AND (Paid_date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationID) AS Emp_P_T ON User_T.RegistrationID = Emp_P_T.RegistrationID) AS T"
        CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:QueryStringParameter Name="RegistrationID" QueryStringField="RegID" />
            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
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
                        <asp:GridView ID="IncomeGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="DetailsSQL" AllowSorting="True" AllowPaging="True" PageSize="30">
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
FROM  Income_PaymentRecord INNER JOIN
                         Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN
                         Student ON Income_PaymentRecord.StudentID = Student.StudentID INNER JOIN
                         StudentsClass  ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID INNER JOIN 
                         CreateClass ON StudentsClass.ClassID = CreateClass.ClassID LEFT OUTER JOIN
                         Account ON Income_PaymentRecord.AccountID = Account.AccountID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND Income_PaymentRecord.RegistrationID = @RegistrationID  and cast(Income_PaymentRecord.PaidDate as Date)   BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND Income_Roles.Role = @Category
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
WHERE(Extra_Income.SchoolID = @SchoolID) AND Extra_Income.RegistrationID = @RegistrationID  and Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND Extra_IncomeCategory.Extra_Income_CategoryName = @Category 

Union 
SELECT 
ISNULL(Account.AccountName, 'N/A') AS AccountName, 
CommitteeDonationCategory.DonationCategory  AS Category, 
CommitteeDonation.Description AS Details, 
CommitteePaymentRecord.PaidAmount AS Amount, 
CommitteeMoneyReceipt.PaidDate AS [Date] 
FROM            CommitteeMoneyReceipt INNER JOIN
                         CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId INNER JOIN
                         CommitteeDonation INNER JOIN
                         CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId ON 
                         CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId LEFT OUTER JOIN 
                         Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID WHERE(CommitteeMoneyReceipt.SchoolID = @SchoolID) AND CommitteeMoneyReceipt.RegistrationID = @RegistrationID AND cast(CommitteeMoneyReceipt.PaidDate as Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND CommitteeDonationCategory.DonationCategory = @Category
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
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) AND Income_PaymentRecord.RegistrationID = @RegistrationID and cast(Income_PaymentRecord.PaidDate as Date)   BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY Income_Roles.Role
Union
SELECT CommitteeDonationCategory.DonationCategory AS Category, SUM(CommitteePaymentRecord.PaidAmount) AS Income FROM CommitteeMoneyReceipt 
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId 
INNER JOIN CommitteeDonation 
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId 
ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId 
WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolID) AND CommitteeMoneyReceipt.RegistrationID = @RegistrationID  AND (CAST(CommitteeMoneyReceipt.PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY CommitteeDonationCategory.DonationCategory

Union 
SELECT Extra_IncomeCategory.Extra_Income_CategoryName AS Category , SUM(Extra_Income.Extra_IncomeAmount) AS Income
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID) AND Extra_Income.RegistrationID = @RegistrationID and Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName)as t   GROUP  BY Category
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
                        <asp:GridView ID="ExpenseGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="DetailsSQL" AllowSorting="True" AllowPaging="True" PageSize="30">
                            <Columns>
                                <asp:BoundField DataField="AccountName" HeaderText="Account" ReadOnly="True" SortExpression="AccountName" />
                                <asp:BoundField DataField="Details" HeaderText="Details" ReadOnly="True" SortExpression="Details" />
                                <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" DataFormatString="{0:N0}" />
                                <asp:BoundField DataField="Date" HeaderText="Date" ReadOnly="True" SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="DetailsSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        ISNULL(Account.AccountName, 'N/A') AS AccountName,Expense_CategoryName.CategoryName as Category, 
                        Expenditure.ExpenseFor AS Details, Expenditure.Amount, Expenditure.ExpenseDate as [Date]
FROM   Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID= Expense_CategoryName.ExpenseCategoryID  LEFT OUTER JOIN
                         Account ON Expenditure.AccountID = Account.AccountID
WHERE        (Expenditure.SchoolID = @SchoolID)AND Expenditure.RegistrationID = @RegistrationID   and Expenditure.ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND Expense_CategoryName.CategoryName = @Category
Union
SELECT        ISNULL(Account.AccountName, 'N/A') AS AccountName,Employee_Payorder_Name.Payorder_Name AS Category, Employee_Payorder_Records.Paid_For AS Details, Employee_Payorder_Records.Amount, Employee_Payorder_Records.Paid_date AS [Date]
FROM          Employee_Payorder_Records INNER JOIN
                         Employee_Payorder ON Employee_Payorder.Employee_PayorderID = Employee_Payorder_Records.Employee_PayorderID INNER JOIN
                         Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID LEFT OUTER JOIN
                         Account ON Employee_Payorder_Records.AccountID = Account.AccountID
WHERE        (Employee_Payorder_Records.SchoolID = @SchoolID) AND Employee_Payorder_Records.RegistrationID = @RegistrationID AND (Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From_Date, N'1-1-1000') AND ISNULL(@To_Date, N'1-1-3000')) AND Employee_Payorder_Name.Payorder_Name = @Category
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
FROM            Employee_Payorder_Records INNER JOIN
                         Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN
                         Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
WHERE        (Employee_Payorder_Records.SchoolID = @SchoolID) AND Employee_Payorder_Records.RegistrationID = @RegistrationID AND (Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From_Date, N'1-1-1000') AND ISNULL(@To_Date, N'1-1-3000'))  
GROUP BY Employee_Payorder_Name.Payorder_Name
Union 
SELECT      Expense_CategoryName.CategoryName AS Category , SUM(Expenditure.Amount) AS Amount
FROM            Expenditure INNER JOIN
                         Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
WHERE        (Expenditure.SchoolID = @SchoolID) AND Expenditure.RegistrationID = @RegistrationID and Expenditure.ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')
GROUP BY Expense_CategoryName.CategoryName)as t  GROUP  BY Category"
                CancelSelectOnNullParameter="False">
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
        $(function () {
            if (!$('[id*=IncomeGridView] tr').length) {
                $('#Income_gv').hide().removeClass('col-md-6');
                $('#Expense_gv').removeClass('col-md-6').addClass('col-md-12');
            }

            if (!$('[id*=ExpenseGridView] tr').length) {
                $('#Expense_gv').hide().removeClass('col-md-6');
                $('#Income_gv').removeClass('col-md-6').addClass('col-md-12');
            }

            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });


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
