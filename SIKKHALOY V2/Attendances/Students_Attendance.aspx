<%@ Page Title="Manual Students Attendance" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Students_Attendance.aspx.cs" Inherits="EDUCATION.COM.ATTENDANCES.Students_Attendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Students_Attendance.css?v=2" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Students Attendance (Insert/Update)</h3>

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
                    <asp:TextBox ID="AttendanceDateTextBox" autocomplete="off" placeholder="Attendance Date" runat="server" CssClass="form-control Datetime" onkeypress="return DisableAllKey()"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="AttendanceDateTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="FindButton" runat="server" Text="Find" CssClass="btn btn-primary" OnClick="FindButton_Click" />
                </div>
                <div class="form-group">
                    <asp:RadioButtonList CssClass="form-control" ID="SMSRadioButtonList" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem Selected="True" Value="0">English SMS</asp:ListItem>
                        <asp:ListItem Value="1">Bangla SMS</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="StudentsAttendanceGridView" AllowSorting="true" runat="server" AutoGenerateColumns="False" DataKeyNames="StudentClassID,SMSPhoneNo,ID,StudentsName,StudentID" DataSourceID="StudentsRecordSQL" CssClass="mGrid" PageSize="50" Visible="False">
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllCheckBox" runat="server" Text="All" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="Attendance_CheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <HeaderStyle Width="50px" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:TemplateField HeaderText="Student's Name" SortExpression="StudentsName">
                            <ItemTemplate>
                                <asp:Label ID="StudentsNameLabel" runat="server" Text='<%# Bind("StudentsName") %>'></asp:Label>
                                 <asp:HiddenField ID="StudentIDHidden" runat="server" Value='<%# Bind("ID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
                        <asp:TemplateField HeaderText="Attendance">
                            <ItemTemplate>
                                <asp:RadioButtonList ID="AttendenceRadioButtonList" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Selected="True">Pre</asp:ListItem>
                                    <asp:ListItem>Abs</asp:ListItem>
                                    <asp:ListItem>Late</asp:ListItem>
                                    <asp:ListItem>Leave</asp:ListItem>
                                    <asp:ListItem>Bunk</asp:ListItem>
                                </asp:RadioButtonList>
                            </ItemTemplate>
                            <ItemStyle Width="235px" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Comment">
                            <ItemTemplate>
                                <asp:Label ID="AtDateLabel" runat="server" CssClass="Leave_Date"></asp:Label>
                                <asp:TextBox ID="ReasonTextBox" runat="server" CssClass="Dtextbox" Enabled="false" TextMode="MultiLine"></asp:TextBox>
                            </ItemTemplate>
                            <ItemStyle Width="203px" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone" SortExpression="SMSPhoneNo" />
                        <asp:TemplateField HeaderText="SMS">
                            <ItemTemplate>
                                <asp:CheckBox ID="SMSCheckBox" runat="server" Enabled="False" Text=" " />
                            </ItemTemplate>
                            <ItemStyle Width="50px" HorizontalAlign="Center" />
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                </asp:GridView>
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

                <div class="Att_Submit">
                    <asp:Button ID="AttendanceButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="AttendanceButton_Click" ValidationGroup="1" />
                    <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any Student from Student list." ForeColor="Red" ValidationGroup="1"> </asp:CustomValidator>
                    <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                </div>
                <asp:SqlDataSource ID="Attendance_RecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT AttendanceRecordID from Attendance_Record Where StudentClassID = @StudentClassID and AttendanceDate = @AttendanceDate and SchoolID = @SchoolID and EducationYearID = @EducationYearID)
