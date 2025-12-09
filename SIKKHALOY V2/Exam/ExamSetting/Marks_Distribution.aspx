<%@ Page Title="Exam Marks Distribution" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Marks_Distribution.aspx.cs" Inherits="EDUCATION.COM.EXAM.ExamSetting.Marks_Distribution" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Mark_Distribution.css?v=3" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <div class="Container">
                <h3>Exam Marks distribution (Insert/Update)</h3>
                <button type="button" class="btn btn-blue-grey" data-toggle="modal" data-target="#CopyModal">Copy Marks from Previous Exam</button>

                <div class="row NoPrint">
                    <div class="col-sm-4">
                        <div class="form-group">
                            <label>
                                Class
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator></label>
                            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT DISTINCT CreateClass.ClassID, CreateClass.Class, CreateClass.SN
FROM            StudentRecord INNER JOIN
                         StudentsClass ON StudentRecord.StudentClassID = StudentsClass.StudentClassID INNER JOIN
                         CreateClass ON StudentsClass.ClassID = CreateClass.ClassID INNER JOIN
                         Student ON StudentRecord.StudentID = Student.StudentID
WHERE        (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = N'Active')
ORDER BY CreateClass.SN">
                                <SelectParameters>
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>
                    <div class="col-sm-4">
                        <div class="form-group">
                            <label>
                                Exam
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator></label>
                            <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
                                <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                SelectCommand="SELECT ExamID, SchoolID, RegistrationID, EducationYearID, ExamName, Date FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>
                    <div class="col-sm-4" id="GS" style="display: none;">
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

                <%if (ExamDropDownList.SelectedIndex > 0)
                    { %>
                <label id="Name" class="Clname"></label>
                <asp:GridView ID="SubjectGridView" runat="server" AutoGenerateColumns="False" DataSourceID="Mark4ClassSQL" CssClass="mGrid" OnDataBound="SubjectGridView_DataBound" DataKeyNames="SubjectID">
                    <Columns>
                        <asp:BoundField DataField="SubjectName" HeaderText="Subjects" SortExpression="SubjectName" />
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <div style="text-align: right">
                                    <asp:Label ID="ExamNameLabel" runat="server"></asp:Label>
                                </div>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <table>
                                    <tr>
                                        <td>
                                            <asp:TextBox ID="HMforExamTextBox" runat="server" CssClass="form-control" placeholder="Exam Full Marks" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:RegularExpressionValidator ID="FRex" runat="server" ControlToValidate="HMforExamTextBox" CssClass="EroorStar" ErrorMessage="*" SetFocusOnError="True" ValidationExpression="^\s*(?=.*[1-9])\d*(?:\.\d{1,2})?\s*$" ValidationGroup="Ex"></asp:RegularExpressionValidator>

                                            <asp:CheckBox ID="AddSubExamCheckBox" runat="server" AutoPostBack="True" OnCheckedChanged="AddSubExamCheckBox_CheckedChanged" Text="Sub-Exam" />
                                        </td>
                                    </tr>
                                </table>

                                <asp:GridView ID="SubExamGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="SubExamID" DataSourceID="SubExamNameQSL" Visible="False" CssClass="mGrid">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Sub-Exam">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="SubExamCheckBox" runat="server" Text='<%# Eval("SubExamName") %>' />
                                            </ItemTemplate>
                                            <ItemStyle HorizontalAlign="Left" />
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Full Marks">
                                            <ItemTemplate>
                                                <asp:RegularExpressionValidator ID="SubRV" runat="server" ControlToValidate="SubExamFullMarkTextBox" CssClass="EroorStar" ErrorMessage="Not Allowed" SetFocusOnError="True" ValidationExpression="^\s*(?=.*[1-9])\d*(?:\.\d{1,2})?\s*$" ValidationGroup="Ex"></asp:RegularExpressionValidator>
                                                <asp:TextBox ID="SubExamFullMarkTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control" Enabled="False" placeholder="Sub-Exam Full Marks"></asp:TextBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <EmptyDataTemplate>
                                        Sub-Exam Not created. <a href="Create_Edit_Delete_Exam_Role.aspx">Click here to create sub-Exam</a>
                                    </EmptyDataTemplate>
                                </asp:GridView>
                                <asp:SqlDataSource ID="SubExamNameQSL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                    SelectCommand="SELECT SubExamID, SchoolID, RegistrationID, EducationYearID, SubExamName FROM Exam_SubExam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) ORDER BY Sub_ExamSN">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </ItemTemplate>
                            <ItemStyle HorizontalAlign="Right" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="Mark4ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT DISTINCT Subject.SubjectName, Subject.SubjectID FROM Subject INNER JOIN StudentRecord ON Subject.SubjectID = StudentRecord.SubjectID INNER JOIN StudentsClass ON StudentRecord.StudentClassID = StudentsClass.StudentClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = N'Active')" UpdateCommand="Exam_Mark_Re_Submit" UpdateCommandType="StoredProcedure">
                    <SelectParameters>
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                    <UpdateParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                    </UpdateParameters>
                </asp:SqlDataSource>

                <div class="H_Submit">
                    <p class="Help">If You Modify Subject Marks After Publish Result, You have to Publish Result Again</p>
                    <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Submit" ValidationGroup="Ex" />
                </div>

                <asp:SqlDataSource ID="ExamFullMarksSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    InsertCommand="INSERT INTO Exam_Full_Marks(SchoolID, RegistrationID, SubjectID, ExamID, ClassID, SubExamID, EducationYearID, FullMarks, Date) VALUES (@SchoolID, @RegistrationID, @SubjectID, @ExamID, @ClassID, @SubExamID, @EducationYearID, @FullMarks, GETDATE())"
                    SelectCommand="SELECT * FROM [Exam_Full_Marks]" DeleteCommand="DELETE FROM Exam_Full_Marks WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)" UpdateCommand="UPDATE  Exam_Full_Marks
