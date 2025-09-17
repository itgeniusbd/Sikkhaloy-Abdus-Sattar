<%@ Page Title="Create Roles" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Create_Delete_Role.aspx.cs" Inherits="EDUCATION.COM.Authority.Create_Delete_Role" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

        <h3>Create Roles</h3>

        <div class="form-inline">
            <div class="form-group">
                <asp:TextBox ID="RoleName" runat="server" CssClass="form-control" placeholder="Role Name"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RoleNameReqField" runat="server" ControlToValidate="RoleName" ErrorMessage="*" CssClass="EroorStar" ValidationGroup="A" />
            </div>
            <div class="form-group">
                <asp:Button ID="CreateRoleButton" runat="server" Text="Create Role" OnClick="CreateRoleButton_Click" CssClass="btn btn-black" ValidationGroup="A" />

            </div>
        </div>
   
        <asp:GridView ID="RoleList" CssClass="mGrid" runat="server" AutoGenerateColumns="False" OnRowDeleting="RoleList_RowDeleting">
            <Columns>
                <asp:TemplateField HeaderText="Delete Role" ShowHeader="False">
                    <ItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" OnClientClick="return confirm('Are you sure you want to delete?')" CausesValidation="False" CommandName="Delete" Text="Delete"></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Role">
                    <ItemTemplate>
                        <asp:Label runat="server" ID="RoleNameLabel" Text='<%# Container.DataItem %>' />
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

</asp:Content>
