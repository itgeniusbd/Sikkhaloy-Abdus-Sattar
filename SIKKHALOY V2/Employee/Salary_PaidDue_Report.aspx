<%@ Page Title="Salary Paid Due Report" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Salary_PaidDue_Report.aspx.cs" Inherits="EDUCATION.COM.Employee.Salary_PaidDue_Report" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="/CSS/bootstrap-multiselect.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Salary Paid And Due Report</h3>
    <div class="form-inline d-print-none">
        <div class="form-group">
            <asp:ListBox ID="RoleListBox" CssClass="form-control" runat="server" DataSourceID="PayorderNameSQL" DataTextField="Payorder_Name" DataValueField="Employee_Payorder_NameID" SelectionMode="Multiple" OnSelectedIndexChanged="RoleListBox_SelectedIndexChanged"></asp:ListBox>
            <asp:SqlDataSource ID="PayorderNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT Employee_Payorder_NameID, Payorder_Name FROM Employee_Payorder_Name WHERE (SchoolID = @SchoolID)">


                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>

            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-blue-grey" Text="Submit" ValidationGroup="1" OnClick="SubmitButton_Click" />
        </div>
    </div>

    <rsweb:ReportViewer ID="PaidDueReportViewer" runat="server" Width="100%" Height="100%" SizeToReportContent="True" BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid" InternalBorderWidth="1px" LinkActiveColor="" LinkActiveHoverColor="" LinkDisabledColor="" PrimaryButtonBackgroundColor="" PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor="" PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor="" ToolbarForegroundColor="" ToolbarForegroundDisabledColor="" ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" ToolBarItemPressedHoverBackColor="153, 187, 226">
        <LocalReport ReportPath="Employee\rdlc\SalayPaid_Due.rdlc">
            <DataSources>
                <rsweb:ReportDataSource DataSourceId="PaidDueODS" Name="DataSet1" />
            </DataSources>
        </LocalReport>
    </rsweb:ReportViewer>
    <asp:ObjectDataSource ID="PaidDueODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.Emp_Monthly_Salary_ReportTableAdapter">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
            <asp:Parameter Name="RoleIDs" Type="String" />
        </SelectParameters>
    </asp:ObjectDataSource>


    <script src="/JS/bootstrap-multiselect.js"></script>
    <script type="text/javascript">
        $('[id*=RoleListBox]').multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            includeResetOption: true,
            includeResetDivider: true,
            nonSelectedText: 'All Payment Role'
        });
    </script>
</asp:Content>
