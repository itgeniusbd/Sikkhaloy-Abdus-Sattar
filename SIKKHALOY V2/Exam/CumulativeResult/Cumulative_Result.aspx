<%@ Page Language="C#" AutoEventWireup="true"MasterPageFile="~/BASIC.Master"  CodeBehind="Cumulative_Result.aspx.cs" Inherits="EDUCATION.COM.Exam.CumulativeResult.CumulativeResultCardt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <!-- jQuery - REQUIRED for Progress Bar -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

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

             // Schedule next step
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
       $('#progressDetails').text('Processing... (' + elapsedSeconds + 's elapsed)');

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
   $('#progressDetails').text('Displaying ' + self.state.actualStudentCount + ' result card(s)');

      // Animate to 100%
self.animateProgressTo(100);

      // Hide after showing completion
                setTimeout(function () {
     self.hide();

     // Show success notification
                    console.log('✅ Results loaded: ' + self.state.actualStudentCount + ' students in ' + Math.floor((Date.now() - self.state.startTime) / 1000) + ' seconds');

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

        // Helper Functions
        function convertBengaliToEnglishJS(bengaliText) {
   if (!bengaliText) return bengaliText;
            
   var bengaliToEnglish = {
            '०': '0', '१': '1', '२': '2', '३': '3', '४': '4',
       '५': '5', '६': '6', '७': '7', '८': '8', '९': '9'
    };
            
    var result = '';
   for (var i = 0; i < bengaliText.length; i++) {
      var char = bengaliText[i];
     result += bengaliToEnglish[char] || char;
            }
          return result;
        }

      function applyPaginationStyles() {
     // Apply styles to pagination buttons
            console.log('Applying pagination styles...');
     }

        function fixPositionColumnsAlignment() {
            // Fix position column alignment
            console.log('Fixing position columns alignment...');
        }

        function convertNumbersAfterPostback() {
      // Convert numbers after postback
            console.log('Converting numbers after postback...');
        }

        function forceFontAwesomeLoad() {
     console.log('Forcing Font Awesome load...');
  
    // Test if Font Awesome is loaded
            var testElement = $('<i class="fa fa-home"></i>').appendTo('body').hide();
            var computed = window.getComputedStyle(testElement[0], ':before');
     var content = computed.getPropertyValue('content');
            testElement.remove();
       
    if (!content || content === 'none' || content === '""') {
          console.warn('Font Awesome not loaded properly, reloading...');
  
     // Remove existing Font Awesome links
             $('link[href*="font-awesome"]').remove();
    
    // Add Font Awesome 6 with integrity check
  $('<link>')
            .attr('rel', 'stylesheet')
     .attr('href', 'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css')
         .attr('crossorigin', 'anonymous')
   .attr('referrerpolicy', 'no-referrer')
          .appendTo('head');
        
          setTimeout(function() {
               fixResultCardIcons();
       }, 500);
            } else {
      console.log('Font Awesome loaded successfully');
      fixResultCardIcons();
  }
    }

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
            $('<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css">').appendTo('head');
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
                
                // Determine signature type from selector
                var signatureType = '';
                if (targetSelector === '.SignTeacher') {
                    signatureType = 'teacher';
                } else if (targetSelector === '.SignGuardian') {
                    signatureType = 'guardian';
                } else if (targetSelector === '.SignHead') {
                    signatureType = 'principal';
                }
                
                // Save signature to database
                if (signatureType) {
                    saveSignatureToDatabase(signatureType, imageData);
                }
            };

            reader.readAsDataURL(file);
        }

        function saveSignatureToDatabase(signatureType, imageData) {
            // Remove data:image/...;base64, prefix
            var base64Data = imageData.split(',')[1] || imageData;
            
            $.ajax({
                type: "POST",
                url: "<%= ResolveUrl("~/Exam/CumulativeResult/Cumulative_Result.aspx/SaveSignature") %>",
                data: JSON.stringify({
                    signatureType: signatureType,
                    imageData: base64Data
                }),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    var result = response.d || response;
                    if (result.success) {
                        console.log('✅ ' + signatureType + ' signature saved successfully');
                        alert(result.message || signatureType.charAt(0).toUpperCase() + signatureType.slice(1) + ' signature saved successfully!');
                    } else {
                        console.error('❌ Error saving signature:', result.message);
                        alert('Error: ' + result.message);
                    }
                },
                error: function (xhr, status, error) {
                    console.error('❌ AJAX Error saving signature:', error);
                    console.error('Response:', xhr.responseText);
                    alert('Error saving signature: ' + error);
                }
            });
        }

        // Document Ready
        $(document).ready(function () {
            console.log('✅ Document ready - initializing...');

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

            // Function to check when results are loaded (ASP.NET postback)
            function checkResultsLoaded() {
                var resultPanel = document.getElementById('<%=ResultPanel.ClientID%>');
                if (resultPanel && $(resultPanel).is(':visible') && $('.result-card').length > 0) {
                    console.log('✅ Results detected, completing progress bar');
                    if (typeof ProgressBarManager !== 'undefined') {
                        ProgressBarManager.forceComplete();
                    }
                    return true;
                }
                return false;
            }

            // Call checkResultsLoaded periodically to detect when results are ready
            var checkInterval = setInterval(function () {
                if (checkResultsLoaded()) {
                    clearInterval(checkInterval);
                }
            }, 500);

            // Also check on page visibility changes (for tab switching)
            $(document).on('visibilitychange', function () {
                if (!document.hidden && !checkResultsLoaded()) {
                    // Page became visible, check again
                }
            });
        });

    </script>
</asp:Content>