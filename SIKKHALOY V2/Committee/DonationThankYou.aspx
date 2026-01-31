<%@ Page Title="Donation Successful" Language="C#" MasterPageFile="~/Basic_Donor.Master" AutoEventWireup="true" ResponseEncoding="utf-8" Culture="auto" UICulture="auto" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <style>
        .thank-you-card {
            background: #fff;
            border-radius: 15px;
            padding: 40px;
            text-align: center;
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
            max-width: 600px;
            margin: 50px auto;
        }
        .icon-box {
            width: 100px;
            height: 100px;
            background: #e6f9f0;
            border-radius: 50%;
            margin: 0 auto 30px;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .icon-box i {
            font-size: 50px;
            color: #28a745;
        }
        .btn-dashboard {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #fff;
            padding: 12px 35px;
            border-radius: 50px;
            font-weight: bold;
            text-decoration: none;
            display: inline-block;
            margin-top: 20px;
            transition: all 0.3s;
        }
        .btn-dashboard:hover {
            transform: translateY(-3px);
            box-shadow: 0 5px 15px rgba(118, 75, 162, 0.4);
            color: #fff;
            text-decoration: none;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="thank-you-card">
        <div class="icon-box">
            <i class="fa fa-check"></i>
        </div>
        <h2 class="mb-3 text-success">Donation Successful!</h2>
        <p class="text-muted mb-4">
            Thank you for your generous donation. Your payment has been processed successfully.
        </p>
        <p class="mb-4">
            <small>আপনার উদার দানের জন্য ধন্যবাদ। আপনার দান সফলভাবে প্রক্রিয়া করা হয়েছে।</small>
        </p>
        
        <a href="Donor_Dues.aspx" class="btn-dashboard">
            <i class="fa fa-dashboard mr-2"></i>Back to Dashboard
        </a>
    </div>
</asp:Content>
