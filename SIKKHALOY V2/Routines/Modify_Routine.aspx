<%@ Page Title="Modify Routine" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Modify_Routine.aspx.cs" Inherits="EDUCATION.COM.Routines.Modify_Routine" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .SuccessMsg { color: #00ff21; }
        .Assign_Grid { width: 100%; border: none; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Modify Created Routiens</h3>

            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.ClassID, CreateClass.Class, CreateClass.SN FROM CreateClass INNER JOIN RoutineForClass ON CreateClass.ClassID = RoutineForClass.ClassID WHERE (CreateClass.SchoolID = @SchoolID) ORDER BY CreateClass.SN">
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
                    <asp:SqlDataSource ID="RoutineSpecificationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT RoutineInfo.RoutineInfoID, RoutineInfo.RoutineSpecification FROM RoutineInfo INNER JOIN RoutineForClass ON RoutineInfo.RoutineInfoID = RoutineForClass.RoutineInfoID WHERE (RoutineInfo.SchoolID = @SchoolID) AND (RoutineForClass.ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ControlToValidate="SpecificationDropDownList" CssClass="EroorStar" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
            </div>

            <div class="table-responsive mb-2">
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
                                    <asp:DataList ID="SubteacherDataList" runat="server" DataKeyField="RoutineTimeID" DataSourceID="RoutineTimeSQL" RepeatDirection="Horizontal" OnItemDataBound="SubteacherDataList_ItemDataBound" Width="100%">
                                        <ItemTemplate>
                                            <asp:HiddenField ID="S_time_HiddenField" runat="server" Value='<%# Eval("StartTime") %>' />
                                            <asp:HiddenField ID="RoutineForClassID_HiddenField" runat="server" Value='<%# Eval("RoutineForClassID") %>' />
                                            <asp:HiddenField ID="E_time_HiddenField" runat="server" Value='<%# Eval("EndTime") %>' />
                                            <asp:HiddenField ID="TeacherID_HiddenField" runat="server" Value='<%# Eval("TeacherID") %>' />
                                            <div class="col my-2">
                                                <asp:DropDownList ID="SubjectDropDownList" runat="server" AutoPostBack="True" CssClass="form-control mb-2" DataSourceID="SubjectSQL" DataTextField="SubjectName" DataValueField="SubjectID" AppendDataBoundItems="True" SelectedValue='<%# Eval("SubjectID") %>'>
                                                    <asp:ListItem Value="0">[ No Class ]</asp:ListItem>
                                                </asp:DropDownList>
                                                <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT       SubjectForGroup.SubjectID , Subject.SubjectName
FROM            SubjectForGroup INNER JOIN
                         Subject ON SubjectForGroup.SubjectID = Subject.SubjectID
WHERE        (SubjectForGroup.ClassID = @ClassID) AND (SubjectForGroup.SubjectGroupID = @SubjectGroupID) 
Union
SELECT        RoutineForClass.SubjectID, Subject.SubjectName
FROM            RoutineForClass INNER JOIN
                         Subject ON RoutineForClass.SubjectID = Subject.SubjectID
WHERE        (RoutineForClass.RoutineForClassID = @RoutineForClassID)">
                                                    <SelectParameters>
                                                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                                        <asp:ControlParameter ControlID="RoutineForClassID_HiddenField" Name="RoutineForClassID" PropertyName="Value" />
                                                    </SelectParameters>
                                                </asp:SqlDataSource>

                                                <asp:DropDownList ID="TeacherDropDownList" runat="server" CssClass="form-control" DataSourceID="TeacherSQL" DataTextField="Name" DataValueField="TeacherID" OnDataBound="TeacherDropDownList_DataBound">
                                                </asp:DropDownList>
                                                <asp:SqlDataSource ID="TeacherSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  Teacher.FirstName + ' ' + Teacher.LastName AS Name, 
Teacher.TeacherID FROM Teacher INNER JOIN RoutineForClass ON Teacher.TeacherID = RoutineForClass.TeacherID
WHERE (RoutineForClass.RoutineForClassID = @RoutineForClassID) AND (RoutineForClass.SubjectID = @SubjectID)
union SELECT Teacher.FirstName + ' ' + Teacher.LastName AS Name, TecherSubject.TeacherID FROM TecherSubject INNER JOIN Teacher ON TecherSubject.TeacherID = Teacher.TeacherID 
WHERE (TecherSubject.SubjectID = @SubjectID) AND (TecherSubject.ClassID = @ClassID) and TecherSubject.TeacherID not in (SELECT RoutineForClass.TeacherID FROM RoutineForClass INNER JOIN RoutineTime ON RoutineForClass.RoutineTimeID = RoutineTime.RoutineTimeID WHERE (RoutineForClass.SchoolID = @SchoolID) AND 
(RoutineForClass.EducationYearID = @EducationYearID) AND (RoutineForClass.Day = @Day)  AND (((RoutineTime.StartTime &lt;= @S_time) AND (RoutineTime.EndTime &gt; @S_time)) or ((RoutineTime.StartTime &lt; @E_time) AND (RoutineTime.EndTime &gt;= @E_time)) or ((RoutineTime.StartTime &gt;= @S_time) AND (RoutineTime.EndTime &lt;= @E_time))))">
                                                    <SelectParameters>
                                                        <asp:ControlParameter ControlID="RoutineForClassID_HiddenField" Name="RoutineForClassID" PropertyName="Value" />
                                                        <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                                                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                                        <asp:ControlParameter ControlID="DayHF" Name="Day" PropertyName="Value" />
                                                        <asp:ControlParameter ControlID="S_time_HiddenField" Name="S_time" PropertyName="Value" />
                                                        <asp:ControlParameter ControlID="E_time_HiddenField" Name="E_time" PropertyName="Value" />
                                                    </SelectParameters>
                                                </asp:SqlDataSource>
                                            </div>
                                        </ItemTemplate>
                                    </asp:DataList>
                                    <asp:SqlDataSource ID="RoutineTimeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutineForClass.RoutineForClassID,
       RoutineTime.RoutineTimeID, 
       RoutineTime.RegistrationID, 
	   RoutineTime.RoutinePeriod, 
	   RoutineTime.StartTime, 
	   RoutineTime.EndTime, 
	   RoutineTime.Duration, 
	   RoutineForClass.Day, 
	   RoutineForClass.RoutineInfoID, 
       RoutineForClass.TeacherID, 
       RoutineForClass.SubjectID 
FROM RoutineTime LEFT OUTER JOIN RoutineForClass ON RoutineTime.RoutineTimeID = RoutineForClass.RoutineTimeID AND RoutineTime.SchoolID = RoutineForClass.SchoolID 
WHERE (RoutineTime.RoutineInfoID = @RoutineInfoID) AND 
      (RoutineForClass.Day = @Day) AND 
	  (RoutineForClass.ShiftID LIKE @ShiftID) AND 
	  (RoutineForClass.SubjectGroupID LIKE @SubjectGroupID) AND 
      (RoutineForClass.SectionID LIKE @SectionID) AND 
      (RoutineForClass.SchoolID = @SchoolID) AND 
      (RoutineForClass.ClassID = @ClassID) AND 
      (RoutineForClass.RoutineInfoID = @RoutineInfoID) AND 
      (RoutineForClass.EducationYearID = @EducationYearID) AND (RoutineTime.Is_OffTime = 0)">
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
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="RoutineDaySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT RoutineDayID, RoutineInfoID, SchoolID, RegistrationID, Day
FROM RoutineDay WHERE (RoutineInfoID = @RoutineInfoID) AND Day  IN(SELECT Day
FROM RoutineForClass WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SectionID LIKE @SectionID) AND (ShiftID LIKE @ShiftID) AND (SubjectGroupID LIKE @SubjectGroupID) AND  (RoutineInfoID LIKE @RoutineInfoID))">
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
            <%if (DayGridView.Rows.Count > 0)
                {%>
            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Update" ValidationGroup="1" />

            <%} %>
            <asp:Label ID="MassageLabel" runat="server" ForeColor="#006600"></asp:Label>

            <asp:SqlDataSource ID="ClassRoutineSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [RoutineForClass]" UpdateCommand="UPDATE       RoutineForClass
SET SubjectID = @SubjectID, TeacherID = @TeacherID WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (RoutineInfoID = @RoutineInfoID) AND (RoutineTimeID = @RoutineTimeID) AND (ClassID = @ClassID) AND (SectionID = @SectionID) AND 
(ShiftID = @ShiftID) AND (SubjectGroupID = @SubjectGroupID) AND (Day = @Day)">
                <UpdateParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SpecificationDropDownList" Name="RoutineInfoID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:Parameter Name="Day" />
                    <asp:Parameter Name="SubjectID" />
                    <asp:Parameter Name="TeacherID" />
                    <asp:Parameter Name="RoutineTimeID" />
                </UpdateParameters>
            </asp:SqlDataSource>
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
