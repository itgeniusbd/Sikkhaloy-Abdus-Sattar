<%@ Page Title="" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Autho_Sub_Category_Link.aspx.cs" Inherits="EDUCATION.COM.Authority.Authority_PageLink.Autho_Sub_Category_Link" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3>Insert URL Under Category &amp; Sub-Category</h3>
    <a href="Autho_Sub_Category.aspx?Category=<% =Request.QueryString["Category"] %>">Back to Sub-Category</a>
    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="LinkAsecendingTextBox" placeholder="Ascending" runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:TextBox ID="PageTitleTextBox" placeholder="Page Title" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="PageTitleTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>

        </div>
        <div class="form-group">
            <asp:TextBox ID="PageURLTextBox" placeholder="Page URL" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="PageURLTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="RoleDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" DataSourceID="RoleSQL" DataTextField="RoleName" DataValueField="RoleId">
                <asp:ListItem>[ SELECT ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [RoleId], [RoleName] FROM [vw_aspnet_Roles]"></asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:Button ID="Button1" runat="server" OnClick="SubmitButton_Click" Text="Submit" ValidationGroup="1" CssClass="btn btn-brown" />
        </div>


    </div>

    <asp:GridView ID="InsertedLinkGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="LinkID,LinkCategoryID,RoleName" DataSourceID="Link_PagesSQL" OnRowUpdating="InsertedLinkGridView_RowUpdating" OnRowDeleting="InsertedLinkGridView_RowDeleting">
        <Columns>
            <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />
            <asp:BoundField DataField="Ascending" HeaderText="Ascending" SortExpression="Ascending" />
            <asp:BoundField DataField="PageTitle" HeaderText="PageTitle" SortExpression="PageTitle" />
            <asp:BoundField DataField="PageURL" HeaderText="PageURL" SortExpression="PageURL" />
            <asp:TemplateField HeaderText="Category">
                <EditItemTemplate>
                    <asp:DropDownList ID="CategotyDropDownList" runat="server" AutoPostBack="True" DataSourceID="CategorySQL" DataTextField="Category"
                        DataValueField="LinkCategoryID" SelectedValue='<%# Bind("LinkCategoryID") %>'>
                    </asp:DropDownList>
                    <asp:DropDownList ID="SubCategoryDropDownList" runat="server" DataSourceID="SubCategorySQL" DataTextField="SubCategory"
                        DataValueField="SubCategoryID" OnDataBound="SubCategoryDropDownList_DataBound" SelectedValue='<%# Bind("SubCategoryID") %>'>
                    </asp:DropDownList>

                    <asp:SqlDataSource ID="SubCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Authority_Link_SubCategory] WHERE ([LinkCategoryID] = @LinkCategoryID)" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="CategotyDropDownList" Name="LinkCategoryID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Authority_Link_Category]" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>"></asp:SqlDataSource>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="CategoryLabel" runat="server" Text='<%# Bind("Category") %>'></asp:Label>
                    <asp:Label ID="SubCategoryLabel" runat="server" Text='<%# Bind("SubCategory") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Role" SortExpression="RoleName">
                <EditItemTemplate>
                    <asp:DropDownList ID="RoleDropDownList" runat="server" AppendDataBoundItems="True" DataSourceID="RoleSQL" DataTextField="RoleName" DataValueField="RoleId" SelectedValue='<%# Bind("RoleId") %>'>
                        <asp:ListItem Value="00000000-0000-0000-0000-000000000000">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="RoleLabel" runat="server" Text='<%# Bind("RoleName") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="Link_PagesSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        DeleteCommand="DELETE FROM [Authority_Link_Pages] WHERE [LinkID] = @LinkID
DELETE FROM Authority_Link_Users WHERE (LinkID = @LinkID)"
        InsertCommand="INSERT INTO Authority_Link_Pages(LinkCategoryID, Ascending, PageURL, PageTitle, SubCategoryID, RoleId) VALUES (@LinkCategoryID, @Ascending, @PageURL, @PageTitle, @SubCategoryID, @RoleId)"
        SelectCommand="SELECT Authority_Link_Pages.LinkID, Authority_Link_Pages.LinkCategoryID, Authority_Link_Pages.Ascending, Authority_Link_Pages.PageURL, Authority_Link_Pages.PageTitle, Link_Category.Category, Link_SubCategory.SubCategory, Link_SubCategory.SubCategoryID, aspnet_Roles.RoleName, ISNULL(Authority_Link_Pages.RoleId,'00000000-0000-0000-0000-000000000000')AS RoleId FROM Authority_Link_Pages INNER JOIN Link_SubCategory ON Authority_Link_Pages.SubCategoryID = Link_SubCategory.SubCategoryID LEFT OUTER JOIN aspnet_Roles ON Authority_Link_Pages.RoleId = aspnet_Roles.RoleId LEFT OUTER JOIN Link_Category ON Authority_Link_Pages.LinkCategoryID = Link_Category.LinkCategoryID WHERE (Authority_Link_Pages.LinkCategoryID = @LinkCategoryID) AND (Authority_Link_Pages.SubCategoryID = @SubCategoryID) ORDER BY Authority_Link_Pages.Ascending"
        UpdateCommand="UPDATE Authority_Link_Pages SET Ascending = @Ascending, PageURL = @PageURL, PageTitle = @PageTitle, LinkCategoryID = @LinkCategoryID, SubCategoryID = @SubCategoryID, RoleId = @RoleId WHERE (LinkID = @LinkID)">
        <DeleteParameters>
            <asp:Parameter Name="LinkID" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:QueryStringParameter Name="LinkCategoryID" QueryStringField="Category" Type="Int32" />
            <asp:ControlParameter ControlID="LinkAsecendingTextBox" Name="Ascending" PropertyName="Text" Type="Int32" />
            <asp:ControlParameter ControlID="PageURLTextBox" Name="PageURL" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="PageTitleTextBox" Name="PageTitle" PropertyName="Text" Type="String" />
            <asp:QueryStringParameter Name="SubCategoryID" QueryStringField="Sub_Category" />
            <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleId" PropertyName="SelectedValue" />
        </InsertParameters>
        <SelectParameters>
            <asp:QueryStringParameter Name="LinkCategoryID" QueryStringField="Category" />
            <asp:QueryStringParameter Name="SubCategoryID" QueryStringField="Sub_Category" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="Ascending" Type="Int32" />
            <asp:Parameter Name="PageURL" Type="String" />
            <asp:Parameter Name="PageTitle" Type="String" />
            <asp:Parameter Name="LinkCategoryID" />
            <asp:Parameter Name="SubCategoryID" />
            <asp:Parameter Name="LinkID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>

</asp:Content>
