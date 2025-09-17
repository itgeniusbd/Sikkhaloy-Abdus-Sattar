<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Sub-Admin.aspx.cs" Inherits="EDUCATION.COM.Profile.Sub_Admin" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="/Employee/CSS/Acadamic_Calender.css" rel="stylesheet" />
    <style>
        .card h4 { color: #777; }
        .card-header > a { color: #777; }
        .card-header > a:hover { color: #0094ff }
        body { background-color: #f0f1f3 !important; }

        .Pre { background-color: #00a12a; }
        .Abs { background-color: #CC0000; }
        .Leave { background-color: #0099CC; }
       .Late.Abs {background-color:#FF4F29} 
        .Late { background-color: #FF8800; }
        .Bunk { background-color: #f4511e; }
        .BirthayHeight { max-height: 300px; overflow: hidden; }
            .BirthayHeight:hover { overflow: auto; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="card mb-4 wow fadeIn">
        <div class="card-body d-sm-flex justify-content-between">
            <h4 class="mb-2 mb-sm-0">
                <span>Dashboard</span>
            </h4>
        </div>
    </div>

    <div class="row wow fadeIn">
        <div class="col-lg-8 mb-4">
            <div class="card mb-4">
                <div class="card-header">
                    <div class="row align-middle">
                        <div class="col-sm-8">
                            <asp:DropDownList ID="EduYearDropDownList" runat="server" CssClass="form-control" DataSourceID="EduYearSQL" DataTextField="EducationYear" DataValueField="EducationYearID"></asp:DropDownList>
                            <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYearID, ('Session: ' + EducationYear) AS EducationYear FROM Education_Year WHERE (SchoolID = @SchoolID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>

                        <div class="col-sm-4 text-center mt-1">
                            <h5 class="mb-0" style="line-height: 0.5">
                                <small id="Tstudent"></small>
                            </h5>
                            <small class="text-muted mb-0">STUDENT</small>
                        </div>
                    </div>
                </div>
                <div class="card-body">
                    <canvas id="myChart"></canvas>
                </div>
            </div>
            <div class="card mb-4">
                <asp:Repeater ID="TodayBirthdayRepeater" runat="server" DataSourceID="TodayBirthSQL">
                    <HeaderTemplate>
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item list-group-item-action flex-column align-items-start active">Today Birthday</li>
                            <div class="BirthayHeight">
                    </HeaderTemplate>
                    <ItemTemplate>

                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" class="img-thumbnail" style="width: 70px; height: 75px" />
                            <div class="mx-2"><%#Eval("StudentsName") %> </div>
                            <small style="color: #f4511e; white-space: nowrap">Just turned <%# Eval("Age") %> years old</small>
                            <span class="badge badge-success badge-pill"><%# Eval("ID") %>: <%# Eval("Class") %></span>
                        </li>

                    </ItemTemplate>
                    <FooterTemplate>
                        </div> </ul>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:SqlDataSource ID="TodayBirthSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentImageID, Student.ID, Student.SMSPhoneNo, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo,DATEDIFF(hour,Student.DateofBirth,GETDATE())/8766 AS Age
FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE (Student.DateofBirth IS NOT NULL) AND (MONTH(Student.DateofBirth) = MONTH(GETDATE())) AND (DAY(Student.DateofBirth) = DAY(GETDATE())) AND (Student.Status = N'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="card mb-4">
                <asp:Repeater ID="UpComingRepeater" runat="server" DataSourceID="UpComingSQL">
                    <HeaderTemplate>
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item list-group-item-action flex-column align-items-start bg-warning white-text">Upcoming Birthday</li>
                            <div class="BirthayHeight">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="list-group-item d-flex justify-content-between align-items-center">
                            <img src="/Handeler/Student_Photo.ashx?SID=<%# Eval("StudentImageID") %>" class="img-thumbnail" style="width: 70px; height: 75px" />
                            <div class="mx-2"><%#Eval("StudentsName") %> </div>
                            <small style="color: #4100bb; white-space: nowrap"><%# Eval("DateofBirth","{0:d MMM}") %></small>
                            <span class="badge badge-secondary badge-pill"><%# Eval("ID") %>: <%# Eval("Class") %></span>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </div></ul>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:SqlDataSource ID="UpComingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentImageID, Student.ID, Student.SMSPhoneNo, Student.StudentsName, CreateClass.Class, StudentsClass.RollNo, Student.DateofBirth,DATEDIFF(hour,Student.DateofBirth,GETDATE())/8766 AS Age
FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE (Student.DateofBirth IS NOT NULL) AND DATEPART( WEEK, DATEADD( Year, DATEPART( Year, GETDATE()) - DATEPART( Year, Student.DateofBirth ), Student.DateofBirth ))= DATEPART( WEEK, GETDATE()) AND (Student.Status = N'Active') AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) order by MONTH(DateofBirth), Day(DateofBirth)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
            <div class="card mb-4">
                <div class="card-body">
                    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
                        <ContentTemplate>
                            <div class="table-responsive" style="overflow-y: hidden !important">
                                <asp:Calendar ID="HolidayCalendar" CssClass="myCalendar" OnDayRender="HolidayCalendar_DayRender" runat="server" NextMonthText="." PrevMonthText="." SelectMonthText="»" SelectWeekText="›" CellPadding="0" FirstDayOfWeek="Saturday">
                                    <DayStyle CssClass="myCalendarDay" />
                                    <DayHeaderStyle CssClass="myCalendarDayHeader" />
                                    <SelectedDayStyle CssClass="myCalendarSelector" />
                                    <TodayDayStyle CssClass="myCalendarToday" />
                                    <SelectorStyle CssClass="myCalendarSelector" />
                                    <NextPrevStyle CssClass="myCalendarNextPrev" />
                                    <TitleStyle CssClass="myCalendarTitle" />
                                </asp:Calendar>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
        <div class="col-lg-4 mb-4">
            <div class="card mb-4">
                <div class="card-header">
                    Male & Female Student
                </div>
                <div class="card-body">
                    <canvas id="GenderChart"></canvas>
                </div>
            </div>
            <div class="card mb-4">
                <div class="card-header">
                    <a href="/Employee/Employee_Attendance_Record.aspx" target="_blank">
                        <i class="fa fa-users mr-1" aria-hidden="true"></i>Employee Attendance
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
                            <FooterTemplate>
                                <asp:Label ID="Emp" runat="server" Visible='<%# EmployeeRepeater.Items.Count == 0 %>' Text="No attendance today" />
                            </FooterTemplate>
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

            <div class="card mb-4">
                <div class="card-header">
                    <a href="/Attendances/Attendance_Records.aspx" target="_blank">
                        <i class="fa fa-users mr-1" aria-hidden="true"></i>Student Attendance
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
                            <FooterTemplate>
                                <asp:Label ID="Stu" runat="server" Visible='<%# StudentRepeater.Items.Count == 0 %>' Text="No attendance today" />
                            </FooterTemplate>
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

            <div class="card mb-4">
                <div class="card-header text-center">SMS</div>
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
        </div>
    </div>


    <!-- Charts -->
    <script>
        $(function () {
            var sy = $("[id*=Session_DropDownList] option:selected").val();
            $('[id*=EduYearDropDownList] option[value="' + sy + '"]').attr('selected', true);

            //Bar chart
            ClassChart();

            //doughnut
            $.ajax({
                type: "POST",
                url: "Sub-Admin.aspx/Get_SentSMS",
                data: '{}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
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
                },
                failure: function (r) {
                    alert(r.d);
                },
                error: function (r) {
                    alert(r.d);
                }
            });
        });

        $("[id*=EduYearDropDownList]").on("change", function () {
            ClassChart();
        });

        var myChart;
        var myChart2;
        function ClassChart() {
            var YearID = $("[id*=EduYearDropDownList] option:selected").val();
            var Year = $("[id*=EduYearDropDownList] option:selected").text();

            $.ajax({
                type: "POST",
                url: "Sub-Admin.aspx/Get_Class_Student",
                data: '{EducationYearID:' + YearID + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    var total = 0;
                    total = Ch_data[1].reduce(function (a, b) {
                        return parseInt(a, 10) + parseInt(b, 10);
                    });

                    $("#Tstudent").text(total);

                    if (myChart) {
                        myChart.destroy();
                    }
                    var ctx = document.getElementById("myChart").getContext('2d');
                    myChart = new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: Ch_data[0],
                            datasets: [{
                                label: Year,
                                data: Ch_data[1],
                                backgroundColor: [
                                    'rgba(79,129,188,.6)',
                                    'rgba(192,80,78,.6)',
                                    'rgba(155,187,88,.6)',
                                    'rgba(35,191,170,.6)',
                                    'rgba(128,100,161,.6)',
                                    'rgba(74,172,197,.6)',
                                    'rgba(247,150,71,.6)',
                                    'rgba(127,96,132,.6)',
                                    'rgba(119,160,51,.6)',
                                    'rgba(51,85,139,.6)',
                                    'rgba(229,149,102,.6)',
                                    'rgba(79,129,188,.6)',
                                    'rgba(192,80,78,.6)',

                                    'rgba(79,129,188,.6)',
                                    'rgba(192,80,78,.6)',
                                    'rgba(155,187,88,.6)',
                                    'rgba(35,191,170,.6)',
                                    'rgba(128,100,161,.6)',
                                    'rgba(74,172,197,.6)',
                                    'rgba(247,150,71,.6)',
                                    'rgba(127,96,132,.6)',
                                    'rgba(119,160,51,.6)',
                                    'rgba(51,85,139,.6)',
                                    'rgba(229,149,102,.6)',
                                    'rgba(79,129,188,.6)',
                                    'rgba(192,80,78,.6)'
                                ],
                                borderColor: [
                                    'rgba(79,129,188,.9)',
                                    'rgba(192,80,78,.9)',
                                    'rgba(155,187,88,.9)',
                                    'rgba(35,191,170,.9)',
                                    'rgba(128,100,161,.9)',
                                    'rgba(74,172,197,.9)',
                                    'rgba(247,150,71,.9)',
                                    'rgba(127,96,132,.9)',
                                    'rgba(119,160,51,.9)',
                                    'rgba(51,85,139,.9)',
                                    'rgba(229,149,102,.9)',
                                    'rgba(79,129,188,.9)',
                                    'rgba(192,80,78,.9)',

                                    'rgba(79,129,188,.9)',
                                    'rgba(192,80,78,.9)',
                                    'rgba(155,187,88,.9)',
                                    'rgba(35,191,170,.9)',
                                    'rgba(128,100,161,.9)',
                                    'rgba(74,172,197,.9)',
                                    'rgba(247,150,71,.9)',
                                    'rgba(127,96,132,.9)',
                                    'rgba(119,160,51,.9)',
                                    'rgba(51,85,139,.9)',
                                    'rgba(229,149,102,.9)',
                                    'rgba(79,129,188,.9)',
                                    'rgba(192,80,78,.9)'
                                ],
                                borderWidth: 1
                            }]
                        },
                        options: {
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true
                                    }
                                }],
                                xAxes: [{
                                    ticks: {
                                        autoSkip: false
                                    }
                                }]
                            }
                        }
                    });
                },
                failure: function (r) {
                    alert(r.d);
                },
                error: function (r) {
                    alert(r.d);
                }
            });

            //Gender
            $.ajax({
                type: "POST",
                url: "Sub-Admin.aspx/Get_Gender",
                data: '{EducationYearID:' + YearID + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    var total = 0;
                    total = Ch_data[1].reduce(function (a, b) {
                        return parseInt(a, 10) + parseInt(b, 10);
                    });

                    $("#Tstudent").text(total);

                    if (myChart2) {
                        myChart2.destroy();
                    }
                    var ctx = document.getElementById("GenderChart").getContext('2d');
                    myChart2 = new Chart(ctx, {
                        type: 'pie',
                        data: {
                            labels: Ch_data[0],
                            datasets: [
                                {
                                    data: Ch_data[1],
                                    backgroundColor: ["#ffc400", "#00e5ff"],
                                }
                            ]
                        },
                        options: {
                            responsive: true
                        }
                    });
                },
                failure: function (r) {
                    alert(r.d);
                },
                error: function (r) {
                    alert(r.d);
                }
            });
        }

        // Define a plugin to provide data labels
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
