<%@ Page Title="Input Exam Marks" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="Input_MarksOld.aspx.cs" Inherits="EDUCATION.COM.Teacher.Input_Marks" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .EmptyRows { background-color: #f9d122; }

        .hide_Cont { color: #fff; margin-bottom: 15px; }
        .FullM { background-color: #0094ff; padding: 4px 0; }
        .PassM { background-color: #00bb93; padding: 4px 0; }
        .PassP { background-color: #efb700; padding: 4px 0; }

        .EroorSummer { font-size: 15px; }
        .SuccessMsg { font-size: 15px; color: #32b000; }
        .mGrid tr:nth-child(even) { background: #eee; }
        .mGrid tr:nth-child(odd) { background: #fff; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Student's Exam Marks (Input/Modify) <small id="Total_Student"></small></h3>
    <h5 id="locked"></h5>

    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
        <ContentTemplate>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="Input_ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Input_ExamSQL" DataTextField="ExamName" DataValueField="ExamID" OnSelectedIndexChanged="Input_ExamDropDownList_SelectedIndexChanged" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Input_ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ExamID, ExamName FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="Input_ExamDropDownList" CssClass="EroorSummer" ErrorMessage="Select Exam" Text="*" InitialValue="0" ValidationGroup="In_Exm"></asp:RequiredFieldValidator>
                </div>

                <div class="form-group">
                    <asp:DropDownList ID="Input_Class_DropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="Input_Class_DropDownList_SelectedIndexChanged" OnDataBound="Input_Class_DropDownList_DataBound">
                        <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.SN, CreateClass.ClassID FROM CreateClass INNER JOIN Exam_Full_Marks ON CreateClass.ClassID = Exam_Full_Marks.ClassID INNER JOIN TecherSubject ON CreateClass.ClassID = TecherSubject.ClassID WHERE (CreateClass.SchoolID = @SchoolID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ExamID = @ExamID) AND (TecherSubject.TeacherID = @TeacherID) ORDER BY CreateClass.SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="Input_ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="Input_Class_DropDownList" CssClass="EroorStar" ErrorMessage="Select class" Text="*" InitialValue="0" ValidationGroup="In_Exm">*</asp:RequiredFieldValidator>
                </div>

                <div class="form-group">
                    <asp:DropDownList ID="Input_Group_DropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="Input_GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="Input_Group_DropDownList_DataBound" OnSelectedIndexChanged="Input_Group_DropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Input_GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Input_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_Section_DropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_Shift_DropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="form-group">
                    <asp:DropDownList ID="Input_Section_DropDownList" runat="server" CssClass="form-control" DataSourceID="Input_SectionSQL" DataTextField="Section" DataValueField="SectionID" AutoPostBack="True" OnDataBound="Input_Section_DropDownList_DataBound" OnSelectedIndexChanged="Input_Section_DropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Input_SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Input_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_Group_DropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_Shift_DropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="form-group">
                    <asp:DropDownList ID="Input_Shift_DropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="Input_ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="Input_Shift_DropDownList_DataBound" OnSelectedIndexChanged="Input_Shift_DropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="Input_ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="Input_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_Group_DropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_Section_DropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="form-group">
                    <asp:DropDownList ID="SubjectDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubjectSQL" DataTextField="SubjectName" DataValueField="SubjectID" OnDataBound="SubjectDropDownList_DataBound" OnSelectedIndexChanged="SubjectDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Subject.SubjectID, Subject.SubjectName FROM Subject INNER JOIN StudentRecord ON Subject.SubjectID = StudentRecord.SubjectID INNER JOIN StudentsClass ON StudentRecord.StudentClassID = StudentsClass.StudentClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN TecherSubject ON Subject.SubjectID = TecherSubject.SubjectID AND StudentsClass.ClassID = TecherSubject.ClassID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = N'Active') AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (TecherSubject.TeacherID = @TeacherID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="Input_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_Group_DropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="SubjectDropDownList" CssClass="EroorStar" ErrorMessage="Select subject" InitialValue="0" ValidationGroup="In_Exm">*</asp:RequiredFieldValidator>
                </div>

                <div class="form-group">
                    <asp:DropDownList ID="SubExamDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubExamSQL" DataTextField="SubExamName" DataValueField="SubExamID" OnDataBound="SubExamDownList_DataBound" Visible="False" OnSelectedIndexChanged="SubExamDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SubExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Exam_SubExam_Name.SubExamID, Exam_SubExam_Name.SubExamName FROM Exam_SubExam_Name INNER JOIN Exam_Full_Marks ON Exam_SubExam_Name.SubExamID = Exam_Full_Marks.SubExamID WHERE (Exam_Full_Marks.SubjectID = @SubjectID) AND (Exam_Full_Marks.ClassID = @ClassID) AND (Exam_SubExam_Name.SchoolID = @SchoolID) AND (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="Input_ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="SubExamRequired" runat="server" ControlToValidate="SubExamDownList" CssClass="EroorStar" ErrorMessage="Select part of exam" ValidationGroup="In_Exm" Enabled="False">*</asp:RequiredFieldValidator>
                </div>

                <div class="form-group">
                    <asp:SqlDataSource ID="PassMarkFullMarkSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Sub_PassMarks AS PassMark, FullMarks AS FullMark, ROUND(Sub_PassMarks * 100 / FullMarks, 2, 0) AS PassPercentage FROM Exam_Full_Marks WHERE (SchoolID = @SchoolID) AND (SubjectID = @SubjectID) AND (ExamID = @ExamID) AND (ClassID = @ClassID) AND (SubExamID = @SubExamID OR @SubExamID = 0) AND (EducationYearID = @EducationYearID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" Type="Int32" />
                            <asp:ControlParameter ControlID="Input_ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                            <asp:ControlParameter ControlID="Input_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                            <asp:Parameter DefaultValue="" Name="SubExamID" Type="Int32" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <asp:Button ID="ShowStudentButton" runat="server" CssClass="btn btn-primary" OnClick="ShowStudentButton_Click" Text="Show Student" ValidationGroup="In_Exm" />
                </div>
            </div>

            <asp:FormView ID="FmPmFormView" CssClass="hide_Cont" runat="server" DataKeyNames="FullMark,PassMark,PassPercentage" DataSourceID="PassMarkFullMarkSQL" Width="100%" Visible="False">
                <ItemTemplate>
                    <div class="row no-gutters text-center">
                        <div class="col-sm-4">
                            <div class="FullM">
                                Full Mark: <%# Eval("FullMark") %>
                            </div>
                        </div>
                        <div class="col-sm-4">
                            <div class="PassM">
                                Pass Mark: <%# Eval("PassMark") %>
                            </div>
                        </div>
                        <div class="col-sm-4">
                            <div class="PassP">
                                Pass Percentage: <%# Eval("PassPercentage") %> %
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:FormView>

            <div class="table-responsive mb-3">
                <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="ShowStudentClassSQL" DataKeyNames="StudentID,StudentClassID" OnRowDataBound="StudentsGridView_RowDataBound" Visible="False" OnDataBound="StudentsGridView_DataBound">
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                        <asp:BoundField DataField="FathersName" HeaderText="Father's Name" SortExpression="FathersName" />
                        <asp:BoundField DataField="RollNo" HeaderText="Roll No." SortExpression="RollNo" />
                        <asp:TemplateField HeaderText="Obtain Marks">
                            <ItemTemplate>
                                <asp:TextBox ID="MarksTextBox" runat="server" CssClass="InputVibl form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false" onkeypress="return isNumberKey(event)"></asp:TextBox>
                            </ItemTemplate>
                            <ItemStyle Width="100px" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Absence">
                            <ItemTemplate>
                                <asp:CheckBox ID="AbsenceCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <ItemStyle Width="50px" HorizontalAlign="Center" />
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.FathersName, StudentsClass.StudentID, StudentsClass.StudentClassID, Student.ID, StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN StudentRecord ON StudentsClass.StudentClassID = StudentRecord.StudentClassID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentRecord.SubjectID = @SubjectID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = N'Active') ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="Input_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="Input_Section_DropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="Input_Group_DropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="Input_Shift_DropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <div class="hide_Cont">
                <div class="alert alert-warning NoPrint">After Marks Input Or Existing Marks Change, You have to Publish The Result</div>
                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" OnClick="SubmitButton_Click" Text="Submit" ValidationGroup="In_Exm" Visible="False" />
                <label id="ErMsg" class="EroorSummer"></label>
                <label id="SuccMsg" class="SuccessMsg"></label>
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" ShowMessageBox="True" ShowSummary="False" ValidationGroup="In_Exm" />
            </div>

            <asp:SqlDataSource ID="Exam_Result_of_StudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="Exam_Mark_Submit" SelectCommand="SELECT * FROM [Exam_Result_of_Student]" InsertCommandType="StoredProcedure">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                    <asp:ControlParameter ControlID="Input_Class_DropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="Input_ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:Parameter Name="StudentID" Type="Int32" />
                    <asp:Parameter Name="SubExamID" Type="Int32" />
                    <asp:Parameter Name="MarksObtained" Type="Double" />
                    <asp:Parameter Name="AbsenceStatus" Type="String" />
                    <asp:Parameter Name="FullMark" Type="Double" />
                    <asp:Parameter Name="PassPercentage" Type="Double" />
                    <asp:Parameter Name="PassMark" Type="Double" />
                </InsertParameters>
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


    <script type="text/javascript">
        $(function () {
            $("#_1").addClass("active");
        });

        //GridView is empty
        if (!$('[id*=StudentsGridView] tr').length) {
            $(".hide_Cont").hide();
        }

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $("#_1").addClass("active");

            //GridView is empty
            $("#locked > label").remove();
            if ($('[id*=StudentsGridView] tr').length) {
                //Count Student
                $('#Total_Student').text("Total Student: " + $("[id*=StudentsGridView] td").closest("tr").length);

                //On load
                var tr = $("[id*=StudentsGridView] tr").hasClass("aspNetDisabled");
                if (!tr) {
                    $(":checkbox").each(function () {
                        $(this).parents("tr:eq(0)").find(".InputVibl").prop("disabled", this.checked);
                    });
                } else {
                    $("#locked").append('<label class="alert alert-danger m-0 d-block">Marks Input Locked</label>');
                }

                //On click
                $(":checkbox").on('click', function () {
                    $(this).parents("tr:eq(0)").find(".InputVibl").prop("disabled", this.checked).val('');
                });
            }
            else {
                $(".hide_Cont").hide();
            }
        });

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
        function DisableButton() { document.getElementById("<%=SubmitButton.ClientID %>").disabled = !0; }
        window.onbeforeunload = DisableButton;
        function Success() {
            var e = $('#SuccMsg');
            e.text("Exam Marks inputed successfully!!");
            e.fadeIn();
            e.queue(function () { setTimeout(function () { e.dequeue() }, 4000) });
            e.fadeOut('slow');
        }
        function ErrorM() {
            var e = $('#ErMsg');
            e.text("Input Mark Or Checked Absent required");
            e.fadeIn();
            e.queue(function () { setTimeout(function () { e.dequeue() }, 4000) });
            e.fadeOut('slow');
        }
    </script>
</asp:Content>
