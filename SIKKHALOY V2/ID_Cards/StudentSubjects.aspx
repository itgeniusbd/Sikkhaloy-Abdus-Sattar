<%@ Page Title="Manage Subjects" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="StudentSubjects.aspx.cs" Inherits="EDUCATION.COM.ID_Cards.StudentSubjects" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Student's Subjects Change</h3>
    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
        <ContentTemplate>  
            <div class="form-inline NoPrint">
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Class, ClassID FROM CreateClass WHERE (SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
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
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%')">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
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
      </ContentTemplate>
    </asp:UpdatePanel>

    <ul class="nav nav-tabs z-depth-1">
        <li class="nav-item">
            <a class="nav-link active" data-toggle="tab" href="#panel1" role="tab" aria-expanded="true">Subjects In Student</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" data-toggle="tab" href="#panel2" role="tab" aria-expanded="false">Subjects In Class</a>
        </li>
    </ul>

    <div class="tab-content card">
        <div class="tab-pane fade in active show" id="panel1" role="tabpanel" aria-expanded="true">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="form-inline">
                        <div class="form-group">
                            <asp:DropDownList ID="SubjectDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubjectSQL" DataTextField="SubjectName" DataValueField="SubjectID" OnDataBound="SubjectDropDownList_DataBound" OnSelectedIndexChanged="SubjectDropDownList_SelectedIndexChanged">
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Subject.SubjectID,Subject.SubjectName
FROM Subject INNER JOIN
StudentRecord ON Subject.SubjectID = StudentRecord.SubjectID INNER JOIN StudentsClass ON StudentRecord.StudentClassID = StudentsClass.StudentClassID INNER JOIN
Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = N'Active') AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID)
union
SELECT DISTINCT Subject.SubjectID, Subject.SubjectName FROM Subject INNER JOIN SubjectForGroup ON Subject.SubjectID = SubjectForGroup.SubjectID
WHERE (Subject.SchoolID = @SchoolID) AND (SubjectForGroup.ClassID = @ClassID) AND (SubjectForGroup.SubjectGroupID LIKE @SubjectGroupID)"
                                DeleteCommand="DELETE FROM StudentRecord
WHERE (SubjectID = @SubjectID) AND (StudentClassID = @StudentClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID)"
                                InsertCommand=" IF NOT EXISTS (SELECT  SubjectType FROM  StudentRecord WHERE (SubjectID = @SubjectID) AND (StudentClassID = @StudentClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID))
BEGIN		
INSERT INTO StudentRecord(StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date) VALUES (@StudentID, @RegistrationID, @SchoolID, @StudentClassID, @SubjectID, @EducationYearID, @SubjectType, GETDATE())
END
ELSE
BEGIN
UPDATE StudentRecord SET SubjectType = @SubjectType
WHERE (SubjectID = @SubjectID) AND (StudentClassID = @StudentClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) END">
                                <DeleteParameters>
                                    <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:Parameter Name="StudentClassID" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:Parameter Name="StudentClassID" />
                                    <asp:Parameter Name="StudentID" />
                                    <asp:Parameter Name="SubjectType" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="SubjectDropDownList" CssClass="EroorStar" ErrorMessage="Select Subject" InitialValue="0" ValidationGroup="2">*</asp:RequiredFieldValidator>
                        </div>
                    </div>

                    <div class="table-responsive">
                        <asp:GridView ID="StudentsGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" PagerStyle-CssClass="pgr"
                            DataKeyNames="StudentID,StudentClassID" CssClass="mGrid" AllowSorting="True" DataSourceID="ShowStudentClassSQL">
                            <Columns>
                                <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                                <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                                <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                                <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                                <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                                <asp:TemplateField HeaderText="Select" ShowHeader="False">
                                    <HeaderTemplate>
                                        <asp:CheckBox ID="AllICheckBox" Text="All" runat="server" />
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <asp:CheckBox ID="SubjectCheckBox" runat="server" Text=" " />
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" Width="30px" />
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Subject Type">
                                    <ItemTemplate>
                                        <asp:RadioButtonList ID="SubjectTypeRadioButtonList" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Selected="True">Compulsory</asp:ListItem>
                                            <asp:ListItem>Optional</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </ItemTemplate>
                                    <ItemStyle Width="175px" />
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                            <SelectedRowStyle CssClass="Selected" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT Student.StudentID, Student.SMSPhoneNo, Student.StudentsName, Student.Gender, StudentsClass.RollNo, Student.ID, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END" UpdateCommand="UPDATE StudentsClass SET RollNo = @RollNo WHERE (SchoolID = @SchoolID) AND (StudentClassID = @StudentClassID) AND (EducationYearID = @EducationYearID)">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                                <asp:Parameter DefaultValue="Active" Name="Status" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                            <UpdateParameters>
                                <asp:Parameter Name="RollNo" />
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:Parameter Name="StudentClassID" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            </UpdateParameters>
                        </asp:SqlDataSource>
                    </div>
                    <br />
                    <asp:Button ID="All_Student_SubmitButton" runat="server" CssClass="btn btn-primary Show" OnClick="All_Student_SubmitButton_Click" Text="Change Subjects" ValidationGroup="2" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="tab-pane fade" id="panel2" role="tabpanel" aria-expanded="false">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="alert alert-success">Change Subjects of Class</div>
                    <div class="table-responsive">
                        <asp:GridView ID="AllStu_SubChangeGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SubjectID" DataSourceID="AllStu_SubChangeSQL" PagerStyle-CssClass="pgr" PageSize="20">
                            <AlternatingRowStyle CssClass="alt" />
                            <RowStyle CssClass="RowStyle" />
                            <Columns>
                                <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName" />
                                <asp:TemplateField HeaderText="Select" ShowHeader="False">
                                    <ItemTemplate>
                                        <asp:CheckBox ID="SubjectCheckBox" runat="server" Text=" " />
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" Width="30px" />
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Subject Type">
                                    <ItemTemplate>
                                        <asp:RadioButtonList ID="SubjectTypeRadioButtonList" runat="server" RepeatDirection="Horizontal">
                                            <asp:ListItem Selected="True">Compulsory</asp:ListItem>
                                            <asp:ListItem>Optional</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </ItemTemplate>
                                    <ItemStyle Width="175px" />
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                Empty
                            </EmptyDataTemplate>
                            <PagerStyle CssClass="pgr" />
                            <SelectedRowStyle CssClass="Selected" />
                        </asp:GridView>
                    </div>
                    <br />
                    <asp:Button ID="AllSubSubmitButton" runat="server" CssClass="btn btn-primary" OnClick="AllSubSubmitButton_Click" Text="Change Subjects" ValidationGroup="1" />
                    <asp:SqlDataSource ID="AllStu_SubChangeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM Subject WHERE (SchoolID = @SchoolID) ORDER BY SubjectName">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="AllStu_SubChangeRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM StudentRecord WHERE (StudentClassID = @StudentClassID) AND (SchoolID = @SchoolID)" InsertCommand="INSERT INTO StudentRecord(StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date) VALUES (@StudentID, @RegistrationID, @SchoolID, @StudentClassID, @SubjectID, @EducationYearID, @SubjectType, GETDATE())" SelectCommand="SELECT * FROM StudentRecord">
                        <DeleteParameters>
                            <asp:Parameter Name="StudentClassID" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:Parameter Name="StudentID" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:Parameter Name="StudentClassID" />
                            <asp:Parameter Name="SubjectID" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:Parameter Name="SubjectType" Type="String" />
                        </InsertParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="UpdateStudentRecordIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentRecordID, StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date FROM StudentRecord" UpdateCommand="UPDATE
