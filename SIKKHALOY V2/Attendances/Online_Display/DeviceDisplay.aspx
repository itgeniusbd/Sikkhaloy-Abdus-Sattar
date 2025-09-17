<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeviceDisplay.aspx.cs" Inherits="EDUCATION.COM.Attendances.Online_Display.DeviceDisplay" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Sikkhaloy - device display</title>

    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" />
    <link href="/CSS/bootstrap/bootstrap.css" rel="stylesheet" />
    <link href="mdb/css/mdb-core.css" rel="stylesheet" />

    <link href="CSS/device-display.css?v=2.0.0" rel="stylesheet" />
</head>
<body>
    <form runat="server">
         <!--student-->
        <div class="student">
            <div class="summary z-depth-1 flex-1">
                <asp:FormView ID="StudentSummaryFV" runat="server" DataSourceID="StudentSummarySQL" RenderOuterTable="false">
                    <ItemTemplate>
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item active">Student: <span><%# Eval("Total_Student") %></span></li>
                            <li class="list-group-item">Current In: <span><%# Eval("Current_IN_Student") %></span></li>
                            <li class="list-group-item">Total Out: <span><%# Eval("Total_Out_Student") %></span></li>
                            <li class="list-group-item">Present: <span><%# Eval("Total_Student_Prasent") %></span></li>
                            <li class="list-group-item">Late: <span><%# Eval("Total_Student_Late") %></span></li>
                            <li class="list-group-item">Absent: <span><%# Eval("Total_Student_Absent") %></span></li>
                            <li class="list-group-item">Late Absent: <span><%# Eval("Total_Student_Late_Absent") %></span></li>
                        </ul>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="StudentSummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT (SELECT COUNT(*) FROM VW_Attendance_Users WHERE (SchoolID = @SchoolID) and (Is_Student = 1))AS Total_Student,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (EntryTime IS NOT NULL) AND (Is_OUT = 0) AND (SchoolID = @SchoolID))AS Current_IN_Student,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (ExitTime IS NOT NULL) AND (Is_OUT = 1) AND (SchoolID = @SchoolID))AS Total_Out_Student,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Pre'))AS Total_Student_Prasent,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Late')) AS Total_Student_Late,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Abs'))AS Total_Student_Absent,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Late Abs'))AS Total_Student_Late_Absent">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="card card-body ml-3 flex-1">
                <!--student in-->
                <div class="slide-in str_wrap">
                    <asp:Repeater ID="StudentEntryLog" runat="server" DataSourceID="Student_Entry_LogSQL">
                        <ItemTemplate>
                            <div class="info-block">
                                <div class="card">
                                    <div class="name-title">
                                        <i class="fa fa-user-o" aria-hidden="true"></i>
                                        <%# Eval("StudentsName") %>
                                    </div>
                                    <img class="card-img-top" src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" alt="" />
                                    <span class="notify-badge z-depth-2 <%# Eval("Attendance") %>"><%# Eval("Attendance") %></span>
                                    <div class="entry-date">
                                        <i class="fa fa-clock-o" aria-hidden="true"></i>
                                        <%# Eval("EntryTime") %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:SqlDataSource ID="Student_Entry_LogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_Record.StudentID, Student.StudentsName, Student.ID, CreateClass.Class, StudentsClass.RollNo, Attendance_Record.Attendance, CONVERT (varchar(15), Attendance_Record.EntryTime, 100) AS EntryTime FROM Attendance_Record INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID INNER JOIN StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Attendance_Record.AttendanceDate = CONVERT (date, GETDATE())) AND (Attendance_Record.EntryTime IS NOT NULL) AND (Attendance_Record.Is_OUT = 0) AND (Attendance_Record.SchoolID = @SchoolID) ORDER BY EntryTime DESC">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <!--student out-->
                <div class="slide-out str_wrap mt-3">
                    <asp:Repeater ID="StudentExitLog" runat="server" DataSourceID="Student_Exit_LogSQL">
                        <ItemTemplate>
                            <div class="info-block">
                                <div class="card">
                                    <div class="name-title">
                                        <i class="fa fa-user-o" aria-hidden="true"></i>
                                        <%# Eval("StudentsName") %>
                                    </div>
                                    <img class="card-img-top" src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" alt="" />
                                    <span class="notify-badge z-depth-2 <%# Eval("Attendance") %>"><%# Eval("Attendance") %></span>
                                    <div class="entry-date">
                                        <i class="fa fa-clock-o" aria-hidden="true"></i>
                                        <%# Eval("EntryTime") %>
                                    </div>
                                    <div class="exit-date">
                                        <i class="fa fa-clock-o" aria-hidden="true"></i>
                                        <%# Eval("ExitTime") %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:SqlDataSource ID="Student_Exit_LogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_Record.StudentID, Student.StudentsName, Student.ID, CreateClass.Class, StudentsClass.RollNo, Attendance_Record.Attendance, CONVERT (varchar(15), Attendance_Record.EntryTime, 100) AS EntryTime, CONVERT (varchar(15), Attendance_Record.ExitTime, 100) AS ExitTime FROM Attendance_Record INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID INNER JOIN StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Attendance_Record.AttendanceDate = CONVERT (date, GETDATE())) AND (Attendance_Record.Is_OUT = 1 or Attendance_Record.Attendance ='Abs') AND (Attendance_Record.SchoolID = @SchoolID) ORDER BY ExitTime DESC">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </div>

        <!--employee-->
        <div class="student mt-2">
            <div class="summary z-depth-1 flex-1">
                <asp:FormView ID="EmployeeSummaryFormView" runat="server" DataSourceID="EmployeeSummarySQL" RenderOuterTable="false">
                    <ItemTemplate>
                        <ul class="list-group list-group-flush">
                            <li class="list-group-item active">Employee: <span><%# Eval("Total_Employee") %></span></li>
                            <li class="list-group-item">Current In: <span><%# Eval("Current_IN_Employee") %></span></li>
                            <li class="list-group-item">Total Out: <span><%# Eval("Total_Out_Employee") %></span></li>
                            <li class="list-group-item">Present: <span><%# Eval("Total_Employee_Prasent") %></span></li>
                            <li class="list-group-item">Late: <span><%# Eval("Total_Employee_Late") %></span></li>
                            <li class="list-group-item">Absent: <span><%# Eval("Total_Employee_Absent") %></span></li>
                            <li class="list-group-item">Late Absent: <span><%# Eval("Total_Employee_Late_Absent") %></span></li>
                        </ul>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="EmployeeSummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT (SELECT  Count(EmployeeID)  FROM Employee_Info WHERE (Job_Status = N'Active') AND (SchoolID = @SchoolID))AS Total_Employee,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (EntryTime IS NOT NULL) AND (Is_OUT = 0) AND (SchoolID = @SchoolID))AS Current_IN_Employee,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (ExitTime IS NOT NULL) AND (Is_OUT = 1) AND (SchoolID = @SchoolID))AS Total_Out_Employee,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Pre'))AS Total_Employee_Prasent,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Late')) AS Total_Employee_Late,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Abs'))AS Total_Employee_Absent,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Late Abs'))AS Total_Employee_Late_Absent">
                    <SelectParameters>
                        <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="card card-body ml-3 flex-1">
                <!--employee in-->
                <div class="slide-in str_wrap">
                    <asp:Repeater ID="EmployeeEntryLog" runat="server" DataSourceID="EmployeeEntryLogSQL">
                        <ItemTemplate>
                            <div class="info-block">
                                <div class="card">
                                    <div class="name-title">
                                        <i class="fa fa-user-o" aria-hidden="true"></i>
                                        <%# Eval("Name") %>
                                    </div>
                                    <img class="card-img-top" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" alt="" />
                                    <span class="notify-badge z-depth-2 <%# Eval("AttendanceStatus") %>"><%# Eval("AttendanceStatus") %></span>
                                    <div class="entry-date">
                                        <i class="fa fa-clock-o" aria-hidden="true"></i>
                                        <%# Eval("EntryTime") %>
                                    </div>

                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:SqlDataSource ID="EmployeeEntryLogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Attendance_Record.EmployeeID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, CONVERT (varchar(15), Employee_Attendance_Record.EntryTime, 100) AS EntryTime, Employee_Attendance_Record.AttendanceStatus FROM Employee_Attendance_Record INNER JOIN VW_Emp_Info ON Employee_Attendance_Record.EmployeeID = VW_Emp_Info.EmployeeID WHERE (Employee_Attendance_Record.AttendanceDate = CONVERT (date, GETDATE())) AND (Employee_Attendance_Record.EntryTime IS NOT NULL) AND (Employee_Attendance_Record.Is_OUT = 0) AND (Employee_Attendance_Record.SchoolID = @SchoolID)  ORDER BY EntryTime DESC">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <!--employee out-->
                <div class="slide-out str_wrap mt-3">
                    <asp:Repeater ID="EmployeeExitLog" runat="server" DataSourceID="EmployeeExitLogSQL">
                        <ItemTemplate>
                            <div class="info-block">
                                <div class="card">
                                    <div class="name-title">
                                        <i class="fa fa-user-o" aria-hidden="true"></i>
                                        <%# Eval("Name") %>
                                    </div>
                                    <img class="card-img-top" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" alt="" />
                                    <span class="notify-badge z-depth-2 <%# Eval("AttendanceStatus") %>"><%# Eval("AttendanceStatus") %></span>
                                    <div class="entry-date">
                                        <i class="fa fa-clock-o" aria-hidden="true"></i>
                                        <%# Eval("EntryTime") %>
                                    </div>
                                    <div class="exit-date">
                                        <i class="fa fa-clock-o" aria-hidden="true"></i>
                                        <%# Eval("ExitTime") %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:SqlDataSource ID="EmployeeExitLogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Attendance_Record.EmployeeID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, CONVERT (varchar(15), Employee_Attendance_Record.EntryTime, 100) AS EntryTime, CONVERT (varchar(15), Employee_Attendance_Record.ExitTime, 100) AS ExitTime, Employee_Attendance_Record.AttendanceStatus FROM Employee_Attendance_Record INNER JOIN VW_Emp_Info ON Employee_Attendance_Record.EmployeeID = VW_Emp_Info.EmployeeID WHERE (Employee_Attendance_Record.AttendanceDate = CONVERT (date, GETDATE())) AND (Employee_Attendance_Record.Is_OUT = 1 or Employee_Attendance_Record.AttendanceStatus='Abs') AND (Employee_Attendance_Record.SchoolID = @SchoolID) ORDER BY ExitTime DESC">
                        <SelectParameters>
                            <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
        </div>
    </form>

    <!-- JQuery -->
    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
    <script src="mdb/js/mdb-core.js"></script>
    <script src="js/jquery.liMarquee.js"></script>
    <script>
        $(function () {
            initMarquee();
        });

        function initMarquee() {
            $('.slide-in').liMarquee({
                direction: 'left',
                loop: -1,
                scrolldelay: 0,
                scrollamount: 35,
                circular: true,
                hoverStop: false,
                drag: false
            });

            $('.slide-out').liMarquee({
                direction: 'right',
                loop: -1,
                scrolldelay: 0,
                scrollamount: 35,
                circular: true,
                hoverStop:false,
                drag: false
            });
        }
    </script>
</body>
</html>
