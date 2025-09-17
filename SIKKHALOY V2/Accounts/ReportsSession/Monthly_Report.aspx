<%@ Page Title="" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Monthly_Report.aspx.cs" Inherits="EDUCATION.COM.Accounts.ReportsSession.Monthly_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="form-group">
        <asp:DropDownList ID="Session_DropDownList" CssClass="form-control" runat="server" DataSourceID="All_SessionSQL" DataTextField="EducationYear" DataValueField="EducationYearID" AutoPostBack="True" AppendDataBoundItems="True">
            <asp:ListItem Value="%">All Session</asp:ListItem>
        </asp:DropDownList>
        <asp:SqlDataSource ID="All_SessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Education_Year] WHERE ([SchoolID] = @SchoolID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>
    <div class="form-group">
        <asp:DropDownList ID="MonthDropDownList" CssClass="form-control" runat="server" DataSourceID="MonthSQL" DataTextField="EducationYear" DataValueField="EducationYearID" AutoPostBack="True">
        </asp:DropDownList>
        <asp:SqlDataSource ID="MonthSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT M_T.Months FROM 
(SELECT FORMAT(Extra_IncomeDate, 'MMM yyyy') AS Months FROM Extra_Income WHERE (SchoolID = @SchoolID) AND (EducationYearID LIKE @EducationYearID) GROUP BY FORMAT(Extra_IncomeDate, 'MMM yyyy')
union
SELECT FORMAT(PaidDate, 'MMM yyyy') AS Months FROM Income_PaymentRecord WHERE (SchoolID = @SchoolID) AND (EducationYearID LIKE @EducationYearID) GROUP BY FORMAT(PaidDate, 'MMM yyyy')
union
SELECT FORMAT(ExpenseDate, 'MMM yyyy') AS Months FROM Expenditure WHERE (SchoolID = @SchoolID) AND (EducationYearID LIKE @EducationYearID) GROUP BY FORMAT(ExpenseDate, 'MMM yyyy')
union
SELECT FORMAT(Paid_date, 'MMM yyyy') AS Months FROM Employee_Payorder_Records WHERE (SchoolID = @SchoolID) AND (EducationYearID LIKE @EducationYearID) GROUP BY FORMAT(Paid_date, 'MMM yyyy')) M_T 
order by  CONVERT(Date, M_T.Months)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:ControlParameter ControlID="Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>
</asp:Content>