Exam_Obtain_Marks
SET 
     Exam_Obtain_Marks.StudentRecordID =  StudentRecord.StudentRecordID
FROM            
   Exam_Obtain_Marks
INNER JOIN
   StudentRecord
ON 
StudentRecord.EducationYearID = Exam_Obtain_Marks.EducationYearID AND
StudentRecord.SubjectID = Exam_Obtain_Marks.SubjectID AND 
StudentRecord.StudentID = Exam_Obtain_Marks.StudentID AND 
StudentRecord.SchoolID = Exam_Obtain_Marks.SchoolID
WHERE (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID)

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
StudentRecord.SchoolID = Exam_Result_of_Subject.SchoolID
WHERE (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID)">
                        <UpdateParameters>
                            <asp:Parameter Name="ClassID" />
                            <asp:Parameter Name="SchoolID" />
                            <asp:Parameter Name="EducationYearID" />
                        </UpdateParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="StudentSubjectRecordsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM StudentRecord WHERE (StudentClassID = @StudentClassID) AND (SchoolID = @SchoolID)" InsertCommand="INSERT INTO StudentRecord(StudentID, RegistrationID, SchoolID, StudentClassID, SubjectID, EducationYearID, SubjectType, Date) VALUES (@StudentID, @RegistrationID, @SchoolID, @StudentClassID, @SubjectID, @EducationYearID, @SubjectType, GETDATE())" SelectCommand="SELECT * FROM StudentRecord">
                        <DeleteParameters>
                            <asp:Parameter Name="StudentClassID" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:Parameter Name="StudentID" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:Parameter Name="StudentClassID" />
                            <asp:Parameter Name="SubjectID" Type="Int32" />
                            <asp:Parameter Name="SubjectType" Type="String" />
                        </InsertParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="CreateGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM SubjectForGroup WHERE (ClassID = @ClassID) AND (SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%')" InsertCommand="INSERT INTO [SubjectForGroup] ([SchoolID], [RegistrationID], [ClassID], [SubjectID], [SubjectGroupID], [SubjectType], [Date]) VALUES (@SchoolID, @RegistrationID, @ClassID, @SubjectID, @SubjectGroupID, @SubjectType, GETDATE())" SelectCommand="SELECT * FROM [SubjectForGroup]">
                        <DeleteParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                            <asp:Parameter Name="SubjectID" Type="Int32" />
                            <asp:Parameter Name="SubjectGroupID" Type="String" />
                            <asp:Parameter Name="SubjectType" Type="String" />
                        </InsertParameters>
                    </asp:SqlDataSource>
                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="EroorSummer" DisplayMode="List" ShowMessageBox="True" ValidationGroup="1" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>


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


    <script>
        $(document).ready(function () {
            if ($('[id*=StudentsGridView] tr').length) {
                $(".Show").show();
            }
        });

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
            if ($('[id*=StudentsGridView] tr').length) {
                $(".Show").show();
            }

            //Select All Checkbox
            $("[id*=AllICheckBox]").on("click", function () {
                var a = $(this), b = $(this).closest("table");
                $("input[type=checkbox]", b).each(function () {
                    a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
                });
            });

            $("[id*=SubjectCheckBox]").on("click", function () {
                var a = $(this).closest("table"), b = $("[id*=AllICheckBox]", a);
                $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=SubjectCheckBox]", a).length == $("[id*=SubjectCheckBox]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
            });
        })
    </script>
</asp:Content>
