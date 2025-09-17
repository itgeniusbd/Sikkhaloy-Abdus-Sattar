<%@ Page Title="Notice Details" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Details.aspx.cs" Inherits="EDUCATION.COM.Notices.Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .n-image img { height: 100%; width: 100%; }
        .n-text { margin-top: 5px; font-size: 15px; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Notice</h3>
    <asp:Repeater ID="Notice_Repeater" runat="server" DataSourceID="NoticeSQL">
        <ItemTemplate>
            <div class="card mb-3">
                <div class="card-header">
                    <div><%# Eval("Notice_Title") %></div>
                    <small style="color: #777">View Date: <%# Eval("Show_Date","{0:d MMM yyyy}") %> - <%# Eval("End_Date","{0:d MMM yyyy}") %></small>
                </div>

                <div class="card-body">
                    <div class="n-text">
                        <%# Eval("Notice") %>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
    <asp:SqlDataSource ID="NoticeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM Notice_Admin"></asp:SqlDataSource>
</asp:Content>
