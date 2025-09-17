<%@ Page Title="Donation Category" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationCategory.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationCategory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Add Donation Category</h3>
    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="DonationCategoryTextBox" runat="server" CssClass="form-control" placeholder="Category Name"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="DonationCategoryTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="F"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="AddCategoryButton" runat="server" CssClass="btn btn-primary" Text="Add Category" OnClick="AddCategoryButton_Click" ValidationGroup="F" />
        </div>
    </div>

    <asp:GridView ID="CategoryGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeDonationCategoryId" DataSourceID="CategorySQL">
        <Columns>
            <asp:TemplateField HeaderText="Category Name" SortExpression="DonationCategory">
                <EditItemTemplate>
                    <asp:TextBox ID="TextBox1" CssClass="form-control" runat="server" Text='<%# Bind("DonationCategory") %>'></asp:TextBox>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("DonationCategory") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Create Date" SortExpression="InsertDate">
                <ItemTemplate>
                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("InsertDate", "{0:d MMM yyyy}") %>'></asp:Label>
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
    <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [CommitteeDonationCategory] WHERE [CommitteeDonationCategoryId] = @CommitteeDonationCategoryId" InsertCommand="IF NOT EXISTS(SELECT * FROM CommitteeDonationCategory WHERE SchoolID = @SchoolID AND DonationCategory = @DonationCategory)
INSERT INTO [CommitteeDonationCategory] ([SchoolID], [RegistrationID], [DonationCategory]) VALUES (@SchoolID, @RegistrationID, @DonationCategory)"
        SelectCommand="SELECT CommitteeDonationCategoryId, SchoolID, RegistrationID, DonationCategory, InsertDate FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)" UpdateCommand="IF NOT EXISTS(SELECT * FROM CommitteeDonationCategory WHERE SchoolID = @SchoolID AND DonationCategory = @DonationCategory)
UPDATE CommitteeDonationCategory SET DonationCategory = @DonationCategory WHERE (CommitteeDonationCategoryId = @CommitteeDonationCategoryId)">
        <DeleteParameters>
            <asp:Parameter Name="CommitteeDonationCategoryId" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="DonationCategoryTextBox" Name="DonationCategory" PropertyName="Text" Type="String" />
        </InsertParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="DonationCategory" Type="String" />
            <asp:Parameter Name="CommitteeDonationCategoryId" Type="Int32" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </UpdateParameters>
    </asp:SqlDataSource>
</asp:Content>
