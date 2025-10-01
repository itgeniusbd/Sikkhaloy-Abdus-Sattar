<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Result_Card_English.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Result_Card_English" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Use Google Fonts for better reliability -->
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">

    <!-- Additional Font Awesome support for this page -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" crossorigin="anonymous" />

    <!-- External CSS for Bangla Result Direct Print -->
    <link href="Assets/bangla-result-directprint.css" rel="stylesheet" type="text/css" />

    <style>
        /* Attendance Table Styling - Fix positioning and appearance */
        .attendance-table {
            border-collapse: collapse;
            width: 100%;
            max-width: 200px;
            margin: 10px 0;
            font-size: 11px;
            background: #f8f9fa;
            border: 2px solid #333;
        }

        .attendance-table td {
            border: 1px solid #333;
            padding: 4px 6px;
            text-align: center;
            font-weight: bold;
            min-width: 35px;
        }

        .attendance-table .label {
            background-color: #ffb3ba;
            font-weight: bold;
            color: #333;
        }

        .attendance-table tr:not(.label) td {
            background-color: #fff;
            color: #333;
        }

        /* New inline attendance/summary table styling */
        .attendance-inline-complete {
            border-collapse: collapse;
            width: 100%;
            margin: 8px 0;
            font-size: 11px;
            font-family: Arial, sans-serif;
            table-layout: auto;
            overflow-x: auto;
        }

        .attendance-inline-complete td {
            border: 1px solid #000;
            padding: 4px 6px;
            text-align: center;
            font-weight: bold;
            white-space: nowrap;
            vertical-align: middle;
        }

        /* Responsive table wrapper for horizontal scrolling */
        .attendance-table-wrapper {
            overflow-x: auto;
            overflow-y: visible;
            width: 100%;
            margin: 8px 0;
        }

        /* Position the attendance table properly in the layout */
        .info-summary {
            display: flex;
            flex-direction: column;
            gap: 1px;
        }

        .info-table,
        .attendance-table,
        .summary {
            width: 100%;
        }

        /* Ensure icons are displayed properly on this page */
        .fa, .fas, .far, .fab, .fal, .fad {
            font-family: "Font Awesome 6 Free", "Font Awesome 5 Free", "FontAwesome" !important;
            font-weight: 900 !important;
            display: inline-block !important;
        }

        /* Fix specific icon display issues */
        .fa-language::before {
            content: "\f1ab" !important;
            font-family: "Font Awesome 6 Free", "FontAwesome" !important;
            font-weight: 900 !important;
        }

        .fa-map-marker::before {
            content: "\f3c5" !important;
            font-family: "Font Awesome 6 Free", "FontAwesome" !important;
        }

        .fa-phone::before {
            content: "\f095" !important;
            font-family: "Font Awesome 6 Free", "FontAwesome" !important;
        }

        .fa-envelope-o::before, .fa-envelope::before {
            content: "\f0e0" !important;
            font-family: "Font Awesome 6 Free", "FontAwesome" !important;
        }

        /* Ensure button icons are visible */
        #languageToggle i {
            margin-right: 5px !important;
            font-size: 14px !important;
        }

        /* Additional fallback styles */
        .fa {
            -webkit-font-smoothing: antialiased;
            -moz-osx-font-smoothing: grayscale;
        }

        /* Test icon visibility */
        .test-icons {
            display: none; /* Hidden test element */
        }

        /* Enhanced responsive table styles for unlimited sub-exams */
        .marks-table {
            width: 100%;
            border-collapse: collapse;
            table-layout: auto;
            font-size: 11px;
            min-width: 800px; /* Minimum width to maintain readability */
        }
