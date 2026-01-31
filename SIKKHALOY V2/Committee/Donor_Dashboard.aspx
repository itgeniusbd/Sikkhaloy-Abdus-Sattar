<%@ Page Title="Donor Dashboard" Language="C#" MasterPageFile="~/Basic_Donor.Master" AutoEventWireup="true" CodeBehind="Donor_Dashboard.aspx.cs" Inherits="EDUCATION.COM.Committee.Donor_Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <style>
        .stat-card { 
            background: #fff; 
            border-radius: 10px; 
            padding: 20px; 
            box-shadow: 0 4px 6px rgba(0,0,0,0.1); 
            text-align: center !important; 
        }
        .stat-card i { 
            font-size: 2.5rem; 
            margin-bottom: 15px;
            text-align: center !important; 
        }
        .stat-card .amount { 
            font-size: 1.8rem; 
            font-weight: bold; 
            display: block;
            text-align: center !important; 
            margin: 0 auto; 
        }
        .stat-card .label { 
            color: #777; 
            text-transform: uppercase; 
            font-size: 0.9rem; 
            text-align: center !important;
        }
        .stat-card table { 
            width: 100%; 
            text-align: center !important; 
        }
        .stat-card td { 
            text-align: center !important; 
        }

        table td {
            font-size: .9rem;
            font-weight: 300;
            text-align: center;
        }
        tbody {
            font-size: .9rem;
            font-weight: 300;
            text-align: center;
        }
        tr span {
            font-size: .9rem;
            font-weight: 300;
            text-align: center;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="font-weight-bold mb-4">
        <i class="fa fa-money text-success mr-2"></i>DONOR DESHBOARD
    </h3>

    <div class="row">
        <div class="col-md-4 mb-4">
            <div class="stat-card">
                <i class="fa fa-exclamation-triangle text-danger"></i>
                <asp:FormView ID="DueDonationFV" runat="server" DataSourceID="SummarySQL">
                    <ItemTemplate>
                        <span class="amount">৳<%# Eval("DueDonation") %></span>
                    </ItemTemplate>
                </asp:FormView>
                <span class="label">Due Amount</span>
                <div class="mt-3">
                    <a href="Donor_Dues.aspx" class="btn btn-danger btn-lg">
                        <i class="fa fa-credit-card mr-2"></i>PAY NOW
                    </a>
                </div>
            </div>
        </div>

        <div class="col-md-4 mb-4">
            <div class="stat-card">
                <i class="fa fa-check-circle text-success"></i>
                <asp:FormView ID="PaidDonationFV" runat="server" DataSourceID="SummarySQL">
                    <ItemTemplate>
                        <span class="amount">৳<%# Eval("PaidDonation") %></span>
                    </ItemTemplate>
                </asp:FormView>
                <span class="label">Paid Amount</span>
            </div>
        </div>

        <div class="col-md-4 mb-4">
            <div class="stat-card">
                <i class="fa fa-money text-primary"></i>
                <asp:FormView ID="TotalDonationFV" runat="server" DataSourceID="SummarySQL">
                    <ItemTemplate>
                        <span class="amount">৳<%# Eval("TotalDonation") %></span>
                    </ItemTemplate>
                </asp:FormView>
                <span class="label">Total Donation</span>
            </div>
        </div>
    </div>

    <div class="card mt-2">
        <div class="card-header bg-white font-weight-bold text-success">
            <i class="fa fa-check-circle mr-1"></i>Your Paid Donations
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <asp:GridView ID="PaidDonationsGV" runat="server" AutoGenerateColumns="False" CssClass="table table-hover mb-0" DataSourceID="PaidDonationsSQL">
                    <Columns>
                        <asp:BoundField DataField="CommitteeMoneyReceiptSn" HeaderText="Receipt #" />
                        <asp:BoundField DataField="PaidDate" HeaderText="Date" DataFormatString="{0:d MMM yyyy}" />
                        <asp:BoundField DataField="TotalAmount" HeaderText="Paid Amount" DataFormatString="৳{0}" ItemStyle-CssClass="font-weight-bold text-success" />
                        <asp:BoundField DataField="AccountName" HeaderText="Paid To" />
                        <asp:TemplateField HeaderText="Details">
                            <ItemTemplate>
                                <a href="DonationReceipt.aspx?id=<%# Eval("CommitteeMoneyReceiptId") %>" target="_blank" class="btn btn-sm btn-link py-0"><i class="fa fa-print"></i> RECEIPT</a>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="p-3 text-center text-muted">No payment records found.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>
    </div>

    <asp:SqlDataSource ID="SummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CM.CommitteeMemberId, CM.MemberName, CM.Photo, CM.SmsNumber, CM.Address, CMT.CommitteeMemberType, CM.TotalDonation, CM.PaidDonation, CM.DueDonation 
                       FROM CommitteeMember CM 
                       INNER JOIN CommitteeMemberType CMT ON CM.CommitteeMemberTypeId = CMT.CommitteeMemberTypeId
                       INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.CommitteeMemberId = CM.CommitteeMemberId
                       WHERE R.RegistrationID = @RegistrationID AND R.SchoolID = @SchoolID">
        <SelectParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:SqlDataSource ID="PaidDonationsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT TOP 5 CMR.CommitteeMoneyReceiptId, CMR.CommitteeMoneyReceiptSn, CMR.PaidDate, CMR.TotalAmount, A.AccountName 
                       FROM CommitteeMoneyReceipt CMR 
                       LEFT JOIN Account A ON CMR.AccountId = A.AccountID
                       INNER JOIN CommitteeMember CM ON CMR.CommitteeMemberId = CM.CommitteeMemberId
                       INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.CommitteeMemberId = CM.CommitteeMemberId
                       WHERE R.RegistrationID = @RegistrationID AND CMR.SchoolId = @SchoolID
                       ORDER BY CMR.PaidDate DESC">
        <SelectParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
