<%@ Page Title="Leave for student" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Leave_for_Student.aspx.cs" Inherits="EDUCATION.COM.ATTENDANCES.Leave_for_Student" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid td { font-size: 17px; padding: 12px; text-align: left; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="Contain">
        <h3>Leave for student</h3>
        <div class="form-inline">
            <div class="form-group">
                <asp:TextBox ID="IDTextBox" placeholder="Student ID" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="form-group">
                <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" OnClick="FindButton_Click" />
            </div>
        </div>

        <asp:DetailsView ID="StudentDetailsView" runat="server" AutoGenerateRows="False" DataKeyNames="StudentID" DataSourceID="StudentInfoSQL" AlternatingRowStyle-CssClass="alt" RowStyle-CssClass="RowStyle" BackColor="White" BorderColor="#EBEBEB" BorderStyle="None" BorderWidth="1px" CellPadding="0" ForeColor="Black" GridLines="Horizontal" CssClass="mGrid">
            <AlternatingRowStyle CssClass="alt" />
            <FooterStyle BackColor="#CCCC99" ForeColor="Black" />
            <HeaderStyle BackColor="#333333" Font-Bold="True" ForeColor="White" />
            <PagerStyle BackColor="White" ForeColor="Black" HorizontalAlign="Right" />
            <RowStyle CssClass="RowStyle" />
            <EditRowStyle BackColor="#CC3333" Font-Bold="True" ForeColor="White" />
            <Fields>
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:BoundField DataField="StudentsName" HeaderText="Student's Name" SortExpression="StudentsName" />
                <asp:BoundField DataField="FathersName" HeaderText="Father's Name" SortExpression="FathersName" />
                <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                <asp:BoundField DataField="Section" HeaderText="Section" ReadOnly="True" SortExpression="Section" />
                <asp:BoundField DataField="Group" HeaderText="Group" ReadOnly="True" SortExpression="Group" />
                <asp:BoundField DataField="Shift" HeaderText="Shift" SortExpression="Shift" />
                <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                <asp:TemplateField HeaderText="From Date">
                    <ItemTemplate>
                        <asp:TextBox ID="StartDateTextBox" placeholder="Start Date" runat="server" CssClass="form-control Datepicker" onkeypress="return isNumberKey(event)" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="To Date">
                    <ItemTemplate>
                        <asp:TextBox ID="EndDateTextBox" placeholder="End Date" runat="server" CssClass="form-control Datepicker" onkeypress="return isNumberKey(event)" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Duration">
                    <ItemTemplate>
                        <label class="calculated alert-success"></label>
                        <asp:HiddenField ID="DurationHF" runat="server" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Description">
                    <ItemTemplate>
                        <asp:TextBox ID="DescriptionTextBox" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="SubmitButton_Click" OnClientClick="this.disabled = true; this.value = 'Submitting...';" UseSubmitBehavior="false" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Fields>
        </asp:DetailsView>
        <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, ISNULL(CreateSection.Section, 'No section') AS Section, ISNULL(CreateSubjectGroup.SubjectGroup, 'No Group') AS [Group], CreateShift.Shift, Student.ID, Student.StudentsName, Student.Gender, Student.MothersName, Student.FathersName, Student.SMSPhoneNo, Student.StudentID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)">
            <SelectParameters>
                <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
                <asp:Parameter DefaultValue="Active" Name="Status" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="LeaveSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Attendance_Leave(SchoolID, RegistrationID, StudentID, StartDate, EndDate, Description, EducationYearID) VALUES (@SchoolID, @RegistrationID, @StudentID, @StartDate, @EndDate, @Description, @EducationYearID)" SelectCommand="SELECT StudentLeaveID, SchoolID, RegistrationID, StudentID, StartDate, EndDate, Description FROM Attendance_Leave WHERE (SchoolID = @SchoolID)">
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:Parameter Name="StudentID" Type="Int32" />
                <asp:Parameter DbType="Date" Name="StartDate" />
                <asp:Parameter DbType="Date" Name="EndDate" />
                <asp:Parameter Name="Description" Type="String" />
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
                var firstDate = new Date($('#body_StudentDetailsView_StartDateTextBox').val());
                var secondDate = new Date($('#body_StudentDetailsView_EndDateTextBox').val());

                if (!isNaN(firstDate) && !isNaN(secondDate)) {
                    var diffDays = Math.round(Math.abs((firstDate.getTime() - secondDate.getTime()) / (oneDay)) + 1);

                    $(".calculated").text("Total: " + diffDays + " Day(s)");
                    $("[id=DurationHF]").val(diffDays);
                }
                else {
                    $(".calculated").text('');
                    $("[id=DurationHF]").val('');
                }

            });
        });
    </script>
</asp:Content>
