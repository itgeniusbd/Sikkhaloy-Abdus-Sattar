<%@ Page Title="Publish Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Publish_Result.aspx.cs" Inherits="EDUCATION.COM.Exam.Publish_Result" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="Container">
                <h3>Publish Individual Result</h3>
                <div class="row">
                    <div class="col-lg-4 col-sm-6">
                        <label>
                            Class
              <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorSummer" ErrorMessage="Select class" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                        </label>
                        <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                            <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>

                    <div class="col-lg-4 col-sm-6">
                        <label>
                            Exam 
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="Select Exam" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                        </label>
                        <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="ExamName" DataValueField="ExamID" OnDataBound="ExamDropDownList_DataBound" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
                            <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Result_of_Student INNER JOIN Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.ClassID = @ClassID) ORDER BY Exam_Name.ExamID">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:HyperLink ID="CreateExamLink" runat="server" Visible="False" NavigateUrl="~/Exam/Input_Exam_Marks.aspx">Exam Mark not given! Click here To Input Eark</asp:HyperLink>
                    </div>
                </div>

                <div class="card my-4">
                    <div class="card-header">Exam Position Settings</div>
                    <div class="card-body">
                        <div class="form-group">
                            <asp:RadioButtonList ID="Position_RadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                                <asp:ListItem Selected="True" Value="Point">Position Based On Grade And Point</asp:ListItem>
                                <asp:ListItem Value="Mark">Position Based On Total obtained mark</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>

                        <asp:CheckBox ID="SectionPositionCheckBox" Text="Hide Section Position" runat="server" />
                        <asp:CheckBox ID="ClassPositionCheckBox" Text="Hide Class Position" runat="server" />
                        <asp:CheckBox ID="H_FullMarkCheckBox" Text="Hide Full Mark" runat="server" />
                        <asp:CheckBox ID="H_PassMarkCheckBox" Text="Hide Pass Mark" runat="server" />
                    </div>
                </div>

                <div class="card mb-4">
                    <div class="card-header">Optional subject Settings</div>
                    <div class="card-body">
                        <asp:RadioButtonList ID="OptionalSubjectRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" CssClass="form-control mb-3">
                            <asp:ListItem Selected="True">Add Optional Subject's Full Mark In Total Marks</asp:ListItem>
                            <asp:ListItem>Add Specific (%) Mark from Optional Subject</asp:ListItem>
                            <asp:ListItem Value="Don't Add Optional Subject's Mark">Do Not Add Optional Subject's Mark</asp:ListItem>
                        </asp:RadioButtonList>

                        <asp:Panel ID="SpecificMarkPanel" runat="server" Visible="False">
                            <div class="md-form m-0">
                                <asp:RequiredFieldValidator ID="OptionalPercentageRequired" runat="server" ControlToValidate="MinPercentageTextBox" ForeColor="Red" ErrorMessage="Enter %" SetFocusOnError="True" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator ID="OptionalRegularExpressionValidator" runat="server" ControlToValidate="MinPercentageTextBox" ErrorMessage="Enter between ( 0 - 100 )" ForeColor="Red" ValidationExpression="^(100\.00|100\.0|100)|([0-9]{1,2}){0,1}(\.[0-9]{1,2}){0,1}$" ValidationGroup="Ex" />
                                <asp:TextBox ID="MinPercentageTextBox" runat="server" CssClass="form-control" placeholder="Enter Additional %" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                            </div>
                            <small class="form-text text-muted mb-3">Minimum ( 0 - 100 )% Mark (Additional obtained mark will be added from the given percentage)</small>
                        </asp:Panel>

                        <asp:CheckBox ID="FailEnableInOptionalCheckBox" runat="server" Text="Enable Fail, If Fail in Optional Subject" />
                        <asp:CheckBox ID="AddOptionalinTotalMarkCheckBox" runat="server" Text="Add Optional Exam Mark in Subject Full Marks" />
                    </div>
                </div>

                <div class="card mb-4">
                    <div class="card-header">Exam Settings</div>
                    <div class="card-body">
                        <div class="form-group">
                            <asp:RadioButtonList ID="GradeSetting_RBList" CssClass="form-control" runat="server" RepeatDirection="Horizontal">
                                <asp:ListItem Selected="True" Value="1">Grade Based On GPA</asp:ListItem>
                                <asp:ListItem Value="0">Grade Based On Average Mark</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>

                        <asp:CheckBox ID="Grade_AS_ItisCheckBox" runat="server" Text="Enable Grade As It Is, If Fail" />

                        <div class="mt-2">
                            <asp:RadioButtonList CssClass="form-control" ID="ExamRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True">
                                <asp:ListItem Selected="True">Countable Exam Marks Same for Subjects</asp:ListItem>
                                <asp:ListItem>Countable Exam Marks Different for Different Subjects</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>

                        <div class="form-group">
                            <asp:MultiView ID="EMMultiView" runat="server">
                                <asp:View ID="SameView" runat="server">
                                    <div class="md-form m-0">
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="AlTotalMarksTextBox" CssClass="EroorStar" ErrorMessage="0 Not Allowed" SetFocusOnError="True" ValidationExpression="^[1-9]\d*$" ValidationGroup="Ex"></asp:RegularExpressionValidator>
                                        <asp:RequiredFieldValidator ID="AlTotalMarkssRequired" runat="server" ControlToValidate="AlTotalMarksTextBox" CssClass="EroorStar" ErrorMessage="Enter Mark" SetFocusOnError="True" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                                        <asp:TextBox ID="AlTotalMarksTextBox" runat="server" CssClass="form-control" placeholder="Enter Publish Mark"></asp:TextBox>
                                    </div>
                                </asp:View>

                                <asp:View ID="DifferentView" runat="server">
                                    <div class="alert alert-warning mt-2">
                                        Unchecked subject marks will not add in total marks and grade point
                                    </div>

                                    <asp:GridView ID="DifferntMarksGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SubjectID" DataSourceID="DifferentMarksSQL" PagerStyle-CssClass="pgr">
                                        <AlternatingRowStyle CssClass="alt" />
                                        <Columns>
                                            <asp:TemplateField HeaderText="Added In Exam" SortExpression="IS_Add_InExam">
                                                <ItemTemplate>
                                                    <asp:CheckBox ID="IS_Add_ExamCheckBox" runat="server" Checked='<%# Bind("IS_Add_InExam") %>' Text=" " />
                                                </ItemTemplate>
                                                <ItemStyle Width="120px" />
                                            </asp:TemplateField>
                                            <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName" />
                                            <asp:TemplateField HeaderText="Enter Marks">
                                                <HeaderTemplate>
                                                    Enter Full Marks or <i class="fa fa-hand-o-right" aria-hidden="true"></i>
                                                    <asp:LinkButton ID="FullMarkButton" runat="server" Text="Set Full Mark As Mark Distribution" OnClick="FullMarkButton_Click" />
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="HMforExamTextBox" CssClass="EroorStar" ErrorMessage="0 Not Allowed" SetFocusOnError="True" ValidationExpression="^[1-9]\d*$" ValidationGroup="Ex"></asp:RegularExpressionValidator>
                                                    &nbsp;<asp:RequiredFieldValidator ID="ExamMarksRequired" runat="server" ControlToValidate="HMforExamTextBox" CssClass="EroorStar" ErrorMessage="Enter Marks" SetFocusOnError="True" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                                                    <asp:TextBox ID="HMforExamTextBox" runat="server" Text='<%# Bind("TotalMark_ofSubject") %>' CssClass="form-control" onkeypress="return isNumberKey(event)" placeholder="Exam Full Marks"></asp:TextBox>
                                                </ItemTemplate>
                                                <HeaderStyle HorizontalAlign="Left" />
                                            </asp:TemplateField>
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <b style="color: red;">Select Class and Exam</b>
                                        </EmptyDataTemplate>
                                        <PagerStyle CssClass="pgr" />
                                    </asp:GridView>
                                    <asp:SqlDataSource ID="DifferentMarksSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Exam_Result_of_Subject.SubjectID, Subject.SubjectName, MAX(Exam_Result_of_Subject.TotalMark_ofSubject) AS TotalMark_ofSubject, Exam_Result_of_Subject.IS_Add_InExam FROM Exam_Result_of_Subject INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) GROUP BY Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Exam_Result_of_Subject.IS_Add_InExam ORDER BY Exam_Result_of_Subject.SubjectID" InsertCommand="IF NOT EXISTS(SELECT * FROM Exam_Publish_Sub_Countable_Mark WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID))