BEGIN
INSERT INTO Attendance_Record(SchoolID, RegistrationID, EducationYearID, StudentID, ClassID, StudentClassID, Attendance, AttendanceDate, Reason) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @StudentID, @ClassID, @StudentClassID, @Attendance, @AttendanceDate, @Reason)
END
ELSE
BEGIN 
UPDATE Attendance_Record SET Attendance = @Attendance
WHERE (StudentClassID = @StudentClassID) AND (AttendanceDate = @AttendanceDate) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)
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
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>


    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
   
    <script>
        function DisableAllKey() {
            return false;
        }

        //Is Gridview is empty
        if ($('[id*=StudentsAttendanceGridView] tr').length) {
            $(".Att_Submit").show();
        }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //Is Gridview is empty
            if ($('[id*=StudentsAttendanceGridView] tr').length) {
                $(".Att_Submit").show();
            }

            //Default SMS
            var eng1 = "Respected Guardian, ";
            var eng2 = ", Today(";
            var eng_Abs = ") absent from class. Please send to class regularly.";
            var eng_Late = ") Late In Class.";
            var eng_Bunk = ") Bunk from class.";

            $("[id*=AttendenceRadioButtonList]").click(function () {
                var td = $("td", $(this).parent().closest("table").closest("tr"));
                var Name = $("[id*=StudentsNameLabel]", td).text();
                var ID = $("[id*=StudentIDHidden]", td).val();
                var Date = $("[id*=AttendanceDateTextBox]").val();
                
                var Name_ID = "" + Name + " ID:" + ID + "";

                if ($(this).val() == "Pre") {
                    $("[id*=ReasonTextBox]", td).val("").attr("disabled", true).removeClass("Etextbox").addClass("Dtextbox");
                    $("[id*=SMSCheckBox]", td).attr('checked', false).attr("disabled", true);
                }

                if ($(this).val() == "Leave") {
                    $("[id*=ReasonTextBox]", td).val("").attr("disabled", false).addClass("Etextbox");
                    $("[id*=SMSCheckBox]", td).attr('checked', false).attr("disabled", false);
                }

                if ($(this).val() == "Abs") {
                    $("[id*=ReasonTextBox]", td).val(eng1 + Name_ID + eng2 + Date + eng_Abs).attr("disabled", false).addClass("Etextbox");
                    $("[id*=SMSCheckBox]", td).attr('checked', true).attr("disabled", false);
                }

                if ($(this).val() == "Late") {
                    $("[id*=ReasonTextBox]", td).val(eng1 + Name_ID + eng2 + Date + eng_Late).attr("disabled", false).addClass("Etextbox");
                    $("[id*=SMSCheckBox]", td).attr('checked', true).attr("disabled", false);
                }

                if ($(this).val() == "Bunk") {
                    $("[id*=ReasonTextBox]", td).val(eng1 + Name_ID + eng2 + Date + eng_Bunk).attr("disabled", false).addClass("Etextbox");
                    $("[id*=SMSCheckBox]", td).attr('checked', true).attr("disabled", false);
                }
            });

            //SMS. Bangla/English
            $("[id*=SMSRadioButtonList]").click(function () {
                if ($(this).val() == "0") {
                    eng1 = "Respected Guardian, ";
                    eng2 = ", Today(";
                    eng_Abs = ") absent from class. Please send to class regularly.";
                    eng_Late = ") Late In Class.";
                    eng_Bunk = ") Bunk from class.";
                }
                if ($(this).val() == "1") {
                    eng1 = "সম্মানিত অভিভাবক, ";
                    eng2 = ", আজ(";
                    eng_Abs = ") ক্লাসে অনুপস্থিত। অনুগ্রহ করে নিয়মিত ক্লাসে পাঠান।";
                    eng_Late = ") ক্লাসে যথাসময় আসেনি।";
                    eng_Bunk = ") ক্লাস থেকে পলায়ন করেছে।";
                }
            });


            //Select All Checkbox
            $("[id*=AllCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("[id*=Attendance_CheckBox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=Attendance_CheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=Attendance_CheckBox]", a).length == $("[id*=Attendance_CheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });

        //Select at least one Checkbox from GridView
        function Validate(d, c) {
            if ($('[id*=StudentsAttendanceGridView] tr').length) {
                for (var b = $('[id*=Attendance_CheckBox]'), a = 0; a < b.length; a++) {
                    if ("checkbox" == b[a].type && b[a].checked) {
                        c.IsValid = !0;
                        return;
                    }
                }
                c.IsValid = !1;
            }
        }
    </script>
</asp:Content>
