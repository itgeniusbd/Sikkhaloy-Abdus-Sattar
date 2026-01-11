<%@ Page Title="Print Invoice" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Print_Invoice.aspx.cs" Inherits="EDUCATION.COM.Authority.Invoice.Print_Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Print Invoice</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="School_DropDownList" AutoPostBack="true" CssClass="form-control SearchDDL" runat="server" AppendDataBoundItems="True" DataSourceID="InstitutionSQL" DataTextField="SchoolName_ID" DataValueField="SchoolID">
                <asp:ListItem Value="0">[ INSTITUTION ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT SchoolInfo.SchoolID,SchoolInfo.SchoolName, CAST(SchoolInfo.SchoolID AS varchar(10)) + ' - ' + SchoolInfo.SchoolName AS SchoolName_ID FROM SchoolInfo INNER JOIN AAP_Invoice ON SchoolInfo.SchoolID = AAP_Invoice.SchoolID WHERE (SchoolInfo.Validation = N'Valid') ORDER BY SchoolInfo.SchoolName"></asp:SqlDataSource>
        </div>
    </div>

    <asp:GridView ID="InvoiceGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="InvoiceID" DataSourceID="InvoiceSQL">
        <Columns>
            <asp:TemplateField>
                <HeaderTemplate>
                    <asp:CheckBox ID="AllCheckBox" Text=" " runat="server" />
                </HeaderTemplate>
                <ItemTemplate>
                    <asp:CheckBox ID="Pay_CheckBox" Text=" " runat="server" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Invoice_SN" HeaderText="SN" SortExpression="Invoice_SN" />
            <asp:BoundField DataField="InvoiceCategory" HeaderText="Category" SortExpression="InvoiceCategory" />
            <asp:BoundField DataField="MonthName" DataFormatString="{0:MMMM yyyy}" HeaderText="Month" SortExpression="MonthName" />
            <asp:BoundField DataField="IssuDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Issue Date" SortExpression="IssuDate" />
            <asp:BoundField DataField="Invoice_For" HeaderText="Invoice For" SortExpression="Invoice_For" />
            <asp:BoundField DataField="Unit" HeaderText="Unit" SortExpression="Unit" />
            <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" SortExpression="UnitPrice" />
            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" />
            <asp:BoundField DataField="Discount" HeaderText="Discount" SortExpression="Discount" />
            <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
            <asp:BoundField DataField="Due" HeaderText="Due" SortExpression="Due" />
            <asp:TemplateField ShowHeader="False" HeaderText="Delete">
                <ItemTemplate>
                    <asp:LinkButton ID="LinkButton1" Enabled='<%# Eval("PaidAmount").ToString()=="0" %>' OnClientClick="return confirm('Are you sure want to delete?')" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete"></asp:LinkButton>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="InvoiceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AAP_Invoice.Invoice_SN, AAP_Invoice.IssuDate, AAP_Invoice.EndDate, AAP_Invoice.Invoice_For, AAP_Invoice.Unit, AAP_Invoice.UnitPrice, AAP_Invoice.TotalAmount, AAP_Invoice.Discount, AAP_Invoice.CreateDate, AAP_Invoice.PaidAmount, AAP_Invoice.Due, AAP_Invoice.MonthName, AAP_Invoice_Category.InvoiceCategory, AAP_Invoice.InvoiceID FROM AAP_Invoice INNER JOIN AAP_Invoice_Category ON AAP_Invoice.InvoiceCategoryID = AAP_Invoice_Category.InvoiceCategoryID WHERE (AAP_Invoice.SchoolID = @SchoolID) AND (AAP_Invoice.IsPaid = 0)" DeleteCommand="DELETE FROM AAP_Invoice WHERE (InvoiceID = @InvoiceID) AND (PaidAmount = 0)">
        <DeleteParameters>
            <asp:Parameter Name="InvoiceID" />
        </DeleteParameters>
        <SelectParameters>
            <asp:ControlParameter ControlID="School_DropDownList" Name="SchoolID" PropertyName="SelectedValue" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <%if (InvoiceGridView.Rows.Count > 0)
        { %>
    <asp:Button ID="InvoiceButton" runat="server" Text="Create Invoice" CssClass="btn btn-green" OnClick="InvoiceButton_Click" />
    <%} %>


    <%if (MoneyReceiptGridView.Rows.Count > 0)
        { %>
    <h5 class="mt-3">Paid Receipt</h5>
    <%} %>
    <asp:GridView ID="MoneyReceiptGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="InvoiceReceiptID" DataSourceID="MoneyReceiptSQL">
        <Columns>
            <asp:BoundField DataField="InvoiceReceipt_SN" HeaderText="Receipt" SortExpression="InvoiceReceipt_SN" />
            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" />
            <asp:BoundField DataField="PaymentBy" HeaderText="Payment By" SortExpression="PaymentBy" />
            <asp:BoundField DataField="PaidByUser" HeaderText="Paid By User" SortExpression="PaidByUser" />
            <asp:BoundField DataField="Collected_By" HeaderText="Collected By" SortExpression="Collected_By" />
            <asp:BoundField DataField="Payment_Method" HeaderText="Payment Method" SortExpression="Payment_Method" />
            <asp:BoundField DataField="PaidDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Paid Date" SortExpression="PaidDate" />
            <asp:TemplateField HeaderText="Print">
                <ItemTemplate>
                    <a href="Print/Paid_Receipt.aspx?SID=<%#Eval("SchoolID") %>&RID=<%#Eval("InvoiceReceiptID") %>">Print</a>
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="MoneyReceiptSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT InvoiceReceiptID, InvoiceReceipt_SN, TotalAmount, PaidDate, PaymentBy, PaidByUser, Collected_By, Payment_Method, SchoolID FROM AAP_Invoice_Receipt WHERE (SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:ControlParameter ControlID="School_DropDownList" Name="SchoolID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $("[id*=AllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=Pay_CheckBox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=Pay_CheckBox]").on("click", function () {
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected")) : ($("td", $(this).closest("tr")).removeClass("selected"));
            });
        });
    </script>
</asp:Content>
