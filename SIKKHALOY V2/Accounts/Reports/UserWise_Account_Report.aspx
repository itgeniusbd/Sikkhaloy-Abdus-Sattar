<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="UserWise_Account_Report.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.UserWise_Account_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Accounts_By_User.css?v=3" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Income Expense Summary by user
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

    

    <asp:GridView ID="ExpenseGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="UserInExSQL" AllowSorting="True" AllowPaging="True" PageSize="30">
    <Columns>
        <asp:BoundField DataField="Name" HeaderText="Name" ReadOnly="True" SortExpression="Name" />
        <asp:BoundField DataField="Designation" HeaderText="Designation" ReadOnly="True" SortExpression="Designation" />
        <asp:BoundField DataField="Income" HeaderText="Income" ReadOnly="True" SortExpression="Income" DataFormatString="{0:N0}" />
        <asp:BoundField DataField="Expense" HeaderText="Expense" ReadOnly="True" SortExpression="Expense" />
    </Columns>
    <PagerStyle CssClass="pgr" />
</asp:GridView>

    <asp:SqlDataSource ID="UserInExSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT RegistrationID, AdminID, Designation, Name, Income, Expense FROM (SELECT User_T.RegistrationID, User_T.AdminID, User_T.Designation, User_T.Name, ISNULL(EX_In_T.Other_Income, 0) + ISNULL(Stu_P_T.Student_Income, 0) + ISNULL(Com_In_T.CommitteeDonation, 0) AS Income, ISNULL(Ex_T.Expenditure, 0) + ISNULL(Emp_P_T.Employee_Paid, 0) AS Expense FROM (SELECT Admin.RegistrationID, Admin.Designation, Admin.AdminID, ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + '(' + r.UserName + ')' AS Name FROM Admin Inner join Registration r on Admin.RegistrationID=r.RegistrationID WHERE (Admin.SchoolID = @SchoolID) ) AS User_T LEFT OUTER JOIN (SELECT RegistrationID, ISNULL(SUM(Extra_IncomeAmount), 0) AS Other_Income FROM Extra_Income WHERE (SchoolID = @SchoolID) AND (Extra_IncomeDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationID) AS EX_In_T ON User_T.RegistrationID = EX_In_T.RegistrationID LEFT OUTER JOIN (SELECT RegistrationId, ISNULL(SUM(TotalAmount), 0) AS CommitteeDonation FROM CommitteeMoneyReceipt WHERE (SchoolId = @SchoolID) AND (CAST(PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationId) AS Com_In_T ON User_T.RegistrationID = Com_In_T.RegistrationId LEFT OUTER JOIN (SELECT RegistrationID, ISNULL(SUM(PaidAmount), 0) AS Student_Income FROM Income_PaymentRecord WHERE (SchoolID = @SchoolID) AND (CAST(PaidDate AS Date) BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationID) AS Stu_P_T ON User_T.RegistrationID = Stu_P_T.RegistrationID LEFT OUTER JOIN (SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Expenditure FROM Expenditure WHERE (SchoolID = @SchoolID) AND (ExpenseDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationID) AS Ex_T ON User_T.RegistrationID = Ex_T.RegistrationID LEFT OUTER JOIN (SELECT RegistrationID, ISNULL(SUM(Amount), 0) AS Employee_Paid FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID) AND (Paid_date BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) GROUP BY RegistrationID) AS Emp_P_T ON User_T.RegistrationID = Emp_P_T.RegistrationID) AS T"
        CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:QueryStringParameter Name="RegistrationID" QueryStringField="RegID" />
            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
        </SelectParameters>
    </asp:SqlDataSource>
    <div class="clearfix"></div>


   

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