<%@ Page Title="Publish Cumulative Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Cumulative_Setting.aspx.cs" Inherits="EDUCATION.COM.Exam.CumulativeResult.Cumulative_Setting" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Publish Cumulative Result</h3>

    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
        <ContentTemplate>
            <div class="row mb-3">
                <div class="col-lg-4">
                    <label>
                        Class
           <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                    </label>
                    <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="col-lg-4">
                    <label>
                        <a class="blue-text" data-toggle="modal" data-target="#myModal"><i class="fa fa-plus-square mr-1"></i>Add New Cumulative Exam</a>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                    </label>
                    <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID" AppendDataBoundItems="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT CumulativeNameID, SchoolID, RegistrationID, EducationYearID, CumulativeResultName, Date FROM Exam_Cumulative_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="col-lg-4" id="GS" style="display: none;">
                    <div class="form-group">
                        <label>Grading System</label>
                        <asp:DropDownList ID="Grading_System_DropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Grading_SystemSQL" DataTextField="GradeName" DataValueField="GradeNameID" AppendDataBoundItems="True">
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="Grading_SystemSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT GradeNameID, GradeName FROM Exam_Grade_Name WHERE (SchoolID = @SchoolID)">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>

            <div class="card mb-4">
                <div class="card-header">Exam & Percentage</div>
                <div class="card-body">
                    <asp:DataList ID="ExamListDataList" runat="server" DataKeyField="ExamID" RepeatLayout="Flow" DataSourceID="ExamListSQL" RepeatDirection="Horizontal" Width="100%">
                        <ItemTemplate>
                            <div class="form-inline">
                                <asp:CheckBox ID="ExamCheckBox" runat="server" Text='<%# Eval("ExamName") %>' />
                                <div class="md-form mx-3 mb-2">
                                    <asp:TextBox onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" ID="PercentageTextBox" CssClass="form-control" runat="server" placeholder="Enter Exam %"></asp:TextBox>
                                </div>
                                <asp:CheckBox ID="EnableFailCheckBox" runat="server" Text="Enable Fail" />
                                <asp:HiddenField ID="ExamID_HF" runat="server" Value='<%# Eval("ExamID") %>' />
                            </div>
                        </ItemTemplate>
                    </asp:DataList>
                    <asp:SqlDataSource ID="ExamListSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Exam_Cumulative_ExamList WHERE (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)" InsertCommand="INSERT INTO Exam_Cumulative_ExamList(SchoolID, RegistrationID, CumulativeNameID, EducationYearID, ExamID, ClassID, ExamAdd_Percentage, Exam_EnableFail, Cumulative_SettingID, Publish_SettingID) 
SELECT  @SchoolID, @RegistrationID, @CumulativeNameID, @EducationYearID, @ExamID, @ClassID, @ExamAdd_Percentage, @Exam_EnableFail, @Cumulative_SettingID,Publish_SettingID
FROM  Exam_Publish_Setting
WHERE        (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)"
                        SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName
FROM            Exam_Result_of_Student INNER JOIN
                         Exam_Name ON Exam_Result_of_Student.ExamID = Exam_Name.ExamID
