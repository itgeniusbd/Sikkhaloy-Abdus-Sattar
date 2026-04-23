<%@ Page Title="Paid Invoice List" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Invoice_List.aspx.cs" Inherits="EDUCATION.COM.Profile.Invoice.Invoice_List" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Paid Invoice</h3>

   <asp:GridView ID="MoneyReceiptGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="InvoiceReceiptID" DataSourceID="MoneyReceiptSQL">
        <Columns>
            <asp:BoundField DataField="InvoiceReceipt_SN" HeaderText="Receipt" SortExpression="InvoiceReceipt_SN" />
            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" />
            <asp:TemplateField HeaderText="Gateway Charge">
                <ItemTemplate>
                    <%# Convert.ToDecimal(Eval("GatewayCharge")) > 0
                        ? string.Format("<span style='color:#e67e22;font-size:.88em;'>{0:F2} + {1:F2} = <b>{2:F2} Tk</b></span>",
                            Eval("TotalAmount"), Eval("GatewayCharge"), Eval("CustomerPaid"))
                        : "—" %>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="PaymentBy" HeaderText="Payment By" SortExpression="PaymentBy" />
            <asp:BoundField DataField="PaidByUser" HeaderText="Paid By User" SortExpression="PaidByUser" />
            <asp:BoundField DataField="Collected_By" HeaderText="Collected By" SortExpression="Collected_By" />
            <asp:BoundField DataField="Payment_Method" HeaderText="Payment Method" SortExpression="Payment_Method" />
            <asp:BoundField DataField="PaidDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Paid Date" SortExpression="PaidDate" />
            <asp:TemplateField HeaderText="Print">
                <ItemTemplate>
                    <a href="Paid_Receipt.aspx?SID=<%#Eval("SchoolID") %>&RID=<%#Eval("InvoiceReceiptID") %>">Print</a>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
       <EmptyDataTemplate>
           No Paid Record!
       </EmptyDataTemplate>
    </asp:GridView>
    <asp:SqlDataSource ID="MoneyReceiptSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT 1">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>
    
</asp:Content>
