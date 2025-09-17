<%@ Page Title="Due Invoice" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Due_Invoice.aspx.cs" Inherits="EDUCATION.COM.Profile.Invoice.Due_Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../css/Invoice.css?v=1.0.2" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="d-print-none">
        <a onclick="window.print();" class="btn btn-sm btn-green">Print</a>
    </div>

    <asp:FormView ID="PrintFormView" runat="server" DataSourceID="InvoiceSQL" RenderOuterTable="false">
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
                    <asp:Repeater ID="DetailsRepeater" runat="server" DataSourceID="DetailsSQL">
                        <HeaderTemplate>
                            <table class="invoice-table">
                                <thead>
                                    <tr>
                                        <th>SN</th>
                                        <th>Description</th>
                                        <th class="text-right">Unit</th>
                                        <th class="text-right">Unit Price</th>
                                        <th class="text-right">Line Total</th>
                                        
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
                                
                                <td class="text-right"><%# Eval("Due") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                   </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:SqlDataSource ID="DetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AAP_Invoice.Invoice_For, AAP_Invoice.Unit, AAP_Invoice.UnitPrice, AAP_Invoice.TotalAmount, AAP_Invoice_Category.InvoiceCategory, AAP_Invoice.InvoiceID, AAP_Invoice.TotalAmount - AAP_Invoice.PaidAmount AS Due FROM AAP_Invoice INNER JOIN AAP_Invoice_Category ON AAP_Invoice.InvoiceCategoryID = AAP_Invoice_Category.InvoiceCategoryID WHERE (AAP_Invoice.SchoolID = @SchoolID) AND (AAP_Invoice.IsPaid = 0)">
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

                <div class="auto-sign">
                    Authorised sign
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
    <asp:SqlDataSource ID="InvoiceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT sum(AAP_Invoice.TotalAmount - AAP_Invoice.PaidAmount) as pid, SUM(AAP_Invoice.TotalAmount - AAP_Invoice.PaidAmount) AS GrandTotal, SUM(AAP_Invoice.Discount) AS Discount, SUM(AAP_Invoice.PaidAmount) AS PaidAmount, SUM(AAP_Invoice.Due) AS Due, SchoolInfo.SchoolName, SchoolInfo.Address, SchoolInfo.Phone, SchoolInfo.Email FROM AAP_Invoice INNER JOIN SchoolInfo ON AAP_Invoice.SchoolID = SchoolInfo.SchoolID WHERE (AAP_Invoice.SchoolID = @SchoolID) AND (AAP_Invoice.IsPaid = 0) GROUP BY SchoolInfo.SchoolName, SchoolInfo.Address, SchoolInfo.Phone, SchoolInfo.Email">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            var Discount = $("#Discount").text();
            if (Discount > 0) {
                $("#Is_Discount").show();
            }
        });
    </script>
</asp:Content>
