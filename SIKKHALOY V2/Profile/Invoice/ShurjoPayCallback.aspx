<%@ Page Title="Payment Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="ShurjoPayCallback.aspx.cs" Inherits="EDUCATION.COM.Profile.Invoice.ShurjoPayCallback" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .payment-result-box {
            max-width: 560px;
            margin: 60px auto;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 8px 30px rgba(0,0,0,.15);
        }
        .payment-result-header {
            padding: 30px 20px;
            text-align: center;
            color: #fff;
        }
        .payment-result-header.success { background: linear-gradient(135deg,#28a745,#20c997); }
        .payment-result-header.failed  { background: linear-gradient(135deg,#dc3545,#c0392b); }
        .payment-result-header.pending { background: linear-gradient(135deg,#ffc107,#e67e22); }
        .payment-result-header i { font-size: 64px; margin-bottom: 10px; display: block; }
        .payment-result-body { padding: 30px; background: #fff; }
        .info-table td { padding: 6px 10px; }
        .info-table td:first-child { font-weight: 600; color: #555; white-space: nowrap; }
    </style>
    <script>
        // এই page-এ BASIC.Master-এর session mismatch reload বন্ধ রাখতে হবে
        // কারণ reload হলে payment duplicate process হতে পারে
        window.__disableSessionReload = true;
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="payment-result-box">
        <!-- Header -->
        <div class="payment-result-header <%= HeaderCssClass %>">
            <i class="fa <%= HeaderIcon %>"></i>
            <h3 class="mb-0"><%= HeaderTitle %></h3>
        </div>

        <!-- Body -->
        <div class="payment-result-body">
            <table class="info-table table table-borderless mb-4">
                <tr>
                    <td>অর্ডার আইডি:</td>
                    <td><asp:Label ID="lblOrderId" runat="server" /></td>
                </tr>
                <tr>
                    <td>পরিমাণ:</td>
                    <td><asp:Label ID="lblAmount" runat="server" /></td>
                </tr>
                <tr>
                    <td>পেমেন্ট পদ্ধতি:</td>
                    <td><asp:Label ID="lblMethod" runat="server" /></td>
                </tr>
                <tr>
                    <td>ট্রানজেকশন আইডি:</td>
                    <td><asp:Label ID="lblTrxId" runat="server" /></td>
                </tr>
                <tr>
                    <td>তারিখ:</td>
                    <td><asp:Label ID="lblDate" runat="server" /></td>
                </tr>
                <tr>
                    <td>স্ট্যাটাস:</td>
                    <td><asp:Label ID="lblStatus" runat="server" /></td>
                </tr>
            </table>

            <asp:Label ID="lblMessage" runat="server" CssClass="alert d-block text-center" />

            <div class="text-center mt-3">
                <a href="Due_Invoice.aspx" class="btn btn-outline-primary mr-2">
                    <i class="fa fa-file-invoice-dollar"></i> Invoice দেখুন
                </a>
                <a href="/Profile/Admin.aspx" class="btn btn-primary">
                    <i class="fa fa-home"></i> Dashboard
                </a>
            </div>
        </div>
    </div>
</asp:Content>
