<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Bangla_Result_DirectPrint.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Bangla_Result_DirectPrint" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Use Google Fonts for better reliability -->
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">
    
    <!-- External CSS for Bangla Result Direct Print -->
    <link href="Assets/bangla-result-directprint.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint" id="pageTitle">বাংলা রেজাল্ট কার্ড</h3>
    
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
            // Convert English numbers to Bengali numbers
            convertNumbersToBengali();
            
            // Fix absent marks display
            fixAbsentMarksDisplay();
            
            // Load database signatures when page loads
            loadDatabaseSignatures();
            
            // Initialize teacher and head teacher text
            updateSignatureTexts();

            function fixAbsentMarksDisplay() {
                // Find all marks tables and fix absent marks
                $('.marks-table').each(function() {
                    var $table = $(this);
                    
                    // Process each row in the table
                    $table.find('tr').each(function(rowIndex) {
                        var $row = $(this);
                        
                        // Skip header rows
                        if ($row.find('th').length > 0) {
                            return;
                        }
                        
                        // Process each cell in the row
                        $row.find('td').each(function(cellIndex) {
                            var $cell = $(this);
                            var cellText = $cell.text().trim();
                            
                            // Get the header for this column to determine what type of column it is
                            var $headerRow = $table.find('tr').first();
                            var $headerCells = $headerRow.find('th');
                            var columnHeader = '';
                            
                            // Find the corresponding header for this cell
                            if (cellIndex < $headerCells.length) {
                                columnHeader = $headerCells.eq(cellIndex).text().trim();
                            }
                            
                            // Only convert 'A' to 'অনুপস্থিত' in marks columns, NOT in grade columns
                            if (cellText === 'A') {
                                // Check if this is a grade column (গ্রেড) - if so, don't convert
                                if (columnHeader === 'গ্রেড' || columnHeader.indexOf('গ্রেড') !== -1) {
                                    return; // Skip grade columns
                                }
                                
                                // Check if this is a marks/score column or sub-exam column
                                if (columnHeader === 'প্রাপ্ত নম্বর' || 
                                    columnHeader.indexOf('নম্বর') !== -1 ||
                                    columnHeader === 'Midterm' ||
                                    columnHeader === 'Periodical' ||
                                    columnHeader === 'Subjective' ||
                                    columnHeader === 'Objective' ||
                                    cellIndex <= 2) { // First few columns are usually marks columns
                                    
                                    $cell.text('অনুপস্থিত');
                                }
                            }
                            // Convert '0' to '-' in total marks column (if it's likely absent)
                            else if (cellText === '0' && $cell.hasClass('total-marks-cell')) {
                                // Check if any sibling cell in same row has 'অনুপস্থিত' or 'A'
                                var hasAbsentMarks = false;
                                
                                $row.find('td').each(function() {
                                    var siblingText = $(this).text().trim();
                                    if (siblingText === 'অনুপস্থিত' || (siblingText === 'A' && !$(this).closest('td').prev().text().trim().match(/গ্রেড/))) {
                                        hasAbsentMarks = true;
                                        return false;
                                    }
                                });
                                
                                if (hasAbsentMarks) {
                                    $cell.text('-');
                                }
                            }
                            // Also check for standalone 0 marks that should be '-' for absent
                            else if (cellText === '0' && !$cell.hasClass('total-marks-cell')) {
                                // Check if this row has absent marks (but not in grade columns)
                                var $row = $cell.closest('tr');
                                var totalCells = $row.find('td').length;
                                var absentCells = 0;
                                
                                $row.find('td').each(function(idx) {
                                    var siblingText = $(this).text().trim();
                                    var siblingHeader = '';
                                    if (idx < $headerCells.length) {
                                        siblingHeader = $headerCells.eq(idx).text().trim();
                                    }
                                    
                                    // Count absent marks but exclude grade columns
                                    if ((siblingText === 'অনুপস্থিত' || siblingText === '-') && 
                                        siblingHeader !== 'গ্রেড' && siblingHeader.indexOf('গ্রেড') === -1) {
                                        absentCells++;
                                    }
                                });
                                
                                // If most non-grade cells are absent, convert 0 to -
                                if (absentCells > totalCells / 3) { // More conservative threshold
                                    // But make sure this isn't a grade column
                                    if (columnHeader !== 'গ্রেড' && columnHeader.indexOf('গ্রেড') === -1 &&
                                        columnHeader !== 'পয়েন্ট' && columnHeader.indexOf('পয়েন্ট') === -1) {
                                        $cell.text('-');
                                    }
                                }
                            }
                        });
                    });
                });
            }

            function convertNumbersToBengali() {
                // Correct Bengali number mapping
                var englishToBengali = {
                    '0': '০',
                    '1': '১',
                    '2': '২', 
                    '3': '৩',
                    '4': '৪',
                    '5': '৫',
                    '6': '৬',
                    '7': '৭',
                    '8': '৮',
                    '9': '৯'
                };

                // Function to convert text
                function convertText(text) {
                    return text.replace(/[0-9]/g, function(match) {
                        return englishToBengali[match] || match;
                    });
                }

                // Convert all text nodes in result cards
                $('.result-card').each(function() {
                    var $card = $(this);
                    
                    // Get elements to exclude from conversion
                    var $excludedElements = $card.find('.header p, .title'); // Address and Exam name
                    
                    // Convert all other elements
                    $card.find('*').not('.header p').not('.title').contents().filter(function() {
                        return this.nodeType === 3; // Text nodes only
                    }).each(function() {
                        var text = this.nodeValue;
                        if (text && /[0-9]/.test(text)) {
                            this.nodeValue = convertText(text);
                        }
                    });

                    // Convert table cell contents (excluding header address area)
                    $card.find('td, th').each(function() {
                        var $cell = $(this);
                        
                        // Skip if this cell is inside header area
                        if ($cell.closest('.header').length > 0) {
                            return;
                        }
                        
                        // Skip if this cell contains absent marks (don't convert '-' or 'অনুপস্থিত')
                        var cellText = $cell.text().trim();
                        if (cellText === '-' || cellText === 'অনুপস্থিত' || cellText === 'A') {
                            return;
                        }
                        
                        var text = $cell.html();
                        if (text && /[0-9]/.test(text)) {
                            // Only convert if it's not an HTML attribute
                            var convertedText = text.replace(/>[^<]*</g, function(match) {
                                return convertText(match);
                            });
                            // Also convert standalone text
                            convertedText = convertedText.replace(/^[^<>]*$/, function(match) {
                                return convertText(match);
                            });
                            $cell.html(convertedText);
                        }
                    });

                    // Convert paragraph and span contents (excluding header p and title)
                    $card.find('p, span, div:not(:has(*))').not('.header p').not('.title').each(function() {
                        var $element = $(this);
                        var text = $element.text();
                        if (text && /[0-9]/.test(text)) {
                            $element.text(convertText(text));
                        }
                    });
                });
            }

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

        // Also convert numbers when new data is loaded via postback
        function Sys$Application$add_pageLoaded(handler) {
            if (typeof(Sys) !== "undefined" && Sys.Application) {
                Sys.Application.add_pageLoaded(handler);
            }
        }

        // Convert numbers after partial postback
        Sys$Application$add_pageLoaded(function() {
            setTimeout(function() {
                // Fix absent marks first
                $('.marks-table').each(function() {
                    var $table = $(this);
                    
                    // Process each row in the table
                    $table.find('tr').each(function(rowIndex) {
                        var $row = $(this);
                        
                        // Skip header rows
                        if ($row.find('th').length > 0) {
                            return;
                        }
                        
                        // Get header cells for column identification
                        var $headerRow = $table.find('tr').first();
                        var $headerCells = $headerRow.find('th');
                        
                        // Process each cell in the row
                        $row.find('td').each(function(cellIndex) {
                            var $cell = $(this);
                            var cellText = $cell.text().trim();
                            
                            // Get the header for this column
                            var columnHeader = '';
                            if (cellIndex < $headerCells.length) {
                                columnHeader = $headerCells.eq(cellIndex).text().trim();
                            }
                            
                            // Only convert 'A' to 'অনুপস্থিত' in marks columns, NOT in grade columns
                            if (cellText === 'A') {
                                // Check if this is a grade column - if so, don't convert
                                if (columnHeader === 'গ্রেড' || columnHeader.indexOf('গ্রেড') !== -1) {
                                    return; // Skip grade columns
                                }
                                
                                // Convert A to অনুপস্থিত in marks columns only
                                if (columnHeader === 'প্রাপ্ত নম্বর' || 
                                    columnHeader.indexOf('নম্বর') !== -1 ||
                                    columnHeader === 'Midterm' ||
                                    columnHeader === 'Periodical' ||
                                    columnHeader === 'Subjective' ||
                                    columnHeader === 'Objective' ||
                                    cellIndex <= 2) { // First few columns are usually marks columns
                                    
                                    $cell.text('অনুপস্থিত');
                                }
                            }
                            // Convert '0' to '-' in total marks column (if it's likely absent)
                            else if (cellText === '0' && $cell.hasClass('total-marks-cell')) {
                                var hasAbsentMarks = false;
                                
                                $row.find('td').each(function() {
                                    var siblingText = $(this).text().trim();
                                    if (siblingText === 'অনুপস্থিত') {
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
                
                // Then convert numbers to Bengali
                var englishToBengali = {
                    '0': '০', '1': '১', '2': '২', '3': '৩', '4': '৪',
                    '5': '৫', '6': '৬', '7': '৭', '8': '৮', '9': '৯'
                };

                function convertText(text) {
                    return text.replace(/[0-9]/g, function(match) {
                        return englishToBengali[match] || match;
                    });
                }

                // Convert all result cards after postback (excluding header and title areas)
                $('.result-card').each(function() {
                    var $card = $(this);
                    
                    // Convert only non-excluded elements
                    $card.find('*').not('.header p').not('.title').contents().filter(function() {
                        return this.nodeType === 3;
                    }).each(function() {
                        var text = this.nodeValue;
                        if (text && /[0-9]/.test(text)) {
                            this.nodeValue = convertText(text);
                        }
                    });

                    // Convert table cells and other elements (excluding header area)
                    $card.find('td, th, span').each(function() {
                        var $element = $(this);
                        
                        // Skip if inside header
                        if ($element.closest('.header').length > 0) {
                            return;
                        }
                        
                        // Skip if contains absent marks or grades
                        var elementText = $element.text().trim();
                        if (elementText === '-' || elementText === 'অনুপস্থিত' || 
                            elementText === 'A+' || elementText === 'A-' || elementText === 'B' || 
                            elementText === 'C' || elementText === 'D' || elementText === 'F') {
                            return;
                        }
                        
                        var text = $element.text();
                        if (text && /[0-9]/.test(text)) {
                            $element.text(convertText(text));
                        }
                    });
                });
            }, 100);
        });
    </script>
</asp:Content>