BEGIN
INSERT INTO Exam_Publish_Sub_Countable_Mark (SchoolID, RegistrationID, EducationYearID, SubjectID, ExamID, ClassID, Countable_Mark)
                                    VALUES (@SchoolID, @RegistrationID, @EducationYearID, @SubjectID, @ExamID, @ClassID, @Countable_Mark)
END
ELSE
BEGIN
UPDATE Exam_Publish_Sub_Countable_Mark SET  Countable_Mark = @Countable_Mark 
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID)
END"
                                        UpdateCommand="UPDATE Exam_Result_of_Subject SET  IS_Add_InExam = @IS_Add_InExam 
WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID)">
                                        <InsertParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                            <asp:Parameter Name="SubjectID" Type="Int32" />
                                            <asp:Parameter Name="Countable_Mark" />
                                        </InsertParameters>
                                        <SelectParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                        </SelectParameters>
                                        <UpdateParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                            <asp:Parameter Name="SubjectID" />
                                            <asp:Parameter Name="IS_Add_InExam" />
                                        </UpdateParameters>
                                    </asp:SqlDataSource>
                                </asp:View>
                            </asp:MultiView>
                        </div>
                    </div>
                </div>

                <div class="card mb-4">
                    <div class="card-header">Sub Exam Settings</div>
                    <div class="card-body">
                        <asp:CheckBox ID="SubExamFailCheckBox" runat="server" Text="Enable Fail, if fail in sub-Exam" />

                        <div class="form-group mt-2">
                            <asp:RadioButtonList CssClass="form-control" ID="SubExamRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True">
                                <asp:ListItem Selected="True">Distribute Equally Sub-Exam Full Marks</asp:ListItem>
                                <asp:ListItem>Distribute Specifically Sub-Exam Marks</asp:ListItem>
                            </asp:RadioButtonList>
                        </div>

                        <asp:Panel ID="SubExamPanel" runat="server" Visible="False">
                            <asp:GridView ID="SubExam_Mark_GridView" runat="server" AutoGenerateColumns="False" DataSourceID="SubExam_Mark_SQL" CssClass="mGrid" DataKeyNames="SubjectID,SubExamID">
                                <Columns>
                                    <asp:BoundField DataField="SubjectName" HeaderText="Subject Name" SortExpression="SubjectName" />
                                    <asp:BoundField DataField="SubExamName" HeaderText="Sub-Exam Name" SortExpression="SubExamName" />
                                    <asp:TemplateField HeaderText="% (0-100)">
                                        <ItemTemplate>
                                            <asp:TextBox ID="SpecificSubExammarksTextBox" Text='<%# Bind("AddPercentage") %>' runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="ExamMarksRequired" runat="server" ControlToValidate="SpecificSubExammarksTextBox" CssClass="EroorStar" ErrorMessage="Enter %" SetFocusOnError="True" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                                            &nbsp;<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="SpecificSubExammarksTextBox" ErrorMessage="Enter between ( 0 - 100 )" ForeColor="Red" ValidationExpression="^(100\.00|100\.0|100)|([0-9]{1,2}){0,1}(\.[0-9]{1,2}){0,1}$" ValidationGroup="Ex"></asp:RegularExpressionValidator>
                                        </ItemTemplate>
                                        <ItemStyle Width="200px" />
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    No Sub Exam
                                </EmptyDataTemplate>
                            </asp:GridView>
                            <asp:SqlDataSource ID="SubExam_Mark_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT DISTINCT Exam_Obtain_Marks.SubjectID, Exam_Obtain_Marks.SubExamID, Subject.SubjectName, Exam_SubExam_Name.SubExamName, MAX(Exam_Obtain_Marks.AddPercentage) AS AddPercentage FROM Exam_Obtain_Marks INNER JOIN Subject ON Exam_Obtain_Marks.SubjectID = Subject.SubjectID INNER JOIN Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID WHERE (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.ExamID = @ExamID) GROUP BY Exam_Obtain_Marks.SubjectID, Exam_Obtain_Marks.SubExamID, Exam_SubExam_Name.SubExamName, Subject.SubjectName ORDER BY Exam_Obtain_Marks.SubjectID" UpdateCommand="UPDATE Exam_Obtain_Marks SET AddPercentage = @AddPercentage WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID) AND (SubjectID = @SubjectID) AND (SubExamID = @SubExamID)" InsertCommand="UPDATE Exam_Obtain_Marks SET AddPercentage = 100 WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)">
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                    <asp:Parameter Name="AddPercentage" />
                                    <asp:Parameter Name="SubjectID" />
                                    <asp:Parameter Name="SubExamID" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="Student_ResultSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="EXEC [dbo].[SP_Exam_Subject] @SchoolID,@EducationYearID,@ClassID,@ExamID
