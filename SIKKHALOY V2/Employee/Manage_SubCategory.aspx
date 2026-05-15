<%@ Page Title="Manage Employee Sub-Category" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Manage_SubCategory.aspx.cs" Inherits="EDUCATION.COM.Employee.Manage_SubCategory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .page-header { background: linear-gradient(135deg,#1a6fc4,#0d47a1); color:#fff; border-radius:12px; padding:20px 28px; margin-bottom:22px; display:flex; align-items:center; gap:16px; }
        .page-header-icon { background:rgba(255,255,255,.15); border-radius:10px; padding:10px; display:flex; align-items:center; justify-content:center; flex-shrink:0; }
        .page-header h3 { margin:0; font-size:20px; font-weight:700; }
        .page-header p { margin:4px 0 0; opacity:.8; font-size:13px; }
        .add-card { background:#f4f7fb; border:1px solid #dce3ec; border-radius:10px; padding:16px 20px; margin-bottom:18px; }
        .add-card-title { display:flex; align-items:center; gap:8px; margin:0 0 14px; font-size:15px; font-weight:700; color:#1a6fc4; }
        .add-row { display:flex; gap:10px; align-items:center; flex-wrap:wrap; }
        .add-row input[type=text] { flex:1; min-width:200px; border-radius:8px; border:1.5px solid #ccc; padding:8px 14px; font-size:14px; }
        .add-row select { border-radius:8px; border:1.5px solid #ccc; padding:8px 14px; font-size:14px; min-width:160px; }
        .btn-add { display:inline-flex; align-items:center; gap:7px; background:#28a745; color:#fff; border:none; border-radius:8px; padding:9px 22px; font-size:14px; font-weight:600; cursor:pointer; white-space:nowrap; }
        .btn-add:hover { background:#218838; }
        .cat-grid { border-radius:10px; overflow:hidden; box-shadow:0 2px 10px rgba(0,0,0,.08); margin-bottom:20px; }
        .cat-grid table { width:100%; border-collapse:collapse; }
        .cat-grid thead th { background:#1a6fc4; color:#fff; padding:11px 16px; font-size:13px; text-align:left; }
        .cat-grid tbody tr:nth-child(even) { background:#f8f9fa; }
        .cat-grid tbody tr:hover { background:#e8f0fe; transition:background .15s; }
        .cat-grid tbody td { padding:10px 16px; font-size:13px; border-bottom:1px solid #eee; vertical-align:middle; }
        .badge-teacher { background:#e3f2fd; color:#1565c0; border-radius:12px; padding:3px 12px; font-size:12px; font-weight:600; }
        .badge-staff { background:#e8f5e9; color:#2e7d32; border-radius:12px; padding:3px 12px; font-size:12px; font-weight:600; }
        .action-btns { display:flex; gap:6px; align-items:center; flex-wrap:nowrap; }
        .btn-action { display:inline-flex; align-items:center; gap:5px; border:none; border-radius:6px; padding:5px 13px; font-size:12px; font-weight:600; cursor:pointer; white-space:nowrap; text-decoration:none; }
        .btn-action-edit { background:#1a6fc4; color:#fff; }
        .btn-action-edit:hover { background:#155fa0; color:#fff; }
        .btn-action-del { background:#dc3545; color:#fff; }
        .btn-action-del:hover { background:#b02a37; color:#fff; }
        .btn-action-save { background:#28a745; color:#fff; }
        .btn-action-save:hover { background:#218838; color:#fff; }
        .btn-action-cancel { background:#6c757d; color:#fff; }
        .btn-action-cancel:hover { background:#545b62; color:#fff; }
        .emp-count { display:inline-flex; align-items:center; gap:4px; background:#1a6fc4; color:#fff; border-radius:10px; padding:2px 9px; font-size:11px; margin-left:6px; }
        .btn-back { display:inline-flex; align-items:center; gap:8px; background:#6c757d; color:#fff; border-radius:8px; padding:9px 20px; font-size:13px; font-weight:600; text-decoration:none; }
        .btn-back:hover { background:#545b62; color:#fff; text-decoration:none; }
        h3 {
    padding: 1rem;
    border-radius: .25rem;
    margin-top: 3px;
    text-transform: uppercase;
    /* font-size: 1rem; */
    font-weight: 400;
    /* background-color: #fff; */
    /* box-shadow: 0 2px 5px 0 rgba(0, 0, 0, .16), 0 2px 10px 0 rgba(0, 0, 0, .12); */
    /* margin-bottom: 1.5rem !important; */
}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="page-header">
        <div class="page-header-icon">
            <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        </div>
        <div>
            <h3>কর্মী সাব-ক্যাটাগরি ম্যানেজ</h3>
            <p>প্রতিষ্ঠানের শিক্ষক ও স্টাফদের জন্য পছন্দমতো সাব-ক্যাটাগরি তৈরি করুন</p>
        </div>
    </div>

    <asp:Label ID="MsgLabel" runat="server" CssClass="alert alert-success" Visible="false" EnableViewState="false"></asp:Label>
    <asp:Label ID="ErrLabel" runat="server" CssClass="alert alert-danger" Visible="false" EnableViewState="false"></asp:Label>

    <%-- Add new sub-category --%>
    <div class="add-card">
        <h5 class="add-card-title">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#1a6fc4" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="16"/><line x1="8" y1="12" x2="16" y2="12"/></svg>
            নতুন সাব-ক্যাটাগরি যোগ করুন
        </h5>
        <div class="add-row">
            <asp:DropDownList ID="EmpTypeDropDownList" runat="server" CssClass="">
                <asp:ListItem Value="Teacher">শিক্ষক (Teacher)</asp:ListItem>
                <asp:ListItem Value="Staff">স্টাফ (Staff)</asp:ListItem>
            </asp:DropDownList>
            <asp:TextBox ID="SubCategoryNameTextBox" runat="server" placeholder="সাব-ক্যাটাগরির নাম (যেমন: মর্নিং শিফট শিক্ষক)" CssClass=""></asp:TextBox>
            <asp:Button ID="AddButton" runat="server" Text="যোগ করুন" CssClass="btn-add" OnClick="AddButton_Click" />
        </div>
    </div>

    <%-- List --%>
    <div class="cat-grid">
        <asp:GridView ID="SubCategoryGridView" runat="server" AutoGenerateColumns="False"
            DataKeyNames="SubCategoryID" DataSourceID="SubCategorySQL"
            OnRowDeleting="SubCategoryGridView_RowDeleting"
            OnRowEditing="SubCategoryGridView_RowEditing"
            OnRowUpdating="SubCategoryGridView_RowUpdating"
            OnRowCancelingEdit="SubCategoryGridView_RowCancelingEdit"
            CssClass="table table-bordered" GridLines="None">
            <Columns>
                <asp:TemplateField HeaderText="সাব-ক্যাটাগরির নাম">
                    <ItemTemplate>
                        <%# Eval("SubCategoryName") %>
                        <span class="emp-count"><%# Eval("EmpCount") %> জন</span>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="EditNameTextBox" runat="server" Text='<%# Bind("SubCategoryName") %>' CssClass="form-control" style="display:inline-block;width:auto;" />
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="ধরন">
                    <ItemTemplate>
                        <span class='<%# Eval("EmployeeType").ToString()=="Teacher" ? "badge-teacher" : "badge-staff" %>'>
                            <%# Eval("EmployeeType").ToString()=="Teacher" ? "শিক্ষক" : "স্টাফ" %>
                        </span>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:DropDownList ID="EditTypeDropDownList" runat="server" SelectedValue='<%# Bind("EmployeeType") %>' CssClass="form-control" style="display:inline-block;width:auto;">
                            <asp:ListItem Value="Teacher">Teacher</asp:ListItem>
                            <asp:ListItem Value="Staff">Staff</asp:ListItem>
                        </asp:DropDownList>
                    </EditItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="অ্যাকশন" ItemStyle-Width="160px">
                    <ItemTemplate>
                        <div class="action-btns">
                            <asp:LinkButton ID="EditBtn" runat="server" CommandName="Edit" CssClass="btn-action btn-action-edit">
                                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                                এডিট
                            </asp:LinkButton>
                            <asp:LinkButton ID="DeleteBtn" runat="server" CommandName="Delete" CssClass="btn-action btn-action-del"
                                OnClientClick="return confirm('এই সাব-ক্যাটাগরি মুছে ফেলবেন? Assigned কর্মীদের সাব-ক্যাটাগরি খালি হয়ে যাবে।')">
                                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
                                মুছুন
                            </asp:LinkButton>
                        </div>
                    </ItemTemplate>
                    <EditItemTemplate>
                        <div class="action-btns">
                            <asp:LinkButton ID="UpdateBtn" runat="server" CommandName="Update" CssClass="btn-action btn-action-save">
                                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
                                সেভ
                            </asp:LinkButton>
                            <asp:LinkButton ID="CancelBtn" runat="server" CommandName="Cancel" CssClass="btn-action btn-action-cancel">
                                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                                বাতিল
                            </asp:LinkButton>
                        </div>
                    </EditItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div style="padding:20px; text-align:center; color:#888;">এখনো কোন সাব-ক্যাটাগরি নেই। উপরে থেকে যোগ করুন।</div>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>

    <asp:SqlDataSource ID="SubCategorySQL" runat="server"
        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT sc.SubCategoryID, sc.SubCategoryName, sc.EmployeeType, COUNT(ei.EmployeeID) AS EmpCount FROM Employee_SubCategory sc LEFT JOIN Employee_Info ei ON sc.SubCategoryID = ei.SubCategoryID AND ei.Job_Status='Active' WHERE sc.SchoolID = @SchoolID GROUP BY sc.SubCategoryID, sc.SubCategoryName, sc.EmployeeType ORDER BY sc.EmployeeType, sc.SubCategoryName"
        UpdateCommand="UPDATE Employee_SubCategory SET SubCategoryName=@SubCategoryName, EmployeeType=@EmployeeType WHERE SubCategoryID=@SubCategoryID AND SchoolID=@SchoolID"
        DeleteCommand="UPDATE Employee_Info SET SubCategoryID=NULL WHERE SubCategoryID=@SubCategoryID; DELETE FROM Employee_SubCategory WHERE SubCategoryID=@SubCategoryID AND SchoolID=@SchoolID">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="SubCategoryName" />
            <asp:Parameter Name="EmployeeType" />
            <asp:Parameter Name="SubCategoryID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </UpdateParameters>
        <DeleteParameters>
            <asp:Parameter Name="SubCategoryID" />
        </DeleteParameters>
    </asp:SqlDataSource>

    <br />
    <a href="Employee_List.aspx" class="btn-back">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
        Employee List-এ ফিরুন
    </a>

</asp:Content>
