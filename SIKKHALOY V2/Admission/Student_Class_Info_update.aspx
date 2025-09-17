<%@ Page Title="Change academic info" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Student_Class_Info_update.aspx.cs" Inherits="EDUCATION.COM.Admission.Student_Class_Info_update" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Change academic info</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:TextBox ID="IDTextBox" placeholder="Separate the ID by comma" runat="server" CssClass="form-control" TextMode="MultiLine" Height="34px"></asp:TextBox>
        </div>
        <div class="form-group">
            <asp:Button ID="Find_Button" runat="server" CssClass="btn btn-primary" ValidationGroup="1" Text="Find Student" OnClick="Find_Button_Click" />
        </div>
        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="IDTextBox" CssClass="EroorStar" ErrorMessage="Enter Id. " ValidationGroup="1"></asp:RequiredFieldValidator>
    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1"></asp:RequiredFieldValidator>
    </div>
    
    <div class="mb-3 Disable_Sub">
        <div class="alert-success">Student list according to ID</div>
        <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="StudentClassID,StudentID" CssClass="mGrid" DataSourceID="ShowIDSQL">
            <Columns>
                <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                <asp:TemplateField HeaderText="Roll" SortExpression="RollNo">
                    <ItemTemplate>
                        <asp:TextBox ID="RollTextBox" CssClass="form-control" runat="server" Text='<%# Bind("RollNo") %>'></asp:TextBox>
                    </ItemTemplate>
                    <ItemStyle Width="80px" />
                </asp:TemplateField>
                <asp:BoundField DataField="SubjectGroup" HeaderText="Group" SortExpression="SubjectGroup" />
                <asp:BoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                <asp:BoundField DataField="Shift" HeaderText="Shift" SortExpression="Shift" />

            </Columns>
        </asp:GridView>
        <asp:SqlDataSource ID="ShowIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        Stu_T.StudentID, Stu_T.StudentClassID, Stu_T.ClassID, Stu_T.ID, Stu_T.StudentsName, Stu_T.RollNo, Stu_T.Section, Stu_T.Shift, Stu_T.SubjectGroup
FROM            (SELECT        Student.ID, Student.StudentsName, StudentsClass.RollNo, StudentsClass.StudentClassID, StudentsClass.StudentID, StudentsClass.ClassID, CreateSection.Section, CreateShift.Shift, 
                                                    CreateSubjectGroup.SubjectGroup
                          FROM            StudentsClass INNER JOIN
                                                    Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN
                                                    CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN
                                                    CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN
                                                    CreateSection ON StudentsClass.SectionID = CreateSection.SectionID
                          WHERE        (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID)) AS Stu_T INNER JOIN
                             (SELECT        SN, id
                               FROM            dbo.In_Function_Parameter(@IDs) AS In_Function_Parameter_1) AS ID_T ON Stu_T.ID = ID_T.id
ORDER BY ID_T.SN">
            <SelectParameters>
                <asp:CookieParameter DefaultValue="Active" Name="Status" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="IDTextBox" Name="IDs" PropertyName="Text" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <div class="alert alert-success Disable_Sub">Only selected item will be updated</div>
    <div class="form-inline Disable_Sub">
        <div class="form-group">
            <asp:DropDownList ID="GroupDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%')">
                <SelectParameters>
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" AutoPostBack="True" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
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
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ClassID = @ClassID)">
                <SelectParameters>
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <div class="mt-3">
        <div class="alert-info Dis_Group">Subject Of this Class</div>
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
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <div class="form-inline Disable_Sub">
        <div class="form-group">
            <asp:Button ID="ChangeButton" runat="server" CssClass="btn btn-primary" OnClick="ChangeButton_Click" Text="Submit" ValidationGroup="1" />
            <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
            <asp:SqlDataSource ID="StudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM StudentsClass"
                UpdateCommand="UPDATE StudentsClass SET RollNo = @RollNo WHERE (StudentClassID = @StudentClassID)
IF(@SectionID &lt;&gt;'%')
UPDATE StudentsClass SET  SectionID = @SectionID WHERE (StudentClassID = @StudentClassID)
IF(@ShiftID &lt;&gt;'%')
UPDATE StudentsClass SET ShiftID = @ShiftID WHERE (StudentClassID = @StudentClassID)
IF(@SubjectGroupID &lt;&gt;'%')
UPDATE StudentsClass SET SubjectGroupID = @SubjectGroupID WHERE (StudentClassID = @StudentClassID)">
                <UpdateParameters>
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                    <asp:Parameter Name="RollNo" />
                    <asp:Parameter Name="StudentClassID" />
                </UpdateParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="StudentSubjectRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO StudentRecord(StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date) VALUES (@StudentID, @RegistrationID, @SchoolID, @StudentClassID, @SubjectID, @EducationYearID, @SubjectType, GETDATE())" SelectCommand="SELECT * FROM StudentRecord" DeleteCommand="DELETE FROM StudentRecord WHERE (StudentClassID = @StudentClassID) AND (SchoolID = @SchoolID)">
                <DeleteParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:Parameter Name="StudentClassID" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:Parameter Name="StudentClassID" />
                    <asp:Parameter Name="StudentID" />
                    <asp:Parameter Name="SubjectID" Type="Int32" />
                    <asp:Parameter Name="SubjectType" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>
            <asp:SqlDataSource ID="UpdateStudentRecordIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentRecordID, StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date FROM StudentRecord" UpdateCommand="UPDATE
Exam_Obtain_Marks
SET  Exam_Obtain_Marks.StudentRecordID =  StudentRecord.StudentRecordID FROM  Exam_Obtain_Marks
INNER JOIN StudentRecord ON StudentRecord.EducationYearID = Exam_Obtain_Marks.EducationYearID AND StudentRecord.SubjectID = Exam_Obtain_Marks.SubjectID AND 
StudentRecord.StudentID = Exam_Obtain_Marks.StudentID AND 
StudentRecord.SchoolID = Exam_Obtain_Marks.SchoolID

UPDATE  
Exam_Result_of_Subject
SET    
 Exam_Result_of_Subject.StudentRecordID = StudentRecord.StudentRecordID
FROM          
  StudentRecord 
INNER JOIN
 Exam_Result_of_Subject 
ON 
StudentRecord.StudentID = Exam_Result_of_Subject.StudentID AND 
StudentRecord.EducationYearID = Exam_Result_of_Subject.EducationYearID AND 
StudentRecord.SubjectID = Exam_Result_of_Subject.SubjectID AND 
StudentRecord.SchoolID = Exam_Result_of_Subject.SchoolID"></asp:SqlDataSource>
        </div>
    </div>

    <script>
        $(function () {
            //GridView is empty
            if (!$('[id*=StudentsGridView] tr').length) {
                $(".Disable_Sub").hide();
            }

            if (!$('[id*=GroupGridView] tr').length) {
                $(".Dis_Group").hide();
            }
        });
    </script>
</asp:Content>
