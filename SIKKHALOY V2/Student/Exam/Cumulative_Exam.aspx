<%@ Page Title="Cumulative Exam" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Cumulative_Exam.aspx.cs" Inherits="EDUCATION.COM.Student.Exam.Cumulative_Exam" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Cumulative Result</h3>
    <asp:UpdatePanel ID="UpdatePanel6" runat="server">
        <ContentTemplate>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="Cum_ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="CumiExamSQL" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID" AppendDataBoundItems="True" OnSelectedIndexChanged="Cum_ExamDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="CumiExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT Exam_Cumulative_Name.CumulativeNameID, Exam_Cumulative_Name.CumulativeResultName FROM Exam_Cumulative_Name INNER JOIN Exam_Cumulative_Student ON Exam_Cumulative_Name.CumulativeNameID = Exam_Cumulative_Student.CumulativeNameID INNER JOIN Exam_Cumulative_Setting ON Exam_Cumulative_Student.Cumulative_SettingID = Exam_Cumulative_Setting.Cumulative_SettingID WHERE (Exam_Cumulative_Name.SchoolID = @SchoolID) AND (Exam_Cumulative_Name.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Setting.IS_Published = 1) AND (Exam_Cumulative_Student.StudentClassID = @StudentClassID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <%if (Cum_ExamDropDownList.SelectedIndex > 0)
                {%>
            <div class="table-responsive">
                <rsweb:ReportViewer ID="Cu_ResultReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" ShowRefreshButton="False" WaitMessageFont-Size="14pt" AsyncRendering="False" SizeToReportContent="True" SplitterBackColor="White" DocumentMapWidth="" ZoomMode="PageWidth" Width="100%" Height="100%" BackColor="White">
                    <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Individual_Result.rdlc" ReportPath="Exam\CumulativeResult\Cumulative_Sub_Exam.rdlc">
                        <DataSources>
                            <rsweb:ReportDataSource DataSourceId="Cum_ExamResultODS" Name="DataSet1" />
                        </DataSources>
                    </LocalReport>
                </rsweb:ReportViewer>
                <asp:ObjectDataSource ID="Cum_ExamResultODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam.CumulativeResult.Cu_ExamTableAdapters.Cumi_Student_ProfileTableAdapter">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="Cum_ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                        <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
                <asp:ObjectDataSource ID="Cu_GradingSystemODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Cumuletive_Grading_SystemTableAdapter">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        <asp:SessionParameter Name="ClassID" SessionField="ClassID" Type="Int32" />
                        <asp:ControlParameter ControlID="Cum_ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </div>
            <%} %>
        </ContentTemplate>
    </asp:UpdatePanel>

    <script>
        $(function () {
            $("#_2").addClass("active");
        });
    </script>
</asp:Content>
