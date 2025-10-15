<%@ Page Title="Analytical Smart Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Analytical_Smart_Result.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Analytical_Smart_Result" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Analytical_Smart_Result.css?v=5" rel="stylesheet" />
    <style>
        /* Enhanced Table Styles */
        .report-table {
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
            font-family: Arial, sans-serif;
            font-size: 12px;
            table-layout: fixed; /* Fix layout to prevent overflow */
        }
        
        .report-table th, 
        .report-table td {
            border: 1px solid #ddd;
            padding: 6px 4px; /* Reduced padding for better fit */
            text-align: center;
            word-wrap: break-word;
            overflow: hidden;
        }
        
        .report-table th {
            background-color: #f2f2f2;
            font-weight: bold;
            color: #333;
            font-size: 11px; /* Smaller font for headers */
        }
        
        .report-table tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        
        .report-table tr:hover {
            background-color: #f5f5f5;
        }
        
        /* Enhanced responsive table container */
        .table-responsive {
            width: 100%;
            overflow-x: auto;
            overflow-y: hidden;
            -webkit-overflow-scrolling: touch;
            border: 1px solid #ddd;
            border-radius: 4px;
            margin-bottom: 20px;
        }
        
        /* Subject Statistics Table Specific Styles */
        #tab2 .table-responsive {
            max-width: 100%;
            overflow-x: auto;
        }
        
        #tab2 .report-table {
            min-width: 800px; /* Minimum width to ensure readability */
            font-size: 11px;
        }
        
        #tab2 .report-table th:nth-child(1), /* Subject */
        #tab2 .report-table td:nth-child(1) {
            width: 15%;
            min-width: 100px;
            text-align: left !important;
            font-weight: 500;
        }
        
        #tab2 .report-table th:nth-child(2), /* Total Students */
        #tab2 .report-table td:nth-child(2) {
            width: 10%;
            min-width: 60px;
        }
        
        #tab2 .report-table th:nth-child(3), /* Passed */
        #tab2 .report-table td:nth-child(3) {
            width: 10%;
            min-width: 60px;
            background-color: #d4edda !important;
        }
        
        #tab2 .report-table th:nth-child(4), /* Failed */
        #tab2 .report-table td:nth-child(4) {
            width: 10%;
            min-width: 60px;
            background-color: #f8d7da !important;
        }
        
        #tab2 .report-table th:nth-child(5), /* Pass % */
        #tab2 .report-table td:nth-child(5) {
            width: 10%;
            min-width: 70px;
        }
        
        #tab2 .report-table th:nth-child(6), /* Highest */
        #tab2 .report-table td:nth-child(6) {
            width: 12%;
            min-width: 80px;
        }
        
        #tab2 .report-table th:nth-child(7), /* Lowest */
        #tab2 .report-table td:nth-child(7) {
            width: 12%;
            min-width: 80px;
            background-color: #fff3cd !important;
        }
        
        #tab2 .report-table th:nth-child(8), /* Average */
        #tab2 .report-table td:nth-child(8) {
            width: 12%;
            min-width: 80px;
        }
        
        .report-header {
            text-align: center;
            margin: 20px 0;
            page-break-inside: avoid;
        }
        
        .chart-container {
            text-align: center;
            margin: 20px 0;
            page-break-inside: avoid;
        }
        
        .grade-chart {
            display: inline-block;
            margin: 10px;
            padding: 15px;
            border: 2px solid #ddd;
            border-radius: 8px;
            background-color: #f8f9fa;
            min-width: 80px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        
        .grade-count {
            font-size: 28px;
            font-weight: bold;
            color: #007bff;
            margin-bottom: 5px;
        }
        
        .grade-label {
            font-size: 14px;
            font-weight: 600;
            color: #495057;
        }
        
        .form-inline .form-group {
            margin-right: 15px;
            margin-bottom: 10px;
        }
        
        .btn-primary {
            background-color: #007bff;
            border-color: #007bff;
            padding: 8px 16px;
        }
        
        /* Enhanced Tab Styling */
        .nav-tabs {
            border-bottom: 2px solid #dee2e6;
            margin-bottom: 0;
            flex-wrap: wrap; /* Allow tab wrapping on smaller screens */
        }
        
        .nav-tabs .nav-link {
            color: #495057;
            font-weight: 500;
            border: 1px solid transparent;
            border-top-left-radius: 8px;
            border-top-right-radius: 8px;
            padding: 12px 16px; /* Slightly reduced padding */
            transition: all 0.3s ease;
            font-size: 13px; /* Smaller font for better fit */
        }
        
        .nav-tabs .nav-link:hover {
            color: #007bff;
            background-color: #f8f9fa;
            border-color: #dee2e6 #dee2e6 #f8f9fa;
        }
        
        .nav-tabs .nav-link.active {
            color: #fff !important;
            background-color: #007bff !important;
            border-color: #007bff #007bff #007bff !important;
            font-weight: 600;
            box-shadow: 0 2px 4px rgba(0, 123, 255, 0.25);
        }
        
        .tab-content {
            padding: 25px;
            border: 1px solid #dee2e6;
            border-top: none;
            background-color: #fff;
            border-bottom-left-radius: 8px;
            border-bottom-right-radius: 8px;
            min-height: 400px;
        }
        
        /* Ensure all tab panes have proper spacing */
        .tab-pane {
            padding-top: 10px;
        }
        
        .tab-pane h4 {
            margin-bottom: 20px;
            color: #333;
            font-weight: 600;
        }
        
        /* Fix for empty data message */
        .no-data-message {
            text-align: center;
            padding: 40px 20px;
            color: #6c757d;
            font-style: italic;
        }
        
        /* Enhanced styling for pass/fail data */
        .text-success {
            color: #28a745 !important;
            font-weight: 600;
        }
        
        .text-danger {
            color: #dc3545 !important;
            font-weight: 600;
        }
        
        /* Highlight zero values */
        .report-table td {
            position: relative;
        }
        
        .report-table td:contains('0') {
            background-color: #fff3cd;
            color: #856404;
        }
        
        /* Special styling for unsuccessful students table */
        .unsuccessful-table .failed-subjects-cell {
            max-width: 250px;
            word-wrap: break-word;
            text-align: left !important;
            font-size: 11px;
            line-height: 1.4;
            padding: 8px 6px;
        }
        
        .unsuccessful-table .student-name-cell {
            max-width: 150px;
            text-align: left !important;
        }
        
        .unsuccessful-table .grade-f {
            background-color: #f8d7da !important;
            color: #721c24 !important;
            font-weight: bold;
        }
        
        .unsuccessful-table .failed-count {
            background-color: #fff3cd !important;
            color: #856404 !important;
            font-weight: bold;
            min-width: 80px;
        }
        
        /* Remove subject pills styling - keep as simple text */
        .unsuccessful-table .failed-subjects-cell {
            background-color: transparent !important;
        }
        
        /* Removed subject pills styling - now using simple comma-separated text */
        /* .subject-pill styles have been removed as we use plain text formatting */
        
        /* Improve Failed Subjects column display - simple text formatting */
        .failed-subjects-cell {
            min-width: 200px !important;
            max-width: 280px !important;
            white-space: normal !important;
            word-break: break-word !important;
            font-family: Arial, sans-serif;
            color: #495057;
        }
        
        /* Enhanced styling for detailed unsuccessful students table */
        .unsuccessful-students-detailed {
            table-layout: auto !important;
            width: 100% !important;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            font-size: 11px;
        }
        
        .unsuccessful-students-detailed th {
            background: linear-gradient(135deg, #343a40 0%, #495057 100%);
            color: white;
            font-weight: 600;
            text-transform: uppercase;
            font-size: 10px;
            letter-spacing: 0.5px;
            padding: 8px 4px !important;
            text-align: center;
            border: 1px solid #dee2e6;
        }
        
        .unsuccessful-students-detailed td {
            border: 1px solid #dee2e6;
            padding: 6px 4px !important;
            vertical-align: middle;
            font-size: 10px;
        }
        
        .unsuccessful-students-detailed .student-id-col {
            width: 60px !important;
            text-align: center !important;
            font-weight: 600;
            background-color: #f8f9fa;
        }
        
        .unsuccessful-students-detailed .student-name-col {
            width: 150px !important;
            text-align: left !important;
            font-weight: 500;
            background-color: #f8f9fa;
        }
        
        /* Subject detail columns */
        .subject-detail-table {
            width: 100%;
            border-collapse: collapse;
            margin: 0;
            font-size: 10px;
        }
        
        .subject-detail-table th {
            background-color: #e9ecef;
            color: #495057;
            padding: 4px 3px;
            text-align: center;
            border: 1px solid #dee2e6;
            font-weight: 600;
            font-size: 9px;
        }
        
        .subject-detail-table td {
            padding: 3px;
            text-align: center;
            border: 1px solid #dee2e6;
            font-size: 9px;
        }
        
        .subject-detail-table .subject-name {
            writing-mode: vertical-rl;
            text-orientation: mixed;
            font-weight: 600;
            background-color: #f8f9fa;
            min-width: 25px;
            padding: 2px;
        }
        
        .subject-detail-table .creative-label {
            background-color: #fff3cd;
            font-size: 8px;
            font-weight: 500;
        }
        
        .subject-detail-table .om-cell {
            background-color: #d1ecf1;
            color: #0c5460;
            font-weight: 500;
        }
        
        .subject-detail-table .lack-cell {
            background-color: #f8d7da;
            color: #721c24;
            font-weight: 600;
        }
        
        .subject-detail-table .empty-cell {
            background-color: #f8f9fa;
        }
        
        /* Mobile responsiveness */
        @media (max-width: 768px) {
            .nav-tabs .nav-link {
                padding: 8px 12px;
                font-size: 12px;
            }
            
            .tab-content {
                padding: 15px;
            }
            
            .report-table {
                font-size: 10px;
            }
            
            .report-table th,
            .report-table td {
                padding: 4px 2px;
            }
            
            .form-inline .form-group {
                margin-bottom: 10px;
                width: 100%;
            }
            
            .form-inline .form-control {
                width: 100%;
                margin-bottom: 5px;
            }
        }
        
        @media print {
            .d-print-none, .NoPrint {
                display: none !important;
            }
            .nav-tabs {
                display: none !important;
            }
            .tab-content {
                border: none !important;
                padding: 0 !important;
            }
            .tab-pane {
                display: block !important;
                opacity: 1 !important;
            }
            .report-table {
                font-size: 10px;
            }
            .grade-chart {
                margin: 5px;
                padding: 10px;
            }
            body {
                font-size: 12px;
            }
            .table-responsive {
                overflow-x: visible !important;
            }
        }
        
        /* Horizontal scroll indicator for Subject Statistics */
        #tab2 .table-responsive::after {
            content: "← Scroll horizontally to view all columns →";
            display: block;
            text-align: center;
            font-size: 11px;
            color: #6c757d;
            font-style: italic;
            padding: 5px;
            background-color: #f8f9fa;
            border-top: 1px solid #dee2e6;
        }
        
        /* Hide scroll indicator on larger screens */
        @media (min-width: 1200px) {
            #tab2 .table-responsive::after {
                display: none;
            }
        }
        
        /* Dynamic Unsuccessful Students Table - Simplified Design */
        .table-wrapper {
            width: 100%;
            overflow-x: auto;
            border: 1px solid #dee2e6;
            border-radius: 6px;
            margin: 20px 0;
            box-shadow: 0 2px 4px rgba(0,0,0,0.08);
            background: #543d3d;
        }
        
        .dynamic-unsuccessful-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 11px;
            min-width: 800px;
            font-family: Arial, sans-serif;
        }
        
        .dynamic-unsuccessful-table th,
        .dynamic-unsuccessful-table td {
            border: 1px solid #dee2e6;
            padding: 6px 4px;
            text-align: center;
            vertical-align: middle;
        }
        
        /* Simplified Header Styling */
        .dynamic-unsuccessful-table .header-row th {
            background-color: #343a40 !important;
            color: white !important;
            font-weight: bold;
            font-size: 12px;
            padding: 10px 6px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }
        
        .dynamic-unsuccessful-table .sub-header-row th {
            background-color: #007bff !important;
            color: white !important;
            font-weight: bold;
            font-size: 11px;
            padding: 8px 4px;
        }
        
        .dynamic-unsuccessful-table .om-lack-header-row th {
            color: white !important;
            font-weight: bold;
            font-size: 10px;
            padding: 6px 3px;
        }
        
        /* Student Data - Clear and Visible */
        .dynamic-unsuccessful-table tbody td {
            background-color: #543d3d;
            color: #212529 !important;
            font-weight: 500;
            font-size: 11px;
        }
        
        .dynamic-unsuccessful-table tbody tr:nth-child(even) {
            background-color: #f8f9fa;
        }
        
        .dynamic-unsuccessful-table tbody tr:hover {
            background-color: #e3f2fd;
        }
        
        /* Student ID and Name columns */
        .dynamic-unsuccessful-table .student-id-cell,
        .dynamic-unsuccessful-table .student-name-cell {
            background-color: #e9ecef !important;
            color: #212529 !important;
            font-weight: bold;
            border-left: 3px solid #007bff;
        }
        
        .dynamic-unsuccessful-table .student-name-cell {
            text-align: left;
            max-width: 150px;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }
        
        /* Mark cells - clear visibility */
        .dynamic-unsuccessful-table .om-cell {
            color: #dc3545 !important;
            font-weight: bold;
            background-color: white;
        }
        
        .dynamic-unsuccessful-table .lack-cell {
            color: #fd7e14 !important;
            font-weight: bold;
            background-color: white;
        }
        
        /* Pass/Fail indicators */
        .dynamic-unsuccessful-table td[style*="color: #28a745"] {
            background-color: #d4edda !important;
            color: #155724 !important;
            font-weight: bold;
        }
        
        .dynamic-unsuccessful-table td[style*="color: #6c757d"] {
            background-color: #f8f9fa !important;
            color: #6c757d !important;
        }
        
        /* Responsive design */
        @media (max-width: 1200px) {
            .dynamic-unsuccessful-table {
                font-size: 10px;
                min-width: 900px;
            }
            
            .dynamic-unsuccessful-table th,
            .dynamic-unsuccessful-table td {
                padding: 4px 3px;
            }
        }
        
        @media (max-width: 768px) {
            .dynamic-unsuccessful-table {
                font-size: 9px;
                min-width: 700px;
            }
            
            .dynamic-unsuccessful-table th,
            .dynamic-unsuccessful-table td {
                padding: 3px 2px;
            }
            
            .dynamic-unsuccessful-table .student-name-cell {
                max-width: 120px;
            }
        }
        
        /* Print styles */
        @media print {
            .table-wrapper {
                border: none;
                box-shadow: none;
                overflow: visible;
                margin: 5px 0;
            }
            
            .dynamic-unsuccessful-table {
                font-size: 12px !important; /* Increased from 8px */
                min-width: auto;
                width: 100%;
                page-break-inside: auto;
            }
            
            .dynamic-unsuccessful-table th,
            .dynamic-unsuccessful-table td {
                padding: 3px 2px !important; /* Increased padding */
                border: 1px solid #000;
                color: black !important;
                font-size: 12px !important;
            }
            
            /* Enhanced print headers */
            .dynamic-unsuccessful-table .header-row th,
            .dynamic-unsuccessful-table .sub-header-row th,
            .dynamic-unsuccessful-table .om-lack-header-row th {
                background: #f0f0f0 !important;
                color: black !important;
                font-weight: bold !important;
                font-size: 11px !important;
                padding: 4px 2px !important;
            }
            
            /* Better student name visibility in print */
            .dynamic-unsuccessful-table td:nth-child(2) {
                font-size: 13px !important;
                font-weight: bold !important;
                text-align: left !important;
                min-width: 120px !important;
                max-width: none !important;
                white-space: normal !important;
                word-wrap: break-word !important;
            }
            
            /* Student ID column */
            .dynamic-unsuccessful-table td:nth-child(1) {
                font-size: 12px !important;
                font-weight: bold !important;
                min-width: 40px !important;
            }
            
            /* Data cells with better visibility */
            .dynamic-unsuccessful-table tbody td {
                font-size: 11px !important;
                font-weight: 500 !important;
            }
            
            @page {
                margin: 0.4in;
                size: A4 landscape;
            }
            
            /* Better page breaks */
            .dynamic-unsuccessful-table {
                page-break-inside: avoid;
            }
            
            .dynamic-unsuccessful-table thead {
                display: table-header-group;
            }
            
            .dynamic-unsuccessful-table tbody {
                display: table-row-group;
            }
            
            .dynamic-unsuccessful-table tr {
                page-break-inside: avoid;
            }
        }
        
        /* Custom scrollbar */
        .table-wrapper::-webkit-scrollbar {
            height: 6px;
        }
        
        .table-wrapper::-webkit-scrollbar-track {
            background: #f1f1f1;
            border-radius: 3px;
        }
        
        .table-wrapper::-webkit-scrollbar-thumb {
            background: #c1c1c1;
            border-radius: 3px;
        }
        
        .table-wrapper::-webkit-scrollbar-thumb:hover {
            background: #a8a8a8;
        }
    </style>
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
                    OnDataBound="ExamDropDownList_DataBound">
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
                            COUNT(*) as TotalStudents,
                            -- Calculate Passed Students with safe numeric conversion
                            SUM(CASE 
                                -- Check by Grade first
                                WHEN ers.SubjectGrades IN ('A+', 'A', 'A-', 'B', 'C', 'D') THEN 1 
                                -- Check by Pass Status
                                WHEN UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('PASS', 'P') THEN 1
                                -- Check by numerical marks with enhanced validation
                                WHEN UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', '', '0') 
                                     AND ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                     AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                     AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) >= 33 THEN 1
                                ELSE 0 
                            END) as PassedStudents,
                            -- Calculate Failed Students
                            SUM(CASE 
                                -- Check by Grade
                                WHEN ers.SubjectGrades = 'F' THEN 1
                                -- Check by Pass Status
                                WHEN UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F') THEN 1
                                -- Check if absent
                                WHEN UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) IN ('A', 'ABS') THEN 1
                                -- Check by numerical marks
                                WHEN ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                     AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                     AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS')
                                     AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) < 33 THEN 1
                                ELSE 0 
                            END) as FailedStudents,
                            -- Calculate Pass Percentage
                            CAST(CASE 
                                WHEN COUNT(*) > 0 THEN 
                                    (SUM(CASE 
                                        WHEN ers.SubjectGrades IN ('A+', 'A', 'A-', 'B', 'C', 'D') THEN 1 
                                        WHEN UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('PASS', 'P') THEN 1
                                        WHEN UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', '', '0') 
                                             AND ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                             AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                             AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) >= 33 THEN 1
                                        ELSE 0 
                                    END) * 100.0 / COUNT(*))
                                ELSE 0
                            END AS DECIMAL(5,2)) as PassPercentage,
                            -- Calculate Highest Marks
                            ISNULL(MAX(CASE 
                                WHEN ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                     AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                     AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS')
                                THEN CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) 
                                ELSE NULL 
                            END), 0) as HighestMarks,
                            -- Calculate Lowest Marks (excluding absent and zero)
                            ISNULL(MIN(CASE 
                                WHEN ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                     AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                     AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS', '0')
                                THEN CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) 
                                ELSE NULL 
                            END), 0) as LowestMarks,
                            -- Calculate Average Marks (excluding absent)
                            CAST(ISNULL(AVG(CASE 
                                WHEN ISNUMERIC(ers.ObtainedMark_ofSubject) = 1 
                                     AND LEN(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) > 0
                                     AND UPPER(LTRIM(RTRIM(ers.ObtainedMark_ofSubject))) NOT IN ('A', 'ABS')
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
                                -- Multiple failure conditions (same as other queries)
                                UPPER(LTRIM(RTRIM(ISNULL(ers.SubjectGrades, '')))) = 'F'
                                OR UPPER(LTRIM(RTRIM(ISNULL(ers.PassStatus_Subject, '')))) IN ('FAIL', 'F')
                                OR UPPER(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) IN ('A', 'ABS')
                                OR (
                                    ISNUMERIC(ISNULL(ers.ObtainedMark_ofSubject, '')) = 1 
                                    AND LEN(LTRIM(RTRIM(ISNULL(ers.ObtainedMark_ofSubject, '')))) > 0
                                    AND CAST(ers.ObtainedMark_ofSubject AS DECIMAL(10,2)) < 33
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
                $('[id*=ClassDropDownList], [id*=ExamDropDownList]').change(function() {
                    if ($(this).val() !== "0") {
                        $(this).addClass('loading');
                        setTimeout(() => {
                            $(this).removeClass('loading');
                        }, 1000);
                    }
                });

                // Highlight zero values in statistics table
                function highlightZeroValues() {
                    $('.report-table td').each(function() {
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
                    $('.unsuccessful-table tr').each(function() {
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
                    $('.unsuccessful-students-detailed tr').each(function() {
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
                            function() {
                                $(this).css('background-color', '#e3f2fd');
                            },
                            function() {
                                $(this).css('background-color', '');
                            }
                        );
                        
                        // Add tooltips to OM and Lack cells
                        $(this).find('.om-cell').attr('title', 'Obtained Marks in this subject');
                        $(this).find('.lack-cell').attr('title', 'Shortage from required pass marks (33)');
                        
                        // Highlight high shortage values
                        $(this).find('.lack-cell').each(function() {
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
                        $(this).find('.om-cell').each(function() {
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
                    $('.dynamic-unsuccessful-table tr').each(function() {
                        // Add hover effects
                        $(this).hover(
                            function() {
                                $(this).css('background-color', '#f0f8ff');
                            },
                            function() {
                                $(this).css('background-color', '');
                            }
                        );
                        
                        // Enhance OM and Lack cells
                        $(this).find('.om-cell').each(function() {
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
                        
                        $(this).find('.lack-cell').each(function() {
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
                        function() {
                            var index = $(this).index();
                            // Highlight corresponding OM and Lack columns
                            $('.dynamic-unsuccessful-table tr').each(function() {
                                $(this).find('td:eq(' + (index) + '), td:eq(' + (index + 1) + ')').css('background-color', '#e3f2fd');
                            });
                        },
                        function() {
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
                $(document).ajaxComplete(function() {
                    setTimeout(() => {
                        highlightZeroValues();
                        enhanceDynamicUnsuccessfulTable(); // New function name
                    }, 200);
                });

                // Enhanced print functionality - show all tabs when printing
                $('button[onclick*="print"]').off('click').on('click', function(e) {
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
                $('.nav-tabs .nav-link').on('click', function(e) {
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