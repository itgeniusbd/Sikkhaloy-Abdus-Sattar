<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Device_Attendance_Report.aspx.cs" Inherits="EDUCATION.COM.Attendances.Display_User.Device_Attendance_Report" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="CSS/Summery.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:FormView ID="StudentSummaryFV" runat="server" DataSourceID="StudentSummarySQL" RenderOuterTable="false">
            <ItemTemplate>
                <%# Eval("Total_Student") %>
                <%# Eval("Current_IN_Student") %>
                <%# Eval("Total_Out_Student") %>
                <%# Eval("Total_Student_Prasent") %>
                <%# Eval("Total_Student_Late") %>
                <%# Eval("Total_Student_Absent") %>
                <%# Eval("Total_Student_Late_Absent") %> 
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="StudentSummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="select (SELECT  COUNT(Student.StudentID) FROM  Student INNER JOIN
           StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID
WHERE (Student.Status = N'Active') AND (Education_Year.Status = N'True') AND (Student.SchoolID = @SchoolID))AS Total_Student,

(SELECT COUNT(StudentID)  FROM Attendance_Record
WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID))AS Current_IN_Student,

(SELECT COUNT(StudentID)  FROM Attendance_Record
WHERE(AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID))AS Total_Out_Student,

(SELECT COUNT(StudentID) FROM Attendance_Record
WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Pre'))AS Total_Student_Prasent,
(SELECT COUNT(StudentID) FROM Attendance_Record
WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Late')) AS Total_Student_Late,
(SELECT COUNT(StudentID) FROM Attendance_Record
WHERE(AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Abs'))AS Total_Student_Absent,
(SELECT COUNT(StudentID) FROM Attendance_Record
WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Late Abs'))AS Total_Student_Late_Absent">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    

        <asp:FormView ID="EmployeeSummaryFormView" runat="server" DataSourceID="EmployeeSummarySQL" RenderOuterTable="false">
            <ItemTemplate>
                <%# Eval("Total_Employee") %>
                <%# Eval("Current_IN_Employee") %>
                <%# Eval("Total_Out_Employee") %>
                <%# Eval("Total_Employee_Prasent") %>
                <%# Eval("Total_Employee_Late") %>
                <%# Eval("Total_Employee_Absent") %>
                <%# Eval("Total_Employee_Late_Absent") %>  
            </ItemTemplate>
        </asp:FormView>
        <asp:SqlDataSource ID="EmployeeSummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="select 

(SELECT  Count(EmployeeID)  FROM Employee_Info WHERE (Job_Status = N'Active') AND (SchoolID = @SchoolID))AS Total_Employee,

(SELECT COUNT(*)  FROM Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (ExitConfirmed_Status = N'No') AND (SchoolID = @SchoolID))AS Current_IN_Employee,

(SELECT COUNT(*)  FROM Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (ExitConfirmed_Status = N'Yes') AND (SchoolID = @SchoolID))AS Total_Out_Employee,

(SELECT        COUNT(*) 
FROM            Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Pre'))AS Total_Employee_Prasent,

(SELECT        COUNT(*)
FROM            Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Late')) AS Total_Employee_Late,

(SELECT        COUNT(*) 
FROM            Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Abs'))AS Total_Employee_Absent,
(SELECT        COUNT(*) 
FROM            Employee_Attendance_Record
WHERE        (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Late Abs'))AS Total_Employee_Late_Absent">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </form>
</body>
</html>