SET Sub_PassMarks = ROUND(Exam_Full_Marks.FullMarks * PM_T.PassMark / 100, 2, 0)
FROM Exam_Full_Marks INNER JOIN
(SELECT        ROUND(Exam_Grading_System.MaxPercentage, 0, 1) + 1 AS PassMark, Exam_Grading_Assign.SchoolID
FROM            Exam_Grading_System INNER JOIN
                         Exam_Grading_Assign ON Exam_Grading_System.GradeNameID = Exam_Grading_Assign.GradeNameID
WHERE        (Exam_Grading_System.Grades = 'F') AND (Exam_Grading_Assign.ExamID = @ExamID) AND (Exam_Grading_Assign.ClassID = @ClassID) AND (Exam_Grading_Assign.EducationYearID = @EducationYearID) AND 
                         (Exam_Grading_Assign.SchoolID = @SchoolID)) AS PM_T 
ON Exam_Full_Marks.SchoolID = PM_T.SchoolID
WHERE        
(Exam_Full_Marks.SchoolID = @SchoolID) AND 
(Exam_Full_Marks.ExamID = @ExamID) AND 
(Exam_Full_Marks.EducationYearID = @EducationYearID) AND 
(Exam_Full_Marks.ClassID = @ClassID)">
                    <DeleteParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    </DeleteParameters>
                    <InsertParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                        <asp:Parameter Name="SubjectID" Type="Int32" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                        <asp:Parameter Name="SubExamID" Type="Int32" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:Parameter Name="FullMarks" Type="Double" />
                    </InsertParameters>
                    <UpdateParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    </UpdateParameters>
                </asp:SqlDataSource>
                <%} %>
                <asp:SqlDataSource ID="UpdateGradeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT GradeNameID FROM Exam_Grading_Assign WHERE  (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID))
 BEGIN
 INSERT INTO Exam_Grading_Assign (SchoolID, RegistrationID, EducationYearID, ClassID, ExamID, GradeNameID) VALUES  (@SchoolID, @RegistrationID, @EducationYearID, @ClassID, @ExamID, @GradeNameID)
 END
 ELSE
 BEGIN
     UPDATE Exam_Grading_Assign SET GradeNameID =@GradeNameID WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @ExamID)
 END"
                    SelectCommand="SELECT SchoolID FROM Exam_Grading_Assign">
                    <InsertParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                        <asp:ControlParameter ControlID="Grading_System_DropDownList" Name="GradeNameID" PropertyName="SelectedValue" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>


    <!--Copy Marks Modal -->
    <div class="modal fade" id="CopyModal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel"
        aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">Copy From Previous Exam</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <div class="form-group">
                                <label>
                                    Class
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="CopyClass_DropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Copy"></asp:RequiredFieldValidator></label>
                                <asp:DropDownList ID="CopyClass_DropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True">
                                    <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="form-group">
                                <label>
                                    Copy From Exam
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="CopyFromExam_DropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Copy"></asp:RequiredFieldValidator></label>
                                <asp:DropDownList ID="CopyFromExam_DropDownList" runat="server" CssClass="form-control" DataSourceID="CopyFromExamSQL" DataTextField="ExamName" DataValueField="ExamID" OnDataBound="CopyFromExam_DropDownList_DataBound">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="CopyFromExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                    SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Full_Marks ON Exam_Name.ExamID = Exam_Full_Marks.ExamID WHERE (Exam_Name.SchoolID = @SchoolID) AND (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ClassID = @ClassID)">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                        <asp:ControlParameter ControlID="CopyClass_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            <div class="form-group">
                                <label>
                                    Copy To Exam
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="CopyToExam_DropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Copy"></asp:RequiredFieldValidator></label>
                                <asp:DropDownList ID="CopyToExam_DropDownList" runat="server" CssClass="form-control" DataSourceID="CopyToExamSQL" DataTextField="ExamName" DataValueField="ExamID" OnDataBound="CopyToExam_DropDownList_DataBound">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="CopyToExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                    SelectCommand="SELECT DISTINCT ExamID, ExamName FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ExamID NOT IN (SELECT DISTINCT ExamID FROM Exam_Full_Marks WHERE (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID)))">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                        <asp:ControlParameter ControlID="CopyClass_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            <div class="form-group">
                                <asp:Button ID="CopyButton" OnClick="CopyButton_Click" ValidationGroup="Copy" runat="server" Text="Submit" CssClass="btn btn-deep-orange" />
                                <asp:Label ID="SuccessLabel" runat="server" CssClass="text-success"></asp:Label>
                                 <asp:SqlDataSource ID="CopyMarksSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="DELETE FROM Exam_Grading_Assign WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @To_ExamID)
