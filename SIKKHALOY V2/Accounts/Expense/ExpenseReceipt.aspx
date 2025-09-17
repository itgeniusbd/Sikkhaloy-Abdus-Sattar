<%@ Page Title="Expense Receipt" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="ExpenseReceipt.aspx.cs" Inherits="EDUCATION.COM.Accounts.Expense.ExpenseReceipt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .info { text-align: center; color: #000; font-weight: 400; margin-bottom: 6px; }
        .mGrid td { padding: 3px; border: 1px solid #a0a0a0; }
        .grid-footer td { font-weight: bold; }
        .received-by-user-container { text-align: center; color: #333; font-size: 11px; margin-top: 5px; }

        @page { margin: 0 13.3rem !important; }
        @media print {
            .logo-waper { display: none; }
            #header { margin-bottom: 10px; border-bottom: none !important; }

            /*for black and white page*/
            .bg-main { background-color: #fff; color: #000; box-shadow: none !important }
            #InstitutionName { font-weight: bold; color: #000 !important; }
        }
    </style>
    <!--add dynamic css for printing-->
    <style type="text/css" media="print" id="print-content"></style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h6 class="font-weight-bold text-center">Expense Receipt</h6>
    <asp:GridView ID="ExpenseGridView" runat="server" AutoGenerateColumns="False" DataSourceID="ExpenseSQL" CssClass="mGrid" ShowFooter="True" Font-Bold="False" RowStyle-CssClass="Rows">
        <Columns>
            <asp:BoundField DataField="ExpenseID" HeaderText="Voucher No." SortExpression="ExpenseID" />
            <asp:BoundField DataField="CategoryName" HeaderText="Category" SortExpression="CategoryName" />
            <asp:BoundField DataField="ExpenseFor" HeaderText="Description" SortExpression="ExpenseFor" />
            <asp:BoundField DataField="ExpenseDate" HeaderText="Date" SortExpression="ExpenseDate" DataFormatString="{0:d MMM yyyy}" />
            <asp:TemplateField HeaderText="Amount" SortExpression="Amount">
                <ItemTemplate>
                    <label class="amount"><%# Eval("Amount") %></label>
                </ItemTemplate>
                <FooterTemplate>
                    <strong id="total"></strong>
                </FooterTemplate>
            </asp:TemplateField>
        </Columns>
        <FooterStyle CssClass="grid-footer" />
        <RowStyle CssClass="Rows" />
    </asp:GridView>
    <asp:SqlDataSource ID="ExpenseSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT Expenditure.ExpenseID, Expenditure.Amount, Expenditure.ExpenseFor, Expenditure.ExpenseDate, Expense_CategoryName.CategoryName FROM Expenditure INNER JOIN Expense_CategoryName ON Expenditure.ExpenseCategoryID = Expense_CategoryName.ExpenseCategoryID WHERE (Expenditure.SchoolID = @SchoolID) AND (Expenditure.EducationYearID = @EducationYearID) AND (Expenditure.ExpenseID = @ExpenseID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
            <asp:QueryStringParameter Name="ExpenseID" QueryStringField="id" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:FormView ID="ReceivedByFormView" runat="server" DataSourceID="ReceivedBySQL" RenderOuterTable="false">
        <ItemTemplate>
            <div class="received-by-user-container">
                (© Sikkhaloy.com) Inserted By: <%# Eval("Name") %>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="ReceivedBySQL" runat="server"
        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT Admin.FirstName + ' ' + Admin.LastName AS Name, Expenditure.ExpenseID, 
        Expenditure.SchoolID FROM Admin INNER JOIN Expenditure ON Admin.RegistrationID = Expenditure.RegistrationID
        WHERE (Expenditure.SchoolID = @SchoolID) AND (Expenditure.ExpenseID = @ExpenseID)">
        <SelectParameters>
           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:QueryStringParameter Name="ExpenseID" QueryStringField="id" />
        </SelectParameters>
    </asp:SqlDataSource>

    <div class="d-print-none mt-4">
        <input type="button" value="Print" onclick="window.print();" class="btn btn-outline-primary" />
    </div>


    <script>
        $(function () {
            //show total in gridview footer
            let total = 0;
            $(".amount").each(function () {
                total += parseFloat($(this).text())
            });

            $("#total").text(`Total: ${total}`);
        });
    </script>
</asp:Content>

