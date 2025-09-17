<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Print_MemberList.aspx.cs" Inherits="EDUCATION.COM.Committee.Print_MemberList" %>

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
        .form-group {
  margin-bottom: 0rem;
}
h3 {
  padding-top: 3rem !important;
   padding-bottom: 3rem !important;
  border-radius: .25rem;
  margin-top: 3px;
  text-transform: uppercase;
  color: #3e4551;
  font-size: 1rem;
  font-weight: 400;
  background-color: #fff;
  box-shadow: 0 2px 5px 0 rgba(0,0,0,.16),0 2px 10px 0 rgba(0,0,0,.12);
  margin-bottom: 0.5rem !important;
}

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="d-flex justify-content-between align-items-center py-0"> Member List </h3>

    <div class="custom-form-row">


        <div class="form-group NoPrint">
            Member Type
                <asp:DropDownList ID="TypeDropDownList" required="" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId" AutoPostBack="True">
                    <asp:ListItem Value="">[ All Type ]</asp:ListItem>
                </asp:DropDownList>
                        <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>


    </div>

    <div class="table-responsive mt-3">
        <asp:GridView ID="MemberGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="MemberSQL" DataKeyNames="CommitteeMemberId" AllowPaging="True" PageSize="200">
            <Columns>


<%--                <asp:TemplateField HeaderText="Photo">                   
                    <ItemTemplate>
                        <img src="data:image/jpg;base64, <%# Convert.ToBase64String(string.IsNullOrEmpty(Eval("Photo").ToString())? new byte[]{}: (byte[]) Eval("Photo"))  %>" onerror="this.src='/Handeler/Default/Male.png'" class="photo" style="width: 50px" alt="<%#Eval("MemberName") %>" />
                    </ItemTemplate>
                </asp:TemplateField>--%>

                <asp:TemplateField HeaderText="Name" SortExpression="MemberName">
                    <EditItemTemplate>
                        <asp:TextBox ID="MemberNameTB" required="" runat="server" CssClass="form-control" Text='<%# Bind("MemberName") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("MemberName") %>
                    </ItemTemplate>
                </asp:TemplateField>
                   <asp:TemplateField HeaderText="Phone" SortExpression="SmsNumber">
                    <EditItemTemplate>
                        <asp:TextBox ID="SmsNumberTB" required="" runat="server" CssClass="form-control" Text='<%# Bind("SmsNumber") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("SmsNumber") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Address" SortExpression="Address">
                    <EditItemTemplate>
                        <asp:TextBox ID="AddressTB" runat="server" CssClass="form-control" Text='<%# Bind("Address") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("Address") %>
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
                    <asp:TemplateField HeaderText="Reference By" SortExpression="MemberName"> 
                    <EditItemTemplate>
                        <asp:TextBox ID="ReferenceByTB" required="" runat="server" CssClass="form-control" Text='<%# Bind("ReferenceBy") %>'></asp:TextBox>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <%#Eval("ReferenceBy") %>
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
    <asp:SqlDataSource ID="MemberSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="Select *from CommitteeMember WHERE CommitteeMemberTypeId=@CommitteeMemberTypeId And SchoolID=@SchoolID">
        <SelectParameters>
               <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
               <asp:ControlParameter ControlID="TypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    </div>
</asp:Content>
