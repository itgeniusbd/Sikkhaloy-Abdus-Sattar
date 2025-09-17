<%@ Page Title="Attendance Display" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Attendance_Slider.aspx.cs" Inherits="EDUCATION.COM.Attendances.Online_Display.Attendance_Slider" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Display.css?v=9.4" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>      
            <div class="row">
                <div class="col-md-3 mb-3">
                    <ul class="list-group list-group-flush z-depth-1">
                        <li class="list-group-item active">
                            <asp:CheckBoxList ID="Employee_CheckBoxList" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="Employee_CheckBoxList_SelectedIndexChanged">
                                <asp:ListItem Value="Pre">Pre</asp:ListItem>
                                <asp:ListItem Value="Abs">Abs</asp:ListItem>
                                <asp:ListItem>Late</asp:ListItem>
                                <asp:ListItem Value="Late Abs">Late Abs</asp:ListItem>
                            </asp:CheckBoxList>

                            <asp:LinkButton ID="Reload_LinkButton" ToolTip="Reload this page" OnClick="Reload_LinkButton_Click" CssClass="pull-right btn_reload" runat="server"><i class="fa fa-refresh" aria-hidden="true"></i></asp:LinkButton>
                        </li>
                        <asp:FormView ID="Employee_FormView" runat="server" DataSourceID="Emp_Att_Report_SQL" Width="100%">
                            <ItemTemplate>
                                <li class="list-group-item font-weight-bold">Total Employee
                            <span class="badge badge-pill badge-primary pull-right"><%#Eval("Total_Employee") %></span>
                                </li>
                                <li class="list-group-item">Current IN
                            <span class="badge badge-pill badge-secondary pull-right"><%#Eval("Current_IN_Employee") %></span>
                                </li>
                                <li class="list-group-item">OUT
                            <span class="badge badge-pill badge-dark pull-right"><%#Eval("Total_Out_Employee") %></span>
                                </li>
                                <li class="list-group-item">Present
                            <span class="badge badge-pill Pre pull-right"><%#Eval("Total_Employee_Prasent") %></span>
                                </li>
                                <li class="list-group-item">Late
                            <span class="badge badge-pill Late pull-right"><%#Eval("Total_Employee_Late") %></span>
                                </li>
                                <li class="list-group-item">Late Absent
                            <span class="badge badge-pill Late Abs pull-right"><%#Eval("Total_Employee_Late_Absent") %></span>
                                </li>
                                <li class="list-group-item">Absent
                            <span class="badge badge-pill Abs pull-right"><%#Eval("Total_Employee_Absent") %></span>
                                </li>
                            </ItemTemplate>
                        </asp:FormView>
                        <asp:SqlDataSource ID="Emp_Att_Report_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT (SELECT  Count(EmployeeID)  FROM Employee_Info WHERE (Job_Status = N'Active') AND (SchoolID = @SchoolID))AS Total_Employee,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (EntryTime IS NOT NULL) AND (Is_OUT = 0) AND (SchoolID = @SchoolID))AS Current_IN_Employee,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (ExitTime IS NOT NULL) AND (Is_OUT = 1) AND (SchoolID = @SchoolID))AS Total_Out_Employee,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Pre'))AS Total_Employee_Prasent,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Late')) AS Total_Employee_Late,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Abs'))AS Total_Employee_Absent,
