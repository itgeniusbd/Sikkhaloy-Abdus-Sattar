<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="BanglaResult.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Bangla_Result_DirectPrint" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Font Awesome CDN - Latest Version with Fallback -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" 
          integrity="sha512-iecdLmaskl7CVkqkXNQ/ZH/XLlvWZOJyj7Yy7tcenmpD1ypASozpmT/E0iPtmFIB46ZmdtAc9eNBvH0H/ZpiBw==" 
          crossorigin="anonymous" referrerpolicy="no-referrer" />
    <!-- Fallback to Font Awesome 4.7 -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" 
          crossorigin="anonymous" />
    
    <!-- Use Google Fonts for better reliability -->
    <link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Bengali:wght@400;700&display=swap" rel="stylesheet">
    
    <!-- External CSS for Bangla Result Direct Print -->
    <link href="Assets/bangla-result-directprint.css" rel="stylesheet" type="text/css" />

    <style>
        /* Font Awesome icon fallback fix */
        .fa, .fas, .far, .fal, .fab {
            font-family: "Font Awesome 6 Free", "Font Awesome 5 Free", FontAwesome !important;
            font-weight: 900;
        }
        
        /* Ensure icons are visible */
        .btn i.fa, .btn i.fas {
            margin-right: 5px;
            display: inline-block;
            font-style: normal;
        }
        
        /* Signature display fix */
        .SignTeacher, .SignHead {
            min-height: 40px !important;
            display: flex !important;
            align-items: flex-end !important;
            justify-content: center !important;
            visibility: visible !important;
        }
        
        .SignTeacher img, .SignHead img {
            max-height: 35px !important;
            max-width: 80px !important;
            object-fit: contain !important;
            display: block !important;
            visibility: visible !important;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <!-- Progress Bar Loading Overlay -->
    <div id="loadingOverlay" class="loading-overlay">
        <div class="loading-container">
            <div class="loading-title">বাংলা রেজাল্ট লোড হচ্ছে</div>
            <div class="progress-bar-container">
                <div id="progressBar" class="progress-bar animate"></div>
            </div>
            <div id="progressPercentage" class="progress-percentage">০%</div>
            <div id="progressMessage" class="progress-message">রেজাল্ট প্রস্তুত করা হচ্ছে...</div>
            <div id="progressDetails" class="progress-details">অনুগ্রহ করে অপেক্ষা করুন</div>
            <div class="loading-spinner">
                <div class="spinner"></div>
            </div>
        </div>
    </div>

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
                        <button type="button" onclick="printResults()" class="btn btn-primary btn-sm" 
                            id="PrintButton" style="display:none; flex: 1; height: 34px;">
                            <i class="fa fa-print"></i> PRINT
                        </button>
                        <button type="button" onclick="toggleNumberLanguage()" class="btn btn-warning btn-sm" 
                            id="NumberToggleButton" style="display:none; flex: 1; height: 34px; margin-left: 5px;">
                            <i class="fa fa-language"></i> বাংলা সংখ্যা
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>

   

    <!-- Teacher and Head Teacher Signature Controls with Pagination -->
    <div class="form-inline NoPrint Card-space" style="margin-bottom: 15px; padding: 10px; background: #f8f9fa; border-radius: 5px; display: flex; align-items: center; justify-content: space-between;">
        <div style="display: flex; align-items: center;">
            <!-- Date Picker for Result Date -->
            <div class="form-group NoPrint" style="margin-right: 15px;">
                <label style="margin-right: 5px; font-weight: bold;">তিরিখ নির্বাচন:</label>
                <input type="date" id="ResultDatePicker" class="form-control" style="width: 150px;" />
            </div>

            <!-- Class Teacher Signature -->
            <div class="form-group NoPrint" style="margin-right: 15px;">
                <asp:TextBox ID="TeacherSignTextBox" Text="শ্রেনি শিক্ষক" runat="server" placeholder="শ্রেষ্ঠ শিক্ষকের স্বাক্ষর" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                <label class="btn btn-secondary btn-sm NoPrint" for="Tfileupload" style="margin-left: 5px; margin-top: 5px; cursor: pointer;">
                    Browse
                </label>
                <input id="Tfileupload" type="file" accept="image/*" style="position: absolute; left: -9999px; opacity: 0;" />
            </div>
            
            <!-- ✅ Guardian Signature (Client-side Only) -->
            <div class="form-group NoPrint" style="margin-right: 15px;">
                <input type="text" id="GuardianSignTextBox" value="অভিভাবক" placeholder="অভিভাবক নাম" class="form-control" autocomplete="off" style="width: 150px;" />
                <label class="btn btn-secondary btn-sm NoPrint" for="Gfileupload" style="margin-left: 5px; margin-top: 5px; cursor: pointer;">
                    Browse
                </label>
                <input id="Gfileupload" type="file" accept="image/*" style="position: absolute; left: -9999px; opacity: 0;" />
            </div>
            
            <!-- Principal Signature -->
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
                Text="লোড হয়েছে 0 থেকে 0 জন। মোট 0 শিক্ষার্থী থেকে"></asp:Label>
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
                    <!-- Dynamic Header Section -->
                    <!-- School Name Logo Display (Full Width - Center Aligned) -->
                    <asp:Panel ID="SchoolNameLogoHeaderPanel" runat="server" CssClass="hide-panel" style="display: none;">
                        <div class="school-name-logo-header">
                            <!-- Date Display on Left -->
                            <div class="result-date-display">
                               <span style="margin-bottom:10px;color:#0072bc"> ফলাফল প্রকাশের তারিখ</span>
                                <span class="date-text-display"></span>
                            </div>

                            <img id="SchoolNameLogoImage" runat="server" 
                                 alt="School Name" 
                                 class="school-name-logo-img"
                                 onerror="this.style.display='none';" />
                            <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" 
                                 alt="Student Photo" 
                                 class="student-photo-logo" 
                                 onerror="this.style.display='none';" />
                        </div>
                    </asp:Panel>

                    <!-- Traditional Header Display (When No School Name Logo) -->
                    <asp:Panel ID="TraditionalHeaderPanel" runat="server" CssClass="show-panel">
                        <div class="header">
                            <img src="/Handeler/SchoolLogo.ashx?SLogo=<%# Eval("SchoolID") %>" alt="School Logo" onerror="this.style.display='none';" />
                            <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" alt="Student Photo" class="student-photo" onerror="this.style.display='none';" />
                            <h2><%# Eval("SchoolName") %></h2>
                            <p><%# Eval("Address") %></p>
                            <p>Phone: <%# Eval("Phone") %> </p>
                        </div>
                    </asp:Panel>

                    <!-- Title Section -->
                    <div >
                       <p class="Exam_name">নম্বর পত্র</p>
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
                                    <td>ফলাফল</td>
                                </tr>
                                <tr class="summary-values">
                                    <td><%# Eval("TotalExamObtainedMark_ofStudent") %>/<%# Eval("TotalMark_ofStudent") %></td>
                                    <td><%# Eval("ObtainedPercentage_ofStudent") == DBNull.Value ? "0.00" : String.Format("{0:F2}", Eval("ObtainedPercentage_ofStudent")) %></td>
                                    <td><%# Eval("Average") == DBNull.Value ? "0.00" : String.Format("{0:F2}", Eval("Average")) %></td>
                                    <td><%# Eval("Student_Grade") == DBNull.Value ? "N/A" : Eval("Student_Grade") %></td>
                                    <td><%# Eval("Student_Point") == DBNull.Value ? "0.0" : String.Format("{0:F1}", Eval("Student_Point")) %></td>
                                    <td><%# Eval("Position_InExam_Class") == DBNull.Value ? "N/A" : Eval("Position_InExam_Class") %></td>
                                    <%# GetSectionColumnData(Container.DataItem) %>
                                    <%# GetResultStatusColumn(Container.DataItem) %>
                                </tr>
                            </table>
                        </div>

                        <!-- Right: Grade Chart -->
                        <div class="grade-chart">
                            <table>
                                <tr><th>মার্ক</th><th>গ্রেড</th><th>পয়েন্ট</th></tr>
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

                        <!-- ✅ Guardian Signature Display -->
                        <div style="text-align: center;">
                            <div class="SignGuardian" style="height: 40px; margin-bottom: 5px;"></div>
                            <div class="GuardianText" style="border-top: 1px solid #333; padding-top: 5px; font-weight: bold;">অভিভাবক</div>
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
                    { step: 1, message: "ডেটাবেসে সংযুক্ত হচ্ছে...", detail: "নিরাপদ ডেটাবেস সংযোগ স্থাপন করা হচ্ছে", duration: 1000 },
                    { step: 2, message: "প্যারামিটার যাচাই...", detail: "ক্লাস, পরীক্ষা এবং শিক্ষার্থী নির্বাচন পরীক্ষা করা হচ্ছে", duration: 800 },
                    { step: 3, message: "ক্লাস কনফিগারেশন লোড হচ্ছে...", detail: "ক্লাস, শাখা, গ্রূপের তথ্য সংগ্রহ করা হচ্ছে", duration: 600 },
                    { step: 4, message: "শিক্ষার্থী সংখ্যা গণনা হচ্ছে...", detail: "মোট শিক্ষার্থী সংখ্যা নির্ণয় করা হচ্ছে", duration: 1200 },
                    { step: 5, message: "শিক্ষার্থীর তথ্য প্রক্রিয়াকরণ...", detail: "শিক্ষার্থী তথ্য এবং ছবি লোড করা হচ্ছে", duration: 0 }, // Dynamic
                    { step: 6, message: "পরীক্ষার ফলাফল গণনা হচ্ছে...", detail: "নম্বর এবং গ্রেড প্রক্রিয়াকরণ", duration: 0 }, // Dynamic
                    { step: 7, message: "রেজাল্ট কার্ড তৈরি হচ্ছে...", detail: "ফরম্যাট এবং প্রদর্শনের প্রস্তুতি নেওয়া হচ্ছে", duration: 0 }, // Dynamic
                    { step: 8, message: "চূড়ান্তকরণ...", detail: "চূড়ান্ত আউটপুট প্রস্তুত করা হচ্ছে", duration: 500 }
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
                $('#progressPercentage').text('০%');
                $('#progressMessage').text('রেজাল্ট প্রস্তুত করা হচ্ছে...');
                $('#progressDetails').text('অনুগ্রহ করে অপেক্ষা করুন');
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
                $('#progressMessage').text('শিক্ষার্থীর তথ্য প্রক্রিয়াকরণ...');
                $('#progressDetails').text('রেজাল্ট লোড এবং গণনা করা হচ্ছে');

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
                $('#progressDetails').text(`প্রক্রিয়াকরণ... (${elapsedSeconds}s অতিবাহিত)`);

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
                $('#progressMessage').text('রেজাল্ট সফলভাবে লোড হয়েছে!');
                $('#progressDetails').text(`${self.state.actualStudentCount} টি রেজাল্ট কার্ড প্রদর্শিত হচ্ছে`);

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
                        var numberToggleBtn = document.getElementById('NumberToggleButton');
                        if (printBtn && $('.result-card').length > 0) {
                            printBtn.style.display = 'inline-block';
                        }
                        if (numberToggleBtn && $('.result-card').length > 0) {
                            numberToggleBtn.style.display = 'inline-block';
                        }
                    }, 100);

                }, 800);
            },

            // Handle timeout case
            completeWithTimeout: function () {
                var self = this;

                $('#progressMessage').text('লোডিং সম্পন্ন');
                $('#progressDetails').text('প্রক্রিয়াকরণ শেষ');
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

                // Convert percentage to Bengali numbers
                var bengaliPercentage = this.convertToBengaliNumber(Math.round(Math.min(targetPercentage, 100))) + '%';
                $progressPercentage.text(bengaliPercentage);

                // Add pulse effect for active progress
                if (targetPercentage < 95) {
                    $progressBar.addClass('animate');
                } else {
                    $progressBar.removeClass('animate');
                }
            },

            // Convert English numbers to Bengali
            convertToBengaliNumber: function (number) {
                var englishToBengali = {
                    '0': '০', '1': '১', '২': '২', '3': '৩', '৪': '৪',
                    '5': '৫', '6': '৬', '7': '৭', '৮': '৮', '৯': '৯'
                };

                return number.toString().replace(/[0-9]/g, function (match) {
                    return englishToBengali[match] || match;
                });
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

                $('#progressMessage').text(message || 'লোডিং সম্পন্ন');
                $('#progressDetails').text(detail || 'প্রক্রিয়া শেষ');
                self.animateProgressTo(100);

                setTimeout(function () {
                    self.hide();
                }, 800);
            }
        };

        // Date Display Management Functions
        function initializeDatePicker() {
            try {
                var datePicker = document.getElementById('ResultDatePicker');
                if (!datePicker) {
                    console.warn('Date picker not found');
                    return;
                }

                // Set default to today's date if empty
                if (!datePicker.value) {
                    var today = new Date();
                    var dateString = today.getFullYear() + '-' +
                        String(today.getMonth() + 1).padStart(2, '0') + '-' +
                        String(today.getDate()).padStart(2, '0');
                    datePicker.value = dateString;
                }

                // Add change event listener
                $(datePicker).off('change').on('change', function () {
                    console.log('Date picker changed to:', this.value);
                    updateResultDate();
                });

                console.log('Date picker initialized with value:', datePicker.value);
            } catch (error) {
                console.error('Error initializing date picker:', error);
            }
        }

        function updateResultDate() {
            try {
                var datePicker = document.getElementById('ResultDatePicker');
                if (!datePicker || !datePicker.value) {
                    console.warn('Date picker not available or has no value');
                    return;
                }

                // Parse the date
                var dateValue = new Date(datePicker.value);
                if (isNaN(dateValue.getTime())) {
                    console.warn('Invalid date value');
                    return;
                }

                // Format date in Bengali
                var bengaliDate = formatDateInBengali(dateValue);

                console.log('Updating date displays with:', bengaliDate);

                // Update all date display elements
                var dateDisplays = document.querySelectorAll('.date-text-display');
                console.log('Found date display elements:', dateDisplays.length);

                dateDisplays.forEach(function (element, index) {
                    element.textContent = bengaliDate;
                    console.log('Updated date display', index, ':', element.textContent);

                    // Check if this date display is in School Name Logo header or Footer
                    var schoolNameLogoParent = element.closest('.result-date-display');
                    var footerDateParent = element.closest('.footer-date-display');

                    if (schoolNameLogoParent) {
                        // This is for School Name Logo header
                        schoolNameLogoParent.style.display = 'block';
                        schoolNameLogoParent.style.visibility = 'visible';
                        schoolNameLogoParent.style.opacity = '1';
                    }

                    if (footerDateParent) {
                        // This is for Traditional Header (in footer)
                        footerDateParent.style.display = 'block';
                        footerDateParent.style.visibility = 'visible';
                        footerDateParent.style.opacity = '1';
                    }
                });

                // Additional jQuery approach for safety
                $('.date-text-display').text(bengaliDate);

                // Show/hide date displays based on which header is visible
                $('.result-card').each(function () {
                    var card = $(this);
                    var schoolNameLogoPanel = card.find('#SchoolNameLogoHeaderPanel, [id*="SchoolNameLogoHeaderPanel"]');
                    var traditionalHeaderPanel = card.find('#TraditionalHeaderPanel, [id*="TraditionalHeaderPanel"]');
                    var footerDateDisplay = card.find('.footer-date-display');
                    var schoolNameLogoDateDisplay = card.find('.result-date-display');

                    // Check which panel is visible
                    var isSchoolNameLogoVisible = schoolNameLogoPanel.length > 0 &&
                        (schoolNameLogoPanel.hasClass('show-panel') ||
                            schoolNameLogoPanel.css('display') !== 'none');

                    console.log('School Name Logo visible:', isSchoolNameLogoVisible);

                    if (isSchoolNameLogoVisible) {
                        // Show date in School Name Logo header, hide in footer
                        schoolNameLogoDateDisplay.css({
                            'display': 'block',
                            'visibility': 'visible',
                            'opacity': '1'
                        });
                        footerDateDisplay.css({
                            'display': 'none'
                        });
                    } else {
                        // Show date in footer for traditional header, hide in School Name Logo
                        footerDateDisplay.css({
                            'display': 'block',
                            'visibility': 'visible',
                            'opacity': '1'
                        });
                        schoolNameLogoDateDisplay.css({
                            'display': 'none'
                        });
                    }
                });

                console.log('Date update completed');
            } catch (error) {
                console.error('Error updating result date:', error);
            }
        }

        function formatDateInBengali(date) {
            try {
                var day = date.getDate();
                var month = date.getMonth() + 1;
                var year = date.getFullYear();

                // Bengali month names
                var bengaliMonths = [
                    'জানুয়ারী', 'ফেব্রুয়ারী', 'মার্চ', 'এপ্রি্েল', 'মে', 'জুন',
                    'জুলাই', 'আগস্ট', 'সেপ্টেম্বর', 'অক্টোবর', 'নভেম্বর', 'ডিসেম্বর'
                ];

                // Convert to Bengali numbers
                var bengaliDay = convertToBengaliNumber(day);
                var bengaliYear = convertToBengaliNumber(year);
                var bengaliMonth = bengaliMonths[month - 1];

                return bengaliDay + ' ' + bengaliMonth + ', ' + bengaliYear;
            } catch (error) {
                console.error('Error formatting date in Bengali:', error);
                return '';
            }
        }

        function convertToBengaliNumber(number) {
            var bengaliDigits = ['০', '১', '২', '৩', '৪', '৫', '৬', '৭', '৮', '৯'];
            return String(number).split('').map(function (digit) {
                return bengaliDigits[parseInt(digit)] || digit;
            }).join('');
        }

        function convertBengaliToEnglishJS(bengaliText) {
            if (!bengaliText) return bengaliText;

            var bengaliToEnglish = {
                '০': '0', '১': '1', '২': '2', '৩': '3', '৪': '4',
                '৫': '5', '৬': '6', '৭': '7', '৮': '8', '৯': '9'
            };

            return bengaliText.split('').map(function (char) {
                return bengaliToEnglish[char] || char;
            }).join('');
        }

        // Helper functions for signature and other features
        function fixAbsentMarksDisplay() {
            // Fix display of absent marks
            $('.marks-table td').each(function () {
                var text = $(this).text().trim();
                if (text === 'A' || text === '0') {
                    // Already handled in server-side code
                }
            });
        }

        function updateSignatureTexts() {
            // Update signature text displays
            var teacherText = $("[id*=TeacherSignTextBox]").val() || 'শ্রেণি শিক্ষক';
            var guardianText = $('#GuardianSignTextBox').val() || 'অভিভাবক';
            var headText = $("[id*=HeadTeacherSignTextBox]").val() || 'প্রধান শিক্ষক';

            $('.Teacher').text(teacherText);
            $('.GuardianText').text(guardianText);
            $('.Head').text(headText);
        }

        function initializeSignatureUpload() {
            console.log('Initializing signature upload functionality');

            // Teacher signature upload (with database)
            $('#Tfileupload').off('change').on('change', function (e) {
                console.log('Teacher file selected');
                handleSignatureUpload(e, 'teacher');
            });

            // ✅ Guardian signature upload (client-side only - NO DATABASE)
            $('#Gfileupload').off('change').on('change', function (e) {
                console.log('Guardian file selected (client-side only)');
                handleGuardianSignatureUpload(e);
            });

            // Principal signature upload (with database)
            $('#Hfileupload').off('change').on('change', function (e) {
                console.log('Principal file selected');
                handleSignatureUpload(e, 'principal');
            });

            // Teacher text change event
            $("[id*=TeacherSignTextBox]").off('input').on('input', function () {
                var text = $(this).val() || 'শ্রেণি শিক্ষক';
                $('.Teacher').text(text);
            });

            // ✅ Guardian text change event (client-side only)
            $('#GuardianSignTextBox').off('input').on('input', function () {
                var text = $(this).val() || 'অভিভাবক';
                $('.GuardianText').text(text);
            });

            // Head teacher text change event
            $("[id*=HeadTeacherSignTextBox]").off('input').on('input', function () {
                var text = $(this).val() || 'প্রধান শিক্ষক';
                $('.Head').text(text);
            });
        }

        // ✅ NEW: Guardian signature upload handler (CLIENT-SIDE ONLY - NO DATABASE)
        function handleGuardianSignatureUpload(event) {
            var file = event.target.files[0];
            if (!file) return;

            console.log('📸 Guardian signature upload (client-side only):', { 
                fileName: file.name, 
                fileSize: file.size 
            });

            // Validate file type
            if (!file.type.match('image.*')) {
                alert('অনুগ্রহ করে একটি ছবি ফাইল নির্বাচন করুন।');
                return;
            }

            // Validate file size
            if (file.size > 2 * 1024 * 1024) {
                alert('ফাইলের আকার ২MB এর কম হতে হবে।');
                return;
            }

            var reader = new FileReader();
            reader.onload = function (e) {
                var imageData = e.target.result;

                // ✅ Apply to ALL result cards (client-side only)
                $('.SignGuardian').html('<img src="' + imageData + '" style="height:35px;width:80px;object-fit:contain;">');
                
                console.log('✅ অভিভাবকের স্বাক্ষর সব রেজাল্ট কার্ডে যুক্ত হয়েছে (শুধুমাত্র ক্লায়েন্ট-সাইড)');
                
                // Show success notification
                showBanglaNotification('অভিভাবকের স্বাক্ষর আপলোড সফল! (শুধু ব্রাউজারে - ডাটাবেসে সংরক্ষিত নয়)', 'success');
            };

            reader.onerror = function (error) {
                console.error('FileReader error:', error);
                alert('ফাইল পড়তে সমস্যা হয়েছে। আবার চেষ্টা করুন।');
            };

            reader.readAsDataURL(file);
        }

        function loadDatabaseSignatures() {
            console.log('🔄 Loading database signatures...');
            try {
                var teacherPath = $("[id$='HiddenTeacherSign']").val();
                var principalPath = $("[id$='HiddenPrincipalSign']").val();

                console.log('Teacher path:', teacherPath);
                console.log('Principal path:', principalPath);

                if (teacherPath && teacherPath.trim() !== '') {
                    var teacherImg = '<img src="' + teacherPath + '" style="height:35px;max-width:80px;object-fit:contain;display:block;margin:0 auto;" onerror="console.error(\'Failed to load teacher signature\');">';
                    $('.SignTeacher').html(teacherImg);
                    console.log('✅ Teacher signature loaded');
                } else {
                    console.warn('⚠️ No teacher signature path found');
                }

                if (principalPath && principalPath.trim() !== '') {
                    var principalImg = '<img src="' + principalPath + '" style="height:35px;max-width:80px;object-fit:contain;display:block;margin:0 auto;" onerror="console.error(\'Failed to load principal signature\');">';
                    $('.SignHead').html(principalImg);
                    console.log('✅ Principal signature loaded');
                } else {
                    console.warn('⚠️ No principal signature path found');
                }

                setTimeout(function () {
                    console.log('📊 Final check - Teacher:', $('.SignTeacher img').length, 'Principal:', $('.SignHead img').length);
                }, 1000);
            } catch (error) {
                console.error('❌ Error loading signatures:', error);
            }
        }

        // ✅ ADD: handleSignatureUpload function for Teacher and Principal signatures with database save
        function handleSignatureUpload(event, signatureType) {
            var file = event.target.files[0];
            if (!file) return;

            console.log('📤 Uploading signature:', signatureType);

            if (!file.type.match('image.*')) {
                alert('অনুগ্রহ করে একটি ছবি ফাইল নির্বাচন করুন।');
                return;
            }

            if (file.size > 2 * 1024 * 1024) {
                alert('ফাইলের আকার ২MB এর কম হতে হবে।');
                return;
            }

            var reader = new FileReader();
            reader.onload = function (e) {
                var imageData = e.target.result;
                var targetClass = signatureType === 'teacher' ? '.SignTeacher' : '.SignHead';

                // Show preview immediately
                $(targetClass).html('<img src="' + imageData + '" style="height:35px;max-width:80px;object-fit:contain;display:block;margin:0 auto;">');
                console.log('✅ Preview set for:', signatureType);

                var base64Data = imageData.split(',')[1];

                // Upload to database
                $.ajax({
                    type: 'POST',
                    url: window.location.pathname + '/SaveSignature',
                    data: JSON.stringify({
                        signatureType: signatureType,
                        imageData: base64Data
                    }),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    success: function (response) {
                        console.log('Server response:', response);

                        if (response && response.d && response.d.success) {
                            console.log('✅ Signature saved successfully');
                            
                            var successMsg = signatureType === 'teacher' ?
                                'শ্রেণি শিক্ষকের স্বাক্ষর সফলভাবে সংরক্ষিত হয়েছে!' :
                                'প্রধান শিক্ষকের স্বাক্ষর সফলভাবে সংরক্ষিত হয়েছে!';
                            
                            showBanglaNotification(successMsg, 'success');

                            // Reload signature from database
                            setTimeout(function () {
                                loadDatabaseSignatures();
                            }, 500);
                        } else {
                            console.error('❌ Signature save failed:', response.d ? response.d.message : 'Unknown error');
                            alert('স্বাক্ষর সংরক্ষণে ত্রুটি: ' + (response.d ? response.d.message : 'Unknown error'));
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('❌ AJAX Error:', error);
                        console.error('Response:', xhr.responseText);
                        alert('স্বাক্ষর সংরক্ষণে ত্রুটি হয়েছে।');
                    }
                });
            };

            reader.onerror = function (error) {
                console.error('FileReader error:', error);
                alert('ফাইল পড়তে সমস্যা হয়েছে। আবার চেষ্টা করুন।');
            };

            reader.readAsDataURL(file);
        }

        function showBanglaNotification(message, type) {
            var bgColor = type === 'success' ? '#4CAF50' : '#f44336';
            var notification = $('<div>')
                .css({
                    'position': 'fixed',
                    'top': '20px',
                    'right': '20px',
                    'background': bgColor,
                    'color': 'white',
                    'padding': '15px 25px',
                    'border-radius': '5px',
                    'box-shadow': '0 4px 6px rgba(0,0,0,0.1)',
                    'z-index': 10000,
                    'font-size': '16px',
                    'font-weight': 'bold'
                })
                .text(message)
                .appendTo('body');

            setTimeout(function () {
                notification.fadeOut(500, function () {
                    $(this).remove();
                });
            }, 3000);
        }

        // Initialize everything when document is ready
        $(document).ready(function () {
            console.log('🚀 Initializing BanglaResult page...');
            
            initializeDatePicker();
            updateResultDate();
            initializeSignatureUpload();
            updateSignatureTexts();
            
            // Load signatures from database on page load
            setTimeout(function() {
                loadDatabaseSignatures();
            }, 500);

            // Load Results Button Handler
            $("[id*=LoadResultsButton]").off('click').on('click', function (e) {
                console.log('🚀 Load Results button clicked');

                var classValue = $("[id*=ClassDropDownList]").val();
                var examValue = $("[id*=ExamDropDownList]").val();

                if (!classValue || classValue === "0") {
                    alert("দয়া করে একটি ক্লাস নির্বাচন করুন!");
                    e.preventDefault();
                    return false;
                }

                if (!examValue || examValue === "0") {
                    alert("দয়া করে একটি পরীক্ষা নির্বাচন করুন!");
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
                    // Call signature loading and other initialization
                    onResultsLoaded();
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

        // Also load signatures when results are loaded
        function onResultsLoaded() {
            console.log('📋 Results loaded, loading signatures...');
            setTimeout(function() {
                loadDatabaseSignatures();
                updateResultDate();
                updateSignatureTexts(); // ✅ Ensure signature labels are updated too
            }, 500);
        }

        // Print function
        function printResults() {
            window.print();
        }

        // Number language toggle function
        var isEnglishNumber = true;
        function toggleNumberLanguage() {
            var numberToggleBtn = document.getElementById('NumberToggleButton');
            
            if (isEnglishNumber) {
                convertAllNumbersToBengali();
                if (numberToggleBtn) {
                    numberToggleBtn.innerHTML = '<i class="fa fa-language"></i> English সংখ্যা';
                }
            } else {
                convertAllNumbersToEnglish();
                if (numberToggleBtn) {
                    numberToggleBtn.innerHTML = '<i class="fa fa-language"></i> বাংলা সংখ্যা';
                }
            }
            
            isEnglishNumber = !isEnglishNumber;
        }

        function convertAllNumbersToBengali() {
            console.log('Converting numbers to Bengali...');
            var englishToBengaliMap = {
                '0': '০', '1': '১', '2': '২', '3': '৩', '4': '৪',
                '5': '৫', '6': '৬', '7': '৭', '8': '৮', '9': '৯'
            };

            // Convert in text nodes only, not in HTML attributes or tags
            $('.result-card').each(function() {
                convertTextNodesToBengali(this, englishToBengaliMap);
            });

            console.log('Conversion to Bengali completed');
        }

        function convertAllNumbersToEnglish() {
            console.log('Converting numbers to English...');
            var bengaliToEnglishMap = {
                '০': '0', '১': '1', '২': '2', '৩': '3', '৪': '4',
                '৫': '5', '৬': '6', '৭': '7', '৮': '8', '৯': '9'
            };

            // Convert in text nodes only, not in HTML attributes or tags
            $('.result-card').each(function() {
                convertTextNodesToEnglish(this, bengaliToEnglishMap);
            });

            console.log('Conversion to English completed');
        }

        function convertTextNodesToBengali(element, map) {
            // Only process text nodes, skip HTML tags
            if (element.nodeType === Node.TEXT_NODE) {
                var text = element.nodeValue;
                var converted = false;

                for (var digit in map) {
                    if (text.indexOf(digit) !== -1) {
                        element.nodeValue = text.replace(/[0-9]/g, function(match) {
                            return map[match] || match;
                        });
                        converted = true;
                        break;
                    }
                }
            } else {
                // Recursively process child nodes, but skip script and style tags
                if (element.tagName !== 'SCRIPT' && element.tagName !== 'STYLE') {
                    for (var i = 0; i < element.childNodes.length; i++) {
                        convertTextNodesToBengali(element.childNodes[i], map);
                    }
                }
            }
        }

        function convertTextNodesToEnglish(element, map) {
            // Only process text nodes, skip HTML tags
            if (element.nodeType === Node.TEXT_NODE) {
                var text = element.nodeValue;
                var converted = false;

                for (var digit in map) {
                    if (text.indexOf(digit) !== -1) {
                        element.nodeValue = text.replace(/[০-৯]/g, function(match) {
                            return map[match] || match;
                        });
                        converted = true;
                        break;
                    }
                }
            } else {
                // Recursively process child nodes, but skip script and style tags
                if (element.tagName !== 'SCRIPT' && element.tagName !== 'STYLE') {
                    for (var i = 0; i < element.childNodes.length; i++) {
                        convertTextNodesToEnglish(element.childNodes[i], map);
                    }
                }
            }
        }
    </script>
</asp:Content>