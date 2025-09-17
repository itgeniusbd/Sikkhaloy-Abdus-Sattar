<%@ Page Title="Manage Sub-Admin" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Active_deactivate_Sub_Admin.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.Active_deactivate_Sub_Admin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Temporarily Deactivate Sub-Admin Login access</h3>
    <p>Click On Approved to activate/Deactivate Sub-Admin Login access:</p>

    <div class="table-responsive">
        <asp:GridView ID="SubAdminGV" runat="server" AutoGenerateColumns="False" DataKeyNames="UserName" CssClass="mGrid" DataSourceID="SubAdminSQL">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" ReadOnly="True" SortExpression="Name" />
                <asp:BoundField DataField="Designation" HeaderText="Designation" SortExpression="Designation" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                <asp:BoundField DataField="UserName" HeaderText="User Name" SortExpression="UserName" />
                <asp:BoundField DataField="Validation" HeaderText="Validation" SortExpression="Validation" />
                <asp:TemplateField HeaderText="Approved?">
                    <ItemTemplate>
                        <asp:CheckBox ID="ApprovedCheckBox" runat="server" Checked='<%# Bind("IsApproved") %>' Text=" " OnCheckedChanged="ApprovedCheckBox_CheckedChanged" AutoPostBack="true" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Locked Out?">
                    <ItemTemplate>
                        <asp:CheckBox ID="LockedOutCheckBox" runat="server" Checked='<%# Bind("IsLockedOut") %>' Text=" " OnCheckedChanged="LockedOutCheckBox_CheckedChanged" AutoPostBack="true" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="CreateDate" HeaderText="Create Date" SortExpression="CreateDate" />
                <asp:BoundField DataField="LastLoginDate" HeaderText="Last Login" SortExpression="LastLoginDate" />
                <asp:BoundField DataField="LastPasswordChangedDate" HeaderText="Last Password Changed" SortExpression="LastPasswordChangedDate" />
            </Columns>
            <EmptyDataTemplate>
                No Record(s) Found!
            </EmptyDataTemplate>

            <PagerStyle CssClass="pgr"></PagerStyle>
        </asp:GridView>
        <asp:SqlDataSource ID="SubAdminSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name, Admin.Designation, Admin.Phone, aspnet_Membership.Email, Registration.UserName, Registration.Validation, aspnet_Membership.IsApproved, aspnet_Membership.IsLockedOut, aspnet_Membership.CreateDate, aspnet_Membership.LastLoginDate, aspnet_Membership.LastPasswordChangedDate FROM aspnet_Users INNER JOIN aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId INNER JOIN Registration ON aspnet_Users.UserName = Registration.UserName INNER JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID WHERE (Registration.Category = N'Sub-Admin') AND (Registration.SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>
</asp:Content>
