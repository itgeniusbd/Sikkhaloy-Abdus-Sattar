<%@ Page Title="Donor SMS Inbox" Language="C#" MasterPageFile="~/Basic_Donor.Master" AutoEventWireup="true" CodeBehind="Donor_SMS_Inbox.aspx.cs" Inherits="EDUCATION.COM.Committee.Donor_SMS_Inbox" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>SMS Inbox</h3>
    <div class="card">
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="SMSGridView" AllowPaging="true" PageSize="30" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="SMSRecordSQL">
                    <Columns>
                        <asp:BoundField DataField="PhoneNumber" HeaderText="Phone" />
                        <asp:BoundField DataField="TextSMS" HeaderText="SMS Content" />
                        <asp:BoundField DataField="PurposeOfSMS" HeaderText="Purpose" />
                        <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:g}" />
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="p-3 text-center">No SMS records found.</div>
                    </EmptyDataTemplate>
                    <PagerStyle CssClass="pgr" />
                </asp:GridView>
                <asp:SqlDataSource ID="SMSRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                    SelectCommand="SELECT R.PhoneNumber, R.TextSMS, R.Date, R.PurposeOfSMS 
                                   FROM SMS_Send_Record R 
                                   INNER JOIN SMS_OtherInfo O ON R.SMS_Send_ID = O.SMS_Send_ID 
                                   INNER JOIN CommitteeMember CM ON O.CommitteeMemberId = CM.CommitteeMemberId
                                   INNER JOIN Registration REG ON REG.SchoolID = CM.SchoolID AND REG.CommitteeMemberId = CM.CommitteeMemberId
                                   WHERE REG.RegistrationID = @RegistrationID AND O.SchoolID = @SchoolID
                                   ORDER BY R.Date DESC">
                    <SelectParameters>
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>
    </div>
</asp:Content>
