<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="EDUCATION.COM.Profile.Admin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="/Employee/CSS/Acadamic_Calender.css" rel="stylesheet" />
    <link href="css/Admin.css?v=1.2" rel="stylesheet" />
    <style>
    .AdminNotice a{
    font-size: 16px;
    height: 43px;
    background-color: #fff;
    border-bottom: 1px solid #ddd;
    color: red;
    padding-top: 10px;
    padding-bottom: 10px;
}
    @media (min-width: 1200px) {
    .container {
        max-width: 1160px;
    }
}

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="card my-3 wow fadeIn">
        <div class="card-body d-sm-flex justify-content-between">
            <h4 class="mb-0"><span>Dashboard</span></h4>
            <%if (EmployeeRepeater.Items.Count > 0 || StudentRepeater.Items.Count > 0)
                {%>
            <a target="_blank" href="../Attendances/Online_Display/Attendance_Slider.aspx">
                <i class="fa fa-television" aria-hidden="true"></i>
                Online Attendance Display</a>
            <%} %>
        </div>
    </div>
    <label id="results"></label>
    <div class="row wow fadeIn">
        <div class="col-lg-8 mb-4">
            <!-- Old New Student-->
            <div class="card mb-4">
                <div class="card-header teal darken-4 white-text">
                    <i class="fa fa-pie-chart" aria-hidden="true"></i>
                    Old & New Student of Current Session
                    <a href="Total_Student_List.aspx" title="Print" target="_blank" style="float: right; text-decoration: none;">Total Student:
                        <label id="SGT" class="mb-0"></label>
                        <i class="fa fa-print text-muted" aria-hidden="true"></i>
                    </a>
                </div>
                <div class="card-body pt-0 pb-0">
                    <asp:Repeater runat="server" DataSourceID="OldNewStudentSQL">
                        <HeaderTemplate>
                            <table id="table_total" class="table table-sm">
                                <thead>
                                    <th><i class="fa fa-book" aria-hidden="true"></i>
                                        Class</th>
                                    <th><i class="fa fa-user-plus" aria-hidden="true"></i>
                                        New Student</th>
                                    <th><i class="fa fa-user" aria-hidden="true"></i>
                                        Old Student</th>

                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <th><%#Eval("Class") %></th>
                                <td><%#Eval("New_Student") %></td>
                                <td><%#Eval("Old_Student") %></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:SqlDataSource ID="OldNewStudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, SUM(CAST(StudentsClass.Is_New AS int)) AS New_Student, SUM(CAST(~ StudentsClass.Is_New AS int)) AS Old_Student FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.Status = N'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.SchoolID = @SchoolID) GROUP BY CreateClass.Class, StudentsClass.ClassID ORDER BY StudentsClass.ClassID">
                        <SelectParameters>
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <!--Session chart-->
            <div class="card mb-4" id="SessionBs" style="display: none;">
                <div class="card-header deep-purple darken-2 white-text">
                    <i class="fa fa-area-chart" aria-hidden="true"></i>
                    Session Based Student
                </div>
                <div class="card-body">
                    <canvas id="myChart"></canvas>
                </div>
            </div>


            <!-- Birthday-->
            <%if (TodayBirthdayRepeater.Items.Count > 0)
                {%>
            <asp:Label ID="ErrorLabel" CssClass="EroorStar" runat="server"></asp:Label>
            <div class="card mb-4">
                <div class="card-header bg-primary white-text">
                    <i class="fa fa-birthday-cake" aria-hidden="true"></i>
                    Today Birthday
                    <asp:Button ID="SendButton" OnClick="SendButton_Click" CssClass="btn btn-sm primary-color-dark pull-right m-0" runat="server" Text="Send SMS" />
                </div>
                <asp:Repeater ID="TodayBirthdayRepeater" runat="server" DataSourceID="TodayBirthSQL">
                    <HeaderTemplate>
                        <table class="table table-filter m-0">
                            <tbody class="BirthayHeight">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:HiddenField ID="StudentsName" Value='<%#Eval("StudentsName") %>' runat="server" />
                        <asp:HiddenField ID="SMSPhoneNo" Value='<%#Eval("SMSPhoneNo") %>' runat="server" />
                        <asp:HiddenField ID="StudentID" Value='<%#Eval("StudentID") %>' runat="server" />

                        <tr>
                            <td>
                                <div class="media">
                                    <div class="pull-left mr-2">
                                        <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" class="img-thumbnail rounded-circle z-depth-1" style="width: 80px; height: 80px" />
                                    </div>
                                    <div class="media-body">
                                        <span class="media-meta pull-right badge badge-primary badge-pill">
                                            <%#Eval("Age") %> Years
                                        </span>
                                        <h4 class="title"><%#Eval("StudentsName") %></h4>
                                        <p class="summary">ID: <%#Eval("ID") %>. Class: <%#Eval("Class") %></p>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </tbody></table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:SqlDataSource ID="TodayBirthSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentID,Student.StudentImageID, Student.ID, Student.SMSPhoneNo, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo,DATEDIFF(hour,Student.DateofBirth,GETDATE())/8766 AS Age
FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE (Student.DateofBirth IS NOT NULL) AND (MONTH(Student.DateofBirth) = MONTH(GETDATE())) AND (DAY(Student.DateofBirth) = DAY(GETDATE())) AND (Student.Status = N'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) order by Age">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
                    <InsertParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                        <asp:Parameter Name="StudentID" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </div>
            <%} %>

            <%if (UpComingRepeater.Items.Count > 0)
                {%>
            <div class="card mb-4">
                <div class="card-header brown darken-2 white-text">
                    <i class="fa fa-birthday-cake" aria-hidden="true"></i>
                    Upcoming Birthday
                </div>
                <asp:Repeater ID="UpComingRepeater" runat="server" DataSourceID="UpComingSQL">
                    <HeaderTemplate>
                        <table class="table table-filter m-0">
                            <tbody class="BirthayHeight">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td>
                                <div class="media">
                                    <div class="pull-left mr-2">
                                        <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" class="img-thumbnail rounded-circle z-depth-1" style="width: 80px; height: 80px" />
                                    </div>
                                    <div class="media-body">
                                        <span class="pull-right badge brown p-1 badge-pill d-block">
                                            <%#Eval("DateofBirth","{0:d MMM}") %>, Age Will be <%#Eval("AGE_Will_be") %> Years
                                        </span>

                                        <h4 class="title brown-text"><%#Eval("StudentsName") %></h4>
                                        <p class="summary">ID: <%#Eval("ID") %>. Class: <%#Eval("Class") %></p>
                                    </div>
                                </div>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </tbody></table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:SqlDataSource ID="UpComingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentImageID,Student.ID, Student.SMSPhoneNo, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo, Student.DateofBirth,FLOOR(DATEDIFF(dd,Student.DateofBirth,GETDATE()+7) / 365.25) AS AGE_Will_be
FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE (Student.DateofBirth IS NOT NULL) AND 
	   1 = (FLOOR(DATEDIFF(dd,Student.DateofBirth,GETDATE()+7) / 365.25)) - (FLOOR(DATEDIFF(dd,Student.DateofBirth,GETDATE()+1) / 365.25)) AND 
	   (Student.Status = N'Active') AND 
	   (StudentsClass.SchoolID = @SchoolID) AND 
	   (StudentsClass.EducationYearID = @EducationYearID) order by MONTH(Student.DateofBirth), Day(Student.DateofBirth), AGE_Will_be">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
            <%} %>

            <!-- Calendar-->
            <div class="card mb-4">
                <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
                    <ContentTemplate>
                        <div class="table-responsive" style="overflow-y: hidden !important;">
                            <asp:Calendar ID="HolidayCalendar" CssClass="myCalendar" OnDayRender="HolidayCalendar_DayRender" runat="server" NextMonthText="." PrevMonthText="." SelectMonthText="»" SelectWeekText="›" CellPadding="0" FirstDayOfWeek="Saturday" SelectionMode="None">
                                <DayStyle CssClass="myCalendarDay" />
                                <DayHeaderStyle CssClass="myCalendarDayHeader" />
                                <SelectedDayStyle CssClass="myCalendarSelector" />
                                <SelectorStyle CssClass="myCalendarSelector" />
                                <NextPrevStyle CssClass="myCalendarNextPrev" />
                                <TitleStyle CssClass="myCalendarTitle" />
                            </asp:Calendar>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>

        <div class="col-lg-4 mb-4">
            <!--Male Female -->
            <div class="card mb-4">
                <div class="card-header info-color white-text">
                    <i class="fa fa-users" aria-hidden="true"></i>
                    No. Of Male & Female Student
                </div>
                <div class="card-body">
                    <canvas id="GenderChart"></canvas>
                </div>
            </div>

            <!--Employee Attendance -->
            <%if (EmployeeRepeater.Items.Count > 0)
                {%>
            <div class="card mb-4">
                <div class="card-header teal darken-1 white-text">
                    <a href="/Employee/Employee_Attendance_Record.aspx" target="_blank">
                        <i class="fa fa-bell mr-1" aria-hidden="true"></i>Employee Attendance
                    </a>
                    <small class="pull-right">Today</small>
                </div>
                <div class="card-body">
                    <div class="list-group list-group-flush">
                        <asp:Repeater ID="EmployeeRepeater" runat="server" DataSourceID="EmployeeSQL">
                            <ItemTemplate>
                                <div class="list-group-item">
                                    <%#Eval("AttendanceStatus") %>
                                    <span class="badge badge-pill <%#Eval("AttendanceStatus") %> pull-right"><%#Eval("Total") %> </span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:SqlDataSource ID="EmployeeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AttendanceStatus, COUNT(Employee_Attendance_RecordID) AS Total
FROM    Employee_Attendance_Record
WHERE  (SchoolID = @SchoolID) AND (AttendanceDate = cast(Getdate() as date))
GROUP BY AttendanceStatus
ORDER BY AttendanceStatus DESC">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>
            <%} %>

            <!--Student Attendance -->
            <%if (StudentRepeater.Items.Count > 0)
                {%>
            <div class="card mb-4">
                <div class="card-header deep-purple white-text">
                    <a href="/Attendances/Attendance_Records.aspx" target="_blank">
                        <i class="fa fa-bell mr-1" aria-hidden="true"></i>Student Attendance
                    </a>
                    <small class="pull-right">Today</small>
                </div>
                <div class="card-body">
                    <div class="list-group list-group-flush">
                        <asp:Repeater ID="StudentRepeater" runat="server" DataSourceID="StudentSQL">
                            <ItemTemplate>
                                <div class="list-group-item">
                                    <%#Eval("Attendance") %>
                                    <span class="badge badge-pill <%#Eval("Attendance") %> pull-right"><%#Eval("Total") %> </span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:SqlDataSource ID="StudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance, COUNT(AttendanceRecordID) AS Total
FROM Attendance_Record WHERE (SchoolID = @SchoolID) AND (AttendanceDate = cast(Getdate() as date))
GROUP BY Attendance
ORDER BY Attendance DESC">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>
            <%} %>

            <!--SMS -->
            <div class="card mb-4">
                <div class="card-header bg-success white-text">
                    <i class="fa fa-envelope" aria-hidden="true"></i>
                    SMS
                </div>
                <div class="card-body">
                    <asp:FormView ID="SMSBalanceFormView" runat="server" DataSourceID="SMSBalanceSQL" Width="100%">
                        <ItemTemplate>
                            <div class="row mb-2">
                                <div class="col-sm-6 text-center">
                                    <h4 class="mb-0" style="line-height: 0.5">
                                        <small class="text-success"><%# Eval("SMS_Balance") %></small>
                                    </h4>
                                    <small class="mb-0">REMAINING SMS</small>
                                </div>
                                <div class="col-sm-6 text-center">
                                    <h4 class="mb-0" style="line-height: 0.5">
                                        <small class="text-danger"><%#Eval("Total_SENT") %></small>
                                    </h4>
                                    <small class="mb-0">TOTAL SENT</small>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="SMSBalanceSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SMS.SMS_Balance, ISNULL(SUM(SMS_Send_Record.SMSCount), 0) AS Total_SENT FROM SMS_Send_Record INNER JOIN SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID RIGHT OUTER JOIN SMS ON SMS_OtherInfo.SchoolID = SMS.SchoolID WHERE (SMS.SchoolID = @SchoolID) GROUP BY SMS.SMS_Balance">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <canvas id="doughnutChart"></canvas>
                </div>
            </div>

            <!--Employee -->
            <div class="card mb-4">
                <div class="card-header cyan darken-1 white-text">
                    <i class="fa fa-users" aria-hidden="true"></i>
                    No. Of Employee <span class="badge badge-pill pull-right" id="TEmp"></span>
                </div>
                <div class="card-body">
                    <canvas id="EmployeeChart"></canvas>
                </div>
            </div>

            <!--Blood Group -->
            <div class="card mb-4">
                <div class="card-header bg-danger white-text">
                    <i class="fa fa-tint" aria-hidden="true"></i>
                    Blood Group
                </div>
                <div class="card-body">
                    <asp:Repeater ID="BloodRepeater" DataSourceID="BloodSQL" runat="server">
                        <HeaderTemplate>
                            <ul class="list-group list-group-flush">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                <%#Eval("BloodGroup") %>
                                <span class="badge badge-danger badge-pill"><%#Eval("Total") %></span>
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                    <asp:SqlDataSource ID="BloodSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.BloodGroup,COUNT(StudentsClass.StudentClassID) AS Total FROM  Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = N'Active') GROUP BY Student.BloodGroup">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </div>
    </div>


    <script src="/JS/jquery.tableTotal.js"></script>
    <script>
        $(function () {
            $('#table_total').tableTotal();
            $('#SGT').text($('#table_total tr:last td:last').html())

            //Session
            $.ajax({
                type: "POST",
                url: "Admin.aspx/Get_Session_Student",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    if (Ch_data[0].length > 1) {
                        $('#SessionBs').show();
                        var ctx = document.getElementById("myChart").getContext('2d');

                        var gradientStroke = ctx.createLinearGradient(500, 0, 100, 0);
                        gradientStroke.addColorStop(0, 'rgba(136, 14, 79, .5)');
                        gradientStroke.addColorStop(1, 'rgba(49, 27, 146, .6)');

                        var gradientFill = ctx.createLinearGradient(600, 0, 100, 0);
                        gradientFill.addColorStop(0, "rgba(136, 14, 79, .5)");
                        gradientFill.addColorStop(1, "rgba(49, 27, 146, .6)");


                        var myChart = new Chart(ctx, {
                            type: 'line',
                            data: {
                                labels: Ch_data[0],
                                datasets: [{
                                    label: "Session Based Student",
                                    data: Ch_data[1],
                                    fill: true,
                                    backgroundColor: gradientFill,
                                    borderWidth: 2,
                                    borderColor: gradientStroke,
                                    pointBorderColor: gradientStroke,
                                    pointBackgroundColor: gradientStroke,
                                    pointHoverBackgroundColor: gradientStroke,
                                    pointHoverBorderColor: gradientStroke,

                                }]
                            },
                            options: {
                                legend: {
                                    position: "bottom"
                                },

                            }
                        });
                    }
                },
                failure: function (r) {
                    console.log(r.d);
                },
                error: function (r) {
                    console.log(r.d);
                }
            });

            //Gender
            $.ajax({
                type: "POST",
                url: "Admin.aspx/Get_Gender",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    if (Ch_data[0].length > 0) {
                        var ctx = document.getElementById("GenderChart").getContext('2d');
                        var myChart = new Chart(ctx, {
                            type: 'pie',
                            data: {
                                labels: Ch_data[0],
                                datasets: [
                                    {
                                        data: Ch_data[1],
                                        backgroundColor: ["#26a69a", "#00e5ff"],
                                    }
                                ]
                            },
                            options: {
                                responsive: true
                            }
                        });
                    }
                },
                failure: function (r) {
                    console.log(r.d);
                },
                error: function (r) {
                    console.log(r.d);
                }
            });

            //SMS
            $.ajax({
                type: "POST",
                url: "Admin.aspx/Get_SentSMS",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    if (Ch_data[0].length > 0) {
                        var ctxD = document.getElementById("doughnutChart").getContext('2d');
                        var myLineChart = new Chart(ctxD, {
                            type: 'doughnut',
                            data: {
                                labels: Ch_data[0],
                                datasets: [
                                    {
                                        data: Ch_data[1],
                                        backgroundColor: [
                                            '#4bc0c0',
                                            '#36a2eb',
                                            '#ffcd56',
                                            '#69f0ae',
                                            '#ff6384',
                                            '#ff9f40',
                                            'rgba(128,100,161,1)',
                                            'rgba(74,172,197,1)',
                                            'rgba(247,150,71,1)',
                                            'rgba(127,96,132,1)',
                                            'rgba(119,160,51,1)',
                                            'rgba(51,85,139,1)'

                                        ],
                                    }
                                ]
                            },
                            options: {
                                responsive: true
                            }
                        });
                    }
                },
                failure: function (r) {
                    console.log(r.d);
                },
                error: function (r) {
                    console.log(r.d);
                }
            });

            //Employee
            $.ajax({
                type: "POST",
                url: "Admin.aspx/Get_Employee",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;

                    if (Ch_data[0].length > 0) {
                        var total = 0;
                        total = Ch_data[1].reduce(function (a, b) {
                            return parseInt(a, 10) + parseInt(b, 10);
                        });
                        $("#TEmp").text("Total " + total);

                        var ctxD = document.getElementById("EmployeeChart").getContext('2d');
                        var myLineChart = new Chart(ctxD, {
                            type: 'pie',
                            data: {
                                labels: Ch_data[0],
                                datasets: [
                                    {
                                        data: Ch_data[1],
                                        backgroundColor: ['#9575cd', '#26c6da', '#d81b60', '#64b5f6', '#69f0ae'],
                                    }
                                ]
                            },
                            options: {
                                responsive: true
                            }
                        });
                    }
                },
                failure: function (r) {
                    console.log(r.d);
                },
                error: function (r) {
                    console.log(r.d);
                }
            });
        });

        // provide data in chart labels
        Chart.plugins.register({
            afterDatasetsDraw: function (chart) {
                var ctx = chart.ctx;

                chart.data.datasets.forEach(function (dataset, i) {
                    var meta = chart.getDatasetMeta(i);
                    if (!meta.hidden) {
                        meta.data.forEach(function (element, index) {
                            // Draw the text in black, with the specified font
                            ctx.fillStyle = '#000';

                            var fontSize = 11;
                            var fontStyle = 'normal';
                            var fontFamily = 'tahoma';
                            ctx.font = Chart.helpers.fontString(fontSize, fontStyle, fontFamily);

                            // Just naively convert to string for now
                            var dataString = dataset.data[index].toString();

                            // Make sure alignment settings are correct
                            ctx.textAlign = 'center';
                            ctx.textBaseline = 'middle';

                            var padding = 3;
                            var position = element.tooltipPosition();
                            ctx.fillText(dataString, position.x, position.y - (fontSize / 2) - padding);
                        });
                    }
                });
            }
        });
    </script>
</asp:Content>
