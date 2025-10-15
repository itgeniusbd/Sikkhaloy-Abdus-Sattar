<%@ Page Title="Debug Failed Subjects" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Debug Failed Subjects</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container-fluid">
        <h3>?? Debug Failed Subjects Data</h3>
        
        <div class="form-inline mb-4">
            <div class="form-group">
                <label>Class:</label>
                <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" 
                    DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True">
                    <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="ClassSQL" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) ORDER BY CreateClass.ClassID">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
            
            <div class="form-group">
                <label>Exam:</label>
                <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" 
                    DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" AutoPostBack="True">
                    <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="ExamSQL" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) ORDER BY Exam_Name.ExamID">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
            
            <div class="form-group">
                <label>Student ID:</label>
                <asp:TextBox ID="StudentIDTextBox" runat="server" CssClass="form-control" placeholder="1801" />
            </div>
        </div>

        <% if (ClassDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedIndex > 0) { %>
        
        <h4>?? Raw Subject Data for Debugging</h4>
        <div class="table-responsive">
            <asp:GridView ID="DebugGridView" runat="server" CssClass="table table-striped table-bordered" 
                AutoGenerateColumns="False" DataSourceID="DebugDataSource">
                <Columns>
                    <asp:BoundField DataField="StudentID" HeaderText="Student ID" />
                    <asp:BoundField DataField="StudentsName" HeaderText="Student Name" />
                    <asp:BoundField DataField="SubjectName" HeaderText="Subject Name" />
                    <asp:BoundField DataField="SubjectGrades" HeaderText="Subject Grades" />
                    <asp:BoundField DataField="PassStatus_Subject" HeaderText="Pass Status" />
                    <asp:BoundField DataField="ObtainedMark_ofSubject" HeaderText="Obtained Mark" />
                    <asp:BoundField DataField="IS_Add_InExam" HeaderText="Is Added In Exam" />
                    <asp:BoundField DataField="ConditionMatch" HeaderText="Condition Match" />
                </Columns>
            </asp:GridView>
        </div>
        
        <asp:SqlDataSource ID="DebugDataSource" runat="server" 
            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="
                SELECT 
                    s.ID as StudentID,
                    s.StudentsName,
                    sub.SubjectName,
                    ers.SubjectGrades,
                    ers.PassStatus_Subject,
                    ers.ObtainedMark_ofSubject,
                    ISNULL(ers.IS_Add_InExam, 1) as IS_Add_InExam,
                    CASE 
                        WHEN UPPER(LTRIM(RTRIM(ISNULL(ers.SubjectGrades, '')))) = 'F' THEN 'Grade=F'
                        WHEN UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F') THEN 'Status=FAIL'
                        WHEN UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS') THEN 'Absent'
                        ELSE 'NO MATCH'
                    END as ConditionMatch
                FROM Exam_Result_of_Subject ers
                INNER JOIN Subject sub ON ers.SubjectID = sub.SubjectID
                INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                INNER JOIN Student s ON sc.StudentID = s.StudentID
                WHERE erst.SchoolID = @SchoolID 
                    AND erst.EducationYearID = @EducationYearID 
                    AND erst.ClassID = @ClassID 
                    AND erst.ExamID = @ExamID
                    AND s.Status = 'Active'
                    AND (@StudentID = '' OR s.ID = @StudentID)
                ORDER BY s.ID, sub.SubjectName">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="StudentIDTextBox" Name="StudentID" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>
        
        <% } %>
    </div>
</asp:Content>