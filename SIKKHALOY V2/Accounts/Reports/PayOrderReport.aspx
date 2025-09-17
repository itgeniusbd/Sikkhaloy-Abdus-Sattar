<%@ Page Title="Payorder Report" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="PayOrderReport.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.PayOrderReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Payorder_Report.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>All session Payorder Report
      <small class="Date"></small>
    </h3>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="Find" />
                </div>
                <div class="form-group d-print-none">
                    <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
                </div>
            </div>

            <asp:FormView ID="Stu_PD_FormView" runat="server" DataSourceID="Stu_PD_SQL" Width="100%">
                <ItemTemplate>
                    <div class="row mb-3">
                        <div class="col-lg-6">
                            <div class="row no-gutters">
                                <div class="col-md-4 col-sm-4">
                                    <div class="Payorder-bg">
                                        <div class="Fees-section">
                                            <small>Payorder</small>
                                            <h2><%# Eval("TotalFee","{0:N0}") %> Tk</h2>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-4 col-sm-4">
                                    <div class="Latefee-bg">
                                        <div class="Fees-section">
                                            <small>Late Fee</small>
                                            <h2><%# Eval("TotalLateFee","{0:N0}") %> Tk</h2>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-4 col-sm-4">
                                    <div class="Concession-bg">
                                        <div class="Fees-section">
                                            <small>Concession</small>
                                            <h2><%# Eval("TotalDiscount","{0:N0}") %> Tk</h2>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-6">
                            <div class="row">
                                <div class="col-md-6 col-sm-6">
                                    <div class="Paid-bg">
                                        <div class="Fees-section">
                                            <small>Paid</small>
                                            <h2><%# Eval("Paid","{0:N0}") %> Tk</h2>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-md-6  col-sm-6">
                                    <div class="Unpaid-bg">
                                        <div class="Fees-section">
                                            <small>Unpaid</small>
                                            <h2><%# Eval("Unpaid","{0:N0}") %> Tk</h2>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="Stu_PD_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="
SELECT SUM(Amount) AS TotalFee, 
SUM(LateFeeCountable) AS TotalLateFee, 
SUM(Total_Discount)  AS TotalDiscount,
SUM(PaidAmount)AS Paid,
SUM(CASE WHEN Status = 'Paid' THEN 0 WHEN EndDate &lt; GETDATE() - 1 THEN ISNULL(Amount, 0) + ISNULL(LateFee, 0) - ISNULL(Discount, 0)  - ISNULL(PaidAmount, 0) - ISNULL(LateFee_Discount, 0) ELSE ISNULL(Amount, 0) - ISNULL(Discount, 0) - ISNULL(PaidAmount, 0) END) AS Unpaid
FROM Income_PayOrder
WHERE (SchoolID = @SchoolID) AND (Is_Active = 1) AND (EndDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))"
                CancelSelectOnNullParameter="False">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                    <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="table-responsive mb-4">
                <asp:GridView ID="RoleGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="RoleID" DataSourceID="RoleSQL" AllowSorting="True">
                    <Columns>
                        <asp:TemplateField HeaderText="Role" SortExpression="Role">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" CssClass="scrollToBottom" Text='<%# Bind("Role") %>' CommandArgument='<%# Bind("Role") %>' CausesValidation="False" CommandName="Select" OnCommand="LinkButton1_Command"></asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="TotalFee" HeaderText="Fee" DataFormatString="{0:N0}" SortExpression="TotalFee" />
                        <asp:BoundField DataField="TotalLateFee" HeaderText="Late Fee" DataFormatString="{0:N0}" SortExpression="TotalLateFee" />
                        <asp:BoundField DataField="TotalDiscount" HeaderText="Consession" DataFormatString="{0:N0}" SortExpression="TotalDiscount" />
                        <asp:BoundField DataField="Paid" HeaderText="Paid" DataFormatString="{0:N0}" SortExpression="Paid" />
                        <asp:BoundField DataField="Unpaid" HeaderText="Unpaid" DataFormatString="{0:N0}" SortExpression="Unpaid" />
                    </Columns>
                    <SelectedRowStyle CssClass="Selected" />
                </asp:GridView>
                <asp:SqlDataSource ID="RoleSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Income_Roles.RoleID,
 Income_Roles.Role, 
 SUM(Income_PayOrder.Amount) AS TotalFee, 
 SUM(LateFeeCountable) AS TotalLateFee,
 SUM(Total_Discount) AS TotalDiscount,
 SUM(Income_PayOrder.PaidAmount) AS Paid, 
 SUM(Receivable_Amount) AS Unpaid
FROM Income_PayOrder INNER JOIN
  Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID
WHERE  (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.Is_Active = 1) AND (EndDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000'))
GROUP BY Income_Roles.Role, Income_Roles.RoleID
ORDER BY Income_Roles.Role">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                        <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div id="IS_Pfor">
                <div class="grid-title For-tle-bg">
                    PAY ORDER SUMMARY OF:
                    <asp:Label ID="Role_Label" runat="server"></asp:Label>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="PayForGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="PayForSQL" AllowSorting="True">
                        <Columns>
                            <asp:BoundField DataField="PayFor" HeaderText="Pay For" SortExpression="PayFor" />
                            <asp:BoundField DataField="TotalFee" DataFormatString="{0:N0}" HeaderText="Fee" SortExpression="TotalFee" />
                            <asp:BoundField DataField="TotalLateFee" DataFormatString="{0:N0}" HeaderText="Late Fee" SortExpression="TotalLateFee" />
                            <asp:BoundField DataField="TotalDiscount" DataFormatString="{0:N0}" HeaderText="Consession" SortExpression="TotalDiscount" />
                            <asp:BoundField DataField="Paid" HeaderText="Paid" DataFormatString="{0:N0}" SortExpression="Paid" />
                            <asp:BoundField DataField="Unpaid" HeaderText="Unpaid" DataFormatString="{0:N0}" SortExpression="Unpaid" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="PayForSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        PayFor, 
        SUM(Amount) AS TotalFee, 
        SUM(LateFeeCountable) AS TotalLateFee, 
        SUM(Total_Discount) AS TotalDiscount, 
		SUM(PaidAmount) AS Paid,
		SUM(Receivable_Amount) AS Unpaid
FROM            Income_PayOrder
WHERE        (SchoolID = @SchoolID) AND (Is_Active = 1) AND (EndDate BETWEEN ISNULL(@From_Date, '1-1-1000') AND ISNULL(@To_Date, '1-1-3000')) AND (RoleID = @RoleID) 
GROUP BY PayFor ORDER by max(EndDate)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="From_Date_TextBox" Name="From_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="To_Date_TextBox" Name="To_Date" PropertyName="Text" />
                            <asp:ControlParameter ControlID="RoleGridView" Name="RoleID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="/CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>


    <script>
        $(function () {
            if (!$('[id*=PayForGridView] tr').length) {
                $('#IS_Pfor').hide();
            }

            //scroll To Bottom
            $('.scrollToBottom').bind("click", function () {
                $('html, body').animate({ scrollTop: $(document).height() }, 1200);
            });


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
        });

        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            if (!$('[id*=PayForGridView] tr').length) {
                $('#IS_Pfor').hide();
            }

            //scroll To Bottom
            $('.scrollToBottom').bind("click", function () {
                $('html, body').animate({ scrollTop: $(document).height() }, 1200);
            });


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
        });


        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
