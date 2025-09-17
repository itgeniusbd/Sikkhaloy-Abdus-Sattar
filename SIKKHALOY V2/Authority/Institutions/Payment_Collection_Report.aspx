<%@ Page Title="Payment Collection Report" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Payment_Collection_Report.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.Payment_Collection_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .Count { text-align: center; margin-bottom: 15px; color: #fff; }
        .Count .Box { background: -webkit-gradient(linear,left top,right top,from(#0288d1),to(#002fd1)); background: linear-gradient(to right,#0288d1,#002fd1); padding: 1rem 0; }
        .Count .Box1 { background: -webkit-gradient(linear,left top,right top,from(#00796b),to(#00897b)); background: linear-gradient(to right,#00796b,#00897b); padding: 1rem 0; }
        .Count .Box1 a{color:#ffd800}
        .Count h5 { margin: 0; }
        .Count p { margin: 0; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Payment Collection Report</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList AutoPostBack="true" ID="CategoryDropDownList" CssClass="form-control" runat="server" DataSourceID="CategorySQL" DataTextField="InvoiceCategory" DataValueField="InvoiceCategoryID" AppendDataBoundItems="True">
                <asp:ListItem Value="%">[ ALL CATEGORY ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [InvoiceCategory], [InvoiceCategoryID] FROM [AAP_Invoice_Category]"></asp:SqlDataSource>
        </div>
    </div>

    <asp:FormView ID="TotalFormView" DataSourceID="Total_SQL" Width="100%" runat="server">
        <ItemTemplate>
            <div class="row Count">
                <div class="col">
                    <div class="Box">
                        <h5><%# Eval("InvoiceCount") %></h5>
                        <p>Invoice Count</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box">
                        <h5><%# Eval("Unit_Count") %></h5>
                        <p>Unit Count</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box">
                        <h5>৳<%# Eval("TotalAmount","{0:N}") %></h5>
                        <p>Total Amount</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box">
                        <h5>৳<%# Eval("Discount","{0:N}") %></h5>
                        <p>Discount</p>
                    </div>
                </div>
            </div>
            <div class="row Count">
                <div class="col">
                    <div class="Box1">
                        <h5>৳<%# Eval("Receivable","{0:N}") %></h5>
                        <p>Receivable</p>
                    </div>
                </div>
                <div class="col">
                    <div class="Box1">
                        <a href="Paid_Due/PaidDetails.aspx" target="_blank">
                            <h5>৳<%# Eval("PaidAmount","{0:N}") %>
                                <i class="fa fa-caret-right" aria-hidden="true"></i>
                            </h5>
                            <p>Paid Amount</p>
                        </a>
                    </div>
                </div>
                <div class="col">
                    <div class="Box1">
                        <a href="Paid_Due/Due_Details.aspx" target="_blank">
                            <h5>৳<%# Eval("Due","{0:N}") %>
                                <i class="fa fa-caret-right" aria-hidden="true"></i>
                            </h5>
                            <p>Due</p>
                        </a>
                    </div>
                </div>
                <div class="col">
                    <div class="Box1">
                        <h5><%# Eval("Collect_Percent") %>%</h5>
                        <p>Collected %</p>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="Total_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT *, ROUND((T.PaidAmount*100)/(T.Receivable),2) AS Collect_Percent from (SELECT COUNT(InvoiceID)AS InvoiceCount ,SUM(Unit)AS Unit_Count,SUM(TotalAmount) AS TotalAmount, SUM(Discount) AS Discount, SUM(TotalAmount - Discount) AS Receivable,SUM(PaidAmount) AS PaidAmount, SUM(Due) AS Due  FROM  AAP_Invoice WHERE InvoiceCategoryID LIKE @InvoiceCategoryID) AS T">
        <SelectParameters>
            <asp:ControlParameter ControlID="CategoryDropDownList" Name="InvoiceCategoryID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:GridView ID="DetailsGridView" CssClass="mGrid table-hover" runat="server" AutoGenerateColumns="False" DataSourceID="DetailsSQL" AllowSorting="True">
        <Columns>
            <asp:BoundField DataField="Month" HeaderText="Month" ReadOnly="True" SortExpression="Month" />
            <asp:BoundField DataField="IncoiceCount" HeaderText="Invoice Count" ReadOnly="True" SortExpression="IncoiceCount" />
            <asp:BoundField DataField="Unit_Count" HeaderText="Unit" ReadOnly="True" SortExpression="Unit_Count" />
            <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" SortExpression="TotalAmount" ReadOnly="True" />
            <asp:BoundField DataField="Discount" HeaderText="Discount" SortExpression="Discount" ReadOnly="True" />
            <asp:BoundField DataField="Receivable" HeaderText="Receivable" SortExpression="Receivable" ReadOnly="True"></asp:BoundField>
            <asp:BoundField DataField="PaidAmount" HeaderText="Paid" SortExpression="PaidAmount" ReadOnly="True"></asp:BoundField>
            <asp:BoundField DataField="Due" HeaderText="Due" ReadOnly="True" SortExpression="Due"></asp:BoundField>
            <asp:BoundField DataField="Collect_Percent" HeaderText="Collected%" ReadOnly="True" SortExpression="Collect_Percent" />
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="DetailsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT *, ROUND((T.PaidAmount*100)/(T.Receivable),2) AS Collect_Percent from (SELECT  YEAR(MonthName) AS Year, DATENAME(month, MonthName) AS Month, MONTH(MonthName) AS MonthNo
,COUNT(InvoiceID)AS IncoiceCount ,SUM(Unit)AS Unit_Count,SUM(TotalAmount) AS TotalAmount, SUM(Discount) AS Discount, SUM(TotalAmount - Discount) AS Receivable,SUM(PaidAmount) AS PaidAmount, SUM(Due) AS Due  FROM  AAP_Invoice WHERE InvoiceCategoryID LIKE @InvoiceCategoryID
GROUP BY MONTH(MonthName), DATENAME(month, MonthName), YEAR(MonthName)) AS T order by T.Year, T.MonthNo">
        <SelectParameters>
            <asp:ControlParameter ControlID="CategoryDropDownList" Name="InvoiceCategoryID" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
