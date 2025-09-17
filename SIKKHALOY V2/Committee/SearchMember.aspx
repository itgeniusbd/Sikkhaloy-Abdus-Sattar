<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="SearchMember.aspx.cs" Inherits="EDUCATION.COM.Committee.SearchMember" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .custom-form-row { display: flex; -ms-flex-wrap: wrap; flex-wrap: wrap; gap: 20px }
        .photo{width:50px;border-radius: 5px;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <%--<h3 class="d-flex justify-content-between align-items-center py-0">Add Member
         <a class="btn btn-info d-print-none" href="SearchMember.aspx" style="margin-left: 45%">Search Member</a>
        <a class="btn btn-dark d-print-none" href="MemberType.aspx">Add Member Type</a>
    </h3>--%>

    <div class="custom-form-row">
        <div class="form-group">
            <label>Donor Type</label>
            <asp:DropDownList ID="CommitteeMemberDropDownList" required="" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                <asp:ListItem Value="%">[ All Type ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group ml-3">
            <label>Donation Type</label>
            <asp:DropDownList ID="DonationCategoryDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="SqlCommitteeDonationCategory" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId">
                <asp:ListItem Value="%">[ ALL Type ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="SqlCommitteeDonationCategory" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <label>Name/Phone</label>
            <asp:TextBox ID="NamePhoneTextBox" required="" runat="server" CssClass="form-control"></asp:TextBox>
        </div>
        <div class="form-group ml-3" style="padding-top: 1.8rem">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-outline-primary btn-md" Text="Find" />
        </div>
        <div class="form-group ml-3" style="padding-top: 1.8rem">
            <input id="PrintButton" type="button" value="Print" onclick="window.print();" class="btn btn-info" />
        </div>
    </div>

    <div class="table-responsive mt-3">
        <asp:GridView ID="MemberGridView" AllowSorting="True" AllowPaging="True" PageSize="50" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataSourceID="MemberSQL" DataKeyNames="CommitteeMemberId">
            <Columns>
                <asp:TemplateField HeaderText="Photo">
                    <ItemTemplate>
                        <img src="data:image/jpg;base64, <%# Convert.ToBase64String(string.IsNullOrEmpty(Eval("Photo").ToString())? new byte[]{}: (byte[]) Eval("Photo"))  %>" onerror="this.src='/Handeler/Default/Male.png'" class="photo" alt="<%#Eval("MemberName") %>" />
                    </ItemTemplate>                  
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Name" SortExpression="MemberName">
                    <ItemTemplate>
                        <%#Eval("MemberName") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Type" SortExpression="CommitteeMemberType">
                    <ItemTemplate>
                        <%#Eval("CommitteeMemberType") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Address" SortExpression="Address">
                    <ItemTemplate>
                        <%#Eval("Address") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Phone" SortExpression="SmsNumber">
                    <ItemTemplate>
                        <%#Eval("SmsNumber") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Total Donation" SortExpression="TotalDonation">
                    <ItemTemplate>
                        <%#Eval("TotalDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Paid Donation" SortExpression="PaidDonation">
                    <ItemTemplate>
                        <%#Eval("PaidDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Due Donation" SortExpression="DueDonation">
                    <ItemTemplate>
                        <%#Eval("DueDonation") %>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
        <asp:SqlDataSource ID="MemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
            SelectCommand="SELECT DISTINCT(CommitteeMember.CommitteeMemberId), CommitteeMemberType.CommitteeMemberTypeId, CommitteeMemberType.CommitteeMemberType,
 CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMember.Photo, CommitteeMember.TotalDonation, CommitteeMember.PaidDonation, CommitteeMember.DueDonation, CommitteeMember.InsertDate 
			FROM CommitteeMember 
			INNER JOIN CommitteeMemberType 
			ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId 
			INNER JOIN CommitteeDonation
			ON CommitteeDonation.CommitteeMemberId = CommitteeMember.CommitteeMemberId
			INNER JOIN CommitteeDonationCategory
			ON CommitteeDonationCategory.CommitteeDonationCategoryId = CommitteeDonation.CommitteeDonationCategoryId
            WHERE (CommitteeMember.SchoolID = @SchoolID)
            AND CommitteeMemberType.CommitteeMemberTypeId LIKE @CommitteeMemberTypeId
            AND CommitteeDonationCategory.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId
            AND (CommitteeMember.SmsNumber LIKE ISNULL(@NamePhoneTextBox, '%') OR CommitteeMember.MemberName LIKE ISNULL(@NamePhoneTextBox, '%'))"
            CancelSelectOnNullParameter="False">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="CommitteeMemberDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="DonationCategoryDropDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="NamePhoneTextBox" Name="NamePhoneTextBox" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>
</asp:Content>
