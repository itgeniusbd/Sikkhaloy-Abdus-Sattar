<%@ Page Title="Routine" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Class_Routine.aspx.cs" Inherits="EDUCATION.COM.Routines.Class_Routine" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Routine.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="NoPrint">Routine</h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.SN, CreateClass.ClassID, CreateClass.Class FROM CreateClass INNER JOIN RoutineForClass ON CreateClass.ClassID = RoutineForClass.ClassID WHERE (CreateClass.SchoolID = @SchoolID) ORDER BY CreateClass.SN">
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
            <asp:DropDownList ID="SpecificationDropDownList" runat="server" AutoPostBack="True" DataSourceID="RoutineSpecificationSQL" DataTextField="RoutineSpecification" DataValueField="RoutineInfoID" CssClass="form-control" OnDataBound="SpecificationDropDownList_DataBound">
            </asp:DropDownList>
            <asp:SqlDataSource ID="RoutineSpecificationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT RoutineInfo.RoutineInfoID, RoutineInfo.SchoolID, RoutineInfo.RegistrationID, RoutineInfo.RoutineSpecification, RoutineInfo.Date FROM RoutineInfo INNER JOIN RoutineForClass ON RoutineInfo.RoutineInfoID = RoutineForClass.RoutineInfoID WHERE (RoutineInfo.SchoolID = @SchoolID) AND (RoutineForClass.ClassID = @ClassID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ControlToValidate="SpecificationDropDownList" CssClass="EroorStar" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
        </div>
    </div>

    <div class="Title_Head">
        <label id="Class_Section"></label>
    </div>

    <div class="table-responsive mb-2">
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
                                <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
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
        <asp:SqlDataSource ID="RoutineDaySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutineDayID, RoutineInfoID, SchoolID, RegistrationID, Day FROM RoutineDay
WHERE (RoutineInfoID = @RoutineInfoID) AND Day IN(SELECT Day FROM RoutineForClass
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SectionID LIKE @SectionID) AND (ShiftID LIKE @ShiftID) AND (SubjectGroupID LIKE @SubjectGroupID) AND  (RoutineInfoID LIKE @RoutineInfoID))">
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
    </div>

    <input id="print" type="button" style="display: none" value="Print" class="btn btn-primary NoPrint" onclick="window.print();" />

    <script>
        $(function () {
            var Class = "";
            if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                Class = "Routine For Class: " + $('[id*=ClassDropDownList] :selected').text() + ".";
            }

            var Group = "";
            if ($('[id*=GroupDropDownList] :selected').index() > 0) {
                Group = " Group: " + $('[id*=GroupDropDownList] :selected').text() + ".";
            }

            var Section = "";
            if ($('[id*=SectionDropDownList] :selected').index() > 0) {
                Section = " Section: " + $('[id*=SectionDropDownList] :selected').text() + ".";
            }

            var Shift = "";
            if ($('[id*=ShiftDropDownList] :selected').index() > 0) {
                Shift = " Shift: " + $('[id*=ShiftDropDownList] :selected').text() + ".";
            }


            if ($('#body_DayGridView tr').length) {
                $("#Class_Section").text(Class + Group + Section + Shift);
                $("#print").show();
            }
        });
    </script>
</asp:Content>