EXEC [dbo].[SP_Exam_Student] @SchoolID,@EducationYearID,@ClassID,@ExamID
EXEC [dbo].[SP_Exam_Attendance] @SchoolID,@EducationYearID,@ClassID,@ExamID,@RegistrationID,@From_Date,@To_Date
EXEC [dbo].[HighestMark_Position] @SchoolID,@EducationYearID,@ClassID,@ExamID,@Exam_Position_Format"
                                SelectCommand="SELECT * FROM [Exam_Result_of_Student]" OnInserting="Student_ResultSQL_Inserting">
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="Position_RadioButtonList" Name="Exam_Position_Format" PropertyName="SelectedValue" />
                                </InsertParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="Exam_Publish_SettingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Exam_Obtain_Marks FROM Exam_Obtain_Marks LEFT OUTER JOIN Exam_Full_Marks ON Exam_Obtain_Marks.SchoolID = Exam_Full_Marks.SchoolID AND Exam_Obtain_Marks.SubjectID = Exam_Full_Marks.SubjectID AND Exam_Obtain_Marks.ExamID = Exam_Full_Marks.ExamID AND Exam_Obtain_Marks.ClassID = Exam_Full_Marks.ClassID AND ISNULL(Exam_Obtain_Marks.SubExamID, 0) = ISNULL(Exam_Full_Marks.SubExamID, 0) AND Exam_Obtain_Marks.EducationYearID = Exam_Full_Marks.EducationYearID 
