<%@ Page Title="Leave Report" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Leave_Report.aspx.cs" Inherits="EDUCATION.COM.ATTENDANCES.Leave_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .lr-wrapper { max-width: 100%; margin: 18px auto; padding: 0 16px; font-family: 'Segoe UI', Arial, sans-serif; }

        /* Title */
        .lr-title {
            display: flex; align-items: center; gap: 14px;
            margin-bottom: 18px; padding: 14px 20px;
            background: linear-gradient(135deg, #1a6fc4 0%, #0e4f96 100%);
            border-radius: 12px; box-shadow: 0 4px 14px rgba(26,111,196,.25);
        }
        .lr-title .t-icon {
            width: 44px; height: 44px; background: rgba(255,255,255,.18);
            border-radius: 10px; display: flex; align-items: center; justify-content: center;
            flex-shrink: 0; border: 1.5px solid rgba(255,255,255,.3);
        }
        .lr-title .t-icon svg { width: 22px; height: 22px; fill: none; stroke: #fff; stroke-width: 2; stroke-linecap: round; stroke-linejoin: round; }
        .lr-title h3 { margin: 0; font-size: 18px; font-weight: 700; color: #fff !important; background: none !important; box-shadow: none !important; padding: 0 !important; }
        .lr-title p  { margin: 2px 0 0; font-size: 12px; color: rgba(255,255,255,.75); }

        /* Filter Card */
        .filter-card {
            background: #fff; border: 1px solid #e0eaf6; border-radius: 12px;
            padding: 18px 22px; margin-bottom: 18px;
            box-shadow: 0 2px 8px rgba(26,111,196,.07);
        }
        .filter-row { display: flex; align-items: flex-end; gap: 14px; flex-wrap: wrap; }
        .filter-group { display: flex; flex-direction: column; gap: 5px; }
        .filter-group label { font-size: 12px; font-weight: 600; color: #1a6fc4; }
        .filter-group .form-control {
            border: 1px solid #c5d8f0; border-radius: 7px;
            font-size: 13.5px; padding: 7px 12px; min-width: 155px;
        }
        .filter-group .form-control:focus { border-color: #1a6fc4; box-shadow: 0 0 0 3px rgba(26,111,196,.12); outline: none; }
        .btn-filter {
            background: linear-gradient(135deg,#1a6fc4,#0e4f96); border: none;
            color: #fff; padding: 8px 26px; border-radius: 8px;
            font-size: 14px; font-weight: 600; cursor: pointer; transition: opacity .2s;
            height: 36px;
        }
        .btn-filter:hover { opacity: .88; }
        .btn-print {
            background: #fff; border: 1.5px solid #1a6fc4;
            color: #1a6fc4; padding: 7px 20px; border-radius: 8px;
            font-size: 14px; font-weight: 600; cursor: pointer; transition: all .2s;
            height: 36px;
        }
        .btn-print:hover { background: #f0f6ff; }

        /* Summary badges */
        .summary-bar {
            display: flex; align-items: center; gap: 12px; flex-wrap: wrap;
            margin-bottom: 12px;
        }
        .badge-count {
            display: inline-flex; align-items: center; gap: 6px;
            background: #e8f4fd; color: #1a6fc4; border: 1px solid #c5d8f0;
            border-radius: 20px; padding: 4px 14px; font-size: 13px; font-weight: 700;
        }
        .badge-count span { font-weight: 400; font-size: 12px; color: #555; }

        /* Grid */
        .grid-wrap { overflow-x: auto; border-radius: 10px; box-shadow: 0 2px 8px rgba(26,111,196,.07); }
        .mGrid { width: 100%; border-collapse: collapse; font-size: 13.5px; }
        .mGrid thead tr th {
            background: linear-gradient(135deg, #1a6fc4, #0e4f96);
            color: #fff; padding: 10px 12px; text-align: left;
            font-weight: 600; font-size: 13px; white-space: nowrap;
        }
        .mGrid tbody tr:nth-child(even) { background: #f5f9ff; }
        .mGrid tbody tr:hover { background: #e8f0fb; }
        .mGrid tbody td { padding: 9px 12px; border-bottom: 1px solid #eef3fb; vertical-align: middle; }
        .mGrid tfoot td { background: #f0f6ff; font-weight: 700; padding: 9px 12px; }

        /* Type badges */
        .type-badge {
            display: inline-block; padding: 2px 10px; border-radius: 12px;
            font-size: 11.5px; font-weight: 700;
        }
        .type-student  { background: #e6f5ee; color: #0e6640; border: 1px solid #a3d9bc; }
        .type-teacher  { background: #fff3e0; color: #b45309; border: 1px solid #fcd38d; }

        .leave-type-badge {
            display: inline-block; padding: 2px 10px; border-radius: 12px;
            font-size: 11px; background: #f0f6ff; color: #1a6fc4; border: 1px solid #c5d8f0;
        }

        /* No data */
        .no-data { text-align: center; padding: 40px; color: #aaa; font-size: 15px; }
        .no-data svg { width: 48px; height: 48px; stroke: #ccc; fill: none; margin-bottom: 10px; display: block; margin: 0 auto 10px; }

        /* Action buttons */
        .btn-act { border: none; background: none; cursor: pointer; font-size: 16px; padding: 2px 5px; border-radius: 5px; transition: background .15s; }
        .btn-edit:hover { background: #e8f0fb; }
        .btn-del:hover  { background: #fee2e2; }

        /* Modal */
        .modal-overlay { position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,.45);z-index:9999;display:flex;align-items:center;justify-content:center; }
        .modal-box { background:#fff;border-radius:12px;width:90%;max-width:620px;box-shadow:0 8px 32px rgba(0,0,0,.22);overflow:hidden; }
        .modal-header { background:linear-gradient(135deg,#1a6fc4,#0e4f96);color:#fff;padding:14px 20px;display:flex;justify-content:space-between;align-items:center;font-size:15px;font-weight:700; }
        .modal-close { background:none;border:none;color:#fff;font-size:18px;cursor:pointer;line-height:1; }
        .modal-body { padding:20px; }
        .modal-footer { padding:14px 20px;display:flex;justify-content:flex-end;gap:10px;border-top:1px solid #eee;background:#f9fbff; }
        .edit-row { display:grid;grid-template-columns:1fr 1fr;gap:14px;margin-bottom:14px; }
        .lf-group label { font-size:12px;font-weight:600;color:#555;margin-bottom:4px;display:block; }
        .lf-group .form-control { border:1px solid #c5d8f0;border-radius:7px;font-size:13.5px;padding:7px 12px;width:100%; }
        .btn-save { background:linear-gradient(135deg,#1a6fc4,#0e4f96);border:none;color:#fff;padding:9px 28px;border-radius:8px;font-size:14px;font-weight:700;cursor:pointer; }
        .btn-cancel { background:#f3f4f6;border:1px solid #ddd;color:#555;padding:9px 22px;border-radius:8px;font-size:14px;cursor:pointer; }
        .btn-del-confirm { background:#e53e3e;border:none;color:#fff;padding:9px 28px;border-radius:8px;font-size:14px;font-weight:700;cursor:pointer; }

        @media print {
            .NoPrint, .filter-card { display: none !important; }
            .lr-wrapper { max-width: 100%; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
<div class="lr-wrapper">

    <%-- Title --%>
    <div class="lr-title">
        <div class="t-icon">
            <svg viewBox="0 0 24 24"><path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/></svg>
        </div>
        <div>
            <h3>Leave Report</h3>
            <p>শিক্ষার্থী ও শিক্ষকদের ছুটির তালিকা</p>
        </div>
    </div>

    <%-- Filter --%>
    <div class="filter-card NoPrint">
        <div class="filter-row">
            <div class="filter-group">
                <label>ধরন (Type)</label>
                <asp:DropDownList ID="TypeDropDownList" runat="server" CssClass="form-control">
                    <asp:ListItem Value="Student">শিক্ষার্থী (Student)</asp:ListItem>
                    <asp:ListItem Value="Teacher">শিক্ষক (Teacher)</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <label>শুরুর তারিখ (From)</label>
                <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control Datepicker" placeholder="From Date" autocomplete="off"></asp:TextBox>
            </div>
            <div class="filter-group">
                <label>শেষ তারিখ (To)</label>
                <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datepicker" placeholder="To Date" autocomplete="off"></asp:TextBox>
            </div>
            <asp:Button ID="FilterButton" runat="server" CssClass="btn-filter" Text="🔍 দেখুন" OnClick="FilterButton_Click" />
            <button type="button" class="btn-print" onclick="window.print()">🖨️ Print</button>
        </div>
    </div>

    <%-- Results --%>
    <asp:Panel ID="ResultPanel" runat="server" Visible="false">

        <div class="summary-bar">
            <div class="badge-count">
                মোট রেকর্ড: <strong><asp:Label ID="TotalLabel" runat="server" Text="0"></asp:Label></strong>
                <span id="summaryText" runat="server"></span>
            </div>
        </div>

        <div class="grid-wrap">
            <asp:GridView ID="LeaveGridView" runat="server" CssClass="mGrid"
                AutoGenerateColumns="False" EmptyDataText="কোনো ছুটির রেকর্ড পাওয়া যায়নি।"
                AllowPaging="True" PageSize="50" OnPageIndexChanging="LeaveGridView_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="SL" HeaderText="#" ItemStyle-Width="40px" />
                    <asp:TemplateField HeaderText="ধরন">
                        <ItemTemplate>
                            <span class='type-badge <%# Eval("Type").ToString() == "Student" ? "type-student" : "type-teacher" %>'>
                                <%# Eval("Type").ToString() == "Student" ? "শিক্ষার্থী" : "শিক্ষক" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ID" HeaderText="ID" />
                    <asp:BoundField DataField="Name" HeaderText="নাম" />
                    <asp:BoundField DataField="ClassName" HeaderText="ক্লাস / পদবি" />
                    <asp:TemplateField HeaderText="ছুটির ধরণ">
                        <ItemTemplate>
                            <span class="leave-type-badge"><%# Eval("LeaveType") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="StartDate" HeaderText="শুরু" DataFormatString="{0:d MMM yyyy}" />
                    <asp:BoundField DataField="EndDate" HeaderText="শেষ" DataFormatString="{0:d MMM yyyy}" />
                    <asp:BoundField DataField="Days" HeaderText="মোট দিন" ItemStyle-HorizontalAlign="Center" />
                    <asp:BoundField DataField="GuardianName" HeaderText="অভিভাবক" />
                <asp:BoundField DataField="Description" HeaderText="কারণ" />
                    <asp:TemplateField HeaderText="Action" ItemStyle-Width="90px" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <button type="button" class="btn-act btn-edit NoPrint"
                                onclick="openEdit('<%# Eval("LeaveID") %>','<%# Eval("StartDate","{0:d MMM yyyy}") %>','<%# Eval("EndDate","{0:d MMM yyyy}") %>','<%# Eval("LeaveType") %>','<%# Eval("GuardianName") %>','<%# Server.HtmlEncode(Eval("Description").ToString()) %>')">
                                ✏️
                            </button>
                            <button type="button" class="btn-act btn-del NoPrint"
                                onclick="confirmDelete('<%# Eval("LeaveID") %>')">
                                🗑️
                            </button>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <PagerStyle CssClass="pgr" />
            </asp:GridView>
        </div>

    </asp:Panel>

    <%-- Hidden fields for delete/edit operations --%>
    <asp:HiddenField ID="hfLeaveID" runat="server" />
    <asp:HiddenField ID="hfAction" runat="server" />
    <asp:HiddenField ID="hfStartDate" runat="server" />
    <asp:HiddenField ID="hfEndDate" runat="server" />
    <asp:HiddenField ID="hfLeaveType" runat="server" />
    <asp:HiddenField ID="hfGuardianName" runat="server" />
    <asp:HiddenField ID="hfDescription" runat="server" />
    <asp:Button ID="ActionButton" runat="server" Style="position:absolute;left:-9999px;top:-9999px" OnClick="ActionButton_Click" />

    <%-- Edit Modal --%>
    <div id="editModal" class="modal-overlay" style="display:none">
        <div class="modal-box">
            <div class="modal-header">
                <span>✏️ ছুটি সম্পাদনা করুন</span>
                <button onclick="document.getElementById('editModal').style.display='none'" class="modal-close">✕</button>
            </div>
            <div class="modal-body">
                <div class="edit-row">
                    <div class="lf-group">
                        <label>শুরুর তারিখ</label>
                        <input type="text" id="editStartDate" class="form-control Datepicker2" />
                    </div>
                    <div class="lf-group">
                        <label>শেষ তারিখ</label>
                        <input type="text" id="editEndDate" class="form-control Datepicker2" />
                    </div>
                </div>
                <div class="edit-row">
                    <div class="lf-group">
                        <label>ছুটির ধরণ</label>
                        <select id="editLeaveType" class="form-control">
                            <option>অসুস্থতার জন্য</option>
                            <option>ব্যাক্তিগত কারনে</option>
                            <option>ফ্যামেলি প্রয়োজনে</option>
                            <option>মেডিক্যাল</option>
                            <option>Medical</option>
                            <option>সাময়িক</option>
                            <option>সাপ্তাহিক</option>
                            <option>মাসিক</option>
                            <option>অন্যান্ন</option>
                        </select>
                    </div>
                    <div class="lf-group">
                        <label>অভিভাবকের নাম</label>
                        <input type="text" id="editGuardian" class="form-control" />
                    </div>
                </div>
                <div class="lf-group" style="margin-top:10px">
                    <label>ছুটির কারণ</label>
                    <textarea id="editDesc" class="form-control" rows="3"></textarea>
                </div>
            </div>
            <div class="modal-footer">
                <button onclick="document.getElementById('editModal').style.display='none'" class="btn-cancel">বাতিল</button>
                <button onclick="submitEdit()" class="btn-save">✔ সংরক্ষণ করুন</button>
            </div>
        </div>
    </div>

    <%-- Delete Confirm Modal --%>
    <div id="delModal" class="modal-overlay" style="display:none">
        <div class="modal-box" style="max-width:400px">
            <div class="modal-header" style="background:#e53e3e">
                <span>🗑️ ছুটি বাতিল করুন</span>
                <button onclick="document.getElementById('delModal').style.display='none'" class="modal-close">✕</button>
            </div>
            <div class="modal-body" style="text-align:center;padding:24px">
                <p style="font-size:15px;color:#333">আপনি কি এই ছুটির রেকর্ডটি মুছে ফেলতে চান?</p>
                <p style="color:#e53e3e;font-size:13px">এই কাজটি পূর্বাবস্থায় ফেরানো যাবে না।</p>
            </div>
            <div class="modal-footer">
                <button onclick="document.getElementById('delModal').style.display='none'" class="btn-cancel">না</button>
                <button onclick="submitDelete()" class="btn-del-confirm">হ্যাঁ, মুছুন</button>
            </div>
        </div>
    </div>

    <asp:Panel ID="NoSearchPanel" runat="server" Visible="true">
        <div class="no-data">
            <svg viewBox="0 0 24 24" stroke-width="1.5"><path d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/></svg>
            তারিখ নির্বাচন করে <strong>দেখুন</strong> বাটন চাপুন
        </div>
    </asp:Panel>

</div>

<script>
    $(function () {
        var dpOpts = { format: 'dd M yyyy', todayBtn: 'linked', todayHighlight: true, autoclose: true };
        $('.Datepicker').datepicker(dpOpts);
        $(document).on('focus', '.Datepicker2', function () { $(this).datepicker(dpOpts); });
    });

    var currentLeaveID = 0;

    function openEdit(id, start, end, type, guardian, desc) {
        currentLeaveID = id;
        document.getElementById('editStartDate').value  = start;
        document.getElementById('editEndDate').value    = end;
        document.getElementById('editGuardian').value   = guardian;
        document.getElementById('editDesc').value       = desc;
        var sel = document.getElementById('editLeaveType');
        for (var i = 0; i < sel.options.length; i++) {
            if (sel.options[i].value === type) { sel.selectedIndex = i; break; }
        }
        document.getElementById('editModal').style.display = 'flex';
    }

    function submitEdit() {
        $('[id$=hfLeaveID]').val(currentLeaveID);
        $('[id$=hfAction]').val('Edit');
        $('[id$=hfStartDate]').val(document.getElementById('editStartDate').value);
        $('[id$=hfEndDate]').val(document.getElementById('editEndDate').value);
        $('[id$=hfLeaveType]').val(document.getElementById('editLeaveType').value);
        $('[id$=hfGuardianName]').val(document.getElementById('editGuardian').value);
        $('[id$=hfDescription]').val(document.getElementById('editDesc').value);
        document.getElementById('editModal').style.display = 'none';
        __doPostBack('<%= ActionButton.UniqueID %>', '');
    }

    function confirmDelete(id) {
        currentLeaveID = id;
        document.getElementById('delModal').style.display = 'flex';
    }

    function submitDelete() {
        $('[id$=hfLeaveID]').val(currentLeaveID);
        $('[id$=hfAction]').val('Delete');
        document.getElementById('delModal').style.display = 'none';
        __doPostBack('<%= ActionButton.UniqueID %>', '');
    }
</script>
</asp:Content>
