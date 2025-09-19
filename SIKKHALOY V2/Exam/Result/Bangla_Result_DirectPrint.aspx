<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Bangla_Result_DirectPrint.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Bangla_Result_DirectPrint" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <style>
        @media print {
            .NoPrint { display: none !important; }
            body { 
                font-family: 'Kalpurush', Arial, sans-serif; 
                margin: 0; 
                padding: 0; 
                background: white !important;
            }
            .result-card { 
                margin: 0 !important; 
                padding: 5mm !important;
                page-break-after: auto !important;
                box-shadow: none !important;
                border: 2px solid #000 !important;
            }
            #header { display: none !important; }
            #sidedrawer { display: none !important; }
            #footer { display: none !important; }
            #content-wrapper { 
                margin: 0 !important; 
                padding: 0 !important; 
            }
        }
       
        body {
            font-family: 'Kalpurush', Arial, sans-serif;
            background: #fff;
            margin: 0;
            padding: 20px;
        }
        
        .result-card {
            border: 2px solid #0072bc;
            border-radius: 5px;
            padding: 15px; /* Reduced padding */
            max-width: 1200px; /* Increased width for landscape */
            margin: auto;
            page-break-after: always;
            page-break-inside: avoid;
        }
        
        .result-card:last-child {
            page-break-after: auto;
        }
        
        .header {
            text-align: center;
            border-bottom: 2px solid #0072bc;
            padding-bottom: 8px; /* Reduced padding */
            margin-bottom: 8px; /* Reduced margin */
            position: relative;
        }
        
        .header img {
            width: 50px; /* Slightly smaller */
            height: 50px;
            border-radius: 50%;
            object-fit: cover;
            position: absolute;
            left: 0;
            top: 0;
        }
        
        .header h2 {
            margin: 0;
            color: #0072bc;
            font-size: 20px; /* Slightly smaller */
        }
        
        .header p {
            margin: 1px 0; /* Reduced margin */
            font-size: 12px; /* Slightly smaller */
        }
        
        .title {
            text-align: center;
            font-weight: bold;
            margin: 10px 0; /* Reduced margin */
            color: green;
            font-size: 16px; /* Slightly smaller */
        }
        
        .marks-heading {
            text-align: center;
            font-weight: bold;
            margin: 20px 0 10px 0;
            color: #0072bc;
            font-size: 16px;
        }
        
        .top-section {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 10px; /* Reduced margin */
        }
        
        .info-summary {
            width: 72%;
        }
        
        .info-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 6px; /* Reduced margin */
        }
        
        .info-table td {
            border: 1px solid #ccc;
            padding: 4px 8px; /* Reduced padding */
            font-size: 13px; /* Slightly smaller */
        }
        
        .summary {
            width: 100%;
            border-collapse: collapse;
            margin-top: 4px; /* Reduced margin */
        }
        
        .summary td {
            padding: 6px; /* Reduced padding */
            font-size: 13px; /* Slightly smaller */
            font-weight: bold;
            text-align: center;
            border: 1px solid #ddd;
        }
        
        .summary td:nth-child(1) { background: #ffd966; }
        .summary td:nth-child(2) { background: #f4b183; }
        .summary td:nth-child(3) { background: #a9d08e; }
        .summary td:nth-child(4) { background: #9dc3e6; }
        .summary td:nth-child(5) { background: #c5e0b4; }
        .summary td:nth-child(6) { background: #ffe699; }
        .summary td:nth-child(7) { background: #d9d2e9; }

        .grade-chart {
            width: 26%;
        }
        
        .grade-chart table {
            border-collapse: collapse;
            width: 100%;
            font-size: 9px; /* Reduced font size */
        }
        
        .grade-chart th, .grade-chart td {
            border: 1px solid #333;
            padding: 1px 2px; /* Reduced padding */
            text-align: center;
        }
        
        .grade-chart th {
            background: #f0f0f0;
            font-weight: bold;
            font-size: 9px; /* Even smaller for header */
        }
        
        .marks-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 8px; /* Reduced margin */
        }
        
        .marks-table th, .marks-table td {
            border: 1px solid #000;
            text-align: center;
            padding: 4px; /* Reduced padding */
            font-size: 12px; /* Smaller font */
        }
        
        .marks-table th {
            background: #f2f2f2;
        }
        
        .marks-table .failed-row {
            background: #ffebee !important;
        }
        
        .marks-table .failed-row td {
            color: #d32f2f !important;
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
            margin-top: 30px; /* Reduced margin */
            font-size: 12px; /* Slightly smaller */
        }

        /* Print color adjustments */
        @media print {
            /* Landscape orientation optimization */
            @page {
                size: A4 landscape;
                margin: 10mm;
            }
            
            .result-card {
                max-width: 100% !important;
                width: 100% !important;
                padding: 8mm !important;
                margin: 0 !important;
                border: 1px solid #000 !important;
                transform: scale(0.95); /* Slightly scale down */
                transform-origin: top left;
            }
            
            .header {
                padding-bottom: 5px !important;
                margin-bottom: 5px !important;
            }
            
            .header h2 {
                font-size: 18px !important;
            }
            
            .header p {
                font-size: 11px !important;
            }
            
            .title {
                margin: 8px 0 !important;
                font-size: 14px !important;
            }
            
            .top-section {
                margin-bottom: 8px !important;
            }
            
            .info-table td {
                padding: 3px 6px !important;
                font-size: 12px !important;
            }
            
            .summary td {
                padding: 4px !important;
                font-size: 11px !important;
            }
            
            .grade-chart table {
                font-size: 9px !important;
            }
            
            .grade-chart th, .grade-chart td {
                padding: 1px 3px !important;
            }
            
            .marks-table th, .marks-table td {
                padding: 3px !important;
                font-size: 11px !important;
            }
            
            .footer {
                margin-top: 20px !important;
                font-size: 11px !important;
            }
            
            .summary td:nth-child(1) { 
                background: #ffd966 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary td:nth-child(2) { 
                background: #f4b183 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary td:nth-child(3) { 
                background: #a9d08e !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary td:nth-child(4) { 
                background: #9dc3e6 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary td:nth-child(5) { 
                background: #c5e0b4 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary td:nth-child(6) { 
                background: #ffe699 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary td:nth-child(7) { 
                background: #d9d2e9 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint">Individual Exam Result (Board) - Final Design</h3>
    <a href="Board_ResultV2.aspx" class="NoPrint">Board Result V2</a>

    <div class="controls NoPrint">
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

        <div class="form-group">
            <label>Group</label>
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" 
                DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" 
                OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group">
            <label>Section</label>
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" 
                DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" 
                OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

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

        <div class="form-group">
            <label>Exam</label>
            <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" 
                OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged" AutoPostBack="True" 
                DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" 
                OnDataBound="ExamDropDownList_DataBound">
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

        <div class="form-group">
            <button type="button" onclick="window.print()" class="btn btn-primary">
                <i class="fa fa-print"></i> Print
            </button>
            <asp:Button ID="LoadResultsButton" runat="server" Text="Load Results" 
                CssClass="btn btn-success" OnClick="LoadResultsButton_Click" />
        </div>
    </div>

    <div class="alert alert-success NoPrint">Page Setup Must Be (Page Size: A4. Page Margin: Narrow) In Word File</div>

    <asp:Panel ID="ResultPanel" runat="server" Visible="false">
        <asp:Repeater ID="ResultRepeater" runat="server" OnItemDataBound="ResultRepeater_ItemDataBound">
            <ItemTemplate>
                <div class="result-card">
                    <!-- Header Section -->
                    <div class="header">
                        <img src="/Handeler/SchoolLogo.ashx?SLogo=<%# Eval("SchoolID") %>" alt="School Logo" onerror="this.style.display='none';" />
                        <h2><%# Eval("SchoolName") %></h2>
                        <p><%# Eval("Address") %></p>
                        <p>Phone: <%# Eval("Phone") %> | idealedu8@gmail.com</p>
                    </div>

                    <!-- Title Section -->
                    <div class="title">
                        Result Card <br> <%# Eval("ExamName") %>
                    </div>

                    <!-- Top Section with Info and Grade Chart -->
                    <div class="top-section">
                        <!-- Left: Student Info + Summary -->
                        <div class="info-summary">
                            <table class="info-table">
                                <tr>
                                    <td>ক্লাস: <%# Eval("ClassName") %></td>
                                    <td>শাখা: <%# Eval("GroupName") %></td>
                                    <td>নাম: <b><%# Eval("StudentsName") %></b></td>
                                </tr>
                                <tr>
                                    <td>রোল: <%# Eval("RollNo") %></td>
                                    <td>আইডি: <%# Eval("ID") %></td>
                                    <td>সেকশন: <%# Eval("SectionName") %></td>
                                </tr>
                            </table>

                            <table class="summary">
                                <tr>
                                    <td>মোট নাম্বার: <%# Eval("TotalExamObtainedMark_ofStudent") %>/<%# Eval("TotalMark_ofStudent") %></td>
                                    <td>%: <%# Eval("ObtainedPercentage_ofStudent", "{0:F2}") %></td>
                                    <td>গড়: <%# Eval("Average", "{0:F2}") %></td>
                                    <td>গ্রেড: <%# Eval("Student_Grade") %></td>
                                    <td>জিপিএ: <%# Eval("Student_Point", "{0:F1}") %></td>
                                    <td>ক্লাস মেধা: <%# Eval("Position_InExam_Class") %></td>
                                    <td>শাখা মেধা: <%# Eval("Position_InExam_Subsection") %></td>
                                </tr>
                            </table>
                        </div>

                        <!-- Right: Grade Chart -->
                        <div class="grade-chart">
                            <table>
                                <tr><th>Mark</th><th>Grade</th><th>Point</th></tr>
                                <asp:Repeater ID="GradingSystemRepeater" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("MaxPercentage") %>-<%# Eval("MinPercentage") %></td>
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
                        <div>শ্রেণি শিক্ষক</div>
                        <div>প্রধান শিক্ষক</div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
</asp:Content>