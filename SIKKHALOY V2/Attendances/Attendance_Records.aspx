<%@ Page Title="Attendance Record" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Attendance_Records.aspx.cs" Inherits="EDUCATION.COM.ATTENDANCES.Attendance_Records" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Attendance_Records.css?v=1" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="contain">
        <h3>Student Attendance Record(s)
		 <label class="Date"></label>
        </h3>

        <asp:UpdatePanel ID="UpdatePanel3" runat="server">
            <ContentTemplate>
                <div class="form-inline NoPrint">
                    <div class="form-group">
                        <asp:DropDownList ID="AttenDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" OnSelectedIndexChanged="AttenDropDownList_SelectedIndexChanged">
                            <asp:ListItem Value="%">Attendance</asp:ListItem>
                            <asp:ListItem Value="Pre">Present</asp:ListItem>
                            <asp:ListItem>Late</asp:ListItem>
                            <asp:ListItem>Late Abs</asp:ListItem>
                            <asp:ListItem Value="Abs">Absence</asp:ListItem>
                            <asp:ListItem>Bunk</asp:ListItem>
                            <asp:ListItem>Leave</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                            <asp:ListItem Value="0">All class</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ClassID, SchoolID, RegistrationID, Class FROM CreateClass WHERE (SchoolID = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <div class="form-group">
                        <asp:DropDownList ID="GroupDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%')">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <div class="form-group">
                        <asp:DropDownList ID="SectionDropDownList" runat="server" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" AutoPostBack="True" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
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
                        <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ClassID = @ClassID)">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                    <div class="form-group">
                        <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control Datetime" placeholder="From Date"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control Datetime" placeholder="To Date"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="SubmitButton_Click" />
                        <asp:Label ID="AttendanceCountLabel" runat="server" CssClass="d-none"></asp:Label>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

        <ul class="nav nav-tabs z-depth-1">
            <li class="nav-item"><a class="nav-link active" href="#tab1" data-toggle="tab" role="tab" aria-expanded="true">Attendance Record</a></li>
            <li class="nav-item"><a class="nav-link" href="#tab2" data-toggle="tab" role="tab" aria-expanded="false">Attendance Summary</a></li>
        </ul>

        <div class="tab-content card">
            <div id="tab1" class="tab-pane fade in active show" role="tabpanel" aria-expanded="true">
                <label class="Class_Info"></label>
                <div class="table-responsive">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                        <ContentTemplate>
                            <asp:GridView ID="AttendanceGridView" ShowHeaderWhenEmpty="True" EmptyDataText="No Records Found!" runat="server" AutoGenerateColumns="False" DataSourceID="AttendanceSQL" CssClass="mGrid" AllowPaging="True" AllowSorting="True" PageSize="60" OnRowDataBound="AttendanceGridView_RowDataBound">
                                <Columns>
                                    <asp:HyperLinkField SortExpression="ID" Target="_blank" DataNavigateUrlFields="StudentID,StudentClassID" DataTextField="ID" HeaderText="ID"
                                        DataNavigateUrlFormatString="/Admission/Student_Report/Report.aspx?Student={0}&Student_Class={1}" />
                                    <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone" SortExpression="SMSPhoneNo" />
                                    <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                                    <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
                                    <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                                    <asp:BoundField DataField="Attendance" HeaderText="Attendance" SortExpression="Attendance" />
                                    <asp:BoundField DataField="AttendanceDate" HeaderText="Attendance Date" SortExpression="AttendanceDate" DataFormatString="{0:d MMM yyyy}" />
                                    <asp:BoundField DataField="EntryTime" HeaderText="Entry Time" SortExpression="EntryTime" />
                                    <asp:BoundField DataField="ExitTime" HeaderText="Exit Time" SortExpression="ExitTime" />
                                    <asp:BoundField DataField="ExitStatus" HeaderText="Exit Status" SortExpression="ExitStatus" />
                                    <asp:BoundField DataField="Reason" HeaderText="Comment" SortExpression="Reason">
                                        <HeaderStyle CssClass="NoPrint" />
                                        <ItemStyle CssClass="NoPrint" />
                                    </asp:BoundField>
                                </Columns>
                                <PagerSettings FirstPageText="First" LastPageText="Last" Mode="NumericFirstLast" NextPageText="Next" PreviousPageText="Previous" />
                                <PagerStyle CssClass="pgr" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="AttendanceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT Attendance_Record.AttendanceDate, Attendance_Record.Reason, CreateClass.Class, StudentsClass.RollNo, Student.StudentsName, Student.SMSPhoneNo, Student.ID, CONVERT (varchar(15), Attendance_Record.EntryTime, 100) AS EntryTime, CONVERT (varchar(15), Attendance_Record.ExitTime, 100) AS ExitTime, Attendance_Record.Attendance, Attendance_Record.StudentClassID, Attendance_Record.StudentID, Attendance_Record.ExitStatus FROM Attendance_Record INNER JOIN CreateClass ON Attendance_Record.ClassID = CreateClass.ClassID INNER JOIN StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID WHERE (Student.Status = 'Active') AND (Attendance_Record.Attendance LIKE @Attendance) AND (Attendance_Record.SchoolID = @SchoolID) AND (Attendance_Record.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) OR (Student.Status = 'Active') AND (Attendance_Record.Attendance LIKE @Attendance) AND (Attendance_Record.SchoolID = @SchoolID) AND (Attendance_Record.EducationYearID = @EducationYearID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (@ClassID = 0) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo , '$' , '') , ',' , '') AS INT) ELSE 0 END"
                                FilterExpression="AttendanceDate >= '{0}' AND AttendanceDate <= '{1}'">
                                <FilterParameters>
                                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To" PropertyName="Text" />
                                </FilterParameters>
                                <SelectParameters>
                                    <asp:ControlParameter ControlID="AttenDropDownList" Name="Attendance" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="SectionDropDownList" DefaultValue="%" Name="SectionID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ShiftDropDownList" DefaultValue="%" Name="ShiftID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="GroupDropDownList" DefaultValue="%" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
            <div id="tab2" class="tab-pane fade" role="tabpanel" aria-expanded="false">
                <label class="Att_Summary"></label>
                <div class="table-responsive">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <asp:GridView ID="Summery_GridView" AllowPaging="True" AllowSorting="True" PageSize="100" runat="server" AutoGenerateColumns="False" CssClass="mGrid TSummery" DataSourceID="SummerySQL">
                                <Columns>
                                    <asp:HyperLinkField SortExpression="ID" Target="_blank" DataNavigateUrlFields="StudentID,StudentClassID" DataTextField="ID" HeaderText="ID"
                                        DataNavigateUrlFormatString="/Admission/Student_Report/Report.aspx?Student={0}&Student_Class={1}" />
                                    <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                                    <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone" SortExpression="SMSPhoneNo" />
                                    <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" />
                                    <asp:BoundField DataField="WorkingDay" HeaderText="W.D" SortExpression="WorkingDay" />
                                    <asp:BoundField DataField="Pre" HeaderText="Pre" SortExpression="Pre" />
                                    <asp:BoundField DataField="Abs" HeaderText="Abs" SortExpression="Abs" />
                                    <asp:BoundField DataField="Late" HeaderText="Late" SortExpression="Late" />
                                    <asp:BoundField DataField="LateAbs" HeaderText="Late Abs" SortExpression="LateAbs" />
                                    <asp:BoundField DataField="Leave" HeaderText="Leave" ReadOnly="True" SortExpression="Leave" />
                                </Columns>
                                <EmptyDataTemplate>
                                    Select Class To display summary
                                </EmptyDataTemplate>
                                <PagerStyle CssClass="pgr" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="SummerySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT   StudentsClass.StudentID,StudentsClass.StudentClassID,Student.SMSPhoneNo, Student.StudentsName, StudentsClass.RollNo, Student.ID,
														 dbo.F_Stu_WorkingDay(StudentsClass.SchoolID, StudentsClass.EducationYearID, StudentsClass.ClassID, @From_Date, @To_Date)as WorkingDay,
			 dbo.F_Stu_Attendance_Summary( StudentsClass.SchoolID, StudentsClass.EducationYearID, StudentsClass.StudentClassID, 'Pre', @From_Date, @To_Date)  AS Pre,
			 dbo.F_Stu_Attendance_Summary( StudentsClass.SchoolID, StudentsClass.EducationYearID,  StudentsClass.StudentClassID, 'Abs', @From_Date, @To_Date) AS Abs,
			 dbo.F_Stu_Attendance_Summary( StudentsClass.SchoolID, StudentsClass.EducationYearID, StudentsClass.StudentClassID, 'Late', @From_Date, @To_Date)  AS Late,
			 dbo.F_Stu_Attendance_Summary( StudentsClass.SchoolID, StudentsClass.EducationYearID, StudentsClass.StudentClassID, 'Leave', @From_Date, @To_Date)  AS Leave,
			 dbo.F_Stu_Attendance_Summary( StudentsClass.SchoolID, StudentsClass.EducationYearID, StudentsClass.StudentClassID, 'Bunk', @From_Date, @To_Date)  AS Bunk,
			 dbo.F_Stu_Attendance_Summary( StudentsClass.SchoolID, StudentsClass.EducationYearID, StudentsClass.StudentClassID, 'Late Abs', @From_Date, @To_Date)  AS LateAbs FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo , '$' , '') , ',' , '') AS FLOAT) ELSE 0 END"
                                CancelSelectOnNullParameter="False">
                                <SelectParameters>
                                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" DefaultValue="%" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" DefaultValue="%" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" DefaultValue="%" PropertyName="SelectedValue" />
                                    <asp:Parameter DefaultValue="Active" Name="Status" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>

        <button type="button" class="btn btn-primary d-print-none" onclick="window.print()">Print</button>
    </div>

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
        $(function () {
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //get date in label
            var from = $("[id*=FromDateTextBox]").val();
            var To = $("[id*=ToDateTextBox]").val();

            var tt;
            var Brases1 = "";
            var Brases2 = "";
            var A = "";
            var B = "";
            var TODate = "";

            if (To == "" || from == "" || To == "" && from == "") {
                tt = "";
                A = "";
                B = "";
            }
            else {
                tt = " To ";
                Brases1 = "(";
                Brases2 = ")";
            }

            if (To == "" && from == "") { Brases1 = ""; }

            if (To == from) {
                TODate = "";
                tt = "";
                var Brases1 = "";
                var Brases2 = "";
            }
            else { TODate = To; }

            if (from == "" && To != "") {
                B = " Before ";
            }

            if (To == "" && from != "") {
                A = " After ";
            }

            if (from != "" && To != "") {
                A = "";
                B = "";
            }

            var AttCount = $('[id*=AttendanceCountLabel]').text();
            var Attendance = $('[id*=AttenDropDownList] :selected').text();
            var Class = $('[id*=ClassDropDownList] :selected').text();

            var group = "";
            if ($('[id*=GroupDropDownList] :selected').index() > 0) {
                group = ". Group: " + $('[id*=GroupDropDownList] :selected').text();
            }

            var Section = "";
            if ($('[id*=SectionDropDownList] :selected').index() > 0) {
                Section = ". Section: " + $('[id*=SectionDropDownList] :selected').text();
            }

            var Shift = "";
            if ($('[id*=ShiftDropDownList] :selected').index() > 0) {
                Shift = ". Shift: " + $('[id*=ShiftDropDownList] :selected').text();
            }


            $(".Class_Info").text(AttCount + " " + Attendance + " At " + Class + group + Section + Shift);

            if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                $('.Att_Summary').text("Summary For Class " + Class + group + Section + Shift);
            }
            $(".Date").text(Brases1 + B + A + from + tt + TODate + Brases2);
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $(".TSummery tbody").find('tr').each(function () {
                var tds = $(this).find('td'),
                    w = parseFloat(tds.eq(4).text()),
                    p = parseFloat(tds.eq(5).text()),
                    a = parseFloat(tds.eq(6).text()),
                    l = parseFloat(tds.eq(7).text()),
                    la = parseFloat(tds.eq(8).text())

                tds.eq(5).append(" (" + ((p * 100) / w).toFixed(2) + "%)");
                tds.eq(6).append(" (" + ((a * 100) / w).toFixed(2) + "%)");
                tds.eq(7).append(" (" + ((l * 100) / w).toFixed(2) + "%)");
                tds.eq(8).append(" (" + ((la * 100) / w).toFixed(2) + "%)");
            });

            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //get date in label
            var from = $("[id*=FromDateTextBox]").val();
            var To = $("[id*=ToDateTextBox]").val();

            var tt;
            var Brases1 = "";
            var Brases2 = "";
            var A = "";
            var B = "";
            var TODate = "";

            if (To == "" || from == "" || To == "" && from == "") {
                tt = "";
                A = "";
                B = "";
            }
            else {
                tt = " To ";
                Brases1 = "(";
                Brases2 = ")";
            }

            if (To == "" && from == "") { Brases1 = ""; }

            if (To == from) {
                TODate = "";
                tt = "";
                var Brases1 = "";
                var Brases2 = "";
            }
            else { TODate = To; }

            if (from == "" && To != "") {
                B = " Before ";
            }

            if (To == "" && from != "") {
                A = " After ";
            }

            if (from != "" && To != "") {
                A = "";
                B = "";
            }

            var AttCount = $('[id*=AttendanceCountLabel]').text();
            var Attendance = $('[id*=AttenDropDownList] :selected').text();
            var Class = $('[id*=ClassDropDownList] :selected').text();

            var group = "";
            if ($('[id*=GroupDropDownList] :selected').index() > 0) {
                group = ". Group: " + $('[id*=GroupDropDownList] :selected').text();
            }

            var Section = "";
            if ($('[id*=SectionDropDownList] :selected').index() > 0) {
                Section = ". Section: " + $('[id*=SectionDropDownList] :selected').text();
            }

            var Shift = "";
            if ($('[id*=ShiftDropDownList] :selected').index() > 0) {
                Shift = ". Shift: " + $('[id*=ShiftDropDownList] :selected').text();
            }


            $(".Class_Info").text(AttCount + " " + Attendance + " At " + Class + group + Section + Shift);

            if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                $('.Att_Summary').text("Summary For Class " + Class + group + Section + Shift);
            }
            $(".Date").text(Brases1 + B + A + from + tt + TODate + Brases2);
        });
    </script>
</asp:Content>