(SELECT COUNT(*) FROM Employee_Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (AttendanceStatus = 'Late Abs'))AS Total_Employee_Late_Absent">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ul>
                </div>

                <div class="col-md-9 mb-3">
                    <div class="card">
                        <div class="card-body p-3">
                            <div class="IN str_wrap">
                                <asp:Repeater ID="EmployeeIN_Repeater" runat="server" DataSourceID="Emp_INSQL">
                                    <ItemTemplate>
                                        <div class="Info_block z-depth-1">
                                            <div class="card">
                                                <div class="name-title">
                                                    <i class="fa fa-user-o" aria-hidden="true"></i>
                                                    <%# Eval("Name") %>
                                                </div>
                                                <img class="card-img-top" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>">
                                                <span class="notify-badge z-depth-2 <%#Eval("AttendanceStatus") %>"><%# Eval("AttendanceStatus") %></span>
                                                <div class="EntryDate">
                                                    <i class="fa fa-clock-o" aria-hidden="true"></i>
                                                    <span class="Etime"><%# Eval("EntryTime") %></span>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="Emp_INSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Attendance_Record.EmployeeID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, CONVERT (varchar(15), Employee_Attendance_Record.EntryTime, 100) AS EntryTime, Employee_Attendance_Record.AttendanceStatus FROM Employee_Attendance_Record INNER JOIN VW_Emp_Info ON Employee_Attendance_Record.EmployeeID = VW_Emp_Info.EmployeeID WHERE (Employee_Attendance_Record.AttendanceDate = CONVERT (date, GETDATE())) AND (Employee_Attendance_Record.EntryTime IS NOT NULL) AND (Employee_Attendance_Record.Is_OUT = 0) AND (Employee_Attendance_Record.SchoolID = @SchoolID)  ORDER BY EntryTime DESC">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>

                            <div class="OUT str_wrap mt-3">
                                <asp:Repeater ID="EmployeeOUT_Repeater" runat="server" DataSourceID="Emp_OUTSQL">
                                    <ItemTemplate>
                                        <div class="Info_block">
                                            <div class="card">
                                                <div class="name-title">
                                                    <i class="fa fa-user-o" aria-hidden="true"></i>
                                                    <%# Eval("Name") %>
                                                </div>
                                                <img class="card-img-top" src="/Handeler/Employee_Image.ashx?Img=<%#Eval("EmployeeID") %>">
                                                <span class="notify-badge z-depth-2 <%#Eval("AttendanceStatus") %>"><%# Eval("AttendanceStatus") %></span>
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
                                <asp:SqlDataSource ID="Emp_OUTSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Employee_Attendance_Record.EmployeeID, VW_Emp_Info.FirstName + ' ' + VW_Emp_Info.LastName AS Name, VW_Emp_Info.Designation, VW_Emp_Info.ID, VW_Emp_Info.EmployeeType, CONVERT (varchar(15), Employee_Attendance_Record.EntryTime, 100) AS EntryTime, CONVERT (varchar(15), Employee_Attendance_Record.ExitTime, 100) AS ExitTime, Employee_Attendance_Record.AttendanceStatus FROM Employee_Attendance_Record INNER JOIN VW_Emp_Info ON Employee_Attendance_Record.EmployeeID = VW_Emp_Info.EmployeeID WHERE (Employee_Attendance_Record.AttendanceDate = CONVERT (date, GETDATE())) AND (Employee_Attendance_Record.Is_OUT = 1 or Employee_Attendance_Record.AttendanceStatus='Abs') AND (Employee_Attendance_Record.SchoolID = @SchoolID) ORDER BY ExitTime DESC">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
         
            <div class="row">
                <div class="col-md-3 mb-3">
                    <ul class="list-group list-group-flush z-depth-1">
                        <li class="list-group-item active bg-success border-0">
                            <asp:CheckBoxList ID="Student_CheckBoxList" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="True" OnSelectedIndexChanged="Student_CheckBoxList_SelectedIndexChanged">
                                <asp:ListItem Value="Pre">Pre</asp:ListItem>
                                <asp:ListItem Value="Abs">Abs</asp:ListItem>
                                <asp:ListItem>Late</asp:ListItem>
                                <asp:ListItem Value="Late Abs">Late Abs</asp:ListItem>
                            </asp:CheckBoxList>
                        </li>
                        <asp:FormView ID="Student_FormView" runat="server" DataSourceID="Stu_Att_Report_SQL" Width="100%">
                            <ItemTemplate>
                                <li class="list-group-item font-weight-bold">Total Student
                            <span class="badge badge-pill badge-primary pull-right"><%#Eval("Total_Student") %></span>
                                </li>
                                <li class="list-group-item">Current IN
                            <span class="badge badge-pill badge-secondary pull-right"><%#Eval("Current_IN_Student") %></span>
                                </li>
                                <li class="list-group-item">OUT
                            <span class="badge badge-pill badge-dark pull-right"><%#Eval("Total_Out_Student") %></span>
                                </li>
                                <li class="list-group-item">Present
                            <span class="badge badge-pill Pre pull-right"><%#Eval("Total_Student_Prasent") %></span>
                                </li>
                                <li class="list-group-item">Late
                            <span class="badge badge-pill Late pull-right"><%#Eval("Total_Student_Late") %></span>
                                </li>
                                <li class="list-group-item">Late Absent
                            <span class="badge badge-pill Late Abs pull-right"><%#Eval("Total_Student_Late_Absent") %></span>
                                </li>
                                <li class="list-group-item">Absent
                            <span class="badge badge-pill Abs pull-right"><%#Eval("Total_Student_Absent") %></span>
                                </li>
                            </ItemTemplate>
                        </asp:FormView>
                        <asp:SqlDataSource ID="Stu_Att_Report_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT (SELECT COUNT(*) FROM VW_Attendance_Users WHERE (SchoolID = @SchoolID) and (Is_Student = 1))AS Total_Student,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (EntryTime IS NOT NULL) AND (Is_OUT = 0) AND (SchoolID = @SchoolID))AS Current_IN_Student,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (ExitTime IS NOT NULL) AND (Is_OUT = 1) AND (SchoolID = @SchoolID))AS Total_Out_Student,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Pre'))AS Total_Student_Prasent,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Late')) AS Total_Student_Late,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Abs'))AS Total_Student_Absent,
