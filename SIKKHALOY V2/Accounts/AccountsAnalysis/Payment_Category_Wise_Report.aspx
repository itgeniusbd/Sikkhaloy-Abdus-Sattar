<%@ Page Title="Payment Category Wise Report" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Payment_Category_Wise_Report.aspx.cs" Inherits="EDUCATION.COM.Accounts.AccountsAnalysis.Payment_Category_Wise_Report" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<%@ Register assembly="Microsoft.ReportViewer.WebForms" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Category_Report.css?v=2" rel="stylesheet" />
    <link href="/CSS/bootstrap-multiselect.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint">Accounts Report</h3>

    <asp:UpdatePanel ID="UpdatePanel4" runat="server">
        <ContentTemplate>
            <div class="form-inline d-print-none">
                <div class="form-group">
                    <asp:TextBox ID="FormDateTextBox" runat="server" autocomplete="off" CssClass="form-control Datetime" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="DateButton" runat="server" Text="Find" CssClass="btn btn-grey" OnClick="DateButton_Click" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item"><a class="nav-link active" href="#Income_Student" data-toggle="tab" role="tab" aria-expanded="true">Month Based Income Summery</a></li>
        <li class="nav-item"><a class="nav-link" href="#Income" data-toggle="tab" role="tab" aria-expanded="false">Income Report</a></li>
        <li class="nav-item"><a class="nav-link" href="#Expense" data-toggle="tab" role="tab" aria-expanded="false">Expense Report</a></li>
    </ul>

    <div class="tab-content z-depth-1">
        <div id="Income_Student" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="form-inline d-print-none">
                        <div class="form-group">
                            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                        <div class="S_Show form-group" style="display: none">
                            <asp:DropDownList ID="SectionDropDownList" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound">
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID)">
                                <SelectParameters>
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                        <div class="form-group">
                            <asp:ListBox ID="RoleListBox" CssClass="form-control" runat="server" DataSourceID="RoleSQL" DataTextField="Role" DataValueField="RoleID" SelectionMode="Multiple"></asp:ListBox>
                            <asp:SqlDataSource ID="RoleSQL" runat="server" CancelSelectOnNullParameter="False" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Income_Roles.Role, Income_Roles.RoleID FROM Income_PayOrder INNER JOIN Income_Roles ON Income_PayOrder.RoleID = Income_Roles.RoleID WHERE (Income_PayOrder.SchoolID = @SchoolID) AND (Income_PayOrder.EducationYearID = @EducationYearID) AND ((Income_PayOrder.ClassID = @ClassID) OR (@ClassID =0))">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                        <div class="form-group">
                            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-blue-grey" Text="Submit" ValidationGroup="1" OnClick="SubmitButton_Click" />
                        </div>
                    </div>

                    <div class="table-responsive">
                        <rsweb:ReportViewer ID="Student_Icome_ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="" Height="" AsyncRendering="False" SizeToReportContent="True" SplitterBackColor="White" BackColor="White">
                            <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Income_Cetagoty.rdlc" ReportPath="Accounts\AccountsAnalysis\Report\Monthly_Stu_Fee.rdlc">
                                <DataSources>
                                    <rsweb:ReportDataSource DataSourceId="Student_Inc_ODS" Name="DataSet1" />
                                </DataSources>
                            </LocalReport>
                        </rsweb:ReportViewer>
                        <asp:ObjectDataSource ID="Student_Inc_ODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.Income_Stu_Class_MonthlyReportTableAdapter">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" Type="String" />
                                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                                <asp:Parameter Name="RoleIDs" Type="String" />
                            </SelectParameters>
                        </asp:ObjectDataSource>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div id="Income" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <ul class="nav nav-tabs z-depth-1">
                        <li class="nav-item"><a class="nav-link active" href="#Income_Daily" data-toggle="tab" role="tab" aria-expanded="true">Daily</a></li>
                        <li class="nav-item"><a class="nav-link" href="#Income_Monthly" data-toggle="tab" role="tab" aria-expanded="false">Monthly</a></li>
                    </ul>
                    <div class="tab-content z-depth-1">
                        <div id="Income_Daily" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
                            <div class="table-responsive">
                                <rsweb:ReportViewer ID="Daily_Income_ReportViewer" runat="server" AsyncRendering="False" BackColor="White" Font-Names="Verdana" Font-Size="8pt" Height="" SizeToReportContent="True" SplitterBackColor="White" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="">
                                    <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Income_Cetagoty.rdlc" ReportPath="Accounts\AccountsAnalysis\Report\Daily_Income.rdlc">
                                        <DataSources>
                                            <rsweb:ReportDataSource DataSourceId="Daily_IncomeODS" Name="DataSet1" />
                                        </DataSources>
                                    </LocalReport>
                                </rsweb:ReportViewer>
                                <asp:ObjectDataSource ID="Daily_IncomeODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.Income_DailyTableAdapter">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                        <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" Type="DateTime" />
                                        <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" Type="DateTime" />
                                    </SelectParameters>
                                </asp:ObjectDataSource>
                            </div>
                        </div>
                        <div id="Income_Monthly" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                            <div class="table-responsive">
                                <rsweb:ReportViewer ID="Monthly_Income_ReportViewer" runat="server" AsyncRendering="False" BackColor="White" Font-Names="Verdana" Font-Size="8pt" Height="" SizeToReportContent="True" SplitterBackColor="White" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="">
                                    <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Income_Cetagoty.rdlc" ReportPath="Accounts\AccountsAnalysis\Report\Monthly_Income.rdlc">
                                        <DataSources>
                                            <rsweb:ReportDataSource DataSourceId="Monthly_IncomeODS" Name="DataSet1" />
                                        </DataSources>
                                    </LocalReport>
                                </rsweb:ReportViewer>
                                <asp:ObjectDataSource ID="Monthly_IncomeODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.Income_MonthlyTableAdapter">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                        <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" Type="DateTime" />
                                        <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" Type="DateTime" />
                                    </SelectParameters>
                                </asp:ObjectDataSource>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div id="Expense" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <ul class="nav nav-tabs z-depth-1">
                        <li class="nav-item"><a class="nav-link active" href="#Expense_Daily" data-toggle="tab" role="tab" aria-expanded="true">Daily</a></li>
                        <li class="nav-item"><a class="nav-link" href="#Expense_Monthly" data-toggle="tab" role="tab" aria-expanded="false">Monthly</a></li>
                    </ul>
                    <div class="tab-content z-depth-1">
                        <div id="Expense_Daily" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
                            <div class="table-responsive">
                                <rsweb:ReportViewer ID="Daily_Expense_ReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="" Height="" AsyncRendering="False" SizeToReportContent="True" SplitterBackColor="White" BackColor="White">
                                    <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Income_Cetagoty.rdlc" ReportPath="Accounts\AccountsAnalysis\Report\Daily_Expense.rdlc">
                                        <DataSources>
                                            <rsweb:ReportDataSource DataSourceId="DailyExpenseODS" Name="DataSet1" />
                                        </DataSources>
                                    </LocalReport>
                                </rsweb:ReportViewer>
                                <asp:ObjectDataSource ID="DailyExpenseODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.Expense_DailyTableAdapter">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                        <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" Type="String" />
                                        <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" Type="String" />
                                    </SelectParameters>
                                </asp:ObjectDataSource>
                            </div>
                        </div>
                        <div id="Expense_Monthly" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                            <div class="table-responsive">
                                <rsweb:ReportViewer ID="Monthly_Expense_ReportViewer" runat="server" AsyncRendering="False" BackColor="White" Font-Names="Verdana" Font-Size="8pt" Height="" SizeToReportContent="True" SplitterBackColor="White" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="">
                                    <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Income_Cetagoty.rdlc" ReportPath="Accounts\AccountsAnalysis\Report\Monthly_Expense.rdlc">
                                        <DataSources>
                                            <rsweb:ReportDataSource DataSourceId="MonthlyExpenseODS" Name="DataSet1" />
                                        </DataSources>
                                    </LocalReport>
                                </rsweb:ReportViewer>
                                <asp:ObjectDataSource ID="MonthlyExpenseODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.PaymentDataSetTableAdapters.Expense_MonthlyTableAdapter">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                        <asp:ControlParameter ControlID="FormDateTextBox" Name="From_Date" PropertyName="Text" Type="String" />
                                        <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" Type="String" />
                                    </SelectParameters>
                                </asp:ObjectDataSource>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>


    <script src="/JS/bootstrap-multiselect.js"></script>
    <script type="text/javascript">
        $('[id*=RoleListBox]').multiselect({
            includeSelectAllOption: true,
            enableFiltering: true,
            includeResetOption: true,
            includeResetDivider: true,
            nonSelectedText: 'All Payment Role'
        });

        $(function () {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            $('[id*=RoleListBox]').multiselect({
                includeSelectAllOption: true,
                enableFiltering: true,
                includeResetOption: true,
                includeResetDivider: true
            });

            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            if ($('[id*=SectionDropDownList]').find('option').length > 1) {
                $(".S_Show").show();
            }
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 !== a && 31 < a && (48 > a || 57 < a) ? false : true };
    </script>
</asp:Content>
