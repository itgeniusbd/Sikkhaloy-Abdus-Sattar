<%@ Page Language="C#" AutoEventWireup="true"MasterPageFile="~/BASIC.Master"  CodeBehind="Cumulative_Result.aspx.cs" Inherits="EDUCATION.COM.Exam.CumulativeResult.CumulativeResultCardt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <!-- Use Google Fonts for better reliability -->
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">

    <!-- Font Awesome - Multiple CDN fallbacks -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v6.5.1/css/all.css" crossorigin="anonymous" />

    <!-- External CSS for English Result Card with cache busting -->
    <link href="../Result/Assets/Cumulative_Result.css?v=<%= DateTime.Now.Ticks %>" rel="stylesheet" type="text/css" />
  
    
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <!-- Progress Bar Loading Overlay -->
    <div id="loadingOverlay" class="loading-overlay">
        <div class="loading-container">
            <div class="loading-title">Loading Result Cards</div>
            <div class="progress-bar-container">
                <div id="progressBar" class="progress-bar animate"></div>
            </div>
            <div id="progressPercentage" class="progress-percentage">0%</div>
            <div id="progressMessage" class="progress-message">Preparing to load results...</div>
            <div id="progressDetails" class="progress-details">Please wait while we fetch the data</div>
            <div class="loading-spinner">
                <div class="spinner"></div>
            </div>
        </div>
    </div>

    <h3 class="NoPrint" id="pageTitle">Cumulative Result Card     
        <a href="Cumulative_Result_old.aspx"><span class="btn-text-full"> ----> Old</span></a>
    </h3>
   
    
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
                        DataSourceID="ExamNameSQl" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID" 
                        OnDataBound="ExamDropDownList_DataBound" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                        SelectCommand="SELECT CumulativeNameID, SchoolID, RegistrationID, EducationYearID, CumulativeResultName, Date FROM Exam_Cumulative_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) ORDER BY Date DESC, CumulativeResultName">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
            <%}%>

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

   

    <!-- Signature Controls with Guardian Section -->
    <div class="form-inline NoPrint Card-space" style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px;">
        <div class="signature-upload-section">
            <!-- Class Teacher Signature -->
            <div class="signature-upload-group">
                <asp:TextBox ID="TeacherSignTextBox" Text="Class Teacher" runat="server" 
                    placeholder="Class Teacher" CssClass="form-control" style="width: 130px;"
                    autocomplete="off"></asp:TextBox>
                <label class="btn btn-secondary btn-sm NoPrint" for="Tfileupload" 
                    style="cursor: pointer; margin: 0;">
                    Browse
                </label>
                <input id="Tfileupload" type="file" accept="image/*" 
                    style="position: absolute; left: -9999px; opacity: 0;" />
            </div>

            <!-- Guardian Signature (NEW - EDITABLE LABEL) -->
            <div class="signature-upload-group">
                <asp:TextBox ID="GuardianSignTextBox" Text="Guardian" runat="server" 
                    placeholder="Guardian/Manager" CssClass="form-control" style="width: 130px;"
                    autocomplete="off"></asp:TextBox>
                <label class="btn btn-secondary btn-sm NoPrint" for="Gfileupload" 
                    style="cursor: pointer; margin: 0;">
                    Browse
                </label>
                <input id="Gfileupload" type="file" accept="image/*" 
                    style="position: absolute; left: -9999px; opacity: 0;" />
            </div>

            <!-- Principal Signature -->
            <div class="signature-upload-group">
                <asp:TextBox ID="HeadTeacherSignTextBox" Text="Principal" runat="server" 
                    placeholder="Principal" CssClass="form-control" style="width: 130px;"
                    autocomplete="off"></asp:TextBox>
                <label class="btn btn-secondary btn-sm" for="Hfileupload" 
                    style="cursor: pointer; margin: 0;">
                    Browse
                </label>
                <input id="Hfileupload" type="file" accept="image/*" 
                    style="position: absolute; left: -9999px; opacity: 0;" />
            </div>

            <div class="pagination-inline NoPrint" style="margin-left: auto;">
                <asp:Label ID="PaginationInfoLabel" runat="server" CssClass="pagination-label" 
                    Text="Loaded 0 to 0 students. Total 0 students"></asp:Label>
            </div>

            <!-- Pagination Controls -->
            <div class="pagination-inline NoPrint" style="display: flex; align-items: center; gap: 2px;">
                <asp:Button ID="FirstPageButton" runat="server" Text="First" CssClass="btn btn-xs btn-outline-primary" 
                    OnClick="FirstPageButton_Click" style="padding: 2px 6px; font-size: 11px;" />
                <asp:Button ID="PrevPageButton" runat="server" Text="Prev" CssClass="btn btn-xs btn-outline-primary" 
                    OnClick="PrevPageButton_Click" style="padding: 2px 6px; font-size: 11px;" />
                <asp:Label ID="PageInfoLabel" runat="server" CssClass="page-info-inline" 
                    Text="Page 1 of 1" style="margin: 0 8px; font-size: 12px; font-weight: bold; color: #495057;"></asp:Label>
                <asp:Button ID="NextPageButton" runat="server" Text="Next" CssClass="btn btn-xs btn-outline-primary" 
                    OnClick="NextPageButton_Click" style="padding: 2px 6px; font-size: 11px;" />
                <asp:Button ID="LastPageButton" runat="server" Text="Last" CssClass="btn btn-xs btn-outline-primary" 
                    OnClick="LastPageButton_Click" style="padding: 2px 6px; font-size: 11px;" />
            </div>
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
                        <p><i class="fa-solid fa-location-dot icon-fallback"></i> <%# Eval("Address") %></p>
                        <p><i class="fa-solid fa-phone icon-fallback"></i> <%# Eval("Phone") %></p>
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

                    <!-- Footer with 3 Signatures (Class Teacher | Guardian | Principal) -->
                    <div class="footer">
                        <div>
                            <div class="SignTeacher signature-container"></div>
                            <div class="Teacher signature-label">Class Teacher</div>
                        </div>
                        <div>
                            <div class="SignGuardian signature-container"></div>
                            <div class="Guardian signature-label">Guardian</div>
                        </div>
                        <div>
                            <div class="SignHead signature-container"></div>
                            <div class="Head signature-label">Principal</div>
                        </div>
                    </div>
                    <p class="note">WD: Working Days.PM: Pass Marks. FM: Full Marks. OM: Obtained Marks. PC: Position in Class. PS: Position in Section.HMC: Highest Marks in Class. HMS: Highest Marks in Section</p>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>

    <script type="text/javascript">
        $(document).ready(function () {
            // Force load Font Awesome if not loaded
            if (!$('link[href*="font-awesome"]').length || !document.fonts || !document.fonts.check('16px "Font Awesome 6 Free"')) {
                console.warn('🔄 Font Awesome not detected, force loading...');
                var faLink = document.createElement('link');
                faLink.rel = 'stylesheet';
                faLink.href = 'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css';
                faLink.crossOrigin = 'anonymous';
                document.head.appendChild(faLink);
                
                setTimeout(checkAndFixFontAwesome, 1000);
            }

            // Initialize everything
            checkAndFixFontAwesome();
            fixAbsentMarksDisplay();
            loadDatabaseSignatures();
            updateSignatureTexts(); // Initial update
            initializeSignatureUpload();
            applyPaginationStyles();

            if ($('.result-card').length > 0) {
                $('#PrintButton').show();
                fixResultCardIcons();
                fixPositionColumnsAlignment();
            }

            // Real-time update when typing in textboxes
            $('[id$="TeacherSignTextBox"], [id$="GuardianSignTextBox"], [id$="HeadTeacherSignTextBox"]').on('keyup change paste input', function() {
                updateSignatureTexts();
            });

            // Handle Enter key in Student ID textbox
            $("[id*=StudentIDTextBox]").keypress(function (e) {
                if (e.which == 13) {
                    e.preventDefault();
                    $("[id*=LoadResultsButton]").click();
                }
            });

            // Clear Student ID textbox when Class dropdown changes
            $("[id*=ClassDropDownList]").change(function () {
                $("[id*=StudentIDTextBox]").val('');
            });

            // Load Results Button Handler
            $("[id*=LoadResultsButton]").off('click').on('click', function (e) {
                console.log('🚀 Load Results button clicked');

                var classValue = $("[id*=ClassDropDownList]").val();
                var examValue = $("[id*=ExamDropDownList]").val();

                if (!classValue || classValue === "0") {
                    alert("Please select a class first!");
                    e.preventDefault();
                    return false;
                }

                if (!examValue || examValue === "0") {
                    alert("Please select an exam first!");
                    e.preventDefault();
                    return false;
                }

                var resultPanel = document.getElementById('<%=ResultPanel.ClientID%>');
                if (resultPanel) {
                    $(resultPanel).hide();
                }
                $('.result-card').remove();

                if (typeof ProgressBarManager !== 'undefined') {
                    ProgressBarManager.show();
                }

                return true;
            });
        });

        function checkAndFixFontAwesome() {
            console.log('✅ Checking Font Awesome icons...');
            
            setTimeout(function() {
                fixResultCardIcons();
            }, 500);
        }

        function fixResultCardIcons() {
            $('.result-card').find('.fa-location-dot, .fa-map-marker-alt, .fa-phone').each(function() {
                var $icon = $(this);
                
                // Check if icon is rendering properly
                var hasContent = $icon.text().trim().length > 0;
                var hasBeforeContent = window.getComputedStyle($icon[0], ':before').content;
                
                if (!hasContent || hasBeforeContent === 'none' || hasBeforeContent === '""') {
                    // Fallback to emoji if icon doesn't load
                    if ($icon.hasClass('fa-location-dot') || $icon.hasClass('fa-map-marker-alt')) {
                        $icon.html('📍 ');
                    } else if ($icon.hasClass('fa-phone')) {
                        $icon.html('📞 ');
                    }
                }
            });
        }

        function fixAbsentMarksDisplay() {
            $('.marks-table').each(function () {
                var $table = $(this);
                $table.find('tr').each(function () {
                    var $row = $(this);
                    if ($row.find('th').length > 0) return;
                    $row.find('td').each(function () {
                        var $cell = $(this);
                        var cellText = $cell.text().trim();
                        if (cellText === 'A' && !$cell.hasClass('grade-cell')) {
                            $cell.text('Abs').addClass('absent-mark');
                        }
                    });
                });
            });
        }

        function loadDatabaseSignatures() {
            var teacherSignPath = $('[id$="HiddenTeacherSign"]').val();
            var principalSignPath = $('[id$="HiddenPrincipalSign"]').val();

            if (teacherSignPath && teacherSignPath.trim() !== '') {
                $('.SignTeacher').html('<img src="' + teacherSignPath + '" style="max-height: 35px; max-width: 120px;">');
            }

            if (principalSignPath && principalSignPath.trim() !== '') {
                $('.SignHead').html('<img src="' + principalSignPath + '" style="max-height: 35px; max-width: 120px;">');
            }
        }

        function updateSignatureTexts() {
            var teacherText = $('[id$="TeacherSignTextBox"]').val() || 'Class Teacher';
            var guardianText = $('[id$="GuardianSignTextBox"]').val() || 'Guardian';
            var principalText = $('[id$="HeadTeacherSignTextBox"]').val() || 'Principal';

            $('.Teacher').text(teacherText);
            $('.Guardian').text(guardianText);
            $('.Head').text(principalText);
            
            console.log('📝 Signature labels updated:', { teacher: teacherText, guardian: guardianText, principal: principalText });
        }

        function initializeSignatureUpload() {
            // Teacher signature
            $('#Tfileupload').off('change').on('change', function (e) {
                handleSignatureUpload(e, '.SignTeacher');
            });

            // Guardian signature (MIDDLE)
            $('#Gfileupload').off('change').on('change', function (e) {
                handleSignatureUpload(e, '.SignGuardian');
            });

            // Principal signature
            $('#Hfileupload').off('change').on('change', function (e) {
                handleSignatureUpload(e, '.SignHead');
            });

            console.log('✅ Signature upload handlers initialized with Guardian support');
        }

        function handleSignatureUpload(event, targetSelector) {
            var file = event.target.files[0];
            if (!file) return;

            if (!file.type.match('image.*')) {
                alert('Please select an image file.');
                return;
            }

            if (file.size > 2 * 1024 * 1024) {
                alert('File size should be less than 2MB.');
                return;
            }

            var reader = new FileReader();
            reader.onload = function (e) {
                var imageData = e.target.result;
                $(targetSelector).html('<img src="' + imageData + '" style="max-height: 35px; max-width: 120px; object-fit: contain;">');
                console.log('✅ Signature uploaded to:', targetSelector);
            };

            reader.readAsDataURL(file);
        }

        function applyPaginationStyles() {
            // Apply black background with white text to pagination buttons
            $('.btn-xs.btn-outline-primary').css({
                'color': '#ffffff',
                'background-color': '#000000',
                'border-color': '#000000'
            });
            
            $('.btn-xs.btn-outline-primary:disabled').css({
                'color': '#6c757d',
                'background-color': '#f8f9fa',
                'border-color': '#dee2e6',
                'opacity': '0.65'
            });
        }

        function fixPositionColumnsAlignment() {
            // Fix position column alignment if needed
            $('.position-col-pc, .position-col-ps, .position-col-hmc, .position-col-hms').css({
                'text-align': 'center'
            });
        }

        // Enhanced pageLoad function for ASP.NET postbacks
        function pageLoad(sender, args) {
            setTimeout(function () {
                updateSignatureTexts(); // Update signature labels after postback
                applyPaginationStyles();
                fixPositionColumnsAlignment();

                if (typeof ProgressBarManager !== 'undefined' && ProgressBarManager.state && ProgressBarManager.state.isRunning) {
                    if ($('.result-card').length > 0) {
                        ProgressBarManager.forceComplete();
                    }
                }
                
                // Fix icons after postback
                fixResultCardIcons();
            }, 100);
        }

        // ProgressBarManager implementation (minimal version)
        var ProgressBarManager = {
            state: { isRunning: false },
            show: function() { 
                $('#loadingOverlay').css('display', 'flex'); 
                this.state.isRunning = true; 
            },
            hide: function() { 
                $('#loadingOverlay').fadeOut(300); 
                this.state.isRunning = false; 
            },
            forceComplete: function() { 
                this.hide(); 
            }
        };
    </script>
</asp:Content>