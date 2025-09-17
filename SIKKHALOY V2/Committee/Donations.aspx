<%@ Page Title="Donations" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Donations.aspx.cs" Inherits="EDUCATION.COM.Committee.Donations" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="d-flex justify-content-between align-items-center py-0">Donations
        <a class="btn btn-dark d-print-none" href="DonationAdd.aspx">Add Donation</a>
    </h3>

    <div class="d-flex align-items-center">
        <div class="form-group">
            <label>Committee Member</label>
            <asp:DropDownList ID="CommitteeMemberDropDownList" required="" runat="server" AppendDataBoundItems="True" AutoPostBack="true" CssClass="form-control" DataSourceID="CommitteeMemberSQL" DataTextField="MemberName" DataValueField="CommitteeMemberId">
                <asp:ListItem Value="">[ All ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="CommitteeMemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMemberId, MemberName FROM CommitteeMember WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group mx-4">
            <label>Donation Category</label>
            <asp:DropDownList ID="DonationCategoryDownList" required="" runat="server" AppendDataBoundItems="True" AutoPostBack="true" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId">
                <asp:ListItem Value="">[ All ]</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="form-group">
            <label>Payment Status</label>
            <asp:RadioButtonList ID="PDRadioButtonList" CssClass="form-control" runat="server" RepeatDirection="Horizontal" AutoPostBack="True">
                <asp:ListItem Selected="True" Value="%">All</asp:ListItem>
                <asp:ListItem Value="1">Paid</asp:ListItem>
                <asp:ListItem Value="0">Due</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>

    <asp:FormView ID="TotalFormView" runat="server" DataSourceID="TotalSQL" RenderOuterTable="false">
        <ItemTemplate>
            <div class="d-flex align-items-center font-weight-bold">
                <div>
                    Total: ৳<%# Eval("Total") %></div>
                <div class="mx-3">
                    Paid: ৳<%# Eval("Paid") %></div>
                <div>
                    Due: ৳<%# Eval("Due") %></div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TotalSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SUM(ISNULL(Amount, 0)) AS Total, SUM(ISNULL(PaidAmount, 0)) AS Paid, SUM(ISNULL(Due, 0)) AS Due FROM CommitteeDonation WHERE (SchoolID = @SchoolID) AND (IsPaid LIKE @IsPaid)"
        CancelSelectOnNullParameter="False">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="PDRadioButtonList" Name="IsPaid" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>


    <div class="table-responsive mt-2">
        <asp:GridView ID="DonationGridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="CommitteeDonationId" DataSourceID="AddDonationSQL">
            <Columns>
                <asp:TemplateField HeaderText="Name" SortExpression="MemberName">
                    <ItemTemplate>
                        <strong class="d-block"><%# Eval("MemberName") %></strong>
                        <small><%# Eval("CommitteeMemberType") %></small>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="SmsNumber" HeaderText="Sms Number" ReadOnly="True" SortExpression="SmsNumber" />
                <asp:TemplateField HeaderText="Donation Category" SortExpression="Amount">
                    <EditItemTemplate>
                        <asp:DropDownList ID="EditCategoryDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId" SelectedValue='<%# Bind("CommitteeDonationCategoryId") %>'>
                        </asp:DropDownList>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%# Eval("DonationCategory") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Donation Amount" SortExpression="Amount">
                    <EditItemTemplate>
                        <asp:TextBox ID="TextBox3" CssClass="form-control" runat="server" Text='<%# Bind("Amount") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%# Eval("Amount") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Paid Amount" SortExpression="PaidAmount">
                    <ItemTemplate>
                        <%# Eval("PaidAmount") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Due" SortExpression="Due">
                    <ItemTemplate>
                        <%# Eval("Due") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description" SortExpression="Description">
                    <EditItemTemplate>
                        <asp:TextBox ID="TextBox1" CssClass="form-control" runat="server" Text='<%# Bind("Description") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%# Eval("Description") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Add Date" SortExpression="InsertDate">
                    <ItemTemplate>
                        <%# Eval("InsertDate", "{0:d MMM yyyy}") %>
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
                    <HeaderStyle CssClass="d-print-none" />
                    <ItemStyle CssClass="d-print-none" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Delete" ShowHeader="False">
                    <ItemTemplate>
                        <asp:LinkButton ID="DeleteLinkButton" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"></asp:LinkButton>
                    </ItemTemplate>
                    <HeaderStyle CssClass="d-print-none" />
                    <ItemStyle CssClass="d-print-none" />
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <asp:SqlDataSource ID="AddDonationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeDonation.CommitteeDonationId, CommitteeDonation.CommitteeDonationCategoryId, CommitteeDonationCategory.DonationCategory, CommitteeDonation.Amount, CommitteeDonation.PaidAmount, CommitteeDonation.Due, CommitteeDonation.IsPaid, CommitteeDonation.Description, CommitteeDonation.InsertDate, CommitteeDonation.PromiseDate, CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMemberType.CommitteeMemberType FROM CommitteeDonation INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId INNER JOIN CommitteeMember ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId WHERE (CommitteeDonation.SchoolID = @SchoolID) AND (CommitteeDonation.CommitteeMemberId LIKE @CommitteeMemberId) AND (CommitteeDonation.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId) AND (CommitteeDonation.IsPaid LIKE @IsPaid)"
        DeleteCommand="DELETE FROM CommitteeDonation WHERE (CommitteeDonationId = @CommitteeDonationId) AND (PaidAmount = 0)"
        UpdateCommand="UPDATE CommitteeDonation SET CommitteeDonationCategoryId = @CommitteeDonationCategoryId, Amount = CASE WHEN PaidAmount &gt; @Amount THEN Amount ELSE @Amount END, Description = @Description WHERE (CommitteeDonationId = @CommitteeDonationId)">
        <DeleteParameters>
            <asp:Parameter Name="CommitteeDonationId" />
        </DeleteParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:ControlParameter ControlID="CommitteeMemberDropDownList" DefaultValue="%" Name="CommitteeMemberId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="DonationCategoryDownList" DefaultValue="%" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="PDRadioButtonList" Name="IsPaid" PropertyName="SelectedValue" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="CommitteeDonationCategoryId" />
            <asp:Parameter Name="Amount" />
            <asp:Parameter Name="Description" />
            <asp:Parameter Name="CommitteeDonationId" />
        </UpdateParameters>
    </asp:SqlDataSource>
    <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            //remove donation add saved record
            sessionStorage.removeItem("_committee_");
        });
    </script>
</asp:Content>
