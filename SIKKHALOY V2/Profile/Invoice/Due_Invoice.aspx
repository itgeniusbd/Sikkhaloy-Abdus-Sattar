<%@ Page Title="Due Invoice" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Due_Invoice.aspx.cs" Inherits="EDUCATION.COM.Profile.Invoice.Due_Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../css/Invoice.css?v=1.0.5" rel="stylesheet" />
    <style>
        .shurjopay-btn {
            background: linear-gradient(135deg, #f7971e, #ffd200);
            color: #222;
            border: none;
            font-weight: 700;
            padding: 10px 24px;
            border-radius: 8px;
            font-size: 1.05rem;
            cursor: pointer;
            box-shadow: 0 4px 14px rgba(247,151,30,.4);
            transition: transform .15s;
        }
        .shurjopay-btn:hover { transform: translateY(-2px); color: #222; }
        .shurjopay-btn img { height: 22px; margin-right: 6px; vertical-align: middle; }
        .online-pay-section {
            background: #fff8e1;
            border: 2px dashed #ffc107;
            border-radius: 10px;
            padding: 18px 20px;
            margin: 18px 0;
            text-align: center;
        }
        /* Subscription Expired Modal */
        .sub-modal-overlay {
            display: none;
            position: fixed;
            z-index: 99999;
            left: 0; top: 0;
            width: 100%; height: 100%;
            background: rgba(0,0,0,0.65);
            justify-content: center;
            align-items: center;
        }
        .sub-modal-overlay.active { display: flex; }
        .sub-modal-box {
            background: #fff;
            border-radius: 14px;
            box-shadow: 0 8px 40px rgba(0,0,0,0.28);
            max-width: 480px;
            width: 95%;
            padding: 0;
            overflow: hidden;
            animation: subModalIn .25s ease;
        }
        @keyframes subModalIn {
            from { transform: translateY(-40px); opacity: 0; }
            to   { transform: translateY(0);    opacity: 1; }
        }
        .sub-modal-header {
            background: #c0392b;
            color: #fff;
            padding: 18px 22px 14px;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .sub-modal-header h4 { margin: 0; font-size: 1.15rem; }
        .sub-modal-close {
            background: none;
            border: none;
            color: #fff;
            font-size: 1.4rem;
            line-height: 1;
            cursor: pointer;
            opacity: .85;
            padding: 0 0 0 12px;
        }
        .sub-modal-close:hover { opacity: 1; }
        .sub-modal-body { padding: 20px 22px 10px; }
        .sub-modal-body p { font-size: 1rem; margin-bottom: 10px; color: #333; }
        .sub-due-amount {
            background: #fff3cd;
            border: 1px solid #ffc107;
            border-radius: 8px;
            padding: 10px 16px;
            margin: 12px 0 16px;
            text-align: center;
            font-size: 1.1rem;
            font-weight: 700;
            color: #856404;
        }
        .sub-modal-footer {
            padding: 12px 22px 18px;
            display: flex;
            flex-direction: column;
            gap: 10px;
        }
        @media print {
            body * {
                visibility: hidden;
            }
            .invoice-wraper, .invoice-wraper * {
                visibility: visible;
            }
            .invoice-wraper {
                position: absolute;
                left: 0;
                top: 0;
                width: 100%;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

<%-- Subscription Expired Modal --%>
<div class="sub-modal-overlay d-print-none" id="subExpiredModal">
    <div class="sub-modal-box">
        <div class="sub-modal-header" id="subModalHeader">
            <h4 id="subModalTitle">
                <i class="fa fa-exclamation-triangle"></i>
                &nbsp;<span id="subModalTitleText">সফটওয়্যার অ্যাক্সেস সাময়িকভাবে বন্ধ আছে</span>
            </h4>
            <button type="button" class="sub-modal-close" onclick="document.getElementById('subExpiredModal').classList.remove('active');" title="বন্ধ করুন">&times;</button>
        </div>
        <div class="sub-modal-body">
            <p id="subModalMessage">
                আপনার সাবস্ক্রিপশনের মেয়াদ শেষ হওয়াতে সফটওয়্যার ব্যবহারের
                অ্যাক্সেস বন্ধ আছে। দয়া করে অনলাইনে পেমেন্ট করে সাবস্ক্রিপশন রিনিউ করুন।
            </p>
            <div class="sub-due-amount" id="subDueAmountBox" style="display:none;">
                <i class="fa fa-money"></i>
                &nbsp;মোট বকেয়া: <span id="subDueAmountText"></span> টাকা
            </div>
        </div>
        <div class="sub-modal-footer">
            <asp:Button ID="btnShurjoPayModal" runat="server"
                CssClass="shurjopay-btn"
                Text="ShurjoPay দিয়ে এখনই পেমেন্ট করুন"
                OnClick="btnShurjoPay_Click"
                UseSubmitBehavior="true"
                style="width:100%; font-size:1.05rem; padding:12px;" />
            <a href="/Profile/Support/Support_Ticket.aspx"
               class="btn btn-warning btn-block"
               style="width:100%; text-align:center; padding:10px; font-weight:600; border-radius:8px;">
                <i class="fa fa-ticket"></i> &nbsp;সাপোর্ট টিকেট করুন
            </a>
            <small class="text-muted text-center d-block" style="font-size:.82rem;">
                <img src="/CSS/Image/shurjopay_logo.png" onerror="this.style.display='none'" alt="ShurjoPay" style="height:15px;" />
                Powered by ShurjoPay &bull; Secure Payment
            </small>
        </div>
    </div>
</div>
<asp:HiddenField ID="hfDueAmount" runat="server" />
<asp:HiddenField ID="hfIsBlocked" runat="server" />
<asp:HiddenField ID="hfDaysLeft" runat="server" />

<div class="d-print-none">
    <a onclick="window.print();" class="btn btn-sm btn-green">Print</a>
</div>

    <asp:FormView ID="PrintFormView" runat="server" DataSourceID="InvoiceSQL" RenderOuterTable="false" OnDataBound="PrintFormView_DataBound">
        <ItemTemplate>
            <div class="invoice-wraper">
                <div class="Inst_Name">
                    <div>
                        <img src="/CSS/Image/Sikkhaloy_Icon.png" />
                    </div>
                    <div>
                        <h2>SIKKHALOY</h2>
                        <small>Educational institution management service</small>
                    </div>
                </div>

                <div class="Ititle">
                    <b>INVOICE</b>
                </div>

                <div class="invoice-to my-3">
                    <h2>INVOICE TO:</h2>
                    <h5><i class="fa fa-user" aria-hidden="true"></i>
                        <%#Eval("SchoolName") %></h5>
                    <p>
                        <i class="fa fa-map-marker" aria-hidden="true"></i>
                        <%#Eval("Address") %>
                    </p>
                    <p>
                        <i class="fa fa-phone" aria-hidden="true"></i>
                        <%#Eval("Phone") %>
                    </p>
                </div>

                <div class="details-list">
                    <asp:Repeater ID="DetailsRepeater" runat="server" DataSourceID="DetailsSQL" OnItemDataBound="DetailsRepeater_ItemDataBound" OnPreRender="DetailsRepeater_PreRender">
                        <HeaderTemplate>
                            <table class="invoice-table">
                                <thead>
                                    <tr>
                                        <th>SN</th>
                                        <th>Description</th>
                                        <th class="text-right">Unit</th>
                                        <th class="text-right">Unit Price</th>
                                        <th class="text-right">Line Total</th>
                                        <th class="text-right">Discount</th>
                                        <th class="text-right">Paid</th>
                                        <th class="text-right">Due</th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%#(((RepeaterItem)Container).ItemIndex+1).ToString()%></td>
                                <td><%# Eval("InvoiceCategory") %> (<%#Eval("Invoice_For") %>)</td>
                                <td class="text-right"><%# Eval("Unit") %></td>
                                <td class="text-right"><%# Eval("UnitPrice") %></td>
                                <td class="text-right"><%# Eval("TotalAmount") %></td>
                                <td class="text-right"><%# Eval("Discount") %></td>
                                <td class="text-right"><%# Eval("PaidAmount") %></td>
                                <td class="text-right"><%# Eval("Due") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                   </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:SqlDataSource ID="DetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AAP_Invoice.Invoice_For, AAP_Invoice.Unit, AAP_Invoice.UnitPrice, AAP_Invoice.TotalAmount, AAP_Invoice.Discount, AAP_Invoice.PaidAmount, AAP_Invoice_Category.InvoiceCategory, AAP_Invoice.InvoiceID, (AAP_Invoice.TotalAmount - AAP_Invoice.PaidAmount - AAP_Invoice.Discount) AS Due FROM AAP_Invoice INNER JOIN AAP_Invoice_Category ON AAP_Invoice.InvoiceCategoryID = AAP_Invoice_Category.InvoiceCategoryID WHERE (AAP_Invoice.SchoolID = @SchoolID) AND (AAP_Invoice.IsPaid = 0)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="row no-gutters my-4">
<div class="col">
                        <div class="conclusion">
                            <h4>Thank you, IT Genius.</h4>
                            <h5>Payment Method:</h5>

                            <table>
                                <tr>
                                    <td style="background-color: #ddd; padding: 0 3px">BANK NAME</td>
                                    
                                    <td>Eastern Bank PLC</td>
                                </tr>
                                <tr>
                                    <td>Account Name</td>
                                  <td>IT Genius</td>
                                </tr>
                                <tr>
                                    <td>Account Number</td>
                                    <td>10510.7000.1333</td>
                                </tr>
                                <tr>
                                    <td>Branch</td>
                                    <td>Sonargaon Branch</td>
                                </tr>
                                    <tr>
                                    <td>Routing Number</td>
                                    <td>095276586</td>
                                   </tr>
                                 <tr>
                                    <td style=" padding: 5px;"><img src="../../CSS/Image/rocket.jpg" /></td>
                                    <td>01739144141-6</td>
                                     
                                </tr>
                                <tr>
                                    <td style=" padding: 5px;">bKash (Personal)</td>
                                    <td>+880 1712-674118</td>
                                     
                                </tr>
                            </table>
                        </div>
                    </div>


                    <div class="col-3">
                        <div class="gt-table">
                            <table>
                                <tr>
                                    <td>Total:</td>
                                    <td><%#Eval("GrandTotal") %> Tk</td>
                                </tr>
                                <tr style="display: none;" id="Is_Discount">
                                    <td>Discount:</td>
                                    <td><span id="Discount"><%#Eval("Discount") %></span> Tk</td>
                                </tr>
                            </table>
                        </div>

                        <div class="grand-total">
                            <table>
                                <tr>
                                    <td>Due:</td>
                                    <td><%#Eval("Due") %> Tk</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>

                <!-- ShurjoPay Online Payment Section -->
                <div class="online-pay-section d-print-none">
                    <h5 class="mb-1" style="color:#856404;">
                        <i class="fa fa-credit-card"></i> অনলাইনে পেমেন্ট করুন
                    </h5>
                    <p class="mb-3 text-muted" style="font-size:.92rem;">ShurjoPay-এর মাধ্যমে কার্ড, মোবাইল ব্যাংকিং বা ইন্টারনেট ব্যাংকিং দিয়ে পেমেন্ট করুন।</p>
                    <asp:Button ID="btnShurjoPay" runat="server" 
                        CssClass="shurjopay-btn"
                        Text="&#xf09d;  ShurjoPay দিয়ে পেমেন্ট করুন"
                        OnClick="btnShurjoPay_Click"
                        UseSubmitBehavior="true" />
                    <br />
                    <small class="text-muted mt-2 d-block">
                        <img src="/CSS/Image/shurjopay_logo.png" onerror="this.style.display='none'" alt="ShurjoPay" style="height:18px;" />
                        Powered by ShurjoPay &bull; Secure Payment
                    </small>
                </div>

                <div class="signature-section">
                    <div class="signature-box">
                        <div class="signature-line">Recipient's Name</div>
                    </div>
                    <div class="signature-box">
                        <div class="signature-line">Amount Collected: __________ Tk</div>
                    </div>
                    <div class="signature-box">
                        <div class="signature-line">Authorised sign</div>
                    </div>
                </div>

                <div class="invc-footer">
                    <div class="footer_title"></div>
                    <div class="row text-center">
                        <div class="col-3">
                            <i class="fa fa-phone" aria-hidden="true"></i>
                            01739144141
                        </div>
                        <div class="col">
                            <i class="fa fa-map-marker" aria-hidden="true"></i>
                           18/11 Mosjid Road, Sontek, Jatrabari, Dhaka
                        </div>
                        <div class="col-3">
                            <i class="fa fa-globe" aria-hidden="true"></i>
                            www.itgeniusbd.com
                        </div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
        <EmptyDataTemplate>
            <h4 class="text-center">You have no due invoice!</h4>
        </EmptyDataTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="InvoiceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SUM(AAP_Invoice.TotalAmount) AS GrandTotal, SUM(AAP_Invoice.Discount) AS Discount, SUM(AAP_Invoice.PaidAmount) AS PaidAmount, SUM(AAP_Invoice.TotalAmount - AAP_Invoice.PaidAmount - AAP_Invoice.Discount) AS Due, SchoolInfo.SchoolName, SchoolInfo.Address, SchoolInfo.Phone, SchoolInfo.Email FROM AAP_Invoice INNER JOIN SchoolInfo ON AAP_Invoice.SchoolID = SchoolInfo.SchoolID WHERE (AAP_Invoice.SchoolID = @SchoolID) AND (AAP_Invoice.IsPaid = 0) GROUP BY SchoolInfo.SchoolName, SchoolInfo.Address, SchoolInfo.Phone, SchoolInfo.Email">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <asp:HiddenField ID="hfPaymentMsg" runat="server" />
    <script>
        $(function () {
            var Discount = $("#Discount").text();
            if (Discount > 0) {
                $("#Is_Discount").show();
            }

            // Payment result message
            var msg = $("#<%= hfPaymentMsg.ClientID %>").val();
            if (msg && msg.length > 0) {
                alert(msg);
            }

            // Subscription expired modal — auto open on page load
            var due = $("#<%= hfDueAmount.ClientID %>").val();
            var isBlocked = $("#<%= hfIsBlocked.ClientID %>").val();
            var daysLeft = parseInt($("#<%= hfDaysLeft.ClientID %>").val()) || 0;

            if (due && parseInt(due) > 0) {
                $("#subDueAmountText").text(parseInt(due).toLocaleString('en-IN'));
                $("#subDueAmountBox").show();

                if (isBlocked === '1') {
                    // Access blocked — বর্তমান মেসেজ ঠিকই আছে
                    $("#subModalHeader").css('background', '#c0392b');
                    $("#subModalTitleText").text('সফটওয়্যার অ্যাক্সেস সাময়িকভাবে বন্ধ আছে');
                    $("#subModalMessage").text('আপনার সাবস্ক্রিপশনের মেয়াদ শেষ হওয়াতে সফটওয়্যার ব্যবহারের অ্যাক্সেস বন্ধ আছে। দয়া করে অনলাইনে পেমেন্ট করে সাবস্ক্রিপশন রিনিউ করুন।');
                } else {
                    // Grace period বা EndDate এখনো পার হয়নি — কত দিন বাকি দেখাও
                    $("#subModalHeader").css('background', '#e67e22');
                    var daysText = daysLeft === 0 ? 'আজই শেষ হচ্ছে' :
                                   daysLeft === 1 ? 'আর মাত্র ১ দিন বাকি' :
                                   'আর মাত্র ' + daysLeft + ' দিন বাকি';
                    $("#subModalTitleText").text('⚠️ পেমেন্টের সময়সীমা');
                    $("#subModalMessage").html('<strong style="font-size:1.3rem; color:#c0392b;">' + daysText + '</strong><br/><br/>আপনার বকেয়া ইনভয়েসের পেমেন্টের সময়সীমা শেষ হতে চলেছে। সময়মতো পেমেন্ট না করলে সফটওয়্যার অ্যাক্সেস বন্ধ হয়ে যাবে।');
                }
                $("#subExpiredModal").addClass("active");
            }
        });
    </script>
</asp:Content>
