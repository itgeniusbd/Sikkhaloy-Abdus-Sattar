<%@ Page Title="Attendance Display" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Attendance_Slider.aspx.cs" Inherits="EDUCATION.COM.Attendances.Online_Display.Attendance_Slider" %>



<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="CSS/Display.css?v=11.4" rel="stylesheet" />

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">

        <ContentTemplate>

            <div class="att-one-view">



            <aside class="summary-column">

                <section class="summary-group">

                    <div class="group-header">STUDENT</div>

                    <div class="att-schedule-cards">

                        <asp:Repeater ID="StudentScheduleSummaryRepeater" runat="server" DataSourceID="StudentScheduleSummarySQL">

                            <ItemTemplate>

                                <div class="schedule-card tone-<%# Container.ItemIndex % 4 %>" data-schedule-id="<%# Eval("ScheduleID") %>">

                                    <div class="schedule-head">

                                        <span class="schedule-title"><%# Eval("ScheduleName") %></span>

                                        <span class="schedule-time"><%# Eval("TimeRange") %></span>

                                        <span class="schedule-users"><i class="fa fa-users"></i> <%# Eval("Total_User") %></span>

                                    </div>

                                    <div class="schedule-stats compact">

                                        <span class="chip">In <b><%# Eval("Current_IN") %></b></span>

                                        <span class="chip">Out <b><%# Eval("Total_Out") %></b></span>

                                        <span class="chip pre">Pre <b><%# Eval("Total_Present") %></b></span>

                                        <span class="chip late">Late <b><%# Eval("Total_Late") %></b></span>

                                        <span class="chip late-abs">L.Abs <b><%# Eval("Total_Late_Absent") %></b></span>

                                        <span class="chip abs">Abs <b><%# Eval("Total_Absent") %></b></span>

                                    </div>

                                </div>

                            </ItemTemplate>

                        </asp:Repeater>

                        <asp:SqlDataSource ID="StudentScheduleSummarySQL" runat="server"

                            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                            SelectCommand="SELECT sch.ScheduleID,

       sch.ScheduleName,

       LTRIM(RIGHT(CONVERT(varchar(20), sd.StartTime, 100), 7)) + N' - ' + LTRIM(RIGHT(CONVERT(varchar(20), sd.EndTime, 100), 7)) AS TimeRange,

       (SELECT COUNT(DISTINCT ass.StudentID)

        FROM Attendance_Schedule_AssignStudent ass

        INNER JOIN Student s ON ass.StudentID = s.StudentID

        INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = ass.SchoolID AND scEy.EducationYearID = @EducationYearID

        WHERE ass.SchoolID = @SchoolID AND ass.ScheduleID = sch.ScheduleID AND s.Status = N'Active') AS Total_User,

       (SELECT COUNT(*)

        FROM Attendance_Record ar
        INNER JOIN Attendance_Schedule_AssignStudent ass ON ass.StudentID = ar.StudentID AND ass.ScheduleID = sch.ScheduleID AND ass.SchoolID = @SchoolID
        INNER JOIN Student s ON s.StudentID = ar.StudentID AND s.Status = N'Active'
        INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
        WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = CONVERT(date, GETDATE())

          AND ISNULL(ar.Attendance_ScheduleID, 0) = sch.ScheduleID

          AND ar.EntryTime IS NOT NULL AND ar.Is_OUT = 0 AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs')) AS Current_IN,

       (SELECT COUNT(*)

        FROM Attendance_Record ar
        INNER JOIN Attendance_Schedule_AssignStudent ass ON ass.StudentID = ar.StudentID AND ass.ScheduleID = sch.ScheduleID AND ass.SchoolID = @SchoolID
        INNER JOIN Student s ON s.StudentID = ar.StudentID AND s.Status = N'Active'
        INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
        WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = CONVERT(date, GETDATE())

          AND ISNULL(ar.Attendance_ScheduleID, 0) = sch.ScheduleID

          AND ar.ExitTime IS NOT NULL AND ar.Is_OUT = 1) AS Total_Out,

       (SELECT COUNT(*)

        FROM Attendance_Record ar
        INNER JOIN Attendance_Schedule_AssignStudent ass ON ass.StudentID = ar.StudentID AND ass.ScheduleID = sch.ScheduleID AND ass.SchoolID = @SchoolID
        INNER JOIN Student s ON s.StudentID = ar.StudentID AND s.Status = N'Active'
        INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
        WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = CONVERT(date, GETDATE())

          AND ISNULL(ar.Attendance_ScheduleID, 0) = sch.ScheduleID AND ar.Attendance = N'Pre') AS Total_Present,

       (SELECT COUNT(*)

        FROM Attendance_Record ar
        INNER JOIN Attendance_Schedule_AssignStudent ass ON ass.StudentID = ar.StudentID AND ass.ScheduleID = sch.ScheduleID AND ass.SchoolID = @SchoolID
        INNER JOIN Student s ON s.StudentID = ar.StudentID AND s.Status = N'Active'
        INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
        WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = CONVERT(date, GETDATE())

          AND ISNULL(ar.Attendance_ScheduleID, 0) = sch.ScheduleID AND ar.Attendance = N'Late') AS Total_Late,

       (SELECT COUNT(*)

        FROM Attendance_Record ar
        INNER JOIN Attendance_Schedule_AssignStudent ass ON ass.StudentID = ar.StudentID AND ass.ScheduleID = sch.ScheduleID AND ass.SchoolID = @SchoolID
        INNER JOIN Student s ON s.StudentID = ar.StudentID AND s.Status = N'Active'
        INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
        WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = CONVERT(date, GETDATE())

          AND ISNULL(ar.Attendance_ScheduleID, 0) = sch.ScheduleID AND ar.Attendance = N'Late Abs') AS Total_Late_Absent,

       ((SELECT COUNT(*)

        FROM Attendance_Record ar
        INNER JOIN Attendance_Schedule_AssignStudent ass ON ass.StudentID = ar.StudentID AND ass.ScheduleID = sch.ScheduleID AND ass.SchoolID = @SchoolID
        INNER JOIN Student s ON s.StudentID = ar.StudentID AND s.Status = N'Active'
        INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID
        WHERE ar.SchoolID = @SchoolID AND ar.AttendanceDate = CONVERT(date, GETDATE())

          AND ISNULL(ar.Attendance_ScheduleID, 0) = sch.ScheduleID AND ar.Attendance = N'Abs')
       +
       (SELECT COUNT(DISTINCT ass.StudentID)
        FROM Attendance_Schedule_AssignStudent ass
        INNER JOIN Student s ON ass.StudentID = s.StudentID AND s.Status = N'Active'
        INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = ass.SchoolID AND scEy.EducationYearID = @EducationYearID
        WHERE ass.SchoolID = @SchoolID
          AND ass.ScheduleID = sch.ScheduleID
          AND CAST(GETDATE() AS time) > sd.LateEntryTime
          AND NOT EXISTS (
              SELECT 1
              FROM Attendance_Record ar2
              WHERE ar2.SchoolID = @SchoolID
                AND ar2.StudentID = ass.StudentID
                AND ar2.AttendanceDate = CONVERT(date, GETDATE())
                AND ISNULL(ar2.Attendance_ScheduleID, 0) = sch.ScheduleID
          )
          AND NOT EXISTS (
              SELECT 1
              FROM Attendance_Leave al
              WHERE al.SchoolID = @SchoolID
                AND al.StudentID = ass.StudentID
                AND CONVERT(date, GETDATE()) BETWEEN al.StartDate AND al.EndDate
          )
       )) AS Total_Absent

FROM Attendance_Schedule sch

INNER JOIN Attendance_Schedule_Day sd ON sch.ScheduleID = sd.ScheduleID AND sd.SchoolID = sch.SchoolID

WHERE sch.SchoolID = @SchoolID

  AND sd.Day = DATENAME(dw, GETDATE()) AND sd.Is_OnDay = 1

  AND EXISTS (

      SELECT 1 FROM Attendance_Schedule_AssignStudent ass

      INNER JOIN Student s ON ass.StudentID = s.StudentID AND s.Status = N'Active'

      INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = ass.SchoolID AND scEy.EducationYearID = @EducationYearID

      WHERE ass.SchoolID = @SchoolID AND ass.ScheduleID = sch.ScheduleID

  )

ORDER BY sd.StartTime">

                            <SelectParameters>

                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />

                            </SelectParameters>

                        </asp:SqlDataSource>

                    </div>

                </section>



                <section class="summary-group">

                    <div class="group-header employee">TEACHER / STAFF</div>

                    <div class="att-schedule-cards">

                        <asp:Repeater ID="EmployeeScheduleSummaryRepeater" runat="server" DataSourceID="EmployeeScheduleSummarySQL">

                            <ItemTemplate>

                                <div class="schedule-card tone-<%# Container.ItemIndex % 4 %>" data-schedule-id="<%# Eval("ScheduleID") %>">

                                    <div class="schedule-head">

                                        <span class="schedule-title"><%# Eval("ScheduleName") %></span>

                                        <span class="schedule-time"><%# Eval("TimeRange") %></span>

                                        <span class="schedule-users"><i class="fa fa-users"></i> <%# Eval("Total_User") %></span>

                                    </div>

                                    <div class="schedule-stats compact">

                                        <span class="chip">In <b><%# Eval("Current_IN") %></b></span>

                                        <span class="chip">Out <b><%# Eval("Total_Out") %></b></span>

                                        <span class="chip pre">Pre <b><%# Eval("Total_Present") %></b></span>

                                        <span class="chip late">Late <b><%# Eval("Total_Late") %></b></span>

                                        <span class="chip late-abs">L.Abs <b><%# Eval("Total_Late_Absent") %></b></span>

                                        <span class="chip abs">Abs <b><%# Eval("Total_Absent") %></b></span>

                                    </div>

                                </div>

                            </ItemTemplate>

                        </asp:Repeater>

                        <asp:SqlDataSource ID="EmployeeScheduleSummarySQL" runat="server"

                            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                            SelectCommand="SELECT sch.ScheduleID,

       sch.ScheduleName,

       LTRIM(RIGHT(CONVERT(varchar(20), sd.StartTime, 100), 7)) + N' - ' + LTRIM(RIGHT(CONVERT(varchar(20), sd.EndTime, 100), 7)) AS TimeRange,

       (SELECT COUNT(DISTINCT eas.EmployeeID)

        FROM Employee_Attendance_Schedule_Assign eas

        INNER JOIN Employee_Info e ON eas.EmployeeID = e.EmployeeID

        WHERE eas.SchoolID = @SchoolID AND eas.ScheduleID = sch.ScheduleID AND e.Job_Status = N'Active') AS Total_User,

       (SELECT COUNT(*)

        FROM Employee_Attendance_Record ear
        INNER JOIN Employee_Attendance_Schedule_Assign eas ON eas.EmployeeID = ear.EmployeeID AND eas.ScheduleID = sch.ScheduleID AND eas.SchoolID = @SchoolID
        INNER JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID AND e.Job_Status = N'Active'
        WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = CONVERT(date, GETDATE())

          AND ear.Attendance_ScheduleID = sch.ScheduleID

          AND ear.EntryTime IS NOT NULL AND ear.Is_OUT = 0 AND ear.AttendanceStatus IN (N'Pre', N'Late', N'Late Abs')) AS Current_IN,

       (SELECT COUNT(*)

        FROM Employee_Attendance_Record ear
        INNER JOIN Employee_Attendance_Schedule_Assign eas ON eas.EmployeeID = ear.EmployeeID AND eas.ScheduleID = sch.ScheduleID AND eas.SchoolID = @SchoolID
        INNER JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID AND e.Job_Status = N'Active'
        WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = CONVERT(date, GETDATE())

          AND ear.Attendance_ScheduleID = sch.ScheduleID

          AND ear.ExitTime IS NOT NULL AND ear.Is_OUT = 1) AS Total_Out,

       (SELECT COUNT(*)

        FROM Employee_Attendance_Record ear
        INNER JOIN Employee_Attendance_Schedule_Assign eas ON eas.EmployeeID = ear.EmployeeID AND eas.ScheduleID = sch.ScheduleID AND eas.SchoolID = @SchoolID
        INNER JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID AND e.Job_Status = N'Active'
        WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = CONVERT(date, GETDATE())

          AND ear.Attendance_ScheduleID = sch.ScheduleID AND ear.AttendanceStatus = N'Pre') AS Total_Present,

       (SELECT COUNT(*)

        FROM Employee_Attendance_Record ear
        INNER JOIN Employee_Attendance_Schedule_Assign eas ON eas.EmployeeID = ear.EmployeeID AND eas.ScheduleID = sch.ScheduleID AND eas.SchoolID = @SchoolID
        INNER JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID AND e.Job_Status = N'Active'
        WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = CONVERT(date, GETDATE())

          AND ear.Attendance_ScheduleID = sch.ScheduleID AND ear.AttendanceStatus = N'Late') AS Total_Late,

       (SELECT COUNT(*)

        FROM Employee_Attendance_Record ear
        INNER JOIN Employee_Attendance_Schedule_Assign eas ON eas.EmployeeID = ear.EmployeeID AND eas.ScheduleID = sch.ScheduleID AND eas.SchoolID = @SchoolID
        INNER JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID AND e.Job_Status = N'Active'
        WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = CONVERT(date, GETDATE())

          AND ear.Attendance_ScheduleID = sch.ScheduleID AND ear.AttendanceStatus = N'Late Abs') AS Total_Late_Absent,

       (SELECT COUNT(*)

        FROM Employee_Attendance_Record ear
        INNER JOIN Employee_Attendance_Schedule_Assign eas ON eas.EmployeeID = ear.EmployeeID AND eas.ScheduleID = sch.ScheduleID AND eas.SchoolID = @SchoolID
        INNER JOIN Employee_Info e ON e.EmployeeID = ear.EmployeeID AND e.Job_Status = N'Active'
        WHERE ear.SchoolID = @SchoolID AND ear.AttendanceDate = CONVERT(date, GETDATE())

          AND ear.Attendance_ScheduleID = sch.ScheduleID AND ear.AttendanceStatus = N'Abs') AS Total_Absent

FROM Attendance_Schedule sch

INNER JOIN Attendance_Schedule_Day sd ON sch.ScheduleID = sd.ScheduleID AND sd.SchoolID = sch.SchoolID

WHERE sch.SchoolID = @SchoolID

  AND sd.Day = DATENAME(dw, GETDATE()) AND sd.Is_OnDay = 1

  AND EXISTS (

      SELECT 1 FROM Employee_Attendance_Schedule_Assign eas

      INNER JOIN Employee_Info e ON eas.EmployeeID = e.EmployeeID

      WHERE eas.SchoolID = @SchoolID AND eas.ScheduleID = sch.ScheduleID AND e.Job_Status = N'Active'

  )

ORDER BY sd.StartTime">

                            <SelectParameters>

                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                            </SelectParameters>

                        </asp:SqlDataSource>

                    </div>

                </section>

            </aside>



            <main class="att-logs-panel">

                <div class="att-block student-block">

                    <div class="att-block-head">

                        <div class="att-section-header">STUDENT ATTENDANCE</div>

                        <div class="att-summary-toolbar">

                            <asp:CheckBoxList ID="Student_CheckBoxList" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="Student_CheckBoxList_SelectedIndexChanged">

                                <asp:ListItem Value="Pre">Pre</asp:ListItem>

                                <asp:ListItem Value="Abs">Abs</asp:ListItem>

                                <asp:ListItem>Late</asp:ListItem>

                                <asp:ListItem Value="Late Abs">Late Abs</asp:ListItem>

                            </asp:CheckBoxList>

                        </div>

                    </div>

                    <div class="att-logs-compact">

                        <div class="att-log-row">

                            <div class="att-log-label in-label">IN</div>

                            <div class="IN str_wrap">

                                <asp:Repeater ID="StudentINRepeater" runat="server" DataSourceID="Student_Entry_LogSQL">

                                    <ItemTemplate>

                                        <div class="Info_block">

                                            <div class="card">

                                                <span class="schedule-badge"><%# Eval("ScheduleName") %></span>

                                                <div class="name-title">

                                                    <i class="fa fa-user-o" aria-hidden="true"></i>

                                                    <%# Eval("StudentsName") %>

                                                </div>

                                                <img class="card-img-top" src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%# Eval("StudentID") %>" alt="" />

                                                <span class="notify-badge z-depth-2 <%# Eval("Attendance") %>"><%# Eval("Attendance") %></span>

                                                <div class="EntryDate">

                                                    <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                    <span class="Etime"><%# Eval("EntryTime") %></span>

                                                </div>

                                            </div>

                                        </div>

                                    </ItemTemplate>

                                </asp:Repeater>

                                <asp:SqlDataSource ID="Student_Entry_LogSQL" runat="server"

                                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                                    SelectCommand="SELECT ar.StudentID,

       s.StudentsName,

       ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,

       ar.Attendance,

       CONVERT(varchar(15), ar.EntryTime, 100) AS EntryTime

FROM Attendance_Record ar

INNER JOIN Student s ON ar.StudentID = s.StudentID AND s.Status = N'Active'

INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID

LEFT JOIN Attendance_Schedule sch ON ar.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ar.SchoolID

WHERE ar.AttendanceDate = CONVERT(date, GETDATE())

  AND ar.EntryTime IS NOT NULL AND ar.Is_OUT = 0

  AND ar.Attendance IN (N'Pre', N'Late', N'Late Abs')

  AND ar.SchoolID = @SchoolID

  AND ISNULL(ar.Attendance_ScheduleID, 0) > 0

ORDER BY ar.EntryTime DESC">

                                    <SelectParameters>

                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />

                                    </SelectParameters>

                                </asp:SqlDataSource>

                            </div>

                        </div>

                        <div class="att-log-row">

                            <div class="att-log-label out-label">OUT</div>

                            <div class="OUT str_wrap">

                                <asp:Repeater ID="StudentOUTRepeater" runat="server" DataSourceID="Student_Exit_LogSQL">

                                    <ItemTemplate>

                                        <div class="Info_block">

                                            <div class="card">

                                                <span class="schedule-badge"><%# Eval("ScheduleName") %></span>

                                                <div class="name-title">

                                                    <i class="fa fa-user-o" aria-hidden="true"></i>

                                                    <%# Eval("StudentsName") %>

                                                </div>

                                                <img class="card-img-top" src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%# Eval("StudentID") %>" alt="" />

                                                <span class="notify-badge z-depth-2 <%# Eval("Attendance") %>"><%# Eval("Attendance") %></span>

                                                <div class="EntryDate">

                                                    <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                    <span class="Etime"><%# Eval("EntryTime") %></span>

                                                </div>

                                                <div class="ExitDate">

                                                    <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                    <span class="Extime"><%# Eval("ExitTime") %></span>

                                                </div>

                                            </div>

                                        </div>

                                    </ItemTemplate>

                                </asp:Repeater>

                                <asp:SqlDataSource ID="Student_Exit_LogSQL" runat="server"

                                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                                    SelectCommand="SELECT StudentID, StudentsName, ScheduleName, Attendance, EntryTime, ExitTime

FROM (

SELECT ar.StudentID, s.StudentsName,

       ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,

       ar.Attendance,

       CONVERT(varchar(15), ar.EntryTime, 100) AS EntryTime,

       CONVERT(varchar(15), ar.ExitTime, 100) AS ExitTime,

       ROW_NUMBER() OVER (

           PARTITION BY ar.StudentID, ISNULL(ar.Attendance_ScheduleID, 0)

           ORDER BY CASE ar.Attendance WHEN N'Abs' THEN 2 ELSE 1 END, ar.ExitTime DESC, ar.EntryTime DESC

       ) AS RowNum

FROM Attendance_Record ar

INNER JOIN Student s ON ar.StudentID = s.StudentID AND s.Status = N'Active'

INNER JOIN StudentsClass scEy ON scEy.StudentID = s.StudentID AND scEy.SchoolID = @SchoolID AND scEy.EducationYearID = @EducationYearID

LEFT JOIN Attendance_Schedule sch ON ar.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ar.SchoolID

WHERE ar.AttendanceDate = CONVERT(date, GETDATE())

  AND (ar.Is_OUT = 1 OR ar.Attendance = N'Abs') AND ar.SchoolID = @SchoolID

  AND ISNULL(ar.Attendance_ScheduleID, 0) > 0

) ranked

WHERE RowNum = 1

ORDER BY ExitTime DESC, EntryTime DESC">

                                    <SelectParameters>

                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />

                                    </SelectParameters>

                                </asp:SqlDataSource>

                            </div>

                        </div>

                    </div>

                </div>



                <div class="att-block employee-block">

                    <div class="att-block-head">

                        <div class="att-section-header employee">TEACHER / STAFF ATTENDANCE</div>

                        <div class="att-summary-toolbar">

                            <asp:CheckBoxList ID="Employee_CheckBoxList" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="Employee_CheckBoxList_SelectedIndexChanged">

                                <asp:ListItem Value="Pre">Pre</asp:ListItem>

                                <asp:ListItem Value="Abs">Abs</asp:ListItem>

                                <asp:ListItem>Late</asp:ListItem>

                                <asp:ListItem Value="Late Abs">Late Abs</asp:ListItem>

                            </asp:CheckBoxList>

                            <asp:LinkButton ID="Reload_LinkButton" ToolTip="Reload this page" OnClick="Reload_LinkButton_Click" CssClass="pull-right btn_reload" runat="server"><i class="fa fa-refresh" aria-hidden="true"></i></asp:LinkButton>

                        </div>

                    </div>

                    <div class="att-logs-compact">

                        <div class="att-log-row">

                            <div class="att-log-label in-label">IN</div>

                            <div class="IN str_wrap">

                                <asp:Repeater ID="EmployeeIN_Repeater" runat="server" DataSourceID="Emp_INSQL">

                                    <ItemTemplate>

                                        <div class="Info_block z-depth-1">

                                            <div class="card">

                                                <span class="schedule-badge"><%# Eval("ScheduleName") %></span>

                                                <div class="name-title">

                                                    <i class="fa fa-user-o" aria-hidden="true"></i>

                                                    <%# Eval("Name") %>

                                                </div>

                                                <img class="card-img-top" src="/Handeler/Employee_Image.ashx?Img=<%# Eval("EmployeeID") %>" alt="" />

                                                <span class="notify-badge z-depth-2 <%# Eval("AttendanceStatus") %>"><%# Eval("AttendanceStatus") %></span>

                                                <div class="EntryDate">

                                                    <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                    <span class="Etime"><%# Eval("EntryTime") %></span>

                                                </div>

                                            </div>

                                        </div>

                                    </ItemTemplate>

                                </asp:Repeater>

                                <asp:SqlDataSource ID="Emp_INSQL" runat="server"

                                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                                    SelectCommand="SELECT ear.EmployeeID,

       e.FirstName + N' ' + e.LastName AS Name,

       ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,

       ear.AttendanceStatus,

       CONVERT(varchar(15), ear.EntryTime, 100) AS EntryTime

FROM Employee_Attendance_Record ear

INNER JOIN VW_Emp_Info e ON ear.EmployeeID = e.EmployeeID

LEFT JOIN Attendance_Schedule sch ON ear.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ear.SchoolID

WHERE ear.AttendanceDate = CONVERT(date, GETDATE())

  AND ear.EntryTime IS NOT NULL AND ear.Is_OUT = 0

  AND ear.AttendanceStatus IN (N'Pre', N'Late', N'Late Abs')

  AND ear.SchoolID = @SchoolID

  AND ISNULL(ear.Attendance_ScheduleID, 0) > 0

ORDER BY ear.EntryTime DESC">

                                    <SelectParameters>

                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                                    </SelectParameters>

                                </asp:SqlDataSource>

                            </div>

                        </div>

                        <div class="att-log-row">

                            <div class="att-log-label out-label">OUT</div>

                            <div class="OUT str_wrap">

                                <asp:Repeater ID="EmployeeOUT_Repeater" runat="server" DataSourceID="Emp_OUTSQL">

                                    <ItemTemplate>

                                        <div class="Info_block">

                                            <div class="card">

                                                <span class="schedule-badge"><%# Eval("ScheduleName") %></span>

                                                <div class="name-title">

                                                    <i class="fa fa-user-o" aria-hidden="true"></i>

                                                    <%# Eval("Name") %>

                                                </div>

                                                <img class="card-img-top" src="/Handeler/Employee_Image.ashx?Img=<%# Eval("EmployeeID") %>" alt="" />

                                                <span class="notify-badge z-depth-2 <%# Eval("AttendanceStatus") %>"><%# Eval("AttendanceStatus") %></span>

                                                <div class="EntryDate">

                                                    <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                    <%# Eval("EntryTime") %>

                                                </div>

                                                <div class="ExitDate">

                                                    <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                    <%# Eval("ExitTime") %>

                                                </div>

                                            </div>

                                        </div>

                                    </ItemTemplate>

                                </asp:Repeater>

                                <asp:SqlDataSource ID="Emp_OUTSQL" runat="server"

                                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                                    SelectCommand="SELECT EmployeeID, Name, ScheduleName, AttendanceStatus, EntryTime, ExitTime

FROM (

SELECT ear.EmployeeID, e.FirstName + N' ' + e.LastName AS Name,

       ISNULL(sch.ScheduleName, N'Schedule') AS ScheduleName,

       ear.AttendanceStatus,

       CONVERT(varchar(15), ear.EntryTime, 100) AS EntryTime,

       CONVERT(varchar(15), ear.ExitTime, 100) AS ExitTime,

       ROW_NUMBER() OVER (

           PARTITION BY ear.EmployeeID, ISNULL(ear.Attendance_ScheduleID, 0)

           ORDER BY CASE ear.AttendanceStatus WHEN N'Abs' THEN 2 ELSE 1 END, ear.ExitTime DESC, ear.EntryTime DESC

       ) AS RowNum

FROM Employee_Attendance_Record ear

INNER JOIN VW_Emp_Info e ON ear.EmployeeID = e.EmployeeID

LEFT JOIN Attendance_Schedule sch ON ear.Attendance_ScheduleID = sch.ScheduleID AND sch.SchoolID = ear.SchoolID

WHERE ear.AttendanceDate = CONVERT(date, GETDATE())

  AND (ear.Is_OUT = 1 OR ear.AttendanceStatus = N'Abs') AND ear.SchoolID = @SchoolID

  AND ISNULL(ear.Attendance_ScheduleID, 0) > 0

) ranked

WHERE RowNum = 1

ORDER BY ExitTime DESC, EntryTime DESC">

                                    <SelectParameters>

                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />

                                    </SelectParameters>

                                </asp:SqlDataSource>

                            </div>

                        </div>

                    </div>

                </div>

            </main>



            </div>

        </ContentTemplate>

    </asp:UpdatePanel>



    <asp:UpdateProgress ID="UpdateProgress" runat="server">

        <ProgressTemplate>

            <div id="progress_BG"></div>

            <div id="progress">

                <img src="../../CSS/loading.gif" alt="Loading..." />

                <br />

                <b>Loading...</b>

            </div>

        </ProgressTemplate>

    </asp:UpdateProgress>



    <script src="js/jquery.liMarquee.js"></script>

    <script type="text/javascript">

        function resizeScheduleCards() {

            $('.att-schedule-cards').each(function () {

                var $wrap = $(this);

                if ($wrap.hasClass('schedule-grid')) return;

                var count = $wrap.children('.schedule-card').length;

                $wrap.removeClass('sched-count-1 sched-count-2 sched-count-3 sched-count-many');

                if (count === 1) $wrap.addClass('sched-count-1');

                else if (count === 2) $wrap.addClass('sched-count-2');

                else if (count === 3) $wrap.addClass('sched-count-3');

                else if (count > 3) $wrap.addClass('sched-count-many');

            });

        }



        function initAttendanceMarquee() {

            $('.IN, .OUT').each(function () {

                var $el = $(this);

                if ($el.data('liMarquee')) {

                    $el.liMarquee('destroy');

                }

            });



            $(".Etime").each(function () {

                $(this).parent('.EntryDate').toggle($(this).text().trim() !== "");

            });

            $(".Extime").each(function () {

                $(this).parent('.ExitDate').toggle($(this).text().trim() !== "");

            });



            $('.IN').liMarquee({

                direction: 'left',

                loop: -1,

                scrolldelay: 0,

                scrollamount: 50,

                circular: true,

                drag: true

            });



            $('.OUT').liMarquee({

                direction: 'right',

                loop: -1,

                scrolldelay: 0,

                scrollamount: 20,

                circular: true,

                drag: true

            });

        }



        $(function () {

            resizeScheduleCards();

            initAttendanceMarquee();

        });



        var prm = Sys.WebForms.PageRequestManager.getInstance();

        prm.add_endRequest(function () {

            resizeScheduleCards();

            initAttendanceMarquee();

        });

    </script>

</asp:Content>

