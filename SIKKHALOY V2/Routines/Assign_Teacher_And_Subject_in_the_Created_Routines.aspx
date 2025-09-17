<%@ Page Title="Assign Routine" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Assign_Teacher_And_Subject_in_the_Created_Routines.aspx.cs" Inherits="EDUCATION.COM.ROUTINES.Assign_Teacher_And_Subject_in_the_Created_Routines__" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Routine.css" rel="stylesheet" />
    <style>
        .SuccessMsg { color: #00ff21; }
        .Assign_Grid { width: 100%; border: none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Assign Teacher & Subject in Routine</h3>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE  @ShiftID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE  @SubjectGroupID) AND ([Join].SectionID like @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="SpecificationDropDownList" runat="server" AutoPostBack="True" DataSourceID="RoutineSpecificationSQL" DataTextField="RoutineSpecification" DataValueField="RoutineInfoID" CssClass="form-control" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ ROUTINE NAME ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="RoutineSpecificationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [RoutineInfo] WHERE ([SchoolID] = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ControlToValidate="SpecificationDropDownList" CssClass="EroorStar" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
            </div>

            <a href="/Employee/Teacher_Allocated_Subjects.aspx">Click Here To Assign Subject For Teacher</a>

            <div class="table-responsive">
                <asp:GridView ID="DayGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RoutineDayID" DataSourceID="RoutineDaySQL" CssClass="Assign_Grid">
                    <Columns>
                        <asp:BoundField DataField="Day" HeaderText="Day" />
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <div class="row no-gutters">
                                    <asp:DataList ID="PeriodDataList" runat="server" DataKeyField="RoutineTimeID" DataSourceID="RoutineTimeSQL" RepeatDirection="Horizontal" Width="100%">
                                        <ItemTemplate>
                                            <div class="col">
                                                <span class="d-block"><%# Eval("RoutinePeriod") %></span>
                                                <span class="d-block"><%# Eval("Time") %></span>
                                                <span class="d-block"><%# Eval("Duration") %></span>
                                            </div>
                                        </ItemTemplate>
                                    </asp:DataList>
                                </div>
                                <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutinePeriod, (SELECT CONVERT (varchar(15), CAST(RoutineTime.StartTime AS TIME), 100) AS Expr1) + ' To ' + (SELECT CONVERT (varchar(15), CAST(RoutineTime.EndTime AS TIME), 100) AS Expr1) AS Time, Duration, RoutineTimeID FROM RoutineTime WHERE (RoutineInfoID = @RoutineInfoID) AND (SchoolID = @SchoolID) AND (Is_OffTime = 0)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div class="row no-gutters">
                                    <asp:DataList ID="SubteacherDataList" runat="server" DataKeyField="RoutineTimeID" DataSourceID="RoutineTimeSQL" RepeatDirection="Horizontal" Width="100%">
                                        <ItemTemplate>
                                            <div class="col my-2">
                                                <asp:DropDownList ID="SubjectDropDownList" runat="server" AutoPostBack="True" CssClass="form-control mb-2" DataSourceID="SubjectSQL" DataTextField="SubjectName" DataValueField="SubjectID" AppendDataBoundItems="True">
                                                    <asp:ListItem Value="0">[ Select Subject ]</asp:ListItem>
                                                </asp:DropDownList>

                                                <asp:DropDownList ID="TeacherDropDownList" OnDataBound="TeacherDropDownList_DataBound" runat="server" CssClass="form-control" DataSourceID="TeacherSQL" DataTextField="Name" DataValueField="TeacherID">
                                                </asp:DropDownList>
                                            </div>

                                            <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Subject.SubjectName, SubjectForGroup.SubjectID FROM SubjectForGroup INNER JOIN Subject ON SubjectForGroup.SubjectID = Subject.SubjectID WHERE (SubjectForGroup.ClassID = @ClassID) AND (SubjectForGroup.SubjectGroupID LIKE  @SubjectGroupID)">
                                                <SelectParameters>
                                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                                </SelectParameters>
                                            </asp:SqlDataSource>
                                            <asp:SqlDataSource ID="TeacherSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Teacher.FirstName + ' ' + Teacher.LastName AS Name, TecherSubject.TeacherID FROM TecherSubject INNER JOIN Teacher ON TecherSubject.TeacherID = Teacher.TeacherID WHERE (TecherSubject.SubjectID = @SubjectID)  AND (TecherSubject.ClassID = @ClassID) and
TecherSubject.TeacherID not in (SELECT RoutineForClass.TeacherID FROM RoutineForClass INNER JOIN RoutineTime ON RoutineForClass.RoutineTimeID = RoutineTime.RoutineTimeID 
WHERE (RoutineForClass.SchoolID = @SchoolID) AND (RoutineForClass.EducationYearID = @EducationYearID) AND (RoutineForClass.Day = @Day)  AND 
(((RoutineTime.StartTime &lt;= @S_time) AND (RoutineTime.EndTime &gt; @S_time)) or ((RoutineTime.StartTime &lt; @E_time) AND (RoutineTime.EndTime &gt;= @E_time)) or ((RoutineTime.StartTime &gt;= @S_time) AND (RoutineTime.EndTime &lt;= @E_time))))">
                                                <SelectParameters>
                                                    <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                                    <asp:ControlParameter ControlID="DayHF" Name="Day" PropertyName="Value" />
                                                    <asp:ControlParameter ControlID="S_time_HiddenField" Name="S_time" PropertyName="Value" />
                                                    <asp:ControlParameter ControlID="E_time_HiddenField" Name="E_time" PropertyName="Value" />
                                                </SelectParameters>
                                            </asp:SqlDataSource>
                                            <asp:HiddenField ID="S_time_HiddenField" runat="server" Value='<%# Eval("StartTime") %>' />
                                            <asp:HiddenField ID="E_time_HiddenField" runat="server" Value='<%# Eval("EndTime") %>' />
                                        </ItemTemplate>
                                    </asp:DataList>
                                </div>
                                <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutineTimeID, RoutineInfoID, SchoolID, RegistrationID, RoutinePeriod, StartTime, EndTime, Duration FROM RoutineTime WHERE(RoutineInfoID = @RoutineInfoID) AND (Is_OffTime = 0)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <asp:HiddenField ID="DayHF" runat="server" Value='<%# Eval("Day") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <asp:SqlDataSource ID="RoutineDaySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        RoutineDayID, RoutineInfoID, SchoolID, RegistrationID, Day
FROM            RoutineDay
WHERE        (RoutineInfoID = @RoutineInfoID) AND Day NOT IN(SELECT Day
FROM            RoutineForClass
WHERE        (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SectionID LIKE @SectionID) AND (ShiftID LIKE @ShiftID) AND (SubjectGroupID LIKE @SubjectGroupID) AND  (RoutineInfoID LIKE @RoutineInfoID))">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="ClassRoutineSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO RoutineForClass(RoutineInfoID, RoutineTimeID, SubjectID, Day, SchoolID, RegistrationID, TeacherID, ClassID, SectionID, SubjectGroupID, EducationYearID, Date, ShiftID) VALUES (@RoutineInfoID, @RoutineTimeID, @SubjectID, @Day, @SchoolID, @RegistrationID, @TeacherID, @ClassID, @SectionID, @SubjectGroupID, @EducationYearID, GETDATE(), @ShiftID)" SelectCommand="SELECT * FROM [RoutineForClass]">
                    <InsertParameters>
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:Parameter Name="RoutineTimeID" Type="Int32" />
                        <asp:Parameter Name="SubjectID" Type="Int32" />
                        <asp:Parameter Name="Day" Type="String" />
                        <asp:Parameter Name="TeacherID" />
                        <asp:Parameter Name="SectionID" />
                        <asp:Parameter Name="SubjectGroupID" />
                        <asp:Parameter Name="ShiftID" />
                    </InsertParameters>
                </asp:SqlDataSource>

                <%if (DayGridView.Rows.Count > 0)
                    {%>
                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Create Routine" ValidationGroup="1" /><br />
                <%} %>

                <asp:Label ID="MassageLabel" runat="server" Font-Size="15pt"></asp:Label>
                <asp:GridView ID="RoutineGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RoutineDayID" DataSourceID="RoutineSQL" CssClass="DRoGrid">
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
                                            <asp:Label ID="TimeLabel0" runat="server" Text='<%# Eval("Time") %>' CssClass="R_Time" /><br />
                                        </div>
                                    </ItemTemplate>
                                </asp:DataList>
                                <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutinePeriod, (SELECT CONVERT (varchar(15), CAST(RoutineTime.StartTime AS TIME), 100) AS Expr1) + ' - ' + (SELECT CONVERT (varchar(15), CAST(RoutineTime.EndTime AS TIME), 100) AS Expr1) AS Time, Duration, RoutineTimeID FROM RoutineTime WHERE (RoutineInfoID = @RoutineInfoID) AND (SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:DataList ID="SubteacherDataList" runat="server" DataSourceID="RoutineTimeSQL" RepeatDirection="Horizontal" Width="100%">
                                    <ItemTemplate>
                                        <div class="Teacher_Sub">
                                            <asp:Label ID="SubjectNameLabel" runat="server" Text='<%# Eval("SubjectName")==null?"No Subject":Eval("SubjectName") %>' CssClass="SubName" /><br />
                                            <asp:Label ID="Teacher_NameLabel" runat="server" Text='<%# Eval("Teacher_Name") %>' />
                                        </div>
                                    </ItemTemplate>
                                </asp:DataList>
                                <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT R_Class_T.SchoolID, R_Class_T.RoutineInfoID, R_Class_T.RoutineTimeID, R_Class_T.RoutineForClassID, R_Class_T.TeacherID, R_Class_T.SubjectID, R_Class_T.Day, ISNULL(R_Class_T.Teacher_Name, 'Off Time') AS Teacher_Name, ISNULL(R_Class_T.SubjectName, RoutineTime.RoutinePeriod) AS SubjectName, RoutineTime.RoutinePeriod, RoutineTime.StartTime, RoutineTime.EndTime, RoutineTime.Duration FROM (SELECT RoutineForClass.SchoolID, RoutineForClass.RoutineTimeID, RoutineForClass.RoutineForClassID, RoutineForClass.Day, RoutineForClass.RoutineInfoID, RoutineForClass.TeacherID, RoutineForClass.SubjectID, ISNULL(Teacher.FirstName + ' ' + Teacher.LastName, '') AS Teacher_Name, ISNULL(Subject.SubjectName, '') AS SubjectName FROM Teacher RIGHT OUTER JOIN RoutineForClass ON Teacher.TeacherID = RoutineForClass.TeacherID LEFT OUTER JOIN Subject ON RoutineForClass.SubjectID = Subject.SubjectID WHERE (RoutineForClass.Day = @Day) AND (RoutineForClass.ShiftID LIKE @ShiftID) AND (RoutineForClass.SubjectGroupID LIKE @SubjectGroupID) AND (RoutineForClass.SectionID LIKE @SectionID) AND (RoutineForClass.SchoolID LIKE @SchoolID) AND (RoutineForClass.ClassID = @ClassID) AND (RoutineForClass.RoutineInfoID = @RoutineInfoID) AND (RoutineForClass.EducationYearID = @EducationYearID)) AS R_Class_T RIGHT OUTER JOIN RoutineTime ON R_Class_T.RoutineTimeID = RoutineTime.RoutineTimeID AND R_Class_T.SchoolID = RoutineTime.SchoolID AND R_Class_T.RoutineInfoID = RoutineTime.RoutineInfoID WHERE (RoutineTime.RoutineInfoID = @RoutineInfoID) AND (RoutineTime.SchoolID = @SchoolID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
                                        <asp:ControlParameter ControlID="DayHF" Name="Day" PropertyName="Value" />
                                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <asp:HiddenField ID="DayHF" runat="server" Value='<%# Eval("Day") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="RoutineSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutineDayID, RoutineInfoID, SchoolID, RegistrationID, Day FROM RoutineDay
WHERE (RoutineInfoID = @RoutineInfoID) AND Day IN(SELECT Day FROM RoutineForClass
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SectionID LIKE @SectionID) AND (ShiftID LIKE @ShiftID) AND (SubjectGroupID LIKE @SubjectGroupID) AND  (RoutineInfoID LIKE @RoutineInfoID))"
                    DeleteCommand="DELETE FROM RoutineForClass
WHERE        (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SectionID LIKE @SectionID) AND (ShiftID LIKE @ShiftID) AND (SubjectGroupID LIKE @SubjectGroupID) AND 
                         (RoutineInfoID = @RoutineInfoID)">
                    <DeleteParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>

                <%if (RoutineGridView.Rows.Count > 0)
                    {%>
                <asp:Button ID="DeleteRoutine_Button" OnClick="DeleteRoutine_Button_Click" runat="server" CssClass="btn btn-danger" Text="Delete Routine" ValidationGroup="1" /><br />
                <%} %>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
</asp:Content>
