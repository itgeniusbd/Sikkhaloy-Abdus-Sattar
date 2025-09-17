<%@ Page Title="Expense Details" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Expense.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.Expense" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
      <style>
        .range_inputs { display: none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="CategoryDropDownList" runat="server" AppendDataBoundItems="True" DataSourceID="CategorySQL" DataTextField="Category" DataValueField="Category" CssClass="form-control" AutoPostBack="True">
                <asp:ListItem Value="%">ALL CATEGORY</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Amount) AS Total from(SELECT Employee_Payorder_Name.Payorder_Name AS Category, SUM(Employee_Payorder_Records.Amount) AS Amount
FROM            Employee_Payorder_Records INNER JOIN
                         Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN
                         Employee_Payorder_Name ON Employee_Payorder.Employee_Payorder_NameID = Employee_Payorder_Name.Employee_Payorder_NameID
WHERE        (Employee_Payorder_Records.SchoolID = @SchoolID)
GROUP BY Employee_Payorder_Name.Payorder_Name
Union 
SELECT      Expense_CategoryName.CategoryName AS Category , SUM(Expenditure.Amount) AS Amount
FROM            Expenditure INNER JOIN
                         Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID
WHERE        (Expenditure.SchoolID = @SchoolID)
GROUP BY Expense_CategoryName.CategoryName)as t GROUP BY Category">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>

        </div>
        <div class="form-group">
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="Submit" />
        </div>
        <div class="form-group pull-right Print">
            <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
        </div>
    </div>

    <asp:FormView ID="TotalFormView" runat="server" DataSourceID="TotalSQL" Width="100%">
        <ItemTemplate>
            <a href="Final_Reports.aspx" class="NoPrint">
                <i class="fa fa-hand-o-left" aria-hidden="true"></i>
                Back to Accounts Summary</a>
            <h3>
                <label class="Date"></label>
                EXPENSE: <%#Eval("Total_Expense","{0:N0}") %> TK</h3>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TotalSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="select ISNULL(Ex,0) + ISNULL(Emp_ex,0) as Total_Expense from ( SELECT 
(SELECT SUM(Expenditure.Amount) FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID WHERE (Expenditure.SchoolID = @SchoolID) and Expense_CategoryName.CategoryName LIKE @Category and Expenditure.ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))AS Ex,
(SELECT SUM(Employee_Payorder_Records.Amount) FROM Employee_Payorder_Records INNER JOIN Employee_Payorder ON Employee_Payorder_Records.Employee_PayorderID = Employee_Payorder.Employee_PayorderID INNER JOIN Employee_Payorder_Name ON Employee_Payorder_Name.Employee_Payorder_NameID = Employee_Payorder.Employee_Payorder_NameID WHERE (Employee_Payorder_Records.SchoolID = @SchoolID) and Employee_Payorder_Name.Payorder_Name LIKE @Category and Employee_Payorder_Records.Paid_date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))AS Emp_ex) as t">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="CategoryDropDownList" Name="Category" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:Repeater ID="CategoryRepeater" runat="server" DataSourceID="ExpenseCategorySQL">
        <ItemTemplate>
            <div class="table-responsive mb-4">
                <div class="pull-left">
                    <asp:Label ID="CategoryLabel" runat="server" Text='<%# Eval("Category") %>' />
                </div>
                <div class="pull-right">
                    ৳<%# Eval("Total","{0:N0}") %>
                        TK
                </div>

                <asp:GridView ID="DetailsGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="DetailsSQL" AllowSorting="True" AllowPaging="True" PageSize="50">
                    <Columns>
                        <asp:BoundField DataField="UserName" HeaderText="User Name" ReadOnly="True" SortExpression="UserName" />
                        <asp:BoundField DataField="AccountName" HeaderText="Account" ReadOnly="True" SortExpression="AccountName" />
                        <asp:BoundField DataField="Details" HeaderText="Details" ReadOnly="True" SortExpression="Details" />
                        <asp:BoundField DataField="Amount" HeaderText="Amount" ReadOnly="True" SortExpression="Amount" DataFormatString="{0:N0}" />
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
Union
SELECT        ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, ISNULL(Account.AccountName, 'N/A') AS AccountName, 
                         Employee_Payorder_Name.Payorder_Name AS Category, Employee_Payorder_Records.Paid_For AS Details, Employee_Payorder_Records.Amount, Employee_Payorder_Records.Paid_date AS [Date]
FROM            Registration INNER JOIN
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
GROUP BY Expense_CategoryName.CategoryName)as t Where Category LIKE @Category  GROUP  BY Category"
        CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="CategoryDropDownList" DefaultValue="" Name="Category" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
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