WHERE (Exam_Full_Marks.FullMarks IS NULL) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.ClassID = @ClassID)
DELETE FROM Exam_Obtain_Marks FROM Exam_Obtain_Marks LEFT OUTER JOIN StudentRecord ON Exam_Obtain_Marks.EducationYearID = StudentRecord.EducationYearID AND Exam_Obtain_Marks.SchoolID = StudentRecord.SchoolID AND Exam_Obtain_Marks.SubjectID = StudentRecord.SubjectID AND Exam_Obtain_Marks.StudentClassID = StudentRecord.StudentClassID 
WHERE (StudentRecord.StudentID IS NULL) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.ClassID = @ClassID)
DELETE FROM Exam_Result_of_Subject FROM Exam_Result_of_Subject LEFT OUTER JOIN StudentRecord ON Exam_Result_of_Subject.SchoolID = StudentRecord.SchoolID AND Exam_Result_of_Subject.SubjectID = StudentRecord.SubjectID AND Exam_Result_of_Subject.EducationYearID = StudentRecord.EducationYearID AND Exam_Result_of_Subject.StudentClassID = StudentRecord.StudentClassID 
WHERE (StudentRecord.StudentRecordID IS NULL) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.ClassID = @ClassID)

/*DELECT Promotion Demotion Student*/
DELETE FROM Exam_Obtain_Marks FROM Exam_Obtain_Marks INNER JOIN StudentsClass ON Exam_Obtain_Marks.StudentClassID = StudentsClass.StudentClassID 
WHERE (StudentsClass.New_StudentClassID IS NOT NULL) AND (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID)
DELETE FROM Exam_Result_of_Student FROM StudentsClass INNER JOIN Exam_Result_of_Student ON StudentsClass.StudentClassID = Exam_Result_of_Student.StudentClassID 
WHERE (StudentsClass.New_StudentClassID IS NOT NULL) AND (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) AND (Exam_Result_of_Student.ExamID = @ExamID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID)
DELETE FROM Exam_Result_of_Subject FROM StudentsClass INNER JOIN Exam_Result_of_Subject ON StudentsClass.StudentClassID = Exam_Result_of_Subject.StudentClassID 
WHERE (StudentsClass.New_StudentClassID IS NOT NULL) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID)