(SELECT COUNT(StudentID) FROM Attendance_Record WHERE (AttendanceDate = CONVERT(date, GETDATE())) AND (SchoolID = @SchoolID) AND (Attendance = 'Late Abs'))AS Total_Student_Late_Absent">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ul>
                </div>
                <div class="col-md-9">
                    <div class="card">
                        <div class="card-body p-3">
                            <div class="IN str_wrap">
                                <asp:Repeater ID="StudentINRepeater" runat="server" DataSourceID="Student_Entry_LogSQL">
                                    <ItemTemplate>
                                        <div class="Info_block">
                                            <div class="card">
                                                <div class="name-title">
                                                    <i class="fa fa-user-o" aria-hidden="true"></i>
                                                    <%# Eval("StudentsName") %>
                                                </div>
                                                <img class="card-img-top" src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" />
                                                <span class="notify-badge z-depth-2 <%#Eval("Attendance") %>"><%# Eval("Attendance") %></span>
                                                <div class="EntryDate">
                                                    <i class="fa fa-clock-o" aria-hidden="true"></i>
                                                    <span class="Etime"><%# Eval("EntryTime") %></span>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="Student_Entry_LogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_Record.StudentID, Student.StudentsName, Student.ID, CreateClass.Class, StudentsClass.RollNo, Attendance_Record.Attendance, CONVERT (varchar(15), Attendance_Record.EntryTime, 100) AS EntryTime FROM Attendance_Record INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID INNER JOIN StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Attendance_Record.AttendanceDate = CONVERT (date, GETDATE())) AND (Attendance_Record.EntryTime IS NOT NULL) AND (Attendance_Record.Is_OUT = 0) AND (Attendance_Record.SchoolID = @SchoolID) ORDER BY EntryTime DESC">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>

                            <div class="OUT str_wrap mt-3">
                                <asp:Repeater ID="StudentOUTRepeater" runat="server" DataSourceID="Student_Exit_LogSQL">
                                    <ItemTemplate>
                                        <div class="Info_block">
                                            <div class="card">
                                                <div class="name-title">
                                                    <i class="fa fa-user-o" aria-hidden="true"></i>
                                                    <%# Eval("StudentsName") %>
                                                </div>
                                                <img class="card-img-top" src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%#Eval("StudentID") %>" />
                                                <span class="notify-badge z-depth-2 <%#Eval("Attendance") %>"><%# Eval("Attendance") %></span>
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
                                <asp:SqlDataSource ID="Student_Exit_LogSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_Record.StudentID, Student.StudentsName, Student.ID, CreateClass.Class, StudentsClass.RollNo, Attendance_Record.Attendance, CONVERT (varchar(15), Attendance_Record.EntryTime, 100) AS EntryTime, CONVERT (varchar(15), Attendance_Record.ExitTime, 100) AS ExitTime FROM Attendance_Record INNER JOIN Student ON Attendance_Record.StudentID = Student.StudentID INNER JOIN StudentsClass ON Attendance_Record.StudentClassID = StudentsClass.StudentClassID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Attendance_Record.AttendanceDate = CONVERT (date, GETDATE())) AND (Attendance_Record.Is_OUT = 1 or Attendance_Record.Attendance ='Abs') AND (Attendance_Record.SchoolID = @SchoolID) ORDER BY ExitTime DESC">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </div>
                    </div>
                </div>
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
        $(function () {
            $(".Etime").each(function () {
                if ($(this).text() == "") {
                    $(this).parent('.EntryDate').hide();
                }
            });
            $(".Extime").each(function () {
                if ($(this).text() == "") {
                    $(this).parent('.ExitDate').hide();
                }
            });

            //Employee
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
        });

        //Update Panel
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            $(".Etime").each(function () {
                if ($(this).text() == "") {
                    $(this).parent('.EntryDate').hide();
                }
            });
            $(".Extime").each(function () {
                if ($(this).text() == "") {
                    $(this).parent('.ExitDate').hide();
                }
            });


            //Employee
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
        });
    </script>
</asp:Content>