DELETE FROM Exam_Full_Marks WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (ClassID = @ClassID) AND (ExamID = @To_ExamID)

INSERT INTO Exam_Grading_Assign (SchoolID, RegistrationID, EducationYearID, ClassID, ExamID, GradeNameID) 
SELECT @SchoolID, @RegistrationID, @EducationYearID, @ClassID, @To_ExamID, GradeNameID FROM Exam_Grading_Assign WHERE (SchoolID = @SchoolID) AND (ClassID = @ClassID) AND (ExamID = @From_ExamID) AND (EducationYearID = @EducationYearID)

INSERT INTO Exam_Full_Marks (SchoolID, RegistrationID, SubjectID, ExamID, ClassID, SubExamID, EducationYearID, FullMarks, Date, Sub_PassMarks) 
SELECT @SchoolID, @RegistrationID, SubjectID, @To_ExamID, @ClassID, SubExamID, @EducationYearID, FullMarks, GETDATE(), Sub_PassMarks FROM Exam_Full_Marks WHERE (ClassID = @ClassID) AND (EducationYearID = @EducationYearID) AND (SchoolID = @SchoolID) AND (ExamID = @From_ExamID)" SelectCommand="SELECT ExamFullMarksID FROM Exam_Full_Marks ">
                                    <InsertParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                        <asp:ControlParameter ControlID="CopyClass_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="CopyFromExam_DropDownList" Name="From_ExamID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="CopyToExam_DropDownList" Name="To_ExamID" PropertyName="SelectedValue" />
                                    </InsertParameters>
                                </asp:SqlDataSource>
                            </div>
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
                <img src="../../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

    <script type="text/javascript">
        //GridView is empty
        if (!$('[id*=SubjectGridView] tr').length) {
            $(".H_Submit").hide();
        }

        if ($("[id*=Grading_System_DropDownList] option").length > 1) {
            $("#GS").show();
        }


        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($("[id*=Grading_System_DropDownList] option").length > 1) {
                $("#GS").show();
            }

            //GridView is empty
            if (!$('[id*=SubjectGridView] tr').length) {
                $(".H_Submit").hide();
            }

            $("#Name").text("(Marks Distribution) Class: " + $('[id*=ClassDropDownList] :selected').text() + ". Exam: " + $('[id*=ExamDropDownList] :selected').text());

            //on click
            $("[id*=SubExamCheckBox]").on("click", function () {
                var td = $("td", $(this).closest("tr"));

                if ($(this).is(":checked")) {
                    $("[id*=SubExamFullMarkTextBox]", td).prop("disabled", false);
                }
                else {
                    $("[id*=SubExamFullMarkTextBox]", td).prop("disabled", true).val('');
                }
            });

            //On load
            $("table tr td table tr td input[type='checkbox']").each(function () {
                var ischecked = $(this).is(":checked");
                if (ischecked) {
                    $(this).parent().next().find("input[type='text']").prop("disabled", false);
                }
            });
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        function DisableButton() { document.getElementById("<%=SubmitButton.ClientID %>").disabled = true; }
        window.onbeforeunload = DisableButton;
    </script>
</asp:Content>
