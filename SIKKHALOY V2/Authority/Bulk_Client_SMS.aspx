<%@ Page Title="Client SMS" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Bulk_Client_SMS.aspx.cs" Inherits="EDUCATION.COM.Authority.Bulk_Client_SMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid { text-align: left; width: 100%; }
        .mGrid th { background: #1e3a5f; color: #fff; padding: 8px 10px; font-size: 12px; }
        .mGrid td { padding: 6px 10px; font-size: 12px; vertical-align: middle; }
        .Invaid_Ins td { color: #dc2626; }
        .Valid_Ins td { color: #166534; }

        .search-filters {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border: 1px solid #dee2e6;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 20px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.08);
        }

        .search-row {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            align-items: end;
        }

        .search-col { flex: 1; min-width: 180px; }
        .search-col-auto { flex: 0 0 auto; }

        .form-label {
            display: block;
            margin-bottom: 5px;
            font-weight: 500;
            color: #495057;
            font-size: 13px;
        }

        .form-control {
            border: 2px solid #e9ecef;
            border-radius: 8px;
            padding: 8px 12px;
            font-size: 14px;
            width: 100%;
            box-sizing: border-box;
        }

        .btn {
            border-radius: 8px;
            padding: 10px 18px;
            font-weight: 500;
            font-size: 13px;
            border: none;
            cursor: pointer;
        }

        .btn-primary { background: #2563eb; color: #fff; }
        .btn-secondary { background: #6b7280; color: #fff; }
        .btn-success { background: #16a34a; color: #fff; }
        .btn-outline { background: #fff; color: #2563eb; border: 1px solid #2563eb; }

        .msg-panel {
            background: #fff;
            border: 1px solid #dee2e6;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 20px;
        }

        .msg-textarea {
            min-height: 110px;
            resize: vertical;
            font-family: inherit;
        }

        .summary-bar {
            display: flex;
            flex-wrap: wrap;
            gap: 12px;
            margin-bottom: 15px;
        }

        .summary-badge {
            background: #eff6ff;
            border: 1px solid #bfdbfe;
            color: #1e40af;
            border-radius: 8px;
            padding: 8px 14px;
            font-size: 13px;
            font-weight: 500;
        }

        .status-valid { color: #16a34a; font-weight: 600; }
        .status-invalid { color: #dc2626; font-weight: 600; }

        .phone-list { font-size: 11px; color: #6b7280; }
        .alert-msg {
            padding: 10px 14px;
            border-radius: 8px;
            margin-bottom: 15px;
            font-size: 13px;
        }
        .alert-success { background: #dcfce7; color: #166534; border: 1px solid #86efac; }
        .alert-error { background: #fee2e2; color: #991b1b; border: 1px solid #fca5a5; }
        .alert-info { background: #e0f2fe; color: #075985; border: 1px solid #7dd3fc; }
        .alert-warning { background: #fef3c7; color: #92400e; border: 1px solid #fcd34d; }

        .page-header h3 { margin: 0 0 20px; color: #1e3a5f; }

        /* Override Authority_Basic.css checkbox hiding inside this grid */
        .sms-select-grid input[type=checkbox] {
            display: inline-block !important;
            width: 17px;
            height: 17px;
            margin: 0;
            cursor: pointer;
            vertical-align: middle;
            opacity: 1;
            position: relative;
            accent-color: #2563eb;
        }

        .sms-select-grid input[type=checkbox] + label {
            display: none !important;
        }

        .sms-select-grid .select-col {
            text-align: center;
            width: 50px;
        }
    </style>
    <script type="text/javascript">
        function toggleAllRows(source) {
            var grid = document.getElementById('<%= SchoolGridView.ClientID %>');
            if (!grid) return;
            var inputs = grid.getElementsByTagName('input');
            for (var i = 0; i < inputs.length; i++) {
                if (inputs[i].type === 'checkbox' && inputs[i].id.indexOf('chkSelect') > -1 && inputs[i].id.indexOf('chkSelectAll') === -1) {
                    inputs[i].checked = source.checked;
                }
            }
        }

        function confirmSend() {
            var msg = document.getElementById('<%= MessageTextBox.ClientID %>');
            if (!msg || !msg.value.trim()) {
                alert('মেসেজ লিখুন।');
                return false;
            }
            return confirm('নির্বাচিত প্রতিষ্ঠানগুলোর সকল নম্বরে SMS পাঠাতে চান?');
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="page-header">
        <h3><i class="fa fa-envelope" aria-hidden="true"></i> Client / Institution SMS</h3>
    </div>

    <asp:Panel ID="DevModePanel" runat="server" Visible="false" CssClass="alert-msg alert-warning">
        <strong>Localhost Dev Mode:</strong> SMS শুধু <code>Logs/sms_dev_log.txt</code>-এ লগ হচ্ছে, মোবাইলে যাবে না।
        আসল SMS পাঠাতে production server ব্যবহার করুন অথবা <code>Web.config</code>-এ
        <code>SmsAllowLocalhost</code> = <code>false</code> করুন।
    </asp:Panel>

    <asp:Panel ID="ResultPanel" runat="server" Visible="false" CssClass="alert-msg">
        <asp:Label ID="ResultLabel" runat="server"></asp:Label>
    </asp:Panel>

    <div class="search-filters">
        <div class="search-row">
            <div class="search-col">
                <label class="form-label">Search</label>
                <asp:TextBox ID="SearchTextBox" runat="server" CssClass="form-control"
                    placeholder="Institution, Username, Phone, School ID"></asp:TextBox>
            </div>
            <div class="search-col" style="max-width: 220px;">
                <label class="form-label">Active / Deactive</label>
                <asp:DropDownList ID="ValidationFilter" runat="server" CssClass="form-control">
                    <asp:ListItem Value="" Text="All"></asp:ListItem>
                    <asp:ListItem Value="Valid" Text="Active (Valid)"></asp:ListItem>
                    <asp:ListItem Value="Invalid" Text="Deactive (Invalid)"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-col-auto">
                <label class="form-label" style="visibility:hidden;">.</label>
                <asp:Button ID="FindButton" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="FindButton_Click" />
                <asp:Button ID="ClearButton" runat="server" Text="Clear" CssClass="btn btn-secondary" OnClick="ClearButton_Click" />
            </div>
        </div>
    </div>

    <div class="summary-bar">
        <span class="summary-badge">Found: <asp:Label ID="TotalCountLabel" runat="server" Text="0"></asp:Label></span>
        <span class="summary-badge">Active: <asp:Label ID="ActiveCountLabel" runat="server" Text="0"></asp:Label></span>
        <span class="summary-badge">Deactive: <asp:Label ID="DeactiveCountLabel" runat="server" Text="0"></asp:Label></span>
        <span class="summary-badge">Provider Balance: <asp:Label ID="SmsBalanceLabel" runat="server" Text="-"></asp:Label></span>
        <span class="summary-badge">Gateway: <asp:Label ID="ActiveProviderLabel" runat="server" Text="-"></asp:Label></span>
    </div>

    <div class="msg-panel">
        <label class="form-label">Message</label>
        <asp:TextBox ID="MessageTextBox" runat="server" TextMode="MultiLine" CssClass="form-control msg-textarea"
            placeholder="Type your message here..."></asp:TextBox>
        <div style="margin-top: 12px; display: flex; flex-wrap: wrap; gap: 10px; align-items: center;">
            <asp:Button ID="SelectAllButton" runat="server" Text="Select All Visible" CssClass="btn btn-outline" OnClick="SelectAllButton_Click" />
            <asp:Button ID="ClearSelectionButton" runat="server" Text="Clear Selection" CssClass="btn btn-outline" OnClick="ClearSelectionButton_Click" />
            <asp:Button ID="SendButton" runat="server" Text="Send SMS" CssClass="btn btn-success"
                OnClick="SendButton_Click" OnClientClick="return confirmSend();" />
        </div>
    </div>

    <div class="table-responsive sms-select-grid">
    <asp:GridView ID="SchoolGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid"
        DataKeyNames="SchoolID,Phone" OnRowDataBound="SchoolGridView_RowDataBound" GridLines="None">
        <Columns>
            <asp:TemplateField HeaderText="Select" ItemStyle-CssClass="select-col" HeaderStyle-CssClass="select-col">
                <HeaderTemplate>
                    <input type="checkbox" onclick="toggleAllRows(this);" title="Select all visible" />
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:CheckBox ID="chkSelect" runat="server" Text="" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="SchoolID" HeaderText="ID" ItemStyle-Width="55px" />
            <asp:TemplateField HeaderText="Institution">
                <ItemTemplate>
                    <%# Eval("SchoolName") %>
                    <asp:HiddenField ID="hidSchoolName" runat="server" Value='<%# Eval("SchoolName") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="UserName" HeaderText="Username" ItemStyle-Width="130px" />
            <asp:TemplateField HeaderText="Phone(s)">
                <ItemTemplate>
                    <div><%# Eval("Phone") %></div>
                    <div class="phone-list"><%# Eval("PhoneCount") %> number(s)</div>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Status" ItemStyle-Width="90px">
                <ItemTemplate>
                    <asp:Label ID="StatusLabel" runat="server" Text='<%# Eval("StatusText") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:dd MMM yyyy}" ItemStyle-Width="100px" />
        </Columns>
    </asp:GridView>
    </div>
</asp:Content>
