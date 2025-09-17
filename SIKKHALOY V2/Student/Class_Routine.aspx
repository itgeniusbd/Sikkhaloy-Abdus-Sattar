<%@ Page Title="Class Routine" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Class_Routine.aspx.cs" Inherits="EDUCATION.COM.Student.Class_Routine" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Routines/CSS/Routine.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Class Routine</h3>

    <div class="table-responsive">
        <asp:Repeater ID="Multi_RoutineRepeater" runat="server" DataSourceID="MultiRoutineSQL">
            <ItemTemplate>
                <asp:HiddenField ID="RoutineInfoID_HF" runat="server" Value='<%# Eval("RoutineInfoID") %>' />
                <asp:HiddenField ID="ClassID_HF" runat="server" Value='<%# Eval("ClassID") %>' />
                <asp:HiddenField ID="SubjectGroupID_HF" runat="server" Value='<%# Eval("SubjectGroupID") %>' />
                <asp:HiddenField ID="ShiftID_HF" runat="server" Value='<%# Eval("ShiftID") %>' />
                <asp:HiddenField ID="SectionID_HF" runat="server" Value='<%# Eval("SectionID") %>' />

                <asp:GridView ID="DayGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RoutineDayID" DataSourceID="RoutineDaySQL" CssClass="DRoGrid">
                    <Columns>
                        <asp:BoundField DataField="Day">
                            <ItemStyle CssClass="R_Day" />
                        </asp:BoundField>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:DataList ID="PeriodDataList" runat="server" DataKeyField="RoutineTimeID" DataSourceID="RoutineTimeSQL" RepeatDirection="Horizontal" Width="100%">
                                    <ItemTemplate>
                                        <div class="Timing">
                                            <asp:Label ID="RoutinePeriodLabel" runat="server" Text='<%# Eval("RoutinePeriod") %>' CssClass="Period" /><br />
                                            <asp:Label ID="DurationLabel" runat="server" Text='<%# Eval("Duration") %>' CssClass="duration" /><br />
                                            <asp:Label ID="TimeLabel" runat="server" Text='<%# Eval("Time") %>' CssClass="R_Time" /><br />
                                        </div>
                                    </ItemTemplate>
                                </asp:DataList>
                                <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutinePeriod, (SELECT CONVERT (varchar(15), CAST(RoutineTime.StartTime AS TIME), 100) AS Expr1) + ' - ' + (SELECT CONVERT (varchar(15), CAST(RoutineTime.EndTime AS TIME), 100) AS Expr1) AS Time, Duration, RoutineTimeID FROM RoutineTime WHERE (RoutineInfoID = @RoutineInfoID) AND (SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="RoutineInfoID_HF" Name="RoutineInfoID" PropertyName="Value" Type="Int32" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:DataList ID="SubteacherDataList" runat="server" DataSourceID="RoutineTimeSQL" RepeatDirection="Horizontal" Width="100%">
                                    <ItemTemplate>
                                        <div class="Teacher_Sub">
                                            <asp:Label ID="SubjectNameLabel" runat="server" Text='<%# Eval("SubjectName") %>' CssClass="SubName" /><br />
                                            <asp:Label ID="Teacher_NameLabel" runat="server" Text='<%# Eval("Teacher_Name") %>' />
                                        </div>
                                    </ItemTemplate>
                                </asp:DataList>
                                <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT R_Class_T.SchoolID, R_Class_T.RoutineInfoID, R_Class_T.RoutineTimeID, R_Class_T.RoutineForClassID, R_Class_T.TeacherID, R_Class_T.SubjectID, R_Class_T.Day, ISNULL(R_Class_T.Teacher_Name, 'Off Time') AS Teacher_Name, ISNULL(R_Class_T.SubjectName, RoutineTime.RoutinePeriod) AS SubjectName, RoutineTime.RoutinePeriod, RoutineTime.StartTime, RoutineTime.EndTime, RoutineTime.Duration FROM (SELECT RoutineForClass.SchoolID, RoutineForClass.RoutineTimeID, RoutineForClass.RoutineForClassID, RoutineForClass.Day, RoutineForClass.RoutineInfoID, RoutineForClass.TeacherID, RoutineForClass.SubjectID, Teacher.FirstName + ' ' + Teacher.LastName AS Teacher_Name, Subject.SubjectName FROM Teacher INNER JOIN RoutineForClass ON Teacher.TeacherID = RoutineForClass.TeacherID INNER JOIN Subject ON RoutineForClass.SubjectID = Subject.SubjectID WHERE (RoutineForClass.Day = @Day) AND (RoutineForClass.ShiftID LIKE @ShiftID) AND (RoutineForClass.SubjectGroupID LIKE @SubjectGroupID) AND (RoutineForClass.SectionID LIKE @SectionID) AND (RoutineForClass.SchoolID LIKE @SchoolID) AND (RoutineForClass.ClassID = @ClassID) AND (RoutineForClass.RoutineInfoID = @RoutineInfoID) AND (RoutineForClass.EducationYearID = @EducationYearID)) AS R_Class_T RIGHT OUTER JOIN RoutineTime ON R_Class_T.RoutineTimeID = RoutineTime.RoutineTimeID AND R_Class_T.SchoolID = RoutineTime.SchoolID AND R_Class_T.RoutineInfoID = RoutineTime.RoutineInfoID WHERE (RoutineTime.RoutineInfoID = @RoutineInfoID) AND (RoutineTime.SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="RoutineInfoID_HF" Name="RoutineInfoID" PropertyName="Value" Type="Int32" />
                                        <asp:ControlParameter ControlID="DayHF" Name="Day" PropertyName="Value" />
                                        <asp:ControlParameter ControlID="ShiftID_HF" Name="ShiftID" PropertyName="Value" />
                                        <asp:ControlParameter ControlID="SubjectGroupID_HF" Name="SubjectGroupID" PropertyName="Value" />
                                        <asp:ControlParameter ControlID="SectionID_HF" Name="SectionID" PropertyName="Value" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:ControlParameter ControlID="ClassID_HF" Name="ClassID" PropertyName="Value" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <asp:HiddenField ID="DayHF" runat="server" Value='<%# Eval("Day") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="RoutineDaySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutineDayID, RoutineInfoID, SchoolID, RegistrationID, Day FROM RoutineDay
WHERE (RoutineInfoID = @RoutineInfoID) AND Day IN(SELECT Day FROM RoutineForClass
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SectionID LIKE @SectionID) AND (ShiftID LIKE @ShiftID) AND (SubjectGroupID LIKE @SubjectGroupID) AND  (RoutineInfoID LIKE @RoutineInfoID))">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="RoutineInfoID_HF" Name="RoutineInfoID" PropertyName="Value" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassID_HF" Name="ClassID" PropertyName="Value" />
                        <asp:ControlParameter ControlID="SectionID_HF" Name="SectionID" PropertyName="Value" />
                        <asp:ControlParameter ControlID="ShiftID_HF" Name="ShiftID" PropertyName="Value" />
                        <asp:ControlParameter ControlID="SubjectGroupID_HF" Name="SubjectGroupID" PropertyName="Value" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </ItemTemplate>
        </asp:Repeater>
        <asp:SqlDataSource ID="MultiRoutineSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT RoutineForClass.RoutineInfoID, RoutineInfo.RoutineSpecification, CreateClass.Class, RoutineForClass.SectionID, RoutineForClass.ShiftID, RoutineForClass.SubjectGroupID, RoutineForClass.ClassID FROM RoutineForClass INNER JOIN
                         StudentsClass ON RoutineForClass.ClassID = StudentsClass.ClassID AND RoutineForClass.SectionID = StudentsClass.SectionID AND RoutineForClass.ShiftID = StudentsClass.ShiftID AND 
                         RoutineForClass.SubjectGroupID = StudentsClass.SubjectGroupID INNER JOIN
                         RoutineInfo ON RoutineForClass.RoutineInfoID = RoutineInfo.RoutineInfoID INNER JOIN
                         CreateClass ON RoutineForClass.ClassID = CreateClass.ClassID WHERE (RoutineForClass.SchoolID = @SchoolID) AND (RoutineForClass.EducationYearID = @EducationYearID) AND (StudentsClass.StudentClassID = @StudentClassID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <script>
        $(function () {
            $("#_6").addClass("active");
        });
    </script>
</asp:Content>
