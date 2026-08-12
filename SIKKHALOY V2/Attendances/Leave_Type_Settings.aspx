<%@ Page Title="Leave Type Settings" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Leave_Type_Settings.aspx.cs" Inherits="EDUCATION.COM.ATTENDANCES.Leave_Type_Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .leave-type-card {
            max-width: 720px;
            margin: 18px auto;
            background: #fff;
            border: 1px solid #e0eaf6;
            border-radius: 12px;
            box-shadow: 0 2px 8px rgba(26,111,196,.07);
            overflow: hidden;
        }
        .leave-type-card .card-header {
            background: linear-gradient(135deg, #1a6fc4, #0e4f96);
            color: #fff;
            padding: 12px 18px;
            font-size: 16px;
            font-weight: 700;
        }
        .leave-type-card .card-body { padding: 18px; }
        .leave-type-note {
            background: #f0f6ff;
            border: 1px solid #c5d8f0;
            border-radius: 8px;
            padding: 10px 12px;
            font-size: 13px;
            color: #444;
            margin-bottom: 14px;
        }
        .add-row { display: flex; gap: 10px; margin-bottom: 16px; }
        .add-row .form-control { flex: 1; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="leave-type-card">
        <div class="card-header">ছুটির ধরণ সেটিং</div>
        <div class="card-body">
            <div class="leave-type-note">
                এখানে আপনার প্রতিষ্ঠানের জন্য গেট পাস/ছুটির ধরণ যোগ, দেখুন ও মুছুন করতে পারবেন।
                কোনো ধরণ না থাকলে সিস্টেমের ডিফল্ট তালিকা দেখাবে।
            </div>

            <div class="add-row">
                <asp:TextBox ID="LeaveTypeTextBox" runat="server" CssClass="form-control" placeholder="নতুন ছুটির ধরণ লিখুন..."></asp:TextBox>
                <asp:Button ID="AddButton" runat="server" CssClass="btn btn-primary" Text="যোগ করুন" OnClick="AddButton_Click" />
            </div>

            <asp:Label ID="MessageLabel" runat="server" CssClass="text-danger"></asp:Label>

            <asp:GridView ID="LeaveTypeGridView" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-striped"
                DataKeyNames="LeaveTypeID" OnRowDeleting="LeaveTypeGridView_RowDeleting" EmptyDataText="এখনো কোনো কাস্টম ছুটির ধরণ যোগ করা হয়নি।">
                <Columns>
                    <asp:BoundField DataField="LeaveTypeName" HeaderText="ছুটির ধরণ" />
                    <asp:BoundField DataField="SortOrder" HeaderText="ক্রম" ItemStyle-Width="80px" />
                    <asp:CommandField ShowDeleteButton="True" DeleteText="মুছুন" ControlStyle-CssClass="btn btn-sm btn-danger" />
                </Columns>
            </asp:GridView>

            <div class="mt-3">
                <a href="Leave_for_Student.aspx" class="btn btn-secondary">← Leave for Student</a>
            </div>
        </div>
    </div>
</asp:Content>
