<%@ Page Title="Analytical Smart Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Analytical_Smart_Result.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Analytical_Smart_Result" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Analytical_Smart_Result.css?v=5" rel="stylesheet" />
    <link href="Assets/Analytical_Result.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container-fluid">
        <h3 class="d-print-none mb-4">📊 Analytical Smart Result</h3>

        <div class="form-inline NoPrint mb-4">
            <div class="form-group">
                <label for="<%= ClassDropDownList.ClientID %>" class="mr-2">Class:</label>
                <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" 
                    CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" 
                    DataValueField="ClassID" AutoPostBack="True" 
                    OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                    <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="ClassSQL" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY CreateClass.ClassID">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
            
            <div class="form-group">
                <label for="<%= ExamDropDownList.ClientID %>" class="mr-2">Exam:</label>
                <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" 
                    DataSourceID="ExamNameSQl" DataTextField="ExamName" DataValueField="ExamID" 
                    AutoPostBack="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged" 
                    OnDataBound="ExamDropDownList_DataBound" AppendDataBoundItems="True">
                    <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="ExamNameSQl" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Name.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) ORDER BY Exam_Name.ExamID">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
            
            <div class="form-group">
                <button type="button" onclick="window.print()" class="btn btn-primary">
                    🖨️ Print Report
                </button>
            </div>
        </div>

        <!-- Add SchoolInfoODS for dynamic school name -->
        <asp:ObjectDataSource ID="SchoolInfoODS" runat="server" 
            OldValuesParameterFormatString="original_{0}" 
            SelectMethod="GetData" 
            TypeName="EDUCATION.COM.Exam_ResultTableAdapters.SchoolInfoTableAdapter">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:ObjectDataSource>

        <% if (ClassDropDownList.SelectedIndex != 0 && ExamDropDownList.SelectedIndex != 0) { %>
        
        <!-- School Header -->
        <div class="report-header">
            <h2><asp:Label ID="SchoolNameLabel" runat="server" Text="School Name"></asp:Label></h2>
            <h4 class="text-primary">📊 Analytical Smart Result</h4>
            <h5 class="text-muted"><asp:Label ID="ClassExamLabel" runat="server" Text=""></asp:Label></h5>
            <hr />
        </div>

        <!-- Tab Navigation -->
        <ul class="nav nav-tabs z-depth-1 d-print-none" id="resultTabs" role="tablist">
            <li class="nav-item">
                <a class="nav-link active" id="grade-tab" data-toggle="tab" href="#tab1" 
                   role="tab" aria-controls="tab1" aria-selected="true">
                    📈 Grade Distribution
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="subject-tab" data-toggle="tab" href="#tab2" 
                   role="tab" aria-controls="tab2" aria-selected="false">
                    📚 Subject Statistics
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="unsummary-tab" data-toggle="tab" href="#tab3" 
                   role="tab" aria-controls="tab3" aria-selected="false">
                    📉 Unsuccessful Summary
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="unstudent-tab" data-toggle="tab" href="#tab4" 
                   role="tab" aria-controls="tab4" aria-selected="false">
                    👥 Unsuccessful Students
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="passfail-tab" data-toggle="tab" href="#tab5" 
                   role="tab" aria-controls="tab5" aria-selected="false">
                    ✅❌ Pass & Fail
                </a>
            </li>
        </ul>

        <!-- Tab Content -->
        <div class="tab-content" id="resultTabContent">
            <!-- Tab 1: Grade Distribution -->
            <div class="tab-pane fade show active" id="tab1" role="tabpanel" aria-labelledby="grade-tab">
                <h4 class="mb-3">📈 Grade Distribution</h4>
                <div class="chart-container">
                    <asp:Literal ID="GradeChartLiteral" runat="server"></asp:Literal>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="GradeGridView" runat="server" CssClass="report-table" 
                        AutoGenerateColumns="False" DataSourceID="GradeDataSource"
                        EmptyDataText="<div class='no-data-message'>📈 No grade data available for selected class and exam.</div>">
                        <Columns>
                            <asp:BoundField DataField="Grade" HeaderText="Grade" />
                            <asp:BoundField DataField="StudentCount" HeaderText="Number of Students" />
                            <asp:BoundField DataField="Percentage" HeaderText="Percentage %" DataFormatString="{0:F2}" />
                        </Columns>
                        <EmptyDataRowStyle CssClass="no-data-message" />
                    </asp:GridView>
                </div>
                <asp:SqlDataSource ID="GradeDataSource" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="
                        SELECT 
                            Student_Grade as Grade,
                            COUNT(*) as StudentCount,
                            CAST((COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Exam_Result_of_Student WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID)) AS DECIMAL(5,2)) as Percentage
                        FROM Exam_Result_of_Student 
                        WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
                        GROUP BY Student_Grade
                        ORDER BY 
                            CASE Student_Grade 
                                WHEN 'A+' THEN 1 
                                WHEN 'A' THEN 2 
                                WHEN 'A-' THEN 3 
                                WHEN 'B' THEN 4 
                                WHEN 'C' THEN 5 
                                WHEN 'D' THEN 6 
                                WHEN 'F' THEN 7 
                                ELSE 8 
                            END">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <!-- Tab 2: Subject Statistics -->
            <div class="tab-pane fade" id="tab2" role="tabpanel" aria-labelledby="subject-tab">
                <h4 class="mb-3">📚 Subject-wise Statistics</h4>
                <div class="alert alert-info d-print-none">
                    <small><i class="fa fa-info-circle"></i> <strong>Tip:</strong> Scroll horizontally to view all columns on smaller screens.</small>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="SubjectGridView" runat="server" CssClass="report-table" 
                        AutoGenerateColumns="False" DataSourceID="SubjectDataSource"
                        EmptyDataText="<div class='no-data-message'>📚 No subject statistics available for selected class and exam.</div>">
                        <Columns>
                            <asp:BoundField DataField="SubjectName" HeaderText="Subject" ItemStyle-CssClass="text-left" />
                            <asp:BoundField DataField="TotalStudents" HeaderText="Total Students" />
                            <asp:BoundField DataField="PassedStudents" HeaderText="Passed" ItemStyle-CssClass="text-success" />
                            <asp:BoundField DataField="FailedStudents" HeaderText="Failed" ItemStyle-CssClass="text-danger" />
                            <asp:BoundField DataField="PassPercentage" HeaderText="Pass %" DataFormatString="{0:F2}" />
                            <asp:BoundField DataField="HighestMarks" HeaderText="Highest Marks" />
                            <asp:BoundField DataField="LowestMarks" HeaderText="Lowest Marks" />
                            <asp:BoundField DataField="AverageMarks" HeaderText="Average Marks" DataFormatString="{0:F2}" />
                        </Columns>
                        <EmptyDataRowStyle CssClass="no-data-message" />
                    </asp:GridView>
                </div>
                <asp:SqlDataSource ID="SubjectDataSource" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="
                        SELECT 
                            s.SubjectName,
                            COUNT(DISTINCT ers.StudentResultID) as TotalStudents,
                            -- Calculate Passed Students with dynamic pass marks (33% of total marks)
                            SUM(CASE 
                                WHEN (
                                    -- First check if NOT failed by explicit fail indicators
                                    NOT (
                                        ers.SubjectGrades = 'F'
                                        OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                                        OR UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS', 'ABSENT')
                                        OR ers.ObtainedMark_ofSubject = '0'
                                        OR LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, ''))) = ''
                                        OR (
                                            ISNUMERIC(ISNULL(ers.ObtainedMark_ofSubject, '')) = 1 
                                            AND ISNUMERIC(ISNULL(ers.TotalMark_ofSubject, '')) = 1
                                            AND LEN(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) > 0
                                            AND LEN(LTRIM(RTRIM(ISNULL(ers.TotalMark_ofSubject, '')))) > 0
                                            AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                            AND CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) > 0
                                            AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) < (CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) * 0.33)
                                        )
                                    )
                                )
                                AND
                                (
                                    -- Then check if passed by any positive indicator
                                    ers.SubjectGrades IN ('A+', 'A', 'A-', 'B', 'C', 'D')
                                    OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('PASS', 'P')
                                    OR (
                                        ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                        AND ISNUMERIC(ISNULL(ers.TotalMark_ofSubject, '')) = 1
                                        AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                        AND LEN(LTRIM(RTRIM(ISNULL(ers.TotalMark_ofSubject, '')))) > 0
                                        AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                        AND CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) > 0
                                        AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) >= (CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) * 0.33)
                                    )
                                )
                                THEN 1
                                ELSE 0 
                            END) as PassedStudents,
                            -- Calculate Failed Students with dynamic pass marks (33% of total marks)
                            SUM(CASE 
                                WHEN (
                                    -- Explicit fail grade
                                    ers.SubjectGrades = 'F'
                                    OR
                                    -- Explicit fail status
                                    UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                                    OR
                                    -- Absent
                                    UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS', 'ABSENT')
                                    OR
                                    -- Zero marks or empty
                                    ers.ObtainedMark_ofSubject = '0'
                                    OR
                                    LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, ''))) = ''
                                    OR
                                    -- Marks below 33% of total marks
                                    (
                                        ISNUMERIC(ISNULL(ers.ObtainedMark_ofSubject, '')) = 1 
                                        AND ISNUMERIC(ISNULL(ers.TotalMark_ofSubject, '')) = 1
                                        AND LEN(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) > 0
                                        AND LEN(LTRIM(RTRIM(ISNULL(ers.TotalMark_ofSubject, '')))) > 0
                                        AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                        AND CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) > 0
                                        AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) < (CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) * 0.33)
                                    )
                                ) THEN 1
                                ELSE 0 
                            END) as FailedStudents,
                            -- Calculate Pass Percentage
                            CAST(CASE 
                                WHEN COUNT(DISTINCT ers.StudentResultID) > 0 THEN 
                                    (SUM(CASE 
                                        WHEN (
                                            NOT (
                                                ers.SubjectGrades = 'F'
                                                OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                                                OR UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS', 'ABSENT')
                                                OR ers.ObtainedMark_ofSubject = '0'
                                                OR LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, ''))) = ''
                                                OR (
                                                    ISNUMERIC(ISNULL(ers.ObtainedMark_ofSubject, '')) = 1 
                                                    AND ISNUMERIC(ISNULL(ers.TotalMark_ofSubject, '')) = 1
                                                    AND LEN(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) > 0
                                                    AND LEN(LTRIM(RTRIM(ISNULL(ers.TotalMark_ofSubject, '')))) > 0
                                                    AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                                    AND CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) > 0
                                                    AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) < (CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) * 0.33)
                                                )
                                            )
                                        )
                                        AND
                                        (
                                            ers.SubjectGrades IN ('A+', 'A', 'A-', 'B', 'C', 'D')
                                            OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('PASS', 'P')
                                            OR (
                                                ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                                AND ISNUMERIC(ISNULL(ers.TotalMark_ofSubject, '')) = 1
                                                AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                                AND LEN(LTRIM(RTRIM(ISNULL(ers.TotalMark_ofSubject, '')))) > 0
                                                AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                                AND CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) > 0
                                                AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) >= (CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) * 0.33)
                                            )
                                        )
                                        THEN 1
                                        ELSE 0 
                                    END) * 100.0 / COUNT(DISTINCT ers.StudentResultID))
                                ELSE 0
                            END AS DECIMAL(5,2)) as PassPercentage,
                            -- Calculate Highest Marks
                            ISNULL(MAX(CASE 
                                WHEN ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                     AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                     AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                THEN CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) 
                                ELSE NULL 
                            END), 0) as HighestMarks,
                            -- Calculate Lowest Marks (excluding absent but including zero)
                            ISNULL(MIN(CASE 
                                WHEN ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                     AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                     AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                THEN CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) 
                                ELSE NULL 
                            END), 0) as LowestMarks,
                            -- Calculate Average Marks (excluding absent but including zero)
                            CAST(ISNULL(AVG(CASE 
                                WHEN ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                     AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                     AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                THEN CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) 
                                ELSE NULL 
                            END), 0) AS DECIMAL(5,2)) as AverageMarks
                        FROM Exam_Result_of_Subject ers
                        INNER JOIN Subject s ON ers.SubjectID = s.SubjectID
                        INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                        WHERE ers.SchoolID = @SchoolID 
                            AND ers.EducationYearID = @EducationYearID 
                            AND erst.ClassID = @ClassID 
                            AND erst.ExamID = @ExamID
                            AND ISNULL(ers.IS_Add_InExam, 1) = 1
                        GROUP BY s.SubjectName, s.SN
                        ORDER BY s.SN">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <!-- Tab 3: Unsuccessful Summary -->
            <div class="tab-pane fade" id="tab3" role="tabpanel" aria-labelledby="unsummary-tab">
                <h4 class="mb-3 text-danger">📉 Unsuccessful Student Summary</h4>
                <div class="table-responsive">
                    <asp:GridView ID="UnSummaryGridView" CssClass="report-table" runat="server" 
                        AutoGenerateColumns="False" DataSourceID="UnSummarySQL"
                        EmptyDataText="<div class='no-data-message'>📊 Great news! All students have passed in all subjects. No unsuccessful students found.</div>">
                        <Columns>
                            <asp:BoundField DataField="Sub_Failed" HeaderText="Number of Subjects Failed" />
                            <asp:BoundField DataField="Student_Count" HeaderText="Number of Students" />
                        </Columns>
                        <EmptyDataRowStyle CssClass="no-data-message" />
                    </asp:GridView>
                </div>
                <asp:SqlDataSource ID="UnSummarySQL" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                    SelectCommand="
                        SELECT 
                            Sub_Failed, 
                            COUNT(S_T.StudentID) AS Student_Count 
                        FROM (
                            SELECT 
                                ers.StudentResultID,
                                sc.StudentID, 
                                COUNT(ers.SubjectID) AS Sub_Failed
                            FROM Exam_Result_of_Subject ers
                            INNER JOIN Exam_Result_of_Student erst ON ers.StudentResultID = erst.StudentResultID
                            INNER JOIN StudentsClass sc ON erst.StudentClassID = sc.StudentClassID
                            INNER JOIN Student s ON sc.StudentID = s.StudentID
                            WHERE (
                                -- Failure conditions with dynamic 33% pass marks
                                UPPER(LTRIM(RTRIM(ISNULL(ers.SubjectGrades, '')))) = 'F'
                                OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                                OR UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS', 'ABSENT')
                                OR ers.ObtainedMark_ofSubject = '0'
                                OR LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, ''))) = ''
                                OR (
                                    ISNUMERIC(ISNULL(ers.ObtainedMark_ofSubject, '')) = 1 
                                    AND ISNUMERIC(ISNULL(ers.TotalMark_ofSubject, '')) = 1
                                    AND LEN(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) > 0
                                    AND LEN(LTRIM(RTRIM(ISNULL(ers.TotalMark_ofSubject, '')))) > 0
                                    AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', 'ABSENT')
                                    AND CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) > 0
                                    AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) < (CAST(ers.TotalMark_ofSubject AS DECIMAL(10,2)) * 0.33)
                                )
                            )
                                AND s.Status = 'Active' 
                                AND ers.SchoolID = @SchoolID 
                                AND ers.EducationYearID = @EducationYearID 
                                AND erst.ClassID = @ClassID 
                                AND erst.ExamID = @ExamID 
                                AND ISNULL(ers.IS_Add_InExam, 1) = 1
                            GROUP BY ers.StudentResultID, sc.StudentID
                        ) AS S_T 
                        GROUP BY Sub_Failed 
                        ORDER BY Sub_Failed">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <!-- Tab 4: Unsuccessful Students -->
            <div class="tab-pane fade" id="tab4" role="tabpanel" aria-labelledby="unstudent-tab">
                <h4 class="mb-3 text-danger">👥 Unsuccessful Student's Report</h4>
                <div class="alert alert-info d-print-none">
                    <small><i class="fa fa-info-circle"></i> <strong>Understanding the Report:</strong> 
                    Shows students with their failed subjects, obtained marks (OM) and shortage from pass marks (Lack).
                    </small>
                </div>
                <div class="table-responsive">
                    <asp:Literal ID="DynamicTableLiteral" runat="server"></asp:Literal>
                </div>
            </div>

            <!-- Tab 5: Pass & Fail Summary -->
            <div class="tab-pane fade" id="tab5" role="tabpanel" aria-labelledby="passfail-tab">
                <h4 class="mb-3">✅❌ Pass & Fail Summary</h4>
                <div class="table-responsive">
                    <asp:GridView ID="PassFailGridView" runat="server" CssClass="report-table" 
                        AutoGenerateColumns="False" DataSourceID="PassFailDataSource"
                        EmptyDataText="<div class='no-data-message'>✅❌ No pass/fail data available for selected class and exam.</div>">
                        <Columns>
                            <asp:BoundField DataField="Result" HeaderText="Result" />
                            <asp:BoundField DataField="StudentCount" HeaderText="Number of Students" />
                            <asp:BoundField DataField="Percentage" HeaderText="Percentage %" DataFormatString="{0:F2}" />
                        </Columns>
                        <EmptyDataRowStyle CssClass="no-data-message" />
                    </asp:GridView>
                </div>
                <asp:SqlDataSource ID="PassFailDataSource" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="
                        SELECT 
                            CASE WHEN Student_Grade = 'F' THEN 'Fail' ELSE 'Pass' END as Result,
                            COUNT(*) as StudentCount,
                            CAST((COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Exam_Result_of_Student WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID)) AS DECIMAL(5,2)) as Percentage
                        FROM Exam_Result_of_Student 
                        WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND ClassID = @ClassID AND ExamID = @ExamID
                        GROUP BY CASE WHEN Student_Grade = 'F' THEN 'Fail' ELSE 'Pass' END
                        ORDER BY Result DESC">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>

        <%} else { %>
        <div class="alert alert-info mt-4" role="alert">
            <h4 class="alert-heading">📋 Instructions</h4>
            <p>Please select both <strong>Class</strong> and <strong>Exam</strong> to view the analytical smart result reports.</p>
            <hr>
            <p class="mb-0">The system will generate comprehensive analytics including grade distribution, subject statistics, and pass/fail analysis.</p>
        </div>
        <%} %>
    </div>

    <script>
        // Enhanced JavaScript for better UX
        $(function () {
            // Initialize tooltips and other UI enhancements
            if (typeof $ !== 'undefined') {

                // Set dynamic school name from InstitutionName element (like old code)
                if ($("#InstitutionName").length > 0) {
                    var schoolName = $("#InstitutionName").text().trim();
                    if (schoolName) {
                        // Update SchoolNameLabel if it exists
                        var schoolNameLabel = $('[id*="SchoolNameLabel"]');
                        if (schoolNameLabel.length > 0) {
                            schoolNameLabel.text(schoolName);
                        }

                        // Also update any other school name displays
                        $('.school-name-display').text(schoolName);
                    }
                }

                // Add loading state for dropdowns
                $('[id*=ClassDropDownList], [id*=ExamDropDownList]').change(function () {
                    if ($(this).val() !== "0") {
                        $(this).addClass('loading');
                        setTimeout(() => {
                            $(this).removeClass('loading');
                        }, 1000);
                    }
                });

                // Highlight zero values in statistics table
                function highlightZeroValues() {
                    $('.report-table td').each(function () {
                        var cellText = $(this).text().trim();
                        if (cellText === '0' || cellText === '0.00') {
                            $(this).addClass('zero-value');
                            $(this).css({
                                'background-color': '#fff3cd',
                                'color': '#856404',
                                'font-weight': 'bold'
                            });
                        }
                    });
                }

                // Enhanced function for unsuccessful students table
                function enhanceUnsuccessfulTable() {
                    $('.unsuccessful-table tr').each(function () {
                        var failedSubjectsCell = $(this).find('td:last-child');
                        var gradeCell = $(this).find('td:nth-child(4)'); // Updated position due to new Student ID column
                        var failedCountCell = $(this).find('td:nth-child(5)'); // Updated position due to new Student ID column

                        // Keep failed subjects as simple comma-separated text (no pills)
                        if (failedSubjectsCell.length > 0) {
                            var failedText = failedSubjectsCell.text().trim();
                            if (failedText === '' || failedText === 'No specific failed subjects found') {
                                failedSubjectsCell.html('<em style="color: #6c757d;">Data being processed...</em>');
                            }
                            // Remove the pill formatting - keep as simple text
                        }

                        // Style grade cell
                        if (gradeCell.text().trim() === 'F') {
                            gradeCell.addClass('grade-f');
                        }

                        // Style failed count cell
                        if (failedCountCell.length > 0) {
                            var count = parseInt(failedCountCell.text().trim());
                            if (count > 0) {
                                failedCountCell.addClass('failed-count');
                                // Add different colors based on count
                                if (count >= 5) {
                                    failedCountCell.css({
                                        'background-color': '#f8d7da !important',
                                        'color': '#721c24 !important'
                                    });
                                } else if (count >= 3) {
                                    failedCountCell.css({
                                        'background-color': '#fff3cd !important',
                                        'color': '#856404 !important'
                                    });
                                }
                            }
                        }
                    });
                }

                // Enhanced function for detailed unsuccessful students table
                function enhanceDetailedUnsuccessfulTable() {
                    // Handle the new detailed table structure
                    $('.unsuccessful-students-detailed tr').each(function () {
                        var studentIdCell = $(this).find('.student-id-col');
                        var studentNameCell = $(this).find('.student-name-col');

                        // Enhance student ID and name cells
                        if (studentIdCell.length > 0) {
                            studentIdCell.prepend('<i class="fa fa-id-badge" style="margin-right: 4px; color: #495057;"></i>');
                        }

                        if (studentNameCell.length > 0) {
                            studentNameCell.prepend('<i class="fa fa-user" style="margin-right: 4px; color: #495057;"></i>');
                        }

                        // Add hover effects to subject detail tables
                        $(this).find('.subject-detail-table tr').hover(
                            function () {
                                $(this).css('background-color', '#e3f2fd');
                            },
                            function () {
                                $(this).css('background-color', '');
                            }
                        );

                        // Add tooltips to OM and Lack cells
                        $(this).find('.om-cell').attr('title', 'Obtained Marks in this subject');
                        $(this).find('.lack-cell').attr('title', 'Shortage from required pass marks (33)');

                        // Highlight high shortage values
                        $(this).find('.lack-cell').each(function () {
                            var lackValue = parseFloat($(this).text().trim());
                            if (lackValue >= 20) {
                                $(this).css({
                                    'background-color': '#f5c6cb !important',
                                    'font-weight': 'bold'
                                });
                            } else if (lackValue >= 10) {
                                $(this).css({
                                    'background-color': '#ffeaa7 !important'
                                });
                            }
                        });

                        // Highlight very low marks
                        $(this).find('.om-cell').each(function () {
                            var omValue = parseFloat($(this).text().trim());
                            if (omValue <= 10 && omValue > 0) {
                                $(this).css({
                                    'color': '#dc3545',
                                    'font-weight': 'bold'
                                });
                            }
                        });
                    });

                    // Make the detailed table horizontally scrollable on mobile
                    if ($(window).width() < 768) {
                        $('.subject-detail-table').wrap('<div style="overflow-x: auto; white-space: nowrap;"></div>');
                    }
                }

                // Enhanced function for dynamic unsuccessful students table
                function enhanceDynamicUnsuccessfulTable() {
                    // Handle the new dynamic table structure
                    $('.dynamic-unsuccessful-table tr').each(function () {
                        // Add hover effects
                        $(this).hover(
                            function () {
                                $(this).css('background-color', '#f0f8ff');
                            },
                            function () {
                                $(this).css('background-color', '');
                            }
                        );

                        // Enhance OM and Lack cells
                        $(this).find('.om-cell').each(function () {
                            var omValue = $(this).text().trim();
                            if (omValue !== '') {
                                $(this).attr('title', 'Obtained Marks: ' + omValue);

                                // Highlight very low marks
                                var marks = parseFloat(omValue);
                                if (!isNaN(marks) && marks <= 10) {
                                    $(this).css({
                                        'color': '#dc3545',
                                        'font-weight': 'bold'
                                    });
                                }
                            }
                        });

                        $(this).find('.lack-cell').each(function () {
                            var lackValue = $(this).text().trim();
                            if (lackValue !== '') {
                                $(this).attr('title', 'Shortage from pass marks (33): ' + lackValue);

                                // Highlight high shortage values
                                var lack = parseFloat(lackValue);
                                if (!isNaN(lack)) {
                                    if (lack >= 20) {
                                        $(this).css({
                                            'background-color': '#dc3545 !important',
                                            'color': 'white !important',
                                            'font-weight': 'bold'
                                        });
                                    } else if (lack >= 10) {
                                        $(this).css({
                                            'background-color': '#ffc107 !important',
                                            'color': '#212529 !important'
                                        });
                                    }
                                }
                            }
                        });
                    });

                    // Add subject column hover effects
                    $('.dynamic-unsuccessful-table .subject-header').hover(
                        function () {
                            var index = $(this).index();
                            // Highlight corresponding OM and Lack columns
                            $('.dynamic-unsuccessful-table tr').each(function () {
                                $(this).find('td:eq(' + (index) + '), td:eq(' + (index + 1) + ')').css('background-color', '#e3f2fd');
                            });
                        },
                        function () {
                            $('.dynamic-unsuccessful-table td').css('background-color', '');
                        }
                    );
                }

                // Apply enhancements after page load
                setTimeout(() => {
                    highlightZeroValues();
                    enhanceDynamicUnsuccessfulTable(); // New function name
                }, 500);

                // Re-apply enhancements after postbacks
                $(document).ajaxComplete(function () {
                    setTimeout(() => {
                        highlightZeroValues();
                        enhanceDynamicUnsuccessfulTable(); // New function name
                    }, 200);
                });

                // Enhanced print functionality - show all tabs when printing
                $('button[onclick*="print"]').off('click').on('click', function (e) {
                    e.preventDefault();

                    // Store current active tab
                    var activeTab = $('.tab-pane.active').attr('id');

                    // Show all tabs for printing
                    $('.tab-pane').addClass('show active');

                    // Add a small delay to ensure content is rendered
                    setTimeout(() => {
                        window.print();

                        // Restore original tab state after printing
                        setTimeout(() => {
                            $('.tab-pane').removeClass('show active');
                            $('#' + activeTab).addClass('show active');
                        }, 100);
                    }, 200);
                });

                // Enhanced tab navigation with animation
                $('.nav-tabs .nav-link').on('click', function (e) {
                    e.preventDefault();

                    var targetTab = $(this).attr('href');

                    // Remove active classes from all tabs and content
                    $('.nav-tabs .nav-link').removeClass('active');
                    $('.tab-pane').removeClass('show active');

                    // Add active class to clicked tab
                    $(this).addClass('active');

                    // Show target content with animation
                    setTimeout(() => {
                        $(targetTab).addClass('show active');
                        // Reapply enhancements
                        highlightZeroValues();
                        enhanceDynamicUnsuccessfulTable(); // New function name
                    }, 50);
                });
            }
        });

        // Update labels function with improved error handling
        function updateLabels() {
            try {
                var className = "";
                if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                    className = "Class: " + '[id*=ClassDropDownList] :selected'.text();
                }

                var examName = "";
                if ($('[id*=ExamDropDownList] :selected').index() > 0) {
                    examName = ", Exam: " + '[id*=ExamDropDownList] :selected'.text();
                }

                // Update the label if it exists
                var classExamLabel = document.getElementById('<%= ClassExamLabel != null ? ClassExamLabel.ClientID : "" %>');
                if (classExamLabel) {
                    classExamLabel.innerText = className + examName;
                }
            } catch (error) {
                console.log('Label update error:', error);
            }
        }

        // Call updateLabels when page loads and on postbacks
        $(document).ready(function () {
            updateLabels();

            // Re-initialize after any postback
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm) {
                prm.add_endRequest(function () {
                    updateLabels();
                });
            }
        });

        // Improved loading states
        function showLoadingState() {
            $('.tab-content').addClass('loading');
            $('.report-table').css('opacity', '0.5');
        }

        function hideLoadingState() {
            $('.tab-content').removeClass('loading');
            $('.report-table').css('opacity', '1');
        }

        // Add loading CSS if not already present
        if (!$('#loadingCSS').length) {
            $('head').append(`
                <style id="loadingCSS">
                .loading {
                    position: relative;
                    pointer-events: none;
                }
                .loading::after {
                    content: '';
                    position: absolute;
                    top: 0;
                    left: 0;
                    right: 0;
                    bottom: 0;
                    background: rgba(255,255,255,0.8);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    z-index: 1000;
                }
                </style>
            `);
        }
    </script>
</asp:Content>