/*DELECT Rejected Student*/
DELETE FROM Exam_Obtain_Marks FROM Exam_Obtain_Marks INNER JOIN Student ON Exam_Obtain_Marks.StudentID = Student.StudentID
WHERE (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Student.Status = N'Rejected')
DELETE FROM Exam_Result_of_Subject FROM Exam_Result_of_Subject INNER JOIN Student ON Exam_Result_of_Subject.StudentID = Student.StudentID
WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Student.Status = N'Rejected') 
DELETE FROM Exam_Result_of_Student FROM Exam_Result_of_Student INNER JOIN Student ON Exam_Result_of_Student.StudentID = Student.StudentID
WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) AND (Exam_Result_of_Student.ExamID = @ExamID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Student.Status = N'Rejected')"
                                InsertCommand="IF NOT EXISTS(SELECT  * FROM  Exam_Publish_Setting
WHERE        (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExamID = @ExamID) AND (ClassID = @ClassID))
BEGIN
INSERT INTO Exam_Publish_Setting
                         (SchoolID, RegistrationID, EducationYearID, ClassID, ExamID, IS_Fail_Enable_Optional_Subject, IS_Add_Optional_Mark_In_FullMarks, IS_Enable_Grade_as_it_is_if_Fail, IS_Enable_Fail_if_fail_in_sub_Exam, 
                         Optional_Percentage_Deduction, IS_Published, Exam_Position_Format, IS_Hide_Sec_Position,IS_Hide_Class_Position, Attendance_FromDate, Attendance_ToDate,IS_Hide_FullMark,IS_Hide_PassMark,IS_Grade_BasePoint)
VALUES        (@SchoolID,@RegistrationID,@EducationYearID,@ClassID,@ExamID,@IS_Fail_Enable_Optional_Subject,@IS_Add_Optional_Mark_In_FullMarks,@IS_Enable_Grade_as_it_is_if_Fail,@IS_Enable_Fail_if_fail_in_sub_Exam,@Optional_Percentage_Deduction,@IS_Published,@Exam_Position_Format,@IS_Hide_Sec_Position,@IS_Hide_Class_Position,@Attendance_FromDate,@Attendance_ToDate,@IS_Hide_FullMark,@IS_Hide_PassMark,@IS_Grade_BasePoint)

