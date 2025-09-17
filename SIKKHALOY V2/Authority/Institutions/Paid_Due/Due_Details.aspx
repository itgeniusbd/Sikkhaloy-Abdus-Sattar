<%@ Page Title="Due Details" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Due_Details.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.Paid_Due.Due_Details" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Due Details</h3>

    <div class="form-inline">
          <div class="form-group">
            <asp:DropDownList ID="CategoryDropDownList" CssClass="form-control" runat="server" DataSourceID="CategorySQL" DataTextField="InvoiceCategory" DataValueField="InvoiceCategoryID" AppendDataBoundItems="True" AutoPostBack="True">
                <asp:ListItem Value="%">[ALL CATEGORY ]</asp:ListItem>
              </asp:DropDownList>
            <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT InvoiceCategory, InvoiceCategoryID FROM AAP_Invoice_Category">
            </asp:SqlDataSource>
        </div>
        <div class="form-group ml-2">
            <asp:DropDownList ID="MonthDropDownList" CssClass="form-control" runat="server" DataSourceID="MonthSQL" DataTextField="Month" DataValueField="Month" AutoPostBack="True" OnDataBound="MonthDropDownList_DataBound"></asp:DropDownList>
            <asp:SqlDataSource ID="MonthSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CONVERT (varchar(7), MonthName, 120) AS Month FROM AAP_Invoice WHERE (IsPaid = 0) AND (InvoiceCategoryID LIKE @InvoiceCategoryID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="CategoryDropDownList" Name="InvoiceCategoryID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <asp:GridView ID="DueGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataSourceID="DueSQL" AllowSorting="True">
        <Columns>
            <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" />
            <asp:BoundField DataField="SchoolName" HeaderText="Institution" SortExpression="SchoolName" />
            <asp:BoundField DataField="InvoiceCategory" HeaderText="Category" SortExpression="InvoiceCategory" />
            <asp:BoundField DataField="Invoice_SN" HeaderText="Invoice SN" SortExpression="Invoice_SN" />
            <asp:BoundField DataField="Invoice_For" HeaderText="For" SortExpression="Invoice_For" />
            <asp:BoundField DataField="Unit" HeaderText="Unit" SortExpression="Unit" />
            <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" SortExpression="UnitPrice" />
            <asp:BoundField DataField="TotalAmount" HeaderText="TotalAmount" SortExpression="TotalAmount" />
            <asp:BoundField DataField="Discount" HeaderText="Discount" SortExpression="Discount" />
            <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" />
            <asp:BoundField DataField="Due" HeaderText="Due" ReadOnly="True" SortExpression="Due" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="DueSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AAP_Invoice.SchoolID, SchoolInfo.SchoolName, AAP_Invoice_Category.InvoiceCategory, AAP_Invoice.Invoice_SN, AAP_Invoice.IssuDate, AAP_Invoice.EndDate, AAP_Invoice.Invoice_For, AAP_Invoice.Unit, AAP_Invoice.UnitPrice, AAP_Invoice.TotalAmount, AAP_Invoice.Discount, AAP_Invoice.CreateDate, AAP_Invoice.PaidAmount, AAP_Invoice.Due, AAP_Invoice.NumberOfPayment, AAP_Invoice.LastPaidDate FROM AAP_Invoice INNER JOIN SchoolInfo ON AAP_Invoice.SchoolID = SchoolInfo.SchoolID INNER JOIN AAP_Invoice_Category ON AAP_Invoice.InvoiceCategoryID = AAP_Invoice_Category.InvoiceCategoryID WHERE (AAP_Invoice.IsPaid = 0) AND (CONVERT (varchar(7), AAP_Invoice.MonthName, 120) LIKE @Month) AND (AAP_Invoice.InvoiceCategoryID LIKE @InvoiceCategoryID) ORDER BY SchoolInfo.SchoolName, AAP_Invoice_Category.InvoiceCategory">
        <SelectParameters>
            <asp:ControlParameter ControlID="MonthDropDownList" Name="Month" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="CategoryDropDownList" Name="InvoiceCategoryID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
