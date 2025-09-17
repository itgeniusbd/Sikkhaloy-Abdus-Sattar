<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Employee_Exit_Log_Display.aspx.cs" Inherits="EDUCATION.COM.Attendances.Display_User.Employee_Exit_Log_Display" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link href="CSS/Style.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">

        <asp:Repeater ID="EmployeeExitLog" runat="server" DataSourceID="EmployeeExitLogSQL">
            <ItemTemplate>
                <%# Eval("Name") %>
                <img src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>" />
                <%# Eval("ExitTime") %>
            </ItemTemplate>
        </asp:Repeater>

        <asp:SqlDataSource ID="EmployeeExitLogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Attendance_Record.EmployeeID, VW_Emp_Info.FirstName+' '+VW_Emp_Info.LastName as Name, VW_Emp_Info.Designation, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, 
        CONVERT(varchar(15), Employee_Attendance_Record.EntryTime, 100) AS EntryTime, CONVERT(varchar(15), Employee_Attendance_Record.ExitTime, 100) AS ExitTime, Employee_Attendance_Record.AttendanceStatus FROM Employee_Attendance_Record INNER JOIN
        VW_Emp_Info ON Employee_Attendance_Record.EmployeeID = VW_Emp_Info.EmployeeID
WHERE (Employee_Attendance_Record.AttendanceDate = CONVERT(date, GETDATE())) AND (Employee_Attendance_Record.SchoolID = @SchoolID)
ORDER BY Employee_Attendance_Record.ExitTime DESC">
            <SelectParameters>
                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </form>
</body>
</html>