WHERE        (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.ClassID = @ClassID)
ORDER BY Exam_Name.ExamID">
                        <DeleteParameters>
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                        </DeleteParameters>
                        <InsertParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                            <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:Parameter Name="ExamID" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:Parameter Name="ExamAdd_Percentage" />
                            <asp:Parameter Name="Exam_EnableFail" />
                            <asp:Parameter Name="Cumulative_SettingID" />
                        </InsertParameters>
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div class="card mb-4">
                <div class="card-header">Position Settings</div>
                <div class="card-body">
                    <asp:RadioButtonList ID="Position_RadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="mb-2">
                        <asp:ListItem Selected="True" Value="Point">Position Based On Grade And Point</asp:ListItem>
                        <asp:ListItem Value="Mark">Position Based On Total obtained mark</asp:ListItem>
                    </asp:RadioButtonList>

                    <asp:CheckBox ID="SectionPositionCheckBox" Text="Hide Section Position" runat="server" />
                    <asp:CheckBox ID="ClassPositionCheckBox" Text="Hide Class Position" runat="server" />
                </div>
            </div>

            <div class="card mb-4">
                <div class="card-header">Optional subject Settings</div>
                <div class="card-body">
                    <div class="form-group">
                        <asp:RadioButtonList ID="OptionalSubjectRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" CssClass="form-control mb-2" OnSelectedIndexChanged="OptionalSubjectRadioButtonList_SelectedIndexChanged">
                            <asp:ListItem Selected="True">Add Optional Subject's Full Mark In Total Marks</asp:ListItem>
                            <asp:ListItem>Add Specific (%) Mark from Optional Subject</asp:ListItem>
                            <asp:ListItem Value="Don't Add Optional Subject's Mark">Do Not Add Optional Subject's Mark</asp:ListItem>
                        </asp:RadioButtonList>

                        <asp:Panel ID="SpecificMarkPanel" runat="server" Visible="False">
                            <div class="md-form m-0">
                                <asp:RequiredFieldValidator ID="OptionalPercentageRequired" runat="server" ControlToValidate="MinPercentageTextBox" CssClass="EroorSummer" ErrorMessage="Enter %" SetFocusOnError="True" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator ID="OptionalRegularExpressionValidator" runat="server" ControlToValidate="MinPercentageTextBox" ErrorMessage="Enter between ( 0 - 100 )" ForeColor="Red" ValidationExpression="^(100\.00|100\.0|100)|([0-9]{1,2}){0,1}(\.[0-9]{1,2}){0,1}$" ValidationGroup="Ex"></asp:RegularExpressionValidator>
                                <asp:TextBox ID="MinPercentageTextBox" runat="server" CssClass="form-control" placeholder="Enter Additional %"></asp:TextBox>
                                <small class="form-text text-muted">Minimum ( 0 - 100 )% Mark (Additional obtained mark will be added from the given percentage)</small>
                            </div>
                        </asp:Panel>
                    </div>

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
                    <asp:CheckBox ID="SubExam_ShowCheckBox" runat="server" Text="Hide Sub-Exam" Checked="true" />

                    <asp:RadioButtonList ID="ExamRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="True" CssClass="form-control my-3" OnSelectedIndexChanged="ExamRadioButtonList_SelectedIndexChanged">
                        <asp:ListItem Selected="True">Countable Exam Marks Same for Subjects</asp:ListItem>
                        <asp:ListItem>Countable Exam Marks Different for Different Subjects</asp:ListItem>
                    </asp:RadioButtonList>

                    <asp:MultiView ID="EMMultiView" runat="server">
                        <asp:View ID="SameView" runat="server">
                            <div class="md-form">
                                <asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="AlTotalMarksTextBox" CssClass="EroorStar" ErrorMessage="0 Not Allowed" SetFocusOnError="True" ValidationExpression="^[1-9]\d*$" ValidationGroup="Ex"></asp:RegularExpressionValidator>
                                <asp:RequiredFieldValidator ID="AlTotalMarkssRequired" runat="server" ControlToValidate="AlTotalMarksTextBox" CssClass="EroorStar" ErrorMessage="Enter Mark" SetFocusOnError="True" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                                <asp:TextBox ID="AlTotalMarksTextBox" runat="server" CssClass="form-control" placeholder="Enter Countable Mark"></asp:TextBox>
                            </div>
                        </asp:View>
                        <asp:View ID="DifferentView" runat="server">
                            <div class="alert alert-warning">
                                Unchecked subject marks will not add in total marks and grade point
                            </div>
                            <asp:GridView ID="DifferntMarksGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="SubjectID" DataSourceID="DifferentMarksSQL" PagerStyle-CssClass="pgr">
                                <AlternatingRowStyle CssClass="alt" />
                                <Columns>
                                    <asp:TemplateField HeaderText="Added In Exam" SortExpression="IS_Add_InExam">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="IS_Add_ExamCheckBox" runat="server" Checked="true" Text=" " />
                                        </ItemTemplate>
                                        <ItemStyle Width="120px" />
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName" />
                                    <asp:TemplateField HeaderText="Enter Marks">
                                        <HeaderTemplate>
                                            <b>Enter Marks</b>
                                            <asp:TextBox ID="MarksSameAllTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control form-control-sm" placeholder="Marks Set For All" runat="server"></asp:TextBox>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="HMforExamTextBox" CssClass="EroorStar" ErrorMessage="0 Not Allowed" SetFocusOnError="True" ValidationExpression="^[1-9]\d*$" ValidationGroup="Ex"></asp:RegularExpressionValidator>
                                            &nbsp;<asp:RequiredFieldValidator ID="ExamMarksRequired" runat="server" ControlToValidate="HMforExamTextBox" CssClass="EroorStar" ErrorMessage="Enter Marks" SetFocusOnError="True" ValidationGroup="Ex"></asp:RequiredFieldValidator>
                                            <asp:TextBox ID="HMforExamTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" placeholder="Exam Full Marks" Text=""></asp:TextBox>
                                            <asp:HiddenField ID="SubID_HF" runat="server" Value='<%# Eval("SubjectID") %>' />
                                        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Right" />
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    <b style="color: red;">Select Class and Exam</b>
                                </EmptyDataTemplate>
                                <PagerStyle CssClass="pgr" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="DifferentMarksSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Exam_Cumulative_FullMarks
