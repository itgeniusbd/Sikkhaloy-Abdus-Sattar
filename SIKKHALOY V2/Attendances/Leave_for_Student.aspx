<%@ Page Title="Leave for student" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Leave_for_Student.aspx.cs" Inherits="EDUCATION.COM.ATTENDANCES.Leave_for_Student" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* ── Page Layout ── */
        .leave-page-wrapper {
            max-width: 860px;
            margin: 18px auto;
            padding: 0 10px;
            font-family: 'Segoe UI', Arial, sans-serif;
        }

        /* ── Page Title ── */
        .leave-page-title {
            display: flex;
            align-items: center;
            gap: 14px;
            margin-bottom: 20px;
            padding: 14px 20px;
            background: linear-gradient(135deg, #1a6fc4 0%, #0e4f96 100%);
            border-radius: 12px;
            box-shadow: 0 4px 14px rgba(26,111,196,.25);
        }
        .leave-page-title .title-icon {
            width: 44px; height: 44px;
            background: rgba(255,255,255,.18);
            border-radius: 10px;
            display: flex; align-items: center; justify-content: center;
            flex-shrink: 0;
            border: 1.5px solid rgba(255,255,255,.3);
        }
        .leave-page-title .title-icon svg {
            width: 22px; height: 22px;
            fill: none;
            stroke: #fff;
            stroke-width: 2;
            stroke-linecap: round;
            stroke-linejoin: round;
        }
        .leave-page-title h3 {
            margin: 0;
            font-size: 18px;
            font-weight: 700;
            color: #fff !important;
            letter-spacing: .3px;
            background: none !important;
            box-shadow: none !important;
            padding: 0 !important;
            border: none !important;
            text-transform: none !important;
        }
        .leave-page-title p {
            margin: 2px 0 0;
            font-size: 12px;
            color: rgba(255,255,255,.75);
        }

        /* ── Search Card ── */
        .search-card {
            background: #fff;
            border: 1px solid #e0eaf6;
            border-radius: 12px;
            padding: 16px 20px;
            margin-bottom: 18px;
            box-shadow: 0 2px 8px rgba(26,111,196,.07);
            display: flex;
            align-items: center;
            gap: 12px;
            flex-wrap: wrap;
        }
        .search-card label {
            font-size: 13px;
            font-weight: 600;
            color: #1a6fc4;
            margin-bottom: 0;
            white-space: nowrap;
            display: flex;
            align-items: center;
            gap: 6px;
        }
        .search-card label::before {
            content: '';
            display: inline-block;
            width: 16px; height: 16px;
            background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%231a6fc4' stroke-width='2.2'%3E%3Ccircle cx='11' cy='11' r='8'/%3E%3Cpath d='M21 21l-4.35-4.35'/%3E%3C/svg%3E") center/contain no-repeat;
            flex-shrink: 0;
        }
        .search-card .input-group {
            flex: 1;
            min-width: 200px;
            max-width: 320px;
        }
        .search-card .input-group-text {
            background: #f0f6ff;
            border-color: #c5d8f0;
            color: #1a6fc4;
        }
        .search-card .form-control {
            border-color: #c5d8f0;
            border-radius: 0 6px 6px 0 !important;
            font-size: 20px;
        }
        .search-card .form-control:focus {
            border-color: #1a6fc4;
            box-shadow: 0 0 0 3px rgba(26,111,196,.12);
        }
        .search-card .btn-find {
            background: linear-gradient(135deg, #1a6fc4, #0e4f96);
            border: none;
            color: #fff;
            padding: 8px 22px;
            border-radius: 8px;
            font-size: 14px;
            font-weight: 600;
            letter-spacing: .3px;
            transition: opacity .2s;
        }
        .search-card .btn-find:hover { opacity: .88; }

        /* ── Student Info Card ── */
        .student-card {
            background: #fff;
            border: 1px solid #e0eaf6;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(26,111,196,.07);
            width:100%;
        }
        .student-card-header {
            background: linear-gradient(135deg, #1a6fc4, #0e4f96);
            color: #fff;
            padding: 10px 18px;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .student-card-header .card-title {
            font-size: 14px;
            font-weight: 700;
            margin: 0;
            letter-spacing: .3px;
        }

        /* ── Student profile row (photo + info) ── */
        .student-profile-row {
            display: flex;
            align-items: stretch;
            gap: 0;
            border-bottom: 1px solid #e8f0fb;
        }
        .student-photo-col {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 16px 14px;
            background: linear-gradient(180deg, #f0f6ff 0%, #e8f0fb 100%);
            border-right: 1px solid #dce8f8;
            min-width: 110px;
            flex-shrink: 0;
        }
        .student-photo-wrap {
            width: 78px;
            height: 78px;
            border-radius: 50%;
            border: 3px solid #1a6fc4;
            overflow: hidden;
            box-shadow: 0 3px 10px rgba(26,111,196,.22);
            background: #fff;
            flex-shrink: 0;
        }
        .student-photo-wrap img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            display: block;
        }
        .student-photo-name {
            margin-top: 7px;
            font-size: 11px;
            font-weight: 700;
            color: #1a3a5c;
            text-align: center;
            max-width: 95px;
            line-height: 1.3;
            word-break: break-word;
        }
        .student-photo-id {
            font-size: 10px;
            color: #1a6fc4;
            font-weight: 600;
            margin-top: 2px;
            background: #ddeeff;
            border-radius: 10px;
            padding: 1px 8px;
        }

        /* ── Student info grid (top section) ── */
        .student-info-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 0;
            flex: 1;
        }
        .si-item {
            padding: 11px 16px;
            border-right: 1px solid #eef3fb;
            border-bottom: 1px solid #eef3fb;
        }
        .si-item:nth-child(3n) { border-right: none; }
        .si-item:nth-last-child(-n+3) { border-bottom: none; }
        .si-item .si-label {
            font-size: 10.5px;
            font-weight: 600;
            color: #1a6fc4;
            text-transform: uppercase;
            letter-spacing: .4px;
            margin-bottom: 3px;
        }
        .si-item .si-value {
            font-size: 14px;
            font-weight: 600;
            color: #1a3a5c;
        }
        .si-item .si-value.muted { color: #888; font-weight: 400; font-size: 13px; }

        /* ── Leave form section ── */
        .leave-form-section {
            padding: 18px 20px 20px;
        }
        .leave-form-section .section-title {
            font-size: 13px;
            font-weight: 700;
            color: #1a6fc4;
            margin-bottom: 14px;
            display: flex;
            align-items: center;
            gap: 8px;
            padding-left: 10px;
            border-left: 3px solid #1a6fc4;
            line-height: 1.2;
        }
        .leave-form-section .section-title::after {
            content: '';
            flex: 1;
            height: 1px;
            background: #e0eaf6;
            margin-left: 4px;
        }

        .leave-form-row {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 14px;
            margin-bottom: 14px;
        }
        .leave-form-row.three-col {
            grid-template-columns: 1fr 1fr 1fr;
        }
        .leave-form-row.full {
            grid-template-columns: 1fr;
        }

        .lf-group label {
            font-size: 12px;
            font-weight: 600;
            color: #555;
            margin-bottom: 5px;
            display: block;
        }
        .lf-group .form-control {
            border: 1px solid #c5d8f0;
            border-radius: 7px;
            font-size: 13.5px;
            padding: 8px 12px;
            color: #222;
            transition: border-color .2s, box-shadow .2s;
        }
        .lf-group .form-control:focus {
            border-color: #1a6fc4;
            box-shadow: 0 0 0 3px rgba(26,111,196,.12);
        }
        .lf-group textarea.form-control {
            resize: vertical;
            min-height: 72px;
        }
        .lf-group select.form-control {
            appearance: auto;
        }

        /* Duration badge */
        .duration-badge {
            display: inline-block;
            background: #e6f5ee;
            color: #0e6640;
            border: 1px solid #a3d9bc;
            border-radius: 20px;
            padding: 4px 14px;
            font-size: 13px;
            font-weight: 700;
            min-height: 36px;
            line-height: 1.8;
            min-width: 100px;
            text-align: center;
        }
        .duration-badge.empty { background: #f5f5f5; color: #bbb; border-color: #ddd; }

        /* Submit row */
        .leave-submit-row {
            display: flex;
            justify-content: flex-end;
            margin-top: 6px;
        }
        .btn-submit-leave {
            background: linear-gradient(135deg, #1a6fc4, #0e4f96);
            border: none;
            color: #fff;
            padding: 10px 36px;
            border-radius: 9px;
            font-size: 15px;
            font-weight: 700;
            letter-spacing: .3px;
            cursor: pointer;
            transition: opacity .2s, transform .1s;
        }
        .btn-submit-leave:hover { opacity: .88; transform: translateY(-1px); }
        .btn-submit-leave:active { transform: translateY(0); }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="leave-page-wrapper">

        <%-- Page Title --%>
        <div class="leave-page-title">
            <div class="title-icon">
                <svg viewBox="0 0 24 24"><path d="M8 7V3m8 4V3M3 11h18M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/></svg>
            </div>
            <div>
                <h3>Leave for Student</h3>
                <p>শিক্ষার্থীর ছুটির আবেদন নিবন্ধন করুন</p>
            </div>
        </div>

        <%-- Search Card --%>
        <div class="search-card">
            <label>শিক্ষার্থী খুঁজুন</label>
            <div class="input-group">
                <div class="input-group-prepend">
                    <span class="input-group-text">ID</span>
                </div>
                <asp:TextBox ID="IDTextBox" placeholder="Student ID" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <asp:Button ID="FindButton" runat="server" CssClass="btn-find" Text="খুঁজুন" OnClick="FindButton_Click" />
        </div>

        <%-- Student Detail + Leave Form (only visible after search) --%>
        <asp:DetailsView ID="StudentDetailsView" runat="server" AutoGenerateRows="False"
            DataKeyNames="StudentID" DataSourceID="StudentInfoSQL"
            CssClass="student-card" BorderStyle="None">
            <Fields>
                <%-- Custom template renders the entire card --%>
                <asp:TemplateField>
                    <ItemTemplate>

                        <%-- Card Header --%>
                        <div class="student-card-header">
                            <svg style="width:16px;height:16px;fill:none;stroke:#fff;stroke-width:2;stroke-linecap:round;stroke-linejoin:round;flex-shrink:0" viewBox="0 0 24 24"><path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                            <span class="card-title">শিক্ষার্থীর তথ্য</span>
                        </div>

                        <%-- Student Profile Row: Photo + Info Grid --%>
                        <div class="student-profile-row">

                            <%-- Photo Column --%>
                            <div class="student-photo-col">
                                <div class="student-photo-wrap">
                                    <img src='<%# "/Handeler/Student_Photo.ashx?SID=" + Eval("StudentImageID") %>'
                                         onerror="this.src='/Handeler/Student_Photo.ashx?SID=0'"
                                         alt="Student Photo" />
                                </div>
                                <div class="student-photo-name"><%# Eval("StudentsName") %></div>
                                <div class="student-photo-id">ID: <%# Eval("ID") %></div>
                            </div>

                            <%-- Info Grid (3 columns) --%>
                            <div class="student-info-grid">
                                <div class="si-item">
                                    <div class="si-label">Father's Name</div>
                                    <div class="si-value"><%# Eval("FathersName") %></div>
                                </div>
                                <div class="si-item">
                                    <div class="si-label">Gender</div>
                                    <div class="si-value"><%# Eval("Gender") %></div>
                                </div>
                                <div class="si-item">
                                    <div class="si-label">Class</div>
                                    <div class="si-value"><%# Eval("Class") %></div>
                                </div>
                                <div class="si-item">
                                    <div class="si-label">Section</div>
                                    <div class="si-value"><%# Eval("Section") %></div>
                                </div>
                                <div class="si-item">
                                    <div class="si-label">Group</div>
                                    <div class="si-value"><%# Eval("Group") %></div>
                                </div>
                                <div class="si-item">
                                    <div class="si-label">Shift</div>
                                    <div class="si-value muted"><%# Eval("Shift") %></div>
                                </div>
                            </div>

                        </div>

                        <%-- Leave Form Section --%>
                        <div class="leave-form-section">
                            <div class="section-title">ছুটির বিবরণ</div>

                            <div class="leave-form-row three-col">
                                <div class="lf-group">
                                    <label>শুরুর তারিখ (From)</label>
                                    <asp:TextBox ID="StartDateTextBox" placeholder="Start Date" runat="server" CssClass="form-control Datepicker" onkeypress="return isNumberKey(event)" />
                                </div>
                                <div class="lf-group">
                                    <label>শেষ তারিখ (To)</label>
                                    <asp:TextBox ID="EndDateTextBox" placeholder="End Date" runat="server" CssClass="form-control Datepicker" onkeypress="return isNumberKey(event)" />
                                </div>
                                <div class="lf-group">
                                    <label>মোট দিন</label>
                                    <div>
                                        <label class="duration-badge empty calculated">-- নির্বাচন করুন --</label>
                                        <asp:HiddenField ID="DurationHF" runat="server" />
                                    </div>
                                </div>
                            </div>

                            <div class="leave-form-row">
                                <div class="lf-group">
                                    <label>ছুটির ধরণ</label>
                                    <asp:DropDownList ID="LeaveTypeDropDownList" runat="server" CssClass="form-control">
                                        <asp:ListItem Value="">-- Select --</asp:ListItem>
                                        <asp:ListItem>অসুস্থতার জন্য</asp:ListItem>
                                        <asp:ListItem>ব্যাক্তিগত কারনে</asp:ListItem>
                                        <asp:ListItem>ফ্যামেলি প্রয়োজনে</asp:ListItem>
                                        <asp:ListItem>মেডিক্যাল</asp:ListItem>
                                        <asp:ListItem>সাময়িক</asp:ListItem>
                                        <asp:ListItem>অন্যান্ন</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="lf-group">
                                    <label>অভিভাবকের নাম</label>
                                    <asp:TextBox ID="GuardianNameTextBox" runat="server" CssClass="form-control" placeholder="Guardian Name"></asp:TextBox>
                                </div>
                            </div>

                            <div class="leave-form-row full">
                                <div class="lf-group">
                                    <label>ছুটির কারণ</label>
                                    <asp:TextBox ID="DescriptionTextBox" runat="server" CssClass="form-control" TextMode="MultiLine" placeholder="ছুটির কারণ লিখুন..."></asp:TextBox>
                                </div>
                            </div>

                            <div class="leave-submit-row">
                                <asp:Button ID="SubmitButton" runat="server" CssClass="btn-submit-leave" Text="✔ Submit Leave" OnClick="SubmitButton_Click" />
                            </div>
                        </div>

                    </ItemTemplate>
                </asp:TemplateField>
            </Fields>
        </asp:DetailsView>

        <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, ISNULL(CreateSection.Section, 'No section') AS Section, ISNULL(CreateSubjectGroup.SubjectGroup, 'No Group') AS [Group], CreateShift.Shift, Student.ID, Student.StudentsName, Student.Gender, Student.MothersName, Student.FathersName, Student.SMSPhoneNo, Student.StudentID, ISNULL(Student.StudentImageID, 0) AS StudentImageID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                <asp:Parameter DefaultValue="Active" Name="Status" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="LeaveSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Attendance_Leave(SchoolID, RegistrationID, StudentID, StartDate, EndDate, Description, EducationYearID, LeaveType, GuardianName) VALUES (@SchoolID, @RegistrationID, @StudentID, @StartDate, @EndDate, @Description, @EducationYearID, @LeaveType, @GuardianName)" SelectCommand="SELECT StudentLeaveID, SchoolID, RegistrationID, StudentID, StartDate, EndDate, Description FROM Attendance_Leave WHERE (SchoolID = @SchoolID)">
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:Parameter Name="StudentID" Type="Int32" />
                <asp:Parameter DbType="Date" Name="StartDate" />
                <asp:Parameter DbType="Date" Name="EndDate" />
                <asp:Parameter Name="Description" Type="String" />
                <asp:Parameter Name="LeaveType" Type="String" />
                <asp:Parameter Name="GuardianName" Type="String" />
            </InsertParameters>
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>

    </div>


    <script type="text/javascript">
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        $(function () {
            $('[id*=IDTextBox]').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "/Handeler/Student_IDs.asmx/GetStudentID",
                        data: JSON.stringify({ 'ids': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            result($.map(JSON.parse(response.d), function (item) {
                                return item;
                            }));
                        }
                    });
                }
            });

            $('.Datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            }).bind("change", function () {
                var oneDay = 24 * 60 * 60 * 1000;
                var firstDate  = new Date($('[id$=StartDateTextBox]').val());
                var secondDate = new Date($('[id$=EndDateTextBox]').val());

                if (!isNaN(firstDate) && !isNaN(secondDate) && firstDate.getTime() && secondDate.getTime()) {
                    var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime()) / (oneDay)) + 1);
                    $(".calculated").text(diffDays + " দিন").removeClass("empty");
                    $("[id$=DurationHF]").val(diffDays);
                } else {
                    $(".calculated").text("-- দিন নির্বাচন করুন --").addClass("empty");
                    $("[id$=DurationHF]").val('');
                }
            });
        });
    </script>
</asp:Content>
