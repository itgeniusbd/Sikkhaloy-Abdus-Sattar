<%@ Page Title="Paid Collection Report" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Paid_Institutions_Report.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.Paid_Institutions_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Summary Cards */
        .summary-row { margin-bottom: 18px; }
        .s-card { border-radius: 8px; color: #fff; padding: 18px 12px; text-align: center; }
        .s-card h4 { font-size: 1.5rem; font-weight: 700; margin: 0 0 4px; }
        .s-card small { font-size: 0.82rem; opacity: 0.9; letter-spacing: 0.5px; }
        .s-blue  { background: linear-gradient(135deg, #0288d1 0%, #0051a2 100%); }
        .s-teal  { background: linear-gradient(135deg, #00897b 0%, #004d40 100%); }
        .s-orange{ background: linear-gradient(135deg, #ef6c00 0%, #b34700 100%); }
        .s-red   { background: linear-gradient(135deg, #c62828 0%, #7f0000 100%); }
        .s-green { background: linear-gradient(135deg, #2e7d32 0%, #1b5e20 100%); }
        .s-purple{ background: linear-gradient(135deg, #6a1b9a 0%, #38006b 100%); }
        .s-grey  { background: linear-gradient(135deg, #455a64 0%, #263238 100%); }

        /* Filter bar */
        .filter-bar { background: #f4f6f8; border: 1px solid #dee2e6; border-radius: 8px; padding: 16px 20px; margin-bottom: 18px; }
        .filter-bar label { font-size: 0.82rem; font-weight: 600; color: #555; margin-bottom: 3px; }

        /* Tabs */
        .report-tabs { border-bottom: 2px solid #00897b; margin-bottom: 0; }
        .report-tabs .nav-link { color: #555; font-weight: 600; border-radius: 6px 6px 0 0; padding: 10px 22px; }
        .report-tabs .nav-link.active { background: #00897b; color: #fff; border-color: #00897b; }

        /* Table */
        .report-table th { background: #263238; color: #fff; font-size: 0.83rem; white-space: nowrap; }
        .report-table td { font-size: 0.87rem; vertical-align: middle; }
        .report-table tr.due-row td { color: #c62828; font-weight: 600; }
        .report-table tr.paid-row td { color: #2e7d32; }
        .tab-content-card { border: 1px solid #dee2e6; border-top: none; border-radius: 0 0 8px 8px; padding: 16px; }

        /* Status badge */
        .badge-paid   { background:#2e7d32; color:#fff; padding:2px 8px; border-radius:3px; font-size:0.78rem; }
        .badge-due    { background:#c62828; color:#fff; padding:2px 8px; border-radius:3px; font-size:0.78rem; }

        /* Print */
        @media print { .d-print-none { display:none!important; } }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="d-flex align-items-center mb-3">
        <h3 class="mb-0 mr-auto">
            <i class="fa fa-check-circle text-success"></i> Paid Collection Report
        </h3>
        <a href="Payment_Collection_Report.aspx" class="btn btn-sm btn-grey d-print-none">
            <i class="fa fa-arrow-left"></i> Back
        </a>
    </div>

    <!-- Filter Bar -->
    <div class="filter-bar d-print-none">
        <div class="row">
            <div class="col-md-3 col-sm-6">
                <label><i class="fa fa-th-list"></i> Category</label>
                <asp:DropDownList ID="CategoryDropDownList" CssClass="form-control form-control-sm"
                    runat="server" DataSourceID="CategorySQL" DataTextField="InvoiceCategory"
                    DataValueField="InvoiceCategoryID" AppendDataBoundItems="True">
                    <asp:ListItem Value="%">[ ALL CATEGORY ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="CategorySQL" runat="server"
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT InvoiceCategory, InvoiceCategoryID FROM AAP_Invoice_Category ORDER BY InvoiceCategory">
                </asp:SqlDataSource>
            </div>
            <div class="col-md-3 col-sm-6">
                <label><i class="fa fa-university"></i> Institution</label>
                <asp:DropDownList ID="InstitutionDropDownList" CssClass="form-control form-control-sm SearchDDL"
                    runat="server" DataSourceID="InstitutionSQL" DataTextField="SchoolName_ID"
                    DataValueField="SchoolID" AppendDataBoundItems="True">
                    <asp:ListItem Value="0">[ ALL INSTITUTIONS ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="InstitutionSQL" runat="server"
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT SchoolInfo.SchoolID, CAST(SchoolInfo.SchoolID AS NVARCHAR) + ' - ' + SchoolInfo.SchoolName AS SchoolName_ID FROM SchoolInfo WHERE SchoolInfo.Validation = N'Valid' ORDER BY SchoolInfo.SchoolName">
                </asp:SqlDataSource>
            </div>
            <div class="col-md-2 col-sm-6">
                <label><i class="fa fa-calendar"></i> From Date (Paid)</label>
                <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control form-control-sm datepicker"
                    placeholder="dd M yyyy" autocomplete="off"></asp:TextBox>
            </div>
            <div class="col-md-2 col-sm-6">
                <label><i class="fa fa-calendar"></i> To Date (Paid)</label>
                <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control form-control-sm datepicker"
                    placeholder="dd M yyyy" autocomplete="off"></asp:TextBox>
            </div>
            <div class="col-md-2 col-sm-12 d-flex align-items-end">
                <asp:Button ID="SearchButton" runat="server" CssClass="btn btn-primary btn-sm btn-block"
                    Text="Search" OnClick="SearchButton_Click" />
            </div>
        </div>
        <asp:Label ID="ErrorLabel" runat="server" CssClass="text-danger d-block mt-1" style="font-size:0.85rem;"></asp:Label>
    </div>

    <!-- Print Header (visible only on print) -->
    <div class="d-none d-print-block mb-3">
        <h4 class="text-center">Paid Collection Report</h4>
        <p class="text-center" style="font-size:0.9rem;">
            Period: <asp:Label ID="PrintPeriodLabel" runat="server"></asp:Label>
        </p>
    </div>

    <!-- Summary Cards -->
    <asp:Panel ID="SummaryPanel" runat="server" Visible="false">

        <asp:Repeater ID="SummaryRepeater" runat="server" DataSourceID="SummarySQL">
            <HeaderTemplate>
                <div class="row summary-row">
            </HeaderTemplate>
            <ItemTemplate>
                <div class="col-6 col-md-3 mb-3">
                    <div class="s-card s-blue">
                        <h4><%# Eval("TotalInvoice") %></h4>
                        <small>TOTAL INVOICES</small>
                    </div>
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <div class="s-card s-teal">
                        <h4>৳<%# Eval("TotalBilled", "{0:N0}") %></h4>
                        <small>TOTAL BILLED</small>
                    </div>
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <div class="s-card s-green">
                        <h4>৳<%# Eval("TotalPaid", "{0:N0}") %></h4>
                        <small>COLLECTED (THIS PERIOD)</small>
                    </div>
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <div class="s-card s-orange">
                        <h4>৳<%# Eval("TotalDiscount", "{0:N0}") %></h4>
                        <small>DISCOUNT</small>
                    </div>
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <div class="s-card s-red">
                        <h4>৳<%# Eval("TotalDue", "{0:N0}") %></h4>
                        <small>STILL DUE (ALL)</small>
                    </div>
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <div class="s-card s-purple">
                        <h4><%# Eval("PaidInstitutions") %></h4>
                        <small>INSTITUTIONS FULLY PAID</small>
                    </div>
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <div class="s-card s-grey">
                        <h4><%# Eval("DueInstitutions") %></h4>
                        <small>INSTITUTIONS WITH DUE</small>
                    </div>
                </div>
                <div class="col-6 col-md-3 mb-3">
                    <div class="s-card s-teal">
                        <h4><%# Eval("CollectPct") %>%</h4>
                        <small>COLLECTED %</small>
                    </div>
                </div>
            </ItemTemplate>
            <FooterTemplate>
                </div>
            </FooterTemplate>
        </asp:Repeater>

        <asp:SqlDataSource ID="SummarySQL" runat="server"
            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="
SELECT
    COUNT(DISTINCT r.InvoiceReceiptID)   AS TotalInvoice,
    SUM(inv.TotalAmount)                 AS TotalBilled,
    SUM(pr.Amount)                       AS TotalPaid,
    SUM(inv.Discount)                    AS TotalDiscount,
    SUM(inv.Due)                         AS TotalDue,
    COUNT(DISTINCT CASE WHEN inv.IsPaid = 1 THEN inv.SchoolID END) AS PaidInstitutions,
    COUNT(DISTINCT CASE WHEN inv.IsPaid = 0 THEN inv.SchoolID END) AS DueInstitutions,
    ROUND(
        SUM(pr.Amount) * 100.0
        / NULLIF(SUM(inv.TotalAmount - inv.Discount), 0)
    , 2) AS CollectPct
FROM AAP_Invoice_Receipt r
INNER JOIN AAP_Invoice_Payment_Record pr ON r.InvoiceReceiptID = pr.InvoiceReceiptID
INNER JOIN AAP_Invoice inv              ON pr.InvoiceID        = inv.InvoiceID
WHERE r.PaidDate BETWEEN @FromDate AND @ToDate
  AND inv.InvoiceCategoryID LIKE @InvoiceCategoryID
  AND ((@SchoolID = 0) OR (inv.SchoolID = @SchoolID))">
            <SelectParameters>
                <asp:Parameter Name="FromDate" DbType="Date" />
                <asp:Parameter Name="ToDate" DbType="Date" />
                <asp:Parameter Name="InvoiceCategoryID" />
                <asp:Parameter Name="SchoolID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>

        <!-- Tabs -->
        <ul class="nav report-tabs d-print-none" id="reportTabs">
            <li class="nav-item">
                <a class="nav-link active" data-toggle="tab" href="#institutionTab">
                    <i class="fa fa-university"></i> Institution Wise (Paid)
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" data-toggle="tab" href="#dueTab">
                    <i class="fa fa-exclamation-circle text-danger"></i> Due Institutions
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" data-toggle="tab" href="#monthTab">
                    <i class="fa fa-calendar"></i> Month Wise
                </a>
            </li>
        </ul>

        <div class="tab-content tab-content-card">

            <!-- Institution Wise Paid -->
            <div id="institutionTab" class="tab-pane fade in active show">
                <div class="d-flex justify-content-between mb-2">
                    <strong><i class="fa fa-university text-teal"></i> Institution Wise Paid Report</strong>
                    <button class="btn btn-sm btn-info d-print-none" onclick="window.print(); return false;">
                        <i class="fa fa-print"></i> Print
                    </button>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="InstitutionGridView" CssClass="mGrid report-table table-hover"
                        runat="server" AutoGenerateColumns="False" DataSourceID="InstitutionSQL_Data"
                        AllowSorting="True" OnRowDataBound="GridView_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="SchoolID"     HeaderText="ID"          SortExpression="SchoolID" />
                            <asp:BoundField DataField="SchoolName"   HeaderText="Institution"  SortExpression="SchoolName" />
                            <asp:BoundField DataField="InvoiceCount" HeaderText="Invoices"     SortExpression="InvoiceCount" />
                            <asp:BoundField DataField="TotalBilled"  HeaderText="Billed"       SortExpression="TotalBilled"  DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="Discount"     HeaderText="Discount"     SortExpression="Discount"     DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="Receivable"   HeaderText="Receivable"   SortExpression="Receivable"   DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="PaidAmount"   HeaderText="Paid"         SortExpression="PaidAmount"   DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="StillDue"     HeaderText="Still Due"    SortExpression="StillDue"     DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="PaidDate"     HeaderText="Last Paid"    SortExpression="PaidDate"     DataFormatString="{0:dd MMM yyyy}" />
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <%# Convert.ToDecimal(Eval("StillDue")) <= 0
                                        ? "<span class='badge-paid'>PAID</span>"
                                        : "<span class='badge-due'>DUE</span>" %>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="alert alert-info m-2">No paid records found for the selected period.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <asp:SqlDataSource ID="InstitutionSQL_Data" runat="server"
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="
SELECT
    inv.SchoolID,
    si.SchoolName,
    COUNT(DISTINCT r.InvoiceReceiptID)    AS InvoiceCount,
    SUM(inv.TotalAmount)                  AS TotalBilled,
    SUM(inv.Discount)                     AS Discount,
    SUM(inv.TotalAmount - inv.Discount)   AS Receivable,
    SUM(pr.Amount)                        AS PaidAmount,
    SUM(inv.Due)                          AS StillDue,
    MAX(r.PaidDate)                       AS PaidDate
FROM AAP_Invoice_Receipt r
INNER JOIN AAP_Invoice_Payment_Record pr ON r.InvoiceReceiptID = pr.InvoiceReceiptID
INNER JOIN AAP_Invoice inv ON pr.InvoiceID = inv.InvoiceID
INNER JOIN SchoolInfo si ON inv.SchoolID = si.SchoolID
WHERE r.PaidDate BETWEEN @FromDate AND @ToDate
  AND inv.InvoiceCategoryID LIKE @InvoiceCategoryID
  AND ((@SchoolID = 0) OR (inv.SchoolID = @SchoolID))
GROUP BY inv.SchoolID, si.SchoolName
ORDER BY si.SchoolName">
                    <SelectParameters>
                        <asp:Parameter Name="FromDate" DbType="Date" />
                        <asp:Parameter Name="ToDate" DbType="Date" />
                        <asp:Parameter Name="InvoiceCategoryID" />
                        <asp:Parameter Name="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <!-- Due Institutions Tab -->
            <div id="dueTab" class="tab-pane fade">
                <div class="d-flex justify-content-between mb-2">
                    <strong><i class="fa fa-exclamation-circle" style="color:#c62828"></i> Institutions With Due (Invoice created but NOT fully paid)</strong>
                    <button class="btn btn-sm btn-info d-print-none" onclick="window.print(); return false;">
                        <i class="fa fa-print"></i> Print
                    </button>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="DueInstitutionGridView" CssClass="mGrid report-table table-hover"
                        runat="server" AutoGenerateColumns="False" DataSourceID="DueInstitutionSQL"
                        AllowSorting="True">
                        <Columns>
                            <asp:BoundField DataField="SchoolID"      HeaderText="ID"           SortExpression="SchoolID" />
                            <asp:BoundField DataField="SchoolName"    HeaderText="Institution"   SortExpression="SchoolName" />
                            <asp:BoundField DataField="InvoiceCount"  HeaderText="Invoices"      SortExpression="InvoiceCount" />
                            <asp:BoundField DataField="TotalBilled"   HeaderText="Billed"        SortExpression="TotalBilled"   DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="Discount"      HeaderText="Discount"      SortExpression="Discount"      DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="Receivable"    HeaderText="Receivable"    SortExpression="Receivable"    DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="PaidAmount"    HeaderText="Paid"          SortExpression="PaidAmount"    DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="StillDue"      HeaderText="Still Due"     SortExpression="StillDue"      DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="IssuDate"      HeaderText="Invoice Date"  SortExpression="IssuDate"      DataFormatString="{0:dd MMM yyyy}" />
                        </Columns>
                        <RowStyle CssClass="due-row" />
                        <EmptyDataTemplate>
                            <div class="alert alert-success m-2"><i class="fa fa-check-circle"></i> No due institutions found for this period.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <asp:SqlDataSource ID="DueInstitutionSQL" runat="server"
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="
SELECT
    inv.SchoolID,
    si.SchoolName,
    COUNT(inv.InvoiceID)                  AS InvoiceCount,
    SUM(inv.TotalAmount)                  AS TotalBilled,
    SUM(inv.Discount)                     AS Discount,
    SUM(inv.TotalAmount - inv.Discount)   AS Receivable,
    SUM(inv.PaidAmount)                   AS PaidAmount,
    SUM(inv.Due)                          AS StillDue,
    MIN(inv.IssuDate)                     AS IssuDate
FROM AAP_Invoice inv
INNER JOIN SchoolInfo si ON inv.SchoolID = si.SchoolID
WHERE inv.IsPaid = 0
  AND inv.Due > 0
  AND si.Validation = N'Valid'
  AND CAST(inv.IssuDate AS DATE) BETWEEN @FromDate AND @ToDate
  AND inv.InvoiceCategoryID LIKE @InvoiceCategoryID
  AND ((@SchoolID = 0) OR (inv.SchoolID = @SchoolID))
GROUP BY inv.SchoolID, si.SchoolName
ORDER BY SUM(inv.Due) DESC">
                    <SelectParameters>
                        <asp:Parameter Name="FromDate" DbType="Date" />
                        <asp:Parameter Name="ToDate" DbType="Date" />
                        <asp:Parameter Name="InvoiceCategoryID" />
                        <asp:Parameter Name="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <!-- Month Wise -->
            <div id="monthTab" class="tab-pane fade">
                <div class="d-flex justify-content-between mb-2">
                    <strong><i class="fa fa-calendar text-teal"></i> Month Wise Paid Report</strong>
                    <button class="btn btn-sm btn-info d-print-none" onclick="window.print(); return false;">
                        <i class="fa fa-print"></i> Print
                    </button>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="MonthGridView" CssClass="mGrid report-table table-hover"
                        runat="server" AutoGenerateColumns="False" DataSourceID="MonthSQL_Data"
                        AllowSorting="True">
                        <Columns>
                            <asp:BoundField DataField="PaidMonth"    HeaderText="Paid Month"    SortExpression="PaidMonth" />
                            <asp:BoundField DataField="InvoiceCount" HeaderText="Invoices"      SortExpression="InvoiceCount" />
                            <asp:BoundField DataField="Institutions" HeaderText="Institutions"  SortExpression="Institutions" />
                            <asp:BoundField DataField="TotalBilled"  HeaderText="Billed"        SortExpression="TotalBilled"  DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="Discount"     HeaderText="Discount"      SortExpression="Discount"     DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="PaidAmount"   HeaderText="Collected"     SortExpression="PaidAmount"   DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="StillDue"     HeaderText="Still Due"     SortExpression="StillDue"     DataFormatString="{0:N2}" />
                            <asp:BoundField DataField="CollectPct"   HeaderText="Collected %"   SortExpression="CollectPct" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="alert alert-info m-2">No paid records found for the selected period.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
                <asp:SqlDataSource ID="MonthSQL_Data" runat="server"
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="
SELECT
    FORMAT(r.PaidDate, 'MMMM yyyy')               AS PaidMonth,
    YEAR(r.PaidDate) * 100 + MONTH(r.PaidDate)    AS SortKey,
    COUNT(DISTINCT r.InvoiceReceiptID)             AS InvoiceCount,
    COUNT(DISTINCT inv.SchoolID)                   AS Institutions,
    SUM(inv.TotalAmount)                           AS TotalBilled,
    SUM(inv.Discount)                              AS Discount,
    SUM(r.TotalAmount)                             AS PaidAmount,
    SUM(inv.Due)                                   AS StillDue,
    ROUND(SUM(r.TotalAmount) * 100.0 / NULLIF(SUM(inv.TotalAmount - inv.Discount), 0), 2) AS CollectPct
FROM AAP_Invoice_Receipt r
INNER JOIN AAP_Invoice_Payment_Record pr ON r.InvoiceReceiptID = pr.InvoiceReceiptID
INNER JOIN AAP_Invoice inv ON pr.InvoiceID = inv.InvoiceID
WHERE r.PaidDate BETWEEN @FromDate AND @ToDate
  AND inv.InvoiceCategoryID LIKE @InvoiceCategoryID
  AND ((@SchoolID = 0) OR (inv.SchoolID = @SchoolID))
GROUP BY FORMAT(r.PaidDate, 'MMMM yyyy'), YEAR(r.PaidDate) * 100 + MONTH(r.PaidDate)
ORDER BY SortKey">
                    <SelectParameters>
                        <asp:Parameter Name="FromDate" DbType="Date" />
                        <asp:Parameter Name="ToDate" DbType="Date" />
                        <asp:Parameter Name="InvoiceCategoryID" />
                        <asp:Parameter Name="SchoolID" Type="Int32" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>

    </asp:Panel>

    <script>
        $(function () {
            // Tab switching
            $('#reportTabs a').on('click', function (e) {
                e.preventDefault();
                $(this).tab('show');
            });

            // Date picker
            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            // Institution search
            $(".SearchDDL").select2({ placeholder: "Search...", allowClear: true, width: '100%' });
        });
    </script>
</asp:Content>
