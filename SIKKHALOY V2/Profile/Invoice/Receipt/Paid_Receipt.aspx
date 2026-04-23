<%@ Page Title="Paid Invoice" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Paid_Receipt.aspx.cs" Inherits="EDUCATION.COM.Profile.Invoice.Paid_Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../../css/Invoice.css?v=1.0.8" rel="stylesheet" />
    <style>
        .invoice-to { padding: 0.5rem 0; margin: 10px 0; }
        .mr-5, .mx-5 { margin-right: 1rem !important; }
        .ml-5, .mx-5 { margin-left: 1rem !important; }

        /* ── Summary Box ── */
        .receipt-summary {
            border-radius: 8px;
            overflow: hidden;
            border: 1px solid #ddd;
            font-size: .92rem;
            min-width: 180px;
        }
        .receipt-summary table { width: 100%; border-collapse: collapse; margin: 0; }
        .receipt-summary td { padding: 6px 10px; }
        .receipt-summary .row-label { color: #000000; font-weight: 600; }
        .receipt-summary .row-value { text-align: right; font-weight: 600; }
        .receipt-summary .row-subtotal td { background: #f8f9fa; border-top: 1px solid #ddd; }
        .receipt-summary .row-charge td  { background: #fff8e1; }
        .receipt-summary .row-charge .row-label { color: #000000; font-weight: 600; }
        .receipt-summary .row-charge .row-value { color: #e67e22; }
        .receipt-summary .row-total td   {
            background: #27ae60;
            color: #fff;
            font-weight: 700;
            font-size: 1.05rem;
        }
        .receipt-summary .row-due td { background: #fdecea; color: #c0392b; }

        @media print {
            .img-sign { float: right !important; }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="d-print-none">
        <a href="Invoice_List.aspx" class="btn btn-sm btn-grey">Back</a>
        <a onclick="window.print();" class="btn btn-sm btn-green">Print</a>
    </div>

    <asp:FormView CssClass="Main-table" ID="PrintFormView" runat="server" DataSourceID="InvoiceSQL" Width="100%" OnDataBound="PrintFormView_DataBound">
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
                    <b>RECEIPT</b>
                </div>

                <div class="row no-gutters">
                    <div class="col">
                        <div class="invoice-to ml-5 mr-2">
                            <h2>RECEIPT TO:</h2>
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
                    </div>
                    <div class="col">
                        <div class="invoice-to ml-2 mr-5 text-right black-text">
                            <h2>RECEIPT #<%#Eval("InvoiceReceipt_SN") %></h2>
                            <h5><i class="fa fa-user" aria-hidden="true"></i>
                                Paid By: <%#Eval("PaymentBy") %></h5>
                            <p>
                                <i class="fa fa-user-circle-o" aria-hidden="true"></i>
                                Collected By: <%#Eval("Collected_By") %>
                            </p>
                            <p>
                                <i class="fa fa-credit-card" aria-hidden="true"></i>
                                Payment Method: <%#Eval("Payment_Method") %>
                            </p>
                            <p>
                                <i class="fa fa-calendar" aria-hidden="true"></i>
                                Paid Date: <%#Eval("PaidDate","{0:d MMM yyyy}") %>
                            </p>
                        </div>
                    </div>
                </div>

                <div class="details-list" style="margin-bottom:20px;">
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
                                        <th class="text-right" id="dueHeader" style="display:none;">Due</th>
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
                                <td class="text-right"><%# Eval("Paid") %></td>
                                <td class="text-right dueCell" style="display:none;" data-due='<%# Eval("Due") %>'><%# Eval("Due") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                   </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:SqlDataSource ID="DetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AAP_Invoice.Invoice_For, AAP_Invoice.Unit, AAP_Invoice.UnitPrice, AAP_Invoice.TotalAmount, AAP_Invoice.Discount, AAP_Invoice_Category.InvoiceCategory, AAP_Invoice.InvoiceID, AAP_Invoice_Payment_Record.InvoiceReceiptID, AAP_Invoice_Payment_Record.Amount AS Paid, (AAP_Invoice.TotalAmount - AAP_Invoice.PaidAmount - AAP_Invoice.Discount) AS Due FROM AAP_Invoice INNER JOIN AAP_Invoice_Category ON AAP_Invoice.InvoiceCategoryID = AAP_Invoice_Category.InvoiceCategoryID INNER JOIN AAP_Invoice_Payment_Record ON AAP_Invoice.InvoiceID = AAP_Invoice_Payment_Record.InvoiceID WHERE (AAP_Invoice_Payment_Record.InvoiceReceiptID = @InvoiceReceiptID)">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="InvoiceReceiptID" QueryStringField="RID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="row no-gutters">
<div class="col">
                        <div class="conclusion">
                            <h4 style="color:#27ae60; font-size:1rem; margin-bottom:6px;">
                                <i class="fa fa-check-circle"></i> Thank you, IT Genius.
                            </h4>
                            <p style="font-size:.82rem; color:#555; margin-bottom:6px; font-weight:600;">Payment Method:</p>
                            <table style="font-size:.82rem; border-collapse:collapse; width:auto;">
                                <!-- ── Bank Section ── -->
                                <tr>
                                    <td colspan="2" style="padding:4px 8px 2px; font-weight:700; color:#1a6b2f; font-size:.78rem; letter-spacing:.5px;">
                                        🏦 BANK TRANSFER
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:2px 8px; color:#555;">Bank</td>
                                    <td style="padding:2px 10px; font-weight:600;">Eastern Bank PLC</td>
                                </tr>
                                <tr>
                                    <td style="padding:2px 8px; color:#555;">Account Name</td>
                                    <td style="padding:2px 10px;">IT Genius</td>
                                </tr>
                                <tr>
                                    <td style="padding:2px 8px; color:#555;">Account No.</td>
                                    <td style="padding:2px 10px; font-weight:600;">10510.7000.1333</td>
                                </tr>
                                <tr>
                                    <td style="padding:2px 8px; color:#555;">Branch</td>
                                    <td style="padding:2px 10px;">Sonargaon Branch</td>
                                </tr>
                                <tr>
                                    <td style="padding:2px 8px 6px; color:#555;">Routing No.</td>
                                    <td style="padding:2px 10px 6px; font-weight:600;">095276586</td>
                                </tr>
                                <!-- ── Divider ── -->
                                <tr><td colspan="2" style="padding:0;"><div style="border-top:1px dashed #bbb; margin:2px 0;"></div></td></tr>
                                <!-- ── Rocket Section ── -->
                                <tr>
                                    <td colspan="2" style="padding:4px 8px 2px; font-weight:700; color:#6a0dad; font-size:.78rem; letter-spacing:.5px;">
                                        <img src="../../../CSS/Image/rocket.jpg" style="height:14px; vertical-align:middle;" /> ROCKET
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:2px 8px; color:#555;">Number</td>
                                    <td style="padding:2px 10px 6px; font-weight:600; color:#6a0dad;">01739144141-6</td>
                                </tr>
                                <!-- ── Divider ── -->
                                <tr><td colspan="2" style="padding:0;"><div style="border-top:1px dashed #bbb; margin:2px 0;"></div></td></tr>
                                <!-- ── bKash Section ── -->
                                <tr>
                                    <td colspan="2" style="padding:4px 8px 2px; font-weight:700; color:#e91e8c; font-size:.78rem; letter-spacing:.5px;">
                                        💗 bKASH (Personal)
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:2px 8px; color:#555;">Number</td>
                                    <td style="padding:2px 10px 4px; font-weight:600; color:#e91e8c;">+880 1712-674118</td>
                                </tr>
                            </table>
                        </div>
</div>

                    <div class="col-auto" style="padding-left:16px;">
                        <div class="receipt-summary">
                            <table>
                                <tr class="row-subtotal">
                                    <td class="row-label">Total:</td>
                                    <td class="row-value"><%#Eval("Total_Amount") %> Tk</td>
                                </tr>
                                <tr id="Is_Discount" style="display:none;" class="row-subtotal">
                                    <td class="row-label">Discount:</td>
                                    <td class="row-value" style="color:#27ae60;">− <span id="Discount"><%#Eval("Total_Discount") %></span> Tk</td>
                                </tr>
                                <tr class="row-subtotal">
                                    <td class="row-label">Paid:</td>
                                    <td class="row-value"><%#Eval("Total_Paid") %> Tk</td>
                                </tr>
                                <% if (HasGatewayCharge) { %>
                                <tr class="row-charge">
                                    <td class="row-label">Gateway Charge:</td>
                                    <td class="row-value">+ <%= GatewayCharge.ToString("F2") %> Tk</td>
                                </tr>
                                <tr class="row-total">
                                    <td>Total Paid:</td>
                                    <td class="row-value"><%= CustomerPaidAmt.ToString("F2") %> Tk</td>
                                </tr>
                                <% } else { %>
                                <tr class="row-total">
                                    <td>Total Paid:</td>
                                    <td class="row-value"><%#Eval("Total_Paid") %> Tk</td>
                                </tr>
                                <% } %>
                                <tr id="Is_Due" style="display:none;" class="row-due">
                                    <td class="row-label">Due:</td>
                                    <td class="row-value"><span id="TotalDue"><%#Eval("Total_Due") %></span> Tk</td>
                                </tr>
                            </table>
                        </div>
                    </div>
                </div>

                <div class="img-sign" style="float: right; clear: both; width: auto;">
                    <div>
                        <table style="display: inline-block;">
                            <tr>
                                <td>
                                    <img src="/CSS/Image/PaidSign.png" /></td>
                            </tr>
                            <tr>
                                <td>Authorised sign</td>
                            </tr>
                        </table>
                    </div>
                </div>
                <div style="clear: both;"></div>


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
    </asp:FormView>
    <asp:SqlDataSource ID="InvoiceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AAP_Invoice_Receipt.InvoiceReceipt_SN, SchoolInfo.SchoolName, SchoolInfo.Address, SchoolInfo.Phone, SchoolInfo.Email, AAP_Invoice_Receipt.TotalAmount AS Total_Paid, T_DUE.Total_Due, T_DUE.Total_Discount, AAP_Invoice_Receipt.PaidDate, AAP_Invoice_Receipt.PaymentBy, AAP_Invoice_Receipt.Collected_By, AAP_Invoice_Receipt.Payment_Method, T_DUE.Total_Amount FROM AAP_Invoice_Receipt INNER JOIN SchoolInfo ON AAP_Invoice_Receipt.SchoolID = SchoolInfo.SchoolID INNER JOIN AAP_Invoice ON SchoolInfo.SchoolID = AAP_Invoice.SchoolID INNER JOIN (SELECT AAP_Invoice_Receipt_1.InvoiceReceiptID, SUM(ISNULL(AAP_Invoice_1.TotalAmount - AAP_Invoice_1.PaidAmount - AAP_Invoice_1.Discount, 0)) AS Total_Due, SUM(ISNULL(AAP_Invoice_1.Discount, 0)) AS Total_Discount, SUM(ISNULL(AAP_Invoice_1.TotalAmount, 0)) AS Total_Amount FROM AAP_Invoice AS AAP_Invoice_1 INNER JOIN AAP_Invoice_Payment_Record ON AAP_Invoice_1.InvoiceID = AAP_Invoice_Payment_Record.InvoiceID INNER JOIN AAP_Invoice_Receipt AS AAP_Invoice_Receipt_1 ON AAP_Invoice_Payment_Record.InvoiceReceiptID = AAP_Invoice_Receipt_1.InvoiceReceiptID GROUP BY AAP_Invoice_Receipt_1.InvoiceReceiptID) AS T_DUE ON AAP_Invoice_Receipt.InvoiceReceiptID = T_DUE.InvoiceReceiptID WHERE (AAP_Invoice_Receipt.InvoiceReceiptID = @InvoiceReceiptID) AND (AAP_Invoice_Receipt.SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SID" />
            <asp:QueryStringParameter Name="InvoiceReceiptID" QueryStringField="RID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            var Discount = $("#Discount").text();
            if (Discount > 0) {
                $("#Is_Discount").show();
            }
            
            // Check if any due exists
            var hasDue = false;
            var totalDue = 0;
            
            $('.dueCell').each(function() {
                var dueValue = parseFloat($(this).data('due')) || 0;
                if (dueValue > 0) {
                    hasDue = true;
                    totalDue += dueValue;
                }
            });
            
            // Show Due column and header if any due exists
            if (hasDue) {
                $('#dueHeader').show();
                $('.dueCell').show();
                $('#Is_Due').show();
            }
            
            // Also check from Total_Due
            var totalDueFromDB = parseFloat($("#TotalDue").text()) || 0;
            if (totalDueFromDB > 0) {
                $('#dueHeader').show();
                $('.dueCell').show();
                $('#Is_Due').show();
            }
        });
    </script>
</asp:Content>
