<%@ Page Title="" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Autho_Sub_Category.aspx.cs" Inherits="EDUCATION.COM.Authority.Authority_PageLink.Autho_Sub_Category" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Insert/Edit/Delete Link Sub-category</h3>

    <div class="form-inline">
        <div class="md-form">
            <asp:TextBox ID="AscendingTextBox" runat="server" placeholder="Ascending" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="md-form mx-2">
            <asp:TextBox ID="SubCategoryTextBox" runat="server" placeholder="Sub-Category" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="md-form">
            <asp:Button ID="SubmitButton" runat="server" Text="Submit" OnClick="SubmitButton_Click" CssClass="btn btn-default" />
        </div>
    </div>
    <a href="Autho_Category.aspx">Back to Category</a>

    <asp:GridView ID="SubCategoryGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="LinkCategoryID,SubCategoryID" DataSourceID="SubCategorySQL" CssClass="mGrid">
        <Columns>
            <asp:BoundField DataField="Ascending" HeaderText="Ascending" SortExpression="Ascending" />
            <asp:BoundField DataField="SubCategory" HeaderText="SubCategory" SortExpression="SubCategory" />
            <asp:CommandField ShowEditButton="True" />
            <asp:CommandField ShowDeleteButton="True" />
            <asp:HyperLinkField DataNavigateUrlFields="LinkCategoryID,SubCategoryID"
                DataNavigateUrlFormatString="Autho_Sub_Category_Link.aspx?Category={0}&Sub_Category={1}" DataTextField="SubCategory" HeaderText="Select Sub-Category to Set URL" />
        </Columns>
        <EmptyDataTemplate>
            No Sub-Category
        </EmptyDataTemplate>
    </asp:GridView>
    <asp:SqlDataSource ID="SubCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
         DeleteCommand="DELETE FROM [Authority_Link_SubCategory] WHERE [SubCategoryID] = @SubCategoryID" 
        InsertCommand="INSERT INTO [Authority_Link_SubCategory] ([LinkCategoryID], [Ascending], [SubCategory]) VALUES (@LinkCategoryID, @Ascending, @SubCategory)"
         SelectCommand="SELECT SubCategoryID, LinkCategoryID, Ascending, SubCategory FROM Authority_Link_SubCategory WHERE (LinkCategoryID = @LinkCategoryID) ORDER BY Ascending"
         UpdateCommand="UPDATE [Authority_Link_SubCategory] SET [LinkCategoryID] = @LinkCategoryID, [Ascending] = @Ascending, [SubCategory] = @SubCategory WHERE [SubCategoryID] = @SubCategoryID">
        <DeleteParameters>
            <asp:Parameter Name="SubCategoryID" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:QueryStringParameter Name="LinkCategoryID" QueryStringField="Category" Type="Int32" />
            <asp:ControlParameter ControlID="AscendingTextBox" Name="Ascending" PropertyName="Text" Type="Int32" />
            <asp:ControlParameter ControlID="SubCategoryTextBox" Name="SubCategory" PropertyName="Text" Type="String" />
        </InsertParameters>
        <SelectParameters>
            <asp:QueryStringParameter Name="LinkCategoryID" QueryStringField="Category" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="LinkCategoryID" Type="Int32" />
            <asp:Parameter Name="Ascending" Type="Int32" />
            <asp:Parameter Name="SubCategory" Type="String" />
            <asp:Parameter Name="SubCategoryID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>



    <h3 class="mt-4">Insert URL Under Category</h3>
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
            <asp:DropDownList ID="RoleDropDownList" runat="server" AppendDataBoundItems="True" DataSourceID="RoleSQL" DataTextField="RoleName" DataValueField="RoleId" CssClass="form-control">
                <asp:ListItem>[ SELECT ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [RoleId], [RoleName] FROM [vw_aspnet_Roles]"></asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:Button ID="UrlButton" runat="server" OnClick="UrlButton_Click" Text="Submit" ValidationGroup="1" CssClass="btn btn-primary" />
        </div>
    </div>

    <asp:GridView ID="InsertedLinkGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="LinkID" DataSourceID="Link_PagesSQL" CssClass="mGrid" OnRowUpdating="InsertedLinkGridView_RowUpdating">
        <Columns>
            <asp:CommandField ShowDeleteButton="True" ShowEditButton="True" />
            <asp:BoundField DataField="Ascending" HeaderText="Ascending" SortExpression="Ascending" />
            <asp:BoundField DataField="PageURL" HeaderText="PageURL" SortExpression="PageURL" />
            <asp:BoundField DataField="PageTitle" HeaderText="PageTitle" SortExpression="PageTitle" />
            <asp:TemplateField HeaderText="Category" SortExpression="Category">
                <EditItemTemplate>
                    <asp:DropDownList ID="CategotyDropDownList" runat="server" AutoPostBack="True" DataSourceID="CategorySQL" DataTextField="Category"
                        SelectedValue='<%#Bind("LinkCategoryID") %>' DataValueField="LinkCategoryID">
                    </asp:DropDownList>
                    <asp:DropDownList ID="SubCategoryDropDownList" runat="server" DataSourceID="SubCategorySQL" DataTextField="SubCategory"
                        DataValueField="SubCategoryID" OnDataBound="SubCategoryDropDownList_DataBound">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SubCategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Authority_Link_SubCategory] WHERE ([LinkCategoryID] = @LinkCategoryID)" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="CategotyDropDownList" Name="LinkCategoryID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Authority_Link_Category]" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>"></asp:SqlDataSource>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Category") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Role" SortExpression="RoleName">
                <EditItemTemplate>
                    <asp:DropDownList ID="RoleDropDownList" runat="server" AppendDataBoundItems="True" DataSourceID="RoleSQL" DataTextField="RoleName" DataValueField="RoleId" SelectedValue='<%# Bind("RoleId") %>'>
                        <asp:ListItem Value="00000000-0000-0000-0000-000000000000">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                </EditItemTemplate>
                <ItemTemplate>
                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("RoleName") %>'></asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="Link_PagesSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        DeleteCommand="DELETE FROM [Authority_Link_Pages] WHERE [LinkID] = @LinkID
