<%@ Page Title="Multiple Re-Admission" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Multiple_Re_Admission.aspx.cs" Inherits="EDUCATION.COM.Admission.Re_Admission.Multiple_Re_Admission" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../CSS/Student_List.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3>Old Session & Class</h3>
            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:DropDownList ID="SessionYearDropDownList" runat="server" CssClass="form-control" DataSourceID="Edu_YearSQL" DataTextField="EducationYear" DataValueField="EducationYearID" AppendDataBoundItems="True" AutoPostBack="True">
                        <asp:ListItem Value="0">[ OLD SESSION ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Edu_YearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYearID, SchoolID, RegistrationID, EducationYear, Status FROM Education_Year WHERE (SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group mx-2">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup"
                        DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group mx-2">
                    <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
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
                    <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:RadioButtonList ID="CuOrExamRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" CssClass="form-control">
                        <asp:ListItem Selected="True">Position By Cumulative Exam</asp:ListItem>
                        <asp:ListItem>Position By Individual Exam</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <div class="form-group">
                    <asp:RadioButtonList ID="Class_Sec_RadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                        <asp:ListItem Selected="True">Position Class Wise</asp:ListItem>
                        <asp:ListItem>Position Section Wise</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
            </div>

            <div class="form-inline">
                <asp:MultiView ID="ExamMultiView" runat="server">
                    <asp:View ID="Cu_ExamView" runat="server">
                        <div class="form-group">
                            <asp:DropDownList ID="Cu_ExamDropDownList" runat="server" DataSourceID="Cu_ExamNameSQL" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID" OnDataBound="Cu_ExamDropDownList_DataBound" CssClass="form-control">
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="Cu_ExamDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="SH"></asp:RequiredFieldValidator>
                            <asp:SqlDataSource ID="Cu_ExamNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Cumulative_Name.CumulativeResultName, Exam_Cumulative_Name.CumulativeNameID
FROM            Exam_Cumulative_Setting INNER JOIN
                         Exam_Cumulative_Name ON Exam_Cumulative_Setting.CumulativeNameID = Exam_Cumulative_Name.CumulativeNameID
WHERE        (Exam_Cumulative_Setting.SchoolID = @SchoolID) AND (Exam_Cumulative_Setting.EducationYearID = @EducationYearID) AND (Exam_Cumulative_Setting.ClassID = @ClassID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:ControlParameter ControlID="SessionYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </asp:View>
                    <asp:View ID="ExamView" runat="server">
                        <div class="form-group">
                            <asp:DropDownList ID="ExamDropDownList" runat="server" DataSourceID="ExamNameSQL" DataTextField="ExamName" DataValueField="ExamID" OnDataBound="ExamDropDownList_DataBound" CssClass="form-control">
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="SH"></asp:RequiredFieldValidator>
                            <asp:SqlDataSource ID="ExamNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName
FROM            Exam_Publish_Setting INNER JOIN
                         Exam_Name ON Exam_Publish_Setting.ExamID = Exam_Name.ExamID
WHERE        (Exam_Publish_Setting.SchoolID = @SchoolID) AND (Exam_Publish_Setting.ClassID = @ClassID) AND (Exam_Publish_Setting.EducationYearID = @EducationYearID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="SessionYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </asp:View>
                </asp:MultiView>
                <div class="form-group">
                    <asp:Button ID="ShowPositionButton" runat="server" CssClass="btn btn-primary" OnClick="ShowPositionButton_Click" Text="Set New Roll No." ValidationGroup="SH" />
                    <button id="SetRollasItIs" class="btn btn-brown">Set Roll As It Is</button>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="StudentsGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" DataKeyNames="StudentID,StudentClassID,RollNo" PagerStyle-CssClass="pgr" CssClass="mGrid" DataSourceID="ShowStudentClassSQL">
                    <AlternatingRowStyle CssClass="alt" />
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text="All" />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <ItemStyle Width="50px" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                        <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                        <asp:TemplateField HeaderText="Merit Status / New Roll">
                            <ItemTemplate>
                                <asp:TextBox ID="Merit_StatusTextBox" runat="server" CssClass="form-control newroll"></asp:TextBox>
                            </ItemTemplate>
                            <ItemStyle Width="200px" />
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                    <SelectedRowStyle CssClass="Selected" />
                </asp:GridView>

                <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT        Student.StudentID, Student.SMSPhoneNo, Student.StudentsName, Student.Gender, Student.StudentsLocalAddress, Student.MothersName, Student.FathersName, Student.FatherPhoneNumber, 
                         Student.GuardianName, StudentsClass.RollNo, Student.ID, Student.MotherPhoneNumber, Student.FatherOccupation, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, 
                         StudentsClass.EducationYearID
FROM            StudentsClass INNER JOIN
                         Student ON StudentsClass.StudentID = Student.StudentID
WHERE        (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND 
                         (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.Class_Status IS NULL)
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:Parameter DefaultValue="Active" Name="Status" />
                        <asp:ControlParameter ControlID="SessionYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <br />
            </div>

            <%if (StudentsGridView.Rows.Count > 0)
            {%>
            <h3>New Session & Class</h3>

            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:DropDownList ID="New_Session_DropDownList" runat="server" CssClass="form-control" DataSourceID="New_SessionSQL" DataTextField="EducationYear" DataValueField="EducationYearID" OnDataBound="New_Session_DropDownList_DataBound">
                        <asp:ListItem Value="0">[ NEW SESSION ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="New_Session_DropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="SB"></asp:RequiredFieldValidator>
                    <asp:SqlDataSource ID="New_SessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYearID, SchoolID, RegistrationID, EducationYear, Status, SN FROM Education_Year WHERE (SchoolID = @SchoolID) AND (SN &gt; (SELECT SN FROM Education_Year AS Education_Year_1 WHERE (SchoolID = @SchoolID) AND (EducationYearID = @OldEducationYearID)))">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="SessionYearDropDownList" Name="OldEducationYearID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="Re_ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="Re_ClassSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="Re_ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Re_ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="Re_GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Re_GroupSQL" DataTextField="SubjectGroup"
                        DataValueField="SubjectGroupID" OnDataBound="Re_GroupDropDownList_DataBound" OnSelectedIndexChanged="Re_GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Re_GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="Re_SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Re_SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="Re_SectionDropDownList_DataBound" OnSelectedIndexChanged="Re_SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Re_SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="Re_ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Re_ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="Re_ShiftDropDownList_DataBound" OnSelectedIndexChanged="Re_ShiftDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Re_ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Re_SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <%if (Re_ClassDropDownList.SelectedIndex > 0)
            {%>
            <div class="table-responsive">
                <br />
                <asp:GridView ID="GroupGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SubjectID,SubjectType" DataSourceID="SubjectGroupSQL" PagerStyle-CssClass="pgr" PageSize="20">
                    <Columns>
                        <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName">
                            <HeaderStyle HorizontalAlign="Left" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="Select" ShowHeader="False">
                            <ItemTemplate>
                                <asp:CheckBox ID="SubjectCheckBox" runat="server" Text=" " Checked='<%# Bind("Check") %>' />
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Center" Width="30px" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="This Subject Is">
                            <ItemTemplate>
                                <asp:RadioButtonList ID="SubjectTypeRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("SubjectType") %>'>
                                    <asp:ListItem>Compulsory</asp:ListItem>
                                    <asp:ListItem>Optional</asp:ListItem>
                                </asp:RadioButtonList>
                            </ItemTemplate>
                            <ItemStyle Width="175px" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="SubjectGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Subject.SubjectID, Subject.SubjectName, SubjectForGroup.SubjectType, CAST((CASE WHEN SubjectForGroup.SubjectType = 'Compulsory' THEN 1 ELSE 0 END)AS BIT) AS [Check] FROM Subject INNER JOIN SubjectForGroup ON Subject.SubjectID = SubjectForGroup.SubjectID WHERE (Subject.SchoolID = @SchoolID) AND (SubjectForGroup.ClassID = @ClassID) AND (SubjectForGroup.SubjectGroupID = (CASE WHEN @SubjectGroupID = '%' THEN 0 ELSE @SubjectGroupID END)) ORDER BY SubjectForGroup.SubjectType, Subject.SubjectName">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="Re_GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="StudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS (SELECT * FROM  StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID))
BEGIN
INSERT INTO [StudentsClass] ([SchoolID], [RegistrationID], [StudentID], [ClassID], [SectionID], [ShiftID], [SubjectGroupID], [RollNo], [EducationYearID], [Date]) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, Getdate())
END"
                    SelectCommand="SELECT * FROM StudentsClass " UpdateCommand="UPDATE StudentsClass SET Class_Status = @Class_Status WHERE (StudentClassID = @StudentClassID)">
                    <InsertParameters>
                        <asp:Parameter Name="StudentID" Type="Int32" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                        <asp:ControlParameter ControlID="New_Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" Type="String" />
                        <asp:ControlParameter ControlID="Re_ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:Parameter Name="SectionID" Type="String" />
                        <asp:Parameter Name="ShiftID" Type="String" />
                        <asp:Parameter Name="SubjectGroupID" Type="String" />
                        <asp:Parameter Name="RollNo" Type="String" />
                    </InsertParameters>
                    <UpdateParameters>
                        <asp:Parameter DefaultValue="Re-Admitted" Name="Class_Status" />
                        <asp:Parameter Name="StudentClassID" Type="Int32" />
                    </UpdateParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="StudentRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    InsertCommand="IF NOT EXISTS (SELECT  * FROM  StudentRecord WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID) AND (SubjectID = @SubjectID))
BEGIN
INSERT INTO StudentRecord(SchoolID, RegistrationID, StudentID, StudentClassID, SubjectID, EducationYearID, Date, SubjectType) VALUES (@SchoolID, @RegistrationID, @StudentID, @StudentClassID, @SubjectID, @EducationYearID, GETDATE(), @SubjectType)
END"
                    SelectCommand="SELECT * FROM [StudentRecord]">
                    <InsertParameters>
                        <asp:Parameter Name="StudentID" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="New_Session_DropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                        <asp:Parameter Name="StudentClassID" Type="Int32" />
                        <asp:Parameter Name="SubjectID" Type="Int32" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                        <asp:Parameter Name="SubjectType" />
                    </InsertParameters>
                </asp:SqlDataSource>
                <br />
                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="SubmitButton_Click" ValidationGroup="SB" />
                <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="SB"> </asp:CustomValidator><br />
                <asp:Label ID="ErrorLabel" runat="server" ForeColor="Red"></asp:Label>
            </div>
            <%}%>
            <%}%>
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

    <script>
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            //Set roll as it is
            $("#SetRollasItIs").on("click", function () {
                $("[id*=StudentsGridView] tr").each(function () {
                    if (!this.rowIndex) return;
                    var oldRoll = $(this).find("td:eq(5)").html();
                    $(this).find("td:eq(6)").find('.newroll').val(oldRoll);
                });
            });

            //Select All Checkbox
            $("[id*=AllIteamCheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SingleCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllIteamCheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SingleCheckBox]", a).length == $("[id*=SingleCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        });

        //Select at least one Checkbox Students GridView
        function Validate(d, c) {
            if ($('[id*=StudentsGridView] tr').length) {
                for (var b = document.getElementById("<%=StudentsGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                    if ("checkbox" == b[a].type && b[a].checked) {
                        c.IsValid = !0;
                        return;
                    }
                }
                c.IsValid = !1;
            }
        }
    </script>
</asp:Content>
