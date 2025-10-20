<%@ Page Language="C#" AutoEventWireup="true"MasterPageFile="~/BASIC.Master"  CodeBehind="CumulativeResultCardt.aspx.cs" Inherits="EDUCATION.COM.Exam.CumulativeResult.CumulativeResultCardt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <!-- Use Google Fonts for better reliability -->
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">

    <!-- Additional Font Awesome support for this page -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" crossorigin="anonymous" />

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
        <a href="Individual_Result_For_Class_old.aspx"><span class="btn-text-full"> ----> Old</span></a>
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
                        <div>
                            <div class="SignTeacher" style="height: 45px; margin-bottom: 5px;"></div>
                            <div class="Teacher">Class Teacher</div>
                        </div>
                        <div>
                            <div class="SignHead" style="height: 45px; margin-bottom: 5px;"></div>
                            <div class="Head">Principal</div>
                        </div>
                    </div>
                    <p class="note">WD: Working Days.PM: Pass Marks. FM: Full Marks. OM: Obtained Marks. PC: Position in Class. PS: Position in Section.HMC: Highest Marks in Class. HMS: Highest Marks in Section</p>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </asp:Panel>

    <script type="text/javascript">
        // Enhanced Progress Bar Control Object with Dynamic Server Integration
        var ProgressBarManager = {
            // Configuration
            config: {
                polling: {
                    interval: 300, // Check every 300ms
                    maxAttempts: 200, // Maximum 60 seconds (200 * 300ms)
                    currentAttempt: 0
                },
                baseMessages: [
                    { step: 1, message: "Connecting to database...", detail: "Establishing secure connection", duration: 1000 },
                    { step: 2, message: "Validating parameters...", detail: "Checking class, exam and student selections", duration: 800 },
                    { step: 3, message: "Loading class configuration...", detail: "Fetching class, section, group information", duration: 600 },
                    { step: 4, message: "Counting students...", detail: "Determining total number of students", duration: 1200 },
                    { step: 5, message: "Processing student data...", detail: "Loading student information and photos", duration: 0 }, // Dynamic
                    { step: 6, message: "Calculating exam results...", detail: "Processing marks and grades", duration: 0 }, // Dynamic
                    { step: 7, message: "Generating result cards...", detail: "Formatting and preparing display", duration: 0 }, // Dynamic
                    { step: 8, message: "Finalizing...", detail: "Preparing final output", duration: 500 }
                ]
            },

            // State management
            state: {
                isRunning: false,
                currentStep: 0,
                estimatedStudentCount: 0,
                actualStudentCount: 0,
                startTime: 0,
                pollingTimer: null,
                stepTimer: null
            },

            // Show progress overlay
            show: function () {
                $('#loadingOverlay').css('display', 'flex');
                this.state.isRunning = true;
                this.state.currentStep = 0;
                this.state.startTime = Date.now();
                this.state.pollingTimer = null;
                this.state.stepTimer = null;
                this.config.polling.currentAttempt = 0;
                this.reset();
                this.startDynamicProgress();
            },

            // Hide progress overlay
            hide: function () {
                var self = this;
                setTimeout(function () {
                    $('#loadingOverlay').fadeOut(300, function () {
                        self.cleanup();
                    });
                }, 500);
            },

            // Cleanup timers and state
            cleanup: function () {
                this.state.isRunning = false;
                if (this.state.pollingTimer) {
                    clearInterval(this.state.pollingTimer);
                    this.state.pollingTimer = null;
                }
                if (this.state.stepTimer) {
                    clearTimeout(this.state.stepTimer);
                    this.state.stepTimer = null;
                }
            },

            // Reset progress bar
            reset: function () {
                $('#progressBar').css('width', '0%');
                $('#progressPercentage').text('0%');
                $('#progressMessage').text('Preparing to load results...');
                $('#progressDetails').text('Please wait while we fetch the data');
            },

            // Start dynamic progress with server monitoring
            startDynamicProgress: function () {
                var self = this;

                // Start initial steps (before server processing)
                self.processInitialSteps();

                // Start polling for server completion after initial steps
                setTimeout(function () {
                    self.startServerPolling();
                }, 2500); // Start polling after initial 2.5 seconds
            },

            // Process initial steps (database connection, validation, etc.)
            processInitialSteps: function () {
                var self = this;
                var initialSteps = self.config.baseMessages.slice(0, 4); // First 4 steps

                function processStep(stepIndex) {
                    if (stepIndex >= initialSteps.length || !self.state.isRunning) {
                        return;
                    }

                    var step = initialSteps[stepIndex];
                    self.state.currentStep = step.step;

                    $('#progressMessage').text(step.message);
                    $('#progressDetails').text(step.detail);

                    // Calculate progress for initial steps (0-40%)
                    var progress = ((stepIndex + 1) / initialSteps.length) * 40;
                    self.animateProgressTo(progress);

                    // Scheduled
                    if (stepIndex < initialSteps.length - 1) {
                        self.state.stepTimer = setTimeout(function () {
                            processStep(stepIndex + 1);
                        }, step.duration);
                    }
                }

                processStep(0);
            },

            // Start polling server for completion status
            startServerPolling: function () {
                var self = this;

                // Update message to show we're processing
                $('#progressMessage').text('Processing student data...');
                $('#progressDetails').text('Loading and calculating results');

                self.state.pollingTimer = setInterval(function () {
                    self.checkServerStatus();
                }, self.config.polling.interval);
            },

            // Check if results are loaded on server
            checkServerStatus: function () {
                var self = this;
                self.config.polling.currentAttempt++;

                // Calculate elapsed time
                var elapsedTime = Date.now() - self.state.startTime;
                var elapsedSeconds = Math.floor(elapsedTime / 1000);

                // Progressive progress updates while polling
                var baseProgress = 40; // Starting from 40% after initial steps
                var pollingProgress = Math.min(50, (self.config.polling.currentAttempt / self.config.polling.maxAttempts) * 50);
                var currentProgress = baseProgress + pollingProgress;

                // Update progress and details
                self.animateProgressTo(Math.min(currentProgress, 90));
                $('#progressDetails').text(`Processing... (${elapsedSeconds}s elapsed)`);

                // Check if results panel is visible (indicates completion)
                var resultPanel = document.getElementById('<%=ResultPanel.ClientID%>');
                var hasResults = false;

                if (resultPanel) {
                    hasResults = $(resultPanel).is(':visible') && $('.result-card').length > 0;
                }

                if (hasResults) {
                    // Results found - complete immediately
                    self.state.actualStudentCount = $('.result-card').length;
                    self.completeWithResults();
                    return;
                }

                // Check for timeout
                if (self.config.polling.currentAttempt >= self.config.polling.maxAttempts) {
                    // Timeout reached - check one more time then complete
                    setTimeout(function () {
                        var finalCheck = $('.result-card').length > 0;
                        if (finalCheck) {
                            self.state.actualStudentCount = $('.result-card').length;
                            self.completeWithResults();
                        } else {
                            self.completeWithTimeout();
                        }
                    }, 500);

                    if (self.state.pollingTimer) {
                        clearInterval(self.state.pollingTimer);
                        self.state.pollingTimer = null;
                    }
                }
            },

            // Complete progress when results are loaded
            completeWithResults: function () {
                var self = this;

                // Stop polling
                if (self.state.pollingTimer) {
                    clearInterval(self.state.pollingTimer);
                    self.state.pollingTimer = null;
                }

                // Show completion messages
                $('#progressMessage').text('Results loaded successfully!');
                $('#progressDetails').text(`Displaying ${self.state.actualStudentCount} result card(s)`);

                // Animate to 100%
                self.animateProgressTo(100);

                // Hide after showing completion
                setTimeout(function () {
                    self.hide();

                    // Show success notification
                    console.log(`✅ Results loaded: ${self.state.actualStudentCount} students in ${Math.floor((Date.now() - self.state.startTime) / 1000)} seconds`);

                    // Ensure print button is visible
                    setTimeout(function () {
                        var printBtn = document.getElementById('PrintButton');
                        if (printBtn && $('.result-card').length > 0) {
                            printBtn.style.display = 'inline-block';
                        }
                    }, 100);

                }, 800);
            },

            // Handle timeout case
            completeWithTimeout: function () {
                var self = this;

                $('#progressMessage').text('Loading completed');
                $('#progressDetails').text('Processing finished');
                self.animateProgressTo(100);

                setTimeout(function () {
                    self.hide();
                }, 1000);
            },

            // Animate progress bar to target percentage
            animateProgressTo: function (targetPercentage) {
                var $progressBar = $('#progressBar');
                var $progressPercentage = $('#progressPercentage');

                $progressBar.css('width', Math.min(targetPercentage, 100) + '%');
                $progressPercentage.text(Math.round(Math.min(targetPercentage, 100)) + '%');

                // Add pulse effect for active progress
                if (targetPercentage < 95) {
                    $progressBar.addClass('animate');
                } else {
                    $progressBar.removeClass('animate');
                }
            },

            // Force completion (called externally when we know results are ready)
            forceComplete: function () {
                if (!this.state.isRunning) return;

                this.state.actualStudentCount = $('.result-card').length;
                this.completeWithResults();
            },

            // Manual completion with custom message
            completeWithMessage: function (message, detail) {
                var self = this;

                if (self.state.pollingTimer) {
                    clearInterval(self.state.pollingTimer);
                    self.state.pollingTimer = null;
                }

                $('#progressMessage').text(message || 'Loading completed');
                $('#progressDetails').text(detail || 'Process finished');
                self.animateProgressTo(100);

                setTimeout(function () {
                    self.hide();
                }, 800);
            }
        };

        // Enhanced Load Results Button Click Handler with Dynamic Progress Bar
        $(document).ready(function () {
            // Check if Font Awesome is loaded properly
            checkAndFixFontAwesome();

            // Fix absent marks display
            fixAbsentMarksDisplay();

            // Load database signatures when page loads
            loadDatabaseSignatures();

            // Initialize teacher and head teacher text
            updateSignatureTexts();

            // Initialize signature upload functionality
            initializeSignatureUpload();

            // Apply pagination button styles
            applyPaginationStyles();

            // Show toggle button if results are already loaded
            if ($('.result-card').length > 0) {
                $('#PrintButton').show();
                fixResultCardIcons();
                fixPositionColumnsAlignment();
            }

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

            // Enhanced Load Results Button Click Handler with Dynamic Progress Bar
            $("[id*=LoadResultsButton]").off('click').on('click', function (e) {
                console.log('🚀 Load Results button clicked - starting dynamic progress monitoring');

                // Test if progress bar manager exists
                if (typeof ProgressBarManager === 'undefined') {
                    console.error('❌ ProgressBarManager is not defined!');
                    alert('Progress bar system not loaded properly. Please refresh the page.');
                    return false;
                }

                // Test if jQuery is loaded
                if (typeof $ === 'undefined') {
                    console.error('❌ jQuery is not loaded!');
                    alert('jQuery not loaded. Please refresh the page.');
                    return false;
                }

                console.log('✅ Dependencies check passed');

                // Check if required selections are made
                var classValue = $("[id*=ClassDropDownList]").val();
                var examValue = $("[id*=ExamDropDownList]").val();

                console.log('📋 Form values:', { class: classValue, exam: examValue });

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

                // Hide any existing results during new load
                var resultPanel = document.getElementById('<%=ResultPanel.ClientID%>');
                if (resultPanel) {
                    $(resultPanel).hide();
                }
                $('.result-card').remove();

                // Test progress bar show function
                console.log('🎯 About to show progress bar...');

                try {
                    // Show dynamic progress bar
                    ProgressBarManager.show();
                    console.log('✅ Progress bar show() called successfully');
                } catch (error) {
                    console.error('❌ Error showing progress bar:', error);
                    alert('Error starting progress bar: ' + error.message);
                }

                // Add debug logging
                console.log('📊 Progress tracking started with dynamic server monitoring');
                console.log(`📋 Loading results for Class: ${classValue}, Exam: ${examValue}`);

                // Let the postback continue normally
                return true;
            });

            // Add input validation for Student ID textbox
            $("[id*=StudentIDTextBox]").on('input', function () {
                var value = $(this).val();
                var validChars = /^[a-zA-Z0-9,\s]*$/;

                if (!validChars.test(value)) {
                    value = value.replace(/[^a-zA-Z0-9,\s]/g, '');
                    $(this).val(value);
                }
            });

            // Add helpful tooltips and validation feedback
            $("[id*=StudentIDTextBox]").on('blur', function () {
                var value = $(this).val().trim();
                if (value) {
                    var ids = value.split(/[,]/).map(function (id) { return id.trim(); }).filter(function (id) { return id; });

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

        // Enhanced pageLoad function for ASP.NET postbacks
        function pageLoad(sender, args) {
            console.log('📄 Page loaded - checking for results...');

            if (args && args.get_isPartialLoad && args.get_isPartialLoad()) {
                // Partial postback
                setTimeout(function () {
                    applyPaginationStyles();
                    fixPositionColumnsAlignment();

                    // Check if progress bar is running and complete it if results are loaded
                    if (ProgressBarManager.state.isRunning && $('.result-card').length > 0) {
                        console.log('✅ Partial postback completed with results - completing progress bar');
                        ProgressBarManager.forceComplete();
                    }
                }, 100);
            } else {
                // Full postback
                setTimeout(function () {
                    var resultCount = $('.result-card').length;
                    console.log(`📊 Full postback completed - found ${resultCount} result cards`);

                    if (ProgressBarManager && ProgressBarManager.state && ProgressBarManager.state.isRunning) {
                        if (resultCount > 0) {
                            console.log('✅ Full postback completed with results - completing progress bar');
                            ProgressBarManager.forceComplete();
                        } else {
                            console.log('⚠️ Full postback completed but no results found');
                            ProgressBarManager.completeWithMessage('No results found', 'Please check your selections and try again');
                        }
                    }
                }, 500);
            }
        }

        // Function implementations
        function checkAndFixFontAwesome() {
            console.log('Checking Font Awesome icons...');
            var testIcon = $('<i class="fa fa-home"></i>').appendTo('body');
            var iconWidth = testIcon.width();
            testIcon.remove();

            if (iconWidth > 0) {
                console.log('Font Awesome loaded successfully');
                fixResultCardIcons();
            } else {
                console.warn('Font Awesome not loaded properly, using fallback');
                if (!$('link[href*="font-awesome"]').length) {
                    $('<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">').appendTo('head');
                    setTimeout(fixResultCardIcons, 500);
                }
            }
        }

        function fixResultCardIcons() {
            $('.result-card').each(function () {
                var $card = $(this);
                $card.find('.fa-map-marker, .fa-map-marker-alt').each(function () {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '📍');
                    }
                });
                $card.find('.fa-phone').each(function () {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '📞');
                    }
                });
                $card.find('.fa-envelope, .fa-envelope-o').each(function () {
                    if ($(this).text().trim() === '' || $(this).is(':empty')) {
                        $(this).attr('data-fallback', '✉️');
                    }
                });
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

        function updateSignatureTexts() {
            var teacherText = $('[id$="TeacherSignTextBox"]').val() || 'Class Teacher';
            var principalText = $('[id$="HeadTeacherSignTextBox"]').val() || 'Principal';

            $('.Teacher').text(teacherText);
            $('.Head').text(principalText);
        }

        function initializeSignatureUpload() {
            console.log('Initializing signature upload functionality...');

            $('#Tfileupload').off('change').on('change', function (e) {
                console.log('Teacher file input changed');
                handleSignatureUpload(e, 'teacher', '.SignTeacher');
            });

            $('#Hfileupload').off('change').on('change', function (e) {
                console.log('Principal file input changed');
                handleSignatureUpload(e, 'principal', '.SignHead');
            });

            $('[id$="TeacherSignTextBox"]').off('input').on('input', function () {
                var text = $(this).val() || 'Class Teacher';
                $('.Teacher').text(text);
            });

            $('[id$="HeadTeacherSignTextBox"]').off('input').on('input', function () {
                var text = $(this).val() || 'Principal';
                $('.Head').text(text);
            });
        }

        function handleSignatureUpload(event, signatureType, targetSelector) {
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
                $(targetSelector).html('<img src="' + imageData + '" style="max-height: 35px; max-width: 120px;">');

                var base64Data = imageData.split(',')[1];

                $.ajax({
                    type: 'POST',
                    url: 'CumulativeResultCardt.aspx/SaveSignature',
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

        function fixPositionColumnsAlignment() {
            console.log('Fixing position columns alignment...');

            $('.marks-table').each(function () {
                var $table = $(this);
                var $headerRows = $table.find('tr').slice(0, 2);
                var headerTexts = [];
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

                if (pcIndex === -1 && hmcIndex === -1) {
                    var totalCells = $titleHeader.find('th').length;
                    var hasSection = $table.find('th').filter(function () {
                        return $(this).text().trim().toUpperCase() === 'PS' || $(this).text().trim().toUpperCase() === 'HMS';
                    }).length > 0;

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

                $table.find('tr').each(function () {
                    var $row = $(this);
                    var $cells = $row.find('th, td');
                    $cells.removeClass('position-col-pc position-col-ps position-col-hmc position-col-hms');

                    if (pcIndex >= 0 && $cells.eq(pcIndex).length) $cells.eq(pcIndex).addClass('position-col-pc');
                    if (psIndex >= 0 && $cells.eq(psIndex).length) $cells.eq(psIndex).addClass('position-col-ps');
                    if (hmcIndex >= 0 && $cells.eq(hmcIndex).length) $cells.eq(hmcIndex).addClass('position-col-hmc');
                    if (hmsIndex >= 0 && $cells.eq(hmsIndex).length) $cells.eq(hmsIndex).addClass('position-col-hms');
                });

                var totalCells = $titleHeader.find('th').length;
                var columnWidth = Math.max(28, Math.min(40, Math.floor(100 / totalCells)));
                var rightOffset = 0;

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
    </script>
</asp:Content>