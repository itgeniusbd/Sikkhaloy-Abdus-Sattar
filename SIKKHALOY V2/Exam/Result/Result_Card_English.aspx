<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Result_Card_English.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Result_Card_English" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <!-- Use Google Fonts for better reliability -->
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">

    <!-- Additional Font Awesome support for this page -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" crossorigin="anonymous" />

    <!-- External CSS for English Result Card with cache busting -->
    <link href="Assets/Result_Card_English.css?v=<%= DateTime.Now.Ticks %>" rel="stylesheet" type="text/css" />

    <style>
        /* PS Column Visibility Control */
        .ps-column.ps-hidden {
            display: none !important;
        }

        .ps-column {
            transition: opacity 0.3s ease;
        }

        /* Force consistent font sizes between normal and print view */
        @media screen {
            .result-card {
                font-size: 14px !important;
            }
            
            .header h2 {
                font-size: 22px !important;
            }
            
            .header p {
                font-size: 14px !important;
            }
            
            .title, .Exam_name {
                font-size: 16px !important;
            }
            
            .info-table td {
                font-size: 14px !important;
            }
            
            .summary-header td, .summary-values td {
                font-size: 14px !important;
            }
            
            .marks-table th, .marks-table td {
                font-size: 12px !important;
            }
            
            .grade-chart th, .grade-chart td {
                font-size: 12px !important;
            }
            
            .footer {
                font-size: 14px !important;
            }
        }

        @media print {
            .result-card {
                font-size: 14px !important;
            }
            
            .header h2 {
                font-size: 22px !important;
            }
            
            .header p {
                font-size: 14px !important;
            }
            
            .title, .Exam_name {
                font-size: 16px !important;
            }
            
            .info-table td {
                font-size: 14px !important;
            }
            
            .summary-header td, .summary-values td {
                font-size: 14px !important;
            }
            
            .marks-table th, .marks-table td {
                font-size: 12px !important;
            }
            
            .grade-chart th, .grade-chart td {
                font-size: 12px !important;
            }
            
            .footer {
                font-size: 14px !important;
            }
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
                                <tr>
                                    <td>Name:</td>
                                    <td colspan="3"><b><%# Eval("StudentsName") %></b></td>
                                </tr>
                                <%-- Use helper method for dynamic row generation --%>
                                <%# GetDynamicInfoRow(Container.DataItem) %>
                                <tr>
                                    <td>Roll:</td>
                                    <td><%# Eval("RollNo") %></td>
                                    <td>ID:</td>
                                    <td><%# Eval("ID") %></td>
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
                    <p class="note">WD: Working Days.PM: Pass Marks. FM: Full Marks. OM: Obtained Marks. PC: Position in Class. PS: Position in Section.HMC: Highest Marks in Class. HMS: Highest Marks in Section</p>
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
            $('.result-card').each(function () {
                var $card = $(this);

                // Fix map marker icon
                $card.find('.fa-map-marker, .fa-map-marker-alt').each(function () {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '📍');
                    }
                });

                // Fix phone icon
                $card.find('.fa-phone').each(function () {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '📞');
                    }
                });

                // Fix envelope icon
                $card.find('.fa-envelope, .fa-envelope-o').each(function () {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '✉️');
                    }
                });
            });
        }

        // Fix absent marks display function
        function fixAbsentMarksDisplay() {
            $('.marks-table').each(function () {
                var $table = $(this);

                // Process each data row
                $table.find('tr').each(function () {
                    var $row = $(this);

                    // Skip header rows
                    if ($row.find('th').length > 0) return;

                    // Check each cell for absent marks
                    $row.find('td').each(function () {
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
            $('#Tfileupload').off('change').on('change', function (e) {
                console.log('Teacher file input changed');
                handleSignatureUpload(e, 'teacher', '.SignTeacher');
            });

            // Principal signature upload
            $('#Hfileupload').off('change').on('change', function (e) {
                console.log('Principal file input changed');
                handleSignatureUpload(e, 'principal', '.SignHead');
            });

            // Update signature texts when textboxes change
            $('[id$="TeacherSignTextBox"]').off('input').on('input', function () {
                var text = $(this).val() || 'Class Teacher';
                $('.Teacher').text(text);
            });

            $('[id$="HeadTeacherSignTextBox"]').off('input').on('input', function () {
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
            reader.onload = function (e) {
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
                    success: function (response) {
                        console.log('Signature saved successfully:', response);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error saving signature:', error);
                        alert('Error saving signature. Please try again.');
                    }
                });
            };

            reader.readAsDataURL(file);
        }

        // Function to apply pagination styles
        function applyPaginationStyles() {
            $('.pagination-inline .btn').each(function () {
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

            return text.replace(/[০-৯]/g, function (match) {
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
                    var ids = englishValue.split(/[,]/).map(function (id) { return id.trim(); }).filter(function (id) { return id; });

                    // More flexible validation for alphanumeric IDs
                    var invalidIds = ids.filter(function (id) { return !/^[a-zA-Z0-9]+$/.test(id) || id.length === 0; });
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

            $('.marks-table').each(function () {
                var $table = $(this);

                // Read header texts to detect column indices dynamically
                var $headerRows = $table.find('tr').slice(0, 2); // first 1-2 rows contain headers
                var headerTexts = [];

                // Build a flat header cell list from the last header row that contains column titles
                var $titleHeader = $headerRows.last();
                var $headerCells = $titleHeader.find('th');

                $headerCells.each(function (i) {
                    headerTexts.push($(this).text().trim().toUpperCase());
                });

                function findIndexByTitle(title) {
                    var upper = title.toUpperCase();
                    for (var i = 0; i < headerTexts.length; i++) {
                        if (headerTexts[i] === upper) return i;
                    }
                    return -1;
                }

                var pcIndex = findIndexByTitle('PC');
                var psIndex = findIndexByTitle('PS');
                var hmcIndex = findIndexByTitle('HMC');
                var hmsIndex = findIndexByTitle('HMS');

                // Fallback if header couldn't be detected: assume last columns in order PC,(PS),HMC,(HMS)
                if (pcIndex === -1 && hmcIndex === -1) {
                    var totalCells = $titleHeader.find('th').length;
                    // Determine how many position columns exist (2 or 4)
                    var hasSection = $table.find('th').filter(function(){return $(this).text().trim().toUpperCase()==='PS' || $(this).text().trim().toUpperCase()==='HMS';}).length > 0;
                    if (hasSection) {
                        pcIndex = totalCells - 4;
                        psIndex = totalCells - 3;
                        hmcIndex = totalCells - 2;
                        hmsIndex = totalCells - 1;
                    } else {
                        pcIndex = totalCells - 2;
                        hmcIndex = totalCells - 1;
                        psIndex = -1;
                        hmsIndex = -1;
                    }
                }

                // Apply classes to all rows for detected indices
                $table.find('tr').each(function () {
                    var $row = $(this);
                    var $cells = $row.find('th, td');

                    // Remove existing position classes
                    $cells.removeClass('position-col-pc position-col-ps position-col-hmc position-col-hms');

                    if (pcIndex >= 0 && $cells.eq(pcIndex).length) $cells.eq(pcIndex).addClass('position-col-pc');
                    if (psIndex >= 0 && $cells.eq(psIndex).length) $cells.eq(psIndex).addClass('position-col-ps');
                    if (hmcIndex >= 0 && $cells.eq(hmcIndex).length) $cells.eq(hmcIndex).addClass('position-col-hmc');
                    if (hmsIndex >= 0 && $cells.eq(hmsIndex).length) $cells.eq(hmsIndex).addClass('position-col-hms');
                });

                // Apply dynamic styling for position columns
                var totalCells = $titleHeader.find('th').length;
                var columnWidth = Math.max(28, Math.min(40, Math.floor(100 / totalCells)));
                var rightOffset = 0;

                // Always align from right: HMS, HMC, PS, PC (only if exists)
                if ($table.find('.position-col-hms').length) {
                    $table.find('.position-col-hms').css({
                        'right': '0px',
                        'min-width': columnWidth + 'px',
                        'background-color': '#f8f9fa',
                        'font-weight': 'bold'
                    });
                    rightOffset += columnWidth;
                }

                if ($table.find('.position-col-hmc').length) {
                    $table.find('.position-col-hmc').css({
                        'right': (rightOffset) + 'px',
                        'min-width': columnWidth + 'px',
                        'background-color': '#f8f9fa',
                        'font-weight': 'bold'
                    });
                    rightOffset += columnWidth;
                }

                if ($table.find('.position-col-ps').length) {
                    $table.find('.position-col-ps').css({
                        'right': (rightOffset) + 'px',
                        'min-width': columnWidth + 'px',
                        'background-color': '#f8f9fa',
                        'font-weight': 'bold'
                    });
                    rightOffset += columnWidth;
                }

                if ($table.find('.position-col-pc').length) {
                    $table.find('.position-col-pc').css({
                        'right': (rightOffset) + 'px',
                        'min-width': columnWidth + 'px',
                        'background-color': '#f8f9fa',
                        'font-weight': 'bold'
                    });
                }

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