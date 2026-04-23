<%@ Page Title="SMS Recharge" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="SMS_Recharge.aspx.cs" Inherits="EDUCATION.COM.SMS.SMS_Recharge" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .balance-card {
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            color: white;
            border-radius: 10px;
            padding: 20px;
            margin-bottom: 20px;
        }
        .balance-card h2 { margin: 0; font-size: 2.5rem; font-weight: bold; }
        .balance-card p  { margin: 0; opacity: 0.9; }
        .recharge-form-card {
            border: 1px solid #dee2e6;
            border-radius: 10px;
            padding: 25px;
            background: #f8f9fa;
            margin-bottom: 20px;
        }
        .badge-paid    { background-color: #28a745; color: white; padding: 4px 10px; border-radius: 4px; font-size: 12px; }
        .badge-unpaid  { background-color: #ffc107; color: #000;   padding: 4px 10px; border-radius: 4px; font-size: 12px; }
        .info-box {
            background: #e8f4f8;
            border-left: 4px solid #17a2b8;
            padding: 12px 16px;
            border-radius: 4px;
            margin-bottom: 15px;
        }
    </style>
    <script type="text/javascript" src="/JS/sms-recharge.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3><i class="fa fa-mobile"></i> SMS রিচার্জ</h3>

    <div class="info-box">
        <i class="fa fa-info-circle"></i>
        <strong>তথ্য:</strong> রিচার্জের সময় পেমেন্ট করতে হবে — কোনো বাকি থাকবে না।
        <br /><strong>রেট: ০.৩৬ টাকা প্রতি SMS</strong>
    </div>

    <!-- Current Balance -->
    <asp:FormView ID="SMSBalanceFormView" runat="server" DataSourceID="SMSBalanceSQL" Width="100%">
        <ItemTemplate>
            <div class="balance-card">
                <p>বর্তমান SMS ব্যালেন্স</p>
                <h2><%# Eval("SMS_Balance") %></h2>
            </div>
        </ItemTemplate>
        <EmptyDataTemplate>
            <div class="balance-card">
                <p>বর্তমান SMS ব্যালেন্স</p>
                <h2>0</h2>
            </div>
        </EmptyDataTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="SMSBalanceSQL" runat="server"
        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT SMS_Balance FROM SMS WHERE SchoolID = @SchoolID">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <!-- Recharge Request Form -->
    <div class="recharge-form-card">
        <h5><i class="fa fa-plus-circle"></i> SMS রিচার্জ করুন</h5>
        <hr />
        <div class="form-inline">
            <div class="form-group mr-2">
                <asp:TextBox ID="SMSQtyTextBox" runat="server" CssClass="form-control"
                    placeholder="SMS পরিমাণ লিখুন"
                    onkeypress="return isNumberKey(event)" autocomplete="off"></asp:TextBox>
                <asp:RequiredFieldValidator runat="server" ControlToValidate="SMSQtyTextBox"
                    CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="R"></asp:RequiredFieldValidator>
            </div>
            <div class="form-group mr-2">
                <asp:Label ID="TotalCostLabel" runat="server" CssClass="badge badge-info" style="font-size:14px;"></asp:Label>
            </div>
            <div class="form-group mr-2">
                <asp:Button ID="RechargeButton" runat="server" CssClass="btn btn-warning font-weight-bold"
                    Text="রিচার্জ ও ShurjoPay পেমেন্ট" OnClick="RechargeButton_Click" ValidationGroup="R"
                    OnClientClick="return confirmRecharge(this);" />
            </div>
            <asp:Label ID="MessageLabel" runat="server"></asp:Label>
        </div>
        <small class="text-muted mt-2 d-block">
            * রেট: <strong>০.৩৬ টাকা</strong> প্রতি SMS। ShurjoPay-এ পেমেন্ট সম্পন্ন হলে রিচার্জ সত্যি হবে।
        </small>
    </div>

    <!-- Recharge History -->
    <h5><i class="fa fa-history"></i> রিচার্জ ইতিহাস</h5>
    <div class="table-responsive">
        <asp:GridView ID="RechargeGridView" runat="server" AutoGenerateColumns="False"
            DataSourceID="RechargeHistorySQL" CssClass="mGrid" AllowPaging="True" PageSize="15">
            <Columns>
                <asp:BoundField DataField="RechargeSMS"   HeaderText="SMS পরিমাণ"  SortExpression="RechargeSMS" />
                <asp:BoundField DataField="PerSMS_Price"  HeaderText="প্রতি SMS মূল্য" SortExpression="PerSMS_Price" />
                <asp:BoundField DataField="Total_Price"   HeaderText="মোট মূল্য"   SortExpression="Total_Price" />
                <asp:BoundField DataField="Date"          HeaderText="তারিখ"        SortExpression="Date" DataFormatString="{0:d MMM yyyy}" />
                <asp:BoundField DataField="UserName"      HeaderText="রিচার্জকারী"  SortExpression="UserName" />
                <asp:TemplateField HeaderText="পেমেন্ট স্ট্যাটাস">
                    <ItemTemplate>
                        <%#
                            Eval("Is_Paid") != DBNull.Value && Convert.ToBoolean(Eval("Is_Paid"))
                            ? "<span class='badge-paid'>পরিশোধিত</span>"
                            : "<span class='badge-unpaid'>বকেয়া</span>"
                        %>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
        <asp:SqlDataSource ID="RechargeHistorySQL" runat="server"
            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT TOP 100 r.SMS_Recharge_RecordID, r.RechargeSMS, r.PerSMS_Price, r.Total_Price, r.Date, r.Is_Paid,
                                  reg.UserName
                           FROM SMS_Recharge_Record r
                           LEFT JOIN Registration reg ON reg.RegistrationID = r.RegistrationID
                           WHERE r.SchoolID = @SchoolID
                           ORDER BY r.Date DESC">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <!-- Due Invoices for this school -->
    <asp:Panel ID="DueInvoicePanel" runat="server" CssClass="mt-4" Visible="false">
        <h5><i class="fa fa-file-text text-warning"></i> SMS সংক্রান্ত বকেয়া ইনভয়েস</h5>
        <div class="table-responsive">
            <asp:GridView ID="DueInvoiceGridView" runat="server" AutoGenerateColumns="False"
                DataSourceID="DueInvoiceSQL" CssClass="mGrid" EmptyDataText="কোনো বকেয়া ইনভয়েস নেই।">
                <Columns>
                    <asp:BoundField DataField="Invoice_SN"     HeaderText="ইনভয়েস নং"  SortExpression="Invoice_SN" />
                    <asp:BoundField DataField="Invoice_For"    HeaderText="বিবরণ"        SortExpression="Invoice_For" />
                    <asp:BoundField DataField="Unit"           HeaderText="SMS পরিমাণ"  SortExpression="Unit" />
                    <asp:BoundField DataField="UnitPrice"      HeaderText="প্রতি SMS"    SortExpression="UnitPrice" />
                    <asp:BoundField DataField="TotalAmount"    HeaderText="মোট"          SortExpression="TotalAmount" />
                    <asp:BoundField DataField="PaidAmount"     HeaderText="পরিশোধিত"    SortExpression="PaidAmount" />
                    <asp:BoundField DataField="Due"            HeaderText="বকেয়া"       SortExpression="Due" />
                    <asp:BoundField DataField="IssuDate"       HeaderText="ইস্যু তারিখ"  DataFormatString="{0:d MMM yyyy}" />
                    <asp:BoundField DataField="EndDate"        HeaderText="শেষ তারিখ"   DataFormatString="{0:d MMM yyyy}" />
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="DueInvoiceSQL" runat="server"
                ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT i.Invoice_SN, i.Invoice_For, i.Unit, i.UnitPrice, i.TotalAmount, i.PaidAmount,
                                      (i.TotalAmount - i.PaidAmount - ISNULL(i.Discount,0)) AS Due, i.IssuDate, i.EndDate
                               FROM AAP_Invoice i
                               INNER JOIN AAP_Invoice_Category c ON i.InvoiceCategoryID = c.InvoiceCategoryID
                               WHERE i.SchoolID = @SchoolID AND i.IsPaid = 0 AND c.InvoiceCategory = N'SMS'
                               ORDER BY i.IssuDate DESC">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <p class="text-muted mt-1">
            <i class="fa fa-arrow-right"></i>
            বিস্তারিত ইনভয়েস দেখতে <a href="/Profile/Invoice/Due_Invoice.aspx">এখানে ক্লিক করুন</a>।
        </p>
    </asp:Panel>

    <script>
        function isNumberKey(e) {
            var c = e.which ? e.which : event.keyCode;
            return !(c > 31 && (c < 48 || c > 57));
        }
    </script>
</asp:Content>
