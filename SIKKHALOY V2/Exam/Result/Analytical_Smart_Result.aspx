<%@ Page Title="Analytical Smart Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Analytical_Smart_Result.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Analytical_Smart_Result" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<%@ Register TagPrefix="rsweb" Namespace="Microsoft.Reporting.WebForms" Assembly="Microsoft.ReportViewer.WebForms" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Analytical_Smart_Result.css?v=3" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="d-print-none">Analytical Smart Result</h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True">
                <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY CreateClass.ClassID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="ExamName" DataValueField="ExamID" AutoPostBack="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged" OnDataBound="ExamDropDownList_DataBound">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Name.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) ORDER BY Exam_Name.ExamID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>
    <asp:ObjectDataSource ID="SchoolInfoODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.SchoolInfoTableAdapter">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:ObjectDataSource>


    <% if (ClassDropDownList.SelectedIndex != 0 && ExamDropDownList.SelectedIndex != 0)
        {%>
    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item"><a class="nav-link active" href="#tab1" data-toggle="tab" role="tab" aria-expanded="true">Grade Letter</a></li>
        <li class="nav-item"><a class="nav-link" href="#tab2" data-toggle="tab" role="tab" aria-expanded="false">Individual Subject</a></li>
        <li class="nav-item"><a class="nav-link" href="#tab3" data-toggle="tab" role="tab" aria-expanded="false">Unsuccessful Summary</a></li>
        <li class="nav-item"><a class="nav-link" href="#tab4" data-toggle="tab" role="tab" aria-expanded="false">Unsuccessful Student</a></li>
        <li class="nav-item"><a class="nav-link" href="#tab5" data-toggle="tab" role="tab" aria-expanded="false">Pass & Fail</a></li>
    </ul>

    <div class="tab-content card">
        <div id="tab1" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
            <rsweb:ReportViewer ID="Examinee_Vs_GradeRV" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="" Height="100%" AsyncRendering="False" SizeToReportContent="True" SplitterBackColor="White" BackColor="White" ToolBarItemHoverBackColor="White">
                <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Examinee_Vs_Grade.rdlc" ReportPath="Report_Examinee_Vs_Grade.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="Examinee_Vs_GradeODS" Name="DataSet1" />
                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>
            <asp:ObjectDataSource ID="Examinee_Vs_GradeODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Exam_VS_SubTableAdapter">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                </SelectParameters>
            </asp:ObjectDataSource>
        </div>

        <div id="tab2" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <div class="table-responsive">
                 <rsweb:ReportViewer ID="Individual_SubjectRV" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="100%" Height="400px" AsyncRendering="True" SizeToReportContent="False" SplitterBackColor="White" BackColor="White">
                       <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Sta_Sub.rdlc" ReportPath="Report_Sta_Sub.rdlc">
                        <DataSources>
                            <rsweb:ReportDataSource DataSourceId="Individual_SubjectODS" Name="DataSet1" />
                        </DataSources>
                    </LocalReport>
                </rsweb:ReportViewer>
                <asp:ObjectDataSource ID="Individual_SubjectODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Statistics_of_SubTableAdapter">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </div>
        </div>

        <div id="tab3" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <div class="text-center">
                <h2 id="InsName"></h2>
                <h4 class="text-danger">Unsuccessful Student Summary</h4>
                <h5 id="ClassName"></h5>
            </div>

            <asp:GridView ID="UnSummaryGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataSourceID="UnSummarySQL">
                <Columns>
                    <asp:BoundField DataField="Sub_Failed" HeaderText="Number of Subject" ReadOnly="True" SortExpression="Sub_Failed" />
                    <asp:BoundField DataField="Student_Count" HeaderText="Number of Student" ReadOnly="True" SortExpression="Student_Count" />
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="UnSummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Sub_Failed, COUNT(S_T.StudentID) AS Student_Count FROM (SELECT Exam_Result_of_Subject.StudentID, COUNT(Exam_Result_of_Subject.SubjectID) AS Sub_Failed
FROM            Student INNER JOIN
                         Exam_Result_of_Subject ON Student.StudentID = Exam_Result_of_Subject.StudentID INNER JOIN
                         Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN
                         Exam_Publish_Setting ON Exam_Result_of_Student.Publish_SettingID = Exam_Publish_Setting.Publish_SettingID
WHERE        (Exam_Result_of_Subject.PassStatus_Subject = 'F') AND (Student.Status = N'Active') AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) 
                         AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.IS_Add_InExam = 1) AND 
                         (Exam_Publish_Setting.IS_Fail_Enable_Optional_Subject = 1) OR
                         (Exam_Result_of_Subject.PassStatus_Subject = 'F') AND (Student.Status = N'Active') AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) 
                         AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.IS_Add_InExam = 1) AND 
                         (Exam_Result_of_Subject.SubjectType = 'Compulsory')
GROUP BY Exam_Result_of_Subject.StudentID) AS S_T GROUP BY Sub_Failed ">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div id="tab4" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <div class="table-responsive">
                <rsweb:ReportViewer ID="UnsuccessfulRV" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="100%" Height="400px" SizeToReportContent="true" BackColor="White">
                       <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Unsuccess_Student.rdlc" ReportPath="Report_Unsuccess_Student.rdlc">
                        <DataSources>
                            <rsweb:ReportDataSource DataSourceId="UnsuccessfulODS" Name="DataSet1" />
                        </DataSources>
                    </LocalReport>
                </rsweb:ReportViewer>
                <asp:ObjectDataSource ID="UnsuccessfulODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.UnsuccessFull_StudentTableAdapter">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </div>
        </div>

        <div id="tab5" class="tab-pane fade" role="tabpanel" aria-expanded="false">
            <rsweb:ReportViewer ID="Pass_FailRV" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt" Width="" Height="520px" AsyncRendering="False" SizeToReportContent="False" BackColor="White">
                <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Pass_Fail.rdlc" ReportPath="Report_Pass_Fail.rdlc">
                    <DataSources>
                        <rsweb:ReportDataSource DataSourceId="Pass_FailODS" Name="DataSet1" />
                    </DataSources>
                </LocalReport>
            </rsweb:ReportViewer>
            <asp:ObjectDataSource ID="Pass_FailODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Exam_PassTableAdapter">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                </SelectParameters>
            </asp:ObjectDataSource>
        </div>
    </div>

    <input class="btn btn-primary d-print-none" onclick="window.print(); return false" type="button" value="Print" />
    <%} %>

    <script>
        $(function () {
            var Class = "";
            if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                Class ="Class: "+ $('[id*=ClassDropDownList] :selected').text();
            }

            var Exam = "";
            if ($('[id*=ExamDropDownList] :selected').index() > 0) {
                Exam =". Exam: "+ $('[id*=ExamDropDownList] :selected').text();
            }

            $('#InsName').text($("#InstitutionName").text());
            $('#ClassName').text(Class+Exam);
        });
    </script>
</asp:Content>
