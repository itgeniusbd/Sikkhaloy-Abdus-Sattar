<%@ Page Title="Donation Templates" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationTemplates.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationTemplates" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Donation Amount Templates</h3>
    <p class="text-muted">Create preset amount templates for different member types and categories</p>

    <div class="card card-body mb-4">
        <h5>Add New Template</h5>
        <div class="form-inline">
            <div class="form-group">
                <label class="mr-2">Member Type</label>
                <asp:DropDownList ID="MemberTypeDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                    <asp:ListItem Value="">[ Select Type ]</asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="MemberTypeDropDownList" ErrorMessage="*" CssClass="EroorStar" ValidationGroup="AddTemplate"></asp:RequiredFieldValidator>
                <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="form-group ml-3">
                <label class="mr-2">Donation Category</label>
                <asp:DropDownList ID="CategoryDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId">
                    <asp:ListItem Value="">[ Select Category ]</asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="CategoryDropDownList" ErrorMessage="*" CssClass="EroorStar" ValidationGroup="AddTemplate"></asp:RequiredFieldValidator>
                <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="form-group ml-3">
                <label class="mr-2">Amount</label>
                <asp:TextBox ID="AmountTextBox" runat="server" CssClass="form-control" placeholder="Amount" type="number" step="0.01" min="0.01"></asp:TextBox>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="AmountTextBox" ErrorMessage="*" CssClass="EroorStar" ValidationGroup="AddTemplate"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group ml-3">
                <asp:Button ID="AddTemplateButton" runat="server" Text="Add Template" CssClass="btn btn-primary" OnClick="AddTemplateButton_Click" ValidationGroup="AddTemplate" />
            </div>
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="TemplatesGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="DonationTemplateId" DataSourceID="TemplatesSQL">
            <Columns>
                <asp:BoundField DataField="CommitteeMemberType" HeaderText="Member Type" ReadOnly="True" SortExpression="CommitteeMemberType" />
                <asp:BoundField DataField="DonationCategory" HeaderText="Donation Category" ReadOnly="True" SortExpression="DonationCategory" />
                <asp:TemplateField HeaderText="Amount" SortExpression="Amount">
                    <EditItemTemplate>
                        <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" Text='<%# Bind("Amount") %>' type="number" step="0.01"></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        ?<%# Eval("Amount") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Created Date" SortExpression="CreatedDate">
                    <ItemTemplate>
                        <%# Eval("CreatedDate", "{0:d MMM yyyy}") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Edit">
                    <EditItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                        &nbsp;<asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit"></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:CommandField ShowDeleteButton="True" HeaderText="Delete" />
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="TemplatesSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT dt.DonationTemplateId, dt.CommitteeMemberTypeId, dt.CommitteeDonationCategoryId, dt.Amount, dt.CreatedDate, 
                           cmt.CommitteeMemberType, cdc.DonationCategory 
                           FROM CommitteeDonationTemplate dt 
                           INNER JOIN CommitteeMemberType cmt ON dt.CommitteeMemberTypeId = cmt.CommitteeMemberTypeId 
                           INNER JOIN CommitteeDonationCategory cdc ON dt.CommitteeDonationCategoryId = cdc.CommitteeDonationCategoryId 
                           WHERE dt.SchoolID = @SchoolID 
                           ORDER BY cmt.CommitteeMemberType, cdc.DonationCategory"
            DeleteCommand="DELETE FROM CommitteeDonationTemplate WHERE DonationTemplateId = @DonationTemplateId"
            UpdateCommand="UPDATE CommitteeDonationTemplate SET Amount = @Amount WHERE DonationTemplateId = @DonationTemplateId"
            InsertCommand="INSERT INTO CommitteeDonationTemplate (SchoolID, RegistrationID, CommitteeMemberTypeId, CommitteeDonationCategoryId, Amount) VALUES (@SchoolID, @RegistrationID, @CommitteeMemberTypeId, @CommitteeDonationCategoryId, @Amount)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
            <DeleteParameters>
                <asp:Parameter Name="DonationTemplateId" Type="Int32" />
            </DeleteParameters>
            <UpdateParameters>
                <asp:Parameter Name="Amount" Type="Decimal" />
                <asp:Parameter Name="DonationTemplateId" Type="Int32" />
            </UpdateParameters>
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:ControlParameter ControlID="MemberTypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="CategoryDropDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="AmountTextBox" Name="Amount" PropertyName="Text" />
            </InsertParameters>
        </asp:SqlDataSource>
    </div>

    <script>
        // Create table if not exists
        $(function () {
            $.ajax({
                url: "DonationTemplates.aspx/CreateTemplateTable",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function () {
                    console.log("Template table checked/created");
                }
            });
        });
    </script>
</asp:Content>
