<%@ Page Title="Committee Member Type" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="MemberType.aspx.cs" Inherits="EDUCATION.COM.Employee.MemberType" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Committee Member Type</h3>
        <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="MemberTypeTextBox" runat="server" CssClass="form-control" placeholder="Member Type Name"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="MemberTypeTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="AddMemberTypeButton" runat="server" CssClass="btn btn-primary" Text="Add Member Type" OnClick="AddMemberTypeButton_Click" ValidationGroup="F" />
        </div>
    </div>

    <asp:GridView ID="MemberTypeGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeMemberTypeId" DataSourceID="MemberTypeSQL">
        <Columns>       
            <asp:TemplateField HeaderText="Member Type Name" SortExpression="CommitteeMemberType">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" Text='<%# Bind("CommitteeMemberType") %>'></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("CommitteeMemberType") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Create Date" SortExpression="InsertDate">
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("InsertDate", "{0:d MMM yyyy}") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField ShowHeader="False">
                <EditItemTemplate>
                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                    &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:CommandField ShowDeleteButton="True" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
        DeleteCommand="DELETE FROM [CommitteeMemberType] WHERE [CommitteeMemberTypeId] = @CommitteeMemberTypeId" 
        InsertCommand="IF NOT EXISTS(SELECT * FROM CommitteeMemberType WHERE SchoolID = @SchoolID AND CommitteeMemberType = @CommitteeMemberType) INSERT INTO [CommitteeMemberType] ([SchoolID], [RegistrationID], [CommitteeMemberType]) VALUES (@SchoolID, @RegistrationID, @CommitteeMemberType)"
        SelectCommand="SELECT CommitteeMemberTypeId, SchoolID, RegistrationID, CommitteeMemberType, InsertDate FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)" 
        UpdateCommand="IF NOT EXISTS(SELECT * FROM CommitteeMemberType WHERE SchoolID = @SchoolID AND CommitteeMemberType = @CommitteeMemberType) UPDATE CommitteeMemberType SET CommitteeMemberType = @CommitteeMemberType WHERE (CommitteeMemberTypeId = @CommitteeMemberTypeId)">
        <DeleteParameters>
            <asp:Parameter Name="CommitteeMemberTypeId" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="MemberTypeTextBox" Name="CommitteeMemberType" PropertyName="Text" Type="String" />
        </InsertParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="CommitteeMemberType" Type="String" />
            <asp:Parameter Name="CommitteeMemberTypeId" Type="Int32" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </UpdateParameters>
    </asp:SqlDataSource>
</asp:Content>
