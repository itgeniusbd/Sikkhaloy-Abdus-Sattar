<%@ Page Title="Payment Collection Report" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Online_Payment_Report.aspx.cs" Inherits="EDUCATION.COM.Authority.Invoice.Online_Payment_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* ── Summary Cards ── */
        .summary-cards { display: flex; flex-wrap: wrap; gap: 14px; margin-bottom: 22px; }
        .s-card {
            flex: 1 1 150px; border-radius: 12px; padding: 16px 18px;
            color: #fff; min-width: 130px; box-shadow: 0 4px 14px rgba(0,0,0,.15);
        }
        .s-card .s-val  { font-size: 1.5rem; font-weight: 700; margin: 0; line-height: 1.1; }
        .s-card .s-lbl  { font-size: .78rem; opacity: .88; margin: 4px 0 0; }
        .s-card .s-icon { font-size: 1.4rem; opacity: .3; float: right; margin-top: -2px; }
        .c-total    { background: linear-gradient(135deg,#1a237e,#1565c0); }
        .c-online   { background: linear-gradient(135deg,#00695c,#26a69a); }
        .c-offline  { background: linear-gradient(135deg,#4a148c,#8e24aa); }
        .c-cnt-all  { background: linear-gradient(135deg,#b71c1c,#e53935); }
        .c-cnt-on   { background: linear-gradient(135deg,#e65100,#fb8c00); }
        .c-inst     { background: linear-gradient(135deg,#1b5e20,#43a047); }

        /* ── Filter Card ── */
        .filter-card {
            background: #fff; border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,.08);
            padding: 16px 18px; margin-bottom: 20px;
        }
        .filter-card .form-group { margin-bottom: 0; }
        .filter-title { font-weight: 700; color: #1565c0; margin-bottom: 10px; font-size: .95rem; }

        /* ── Type filter badges ── */
        .type-btn { border-radius: 20px; padding: 5px 18px; font-weight: 600; font-size: .88rem; border: 2px solid transparent; cursor: pointer; }
        .type-btn.active-all     { background:#1565c0; color:#fff; border-color:#1565c0; }
        .type-btn.active-online  { background:#00897b; color:#fff; border-color:#00897b; }
        .type-btn.active-offline { background:#8e24aa; color:#fff; border-color:#8e24aa; }
        .type-btn:not([class*='active']) { background:#f5f5f5; color:#555; border-color:#ddd; }

        /* ── Grid ── */
        .rpt-table { width:100%; border-collapse:collapse; font-size:.88rem; }
        .rpt-table th { background:#1565c0; color:#fff; padding:9px 11px; text-align:left; white-space:nowrap; }
        .rpt-table td { padding:8px 11px; border-bottom:1px solid #e8eaf0; vertical-align:middle; }
        .rpt-table tr:hover td { background:#f3f6ff; }
        .badge-method { display:inline-block; padding:2px 8px; border-radius:20px; font-size:.75rem; font-weight:600; }
        .badge-nagad  { background:#ffe0b2; color:#e65100; }
        .badge-bkash  { background:#fce4ec; color:#c2185b; }
        .badge-card   { background:#e3f2fd; color:#1565c0; }
        .badge-cash   { background:#e8f5e9; color:#2e7d32; }
        .badge-other  { background:#f3e5f5; color:#6a1b9a; }
        .badge-type-online  { background:#e0f2f1; color:#00695c; font-size:.72rem; font-weight:700; padding:2px 7px; border-radius:10px; }
        .badge-type-offline { background:#f3e5f5; color:#6a1b9a; font-size:.72rem; font-weight:700; padding:2px 7px; border-radius:10px; }
        .amount-col { text-align:right; font-weight:600; }
        .school-link { color:#1565c0; font-weight:500; }
        .school-link:hover { text-decoration:underline; }
        .mGrid .pgr td { padding:4px; }
        .no-data { text-align:center; padding:40px; color:#888; }
        .date-range-info {
            background:#e3f2fd; border:1px solid #90caf9; border-radius:6px;
            padding:5px 13px; font-size:.83rem; color:#1565c0; display:inline-block; margin-bottom:12px;
        }

        /* ── Print Button ── */
        .btn-print {
            background: linear-gradient(135deg,#1565c0,#1976d2);
            color:#fff; border:none; border-radius:8px;
            padding:8px 22px; font-size:.92rem; font-weight:600;
            cursor:pointer; box-shadow:0 2px 8px rgba(21,101,192,.35);
            transition: box-shadow .2s;
        }
        .btn-print:hover { box-shadow:0 4px 14px rgba(21,101,192,.5); color:#fff; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3><i class="fa fa-bar-chart"></i> Payment Collection Report
        <small class="text-muted" style="font-size:.58em;">অনলাইন ও অফলাইন</small>
    </h3>

    <%-- ── Filter ── --%>
    <div class="filter-card">
        <div class="filter-title"><i class="fa fa-filter"></i> Search & Filter</div>
        <div class="form-inline" style="flex-wrap:wrap; gap:10px;">

            <%-- Collection Type --%>
            <div class="form-group">
                <label class="mr-1">ধরন</label>
                <asp:DropDownList ID="TypeDropDownList" runat="server" CssClass="form-control">
                    <asp:ListItem Value="All">সব (অনলাইন + অফলাইন)</asp:ListItem>
                    <asp:ListItem Value="Online">অনলাইন (ShurjoPay)</asp:ListItem>
                    <asp:ListItem Value="Offline">অফলাইন (Cash/Manual)</asp:ListItem>
                </asp:DropDownList>
            </div>

            <%-- Institution --%>
            <div class="form-group">
                <label class="mr-1">প্রতিষ্ঠান</label>
                <asp:DropDownList ID="SchoolDropDownList" runat="server" CssClass="form-control select2-school"
                    DataSourceID="SchoolSQL" DataTextField="DisplayText" DataValueField="SchoolID"
                    AppendDataBoundItems="True" Style="min-width:230px;">
                    <asp:ListItem Value="0">[ সব প্রতিষ্ঠান ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="SchoolSQL" runat="server"
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT SchoolID, CAST(SchoolID AS NVARCHAR)+' - '+SchoolName AS DisplayText FROM SchoolInfo ORDER BY SchoolID DESC">
                </asp:SqlDataSource>
            </div>

            <%-- Method --%>
            <div class="form-group">
                <label class="mr-1">পদ্ধতি</label>
                <asp:DropDownList ID="MethodDropDownList" runat="server" CssClass="form-control">
                    <asp:ListItem Value="">[ সব পদ্ধতি ]</asp:ListItem>
                    <asp:ListItem Value="Nagad">Nagad</asp:ListItem>
                    <asp:ListItem Value="bKash">bKash</asp:ListItem>
                    <asp:ListItem Value="Card">Card</asp:ListItem>
                    <asp:ListItem Value="Rocket">Rocket</asp:ListItem>
                    <asp:ListItem Value="Cash">Cash</asp:ListItem>
                </asp:DropDownList>
            </div>

            <%-- Date range --%>
            <div class="form-group">
                <label class="mr-1">From</label>
                <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control datepicker"
                    Style="width:125px;"></asp:TextBox>
            </div>
            <div class="form-group">
                <label class="mr-1">To</label>
                <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control datepicker"
                    Style="width:125px;"></asp:TextBox>
            </div>

            <div class="form-group">
                <asp:Button ID="SearchButton" runat="server" Text="🔍 Search"
                    CssClass="btn btn-primary" OnClick="SearchButton_Click" />
                <asp:Button ID="ClearButton" runat="server" Text="✕ Clear"
                    CssClass="btn btn-secondary ml-1" OnClick="ClearButton_Click" />
            </div>
        </div>
    </div>

    <%-- ── Date range info + Print Button ── --%>
    <div style="display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:8px; margin-bottom:12px;">
        <div class="date-range-info" style="margin-bottom:0;">
            <i class="fa fa-calendar"></i>
            <asp:Label ID="DateRangeLabel" runat="server"></asp:Label>
        </div>
        <button type="button" class="btn-print" onclick="doPrint()">
            <i class="fa fa-print"></i> প্রিন্ট করুন
        </button>
    </div>

    <%-- ── Summary Cards ── --%>
    <div class="summary-cards">
        <div class="s-card c-total">
            <i class="fa fa-money s-icon"></i>
            <p class="s-val">৳<asp:Label ID="TotalAmountLabel" runat="server" Text="0"></asp:Label></p>
            <p class="s-lbl">মোট কালেকশন</p>
        </div>
        <div class="s-card c-online">
            <i class="fa fa-credit-card s-icon"></i>
            <p class="s-val">৳<asp:Label ID="OnlineAmountLabel" runat="server" Text="0"></asp:Label></p>
            <p class="s-lbl">অনলাইন কালেকশন</p>
        </div>
        <div class="s-card c-offline">
            <i class="fa fa-money s-icon"></i>
            <p class="s-val">৳<asp:Label ID="OfflineAmountLabel" runat="server" Text="0"></asp:Label></p>
            <p class="s-lbl">অফলাইন কালেকশন</p>
        </div>
        <div class="s-card c-cnt-all">
            <i class="fa fa-list-ol s-icon"></i>
            <p class="s-val"><asp:Label ID="TotalCountLabel" runat="server" Text="0"></asp:Label></p>
            <p class="s-lbl">মোট ট্রানজেকশন</p>
        </div>
        <div class="s-card c-cnt-on">
            <i class="fa fa-wifi s-icon"></i>
            <p class="s-val"><asp:Label ID="OnlineCountLabel" runat="server" Text="0"></asp:Label> / <asp:Label ID="OfflineCountLabel" runat="server" Text="0"></asp:Label></p>
            <p class="s-lbl">অনলাইন / অফলাইন ট্রানজেকশন</p>
        </div>
        <div class="s-card c-inst">
            <i class="fa fa-building s-icon"></i>
            <p class="s-val"><asp:Label ID="InstitutionCountLabel" runat="server" Text="0"></asp:Label></p>
            <p class="s-lbl">প্রতিষ্ঠান সংখ্যা</p>
        </div>
    </div>

    <%-- ── Details Grid ── --%>
    <asp:GridView ID="ReportGridView" runat="server" AutoGenerateColumns="False"
        CssClass="mGrid rpt-table" AllowPaging="False"
        EmptyDataText="কোনো ডেটা পাওয়া যায়নি।">
        <Columns>
            <asp:TemplateField HeaderText="#">
                <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                <ItemStyle Width="36px" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="তারিখ">
                <ItemTemplate>
                    <strong><%# Eval("PaymentDate", "{0:d MMM yyyy}") %></strong>
                    <small class="d-block text-muted"><%# Eval("PaymentDate", "{0:hh:mm tt}") %></small>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="প্রতিষ্ঠান">
                <ItemTemplate>
                    <a href="/Authority/Institutions/Institution_Details.aspx?SchoolID=<%# Eval("SchoolID") %>"
                       class="school-link" target="_blank"><%# Eval("SchoolName") %></a>
                    <small class="d-block text-muted">ID: <%# Eval("SchoolID") %></small>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="পরিমাণ" ItemStyle-CssClass="amount-col">
                <ItemTemplate>
                    <strong style="color:#1565c0;">৳<%# string.Format("{0:N2}", Eval("Amount")) %></strong>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="পদ্ধতি">
                <ItemTemplate>
                    <span class='<%# "badge-method " + GetMethodBadge(Convert.ToString(Eval("PayMethod"))) %>'>
                        <%# Eval("PayMethod") %>
                    </span>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="ধরন">
                <ItemTemplate>
                    <span class='<%# Convert.ToString(Eval("CollectionType")) == "Online" ? "badge-type-online" : "badge-type-offline" %>'>
                        <%# Eval("CollectionType") %>
                    </span>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Collected By">
                <ItemTemplate>
                    <small><%# Eval("CollectedBy") %></small>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="রেফারেন্স">
                <ItemTemplate>
                    <small class="text-muted"><%# Eval("Reference") %></small>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
        <EmptyDataRowStyle CssClass="no-data" />
    </asp:GridView>

    <%-- Hidden iframe used for same-page printing --%>
    <iframe id="printFrame" style="display:none; width:0; height:0; border:none;"></iframe>

    <script>
        $(function () {
            if ($.fn.select2) {
                $('.select2-school').select2({ placeholder: 'প্রতিষ্ঠান খুঁজুন...', allowClear: true, width: '240px' });
            }
            $('.datepicker').datepicker({
                format: 'dd M yyyy', todayBtn: 'linked', todayHighlight: true, autoclose: true
            });
        });

        function doPrint() {
            var totalAmt   = '৳' + $('#<%= TotalAmountLabel.ClientID %>').text();
            var onlineAmt  = '৳' + $('#<%= OnlineAmountLabel.ClientID %>').text();
            var offlineAmt = '৳' + $('#<%= OfflineAmountLabel.ClientID %>').text();
            var totalCnt   = $('#<%= TotalCountLabel.ClientID %>').text();
            var instCnt    = $('#<%= InstitutionCountLabel.ClientID %>').text();
            var dateRange  = $('#<%= DateRangeLabel.ClientID %>').text();
            var now        = new Date().toLocaleString('bn-BD');

            // Build table rows from GridView
            var tableRows = '';
            $('#<%= ReportGridView.ClientID %> tr').each(function (i) {
                if (i === 0) return; // skip header row
                var cells = $(this).find('td');
                if (!cells.length) return;
                var bg = (i % 2 === 0) ? 'background:#f7f9ff;' : '';
                tableRows += '<tr style="' + bg + '">';
                cells.each(function () { tableRows += '<td>' + $(this).text().trim() + '</td>'; });
                tableRows += '</tr>';
            });
            if (!tableRows) {
                tableRows = '<tr><td colspan="8" style="text-align:center;padding:24px;color:#888;">কোনো ডেটা নেই</td></tr>';
            }

            var html = '<!DOCTYPE html><html><head><meta charset="utf-8">'
                + '<title>Payment Collection Report</title>'
                + '<style>'
                + 'body{font-family:Arial,sans-serif;margin:0;padding:18px;color:#222;font-size:12px;}'
                + '.hdr{display:flex;align-items:center;gap:12px;border-bottom:3px solid #1565c0;padding-bottom:8px;margin-bottom:14px;}'
                + '.brand{font-size:1.2rem;font-weight:700;color:#1a237e;} .sub{font-size:.75rem;color:#555;}'
                + '.rpt-title{font-size:1rem;font-weight:700;color:#1a237e;margin-bottom:3px;}'
                + '.meta{font-size:.75rem;color:#555;margin-bottom:12px;}'
                + '.cards{display:flex;flex-wrap:wrap;gap:7px;margin-bottom:14px;}'
                + '.card{border-radius:6px;padding:8px 12px;color:#fff;flex:1 1 80px;-webkit-print-color-adjust:exact;print-color-adjust:exact;}'
                + '.card .val{font-size:.95rem;font-weight:700;margin:0;} .card .lbl{font-size:.65rem;opacity:.92;margin:2px 0 0;}'
                + 'table{width:100%;border-collapse:collapse;font-size:.8rem;}'
                + 'th{background:#1565c0;color:#fff;padding:6px 8px;text-align:left;-webkit-print-color-adjust:exact;print-color-adjust:exact;}'
                + 'td{padding:5px 7px;border-bottom:1px solid #e0e0e0;vertical-align:middle;}'
                + '.footer{margin-top:12px;font-size:.68rem;color:#aaa;text-align:right;border-top:1px solid #eee;padding-top:5px;}'
                + '@page{margin:10mm;}'
                + '</style></head><body>'
                + '<div class="hdr"><div><div class="brand">Sikkhaloy.com</div><div class="sub">School Management System</div></div></div>'
                + '<div class="rpt-title">পেমেন্ট কালেকশন রিপোর্ট <small style="font-size:.75em;font-weight:400;color:#555;">অনলাইন ও অফলাইন</small></div>'
                + '<div class="meta">তারিখ: <strong>' + dateRange + '</strong> &nbsp;|&nbsp; প্রিন্ট সময়: ' + now + '</div>'
                + '<div class="cards">'
                + '<div class="card" style="background:linear-gradient(135deg,#1a237e,#1565c0);"><p class="val">' + totalAmt   + '</p><p class="lbl">মোট কালেকশন</p></div>'
                + '<div class="card" style="background:linear-gradient(135deg,#00695c,#26a69a);"><p class="val">' + onlineAmt  + '</p><p class="lbl">অনলাইন কালেকশন</p></div>'
                + '<div class="card" style="background:linear-gradient(135deg,#4a148c,#8e24aa);"><p class="val">' + offlineAmt + '</p><p class="lbl">অফলাইন কালেকশন</p></div>'
                + '<div class="card" style="background:linear-gradient(135deg,#b71c1c,#e53935);"><p class="val">' + totalCnt   + '</p><p class="lbl">মোট ট্রানজেকশন</p></div>'
                + '<div class="card" style="background:linear-gradient(135deg,#1b5e20,#43a047);"><p class="val">' + instCnt    + '</p><p class="lbl">প্রতিষ্ঠান সংখ্যা</p></div>'
                + '</div>'
                + '<table><thead><tr>'
                + '<th>#</th><th>তারিখ</th><th>প্রতিষ্ঠান</th><th style="text-align:right;">পরিমাণ</th>'
                + '<th>পদ্ধতি</th><th>ধরন</th><th>Collected By</th><th>রেফারেন্স</th>'
                + '</tr></thead><tbody>' + tableRows + '</tbody></table>'
                + '<div class="footer">Sikkhaloy.com &mdash; ' + new Date().toLocaleDateString('en-GB') + '</div>'
                + '</body></html>';

            var frame = document.getElementById('printFrame');
            var doc = frame.contentWindow.document;
            doc.open();
            doc.write(html);
            doc.close();
            // Wait for iframe content to render, then print
            setTimeout(function () {
                frame.contentWindow.focus();
                frame.contentWindow.print();
            }, 350);
        }
    </script>
</asp:Content>
