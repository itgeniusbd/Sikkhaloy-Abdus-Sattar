<%@ Page Title="Income Details" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Income.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.Income" %>

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
            <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="To Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="CategoryDropDownList" runat="server" AppendDataBoundItems="True" DataSourceID="CategorySQL" DataTextField="Category" DataValueField="Category" CssClass="form-control" AutoPostBack="True">
                <asp:ListItem Value="%">ALL CATEGORY</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category from(
SELECT Income_Roles.Role AS Category
FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID)
Union 
SELECT CommitteeDonationCategory.DonationCategory AS Category FROM CommitteeMoneyReceipt 
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId INNER JOIN CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId 
WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolID)
Union
SELECT Extra_IncomeCategory.Extra_Income_CategoryName AS Category
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID)
GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName)as t GROUP BY Category order by Category">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>

        </div>
        <div class="form-group">
            <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar"></i>
        </div>
        <div class="form-group">
            <asp:Button ID="Submit_Button" CssClass="btn btn-primary" runat="server" Text="Submit" />
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
                INCOME: <%#Eval("Total_Revenue","{0:N0}") %> TK</h3>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TotalSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ISNULL(Other_Income,0) + ISNULL(Studnet_Paid,0)+ISNULL(Committee_Paid,0) as Total_Revenue from ( SELECT 
(SELECT SUM(Income_PaymentRecord.PaidAmount) FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID WHERE (Income_PaymentRecord.SchoolID = @SchoolID) and Income_Roles.Role LIKE @Category and cast(Income_PaymentRecord.PaidDate as Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))AS Other_Income,
(SELECT SUM(Extra_Income.Extra_IncomeAmount) FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID WHERE (Extra_Income.SchoolID = @SchoolID) and Extra_IncomeCategory.Extra_Income_CategoryName LIKE @Category and Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))AS Studnet_Paid,
(SELECT  SUM(CommitteePaymentRecord.PaidAmount) FROM CommitteeMoneyReceipt INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId 
INNER JOIN CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId 
ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId 
WHERE(CommitteeMoneyReceipt.SchoolId = @SchoolID) AND CommitteeDonationCategory.DonationCategory LIKE @Category AND (CAST(CommitteeMoneyReceipt.PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))) as Committee_Paid) as t">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="CategoryDropDownList" Name="Category" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:Repeater ID="CategoryRepeater" runat="server" DataSourceID="IncomeCategorySQL">
        <ItemTemplate>
            <div class="table-responsive mb-4">
                <div class="pull-left">
                    <asp:Label ID="CategoryLabel" runat="server" Text='<%# Eval("Category") %>' />
                </div>
                <div class="pull-right">
                    ৳<%# Eval("Income","{0:N0}") %></div>
                <asp:GridView ID="DetailsGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="DetailsSQL" AllowSorting="True" AllowPaging="True" PageSize="50">
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
                <asp:SqlDataSource ID="DetailsSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT 
ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, 
ISNULL(Account.AccountName, 'N/A') AS AccountName,Income_Roles.Role as Category, 
'['+Student.ID+'] ' + Student.StudentsName + ', Class: ' + CreateClass.Class + ', For: ' + Income_PaymentRecord.PayFor AS Details,
Income_PaymentRecord.PaidAmount AS Amount, 
cast(Income_PaymentRecord.PaidDate as Date) as [Date] 
FROM Admin RIGHT OUTER JOIN Income_PaymentRecord INNER JOIN
                         Registration ON Income_PaymentRecord.RegistrationID = Registration.RegistrationID INNER JOIN
                         Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID INNER JOIN
                         Student ON Income_PaymentRecord.StudentID = Student.StudentID INNER JOIN
                         StudentsClass INNER JOIN
                         CreateClass ON StudentsClass.ClassID = CreateClass.ClassID ON Income_PaymentRecord.StudentClassID = StudentsClass.StudentClassID ON 
                         Admin.RegistrationID = Registration.RegistrationID LEFT OUTER JOIN
                         Account ON Income_PaymentRecord.AccountID = Account.AccountID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) and cast(Income_PaymentRecord.PaidDate as Date)   BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND Income_Roles.Role = @Category