WHERE        (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)"
                                InsertCommand="INSERT INTO Exam_Cumulative_FullMarks(CumulativeNameID, SchoolID, RegistrationID, SubjectID, ClassID, EducationYearID, FullMarks, Cumulative_SettingID) VALUES (@CumulativeNameID, @SchoolID, @RegistrationID, @SubjectID, @ClassID, @EducationYearID, @FullMarks, @Cumulative_SettingID)" SelectCommand="SELECT DISTINCT Exam_Result_of_Subject.SubjectID, Subject.SubjectName, Subject.SN FROM Exam_Result_of_Subject INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID WHERE (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.ClassID = @ClassID) ORDER BY Subject.SN">
                                <DeleteParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:Parameter Name="SubjectID" Type="Int32" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:Parameter Name="FullMarks" />
                                    <asp:Parameter Name="Cumulative_SettingID" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="ResetExamMarkSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT FullMarks
FROM            Exam_Cumulative_FullMarks
WHERE        (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="Exam_Publish_SettingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Exam_Cumulative_Setting
WHERE  (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)"
                                InsertCommand="INSERT INTO Exam_Cumulative_Setting(CumulativeNameID, SchoolID, RegistrationID, EducationYearID, ClassID, IS_Fail_Enable_Optional_Subject, IS_Add_Optional_Mark_In_FullMarks, IS_Enable_Grade_as_it_is_if_Fail, Optional_Percentage_Deduction, Exam_Position_Format, IS_Hide_SubExam, IS_Hide_Sec_Position,IS_Hide_Class_Position, Attendance_FromDate, Attendance_ToDate, GradeNameID,IS_Grade_BasePoint) VALUES (@CumulativeNameID, @SchoolID, @RegistrationID, @EducationYearID, @ClassID, @IS_Fail_Enable_Optional_Subject, @IS_Add_Optional_Mark_In_FullMarks, @IS_Enable_Grade_as_it_is_if_Fail, @Optional_Percentage_Deduction, @Exam_Position_Format, @IS_Hide_SubExam, @IS_Hide_Sec_Position,@IS_Hide_Class_Position, @Attendance_FromDate, @Attendance_ToDate, @GradeNameID,@IS_Grade_BasePoint)
SET @Cumulative_SettingID = scope_identity()"
                                OnInserted="Exam_Publish_SettingSQL_Inserted" SelectCommand="SELECT * FROM Exam_Cumulative_Setting WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (CumulativeNameID = @CumulativeNameID)">
                                <DeleteParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:ControlParameter ControlID="ExamDropDownList" DefaultValue="" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="FailEnableInOptionalCheckBox" Name="IS_Fail_Enable_Optional_Subject" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="AddOptionalinTotalMarkCheckBox" Name="IS_Add_Optional_Mark_In_FullMarks" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="Grade_AS_ItisCheckBox" Name="IS_Enable_Grade_as_it_is_if_Fail" PropertyName="Checked" />
                                    <asp:Parameter Name="Optional_Percentage_Deduction" />
                                    <asp:ControlParameter ControlID="Position_RadioButtonList" DefaultValue="" Name="Exam_Position_Format" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="SubExam_ShowCheckBox" Name="IS_Hide_SubExam" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="SectionPositionCheckBox" Name="IS_Hide_Sec_Position" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="ClassPositionCheckBox" Name="IS_Hide_Class_Position" PropertyName="Checked" />
                                    <asp:ControlParameter ControlID="FromDateTextBox" Name="Attendance_FromDate" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ToDateTextBox" Name="Attendance_ToDate" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="Grading_System_DropDownList" Name="GradeNameID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="GradeSetting_RBList" Name="IS_Grade_BasePoint" PropertyName="SelectedValue" />
                                    <asp:Parameter Direction="Output" Name="Cumulative_SettingID" Size="50" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="Exam_PS_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="EXEC [dbo].[SP_Cumulative_Exam_Student] @SchoolID,@RegistrationID,@EducationYearID,@ClassID,@CumulativeNameID,@Cumulative_SettingID
EXEC [dbo].[SP_Cumulative_HighestMark_Position]@SchoolID,@EducationYearID,@ClassID,@CumulativeNameID,@Exam_Position_Format 
EXEC [dbo].[SP_Cumulative_Attendance] @SchoolID,@EducationYearID,@ClassID,@CumulativeNameID,@RegistrationID,@From_Date,@To_Date"
                                SelectCommand="SELECT * FROM [Exam_Cumulative_Name]">
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                    <asp:Parameter Name="Cumulative_SettingID" />
                                    <asp:ControlParameter ControlID="Position_RadioButtonList" Name="Exam_Position_Format" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="FromDateTextBox" Name="From_Date" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ToDateTextBox" Name="To_Date" PropertyName="Text" />
                                </InsertParameters>
                            </asp:SqlDataSource>
                            <asp:SqlDataSource ID="Sub_Add_ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="SP_Cumulative_Exam_Subject" InsertCommandType="StoredProcedure" SelectCommand="SELECT DISTINCT SubjectID, IS_Add_InExam FROM Exam_Cumulative_Subject WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (CumulativeNameID = @CumulativeNameID) AND (ClassID = @ClassID)" UpdateCommand="UPDATE       Exam_Cumulative_Subject
SET                IS_Add_InExam = @IS_Add_InExam
WHERE        (CumulativeNameID = @CumulativeNameID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (SubjectID = @SubjectID)">
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" Type="Int32" />
                                    <asp:Parameter Name="Cumulative_SettingID" Type="Int32" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="IS_Add_InExam" />
                                    <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:Parameter Name="SubjectID" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                        </asp:View>
                    </asp:MultiView>
                </div>
            </div>

            <div class="card mb-4">
                <div class="card-header">Count Attendace (From Date - To Date)</div>
                <div class="card-body">
                    <div class="form-inline">
                        <div class="md-form mr-2">
                            <asp:TextBox ID="FromDateTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="From Date" runat="server" CssClass="form-control Datetime"></asp:TextBox>
                        </div>
                        <div class="md-form">
                            <asp:TextBox ID="ToDateTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" placeholder="To Date" runat="server" CssClass="form-control Datetime"></asp:TextBox>
                        </div>
                    </div>
                </div>
            </div>

            <div class="form-group">
                <%if (ClassDropDownList.SelectedIndex > 0 && ExamDropDownList.SelectedIndex > 0)
                    {%>
                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" ValidationGroup="Ex" OnClick="SubmitButton_Click" />
                <%}%>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <!-- Modal -->
    <div id="myModal" class="modal fade" role="dialog">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Add Cumulative Exam</h5>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <div class="form-inline">
                                <div class="md-form">
                                    <asp:TextBox ID="CumulativeNameTextBox" runat="server" CssClass="form-control" placeholder="Cumulative Exam Name"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="CumulativeNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
                                </div>
                                <asp:Button ID="AddCumulativeButton" runat="server" CssClass="btn btn-primary" Text="Add" ValidationGroup="1" OnClick="AddCumulativeButton_Click" />
                            </div>

                            <asp:GridView ID="CumulativetNameGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="CumulativeNameID" DataSourceID="CumulativeNameSQL">
                                <Columns>
                                    <asp:TemplateField HeaderText="Cumulative Exam Name" SortExpression="CumulativeResultName">
                                        <EditItemTemplate>
                                            <asp:TextBox ID="TextBox1" CssClass="form-control" runat="server" Text='<%# Bind("CumulativeResultName") %>'></asp:TextBox>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("CumulativeResultName") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:CommandField ShowEditButton="True" />
                                </Columns>
                            </asp:GridView>
                            <asp:SqlDataSource ID="CumulativeNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Exam_Cumulative_Name] WHERE [CumulativeNameID] = @CumulativeNameID" InsertCommand="IF NOT EXISTS (SELECT * FROM Exam_Cumulative_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (CumulativeResultName = @CumulativeResultName))
INSERT INTO Exam_Cumulative_Name (SchoolID, RegistrationID, EducationYearID, CumulativeResultName) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @CumulativeResultName)"
                                SelectCommand="SELECT * FROM Exam_Cumulative_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)" UpdateCommand="IF NOT EXISTS (SELECT * FROM Exam_Cumulative_Name WHERE (SchoolID = @SchoolID) AND (CumulativeResultName = @CumulativeResultName))
UPDATE Exam_Cumulative_Name SET CumulativeResultName = @CumulativeResultName WHERE (CumulativeNameID = @CumulativeNameID)">
                                <DeleteParameters>
                                    <asp:Parameter Name="CumulativeNameID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                                    <asp:ControlParameter ControlID="CumulativeNameTextBox" Name="CumulativeResultName" PropertyName="Text" Type="String" />
                                </InsertParameters>
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                </SelectParameters>
                                <UpdateParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:Parameter Name="CumulativeResultName" Type="String" />
                                    <asp:Parameter Name="CumulativeNameID" Type="Int32" />
                                </UpdateParameters>
                            </asp:SqlDataSource>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>

        </div>
    </div>


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
        var index = $("[id*=Grading_System_DropDownList] option").length;
        if (index > 1) {
            $("#GS").show();
        }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            //Marks to All
            $("[id*=MarksSameAllTextBox]").on("keyup", function () {
                $("[id*=HMforExamTextBox]").val($.trim($(this).val()));
            });

            var index = $("[id*=Grading_System_DropDownList] option").length;
            if (index > 1) {
                $("#GS").show();
            }
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
