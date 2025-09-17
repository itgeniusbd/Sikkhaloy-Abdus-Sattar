<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Student_Profile.aspx.cs" Inherits="EDUCATION.COM.Student.Student_Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        body { background-color: #f0f1f3; }
        .stats-left {white-space:nowrap; float: left; width: 60%; background-color: #4F52BA; padding: 1.20rem 0; }
        .stats-right { float: right; width: 40%; text-align: center; padding: 1.79rem 0; background-color: rgba(79, 82, 186, 0.88); }

        .states-mdl .stats-left { background-color: #585858; }
        .states-mdl .stats-right { background-color: rgba(88, 88, 88, 0.88); }

        .states-last .stats-left { background-color: #e94e02; }
        .states-last .stats-right { background-color: rgba(233, 78, 2, 0.84); }

        .stats-left h4 { font-size: 0.9rem; color: #fff; font-weight: 500; padding-left: 7px; }
        .stats-right label { font-size: 1rem; color: #fff; font-weight: bold; }

        /** Progressbar class css*/
        .progressbar { width: 100%; position: relative; }
        .proggress { height: 8px; width: 10px; background-color: #3498db; }
        .percentCount { margin-right: 5px; float: right; clear: both; font-weight: bold; font-size: 0.9rem; color: #fff; }

        .card-header { background-color: #fff; }
        .Relation { color: #00a12a; }
        .gimg { width: 160px; display: block; margin: auto; }

        /*Allover report*/
        .statistic { white-space: nowrap; overflow: hidden; padding: 20px 2px 20px 10px; margin-bottom: 15px; margin-right: 8px; }
        .has-shadow { -webkit-box-shadow: 2px 2px 2px rgba(0,0,0,0.1),-1px 0 2px rgba(0,0,0,0.05); box-shadow: 2px 2px 2px rgba(0,0,0,0.1),-1px 0 2px rgba(0,0,0,0.05); }
        .icon { width: 40px; height: 40px; line-height: 40px; text-align: center; min-width: 40px; max-width: 40px; color: #fff; border-radius: 50%; margin-right: 10px; }
        .statistic strong { font-size: 1.5em; color: #333; font-weight: 700; line-height: 1; }
        .statistic small { color: #707070; text-transform: uppercase; }

        .statistic2 { white-space: nowrap; overflow: hidden; padding: 20px 2px 20px 10px; margin-bottom: 15px; }
        .statistic2 strong { font-size: 1.1em; color: #333; font-weight: 700; line-height: 1; }
        .statistic2 small { color: #818181; font-size: 1rem; text-transform: uppercase; }

        .nav-tabs { background-color: #25b79d; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
<div class="row" style="display:flex; margin:0">
    <h3 style="height: 55px; padding: 0; width: 100%; padding-left: 16px;">Dashboard
<%--    <span id="spanBankingMsg" style="margin-left: 2%; font-weight: bold;"> আমরা সকল মোবাইল ব্যাংকিংসহ ন্যাশনাল এবং ইন্টারন্যাশনাল কার্ড পেমেন্ট গ্রহন করি</span>--%>
    <button id="btnPayOnline" type="button" class="btn btn-danger" style="height: 42px; font-size: 14px; border-radius: 5px; font-weight: bold; padding: 0 25px;"
        OnClick="ViewDueClick()">
        Pay Online</button>
    </h3>
</div>
    <div class="row">
        <div class="col-lg-3">
            <div class="card mb-3">
                <asp:FormView ID="GuardianFormView" Width="100%" runat="server" DataSourceID="GuardianSQL">
                    <ItemTemplate>
                        <div class="card-header text-center"><i class="fa fa-user-o mr-1" aria-hidden="true"></i>Guardian
                            <small class="Relation"><%# Eval("GuardianRelationshipwithStudent") %></small></div>
                        <div class="card-body">
                            <img class="gimg img-thumbnail mb-2 z-depth-1" src="/Handeler/Guardian_Photo.ashx?SID=<%# Eval("StudentImageID") %>" />
                            <h6 class="text-center"><%# Eval("GuardianName") %></h6>
                        </div>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="GuardianSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT GuardianRelationshipwithStudent, GuardianName, FathersName, MothersName, StudentsName, StudentImageID FROM Student WHERE (StudentID = @StudentID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <asp:FormView ID="BestSubFormView" Width="100%" DataSourceID="BestSubSQL" runat="server">
                <ItemTemplate>
                    <div class="statistic2 d-flex align-items-center bg-white has-shadow mr-0">
                        <div class="icon bg-success"><i class="fa fa-thumbs-up"></i></div>
                        <div class="text">
                            <strong><%# Eval("SubjectName") %></strong><br>
                            <small><%# Eval("Top_Sub") %>%</small>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="BestSubSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT TOP (1) ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Top_Sub, Subject.SubjectName
FROM Exam_Result_of_Subject INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID WHERE (Exam_Result_of_Subject.StudentID = @StudentID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub')
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN ORDER BY Top_Sub DESC">
                <SelectParameters>
                    <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                </SelectParameters>
            </asp:SqlDataSource>

            <asp:FormView ID="WeakFormView" Width="100%" DataSourceID="WeakSQL" runat="server">
                <ItemTemplate>
                    <div class="statistic2 d-flex align-items-center bg-white has-shadow mr-0">
                        <div class="icon bg-danger"><i class="fa fa-thumbs-down"></i></div>
                        <div class="text">
                            <strong><%# Eval("SubjectName") %></strong><br>
                            <small><%# Eval("Worst_Sub") %>%</small>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="WeakSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT TOP (1) ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Worst_Sub, Subject.SubjectName FROM Exam_Result_of_Subject INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE (Exam_Result_of_Subject.StudentID = @StudentID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub')
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN ORDER BY Worst_Sub ASC">
                <SelectParameters>
                    <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="card mb-4">
                <div class="card-header">
                    <i class="fa fa-bar-chart mr-1" aria-hidden="true"></i>MY SUBJECT AVG.
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <asp:Repeater ID="SubjectAvgRepeater" runat="server" DataSourceID="SubjectAvgSQL">
                            <HeaderTemplate>
                                <table class="table">
                                    <thead>
                                        <tr>
                                            <th>Subject</th>
                                            <th>Avg. Marks</th>
                                            <th>Position</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("SubjectName") %></td>
                                    <td><%# Eval("Sub_Avg") %>%</td>
                                    <td><%# Eval("Sub_Position") %></td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </tbody>
                             </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:SqlDataSource ID="SubjectAvgSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Exam_Result_of_Subject.SubjectID, Subject.SN, Subject.SubjectName, ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Sub_Avg, 
AVG(CAST(Exam_Result_of_Subject.Position_InSubject_Class AS int)) AS Sub_Position FROM Exam_Result_of_Subject INNER JOIN
Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID
WHERE (Exam_Result_of_Subject.StudentID = @StudentID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub')
GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN ORDER BY Sub_Avg DESC">
                            <SelectParameters>
                                <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>

            <div class="card mb-3">
                <div class="card-body text-center">
                    <canvas id="doughnutChart"></canvas>

                    <asp:FormView ID="LastStatusFormView" runat="server" DataSourceID="LastStatusSQL" Width="100%">
                        <ItemTemplate>
                            <small>Last <%# Eval("Attendance") %>:  <%# Eval("AttendanceDate","{0:d MMM yyyy}") %></small>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="LastStatusSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT TOP (1) AttendanceDate, Attendance FROM Attendance_Record WHERE (SchoolID = @SchoolID) AND (StudentID = @StudentID) ORDER BY AttendanceDate DESC">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </div>

        <div class="col-lg-9">
            <asp:FormView ID="Acc_SummeryFormView" runat="server" DataSourceID="Acc_SummerySQL" Width="100%">
                <ItemTemplate>
                    <input id="TotalFee" type="hidden" value='<%# Eval("TotalFee") %>' />
                    <input id="Current_Fee" type="hidden" value='<%# Eval("Current_Fee") %>' />
                    <div class="row">
                        <div class="col-lg-4">
                            <div class="z-depth-2 mb-3">
                                <div class="stats-left">
                                    <h4><i class="fa fa-pie-chart mr-1" aria-hidden="true"></i>Current Due</h4>
                                    <div id="CurrentDue_p"></div>
                                </div>
                                <div class="stats-right" style="padding-bottom: 0px; padding-top: 7%;">
                                    <button type="button" id="btnPayNow" class="btn btn-danger" 
                                                style="height: 30px; font-size: 12px; font-weight: bold; padding: 0px 10px;"
                                                OnClick="ViewDueClick()"> Pay Now</button>
                                    <input id="C_D" type="hidden" value='<%# Eval("CurrentDue") %>' />
                                    <label><%# Eval("CurrentDue","{0:N0}") %>৳</label>
                                </div>
                                <div class="clearfix"></div>
                            </div>
                        </div>
                        <div class="col-lg-4">
                            <div class="states-mdl mb-3">
                                <div class="z-depth-2">
                                    <div class="stats-left">
                                        <h4><i class="fa fa-check-circle mr-1" aria-hidden="true"></i>Paid</h4>
                                        <div id="Paid_p"></div>
                                    </div>
                                    <div class="stats-right">
                                        <input id="Paid" type="hidden" value='<%# Eval("Paid") %>' />
                                        <label><%# Eval("Paid","{0:N0}") %>৳</label>
                                    </div>
                                    <div class="clearfix"></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-4">
                            <div class="mb-3 states-last">
                                <div class="z-depth-2">
                                    <div class="stats-left">
                                        <h4 OnClick="ViewDueClick()" style="cursor:pointer"><i class="fa fa-hourglass-half mr-1" aria-hidden="true"></i>View Due</h4>
                                        <div id="Due_p"></div>
                                    </div>
                                    <div class="stats-right">
                                        <input id="Due" type="hidden" value='<%# Eval("Due") %>' />
                                        <label><%# Eval("Due","{0:N0}") %>৳</label>
                                    </div>
                                    <div class="clearfix"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="Acc_SummerySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SUM(Amount - Total_Discount) AS TotalFee,SUM(PaidAmount) AS Paid, SUM(Receivable_Amount) AS Due,
(SELECT SUM(ISNULL(Receivable_Amount, 0)) AS PresentDue FROM Income_PayOrder WHERE (EndDate &lt; GETDATE()) AND (StudentID = @StudentID) AND (Is_Active = 1)) AS CurrentDue,
(SELECT SUM(ISNULL(Amount - Total_Discount, 0)) AS PresentDue FROM Income_PayOrder WHERE (EndDate &lt; GETDATE()) AND (StudentID = @StudentID) AND (Is_Active = 1)) AS Current_Fee FROM Income_PayOrder WHERE (StudentID = @StudentID) AND (Is_Active = 1) ">
                <SelectParameters>
                    <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                </SelectParameters>
            </asp:SqlDataSource>

            <asp:FormView ID="StdentAvgFormView" runat="server" Width="100%" DataSourceID="StudentAvgSQL">
                <ItemTemplate>
                    <div class="row no-gutters">
                        <div class="col-lg-3">
                            <div class="statistic d-flex align-items-center bg-white has-shadow">
                                <div class="icon bg-danger"><i class="fa fa-list-ol"></i></div>
                                <div class="text">
                                    <strong><%# Eval("Average_Position_Class") %></strong><br>
                                    <small>Avg. Position</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-3">
                            <div class="statistic d-flex align-items-center bg-white has-shadow">
                                <div class="icon bg-warning darken-3"><i class="fa fa-star"></i></div>
                                <div class="text">
                                    <strong><%# Eval("Average_Point") %></strong><br>
                                    <small>Avg. Point</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-3">
                            <div class="statistic d-flex align-items-center bg-white has-shadow">
                                <div class="icon bg-primary"><i class="fa fa-pie-chart"></i></div>
                                <div class="text">
                                    <strong><%# Eval("Average_ObtainedMarkofSubject") %>%</strong><br>
                                    <small>Avg. Mark</small>
                                </div>
                            </div>
                        </div>
                        <div class="col-lg-3">
                            <div class="statistic d-flex align-items-center bg-white has-shadow" style="margin-right: 0;">
                                <div class="icon bg-success"><i class="fa fa-line-chart"></i></div>
                                <div class="text">
                                    <strong><%# Eval("Success_Percentage") %>%</strong><br>
                                    <small>Pass %</small>
                                </div>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="StudentAvgSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AVG(CAST(Position_InExam_Class as int)) AS Average_Position_Class,ROUND(AVG(Student_Point),2,0) AS  Average_Point,(SELECT ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject),2,0) FROM  Exam_Result_of_Subject INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID WHERE (Exam_Result_of_Student.StudentPublishStatus = N'Pub') AND (Exam_Result_of_Student.StudentID = @StudentID)) as Average_ObtainedMarkofSubject,
(SELECT ROUND(100 * SUM(CASE WHEN t.PassStatus_Student = 'P' THEN 1 ELSE 0 END)/COUNT(t.StudentID),2,0) FROM (SELECT StudentID, PassStatus_Student FROM  Exam_Result_of_Student WHERE (StudentPublishStatus = N'Pub') AND (StudentID = @StudentID)UNION ALL SELECT StudentID, PassStatus_Student FROM  Exam_Cumulative_Student WHERE (StudentID = @StudentID)) as T) AS Success_Percentage
FROM Exam_Result_of_Student WHERE (StudentPublishStatus = N'Pub') AND (StudentID = @StudentID)">
                <SelectParameters>
                    <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                </SelectParameters>
            </asp:SqlDataSource>

            <div class="card mb-4">
                <div class="card-header">
                    <i class="fa fa-bar-chart mr-1" aria-hidden="true"></i>SESSION BASED EXAM REPORT
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <asp:Repeater ID="SessionSuccessRepeater" runat="server" DataSourceID="SessionSuccessSQL">
                            <HeaderTemplate>
                                <table class="table">
                                    <thead>
                                        <tr>
                                            <th>Session</th>
                                            <th>Avg. Position</th>
                                            <th>Avg. Point</th>
                                            <th>Pass %</th>
                                            <th>Avg. Marks</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("EducationYear") %></td>
                                    <td><%# Eval("Average_Position_Class") %></td>
                                    <td><%# Eval("Average_Point") %></td>
                                    <td><%# Eval("Success_Percentage") %>%</td>
                                    <td><%# Eval("Average_ObtainedMarkofSubject") %>%</td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </tbody>
                             </table>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:SqlDataSource ID="SessionSuccessSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT T_AP.EducationYearID, Education_Year.EducationYear, T_AP.Average_Position_Class, T_AP.Average_Point, T_S.Success_Percentage, T_B.Average_ObtainedMarkofSubject
FROM (SELECT EducationYearID, AVG(CAST(Position_InExam_Class AS int)) AS Average_Position_Class, ROUND(AVG(Student_Point), 2, 0) AS Average_Point FROM Exam_Result_of_Student WHERE (StudentPublishStatus = N'Pub') AND (StudentID = @StudentID)
GROUP BY EducationYearID) AS T_AP INNER JOIN(SELECT EducationYearID, ROUND(100 * SUM(CASE WHEN t .PassStatus_Student = 'P' THEN 1 ELSE 0 END) / COUNT(StudentID), 2, 0) AS Success_Percentage
FROM (SELECT EducationYearID, StudentID, PassStatus_Student FROM Exam_Result_of_Student AS Exam_Result_of_Student_1 WHERE (StudentPublishStatus = N'Pub') AND (StudentID = @StudentID)
UNION ALL SELECT EducationYearID, StudentID, PassStatus_Student FROM Exam_Cumulative_Student WHERE (StudentID = @StudentID)) AS T
GROUP BY EducationYearID) AS T_S ON T_AP.EducationYearID = T_S.EducationYearID INNER JOIN
(SELECT Exam_Result_of_Student_2.EducationYearID, ROUND(AVG(Exam_Result_of_Subject.ObtainedPercentage_ofSubject), 2, 0) AS Average_ObtainedMarkofSubject
FROM Exam_Result_of_Subject INNER JOIN Exam_Result_of_Student AS Exam_Result_of_Student_2 ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student_2.StudentResultID WHERE (Exam_Result_of_Student_2.StudentPublishStatus = N'Pub') AND (Exam_Result_of_Student_2.StudentID = @StudentID)
GROUP BY Exam_Result_of_Student_2.EducationYearID) AS T_B ON T_AP.EducationYearID = T_B.EducationYearID INNER JOIN
Education_Year ON T_AP.EducationYearID = Education_Year.EducationYearID ORDER BY Education_Year.StartDate">
                            <SelectParameters>
                                <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>

            <ul class="nav nav-tabs nav-justified">
                <li class="nav-item">
                    <a class="nav-link active" data-toggle="tab" href="#panel1" role="tab">INDIVIDUAL RESULT</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" data-toggle="tab" href="#panel2" role="tab">CUMULATIVE RESULT</a>
                </li>
            </ul>

            <div class="tab-content card">
                <div class="mb-3 px-5">
                    <asp:DropDownList ID="EduYearDropDownList" runat="server" CssClass="form-control" DataSourceID="EduYearSQL" DataTextField="EducationYear" DataValueField="EducationYearID"></asp:DropDownList>
                    <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Education_Year.EducationYearID, 'Class: ' + CreateClass.Class+ ' - ' + Education_Year.EducationYear  AS EducationYear, CreateClass.Class FROM Education_Year INNER JOIN StudentsClass ON Education_Year.EducationYearID = StudentsClass.EducationYearID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Education_Year.SchoolID = @SchoolID) AND (StudentsClass.StudentID = @StudentID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="tab-pane fade in show active" id="panel1" role="tabpanel">
                    <canvas id="myChart"></canvas>
                </div>

                <div class="tab-pane fade" id="panel2" role="tabpanel">
                    <canvas id="CumilativeExam"></canvas>
                </div>
            </div>
        </div>
    </div>

    <script src="/JS/ProgressBar/jquery.lineProgressbar.js"></script>
    <script>
        $(function () {
            var TotalFee = $('#TotalFee').val();
            var Current_Fee = $('#Current_Fee').val();

            var CurrentDue = $('#C_D').val();
            var Paid = $('#Paid').val();
            var Due = $('#Due').val();

            $('#CurrentDue_p').LineProgressbar({
                percentage: (CurrentDue * 100) / Current_Fee,
                fillBackgroundColor: '#fff',
                backgroundColor: '#7376d1',
                height: '4px',
            });

            $('#Paid_p').LineProgressbar({
                percentage: (Paid * 100) / TotalFee,
                fillBackgroundColor: '#fff',
                backgroundColor: '#828282',
                height: '4px',
            });

            $('#Due_p').LineProgressbar({
                percentage: (Due * 100) / TotalFee,
                fillBackgroundColor: '#fff',
                backgroundColor: '#f97738',
                height: '4px',
            });


            //Chart
            var sy = $("[id*=Session_DropDownList] option:selected").val();
            $('[id*=EduYearDropDownList] option[value="' + sy + '"]').attr('selected', true);

            ClassChart();
            //showHideOnlinePayButton();
            //doughnut-- Attendance
            $.ajax({
                type: "POST",
                url: "Student_Profile.aspx/Get_Attendance",
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
                                    backgroundColor: ["#F7464A", "#46BFBD", "#FDB45C", "#949FB1", "#4D5360"],
                                    hoverBackgroundColor: ["#FF5A5E", "#5AD3D1", "#FFC870", "#A8B3C5", "#616774"]
                                }
                            ]
                        },
                        options: {
                            title: {
                                display: true,
                                text: 'Attendance (Last 30 days)'
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
        });

        $("[id*=EduYearDropDownList]").on("change", function () {
            ClassChart();
        });

        var myChart;
        var myChart2;
        function ClassChart() {
            var YearID = $("[id*=EduYearDropDownList] option:selected").val();
            var Year = $("[id*=EduYearDropDownList] option:selected").text();

            //Individual
            $.ajax({
                type: "POST",
                url: "Student_Profile.aspx/Get_Exam_GradePoint",
                data: '{EducationYearID:' + YearID + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    var footerLine = Ch_data[2];

                    if (myChart) {
                        myChart.destroy();
                    }

                    var ctx = document.getElementById("myChart").getContext('2d');
                    myChart = new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: Ch_data[0],
                            datasets: [{
                                type: 'line',
                                borderColor: ['rgba(0, 200, 81, .8)'],
                                borderWidth: 5,
                                borderDash: [5, 5],
                                fill: false,
                                data: Ch_data[1]
                            },
                            {
                                data: Ch_data[1],
                                backgroundColor: [
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',
                                    'rgba(155,187,88,.3)',
                                    'rgba(35,191,170,.3)',
                                    'rgba(128,100,161,.3)',
                                    'rgba(74,172,197,.3)',
                                    'rgba(247,150,71,.3)',
                                    'rgba(127,96,132,.3)',
                                    'rgba(119,160,51,.3)',
                                    'rgba(51,85,139,.3)',
                                    'rgba(229,149,102,.3)',
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',

                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',
                                    'rgba(155,187,88,.3)',
                                    'rgba(35,191,170,.3)',
                                    'rgba(128,100,161,.3)',
                                    'rgba(74,172,197,.3)',
                                    'rgba(247,150,71,.3)',
                                    'rgba(127,96,132,.3)',
                                    'rgba(119,160,51,.3)',
                                    'rgba(51,85,139,.3)',
                                    'rgba(229,149,102,.3)',
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)'
                                ],
                                borderColor: [
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',
                                    'rgba(155,187,88,.5)',
                                    'rgba(35,191,170,.5)',
                                    'rgba(128,100,161,.5)',
                                    'rgba(74,172,197,.5)',
                                    'rgba(247,150,71,.5)',
                                    'rgba(127,96,132,.5)',
                                    'rgba(119,160,51,.5)',
                                    'rgba(51,85,139,.5)',
                                    'rgba(229,149,102,.5)',
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',

                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',
                                    'rgba(155,187,88,.5)',
                                    'rgba(35,191,170,.5)',
                                    'rgba(128,100,161,.5)',
                                    'rgba(74,172,197,.5)',
                                    'rgba(247,150,71,.5)',
                                    'rgba(127,96,132,.5)',
                                    'rgba(119,160,51,.5)',
                                    'rgba(51,85,139,.5)',
                                    'rgba(229,149,102,.5)',
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)'
                                ],
                                borderWidth: 1
                            }]
                        },
                        options: {
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        max: 5.5
                                    }
                                }],
                                xAxes: [{
                                    ticks: {
                                        autoSkip: false
                                    },
                                    maxBarThickness: 100,
                                }]
                            },
                            legend: {
                                display: false
                            },
                            tooltips: {
                                enabled: true,
                                mode: 'single',
                                callbacks: {
                                    beforeFooter: function (tooltipItems, data) {
                                        return 'Grade: ' + footerLine[tooltipItems[0].index];
                                    }
                                }
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

            //Cumilative
            $.ajax({
                type: "POST",
                url: "Student_Profile.aspx/Get_CumilativeExam",
                data: '{EducationYearID:' + YearID + '}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var Ch_data = r.d;
                    var footerLine = Ch_data[2];

                    if (myChart2) {
                        myChart2.destroy();
                    }

                    var ctx = document.getElementById("CumilativeExam").getContext('2d');
                    myChart2 = new Chart(ctx, {
                        type: 'bar',
                        data: {
                            labels: Ch_data[0],
                            datasets: [{
                                type: 'line',
                                borderColor: ['rgba(0, 200, 81, .8)'],
                                borderWidth: 5,
                                borderDash: [5, 5],
                                fill: false,
                                data: Ch_data[1]
                            },
                            {
                                data: Ch_data[1],
                                backgroundColor: [
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',
                                    'rgba(155,187,88,.3)',
                                    'rgba(35,191,170,.3)',
                                    'rgba(128,100,161,.3)',
                                    'rgba(74,172,197,.3)',
                                    'rgba(247,150,71,.3)',
                                    'rgba(127,96,132,.3)',
                                    'rgba(119,160,51,.3)',
                                    'rgba(51,85,139,.3)',
                                    'rgba(229,149,102,.3)',
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',

                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)',
                                    'rgba(155,187,88,.3)',
                                    'rgba(35,191,170,.3)',
                                    'rgba(128,100,161,.3)',
                                    'rgba(74,172,197,.3)',
                                    'rgba(247,150,71,.3)',
                                    'rgba(127,96,132,.3)',
                                    'rgba(119,160,51,.3)',
                                    'rgba(51,85,139,.3)',
                                    'rgba(229,149,102,.3)',
                                    'rgba(79,129,188,.3)',
                                    'rgba(192,80,78,.3)'
                                ],
                                borderColor: [
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',
                                    'rgba(155,187,88,.5)',
                                    'rgba(35,191,170,.5)',
                                    'rgba(128,100,161,.5)',
                                    'rgba(74,172,197,.5)',
                                    'rgba(247,150,71,.5)',
                                    'rgba(127,96,132,.5)',
                                    'rgba(119,160,51,.5)',
                                    'rgba(51,85,139,.5)',
                                    'rgba(229,149,102,.5)',
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',

                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)',
                                    'rgba(155,187,88,.5)',
                                    'rgba(35,191,170,.5)',
                                    'rgba(128,100,161,.5)',
                                    'rgba(74,172,197,.5)',
                                    'rgba(247,150,71,.5)',
                                    'rgba(127,96,132,.5)',
                                    'rgba(119,160,51,.5)',
                                    'rgba(51,85,139,.5)',
                                    'rgba(229,149,102,.5)',
                                    'rgba(79,129,188,.5)',
                                    'rgba(192,80,78,.5)'
                                ],
                                borderWidth: 1
                            }]
                        },
                        options: {
                            scales: {
                                yAxes: [{
                                    ticks: {
                                        beginAtZero: true,
                                        max: 5.5
                                    }
                                }],
                                xAxes: [{
                                    ticks: {
                                        autoSkip: false
                                    },
                                    maxBarThickness: 100,
                                }]
                            },
                            legend: {
                                display: false
                            },
                            tooltips: {
                                enabled: true,
                                mode: 'single',
                                callbacks: {
                                    beforeFooter: function (tooltipItems, data) {
                                        return 'Grade: ' + footerLine[tooltipItems[0].index];
                                    }
                                }
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
                            ctx.fillStyle = 'rgb(0, 0, 0)';

                            var fontSize = 16;
                            var fontStyle = 'normal';
                            var fontFamily = 'Helvetica Neue';
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

        function ViewDueClick() {
            window.location.href = "/Student/Accounts.aspx";
        }

        function showHideOnlinePayButton() {
            $('#btnPayOnline').hide();
            $('#btnPayNow').hide();
            $('#spanBankingMsg').hide();

            $.ajax({
                type: "POST",
                url: "Student_Profile.aspx/IsOnlinePaymentApplicable",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (result) {
                    if (result.d) { //enable
                        $('#btnPayOnline').show();
                        $('#btnPayNow').show();
                        $('#spanBankingMsg').show();
                    } 
                },
                failure: function (r) {
                    //alert(r.d);
                },
                error: function (r) {
                    //alert(r.d);
                }
            });
        }

    </script>
</asp:Content>