.marks-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 0px;
}
        .marks-table th,
        .marks-table td {
            border: 1px solid #000;
            padding: 2px;
            text-align: center;
            vertical-align: middle;
            word-wrap: break-word;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .marks-table th:first-child,
        .marks-table td:first-child {
            text-align: left;
            padding-left: 4px;
            min-width: 80px;
            max-width: 120px;
            position: sticky;
            left: 0;
            background-color: #fff;
            z-index: 1;
        }

        /* Dynamic sizing based on number of columns */
        .marks-table.many-columns {
            font-size: 9px;
        }

        .marks-table.many-columns th,
        .marks-table.many-columns td {
            padding: 1px;
            min-width: 20px;
            max-width: 30px;
        }

        .marks-table.many-columns th:first-child,
        .marks-table.many-columns td:first-child {
            min-width: 60px;
            max-width: 80px;
        }

        /* Horizontal scroll container */
        .table-container {
            overflow-x: auto;
            overflow-y: visible;
            width: 100%;
            border: 1px solid #ddd;
            border-radius: 4px;
        }

        /* Position columns styling - last 4 columns always */
        .marks-table td:nth-last-child(-n+4),
        .marks-table th:nth-last-child(-n+4) {
            background-color: #e8f4fd !important;
            font-weight: bold !important;
            min-width: 35px !important;
            max-width: 50px !important;
            font-size: 10px !important;
        }
  


        /* Specific column width adjustments for position columns */
        .marks-table td:nth-last-child(4), /* PC */
        .marks-table th:nth-last-child(4) {
            min-width: 35px !important;
        }

        .marks-table td:nth-last-child(3), /* PS */
        .marks-table th:nth-last-child(3) {
            min-width: 35px !important;
        }

        .marks-table td:nth-last-child(2), /* HMC */
        .marks-table th:nth-last-child(2) {
            min-width: 40px !important;
        }

        .marks-table td:nth-last-child(1), /* HMS */
        .marks-table th:nth-last-child(1) {
            min-width: 40px !important;
        }

        /* Sub-exam specific adjustments */
        .marks-table.sub-exam-0 td:nth-last-child(-n+4),
        .marks-table.sub-exam-0 th:nth-last-child(-n+4) {
            font-size: 11px !important;
            min-width: 40px !important;
        }

        .marks-table.sub-exam-1 td:nth-last-child(-n+4),
        .marks-table.sub-exam-1 th:nth-last-child(-n+4) {
            font-size: 11px !important;
            min-width: 38px !important;
        }

        .marks-table.sub-exam-2 td:nth-last-child(-n+4),
        .marks-table.sub-exam-2 th:nth-last-child(-n+4) {
            font-size: 10px !important;
            min-width: 36px !important;
        }

        .marks-table.sub-exam-3 td:nth-last-child(-n+4),
        .marks-table.sub-exam-3 th:nth-last-child(-n+4) {
            font-size: 10px !important;
            min-width: 34px !important;
        }

        .marks-table.sub-exam-4 td:nth-last-child(-n+4),
        .marks-table.sub-exam-4 th:nth-last-child(-n+4),
        .marks-table[class*="sub-exam-"]:not(.sub-exam-0):not(.sub-exam-1):not(.sub-exam-2):not(.sub-exam-3) td:nth-last-child(-n+4),
        .marks-table[class*="sub-exam-"]:not(.sub-exam-0):not(.sub-exam-1):not(.sub-exam-2):not(.sub-exam-3) th:nth-last-child(-n+4) {
            font-size: 9px !important;
            min-width: 32px !important;
        }

        /* Header background for all table headers */
        .marks-table tr:first-child th {
            background-color: #ffb3ba !important;
            font-weight: bold;
            position: sticky;
            top: 0;
            z-index: 2;
        }

        .marks-table tr:nth-child(2) th {
            background-color: #ffb3ba !important;
            font-weight: bold;
            position: sticky;
            top: 25px; /* Adjust based on first row height */
            z-index: 2;
        }

        /* Ensure position columns in header also have proper background */
        .marks-table tr:first-child th:nth-last-child(-n+4),
        .marks-table tr:nth-child(2) th:nth-last-child(-n+4) {
            background-color: #ffb3ba !important;
        }

        /* Print styles */
        @media print {
            .marks-table {
                font-size: 8px !important;
                min-width: auto !important;
            }
            
            .marks-table th,
            .marks-table td {
                padding: 1px !important;
                min-width: 18px !important;
                max-width: 25px !important;
            }

            .marks-table th:first-child,
            .marks-table td:first-child {
                min-width: 50px !important;
                max-width: 70px !important;
            }

            .marks-table td:nth-last-child(-n+4),
            .marks-table th:nth-last-child(-n+4) {
                font-size: 7px !important;
                min-width: 25px !important;
                max-width: 30px !important;
            }

            .table-container {
                overflow: visible !important;
                border: none !important;
            }

            /* Remove sticky positioning for print */
            .marks-table th:first-child,
            .marks-table td:first-child,
            .marks-table tr:first-child th,
            .marks-table tr:nth-child(2) th {
                position: static !important;
            }
        }

        /* Mobile responsive */
        @media screen and (max-width: 768px) {
            .marks-table {
                font-size: 8px;
            }
            
            .marks-table th,
            .marks-table td {
                padding: 1px;
                min-width: 15px;
                max-width: 20px;
            }

            .marks-table th:first-child,
            .marks-table td:first-child {
                min-width: 40px;
                max-width: 60px;
            }

            .marks-table td:nth-last-child(-n+4),
            .marks-table th:nth-last-child(-n+4) {
                font-size: 7px;
                min-width: 25px;
                max-width: 30px;
            }
        }

        /* Tooltip styles for truncated content */
        .marks-table [title] {
            cursor: help;
        }

        /* Styles for failed subjects */
        .marks-table .failed-row {
            background-color: #ffe6e6;
        }

        .marks-table .failed-row td {
            color: #d32f2f;
            font-weight: bold;
        }

        /* Loading indicator for large tables */
        .table-loading {
            text-align: center;
            padding: 20px;
            font-style: italic;
            color: #666;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint" id="pageTitle">English Result Card     <a href="BanglaResult_Old.aspx"><span class="btn-text-full">Old</span> </a></h3>

    
    <div class="controls NoPrint">
        <div class="row">
            <div class="col-md-2">
                <div class="form-group">
                    <label>Class</label>
                    <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" 
                        CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" 
                        DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" 
                        AutoPostBack="True">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT CreateClass.Class,CreateClass.SN, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY CreateClass.SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <% if (GroupDropDownList.Items.Count > 1) { %>
            <div class="col-md-2">
                <div class="form-group">
                    <label>Group</label>
                    <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" 
                        DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" 
                        OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
            <% } %>

            <% if (SectionDropDownList.Items.Count > 1) { %>
            <div class="col-md-2">
                <div class="form-group">
                    <label>Section</label>
                    <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" 
                        DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" 
                        OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
            <% } %>

            <% if (ShiftDropDownList.Items.Count > 1) { %>
            <div class="col-md-2">
                <div class="form-group">
                    <label>Shift</label>
                    <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" 
                        DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" 
                        OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
            <% } %>

            <% if (ExamDropDownList.Items.Count > 1) { %>
            <div class="col-md-2">
                <div class="form-group">
                    <label>Exam</label>
                    <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" 
                        OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged" AutoPostBack="True" 
                        DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" 
                        OnDataBound="ExamDropDownList_DataBound">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                        SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Name.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) ORDER BY Exam_Name.ExamID">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
            <% } %>

            <div class="col-md-2">
                <div class="form-group">
                    <label>Student ID</label>
                    <asp:TextBox ID="StudentIDTextBox" runat="server" CssClass="form-control" 
                        placeholder="101,105,107" 
                        ToolTip="Enter Student IDs separated by commas"></asp:TextBox>
                </div>
            </div>

            <div class="col-md-2">
                <div class="form-group">
                    <label>&nbsp;</label>
                    <div class="button-container" style="display: flex; gap: 5px; margin-top: 5px;">
                        <asp:Button ID="LoadResultsButton" runat="server" Text="LOAD" 
                            CssClass="btn btn-success btn-sm" OnClick="LoadResultsButton_Click" 
                            style="flex: 1; height: 34px;" />
                        <button type="button" onclick="window.print()" class="btn btn-primary btn-sm" 
                            id="PrintButton" style="display:none; flex: 1; height: 34px;">
                            PRINT
                        </button>

                    </div>
                </div>
            </div>
        </div>
    </div>

   

    <!-- Teacher and Head Teacher Signature Controls with Pagination -->
    <div class="form-inline NoPrint Card-space" style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px; display: flex; align-items: center; justify-content: space-between;">
        <div style="display: flex; align-items: center;">
            <div class="form-group NoPrint" style="margin-right: 15px;">
                <asp:TextBox ID="TeacherSignTextBox" Text="Class Teacher" runat="server" placeholder="Class Teacher Signature" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                <label class="btn btn-secondary btn-sm NoPrint" for="Tfileupload" style="margin-left: 5px; margin-top: 5px; cursor: pointer;">
                    Browse
                </label>
                <input id="Tfileupload" type="file" accept="image/*" style="position: absolute; left: -9999px; opacity: 0;" />
            </div>
            <div class="form-group NoPrint" style="margin-right: 15px;">
                <asp:TextBox ID="HeadTeacherSignTextBox" Text="Principal" runat="server" placeholder="Principal Signature" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                <label class="btn btn-secondary btn-sm" for="Hfileupload" style="margin-left: 5px; margin-top: 5px; cursor: pointer;">
                    Browse
                </label>
                <input id="Hfileupload" type="file" accept="image/*" style="position: absolute; left: -9999px; opacity: 0;" />
            </div>
        </div>
                <div class="pagination-inline NoPrint" style="margin-bottom: 15px; text-align: center;">
            <asp:Label ID="PaginationInfoLabel" runat="server" CssClass="pagination-label" 
                Text="Loaded 0 to 0 students. Total 0 students"></asp:Label>
        </div>
        <!-- Pagination Controls -->
        <div class="pagination-inline NoPrint" style="display: flex; align-items: center;">
            <asp:Button ID="FirstPageButton" runat="server" Text="First" CssClass="btn btn-xs btn-outline-primary" 
                OnClick="FirstPageButton_Click" style="margin: 0 2px; padding: 2px 6px; font-size: 11px;" />
            <asp:Button ID="PrevPageButton" runat="server" Text="Prev" CssClass="btn btn-xs btn-outline-primary" 
                OnClick="PrevPageButton_Click" style="margin: 0 2px; padding: 2px 6px; font-size: 11px;" />
            <asp:Label ID="PageInfoLabel" runat="server" CssClass="page-info-inline" 
                Text="Page 1 of 1" style="margin: 0 8px; font-size: 12px; font-weight: bold; color: #495057;"></asp:Label>
            <asp:Button ID="NextPageButton" runat="server" Text="Next" CssClass="btn btn-xs btn-outline-primary" 
                OnClick="NextPageButton_Click" style="margin: 0 2px; padding: 2px 6px; font-size: 11px;" />
            <asp:Button ID="LastPageButton" runat="server" Text="Last" CssClass="btn btn-xs btn-outline-primary" 
                OnClick="LastPageButton_Click" style="margin: 0 2px; padding: 2px 6px; font-size: 11px;" />
        </div>
    </div>

    <!-- Hidden fields to store database signature values -->
    <asp:HiddenField ID="HiddenTeacherSign" runat="server" />
    <asp:HiddenField ID="HiddenPrincipalSign" runat="server" />

    <asp:Panel ID="ResultPanel" runat="server" Visible="false">
        <!-- Pagination Info -->


        <!-- Results -->
        <asp:Repeater ID="ResultRepeater" runat="server" OnItemDataBound="ResultRepeater_ItemDataBound">
            <ItemTemplate>
                <div class="result-card">
                    <!-- Header Section -->
                    <div class="header">
                        <img src="/Handeler/SchoolLogo.ashx?SLogo=<%# Eval("SchoolID") %>" alt="School Logo" onerror="this.style.display='none';" />
                        <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" alt="Student Photo" class="student-photo" onerror="this.style.display='none';" />
                        <h2><%# Eval("SchoolName") %></h2>
                        <p><i class="fa fa-map-marker icon-fallback" data-fallback="📍"></i> <%# Eval("Address") %></p>
                        <p><i class="fa fa-phone icon-fallback" data-fallback="📞"></i> <%# Eval("Phone") %></p>
                    </div>

                    <!-- Title Section -->
                    <div >
                       <p class="Exam_name">Result Card</p>
                        <p class="title"><%# Eval("ExamName") %></p>
                    </div>

                    <!-- Top Section with Info and Grade Chart -->
                    <div class="top-section">
                        <!-- Left: Student Info + Attendance/Summary (Combined) -->
                        <div class="info-summary">
                            <table class="info-table">
                              <tr style="background:#e8f4fd" >
                           <td> Name:</td> <td colspan="6"><b><%# Eval("StudentsName") %></b></td>
                                </tr>
                                
                                <%-- Use helper method for dynamic row generation --%>
                                <%# GetDynamicInfoRow(Container.DataItem) %> 

                                <tr>
                                    <td>Roll:</td>
                                    <td><%# Eval("RollNo") %></td>
                                    <td>ID:</td>
                                    <td><%# Eval("ID") %></td>
                                    <td colspan="2"></td>
                                </tr>
                            </table>
                            
                            <!-- Combined Attendance and Summary Table in Single Row -->
                            <%# GetAttendanceTableHtml(Container.DataItem) %>
                        </div>

                        <!-- Right: Grade Chart -->
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

                    <!-- Dynamic Subject Marks Table -->
                    <%# GenerateSubjectMarksTable(Eval("StudentResultID").ToString(), Eval("Student_Grade").ToString(), Eval("Student_Point") == DBNull.Value ? 0m : Convert.ToDecimal(Eval("Student_Point"))) %>

                    <!-- Footer -->
                    <div class="footer">
                        <div style="text-align: center;">
                            <div class="SignTeacher" style="height: 40px; margin-bottom: 5px;"></div>
                            <div class="Teacher" style="border-top: 1px solid #333; padding-top: 5px; font-weight: bold;">Class Teacher</div>
                        </div>
                        <div style="text-align: center;">
                            <div class="SignHead" style="height: 40px; margin-bottom: 5px;"></div>
                            <div class="Head" style="border-top: 1px solid #333; padding-top: 5px; font-weight: bold;">Principal</div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>

    <script type="text/javascript">
        // Missing checkAndFixFontAwesome function - Add this first
        function checkAndFixFontAwesome() {
            console.log('Checking Font Awesome icons...');
            
            // Test if Font Awesome is loaded
            var testIcon = $('<i class="fa fa-home"></i>').appendTo('body');
            var iconWidth = testIcon.width();
            testIcon.remove();
            
            if (iconWidth > 0) {
                console.log('Font Awesome loaded successfully');
                fixResultCardIcons();
            } else {
                console.warn('Font Awesome not loaded properly, using fallback');
                // Add fallback Font Awesome if not loaded
                if (!$('link[href*="font-awesome"]').length) {
                    $('<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">').appendTo('head');
                    setTimeout(fixResultCardIcons, 500);
                }
            }
            
            console.log('All Font Awesome icons loaded successfully');
        }

        // Function to fix result card icons
        function fixResultCardIcons() {
            $('.result-card').each(function() {
                var $card = $(this);
                
                // Fix map marker icon
                $card.find('.fa-map-marker, .fa-map-marker-alt').each(function() {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '📍');
                    }
                });
                
                // Fix phone icon
                $card.find('.fa-phone').each(function() {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '📞');
                    }
                });
                
                // Fix envelope icon
                $card.find('.fa-envelope, .fa-envelope-o').each(function() {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '✉️');
                    }
                });
            });
        }

        // Fix absent marks display function
        function fixAbsentMarksDisplay() {
            $('.marks-table').each(function() {
                var $table = $(this);
                
                // Process each data row
                $table.find('tr').each(function() {
                    var $row = $(this);
                    
                    // Skip header rows
                    if ($row.find('th').length > 0) return;
                    
                    // Check each cell for absent marks
                    $row.find('td').each(function() {
                        var $cell = $(this);
                        var cellText = $cell.text().trim();
                        
                        // Convert 'A' to 'Abs' for absent marks (but not in grade columns)
                        if (cellText === 'A' && !$cell.hasClass('grade-cell')) {
                            $cell.text('Abs').addClass('absent-mark');
                        }
                    });
                });
            });
        }

        // Function to load database signatures
        function loadDatabaseSignatures() {
            var teacherSignPath = $('[id$="HiddenTeacherSign"]').val();
            var principalSignPath = $('[id$="HiddenPrincipalSign"]').val();
            
            console.log('Loading database signatures:', {
                teacher: teacherSignPath,
                principal: principalSignPath
            });
            
            if (teacherSignPath && teacherSignPath.trim() !== '') {
                $('.SignTeacher').html('<img src="' + teacherSignPath + '" style="max-height: 35px; max-width: 120px;">');
            }
            
            if (principalSignPath && principalSignPath.trim() !== '') {
                $('.SignHead').html('<img src="' + principalSignPath + '" style="max-height: 35px; max-width: 120px;">');
            }
        }

        // Function to update signature texts
        function updateSignatureTexts() {
            var teacherText = $('[id$="TeacherSignTextBox"]').val() || 'Class Teacher';
            var principalText = $('[id$="HeadTeacherSignTextBox"]').val() || 'Principal';
            
            $('.Teacher').text(teacherText);
            $('.Head').text(principalText);
        }

        // Function to initialize signature upload functionality
        function initializeSignatureUpload() {
            console.log('Initializing signature upload functionality...');
            
            // Teacher signature upload
            $('#Tfileupload').off('change').on('change', function(e) {
                console.log('Teacher file input changed');
                handleSignatureUpload(e, 'teacher', '.SignTeacher');
            });
            
            // Principal signature upload
            $('#Hfileupload').off('change').on('change', function(e) {
                console.log('Principal file input changed');
                handleSignatureUpload(e, 'principal', '.SignHead');
            });
            
            // Update signature texts when textboxes change
            $('[id$="TeacherSignTextBox"]').off('input').on('input', function() {
                var text = $(this).val() || 'Class Teacher';
                $('.Teacher').text(text);
            });
            
            $('[id$="HeadTeacherSignTextBox"]').off('input').on('input', function() {
                var text = $(this).val() || 'Principal';
                $('.Head').text(text);
            });
        }

        // Function to handle signature upload
        function handleSignatureUpload(event, signatureType, targetSelector) {
            var file = event.target.files[0];
            if (!file) return;
            
            // Validate file type
            if (!file.type.match('image.*')) {
                alert('Please select an image file.');
                return;
            }
            
            // Validate file size (max 2MB)
            if (file.size > 2 * 1024 * 1024) {
                alert('File size should be less than 2MB.');
                return;
            }
            
            var reader = new FileReader();
            reader.onload = function(e) {
                var imageData = e.target.result;
                
                // Display the image immediately
                $(targetSelector).html('<img src="' + imageData + '" style="max-height: 35px; max-width: 120px;">');
                
                // Save to database
                var base64Data = imageData.split(',')[1]; // Remove data:image/...;base64, prefix
                
                $.ajax({
                    type: 'POST',
                    url: 'Result_Card_English.aspx/SaveSignature',
                    data: JSON.stringify({
                        signatureType: signatureType,
                        imageData: base64Data
                    }),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function(response) {
                        console.log('Signature saved successfully:', response);
                    },
                    error: function(xhr, status, error) {
                        console.error('Error saving signature:', error);
                        alert('Error saving signature. Please try again.');
                    }
                });
            };
            
            reader.readAsDataURL(file);
        }

        // Function to apply pagination styles
        function applyPaginationStyles() {
            $('.pagination-inline .btn').each(function() {
                var $btn = $(this);
                if ($btn.hasClass('aspNetDisabled') || $btn.prop('disabled')) {
                    $btn.removeClass('btn-outline-primary').addClass('btn-outline-secondary');
                } else {
                    $btn.removeClass('btn-outline-secondary').addClass('btn-outline-primary');
                }
            });
        }

        // Convert Bengali numbers to English
        function convertBengaliToEnglishJS(text) {
            var bengaliToEnglish = {
                '০': '0', '১': '1', '২': '2', '৩': '3', '৪': '4',
                '৫': '5', '৬': '6', '৭': '7', '৮': '8', '৯': '9'
            };
            
            return text.replace(/[০-৯]/g, function(match) {
                return bengaliToEnglish[match] || match;
            });
        }

        $(document).ready(function () {
            // Check if Font Awesome is loaded properly
            checkAndFixFontAwesome();
            
            // DON'T convert numbers to Bengali automatically - keep English by default
            // convertNumbersToBengali(); // Commented out - numbers will stay in English by default

            // Fix absent marks display
            fixAbsentMarksDisplay();

            // Load database signatures when page loads
            loadDatabaseSignatures();

            // Initialize teacher and head teacher text
            updateSignatureTexts();

            // Initialize signature upload functionality - only once
            console.log('About to initialize signature upload...');
            initializeSignatureUpload();

            // Apply pagination button styles
            applyPaginationStyles();

            // Show toggle button if results are already loaded
            if ($('.result-card').length > 0) {
                $('#NumberToggleButton').show();
                $('#PrintButton').show();
                // Set initial button state to show "Bengali Numbers" since numbers are in English by default
                $('#NumberToggleButton').html('Bengali Numbers').removeClass('btn-info').addClass('btn-warning');
                isNumbersBengali = false; // Set to false since numbers are in English by default
                
                // Fix result card icons on page load if results exist
                fixResultCardIcons();
                
                // Fix position columns alignment
                fixPositionColumnsAlignment();
            }

            // Test browse button functionality - only for debugging
            console.log('Testing browse button elements:');
            console.log('Teacher file input:', $('#Tfileupload').length);
            console.log('Principal file input:', $('#Hfileupload').length);
            console.log('Teacher browse label:', $('label[for="Tfileupload"]').length);
            console.log('Principal browse label:', $('label[for="Hfileupload"]').length);

            // Handle Enter key press in Student ID textbox
            $("[id*=StudentIDTextBox]").keypress(function (e) {
                if (e.which == 13) { // Enter key
                    e.preventDefault();
                    $("[id*=LoadResultsButton]").click();
                }
            });

            // Clear Student ID textbox when Class dropdown changes
            $("[id*=ClassDropDownList]").change(function () {
                $("[id*=StudentIDTextBox]").val('');
            });

            // Show toggle button after LOAD button is clicked and results are loaded
            $("[id*=LoadResultsButton]").click(function () {
                setTimeout(function () {
                    if ($('.result-card').length > 0) {
                        $('#NumberToggleButton').show();
                        $('#PrintButton').show();
                        // Set initial button state to show "Bengali Numbers" since numbers are in English by default
                        $('#NumberToggleButton').html('Bengali Numbers').removeClass('btn-info').addClass('btn-warning');
                        isNumbersBengali = false; // Numbers are in English by default
                        
                        // Fix result card icons after loading
                        fixResultCardIcons();
                        
                        // Fix position columns alignment after loading results
                        fixPositionColumnsAlignment();
                    }
                }, 1000);
            });

            // Add input validation for Student ID textbox - allow alphanumeric
            $("[id*=StudentIDTextBox]").on('input', function () {
                var value = $(this).val();
                // Allow alphanumeric characters, commas, and spaces
                var validChars = /^[a-zA-Z0-9,\s]*$/;

                if (!validChars.test(value)) {
                    // Remove invalid characters
                    value = value.replace(/[^a-zA-Z0-9,\s]/g, '');
                    $(this).val(value);
                }
            });

            // Add helpful tooltips and validation feedback
            $("[id*=StudentIDTextBox]").on('blur', function () {
                var value = $(this).val().trim();
                if (value) {
                    // Convert Bengali to English for validation
                    var englishValue = convertBengaliToEnglishJS(value);
                    var ids = englishValue.split(/[,]/).map(function(id) { return id.trim(); }).filter(function(id) { return id; });

                    // More flexible validation for alphanumeric IDs
                    var invalidIds = ids.filter(function(id) { return !/^[a-zA-Z0-9]+$/.test(id) || id.length === 0; });
                    if (invalidIds.length > 0) {
                        $(this).addClass('is-invalid');
                        $(this).attr('title', 'Invalid IDs: ' + invalidIds.join(', '));
                    } else {
                        $(this).removeClass('is-invalid');
                        $(this).attr('title', 'Valid IDs: ' + ids.length + ' student(s)');
                    }
                }
            });
        });

        // Function to fix position columns alignment based on actual sub-exam count
        function fixPositionColumnsAlignment() {
            console.log('Fixing position columns alignment...');
            
            $('.marks-table').each(function() {
                var $table = $(this);
                var $firstRow = $table.find('tr').first();
                
                // Count actual columns by examining the first data row (not header)
                var $dataRow = $table.find('tr').filter(function() {
                    return $(this).find('td').length > 0;
                }).first();
                
                if ($dataRow.length === 0) return;
                
                var totalCells = $dataRow.find('td').length;
                console.log('Table has', totalCells, 'total columns');
                
                // Position columns are always the last 4 columns: PC, PS, HMC, HMS
                var pcIndex = totalCells - 4;  // Position Class
                var psIndex = totalCells - 3;  // Position Section  
                var hmcIndex = totalCells - 2; // Highest Marks Class
                var hmsIndex = totalCells - 1; // Highest Marks Section
                
                console.log('Position column indices:', {pc: pcIndex, ps: psIndex, hmc: hmcIndex, hms: hmsIndex});
                
                // Apply classes to header cells
                $table.find('tr').each(function() {
                    var $row = $(this);
                    var $cells = $row.find('th, td');
                    
                    if ($cells.length >= totalCells) {
                        // Remove existing position classes
                        $cells.removeClass('position-col-pc position-col-ps position-col-hmc position-col-hms');
                        
                        // Add correct position classes
                        if ($cells.eq(pcIndex).length > 0) $cells.eq(pcIndex).addClass('position-col-pc');
                        if ($cells.eq(psIndex).length > 0) $cells.eq(psIndex).addClass('position-col-ps');
                        if ($cells.eq(hmcIndex).length > 0) $cells.eq(hmcIndex).addClass('position-col-hmc');
                        if ($cells.eq(hmsIndex).length > 0) $cells.eq(hmsIndex).addClass('position-col-hms');
                    }
                });
                
                // Apply dynamic styling for position columns
                var columnWidth = Math.max(28, Math.min(40, Math.floor(100/totalCells)));
                var rightOffset = columnWidth;
                
                $table.find('.position-col-hms').css({
                    'right': '0px',
                    'min-width': columnWidth + 'px',
                    'background-color': '#f8f9fa',
                    'font-weight': 'bold'
                });
                
                $table.find('.position-col-hmc').css({
                    'right': rightOffset + 'px',
                    'min-width': columnWidth + 'px',
                    'background-color': '#f8f9fa',
                    'font-weight': 'bold'
                });
                
                rightOffset += columnWidth;
                $table.find('.position-col-ps').css({
                    'right': rightOffset + 'px',
                    'min-width': columnWidth + 'px',
                    'background-color': '#f8f9fa',
                    'font-weight': 'bold'
                });
                
                rightOffset += columnWidth;
                $table.find('.position-col-pc').css({
                    'right': rightOffset + 'px',
                    'min-width': columnWidth + 'px',
                    'background-color': '#f8f9fa',
                    'font-weight': 'bold'
                });
                
                console.log('Applied position column styling with width:', columnWidth);
            });
        }

        function convertNumbersAfterPostback() {
            // Fix result card icons first
            fixResultCardIcons();
            
            // Fix position columns alignment after postback
            fixPositionColumnsAlignment();
            
            // Fix absent marks first
            $('.marks-table').each(function () {
                var $table = $(this);

                // Get header cells for column identification - define this at table level
                var $headerRow = $table.find('tr').first();
                var $headerCells = $headerRow.find('th');

                // Process each row in the table
                $table.find('tr').each(function (rowIndex) {
                    var $row = $(this);

                    // Skip header rows
                    if ($row.find('th').length > 0) {
                        return;
                    }

                    // Process each cell in the row
                    $row.find('td').each(function (cellIndex) {
                        var $cell = $(this);
                        var cellText = $cell.text().trim();

                        // Get the header for this column
                        var columnHeader = '';
                        if (cellIndex < $headerCells.length) {
                            columnHeader = $headerCells.eq(cellIndex).text().trim();
                        }

                        // Only convert 'A' to 'Abs' in marks columns, NOT in grade columns
                        if (cellText === 'A') {
                            // Check if this is a grade column - if so, don't convert
                            if (columnHeader === 'Grade' || columnHeader.indexOf('Grade') !== -1) {
                                return; // Skip grade columns
                            }

                            // Convert A to Abs in marks columns only
                            if (columnHeader === 'Obtain Marks' ||
                                columnHeader.indexOf('Number') !== -1 ||
                                columnHeader === 'Midterm' ||
                                columnHeader === 'Periodical' ||
                                columnHeader === 'Subjective' ||
                                columnHeader === 'Objective' ||
                                cellIndex <= 2) { // First few columns are usually marks columns

                                $cell.text('Abs');
                            }
                        }
                        // Convert '0' to '-' in total marks column (if it's likely absent)
                        else if (cellText === '0' && $cell.hasClass('total-marks-cell')) {
                            var hasAbsentMarks = false;

                            $row.find('td').each(function () {
                                var siblingText = $(this).text().trim();
                                if (siblingText === 'Abs') {
                                    hasAbsentMarks = true;
                                    return false;
                                }
                            });

                            if (hasAbsentMarks) {
                                $cell.text('-');
                            }
                        }
                    });
                });
            });

            // DON'T convert numbers to Bengali after postback - keep them in English by default
            // convertNumbersToBengali(); // Commented out - keep numbers in English by default

            // Reset button state to show "Bengali Numbers" since numbers are in English
            if ($('#NumberToggleButton').length > 0) {
                $('#NumberToggleButton').html('Bengali Numbers').removeClass('btn-info').addClass('btn-warning');
                isNumbersBengali = false;
            }
        }

        // Convert numbers when new data is loaded via postback - using proper ASP.NET approach
        function pageLoad(sender, args) {
            if (args && args.get_isPartialLoad && args.get_isPartialLoad()) {
                setTimeout(function () {
                    convertNumbersAfterPostback();
                    // Reapply pagination styles after postback
                    applyPaginationStyles();
                    // Fix position columns alignment after partial postback
                    fixPositionColumnsAlignment();
                }, 100);
            }
        }
    </script>
</asp:Content>