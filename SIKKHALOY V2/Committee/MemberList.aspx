<%@ Page Title="View Member List" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="MemberList.aspx.cs" Inherits="EDUCATION.COM.Committee.MemberList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .custom-form-row {
            display: flex;
            -ms-flex-wrap: wrap;
            flex-wrap: wrap;
            gap: 20px
        }

        .photo {
            width: 50px;
            border-radius: 5px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Member List</h3>

    <div class="d-flex align-items-center">
        <div class="form-group">
            <label>Donor Type</label>
            <asp:DropDownList ID="DonorTypeDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="SqlCommitteeDonorType" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                <asp:ListItem Value="%">[ ALL Type ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="SqlCommitteeDonorType" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
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

        <div class="form-group ml-3">
            <label>Name</label>
            <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" required=""></asp:TextBox>
        </div>

        <div class="form-group ml-3" style="padding-top: 1.8rem">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-outline-primary btn-md" Text="Find" />
        </div>
        <div class="form-group ml-3" style="padding-top: 1.8rem">
            <input id="PrintButton" type="button" value="Print" onclick="window.print();" class="btn btn-info" />
        </div>
    </div>




    <div class="table-responsive mt-2">
        <asp:GridView ID="MemberGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="MemberSQL" DataKeyNames="CommitteeMemberId" AllowPaging="True">
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
            SelectCommand="SELECT CommitteeMember.CommitteeMemberId, CommitteeMemberType.CommitteeMemberType,CommitteeMemberType.CommitteeMemberTypeId, CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMember.Photo, CommitteeMember.TotalDonation, CommitteeMember.PaidDonation, CommitteeMember.DueDonation, CommitteeMember.InsertDate FROM CommitteeMember 
INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId 
INNER JOIN CommitteeDonation ON CommitteeMember.CommitteeMemberId = CommitteeDonation.CommitteeMemberId 
INNER JOIN CommitteeDonationCategory ON CommitteeDonation.CommitteeDonationCategoryId = CommitteeDonationCategory.CommitteeDonationCategoryId
            WHERE (CommitteeMember.SchoolID = @SchoolID)
                        AND (CommitteeMemberType.CommitteeMemberTypeId LIKE @CommitteeMemberTypeId)
                        AND (CommitteeDonationCategory.CommitteeDonationCategoryId LIKE @CommitteeDonationCategoryId)" CancelSelectOnNullParameter="False">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="DonorTypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="DonationCategoryDropDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>


</asp:Content>
