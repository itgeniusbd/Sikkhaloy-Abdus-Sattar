<%@ Page Title="Cumulative Exam" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Cumulative_Exam.aspx.cs" Inherits="EDUCATION.COM.Student.Exam.Cumulative_Exam" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <link href="../../Exam/Result/Assets/Cumulative_Result.css?v=<%= DateTime.Now.Ticks %>" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Cumulative Result</h3>

    <div class="form-inline" style="margin-bottom: 15px;">
        <div class="form-group">
            <asp:DropDownList ID="Cum_ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="CumiExamSQL" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID"
                AppendDataBoundItems="True" OnSelectedIndexChanged="Cum_ExamDropDownList_SelectedIndexChanged">
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

    <asp:HiddenField ID="HiddenTeacherSign" runat="server" />
    <asp:HiddenField ID="HiddenPrincipalSign" runat="server" />

    <asp:Panel ID="ResultPanel" runat="server" Visible="false">
        <asp:Repeater ID="ResultRepeater" runat="server" OnItemDataBound="ResultRepeater_ItemDataBound">
            <ItemTemplate>
                <div class="result-card">
                    <div class="header">
                        <img src="/Handeler/SchoolLogo.ashx?SLogo=<%# Eval("SchoolID") %>" alt="School Logo" onerror="this.style.display='none';" />
                        <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" alt="Student Photo" class="student-photo" onerror="this.style.display='none';" />
                        <h2><%# Eval("SchoolName") %></h2>
                        <p><i class="fa fa-map-marker icon-fallback" data-fallback="📍"></i> <%# Eval("Address") %></p>
                        <p><i class="fa fa-phone icon-fallback" data-fallback="📞"></i> <%# Eval("Phone") %></p>
                    </div>

                    <div>
                        <p class="Exam_name">Result Card</p>
                        <p class="title"><%# Eval("ExamName") %></p>
                    </div>

                    <div class="top-section">
                        <div class="info-summary">
                            <table class="info-table">
                                <tr>
                                    <td>Name:</td>
                                    <td colspan="3"><b><%# Eval("StudentsName") %></b></td>
                                </tr>
                                <%# GetDynamicInfoRow(Container.DataItem) %>
                                <tr>
                                    <td>Roll:</td>
                                    <td><%# Eval("RollNo") %></td>
                                    <td>ID:</td>
                                    <td><%# Eval("ID") %></td>
                                </tr>
                            </table>
                            <%# GetAttendanceTableHtml(Container.DataItem) %>
                        </div>

                        <div class="grade-chart">
                            <table>
                                <tr><th>Marks</th><th>Grade</th><th>Point</th></tr>
                                <asp:Repeater ID="GradingSystemRepeater" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("MARKS") %></td>
                                            <td><%# Eval("Grades") %></td>
                                            <td><%# String.Format("{0:F1}", Eval("Point")) %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </table>
                        </div>
                    </div>

                    <%# GenerateSubjectMarksTable(Eval("StudentResultID").ToString(), Eval("Student_Grade").ToString(), Eval("Student_Point") == DBNull.Value ? 0m : Convert.ToDecimal(Eval("Student_Point"))) %>

                    <div class="footer">
                        <div>
                            <div class="SignTeacher signature-container"></div>
                            <div class="Teacher signature-label">Class Teacher</div>
                        </div>
                        <div>
                            <div class="SignGuardian signature-container"></div>
                            <div class="Guardian signature-label">Guardian</div>
                        </div>
                        <div>
                            <div class="SignHead signature-container"></div>
                            <div class="Head signature-label">Principal</div>
                        </div>
                    </div>
                    <p class="note">WD: Working Days  FM: Full Marks  OM: Obtained Marks  PC: Position in Class  PS: Position in Section  HMC: Highest Marks in Class  HMS: Highest Marks in Section</p>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>

    <script type="text/javascript">
        $(function () {
            $("#_2").addClass("active");
            ApplySignatures();
        });

        function ApplySignatures() {
            var teacherSign = document.getElementById('<%= HiddenTeacherSign.ClientID %>').value;
            var principalSign = document.getElementById('<%= HiddenPrincipalSign.ClientID %>').value;

            if (teacherSign) {
                $('.SignTeacher').each(function () {
                    $(this).html('<img src="' + teacherSign + '" style="max-height:35px; max-width:120px;" />');
                });
            }
            if (principalSign) {
                $('.SignHead').each(function () {
                    $(this).html('<img src="' + principalSign + '" style="max-height:35px; max-width:120px;" />');
                });
            }
        }
    </script>
</asp:Content>
