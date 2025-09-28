<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="BanglaResult.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Bangla_Result_DirectPrint" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Use Google Fonts for better reliability -->
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">
    
    <!-- External CSS for Bangla Result Direct Print -->
    <link href="Assets/bangla-result-directprint.css" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint" id="pageTitle">বাংলা রেজাল্ট কার্ড     <a href="BanglaResult_Old.aspx"><span class="btn-text-full">Old</span> </a></h3>

    
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

            <%if (GroupDropDownList.Items.Count > 1)
            {%>
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
            <%}%>

            <%if (SectionDropDownList.Items.Count > 1)
            {%>
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
            <%}%>

            <%if (ShiftDropDownList.Items.Count > 1)
            {%>
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
            <%}%>

            <%if (ExamDropDownList.Items.Count > 1)
            {%>
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
            <%}%>

            <div class="col-md-2">
                <div class="form-group">
                    <label>Student ID</label>
                    <asp:TextBox ID="StudentIDTextBox" runat="server" CssClass="form-control" 
                        placeholder="১০১,১০৫,১০৭" 
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
                        <button type="button" onclick="toggleNumberLanguage()" class="btn btn-warning btn-sm" 
                            id="NumberToggleButton" style="display:none; flex: 1; height: 34px; margin-left: 5px;">
                            বাংলা সংখ্যা
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
                <asp:TextBox ID="TeacherSignTextBox" Text="শ্রেনি শিক্ষক" runat="server" placeholder="শ্রেণি শিক্ষকের স্বাক্ষর" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                <label class="btn btn-secondary btn-sm NoPrint" for="Tfileupload" style="margin-left: 5px; margin-top: 5px; cursor: pointer;">
                    Browse
                </label>
                <input id="Tfileupload" type="file" accept="image/*" style="position: absolute; left: -9999px; opacity: 0;" />
            </div>
            <div class="form-group NoPrint" style="margin-right: 15px;">
                <asp:TextBox ID="HeadTeacherSignTextBox" Text="প্রধান শিক্ষক" runat="server" placeholder="মুখ্য শিক্ষকের স্বাক্ষর" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                <label class="btn btn-secondary btn-sm" for="Hfileupload" style="margin-left: 5px; margin-top: 5px; cursor: pointer;">
                    Browse
                </label>
                <input id="Hfileupload" type="file" accept="image/*" style="position: absolute; left: -9999px; opacity: 0;" />
            </div>
        </div>
                <div class="pagination-inline NoPrint" style="margin-bottom: 15px; text-align: center;">
            <asp:Label ID="PaginationInfoLabel" runat="server" CssClass="pagination-label" 
                Text="লোড হয়েছে 0 থেকে 0 জন। মোট 0 শিক্ষার্থী থেকে"></asp:Label>
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
                                    <td><%# Eval("ObtainedPercentage_ofStudent") == DBNull.Value ? "0.00" : String.Format("{0:F2}", Eval("ObtainedPercentage_ofStudent")) %></td>
                                    <td><%# Eval("Average") == DBNull.Value ? "0.00" : String.Format("{0:F2}", Eval("Average")) %></td>
                                    <td><%# Eval("Student_Grade") == DBNull.Value ? "N/A" : Eval("Student_Grade") %></td>
                                    <td><%# Eval("Student_Point") == DBNull.Value ? "0.0" : String.Format("{0:F1}", Eval("Student_Point")) %></td>
                                    <td><%# Eval("Position_InExam_Class") == DBNull.Value ? "N/A" : Eval("Position_InExam_Class") %></td>
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
                    <%# GenerateSubjectMarksTable(Eval("StudentResultID").ToString(), Eval("Student_Grade").ToString(), Eval("Student_Point") == DBNull.Value ? 0m : Convert.ToDecimal(Eval("Student_Point"))) %>

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
        $(document).ready(function () {
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
                // Set initial button state to show "বাংলা সংখ্যা" since numbers are in English by default
                $('#NumberToggleButton').html('বাংলা সংখ্যা').removeClass('btn-info').addClass('btn-warning');
                isNumbersBengali = false; // Set to false since numbers are in English by default
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
                        // Set initial button state to show "বাংলা সংখ্যা" since numbers are in English by default
                        $('#NumberToggleButton').html('বাংলা সংখ্যা').removeClass('btn-info').addClass('btn-warning');
                        isNumbersBengali = false; // Numbers are in English by default
                    }
                }, 1000);
            });

            // Add input validation for Student ID textbox - allow alphanumeric
            $("[id*=StudentIDTextBox]").on('input', function () {
                var value = $(this).val();
                // Allow alphanumeric characters, Bengali numbers, commas, and spaces
                var validChars = /^[a-zA-Z0-9০১২৩৪৫৬৭৮৯,،\s]*$/;

                if (!validChars.test(value)) {
                    // Remove invalid characters
                    value = value.replace(/[^a-zA-Z0-9০১২৩৪৫৬৭৮৯,،\s]/g, '');
                    $(this).val(value);
                }
            });

            // Add helpful tooltips and validation feedback
            $("[id*=StudentIDTextBox]").on('blur', function () {
                var value = $(this).val().trim();
                if (value) {
                    // Convert Bengali to English for validation
                    var englishValue = convertBengaliToEnglishJS(value);
                    var ids = englishValue.split(/[,،]/).map(id => id.trim()).filter(id => id);

                    // More flexible validation for alphanumeric IDs
                    var invalidIds = ids.filter(id => !/^[a-zA-Z0-9]+$/.test(id) || id.length === 0);
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

        // Function to apply pagination button styles
        function applyPaginationStyles() {
            // Apply black background and white text to pagination buttons
            $('.pagination-inline .btn-outline-primary').each(function () {
                $(this).css({
                    'color': '#ffffff',
                    'background-color': '#000000',
                    'border-color': '#000000',
                    'font-weight': 'bold'
                });

                // Additionally, add a hover effect
                $(this).hover(
                    function () {
                        $(this).css('background-color', '#333333');
                    },
                    function () {
                        $(this).css('background-color', '#000000');
                    }
                );
            });
        }

        // Global variable to track number language state
        var isNumbersBengali = false; // Default to English - numbers will start in English

        // Toggle number language between English and Bengali
        function toggleNumberLanguage() {
            var button = document.getElementById('NumberToggleButton');

            if (isNumbersBengali) {
                // Convert to English
                convertNumbersToEnglish();
                button.innerHTML = 'বাংলা সংখ্যা';
                button.className = 'btn btn-warning btn-sm';
                isNumbersBengali = false;
                console.log('Numbers converted to English');
            } else {
                // Convert to Bengali
                convertNumbersToBengali();
                button.innerHTML = 'ইংরেজি সংখ্যা';
                button.className = 'btn btn-info btn-sm';
                isNumbersBengali = true;
                console.log('Numbers converted to Bengali');
            }
        }

        // Convert numbers when new data is loaded via postback - using proper ASP.NET approach
        function pageLoad(sender, args) {
            if (args.get_isPartialLoad()) {
                setTimeout(function () {
                    convertNumbersAfterPostback();
                    // Reapply pagination styles after postback
                    applyPaginationStyles();
                }, 100);
            }
        }

        function convertNumbersAfterPostback() {
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

                            $row.find('td').each(function () {
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

            // DON'T convert numbers to Bengali after postback - keep them in English by default
            // convertNumbersToBengali(); // Commented out - keep numbers in English by default

            // Reset button state to show "বাংলা সংখ্যা" since numbers are in English
            if ($('#NumberToggleButton').length > 0) {
                $('#NumberToggleButton').html('বাংলা সংখ্যা').removeClass('btn-info').addClass('btn-warning');
                isNumbersBengali = false;
            }
        }

        // Function to convert Bengali numbers back to English
        function convertNumbersToEnglish() {
            var bengaliToEnglish = {
                '০': '0', '১': '1', '২': '2', '৩': '3', '৪': '4',
                '৫': '5', '৬': '6', '৭': '7', '৮': '8', '৯': '9'
            };

            function convertText(text) {
                return text.replace(/[০-৯]/g, function (match) {
                    return bengaliToEnglish[match] || match;
                });
            }

            // Convert all text nodes in result cards
            $('.result-card').each(function () {
                var $card = $(this);

                // Skip header elements (address, exam name)
                var $excludedElements = $card.find('.header p, .title');

                // Convert all other elements
                $card.find('*').not('.header p').not('.title').contents().filter(function () {
                    return this.nodeType === 3; // Text nodes only
                }).each(function () {
                    var text = this.nodeValue;
                    if (text && /[০-৯]/.test(text)) {
                        this.nodeValue = convertText(text);
                    }
                });

                // Convert table cell contents (excluding header address area)
                $card.find('td, th').each(function () {
                    var $cell = $(this);

                    // Skip if this cell is inside header area
                    if ($cell.closest('.header').length > 0) {
                        return;
                    }

                    // Skip if this cell contains absent marks
                    var cellText = $cell.text().trim();
                    if (cellText === '-' || cellText === 'অনুপস্থিত') {
                        return;
                    }

                    // Convert Bengali numbers to English numbers
                    $cell.text(convertText(cellText));
                });
            });
        }

        // Convert numbers to Bengali function
        function convertNumbersToBengali() {
            var englishToBengali = {
                '0': '০', '1': '১', '2': '২', '3': '৩', '4': '৪',
                '5': '৫', '6': '৬', '7': '৭', '8': '৮', '9': '৯'
            };

            function convertText(text) {
                return text.replace(/[0-9]/g, function (match) {
                    return englishToBengali[match] || match;
                });
            }

            // Convert all text nodes in result cards
            $('.result-card').each(function () {
                var $card = $(this);

                // Skip header elements (address, exam name)
                var $excludedElements = $card.find('.header p, .title');

                // Convert all other elements
                $card.find('*').not('.header p').not('.title').contents().filter(function () {
                    return this.nodeType === 3; // Text nodes only
                }).each(function () {
                    var text = this.nodeValue;
                    if (text && /[0-9]/.test(text)) {
                        this.nodeValue = convertText(text);
                    }
                });

                // Convert table cell contents (excluding header address area)
                $card.find('td, th').each(function () {
                    var $cell = $(this);

                    // Skip if this cell is inside header area
                    if ($cell.closest('.header').length > 0) {
                        return;
                    }

                    // Skip if this cell contains absent marks
                    var cellText = $cell.text().trim();
                    if (cellText === '-' || cellText === 'অনুপস্থিত' || cellText === 'A') {
                        return;
                    }

                    var text = $cell.html();
                    if (text && /[0-9]/.test(text)) {
                        var convertedText = text.replace(/>[^<]*</g, function (match) {
                            return convertText(match);
                        });
                        convertedText = convertedText.replace(/^[^<>]*$/, function (match) {
                            return convertText(match);
                        });
                        $cell.html(convertedText);
                    }
                });

                // Convert paragraph and span contents (excluding header p and title)
                $card.find('p, span, div:not(:has(*))').not('.header p').not('.title').each(function () {
                    var $element = $(this);
                    var text = $element.text();
                    if (text && /[0-9]/.test(text)) {
                        $element.text(convertText(text));
                    }
                });
            });

            // Also convert pagination info
            $('.pagination-label, .page-info-inline').each(function () {
                var $element = $(this);
                var text = $element.text();
                if (text && /[0-9]/.test(text)) {
                    $element.text(convertText(text));
                }
            });
        }

        // Helper functions
        function convertBengaliToEnglishJS(text) {
            var bengaliToEnglish = {
                '০': '0', '১': '1', '২': '2', '৩': '3', '৪': '4',
                '৫': '5', '৬': '6', '৭': '7', '৮': '8', '৯': '9'
            };

            return text.replace(/[০-৯]/g, function (match) {
                return bengaliToEnglish[match] || match;
            });
        }

        function fixAbsentMarksDisplay() {
            // Find all marks tables and fix absent marks
            $('.marks-table').each(function () {
                var $table = $(this);

                // Get header cells for column identification - define this at table level
                var $headerRow = $table.find('tr').first();
                var $headerCells = $headerRow.find('th');

                console.log('Processing table with', $headerCells.length, 'header cells');

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

                        // Get the header for this column to determine what type of column it is
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

                            $row.find('td').each(function () {
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
                            var $currentRow = $cell.closest('tr');
                            var totalCells = $currentRow.find('td').length;
                            var absentCells = 0;

                            $currentRow.find('td').each(function (idx) {
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

                            // If most non-grade cells are absent, convert 0 to -'
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
            img.onload = function () {
                var $img = $("<img />");
                $img.attr("style", "height:35px;width:80px;object-fit:contain;");
                $img.attr("src", imagePath);
                $(targetElement).html($img);
            };

            img.src = imagePath;
        }

        // Signature upload functionality
        function initializeSignatureUpload() {
            console.log('Initializing signature upload functionality');

            // Make sure file inputs are properly accessible
            var teacherInput = document.getElementById('Tfileupload');
            var headInput = document.getElementById('Hfileupload');

            console.log('Teacher input found:', teacherInput !== null);
            console.log('Head input found:', headInput !== null);

            // Clear any existing event handlers to prevent duplicates
            $('#Tfileupload').off('change');
            $('#Hfileupload').off('change');
            $('label[for="Tfileupload"]').off('click');
            $('label[for="Hfileupload"]').off('click');

            // Teacher signature upload - single binding
            $('#Tfileupload').on('change', function (e) {
                console.log('Teacher file input changed');
                handleFileUpload(e, 'teacher');
                // Don't clear the input value here - let the browser handle it
            });

            // Principal signature upload - single binding
            $('#Hfileupload').on('change', function (e) {
                console.log('Principal file input changed');
                handleFileUpload(e, 'principal');
                // Don't clear the input value here - let the browser handle it
            });

            // Direct click handlers for labels - more reliable
            $('label[for="Tfileupload"]').on('click', function (e) {
                e.preventDefault(); // Prevent any default behavior
                console.log('Teacher browse label clicked');
                var input = document.getElementById('Tfileupload');
                if (input) {
                    // Clear previous value to ensure change event fires even for same file
                    input.value = '';
                    input.click();
                }
            });

            $('label[for="Hfileupload"]').on('click', function (e) {
                e.preventDefault(); // Prevent any default behavior
                console.log('Principal browse label clicked');
                var input = document.getElementById('Hfileupload');
                if (input) {
                    // Clear previous value to ensure change event fires even for same file
                    input.value = '';
                    input.click();
                }
            });

            // Also handle direct clicks on file inputs (fallback)
            $('#Tfileupload').on('click', function () {
                console.log('Teacher file input clicked directly');
                this.value = ''; // Clear to ensure change event
            });

            $('#Hfileupload').on('click', function () {
                console.log('Principal file input clicked directly');
                this.value = ''; // Clear to ensure change event
            });
        }

        // Centralized file upload handler - improved
        function handleFileUpload(e, signatureType) {
            console.log(signatureType + ' file upload started');
            var file = e.target.files[0];

            if (!file) {
                console.log('No file selected for ' + signatureType);
                return;
            }

            console.log('File details for ' + signatureType + ':', {
                name: file.name,
                type: file.type,
                size: file.size
            });

            // Validate file type
            if (!file.type.match(/image\/.*/)) {
                alert('Please select a valid image file (JPG, PNG, GIF, etc.).');
                console.log('Invalid file type selected:', file.type);
                // Clear the input
                e.target.value = '';
                return;
            }

            // Validate file size (max 5MB)
            if (file.size > 5 * 1024 * 1024) {
                alert('File size too large. Please select an image smaller than 5MB.');
                console.log('File size too large:', file.size);
                // Clear the input
                e.target.value = '';
                return;
            }

            var reader = new FileReader();

            reader.onload = function (readerEvent) {
                var targetElement = signatureType === 'teacher' ? '.SignTeacher' : '.SignHead';

                // Show preview with improved styling
                $(targetElement).html('<img src="' + readerEvent.target.result + '" style="height:35px;width:80px;object-fit:contain;border:1px solid #ddd;border-radius:3px;" />');
                console.log(signatureType + ' signature preview updated successfully');

                // Extract base64 data for database save
                var base64 = readerEvent.target.result.split(',')[1];

                // Debug: Log the AJAX URL that will be called
                var ajaxUrl = window.location.pathname.replace(/[^\/]+$/, 'BanglaResult.aspx/SaveSignature');
                console.log('AJAX URL will be:', ajaxUrl);
                console.log('Current page:', window.location.pathname);

                // Save to database with better error handling
                $.ajax({
                    type: 'POST',
                    url: 'BanglaResult.aspx/SaveSignature',
                    data: JSON.stringify({
                        signatureType: signatureType,
                        imageData: base64
                    }),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function (response) {
                        console.log('AJAX Success Response:', response);
                        if (response.d && response.d.success) {
                            console.log(signatureType + ' signature saved to database successfully');
                            // Optional: Show success message
                            // alert(signatureType + ' signature uploaded successfully!');
                        } else {
                            console.error('Server returned failure:', response.d);
                            alert('Error saving ' + signatureType + ' signature: ' + (response.d ? response.d.message : 'Unknown error'));
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('AJAX Error Details:', {
                            status: xhr.status,
                            statusText: xhr.statusText,
                            responseText: xhr.responseText,
                            error: error
                        });
                        
                        var errorMessage = 'Error uploading ' + signatureType + ' signature: ';
                        
                        if (xhr.status === 404) {
                            errorMessage += 'Page method not found. Check if SaveSignature method exists.';
                        } else if (xhr.status === 500) {
                            errorMessage += 'Server error: ' + xhr.responseText;
                        } else {
                            errorMessage += error + ' (Status: ' + xhr.status + ')';
                        }
                        
                        alert(errorMessage);
                    }
                });
            };

            reader.onerror = function (readerEvent) {
                console.error('File read error for ' + signatureType + ':', readerEvent);
                alert('Error reading file. Please try again.');
                // Clear the input
                e.target.value = '';
            };

            // Start reading the file
            reader.readAsDataURL(file);
        }
    </script>
</asp:Content>