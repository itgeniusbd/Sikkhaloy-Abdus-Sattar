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
            padding-bottom: 5px; /* Same as print view */
            margin-bottom: 5px; /* Same as print view */
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
            font-size: 22px; /* Same as print view */
            font-weight: bold;
        }
        
        .header p {
            margin: 1px 0; /* Reduced margin */
            font-size: 13px; /* Same as print view */
            font-weight: bold; /* Made bold */
        }
        
        .title {
            text-align: center;
            font-weight: bold;
            margin: 8px 0; /* Same as print view */
            color: green;
            font-size: 18px; /* Same as print view */
        }
        
        .marks-heading {
            text-align: center;
            font-weight: bold;
            margin: 20px 0 10px 0;
            color: #0072bc;
            font-size: 20px; /* Increased font size */
        }
        
        .top-section {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            margin-bottom: 5px; /* Same as print view */
        }
        
        .info-summary {
            width: 72%;
        }
        
        .info-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 6px; /* Keep existing margin */
        }
        
        .info-table td {
            border: 1px solid #0072bc; /* Changed to blue */
            padding: 5px 8px; /* Same as print view */
            font-size: 16px; /* Same as print view */
            font-weight: bold; /* Made bold */
        }
        
        .summary {
            width: 100%;
            border-collapse: collapse;
            margin-top: 5px; /* Same as print view */
            font-family: 'Kalpurush', Arial, sans-serif;
        }
        
        .summary-header {
            background: #0072bc;
            color: white;
            font-weight: bold;
            text-align: center;
        }
        
        .summary-header td {
            padding: 5px 6px; /* Same as print view */
            font-size: 15px; /* Same as print view */
            background-color: #f8f9fa !important;
            -webkit-print-color-adjust: exact !important;
            print-color-adjust: exact !important;
            color: #333 !important;
            font-weight: bold !important;
            border: 1px solid #0072bc; /* Changed to blue */
        }
        
        .summary-values {
            background: #f9f9f9;
        }
        
        .summary-values td {
            padding: 6px; /* Same as print view */
            font-size: 16px; /* Same as print view */
            font-weight: bold;
            text-align: center;
            border: 1px solid #0072bc; /* Changed to blue */
        }
        
        .grade-chart {
            width: 26%;
        }
        
        .grade-chart table {
            border-collapse: collapse;
            width: 100%;
            font-size: 6px; /* Same as print view */
        }
        
        .grade-chart th, .grade-chart td {
            border: 1px solid #0072bc; /* Changed to blue */
            padding: 1px 1px; /* Same as print view */
            text-align: center;
            font-weight: bold; /* Made bold */
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }
        
        .grade-chart th {
            background: #f0f0f0;
            font-weight: bold;
            font-size: 11px; /* Increased font size */
        }
        
        .marks-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 8px; /* Keep existing margin */
        }
        
        .marks-table th, .marks-table td {
            border: 1px solid #0072bc; /* Changed to blue */
            text-align: center;
            padding: 4px; /* Same as print view */
            font-size: 16px; /* Same as print view default */
            font-weight: bold; /* Made bold */
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }
        
        .marks-table th {
            background: #f2f2f2;
            font-size: 18px; /* Larger for headers */
        }
        
        /* Sub-exam table specific styling */
        .marks-table th[colspan] {
            background: #e8f4fd;
            color: #0072bc;
            font-weight: bold;
        }
        
        .marks-table tr:nth-child(2) th {
            background: #f8f9fa;
            font-size: 14px;
        }
        
        /* Dynamic font sizes based on subject count - same as print view */
        .marks-table.medium-subjects th, .marks-table.medium-subjects td {
            font-size: 12px !important; /* Same as print view */
            padding: 3px !important; /* Same as print view */
        }
        
        .marks-table.medium-subjects th {
            font-size: 13px !important; /* Same as print view */
        }
        
        .marks-table.small-subjects th, .marks-table.small-subjects td {
            font-size: 11px !important; /* Same as print view */
            padding: 2px !important; /* Same as print view */
        }
        
        .marks-table.small-subjects th {
            font-size: 12px !important; /* Same as print view */
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
            margin-top: 20px; /* Same as print view */
            font-size: 15px; /* Same as print view */
            font-weight: bold; /* Made bold */
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }

        /* Print color adjustments */
        @media print {
            /* Default landscape orientation */
            @page {
                size: A4 landscape;
                margin: 2mm; /* Proper margin for pagination */
            }
            
            /* Portrait mode specific rules */
            @media print and (orientation: portrait) {
                @page {
                    size: A4 portrait;
                    margin: 2mm; /* Slightly more margin for portrait */
                }
                
                .result-card {
                    max-width: none !important;
                    width: auto !important;
                    height: auto !important;
                    padding: 8mm !important; /* More padding for portrait */
                    margin: 3mm !important;
                    border: 3px solid #0072bc !important; /* Thicker border */
                    box-sizing: border-box !important;
                    page-break-before: auto !important;
                    page-break-inside: avoid !important;
                    page-break-after: always !important;
                    position: relative !important;
                    top: auto !important;
                    left: auto !important;
                }
                
                /* Larger fonts for portrait mode */
                .header h2 {
                    font-size: 26px !important; /* Larger for portrait */
                }
                
                .header p {
                    font-size: 16px !important; /* Larger for portrait */
                }
                
                .title {
                    font-size: 22px !important; /* Larger for portrait */
                    margin: 12px 0 !important;
                }
                
                .info-table td {
                    font-size: 16px !important; /* Keep original size for portrait */
                    padding: 6px 8px !important; /* Keep original padding */
                }
                
                .summary-header td {
                    font-size: 14px !important; /* Keep original size for portrait */
                    padding: 6px 6px !important; /* Keep original padding */
                }
                
                .summary-values td {
                    font-size: 15px !important; /* Keep original size for portrait */
                    padding: 6px !important; /* Keep original padding */
                }
                
                .grade-chart table {
                    font-size: 11px !important; /* Larger for portrait */
                }
                
                .grade-chart th {
                    font-size: 12px !important;
                }
                
                /* Larger fonts for marks table in portrait */
                .marks-table th, .marks-table td {
                    font-size: 13px !important; /* Default larger font for portrait */
                    padding: 4px !important;
                }
                
                .marks-table th {
                    font-size: 18px !important; /* Larger headers for portrait */
                }
                
                /* Portrait specific dynamic font sizes */
                .marks-table.medium-subjects th, .marks-table.medium-subjects td {
                    font-size: 15px !important; /* Larger medium font for portrait */
                    padding: 5px !important;
                }
                
                .marks-table.medium-subjects th {
                    font-size: 16px !important;
                }
                
                .marks-table.small-subjects th, .marks-table.small-subjects td {
                    font-size: 14px !important; /* Larger small font for portrait */
                    padding: 4px !important;
                }
                
                .marks-table.small-subjects th {
                    font-size: 16px !important;
                }
                
                .footer {
                    font-size: 18px !important; /* Larger footer for portrait */
                    margin-top: 25px !important;
                }
                
                /* Better spacing for portrait */
                .top-section {
                    margin-bottom: 8px !important;
                }
                
                .header {
                    padding-bottom: 8px !important;
                    margin-bottom: 8px !important;
                }
            }
            
            /* Prevent empty page at start */
            * {
                box-sizing: border-box !important;
            }
            
            body {
                margin: 0 !important;
                padding: 0 !important;
            }
            
            .result-card {
                max-width: none !important; /* Remove width restrictions */
                width: auto !important; /* Let it flow naturally */
                height: auto !important; /* Remove fixed height */
                padding: 5mm !important;
                margin: 5mm !important; /* Proper margins */
                border: 4px solid #0072bc !important;
                transform: none !important; /* Remove transform */
                transform-origin: none !important;
                box-sizing: border-box !important;
                page-break-before: auto !important;
                page-break-inside: avoid !important;
                page-break-after: always !important; /* Force page break for multiple cards */
                position: relative !important; /* Remove absolute positioning */
                top: auto !important;
                left: auto !important;
            }
            
            .header {
                padding-bottom: 5px !important;
                margin-bottom: 5px !important;
            }
            
            .header h2 {
                font-size: 22px !important; /* Increased for print */
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: bold !important;
            }
            
            .header p {
                font-size: 13px !important; /* Increased for print */
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: normal !important;
            }
            
            .title {
                margin: 8px 0 !important;
                font-size: 18px !important; /* Increased for print */
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: bold !important;
            }
            
            .top-section {
                margin-bottom: 5px !important;
            }
            
            .info-table td {
                padding: 5px 8px !important; /* Increased padding */
                font-size: 16px !important; /* Keep original font size */
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: bold !important;
            }
            
            .summary td {
                padding: 4px !important;
                font-size: 13px !important;
            }
            
            .grade-chart table {
                font-size: 6px !important; /* Increased font size */
            }
            
            .grade-chart th, .grade-chart td {
                padding: 1.5px 1.5px !important; /* Increased padding */
            }
            
            .marks-table th, .marks-table td {
                padding: 4px !important; /* Increased padding */
                font-size: 16px !important; /* Default font size for 5 or less subjects */
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: bold !important;
            }
            
            /* Dynamic font sizes for print based on subject count */
            .marks-table.medium-subjects th, .marks-table.medium-subjects td {
                font-size: 13px !important; /* Smaller font for 6-10 subjects */
                padding: 3px !important; /* Reduced padding */
            }
            
            .marks-table.medium-subjects th {
                font-size: 14px !important; /* Smaller header font */
            }
            
            .marks-table.small-subjects th, .marks-table.small-subjects td {
                font-size: 11px !important; /* Very small font for 10-15 subjects */
                padding: 2px !important; /* Minimal padding for small fonts */
            }
            
            .marks-table.small-subjects th {
                font-size: 11px !important; /* Very small header font */
            }
            
            .footer {
                margin-top: 20px !important;
                font-size: 15px !important;
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: bold !important;
            }
            
            .summary-header td {
                padding: 6px 6px !important; /* Keep original padding */
                font-size: 14px !important; /* Keep original font size */
                background-color: #f8f9fa !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
                color: #333 !important;
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: bold !important;
            }
            
            .summary-values td {
                padding: 6px !important; /* Keep original padding */
                font-size: 15px !important; /* Keep original font size */
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: bold !important;
            }
            
            .grade-chart th, .grade-chart td {
                padding: 1px 2px !important; /* Increased padding */
                font-family: Arial, sans-serif !important; /* Use Arial for print */
                font-weight: bold !important;
            }
            
            /* Color backgrounds for print view */
            .summary-values td:nth-child(1) { 
                background: #ffd966 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary-values td:nth-child(2) { 
                background: #f4b183 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary-values td:nth-child(3) { 
                background: #a9d08e !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary-values td:nth-child(4) { 
                background: #9dc3e6 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary-values td:nth-child(5) { 
                background: #c5e0b4 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary-values td:nth-child(6) { 
                background: #ffe699 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
            .summary-values td:nth-child(7) { 
                background: #d9d2e9 !important;
                -webkit-print-color-adjust: exact !important;
                print-color-adjust: exact !important;
            }
        }
        
        .summary td {
            padding: 4px; /* Same as print view */
            font-size: 13px; /* Same as print view */
            font-weight: bold;
            text-align: center;
            border: 1px solid #0072bc; /* Changed to blue */
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
                        SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
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
                        SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
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

    <%if (ExamDropDownList.SelectedIndex != 0)
    {%>
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
                           <td> নাম:</td> <td colspan="6"><b><%# Eval("StudentsName") %></b></td>
                                </tr>
                                <tr>
                                    <td>ক্লাস:</td>
                                    <td><%# Eval("ClassName") %></td>
                                    <td>গ্রুপ:</td>
                                    <td><%# Eval("GroupName") %></td>
                                    <td>শাখা:</td>
                                    <td><%# Eval("SectionName") %></td>
                                </tr>

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
                                    <td>শাখা মেধা</td>
                                </tr>
                                <tr class="summary-values">
                                    <td><%# Eval("TotalExamObtainedMark_ofStudent") %>/<%# Eval("TotalMark_ofStudent") %></td>
                                    <td><%# Eval("ObtainedPercentage_ofStudent", "{0:F2}") %></td>
                                    <td><%# Eval("Average", "{0:F2}") %></td>
                                    <td><%# Eval("Student_Grade") %></td>
                                    <td><%# Eval("Student_Point", "{0:F1}") %></td>
                                    <td><%# Eval("Position_InExam_Class") %></td>
                                    <td><%# Eval("Position_InExam_Subsection") %></td>
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
    <%}%>

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