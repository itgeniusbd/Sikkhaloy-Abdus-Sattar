<%@ Page Title="Class Routine" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="Class_Routine.aspx.cs" Inherits="EDUCATION.COM.Teacher.Class_Routine" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .DRoGrid { border: 1px solid #a2a2a2; width: 100%; }
            .DRoGrid th { border: medium none; background-color: #191a6d; }
            .DRoGrid td { border: 1px solid #ddd;padding:4px; }
                .DRoGrid td table td { border: none; }

        .R_Day { background-color: #28bcad; text-align: center; vertical-align: middle; color: #fff; }
        .Period { color: #00bd02; font-size:1.2rem}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Class Routine</h3>
    <asp:GridView ID="DayGridView" runat="server" ShowHeader="false" AutoGenerateColumns="False" DataSourceID="RoutineDaySQL" CssClass="DRoGrid">
        <Columns>
            <asp:BoundField DataField="Day">
                <ItemStyle CssClass="R_Day" />
            </asp:BoundField>
            <asp:TemplateField>
                <ItemTemplate>
                    <asp:DataList ID="SubteacherDataList" runat="server" DataSourceID="RoutineTimeSQL" RepeatDirection="Horizontal">
                        <ItemTemplate>
                            <div class="Timing mb-2">
                                <span class="Period"><%# Eval("RoutinePeriod") %></span>
                                (<span class="Period"><%# Eval("Duration") %></span>)<br />
                                <span><%# Eval("Time") %></span>
                            </div>

                            <h6>
                                <%# Eval("Class") %>
                                <span class="badge badge-primary"><%# Eval("SubjectGroup") %></span>
                                <span class="badge badge-info"><%# Eval("Section") %></span>
                                <span class="badge badge-danger"><%# Eval("Shift") %></span>
                            </h6>
                            <%# Eval("SubjectName") %>
                        </ItemTemplate>
                    </asp:DataList>
                    <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        RoutineTime.RoutinePeriod, RoutineTime.StartTime, RoutineTime.EndTime, RoutineTime.Duration, Subject.SubjectName, CreateShift.Shift, CreateClass.Class, CreateSubjectGroup.SubjectGroup, 
						 CreateSection.Section,
							 (SELECT        CONVERT(varchar(15), CAST(RoutineTime.StartTime AS TIME), 100) AS Expr1) + ' - ' +
							 (SELECT        CONVERT(varchar(15), CAST(RoutineTime.EndTime AS TIME), 100) AS Expr1) AS Time
FROM            RoutineForClass INNER JOIN
						 RoutineTime ON RoutineForClass.RoutineTimeID = RoutineTime.RoutineTimeID INNER JOIN
						 Subject ON RoutineForClass.SubjectID = Subject.SubjectID INNER JOIN
						 CreateClass ON RoutineForClass.ClassID = CreateClass.ClassID LEFT OUTER JOIN
						 CreateSection ON RoutineForClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN
						 CreateSubjectGroup ON RoutineForClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN
						 CreateShift ON RoutineForClass.ShiftID = CreateShift.ShiftID
WHERE        (RoutineForClass.SchoolID = @SchoolID) AND (RoutineForClass.EducationYearID = @EducationYearID) AND (RoutineForClass.TeacherID = @TeacherID) AND (RoutineForClass.Day = @Day)
ORDER BY RoutineTime.StartTime">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
                            <asp:ControlParameter ControlID="DayHF" Name="Day" PropertyName="Value" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:HiddenField ID="DayHF" runat="server" Value='<%# Eval("Day") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:SqlDataSource ID="RoutineDaySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT RoutineForClass.Day,
CASE WHEN RoutineForClass.Day = 'Sat' THEN 1 
	 WHEN RoutineForClass.Day = 'Sun' THEN 2 
	 WHEN RoutineForClass.Day = 'Mon' THEN 3 
	 WHEN RoutineForClass.Day = 'Tue' THEN 4 
	 WHEN RoutineForClass.Day = 'Wed' THEN 5 
	 WHEN RoutineForClass.Day = 'Thu' THEN 6 
	 WHEN RoutineForClass.Day = 'Fri' THEN 7 
	 
END as Ascending
FROM            RoutineForClass INNER JOIN
						 RoutineDay ON RoutineForClass.RoutineInfoID = RoutineDay.RoutineInfoID AND RoutineForClass.Day = RoutineDay.Day
WHERE        (RoutineForClass.SchoolID = @SchoolID) AND (RoutineForClass.EducationYearID = @EducationYearID) AND (RoutineForClass.TeacherID = @TeacherID)
ORDER BY 
Ascending">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $("#_3").addClass("active");
        });
    </script>
</asp:Content>
