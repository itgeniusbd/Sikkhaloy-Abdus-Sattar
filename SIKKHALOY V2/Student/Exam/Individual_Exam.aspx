<%@ Page Title="Individual Exam" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Individual_Exam.aspx.cs" Inherits="EDUCATION.COM.Student.Exam.Individual_Exam" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint">Individual Exam Result</h3>
    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
        <ContentTemplate>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True" AutoPostBack="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID INNER JOIN Exam_Publish_Setting ON Exam_Result_of_Student.Publish_SettingID = Exam_Publish_Setting.Publish_SettingID WHERE (Exam_Name.SchoolID = @SchoolID) AND (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.StudentClassID = @StudentClassID) AND (Exam_Publish_Setting.IS_Published = 1)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="Select Exam" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                </div>
            </div>

            <%if (ExamDropDownList.SelectedIndex > 0)
                { %>
            <div class="table-responsive">
                <rsweb:ReportViewer ID="ResultReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" ShowRefreshButton="false" WaitMessageFont-Size="14pt"
                    ShowPrintButton="true" AsyncRendering="False" SizeToReportContent="True" SplitterBackColor="White" DocumentMapWidth="" ZoomMode="PageWidth" Width="100%" Height="100%" BackColor="White">
                    <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Individual_Result.rdlc" ReportPath="Report_Individual_Result.rdlc">
                        <DataSources>
                            <rsweb:ReportDataSource DataSourceId="ExamResultODS" Name="DataSet1" />
                        </DataSources>
                    </LocalReport>
                </rsweb:ReportViewer>
                <asp:ObjectDataSource ID="ExamResultODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Profile_newTableAdapter">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
                <asp:ObjectDataSource ID="GradingSystemODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Exam_Grading_SystemTableAdapter">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        <asp:SessionParameter Name="ClassID" SessionField="ClassID" Type="Int32" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
                <asp:ObjectDataSource ID="SchoolInfoODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.SchoolInfoTableAdapter">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </div>
            <%} %>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="/CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script>
        $(function () {
            $("#_1").addClass("active");
        });
    </script>
</asp:Content>
