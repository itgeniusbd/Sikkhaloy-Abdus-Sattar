<%@ Page Title="Inbox" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Message_List.aspx.cs" Inherits="EDUCATION.COM.Authority.ContactUs.Message_List" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid td { text-align: left; padding: 5px 10px; }
        .mGrid tr td a { color: #000; cursor:default}
        .UnRead { background-color: #f2f2f2; }
        .UnRead td { color: #000; font-weight: bold; }
        .UnRead td a { font-weight: bold; color: #0036b6 !important; cursor:pointer !important }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <ul class="nav nav-tabs nav-justified z-depth-1">
        <li class="nav-item"><a class="nav-link active" data-toggle="tab" role="tab" href="#Tab1">Support</a></li>
        <li class="nav-item"><a class="nav-link" data-toggle="tab" role="tab" href="#Tab2">Contact</a></li>
    </ul>

    <div class="tab-content card">
        <div class="tab-pane fade in show active" role="tabpanel" id="Tab1">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <asp:GridView ID="SupportGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="SupportID" DataSourceID="SupportSQl">
                        <Columns>
                            <asp:TemplateField HeaderText="Institution" SortExpression="SchoolName">
                                <ItemTemplate>
                                    <asp:LinkButton ID="SupportLinkButton" Enabled='<%#!(bool)Eval("Is_Read") %>' OnCommand="SupportLinkButton_Command" CommandName='<%#Eval("SupportID") %>' Text='<%# Bind("SchoolName") %>' runat="server" />
                                    <input class="Is_Read" type="hidden" value="<%#Eval("Is_Read") %>" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:BoundField DataField="Support_Title" HeaderText="Subject" SortExpression="Support_Title" />
                            <asp:BoundField DataField="Message" HeaderText="Message" SortExpression="Message" />
                            <asp:BoundField DataField="Sent_Date" HeaderText="Date" SortExpression="Sent_Date" DataFormatString="{0:d MMM yy (hh:mm tt)}" />
                        </Columns>
                    </asp:GridView>
                    <asp:SqlDataSource ID="SupportSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Public_Support.SupportID, Public_Support_Title.Support_Title, Admin.FirstName + ' ' + ISNULL(Admin.LastName, '') AS Name, SchoolInfo.SchoolName, Public_Support.Message, Public_Support.Sent_Date, Public_Support.Is_Read FROM Public_Support INNER JOIN Public_Support_Title ON Public_Support.SupportTitleID = Public_Support_Title.SupportTitleID INNER JOIN SchoolInfo ON Public_Support.SchoolID = SchoolInfo.SchoolID INNER JOIN Admin ON Public_Support.RegistrationID = Admin.RegistrationID ORDER BY Public_Support.Sent_Date DESC" UpdateCommand="UPDATE Public_Support SET Is_Read = 1 WHERE (SupportID = @SupportID)">
                        <UpdateParameters>
                            <asp:Parameter Name="SupportID" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="tab-pane fade" role="tabpanel" id="Tab2">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:GridView ID="MessageGridView" CssClass="mGrid table-hover" runat="server" DataSourceID="MsgSQL" AutoGenerateColumns="False" DataKeyNames="ContactUsID" AllowPaging="True" PageSize="100">
                        <Columns>
                            <asp:TemplateField HeaderText="Subject" SortExpression="Subject">
                                <ItemTemplate>
                                    <asp:LinkButton ID="ContactLinkButton" Enabled='<%#!(bool)Eval("Is_Read") %>' OnCommand="ContactLinkButton_Command" CommandName='<%#Eval("ContactUsID") %>' Text='<%# Bind("Subject") %>' runat="server" />
                                    <input class="Is_Read" type="hidden" value="<%#Eval("Is_Read") %>" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                            <asp:BoundField DataField="MobileNo" HeaderText="Mobile" SortExpression="MobileNo" />
                            <asp:BoundField DataField="Message" HeaderText="Message" SortExpression="Message" />
                            <asp:BoundField DataField="Sent_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Sent_Date" />
                            <asp:CommandField ShowDeleteButton="True" />
                        </Columns>
                        <PagerStyle CssClass="pgr" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="MsgSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ContactUsID, Name, Email, MobileNo, Subject, Message, Sent_Date, Is_Read FROM Public_Contact_US ORDER BY ContactUsID DESC" UpdateCommand="UPDATE Public_Contact_US SET Is_Read = @Is_Read WHERE (ContactUsID = @ContactUsID)" DeleteCommand="DELETE FROM Public_Contact_US WHERE (ContactUsID = @ContactUsID)">
                        <DeleteParameters>
                            <asp:Parameter Name="ContactUsID" />
                        </DeleteParameters>
                        <UpdateParameters>
                            <asp:Parameter DefaultValue="1" Name="Is_Read" />
                            <asp:Parameter Name="ContactUsID" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>


    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="/CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script>
        $(function () {
            $('.mGrid tr').each(function () {
                if ($(this).find('.Is_Read').val() === "False") {
                    $(this).addClass("UnRead");
                }
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $('.mGrid tr').each(function () {
                if ($(this).find('.Is_Read').val() === "False") {
                    $(this).addClass("UnRead");
                }
            });
        });
    </script>
</asp:Content>
