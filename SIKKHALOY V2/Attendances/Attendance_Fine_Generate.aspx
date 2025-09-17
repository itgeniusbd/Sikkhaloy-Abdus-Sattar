<%@ Page Title="Attendance Fine" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Attendance_Fine_Generate.aspx.cs" Inherits="EDUCATION.COM.Attendances.Attendance_Fine_Generate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Attendance Fine</h3>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>  </ContentTemplate>
    </asp:UpdatePanel>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" onfocus="SelectedItemCLR(this);" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.Class, StudentsClass.ClassID FROM StudentsClass INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = N'Active') ORDER BY StudentsClass.ClassID">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" ValidationGroup="1" CssClass="EroorSummer" InitialValue="0" ControlToValidate="ClassDropDownList" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group mx-1">
                    <asp:DropDownList ID="MonthNameDropDownList" AutoPostBack="true" runat="server" CssClass="form-control" DataSourceID="MonthNameSQL" DataTextField="Month_Name" DataValueField="date" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ SELECT MONTH ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="MonthNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="with months (date) AS (SELECT StartDate FROM  Education_Year WHERE (EducationYearID = @EducationYearID)
UNION ALL SELECT DATEADD(month,1,date) from months where DATEADD(month,1,date)&lt;= (SELECT EndDate FROM  Education_Year WHERE (EducationYearID = @EducationYearID)))
select  FORMAT(Date,'MMM yyyy') as Month_Name, date from months">
                        <SelectParameters>
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" ValidationGroup="1" CssClass="EroorSummer" InitialValue="0" ControlToValidate="MonthNameDropDownList" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:Button ID="GenarateButton" ValidationGroup="1" OnClick="GenarateButton_Click" runat="server" Text="Submit" CssClass="btn btn-primary" />
                    <asp:SqlDataSource ID="FineSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_Monthly_Report.SchoolID, Attendance_Monthly_Report.ClassID, Attendance_Monthly_Report.RegistrationID, Attendance_Monthly_Report.Monthly_ReportID, Attendance_Monthly_Report.StudentID, Attendance_Monthly_Report.EducationYearID, Attendance_Monthly_Report.StudentClassID, Attendance_Monthly_Report.MonthName, Attendance_Monthly_Report.FineAmount, Attendance_Monthly_Report.WorkingDays, Attendance_Monthly_Report.TotalPresent, Attendance_Monthly_Report.TotalAbsent, Attendance_Monthly_Report.TotalLateAbs, Attendance_Monthly_Report.Abs_Count, Attendance_Monthly_Report.TotalLate, Attendance_Monthly_Report.TotalLeave, Attendance_Monthly_Report.TotalBunk, Attendance_Monthly_Report.PayOrderID, Student.ID, Student.StudentsName, CreateClass.Class FROM Attendance_Monthly_Report INNER JOIN CreateClass ON Attendance_Monthly_Report.ClassID = CreateClass.ClassID INNER JOIN Student ON Attendance_Monthly_Report.StudentID = Student.StudentID WHERE (Attendance_Monthly_Report.SchoolID = @SchoolID) AND (Attendance_Monthly_Report.ClassID = @ClassID) AND (Attendance_Monthly_Report.MonthName = @MonthName)" UpdateCommand="Student_Monthly_AttendanceFine" UpdateCommandType="StoredProcedure">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="MonthNameDropDownList" Name="MonthName" PropertyName="SelectedItem.Text" />
                        </SelectParameters>
                        <UpdateParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                            <asp:ControlParameter ControlID="MonthNameDropDownList" DbType="Date" Name="Get_date" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="MonthNameDropDownList" Name="MonthName" PropertyName="SelectedItem.Text" Type="String" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="FineGridView" runat="server" CssClass="mGrid" DataSourceID="FineSQL" AutoGenerateColumns="False" DataKeyNames="Monthly_ReportID">
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                        <asp:BoundField DataField="MonthName" HeaderText="Month" SortExpression="MonthName" />
                        <asp:BoundField DataField="FineAmount" HeaderText="Fine Amount" SortExpression="FineAmount" />
                        <asp:BoundField DataField="WorkingDays" HeaderText="W.D" SortExpression="WorkingDays" />
                        <asp:BoundField DataField="TotalPresent" HeaderText="Present" SortExpression="TotalPresent" />
                        <asp:BoundField DataField="TotalAbsent" HeaderText="Absent" SortExpression="TotalAbsent" />
                        <asp:BoundField DataField="TotalLateAbs" HeaderText="Late Abs." SortExpression="TotalLateAbs" />
                        <asp:BoundField DataField="Abs_Count" HeaderText="Abs Count" ReadOnly="True" SortExpression="Abs_Count" />
                        <asp:BoundField DataField="TotalLate" HeaderText="Late" SortExpression="TotalLate" />
                        <asp:BoundField DataField="TotalLeave" HeaderText="Leave" SortExpression="TotalLeave" />
                        <asp:BoundField DataField="TotalBunk" HeaderText="Bunk" SortExpression="TotalBunk" />
                    </Columns>
                </asp:GridView>
            </div>
      

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
</asp:Content>