Union ALL 
SELECT 
ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, 
ISNULL(Account.AccountName, 'N/A') AS AccountName, 
Extra_IncomeCategory.Extra_Income_CategoryName AS Category, 
Extra_Income.Extra_IncomeFor AS Details, 
Extra_Income.Extra_IncomeAmount AS Amount, 
Extra_Income.Extra_IncomeDate AS [Date] 
FROM Extra_Income INNER JOIN
                         Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID INNER JOIN
                         Registration ON Extra_Income.RegistrationID = Registration.RegistrationID LEFT OUTER JOIN 
                         Admin ON Registration.RegistrationID = Admin.RegistrationID LEFT OUTER JOIN
                         Account ON Extra_Income.AccountID = Account.AccountID WHERE(Extra_Income.SchoolID = @SchoolID) and Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND Extra_IncomeCategory.Extra_Income_CategoryName = @Category 
Union ALL 
SELECT 
ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + Registration.UserName + ')' AS UserName, 
ISNULL(Account.AccountName, 'N/A') AS AccountName, 
   CommitteeDonationCategory.DonationCategory  AS Category, 
CommitteeDonation.Description AS Details, 
CommitteePaymentRecord.PaidAmount AS Amount, 
cast(CommitteeMoneyReceipt.PaidDate as Date) AS [Date] 
FROM            CommitteeMoneyReceipt INNER JOIN
                         CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId INNER JOIN
                         CommitteeDonation INNER JOIN
                         CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId ON 
                         CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId INNER JOIN
                         Registration ON CommitteeMoneyReceipt.RegistrationId = Registration.RegistrationID LEFT OUTER JOIN 
                         Admin ON Registration.RegistrationID = Admin.RegistrationID LEFT OUTER JOIN 
                         Account ON CommitteeMoneyReceipt.AccountId = Account.AccountID WHERE(CommitteeMoneyReceipt.SchoolID = @SchoolID) and cast(CommitteeMoneyReceipt.PaidDate as Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') AND CommitteeDonationCategory.DonationCategory = @Category order by [Date]">
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
    <asp:SqlDataSource ID="IncomeCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Category, SUM(Income) AS Income from(SELECT Income_Roles.Role AS Category, SUM(Income_PaymentRecord.PaidAmount) AS Income FROM Income_PaymentRecord INNER JOIN Income_Roles ON Income_PaymentRecord.RoleID = Income_Roles.RoleID
WHERE (Income_PaymentRecord.SchoolID = @SchoolID) and cast(Income_PaymentRecord.PaidDate as Date)   BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY Income_Roles.Role
Union ALL
SELECT CommitteeDonationCategory.DonationCategory AS Category, SUM(CommitteePaymentRecord.PaidAmount) AS Income FROM CommitteeMoneyReceipt 
INNER JOIN CommitteePaymentRecord ON CommitteeMoneyReceipt.CommitteeMoneyReceiptId = CommitteePaymentRecord.CommitteeMoneyReceiptId INNER JOIN CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId ON CommitteePaymentRecord.CommitteeDonationId = CommitteeDonation.CommitteeDonationId 
WHERE (CommitteeMoneyReceipt.SchoolId = @SchoolID) AND (CAST(CommitteeMoneyReceipt.PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY CommitteeDonationCategory.DonationCategory
Union ALL
SELECT Extra_IncomeCategory.Extra_Income_CategoryName AS Category , SUM(Extra_Income.Extra_IncomeAmount) AS Income
FROM Extra_Income INNER JOIN Extra_IncomeCategory ON Extra_Income.Extra_IncomeCategoryID = Extra_IncomeCategory.Extra_IncomeCategoryID
WHERE (Extra_Income.SchoolID = @SchoolID) and Extra_Income.Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000') GROUP BY Extra_IncomeCategory.Extra_Income_CategoryName)as t Where Category LIKE @Category  GROUP  BY Category"
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
                autoclose: true,
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

                $("[id*=Submit_Button]").trigger("click");
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
