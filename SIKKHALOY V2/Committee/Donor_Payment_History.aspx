<%@ Page Title="Donor Payment History" Language="C#" MasterPageFile="~/Basic_Donor.Master" AutoEventWireup="true" CodeBehind="Donor_Payment_History.aspx.cs" Inherits="EDUCATION.COM.Committee.Donor_Payment_History" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="card">
        <div class="card-header bg-white font-weight-bold d-flex justify-content-between">
            Your Payment History
            <input type="button" value="Print" onclick="window.print();" class="btn btn-sm btn-info" />
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="PaymentHistoryGV" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="HistorySQL">
                    <Columns>
                        <asp:BoundField DataField="CommitteeMoneyReceiptSn" HeaderText="Receipt #" />
                        <asp:BoundField DataField="PaidDate" HeaderText="Date" DataFormatString="{0:d MMM yyyy}" />
                        <asp:BoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="?{0}" />
                        <asp:BoundField DataField="AccountName" HeaderText="Paid To" />
                        <asp:TemplateField HeaderText="Details">
                            <ItemTemplate>
                                <a href="DonationReceipt.aspx?id=<%# Eval("CommitteeMoneyReceiptId") %>" target="_blank" class="btn btn-sm btn-outline-primary">View</a>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="p-3 text-center">No payment history found.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>

    <asp:SqlDataSource ID="HistorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CMR.CommitteeMoneyReceiptId, CMR.CommitteeMoneyReceiptSn, CMR.PaidDate, CMR.TotalAmount, A.AccountName 
                       FROM CommitteeMoneyReceipt CMR 
                       LEFT JOIN Account A ON CMR.AccountId = A.AccountID
                       INNER JOIN CommitteeMember CM ON CMR.CommitteeMemberId = CM.CommitteeMemberId
                       INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.UserName = CM.SmsNumber
                       WHERE R.RegistrationID = @RegistrationID AND CMR.SchoolId = @SchoolID
                       ORDER BY CMR.PaidDate DESC">
        <SelectParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
