<%@ Page Title="Student Attendance" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="StudentAttendance.aspx.cs" Inherits="EDUCATION.COM.Teacher.StudentAttendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .row-selected { background-color: #f3fdf6 }
        .active-attendance { background-color: #f3fdf6 }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Students Attendance</h3>
    <asp:Label ID="SuccessLabel" runat="server" CssClass="text-success" Font-Bold="true"></asp:Label>
    <div class="form-inline">
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
            <asp:RadioButtonList ClientIDMode="Static" ID="SMSLanguageRadioButtonList" CssClass="form-control" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal">
                <asp:ListItem Selected="True" Value="eng">English SMS</asp:ListItem>
                <asp:ListItem Value="bn">Bangla SMS</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>

     

    <div class="table-responsive">
        <asp:GridView ID="StudentsAttendanceGridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataKeyNames="StudentClassID,SMSPhoneNo,ID,StudentsName,StudentID" DataSourceID="StudentsRecordSQL" CssClass="mGrid" PageSize="50">
            <Columns>
                <asp:TemplateField>
                    <HeaderTemplate>
                        <asp:CheckBox ClientIDMode="Static" ID="AllCheckBox" runat="server" Text="All" />
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
                <asp:TemplateField HeaderText="Attendance">
                    <ItemTemplate>
                        <div class="px-2">
                            <div class="d-flex flex-column flex-md-row justify-content-between align-items-center mb-1">
                                <asp:RadioButtonList ID="AttendenceRadioButtonList" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal" data-name='<%# Eval("StudentsName") %>'>
                                    <asp:ListItem Selected="True">Pre</asp:ListItem>
                                    <asp:ListItem>Abs</asp:ListItem>
                                    <asp:ListItem>Late</asp:ListItem>
                                    <asp:ListItem>Leave</asp:ListItem>
                                    <asp:ListItem>Bunk</asp:ListItem>
                                </asp:RadioButtonList>
                                <asp:CheckBox ID="SMSCheckBox" runat="server" Enabled="False" Text='Send SMS?' />
                            </div>

                            <asp:Label ID="AtDateLabel" runat="server" CssClass="red-text"></asp:Label>
                            <asp:TextBox ID="ReasonTextBox" runat="server" CssClass="form-control" Enabled="false" TextMode="MultiLine"></asp:TextBox>
                        </div>
                    </ItemTemplate>
                    <ItemStyle HorizontalAlign="Left" />
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
    </div>
    <asp:SqlDataSource ID="StudentsRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName, StudentsClass.StudentID, StudentsClass.StudentClassID, StudentsClass.ShiftID, StudentsClass.RollNo, Student.SMSPhoneNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID  WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
        <SelectParameters>
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
        </SelectParameters>
    </asp:SqlDataSource>


    <% if (StudentsAttendanceGridView.Rows.Count > 0)
    { %>
    <asp:Button ID="AttendanceButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="AttendanceButton_Click" ValidationGroup="1" />
    <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
    <%} %>

    <asp:SqlDataSource ID="Attendance_RecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT AttendanceRecordID from Attendance_Record Where StudentClassID = @StudentClassID and AttendanceDate = CAST(@AttendanceDate as date) and SchoolID = @SchoolID and EducationYearID = @EducationYearID)
BEGIN
INSERT INTO Attendance_Record(SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID, Attendance, AttendanceDate, Reason) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @ClassID, @StudentClassID, @Attendance, @AttendanceDate, @Reason)
END
ELSE
BEGIN 
UPDATE Attendance_Record SET Attendance = @Attendance, Reason = @Reason
WHERE (StudentClassID = @StudentClassID) AND (AttendanceDate = CAST(@AttendanceDate as date)) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)
END"
        SelectCommand="SELECT * FROM [Attendance_Record]">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
            <asp:Parameter Name="StudentID" Type="Int32" />
            <asp:Parameter Name="StudentClassID" Type="Int32" />
            <asp:Parameter Name="Attendance" Type="String" />
            <asp:Parameter DbType="Date" Name="AttendanceDate" />
            <asp:Parameter Name="Reason" Type="String" />
        </InsertParameters>
    </asp:SqlDataSource>
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
            checkAll.addEventListener("change", function () {
                for (let i = 1; i < rows.length; i++) {
                    rows[i].querySelector("input[type=checkbox]").checked = this.checked;

                    this.checked ? rows[i].classList.add("row-selected") : rows[i].classList.remove("row-selected");
                }
            });

            //sms generator
            function createTextSMS(status, name, language = "eng") {
                const isEnglish = language === "eng" ? true : false;
                const dateInstance = new Date();
                const option = { year: "numeric", month: "short", day: 'numeric' };
                const date = dateInstance.toLocaleDateString(isEnglish ? 'en-us' : 'bn-BD', option);

                const abs = isEnglish ?
                    `Respected guardian, ${name}, today ${date} absent from class. please send to class regularly` :
                    `সম্মানিত অভিভাবক, ${name}, আজ ${date} ক্লাসে অনুপস্থিত। অনুগ্রহ করে নিয়মিত ক্লাসে পাঠান`;

                const late = isEnglish ?
                    `Respected guardian, ${name}, today ${date} late in class` :
                    `সম্মানিত অভিভাবক, ${name}, আজ ${date} ক্লাসে যথাসময় আসেনি`;

                const bunk = isEnglish ?
                    `Respected guardian, ${name}, today ${date} bunk from class` :
                    `সম্মানিত অভিভাবক, ${name}, আজ ${date} ক্লাস থেকে পলায়ন করেছে`;

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

            //set sms to textarea
            function setMessage(element, message) {
                const parent = element.parentElement.parentElement.parentElement;

                const messageTextarea = parent.getElementsByTagName("textarea")[0];
                messageTextarea.disabled = message ? false : true;
                messageTextarea.value = message;

                const smsCheckbox = parent.querySelector("input[type=checkbox]");
                smsCheckbox.disabled = message ? false : true;
                smsCheckbox.checked = message ? true : false;
            }

            //radio button click
            studentsTable.addEventListener("click", function (e) {
                if (e.target.type === "radio") {
                    const element = e.target;
                    const status = element.value;
                    const studentName = element.parentElement.dataset.name;

                    const language = getCheckedRadio(languageRadio);
                    const message = createTextSMS(status, studentName, language);
                    setMessage(element, message);
                }
            })
        })();

        //on change select
        function onChangeSelect(select) {
            const element = select.parentElement.parentElement;
            select.checked ? element.classList.add("row-selected") : element.classList.remove("row-selected");
        }

        $(function () {
            $("#_8").addClass("active");
        });
    </script>
</asp:Content>
