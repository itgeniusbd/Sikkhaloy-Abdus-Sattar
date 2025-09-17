<%@ Page Title="Add Payment Role" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Payment_Roles.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.Create_Payment_Roles" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <asp:UpdatePanel ID="UpdatePanel1" runat="server">
      <ContentTemplate>
         <h3>Add Payment Role</h3>
         <asp:Label ID="MsgLabel" runat="server" CssClass="EroorSummer"></asp:Label>
         <div class="row">
            <div class="col-sm-3">
               <label>
                  Role Name
                     <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="RoleNameTextBox" CssClass="EroorStar" ErrorMessage="Enter Role Name" ValidationGroup="ADD">*</asp:RequiredFieldValidator>
               </label>
               <asp:TextBox ID="RoleNameTextBox" placeholder="Ex: Tuition Fee" runat="server" CssClass="form-control" onfocus="empty()"></asp:TextBox>
            </div>

            <div class="col-sm-3">
               <label>
                  Number of Payment in a Particular Period of Time
                     <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="NumberOfPayTextBox" CssClass="EroorStar" ErrorMessage="Enter Number Pay" ValidationGroup="ADD">*</asp:RequiredFieldValidator>
               </label>
               <asp:TextBox ID="NumberOfPayTextBox" placeholder="Ex: 12 (Tuition Fee 12 Months)" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)"></asp:TextBox>
            </div>

            <div class="col-sm-3">
               <label>Description</label>
               <asp:TextBox ID="DescriptionTextBox" placeholder="Ex: Any Comment" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="col-sm-1">
               <asp:ValidationSummary ID="ValidationSummary2" runat="server" CssClass="EroorSummer" DisplayMode="List" ShowMessageBox="True" ValidationGroup="ADD" />
               <br />
               <asp:Button ID="AddButton" runat="server" CssClass="btn btn-primary" OnClick="AddButton_Click" Text="Add" ValidationGroup="ADD" />
            </div>
         </div>
         <br />

         <asp:GridView ID="PayCategoryGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RoleID" DataSourceID="RoleSQL" CssClass="mGrid" OnRowDeleted="PayCategoryGridView_RowDeleted">
            <Columns>
               <asp:TemplateField HeaderText="Edit">
                  <EditItemTemplate>
                     <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                     &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                  </EditItemTemplate>
                  <ItemTemplate>
                     <asp:LinkButton ID="EditLinkButton" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                  </ItemTemplate>
               </asp:TemplateField>
               <asp:TemplateField HeaderText="Role" SortExpression="Role">
                  <EditItemTemplate>
                     <asp:TextBox ID="TextBox1" runat="server" Text='<%# Bind("Role") %>' CssClass="textbox"></asp:TextBox>
                  </EditItemTemplate>
                  <ItemTemplate>
                     <asp:Label ID="Label1" runat="server" Text='<%# Bind("Role") %>'></asp:Label>
                  </ItemTemplate>
               </asp:TemplateField>
               <asp:TemplateField HeaderText="Number Of Pay" SortExpression="NumberOfPay">
                  <EditItemTemplate>
                     <asp:TextBox ID="EdNoOfPayTextBox" runat="server" CssClass="textbox" Text='<%# Bind("NumberOfPay") %>' autocomplete="off" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
                  </EditItemTemplate>
                  <ItemTemplate>
                     <asp:Label ID="Label2" runat="server" Text='<%# Bind("NumberOfPay") %>'></asp:Label>
                  </ItemTemplate>
                  <ItemStyle HorizontalAlign="Center" />
               </asp:TemplateField>
               <asp:TemplateField HeaderText="Description" SortExpression="Description">
                  <EditItemTemplate>
                     <asp:TextBox ID="TextBox3" runat="server" Text='<%# Bind("Description") %>' CssClass="textbox"></asp:TextBox>
                  </EditItemTemplate>
                  <ItemTemplate>
                     <asp:Label ID="Label3" runat="server" Text='<%# Bind("Description") %>'></asp:Label>
                  </ItemTemplate>
               </asp:TemplateField>
               <asp:TemplateField HeaderText="Delete">
                  <ItemTemplate>
                     <asp:LinkButton ID="DelLinkButton" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are you sure want to delete?')"></asp:LinkButton>
                  </ItemTemplate>
               </asp:TemplateField>
            </Columns>
         </asp:GridView>
         <asp:SqlDataSource ID="RoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Income_Roles] WHERE [RoleID] = @RoleID" InsertCommand="IF NOT EXISTS ( SELECT  * FROM Income_Roles WHERE (SchoolID = @SchoolID) AND (Role = @Role))
INSERT INTO Income_Roles(SchoolID, RegistrationID, Role, NumberOfPay, Description, Date) VALUES (@SchoolID, @RegistrationID, LTRIM(RTRIM(@Role)), @NumberOfPay, @Description, GETDATE())"
            SelectCommand="SELECT RoleID, SchoolID, RegistrationID, Role, NumberOfPay, Description, Date FROM Income_Roles WHERE (SchoolID = @SchoolID)" UpdateCommand="IF NOT EXISTS (SELECT top(1) RoleID FROM Income_Roles WHERE (SchoolID = @SchoolID) AND (Role = @Role) AND (RoleID &lt;&gt; @RoleID))
UPDATE Income_Roles SET Role = LTRIM(RTRIM(@Role)), NumberOfPay = @NumberOfPay, Description = @Description WHERE (RoleID = @RoleID)">
            <DeleteParameters>
               <asp:Parameter Name="RoleID" Type="Int32" />
            </DeleteParameters>
            <InsertParameters>
               <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
               <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
               <asp:ControlParameter ControlID="RoleNameTextBox" Name="Role" PropertyName="Text" Type="String" />
               <asp:ControlParameter ControlID="NumberOfPayTextBox" Name="NumberOfPay" PropertyName="Text" Type="Double" />
               <asp:ControlParameter ControlID="DescriptionTextBox" Name="Description" PropertyName="Text" Type="String" />
            </InsertParameters>
            <SelectParameters>
               <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
            <UpdateParameters>
               <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
               <asp:Parameter Name="Role" Type="String" />
               <asp:Parameter Name="NumberOfPay" Type="Double" />
               <asp:Parameter Name="Description" Type="String" />
               <asp:Parameter Name="RoleID" Type="Int32" />
            </UpdateParameters>
         </asp:SqlDataSource>

      </ContentTemplate>
   </asp:UpdatePanel>

   <asp:UpdateProgress ID="UpdateProgress" runat="server">
      <ProgressTemplate>
         <div id="progress_BG"></div>
         <div id="progress">
            <img src="../../CSS/loading.gif" alt="Loading..." />
            <br />
            <b>Loading...</b>
         </div>
      </ProgressTemplate>
   </asp:UpdateProgress>

   <script type="text/javascript">
      function empty() {
         $("[id*=MsgLabel]").text("");
      }
      function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0; }
   </script>
</asp:Content>
