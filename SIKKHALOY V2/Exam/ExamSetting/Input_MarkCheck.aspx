<%@ Page Title="Inputted Exam Marks Check" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Input_MarkCheck.aspx.cs" Inherits="EDUCATION.COM.Exam.ExamSetting.Input_MarkCheck" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid th { border: 1px solid #000; }
        .mGrid td { border: 1px solid #000 !important; }
        .mGrid td p{ text-align: right; margin: 0; padding-right: 5px; }
        .Part_Incom { color:#ff0000}

        .bg_completed { background-color:#80cbc4; color: #000; }
        .bg_Incomplete { background-color:#ffcdd2; color: #000; }

        .pddi { padding:5px 10px;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Inputted Exam Marks Check</h3>
    <a href="Subject_Wise_Marks_Check.aspx">Subject Wise Marks Check >></a>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY ISNULL(SN, 9999)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True">
                <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT ExamID, SchoolID, RegistrationID, EducationYearID, ExamName, Date FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <div class="table-responsive">
        <%if (InputCheckGridView.Rows.Count > 0)
         {%>
        <div class="bg_completed pull-left pddi">Marks Input completed</div>
        <div class="bg_Incomplete pull-left pddi">Marks Input Incompleted</div>
        <%}%>

        <asp:GridView ID="InputCheckGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="SubjectID" DataSourceID="InputCheckSQL" CssClass="mGrid">
            <Columns>
                <asp:BoundField DataField="SubjectName" HeaderText="Subject" SortExpression="SubjectName" />
                <asp:BoundField DataField="Total_Student" HeaderText="Total Student" ReadOnly="True" SortExpression="Total_Student" />
                <asp:TemplateField HeaderText="Inputted Mark">
                    <ItemTemplate>
                        <asp:HiddenField ID="SubjectID_HF" runat="server" Value='<%# Eval("SubjectID") %>' />
                        <asp:Repeater runat="server" DataSourceID="SubExamSQL">
                            <ItemTemplate>
                                <p><%# Eval("SubExamName") %> <b><%# Eval("Total_Student") %></b></p>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:SqlDataSource ID="SubExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        Sub_Exam_T.SubExamID, Sub_Exam_T.SubExamName, ISNULL(Total_Stu.Total_Student, 0) AS Total_Student
FROM (SELECT COUNT(Exam_Obtain_Marks.StudentID) AS Total_Student, Exam_Obtain_Marks.SubExamID, Exam_SubExam_Name.SubExamName, Exam_Obtain_Marks.SchoolID, Exam_Obtain_Marks.SubjectID, 
                                                    Exam_Obtain_Marks.ClassID, Exam_Obtain_Marks.ExamID, Exam_Obtain_Marks.EducationYearID
                          FROM            Exam_Obtain_Marks INNER JOIN
                                                    Student ON Exam_Obtain_Marks.StudentID = Student.StudentID LEFT OUTER JOIN
                                                    Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID
                          WHERE        (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND 
                                                    (Exam_Obtain_Marks.SubjectID = @SubjectID) AND (Student.Status = N'Active')
                          GROUP BY Exam_Obtain_Marks.SubExamID, Exam_SubExam_Name.SubExamName, Exam_Obtain_Marks.SchoolID, Exam_Obtain_Marks.SubjectID, Exam_Obtain_Marks.ClassID, Exam_Obtain_Marks.ExamID, 
                                                    Exam_Obtain_Marks.EducationYearID) AS Total_Stu RIGHT OUTER JOIN
                             (SELECT        TOP (100) PERCENT Exam_SubExam_Name_1.SubExamName, Exam_Full_Marks.SubExamID, Exam_Full_Marks.SchoolID, Exam_Full_Marks.SubjectID, Exam_Full_Marks.ExamID, Exam_Full_Marks.ClassID, 
                                                         Exam_Full_Marks.EducationYearID
                               FROM            Exam_Full_Marks LEFT OUTER JOIN
                                                         Exam_SubExam_Name AS Exam_SubExam_Name_1 ON Exam_Full_Marks.SubExamID = Exam_SubExam_Name_1.SubExamID
                               WHERE        (Exam_Full_Marks.SchoolID = @SchoolID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.ClassID = @ClassID) AND 
                                                         (Exam_Full_Marks.SubjectID = @SubjectID)
                               ORDER BY Exam_SubExam_Name_1.Sub_ExamSN) AS Sub_Exam_T ON Total_Stu.SubjectID = Sub_Exam_T.SubjectID AND ISNULL(Total_Stu.SubExamID, 0) = ISNULL(Sub_Exam_T.SubExamID, 0) AND 
                         Total_Stu.SchoolID = Sub_Exam_T.SchoolID AND Total_Stu.ExamID = Sub_Exam_T.ExamID AND Total_Stu.ClassID = Sub_Exam_T.ClassID AND Total_Stu.EducationYearID = Sub_Exam_T.EducationYearID">
                            <SelectParameters>
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="SubjectID_HF" Name="SubjectID" PropertyName="Value" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="InputCheckSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Subject.SN, Subject.SubjectID, Subject.SubjectName, S_T.Total_Student
FROM            Exam_Full_Marks INNER JOIN
                         Subject ON Exam_Full_Marks.SubjectID = Subject.SubjectID INNER JOIN
                             (SELECT        COUNT(Student.StudentID) AS Total_Student, StudentRecord.SubjectID
                               FROM            StudentsClass INNER JOIN
                                                         Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN
                                                         StudentRecord ON StudentsClass.StudentClassID = StudentRecord.StudentClassID
                               WHERE        (Student.Status = 'Active') AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)
                               GROUP BY StudentRecord.SubjectID) AS S_T ON Subject.SubjectID = S_T.SubjectID
WHERE        (Exam_Full_Marks.SchoolID = @SchoolID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ClassID = @ClassID) AND (Exam_Full_Marks.ExamID = @ExamID)
ORDER BY Subject.SN">
            <SelectParameters>
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <script>
        $(function () {
            $("[id*=InputCheckGridView] tr:not(:first)").each(function () {
                var Total_Student = $(this).find("td:eq(1)").html();
                var Completed = 0;
                var Incompete = 0;
                var Total_SubExam = 0;

                $($(this).find("td:eq(2) > p")).each(function () {
                    var Sub_Student = $(this).find('b').html();

                    if (Total_Student == Sub_Student) {
                        $(this).addClass('bg_completed');
                        Completed++;
                    }
                    else if (Sub_Student == 0) {
                        Incompete++;
                        $(this).addClass('bg_Incomplete');
                    }
                    else {
                        $(this).addClass('Part_Incom');

                    }

                    Total_SubExam++;
                });

                if (Completed == Total_SubExam)
                    $(this).addClass('bg_completed');

                if (Incompete == Total_SubExam)
                    $(this).addClass('bg_Incomplete');
            });
        });
    </script>
</asp:Content>
