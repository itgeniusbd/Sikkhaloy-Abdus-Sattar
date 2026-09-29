<%@ Page Title="Student Attendance" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="StudentAttendance.aspx.cs" Inherits="EDUCATION.COM.Teacher.StudentAttendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .att-page .row-selected { background-color: #f3fdf6; }
        .att-page .active-attendance { background-color: #f3fdf6; }
        .att-page .UpdatedByName { color: #2c5282; font-size: 11px; font-weight: bold; white-space: normal; }

        .att-filters .form-group { margin-bottom: 10px; }
        .att-lang { display: inline-flex !important; align-items: center; flex-wrap: wrap; padding: 4px 10px !important; min-height: auto !important; height: auto !important; }
        .att-lang label {
            background-image: none !important;
            height: auto !important;
            line-height: 1.2 !important;
            padding: 6px 10px !important;
            margin: 0 4px 0 0 !important;
            border-radius: 16px;
            border: 1px solid #cfd8dc;
            font-size: 13px !important;
        }
        .att-lang input[type=radio]:checked + label { background: #00a12a; color: #fff; border-color: #00a12a; }

        .att-status { display: flex; flex-wrap: wrap; gap: 6px; align-items: center; }
        .att-status input[type=radio] { position: absolute; opacity: 0; pointer-events: none; }
        .att-status label {
            display: inline-flex !important;
            align-items: center;
            justify-content: center;
            min-width: 52px;
            height: 34px !important;
            line-height: 1 !important;
            padding: 0 12px !important;
            margin: 0 !important;
            border-radius: 17px;
            border: 1px solid #d0d5dd;
            background: #fff !important;
            background-image: none !important;
            font-size: 13px !important;
            font-weight: 600;
            color: #455a64;
        }
        .att-status input[value="Pre"] + label { color: #2e7d32; border-color: #a5d6a7; }
        .att-status input[value="Pre"]:checked + label { background: #2e7d32 !important; color: #fff; border-color: #2e7d32; }
        .att-status input[value="Abs"] + label { color: #c62828; border-color: #ef9a9a; }
        .att-status input[value="Abs"]:checked + label { background: #c62828 !important; color: #fff; border-color: #c62828; }
        .att-status input[value="Late"] + label { color: #ef6c00; border-color: #ffcc80; }
        .att-status input[value="Late"]:checked + label { background: #ef6c00 !important; color: #fff; border-color: #ef6c00; }
        .att-status input[value="Leave"] + label { color: #1565c0; border-color: #90caf9; }
        .att-status input[value="Leave"]:checked + label { background: #1565c0 !important; color: #fff; border-color: #1565c0; }
        .att-status input[value="Bunk"] + label { color: #6a1b9a; border-color: #ce93d8; }
        .att-status input[value="Bunk"]:checked + label { background: #6a1b9a !important; color: #fff; border-color: #6a1b9a; }

        .att-extra { margin-top: 8px; }
        .att-row.att-pre .att-extra { display: none; }
        .att-page .att-extra input[type=checkbox] {
            display: inline-block !important;
            width: 18px;
            height: 18px;
            margin: 0 6px 0 0;
            vertical-align: middle;
            cursor: pointer;
            accent-color: #00a12a;
        }
        .att-page .att-extra input[type=checkbox] + label,
        .att-page .att-sms label {
            background-image: none !important;
            padding-left: 0 !important;
            height: auto !important;
            line-height: 1.4 !important;
            display: inline !important;
            cursor: pointer;
            font-weight: 600;
            color: #37474f;
        }
        .att-submit-bar { margin-top: 12px; }
        .att-all-pre {
            margin-left: 8px;
            border: 1px solid #a5d6a7;
            background: #e8f5e9;
            color: #1b5e20;
            border-radius: 14px;
            font-size: 12px;
            font-weight: 600;
            padding: 3px 10px;
            cursor: pointer;
        }

        @media (max-width: 767.98px) {
            .att-page > h3 { font-size: 0.95rem; padding: 0.7rem 0.85rem; }
            .att-filters.form-inline { display: block; }
            .att-filters .form-group { width: 100%; margin-right: 0; }
            .att-filters .form-control,
            .att-filters select { width: 100%; max-width: 100%; }
            .att-filters .btn { width: 100%; }
            .att-lang { width: 100%; justify-content: space-between; }
            .att-lang label { flex: 1; text-align: center; justify-content: center; }

            .att-table { overflow: visible !important; }
            .att-grid { border: none; text-align: left; }
            .att-grid > tbody > tr:first-child {
                display: flex;
                align-items: center;
                justify-content: space-between;
                background: #1b5e20;
                border-radius: 12px;
                padding: 10px 14px;
                margin-bottom: 10px;
            }
            .att-grid > tbody > tr:first-child th {
                display: none;
                border: none;
                background: transparent;
                color: #fff;
                padding: 0;
            }
            .att-grid > tbody > tr:first-child th:first-child { display: flex; align-items: center; }
            .att-all-pre { border-color: rgba(255,255,255,.55); background: rgba(255,255,255,.12); color: #fff; }

            .att-grid > tbody > tr:not(:first-child):not(.pgr) {
                display: grid;
                grid-template-columns: 36px 1fr auto;
                grid-template-areas:
                    "check name roll"
                    "check id taken"
                    "att att att";
                gap: 2px 8px;
                background: #fff;
                border: 1px solid #e6e8ee;
                border-radius: 12px;
                box-shadow: 0 2px 8px rgba(16, 24, 40, 0.06);
                margin-bottom: 10px;
                padding: 12px;
                text-align: left;
            }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td {
                display: block;
                width: auto;
                border: none;
                padding: 0;
                font-size: 14px;
            }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td:nth-child(1) { grid-area: check; padding-top: 2px; }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td:nth-child(2) { grid-area: id; color: #78909c; font-size: 12px; }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td:nth-child(2)::before { content: "ID "; }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td:nth-child(3) { grid-area: name; font-size: 15px; }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td:nth-child(4) {
                grid-area: roll;
                background: #e8f5e9;
                color: #2e7d32;
                border-radius: 12px;
                padding: 2px 8px;
                font-size: 12px;
                font-weight: 600;
                text-align: center;
            }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td:nth-child(4)::before { content: "Roll "; }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td:nth-child(5) { grid-area: taken; font-size: 11px; text-align: right; }
            .att-grid > tbody > tr:not(:first-child):not(.pgr) > td:nth-child(6) { grid-area: att; padding-top: 8px; }
            .att-grid .row-selected,
            .att-grid .active-attendance { background-color: #f3fdf6; }
            .att-status { width: 100%; }
            .att-status label { flex: 1 1 18%; min-width: 56px; height: 38px !important; }
            .att-submit-bar {
                position: sticky;
                bottom: 0;
                z-index: 8;
                background: #fff;
                padding: 10px 0 6px;
                box-shadow: 0 -6px 16px rgba(16, 24, 40, 0.08);
            }
            .att-submit-bar .btn { width: 100%; padding: 0.7rem 1rem; }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="att-page">
    <h3>Students Attendance</h3>
    <asp:Label ID="SuccessLabel" runat="server" CssClass="text-success" Font-Bold="true"></asp:Label>
    <div class="form-inline att-filters">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group">
            <asp:DropDownList ID="ScheduleDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ScheduleSQL" DataTextField="ScheduleName" DataValueField="ScheduleID" OnSelectedIndexChanged="ScheduleDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ SELECT SCHEDULE ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ScheduleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ScheduleID, ScheduleName FROM Attendance_Schedule WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
            <asp:RequiredFieldValidator ID="ScheduleRFV" runat="server" ControlToValidate="ScheduleDropDownList" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="1" InitialValue="0"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="AttendanceDateTextBox" autocomplete="off" placeholder="Attendance Date" runat="server" CssClass="form-control Datetime" onkeypress="return false;"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="AttendanceDateTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-default" OnClick="FindButton_Click" />
        </div>
        <div class="form-group">
            <asp:RadioButtonList ClientIDMode="Static" ID="SMSLanguageRadioButtonList" CssClass="form-control att-lang" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal">
                <asp:ListItem Selected="True" Value="eng">English SMS</asp:ListItem>
                <asp:ListItem Value="bn">Bangla SMS</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>

     

    <div class="table-responsive att-table">
        <asp:GridView ID="StudentsAttendanceGridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataKeyNames="StudentClassID,SMSPhoneNo,ID,StudentsName,StudentID" DataSourceID="StudentsRecordSQL" CssClass="mGrid att-grid" PageSize="50" OnDataBinding="StudentsAttendanceGridView_DataBinding" OnRowDataBound="StudentsAttendanceGridView_RowDataBound">
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ClientIDMode="Static" ID="AllCheckBox" runat="server" Text="All" />
                        <button type="button" id="MarkAllPresentBtn" class="att-all-pre">All Present</button>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="Attendance_CheckBox" onClick="onChangeSelect(this)" runat="server" Text=" " />
                    </ItemTemplate>
                    <HeaderStyle Width="50px" />
                </asp:TemplateField>
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:TemplateField HeaderText="Student's Name" SortExpression="StudentsName">
                    <ItemTemplate>
                        <strong><%# Eval("StudentsName") %></strong>
                        <small class="text-muted d-block"><%# Eval("SMSPhoneNo") %></small>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>
                <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
                <asp:TemplateField HeaderText="Taken By">
                    <ItemTemplate>
                        <asp:Label ID="UpdatedByLabel" runat="server" CssClass="UpdatedByName"></asp:Label>
                    </ItemTemplate>
                    <HeaderStyle Width="120px" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Attendance">
                    <ItemTemplate>
                        <div class="att-cell">
                            <asp:RadioButtonList ID="AttendenceRadioButtonList" runat="server" CssClass="att-status" RepeatLayout="Flow" RepeatDirection="Horizontal" data-name='<%# Eval("StudentsName") %>'>
                                <asp:ListItem Value="Pre" Selected="True">Pre</asp:ListItem>
                                <asp:ListItem Value="Abs">Abs</asp:ListItem>
                                <asp:ListItem Value="Late">Late</asp:ListItem>
                                <asp:ListItem Value="Leave">Leave</asp:ListItem>
                                <asp:ListItem Value="Bunk">Bunk</asp:ListItem>
                            </asp:RadioButtonList>
                            <div class="att-extra">
                                <asp:CheckBox ID="SMSCheckBox" runat="server" CssClass="att-sms" Enabled="False" Text="Send SMS?" />
                                <asp:Label ID="AtDateLabel" runat="server" CssClass="red-text d-block"></asp:Label>
                                <asp:TextBox ID="ReasonTextBox" runat="server" CssClass="form-control mt-1" Enabled="false" TextMode="MultiLine" Rows="2"></asp:TextBox>
                            </div>
                        </div>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
    </div>
    <asp:SqlDataSource ID="StudentsRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, StudentsClass.StudentID, StudentsClass.StudentClassID, StudentsClass.ShiftID, StudentsClass.RollNo, Student.SMSPhoneNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND EXISTS (SELECT 1 FROM Attendance_Schedule_AssignStudent ass WHERE ass.SchoolID = StudentsClass.SchoolID AND ass.StudentID = Student.StudentID AND ass.ScheduleID = @ScheduleID) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="ScheduleDropDownList" Name="ScheduleID" PropertyName="SelectedValue" Type="Int32" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
        </SelectParameters>
    </asp:SqlDataSource>


    <% if (StudentsAttendanceGridView.Rows.Count > 0)
    { %>
    <div class="att-submit-bar">
        <asp:Button ID="AttendanceButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="AttendanceButton_Click" ValidationGroup="1" CausesValidation="true" />
        <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
    </div>
    <%} %>

    <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
        <InsertParameters>
            <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
            <asp:Parameter Name="SchoolID" />
            <asp:Parameter Name="StudentID" />
            <asp:Parameter Name="TeacherID" />
            <asp:Parameter Name="EducationYearID" />
        </InsertParameters>
    </asp:SqlDataSource>


    <script>
        $(function () {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });

        (function () {
            function getCheckedRadio(selector) {
                const radioButtons = selector.querySelectorAll("input[type=radio]");

                for (let i = 0; i < radioButtons.length; i++) {
                    if (radioButtons[i].checked)
                        return radioButtons[i].value;
                }
                return null;
            }

            //SMS Language change
            const languageRadio = document.getElementById("SMSLanguageRadioButtonList");

            //student table and rows
            const studentsTable = document.querySelector(".mGrid");
            if (!studentsTable) return;

            const rows = studentsTable.rows;
            const checkAll = studentsTable.querySelector("#AllCheckBox");

            //on change select all
            if (checkAll) {
                checkAll.addEventListener("change", function () {
                    for (let i = 1; i < rows.length; i++) {
                        const box = rows[i].querySelector("[id*=Attendance_CheckBox]");
                        if (!box) continue;
                        box.checked = this.checked;
                        this.checked ? rows[i].classList.add("row-selected") : rows[i].classList.remove("row-selected");
                    }
                });
            }

            const markAllPresent = document.getElementById("MarkAllPresentBtn");
            if (markAllPresent) {
                markAllPresent.addEventListener("click", function () {
                    for (let i = 1; i < rows.length; i++) {
                        const pre = rows[i].querySelector('.att-status input[value="Pre"]');
                        if (!pre) continue;
                        pre.checked = true;
                        setMessage(pre, "", "Pre");
                    }
                });
            }

            //sms generator
            function getScheduleName() {
                const schedule = document.querySelector("[id*=ScheduleDropDownList]");
                if (!schedule || !schedule.options || schedule.selectedIndex < 0)
                    return "class";
                const text = schedule.options[schedule.selectedIndex].text;
                if (!text || text.indexOf("SELECT") >= 0)
                    return "class";
                return text;
            }

            function createTextSMS(status, name, language = "eng") {
                const isEnglish = language === "eng" ? true : false;
                const scheduleName = getScheduleName();
                const dateInput = document.getElementById("<%=AttendanceDateTextBox.ClientID%>");
                const date = (dateInput && dateInput.value) ? dateInput.value : (function () {
                    const dateInstance = new Date();
                    const option = { year: "numeric", month: "short", day: 'numeric' };
                    return dateInstance.toLocaleDateString(isEnglish ? 'en-us' : 'bn-BD', option);
                })();

                const abs = isEnglish ?
                    `Respected guardian, ${name}, today ${date} was absent in ${scheduleName} schedule. please send to class regularly` :
                    `সম্মানিত অভিভাবক, ${name}, আজ ${date} ${scheduleName} শিডিউলে অনুপস্থিত। অনুগ্রহ করে নিয়মিত ক্লাসে পাঠান`;

                const late = isEnglish ?
                    `Respected guardian, ${name}, today ${date} was late in ${scheduleName} schedule` :
                    `সম্মানিত অভিভাবক, ${name}, আজ ${date} ${scheduleName} শিডিউলে যথাসময় আসেনি`;

                const bunk = isEnglish ?
                    `Respected guardian, ${name}, today ${date} bunked from ${scheduleName} schedule` :
                    `সম্মানিত অভিভাবক, ${name}, আজ ${date} ${scheduleName} শিডিউল থেকে পলায়ন করেছে`;

                switch (status) {
                    case "Pre":
                        return ""
                    case "Abs":
                        return abs
                    case "Late":
                        return late
                    case "Leave":
                        return ""
                    case "Bunk":
                        return bunk
                    default:
                        return null
                }
            }

            function getRow(element) {
                return element.closest("tr");
            }

            function applyStatusClass(row, status) {
                if (!row) return;
                row.classList.remove("att-pre", "att-abs", "att-late", "att-leave", "att-bunk");
                if (status)
                    row.classList.add("att-" + status.toLowerCase());
            }

            function setMessage(element, message, status) {
                const row = getRow(element);
                applyStatusClass(row, status);

                const messageTextarea = row.querySelector("textarea");
                if (messageTextarea) {
                    messageTextarea.disabled = message ? false : true;
                    messageTextarea.value = message || "";
                }

                const smsCheckbox = row.querySelector("[id*=SMSCheckBox]");
                if (smsCheckbox) {
                    smsCheckbox.disabled = message ? false : true;
                    smsCheckbox.checked = message ? true : false;
                }
            }

            studentsTable.addEventListener("click", function (e) {
                if (e.target.type === "radio") {
                    const element = e.target;
                    const status = element.value;
                    const list = element.closest(".att-status") || element.parentElement;
                    const studentName = list && list.dataset ? list.dataset.name : "";

                    const language = getCheckedRadio(languageRadio);
                    const message = createTextSMS(status, studentName, language);
                    setMessage(element, message, status);
                }
            });

            for (let i = 1; i < rows.length; i++) {
                const checked = rows[i].querySelector(".att-status input[type=radio]:checked");
                applyStatusClass(rows[i], checked ? checked.value : "Pre");
            }
        })();

        function onChangeSelect(select) {
            const element = select.closest("tr") || select.parentElement.parentElement;
            select.checked ? element.classList.add("row-selected") : element.classList.remove("row-selected");
        }

        $(function () {
            $("#_8").addClass("active");
        });
    </script>
    </div>
</asp:Content>
