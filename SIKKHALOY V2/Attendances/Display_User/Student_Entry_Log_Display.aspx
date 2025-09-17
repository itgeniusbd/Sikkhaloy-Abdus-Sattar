<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Student_Entry_Log_Display.aspx.cs" Inherits="EDUCATION.COM.Attendances.Display_User.Student_Log_Display" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link href="CSS/Style.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:Repeater ID="StudentEntryLog" runat="server" DataSourceID="Student_Entry_LogSQL">
            <ItemTemplate>
                <%# Eval("StudentsName") %>
                <%# Eval("EntryTime") %>
                <img src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" />
                <%# Eval("ID") %>
                <%# Eval("Class") %>
                <%# Eval("RollNo") %>
                <%# Eval("EntryTime") %>
                <%# Eval("Attendance") %>
            </ItemTemplate>
        </asp:Repeater>
        <asp:SqlDataSource ID="Student_Entry_LogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_Record.StudentID, Student.StudentsName, Student.ID, CreateClass.Class, StudentsClass.RollNo, Attendance_Record.Attendance, CONVERT(varchar(15), Attendance_Record.EntryTime, 100) AS EntryTime
FROM Attendance_Record INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID INNER JOIN
StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN
CreateClass ON StudentsClass.ClassID = CreateClass.ClassID
WHERE (Attendance_Record.AttendanceDate = CONVERT(date, GETDATE())) AND (Attendance_Record.SchoolID = @SchoolID)
ORDER BY Attendance_Record.EntryTime DESC">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </form>
</body>
</html>