DELETE FROM Authority_Link_Users WHERE (LinkID = @LinkID)"
        InsertCommand="INSERT INTO Authority_Link_Pages(LinkCategoryID, Ascending, PageURL, PageTitle, RoleId) VALUES (@LinkCategoryID, @Ascending, @PageURL, @PageTitle, @RoleId)"
        SelectCommand="SELECT Authority_Link_Pages.LinkID, Authority_Link_Pages.LinkCategoryID, Authority_Link_Pages.Ascending, Authority_Link_Pages.PageURL, Authority_Link_Pages.PageTitle, Link_Category.Category, Authority_Link_Pages.SubCategoryID, aspnet_Roles.RoleName,ISNULL(Authority_Link_Pages.RoleId,'00000000-0000-0000-0000-000000000000')AS RoleId FROM Authority_Link_Pages LEFT OUTER JOIN aspnet_Roles ON Authority_Link_Pages.RoleId = aspnet_Roles.RoleId LEFT OUTER JOIN Link_Category ON Authority_Link_Pages.LinkCategoryID = Link_Category.LinkCategoryID WHERE (Authority_Link_Pages.LinkCategoryID = @LinkCategoryID) AND (Authority_Link_Pages.SubCategoryID IS NULL) ORDER BY Authority_Link_Pages.Ascending"
        UpdateCommand="UPDATE Authority_Link_Pages SET Ascending = @Ascending, PageURL = @PageURL, PageTitle = @PageTitle, LinkCategoryID = @LinkCategoryID, SubCategoryID = @SubCategoryID, RoleId = @RoleId WHERE (LinkID = @LinkID)">
        <DeleteParameters>
            <asp:Parameter Name="LinkID" Type="Int32" />
        </DeleteParameters>
        <InsertParameters>
            <asp:QueryStringParameter Name="LinkCategoryID" QueryStringField="Category" Type="Int32" />
            <asp:ControlParameter ControlID="LinkAsecendingTextBox" Name="Ascending" PropertyName="Text" Type="Int32" />
            <asp:ControlParameter ControlID="PageURLTextBox" Name="PageURL" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="PageTitleTextBox" Name="PageTitle" PropertyName="Text" Type="String" />
            <asp:ControlParameter ControlID="RoleDropDownList" Name="RoleId" PropertyName="SelectedValue" />
        </InsertParameters>
        <SelectParameters>
            <asp:QueryStringParameter Name="LinkCategoryID" QueryStringField="Category" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="Ascending" Type="Int32" />
            <asp:Parameter Name="PageURL" Type="String" />
            <asp:Parameter Name="PageTitle" Type="String" />
            <asp:Parameter Name="LinkCategoryID" />
            <asp:Parameter Name="SubCategoryID" />
            <asp:Parameter Name="RoleId" />
            <asp:Parameter Name="LinkID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>
</asp:Content>
