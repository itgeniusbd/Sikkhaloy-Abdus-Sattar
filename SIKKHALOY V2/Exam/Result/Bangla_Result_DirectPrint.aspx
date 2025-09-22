<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Bangla_Result_DirectPrint.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Bangla_Result_DirectPrint" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Use Google Fonts for better reliability -->
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">
    <style>
        /* Use reliable Google Fonts with local fallbacks */
        * {
            font-family: 'Noto Sans Bengali', 'SolaimanLipi', 'Kalpurush', Arial, sans-serif !important;
        }
        
        @media print {
            .NoPrint { display: none !important; }
            html, body { 
                font-family: Arial, sans-serif !important; /* Use Arial for better print compatibility */
                margin: 0 !important; 
                padding: 0 !important; 
                background: white !important;
                height: auto !important;
                overflow: visible !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            
            /* Remove problematic font forcing */
            * {
                font-family: Arial, sans-serif !important; /* Use Arial instead of Kalpurush for print */
                -webkit-font-smoothing: auto !important;
                -moz-osx-font-smoothing: auto !important;
                text-rendering: auto !important;
            }
            
            .result-card { 
                margin: 5mm !important; /* Normal margin for proper pagination */
                width: auto !important; /* Let it flow naturally */
                height: auto !important; /* Remove fixed height for proper pagination */
                max-width: none !important;
                padding: 5mm !important;
                page-break-after: always !important; /* Force page break after each card */
                page-break-before: auto !important;
                page-break-inside: avoid !important; /* Avoid breaking inside card */
                box-shadow: none !important;
                border: 2px solid #0072bc !important;
                box-sizing: border-box !important;
                position: relative !important; /* Remove absolute positioning */
                top: auto !important;
                left: auto !important;
                font-family: Arial, sans-serif !important;
            }
            
            .result-card:last-child { 
                page-break-after: auto !important; /* Don't force page break after last card */
            }
            
            .result-card:first-child { 
                page-break-before: avoid !important;
            }
            
            #header { display: none !important; }
            #sidedrawer { display: none !important; }
            #footer { display: none !important; }
            #content-wrapper { 
                margin: 0 !important; 
                padding: 0 !important; 
            }
            #form1 { 
                margin: 0 !important; 
                padding: 0 !important; 
            }

            .header h2 {
                font-size: 22px !important;
                font-family: Arial, sans-serif !important;
                font-weight: bold !important;
                color: #0072bc !important; /* School name blue */
                line-height: 1.2 !important; /* Reduced line height */
                margin: 5px 0 !important; /* Reduced margin */
            }
            
            .header p {
                font-size: 13px !important;
                font-family: Arial, sans-serif !important;
                font-weight: normal !important;
                color: #000 !important; /* Address and phone black */
                line-height: 1.1 !important; /* Reduced line height */
                margin: 2px 0 !important; /* Reduced margin */
            }

            /* Student photo styles for print - square with rounded corners */
            .student-photo {
                width: 60px !important;
                height: 60px !important;
                border-radius: 10px !important; /* Square with rounded corners */
                object-fit: cover !important;
                position: absolute !important;
                right: 0 !important;
                top: 0 !important;
                left: auto !important;
                border: 3px solid #0072bc !important;
                background: white !important;
                z-index: 10 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }

            .header img {
                border-radius: 5px !important; /* Square with rounded corners for logo */
                border: 2px solid #0072bc !important; /* Same border as student photo */
                background: white !important; /* Same background as student photo */
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
                width: 60px !important; /* Same as student photo */
                height: 60px !important; /* Same as student photo */
            }
        }
       
        body {
            font-family: 'Kalpurush', Arial, sans-serif;
            background: #fff;
            margin: 0;
            padding: 40px; /* Increased padding around entire page */
        }
        
        .result-card {
            border: 2px solid #0072bc;
            border-radius: 5px;
            padding: 20px; /* Increased padding inside the card */
            max-width: 1100px; /* Slightly reduced to allow more margin */
            margin: 30px auto; /* Increased margin around the card */
            page-break-after: always;
            page-break-inside: avoid;
            page-break-before: avoid; /* Prevent page break before card */
        }
        
        .result-card:last-child {
            page-break-after: auto;
        }
        
        .header {
            text-align: center;
            border-bottom: 2px solid #0072bc;
            padding-bottom: 5px;
            margin-bottom: 5px;
            position: relative;
        }
        
        .header h2 {
            color: #0072bc !important; /* School name blue */
            line-height: 1.2 !important; /* Reduced line height */
            margin: 5px 0 !important; /* Reduced margin */
        }

        .header p {
            color: #000 !important; /* Address and phone black */
            line-height: 1.1 !important; /* Reduced line height */
            margin: 2px 0 !important; /* Reduced margin */
        }
        
        .header img {
            width: 60px; /* Same as student photo */
            height: 60px; /* Same as student photo */
            border-radius: 5px !important; /* Square with rounded corners */
            object-fit: cover;
            position: absolute;
            left: 0;
            top: 0;
            border: 2px solid #0072bc !important; /* Same border as student photo */
            background: white !important; /* Same background as student photo */
        }

        /* Student photo in header - square with rounded corners */
        .student-photo {
            width: 60px !important;
            height: 60px !important;
            border-radius: 5px !important; /* Square with rounded corners */
            object-fit: cover !important;
            position: absolute !important;
            right: 0 !important;
            top: 0 !important;
            left: auto !important;
            border: 3px solid #0072bc !important;
            background: white !important;
            z-index: 10 !important;
        }
        
        .title {
            text-align: center;
            font-weight: bold;
            margin: 8px 0;
            color: darkgreen;
            font-size: 18px;
        }
          .Exam_name {            
             text-align: center;
            font-weight: bold;
            margin: 5px 0;
            color: #0072bc;
            font-size: 18px;
             }
        
        .top-section {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 5px;
        }
        
        .info-summary {
            width: 72%;
        }
        
        .info-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 6px;
        }
        
        .info-table td {
            border: 1px solid #0072bc;
            padding: 5px 8px;
            font-size: 16px;
            font-weight: bold;
        }
        
        .summary {
            width: 100%;
            border-collapse: collapse;
            margin-top: 5px;
            font-family: 'Kalpurush', Arial, sans-serif;
        }
        
        .summary-header {
            background: #0072bc;
            color: white;
            font-weight: bold;
            text-align: center;
        }
        
        .summary-header td {
            padding: 6px 6px !important;
            font-size: 20px !important;
            background-color: #f8f9fa !important;
            -webkit-print-color-adjust: exact !important;
            print-color-adjust: exact !important;
            color: #333 !important;
            font-family: Arial, sans-serif !important;
            font-weight: bold !important;
        }
        
        .summary-values {
            background: #f9f9f9;
        }
        
        .summary-values td {
            padding: 6px !important;
            font-size: 20px !important;
            font-family: Arial, sans-serif !important;
            font-weight: bold !important;
        }
        
        .grade-chart {
            width: 26%;
        }
        
        .grade-chart table {
            border-collapse: collapse;
            width: 100%;
            font-size: 12px;
        }
        
        .grade-chart th, .grade-chart td {
            border: 1px solid #0072bc;
            padding: 3px 4px;
            text-align: center;
            font-weight: bold;
            font-family: 'Kalpurush', Arial, sans-serif !important;
            font-size: 12px;
        }
        
        .grade-chart th {
            background: #f0f0f0;
            font-weight: bold;
            font-size: 13px;
        }
        
        .marks-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 8px;
        }
        
        .marks-table th, .marks-table td {
            border: 1px solid #0072bc;
            text-align: center;
            padding: 4px;
            font-size: 16px;
            font-weight: bold;
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }
        
        .marks-table th {
            background: #d7ede1;
            font-size: 18px;
        }
        
        .marks-table th[colspan="2"] {
            background: #e8f4fd;
            color: #000;
            font-weight: bold;
        }
        
        /* Sub-exam marks styling */
        .marks-table .sub-exam-marks {
            font-size: 14px;
            font-weight: normal;
            white-space: nowrap;
            color: #333;
            display: inline-block;
        }
        
        /* Total marks column styling */
        .marks-table .total-marks-cell {
            background: white;
            font-weight: bold;
            color: #0072bc;
        }
        
        /* Dynamic font sizes based on subject count - same as print view */
        .marks-table.medium-subjects th, .marks-table.medium-subjects td {
            font-size: 12px !important; /* Same as print view */
            padding: 3px !important; /* Same as print view */
        }
        
        .marks-table.medium-subjects th {
            font-size: 13px !important; /* Same as print view */
        }
        
        .marks-table.medium-subjects .sub-exam-marks {
            font-size: 11px !important;
        }
        
        .marks-table.small-subjects th, .marks-table.small-subjects td {
            font-size: 11px !important; /* Same as print view */
            padding: 2px !important; /* Same as print view */
        }
        
        .marks-table.small-subjects th {
            font-size: 12px !important; /* Same as print view */
        }
        
        .marks-table.small-subjects .sub-exam-marks {
            font-size: 9px !important;
        }
        
        .vertical-text {
            writing-mode: vertical-rl;
            transform: rotate(180deg);
            font-weight: bold;
            color: blue;
            text-align: center;
            vertical-align: middle;
        }
        
        .footer {
            display: flex;
            justify-content: space-between;
            align-items: flex-end;
            margin-top: 30px;
            font-size: 15px;
            font-weight: bold;
            font-family: 'Kalpurush', Arial, sans-serif !important;
            padding: 0 50px;
        }

        /* Signature styling */
        .SignTeacher, .SignHead {
            min-height: 40px;
            display: flex;
            align-items: flex-end;
            justify-content: center;
        }

        .SignTeacher img, .SignHead img {
            max-height: 35px;
            max-width: 80px;
            -webkit-print-color-adjust: exact;
            print-color-adjust: exact;
            display: block;
        }

        .Teacher, .Head {
            text-align: center;
            font-weight: bold;
            margin-top: 5px;
            min-width: 120px;
            font-family: Arial, sans-serif;
        }

        .summary td {
            padding: 4px;
            font-size: 20px;
            font-weight: bold;
            text-align: center;
            border: 1px solid #0072bc;
        }
        
        /* Add the same colors as print view for normal view */
        .summary td:nth-child(1) { background: #ffd966; }
        .summary td:nth-child(2) { background: #f4b183; }
        .summary td:nth-child(3) { background: #a9d08e; }
        .summary td:nth-child(4) { background: #9dc3e6; }
        .summary td:nth-child(5) { background: #c5e0b4; }
        .summary td:nth-child(6) { background: #ffe699; }
        .summary td:nth-child(7) { background: #d9d2e9; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint">Individual Exam Result (Board) - Final Design</h3>
    <a href="Board_ResultV2.aspx" class="NoPrint">Board Result V2</a>

    <div class="controls NoPrint">
        <div class="row">
            <div class="col-md-3">
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

            <%if (GroupDropDownList.Items.Count > 1)
            {%>
            <div class="col-md-3">
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
            <%}%>

            <%if (SectionDropDownList.Items.Count > 1)
            {%>
            <div class="col-md-3">
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
            <%}%>

            <%if (ShiftDropDownList.Items.Count > 1)
            {%>
            <div class="col-md-3">
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
            <%}%>

            <%if (ExamDropDownList.Items.Count > 1)
            {%>
            <div class="col-md-3">
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
            <%}%>

            <div class="col-md-3">
                <div class="form-group">
                    <label>&nbsp;</label><br />
                    <button type="button" onclick="window.print()" class="btn btn-primary">
                        <i class="fa fa-print"></i> Print
                    </button>
                    <asp:Button ID="LoadResultsButton" runat="server" Text="Load Results" 
                        CssClass="btn btn-success" OnClick="LoadResultsButton_Click" />
                </div>
            </div>
        </div>
    </div>

    <div class="alert alert-success NoPrint">Page Setup Must Be (Page Size: A4. Page Margin: Narrow) In Word File</div>

    <!-- Teacher and Head Teacher Signature Controls -->
    <div class="form-inline NoPrint Card-space" style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px;">
        <div class="form-group" style="margin-right: 15px;">
            <asp:TextBox ID="TeacherSignTextBox" Text="শ্রেনি শিক্ষক" runat="server" placeholder="শ্রেণি শিক্ষকের স্বাক্ষর" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <label class="btn btn-grey btn-file" style="margin-left: 5px;">
                Browse
                <input id="Tfileupload" type="file" accept="image/*" style="display: none;" />
            </label>
        </div>
        <div class="form-group" style="margin-right: 15px;">
            <asp:TextBox ID="HeadTeacherSignTextBox" Text="প্রধান শিক্ষক" runat="server" placeholder="মুখ্য শিক্ষকের স্বাক্ষর" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
            <label class="btn btn-grey btn-file" style="margin-left: 5px;">
                Browse
                <input id="Hfileupload" type="file" accept="image/*" style="display: none;" />
            </label>
        </div>
    </div>

    <!-- Hidden fields to store database signature values -->
    <asp:HiddenField ID="HiddenTeacherSign" runat="server" />
    <asp:HiddenField ID="HiddenPrincipalSign" runat="server" />

    <asp:Panel ID="ResultPanel" runat="server" Visible="false">
        <asp:Repeater ID="ResultRepeater" runat="server" OnItemDataBound="ResultRepeater_ItemDataBound">
            <ItemTemplate>
                <div class="result-card">
                    <!-- Header Section -->
                    <div class="header">
                        <img src="/Handeler/SchoolLogo.ashx?SLogo=<%# Eval("SchoolID") %>" alt="School Logo" onerror="this.style.display='none';" />
                        <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" alt="Student Photo" class="student-photo" onerror="this.style.display='none';" />
                        <h2><%# Eval("SchoolName") %></h2>
                        <p><%# Eval("Address") %></p>
                        <p>Phone: <%# Eval("Phone") %> </p>
                    </div>

                    <!-- Title Section -->
                    <div >
                       <p class="Exam_name">Result Card</p>
                        <p class="title"><%# Eval("ExamName") %></p>
                    </div>

                    <!-- Top Section with Info and Grade Chart -->
                    <div class="top-section">
                        <!-- Left: Student Info + Summary -->
                        <div class="info-summary">
                            <table class="info-table">
                              <tr style="background:#e8f4fd" >
                           <td> নাম:</td> <td colspan="6"><b><%# Eval("StudentsName") %></b></td>
                                </tr>
                                
                                <%-- Use helper method for dynamic row generation --%>
                                <%# GetDynamicInfoRow(Container.DataItem) %>

                                <tr>
                                    <td>রোল:</td>
                                    <td><%# Eval("RollNo") %></td>
                                    <td>আইডি:</td>
                                    <td><%# Eval("ID") %></td>
                                    <td colspan="2"></td>
                                </tr>
                            </table>

                            <table class="summary">
                                <tr class="summary-header">
                                    <td>মোট নাম্বার</td>
                                    <td>%</td>
                                    <td>গড়</td>
                                    <td>গ্রেড</td>
                                    <td>জিপিএ</td>
                                    <td>ক্লাস মেধা</td>
                                    <%# GetSectionColumnHeader() %>
                                </tr>
                                <tr class="summary-values">
                                    <td><%# Eval("TotalExamObtainedMark_ofStudent") %>/<%# Eval("TotalMark_ofStudent") %></td>
                                    <td><%# Eval("ObtainedPercentage_ofStudent", "{0:F2}") %></td>
                                    <td><%# Eval("Average", "{0:F2}") %></td>
                                    <td><%# Eval("Student_Grade") %></td>
                                    <td><%# Eval("Student_Point", "{0:F1}") %></td>
                                    <td><%# Eval("Position_InExam_Class") %></td>
                                    <%# GetSectionColumnData(Container.DataItem) %>
                                </tr>
                            </table>
                        </div>

                        <!-- Right: Grade Chart -->
                        <div class="grade-chart">
                            <table>
                                <tr><th>মার্ক</th><th>গ্রেড</th><th>পয়েন্ট</th></tr>
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
                    <%# GenerateSubjectMarksTable(Eval("StudentResultID").ToString(), Eval("Student_Grade").ToString(), Convert.ToDecimal(Eval("Student_Point"))) %>

                    <!-- Footer -->
                    <div class="footer">
                        <div style="text-align: center;">
                            <div class="SignTeacher" style="height: 40px; margin-bottom: 5px;"></div>
                            <div class="Teacher" style="border-top: 1px solid #333; padding-top: 5px; font-weight: bold;">শ্রেণি শিক্ষক</div>
                        </div>
                        <div style="text-align: center;">
                            <div class="SignHead" style="height: 40px; margin-bottom: 5px;"></div>
                            <div class="Head" style="border-top: 1px solid #333; padding-top: 5px; font-weight: bold;">প্রধান শিক্ষক</div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>

    <script>
        $(document).ready(function() {
            // Load database signatures when page loads
            loadDatabaseSignatures();
            
            // Initialize teacher and head teacher text
            updateSignatureTexts();

            function updateSignatureTexts() {
                var teacherText = $("[id*=TeacherSignTextBox]").val() || "শ্রেণি শিক্ষক";
                var headText = $("[id*=HeadTeacherSignTextBox]").val() || "প্রধান শিক্ষক";
                
                $(".Teacher").text(teacherText);
                $(".Head").text(headText);
            }

            function loadDatabaseSignatures() {
                // Get signature values from hidden fields
                var teacherSignPath = $("[id*=HiddenTeacherSign]").val();
                var principalSignPath = $("[id*=HiddenPrincipalSign]").val();

                // Load teacher signature if exists
                if (teacherSignPath && teacherSignPath.trim() !== '') {
                    loadSignatureImage(teacherSignPath, 'teacher');
                }

                // Load principal signature if exists
                if (principalSignPath && principalSignPath.trim() !== '') {
                    loadSignatureImage(principalSignPath, 'principal');
                }
            }

            function loadSignatureImage(imagePath, signatureType) {
                var targetElement = signatureType === 'teacher' ? '.SignTeacher' : '.SignHead';
                
                var img = new Image();
                img.onload = function() {
                    var $img = $("<img />");
                    $img.attr("style", "height:35px;width:80px;object-fit:contain;");
                    $img.attr("src", imagePath);
                    $(targetElement).html($img);
                };
                
                img.src = imagePath;
            }
        });
    </script>
</asp:Content>