END
ELSE
BEGIN
UPDATE       Exam_Publish_Setting
SET                IS_Fail_Enable_Optional_Subject = @IS_Fail_Enable_Optional_Subject, IS_Add_Optional_Mark_In_FullMarks = @IS_Add_Optional_Mark_In_FullMarks, 
                         IS_Enable_Grade_as_it_is_if_Fail = @IS_Enable_Grade_as_it_is_if_Fail, IS_Enable_Fail_if_fail_in_sub_Exam = @IS_Enable_Fail_if_fail_in_sub_Exam, 
                         Optional_Percentage_Deduction = @Optional_Percentage_Deduction, IS_Published = @IS_Published, Exam_Position_Format = @Exam_Position_Format, Last_Published_Date = GETDATE(), 
                         IS_Hide_Sec_Position = @IS_Hide_Sec_Position,IS_Hide_Class_Position=@IS_Hide_Class_Position, Attendance_FromDate = @Attendance_FromDate, Attendance_ToDate = @Attendance_ToDate,IS_Hide_FullMark = @IS_Hide_FullMark,IS_Hide_PassMark = @IS_Hide_PassMark, IS_Grade_BasePoint=@IS_Grade_BasePoint
WHERE        (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExamID = @ExamID) AND (ClassID = @ClassID)

END"
                                SelectCommand="SELECT * FROM Exam_Publish_Setting WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)">
                                <DeleteParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:ControlParameter ControlID="FailEnableInOptionalCheckBox" Name="IS_Fail_Enable_Optional_Subject" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="AddOptionalinTotalMarkCheckBox" Name="IS_Add_Optional_Mark_In_FullMarks" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="Grade_AS_ItisCheckBox" Name="IS_Enable_Grade_as_it_is_if_Fail" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="SubExamFailCheckBox" Name="IS_Enable_Fail_if_fail_in_sub_Exam" PropertyName="Checked" />
                                    <asp:Parameter Name="Optional_Percentage_Deduction" />
                                    <asp:Parameter DefaultValue="1" Name="IS_Published" />
                                    <asp:ControlParameter ControlID="Position_RadioButtonList" DefaultValue="" Name="Exam_Position_Format" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="SectionPositionCheckBox" Name="IS_Hide_Sec_Position" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="ClassPositionCheckBox" Name="IS_Hide_Class_Position" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="FromDateTextBox" Name="Attendance_FromDate" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ToDateTextBox" Name="Attendance_ToDate" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="H_FullMarkCheckBox" Name="IS_Hide_FullMark" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="H_PassMarkCheckBox" Name="IS_Hide_PassMark" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="GradeSetting_RBList" Name="IS_Grade_BasePoint" PropertyName="SelectedValue" />
                                    
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </asp:Panel>
                    </div>
                </div>

                <div class="card mb-3">
                    <div class="card-header">
                        Count Attendace (From Date - To Date)
                    </div>
                    <div class="card-body">
                        <div class="form-inline">
                            <div class="md-form mr-2">
                                <asp:TextBox onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" ID="FromDateTextBox" placeholder="From Date" runat="server" CssClass="form-control Datetime"></asp:TextBox>
                            </div>
                            <div class="md-form">
                                <asp:TextBox onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" ID="ToDateTextBox" placeholder="To Date" runat="server" CssClass="form-control Datetime"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </div>


                <%if (ClassDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedIndex > 0)
                    { %>
                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Submit" ValidationGroup="Ex" />
                <%} %>
                <asp:SqlDataSource ID="ResetOptionalSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  * FROM  Exam_Publish_Setting WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="ResetExamMarkSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT  DISTINCT Countable_Mark
                    FROM   Exam_Publish_Sub_Countable_Mark WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="ResetSubExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT AddPercentage
FROM Exam_Obtain_Marks WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="/CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script type="text/javascript">
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });


        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
