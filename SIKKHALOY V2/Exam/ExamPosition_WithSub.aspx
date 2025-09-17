<%@ Page Title="Tabulation Sheet" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="ExamPosition_WithSub.aspx.cs" Inherits="EDUCATION.COM.Exam.ExamPosition_WithSub" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/ExamPosition.css" rel="stylesheet" />
      <style>
        .FthSub { color:#304ffe;font-size: 12px;}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>
        <asp:Label ID="CGSSLabel" runat="server"></asp:Label>
    </h3>

    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY CreateClass.ClassID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
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
        <div class="form-group">
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID"
                OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
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
            <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID"
                OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
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
        <div class="form-group">
            <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control"
                DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID"
                OnDataBound="ExamDropDownList_DataBound" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.EducationYearID = @EducationYearID) AND (Exam_Name.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ClassID = @ClassID) ORDER BY Exam_Name.ExamID">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
        <div class="form-group">
            <asp:RadioButtonList ID="Resultstatus_RBList" CssClass="form-control" runat="server" RepeatDirection="Horizontal" AutoPostBack="True">
                <asp:ListItem Selected="True" Value="%">All</asp:ListItem>
                <asp:ListItem Value="P">Passed</asp:ListItem>
                <asp:ListItem Value="F">Failed</asp:ListItem>
            </asp:RadioButtonList>
        </div>
    </div>

    <div id="ExportPanel" runat="server" class="Exam_Position">
        <asp:Label ID="Export_ClassLabel" runat="server" Font-Bold="True" Font-Names="Tahoma" Font-Size="20px"></asp:Label>
        <div class="table-responsive">
            <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" AllowSorting="True" DataSourceID="ShowStudentClassSQL"
                DataKeyNames="SMSPhoneNo,StudentID,PassStatus_InSubject,StudentsName,ObtainedMark_ofStudent,Student_Grade,Student_Point,Position_InExam_Class,Position_InExam_Subsection"
                OnRowDataBound="StudentsGridView_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                    <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                    <asp:TemplateField HeaderText="Subjects - Marks">
                        <ItemTemplate>
                            <asp:HiddenField ID="StudentResultIDHiddenField" runat="server" Value='<%# Eval("StudentResultID") %>' />
                            <div class="d-flex justify-content-center">
                                <asp:Repeater ID="SMarksRepeater" runat="server" DataSourceID="SUB_markSQL">
                                    <ItemTemplate>
                                        <div class="Is_Subject">
                                             <input class="PassStatus_Subject" type="hidden" value='<%# Eval("PassStatus_Subject") %>' />
                                            <input class="SubjectType" type="hidden" value='<%# Eval("SubjectType") %>' />
                                            <asp:HiddenField ID="SubjectIDHiddenField" runat="server" Value='<%# Eval("SubjectID") %>' />
                                            <div style="border-right: 1px solid #ddd; padding: 0 3px;">
                                                <b class="d-block"><%# Eval("SubjectName") %></b>
                                                <b class="d-block"><%# Eval("Mark","{0:F2}") %></b>
                                                <asp:Repeater ID="SubRepeater" runat="server" DataSourceID="Sub_ExamSQL">
                                                    <ItemTemplate>
                                                        <div class="border-top"></div>
                                                        <span class="d-block"><%# Eval("SubExamName") %>: <%# Eval("MarksObtained") %></span>
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                                <asp:SqlDataSource ID="Sub_ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Exam_SubExam_Name.SubExamName, ISNULL(CAST(Exam_Obtain_Marks.MarksObtained AS char(10)), 'A') as MarksObtained FROM Exam_Obtain_Marks INNER JOIN Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID WHERE (Exam_Obtain_Marks.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.StudentResultID = @StudentResultID) AND (Exam_Obtain_Marks.SubjectID = @SubjectID)">
                                                    <SelectParameters>
                                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                                        <asp:ControlParameter ControlID="StudentResultIDHiddenField" Name="StudentResultID" PropertyName="Value" />
                                                        <asp:ControlParameter ControlID="SubjectIDHiddenField" Name="SubjectID" PropertyName="Value" />
                                                    </SelectParameters>
                                                </asp:SqlDataSource>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:SqlDataSource ID="SUB_markSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Subject.SubjectName, Exam_Result_of_Subject.ObtainedMark_ofSubject AS Mark, Subject.SubjectID, Exam_Result_of_Subject.SubjectType, Exam_Result_of_Subject.PassStatus_Subject FROM Exam_Result_of_Subject INNER JOIN Subject ON Exam_Result_of_Subject.SubjectID = Subject.SubjectID WHERE (Exam_Result_of_Subject.StudentResultID = @StudentResultID) ORDER BY ISNULL(Subject.SN, 999), Subject.SubjectID">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="StudentResultIDHiddenField" Name="StudentResultID" PropertyName="Value" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ObtainedMark_ofStudent" HeaderText="Total" SortExpression="ObtainedMark_ofStudent" DataFormatString="{0:n2}" />
                    <asp:BoundField DataField="Student_Grade" HeaderText="Grade" SortExpression="Student_Grade" />
                    <asp:BoundField DataField="Student_Point" HeaderText="Point" SortExpression="Student_Point" DataFormatString="{0:0.00}" />
                    <asp:BoundField DataField="Position_InExam_Class" HeaderText="P.C" SortExpression="Position_InExam_Class" />
                    <asp:BoundField DataField="Position_InExam_Subsection" HeaderText="P.S" SortExpression="Position_InExam_Subsection" />
                </Columns>
            </asp:GridView>
        </div>
    </div>
    <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT StudentsClass.StudentClassID, Student.StudentID, Student.ID, Student.StudentsName, (CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END) AS RollNo, Exam_Result_of_Student.StudentResultID, Exam_Result_of_Student.ObtainedMark_ofStudent, Exam_Result_of_Student.Student_Grade, Exam_Result_of_Student.Student_Point, CAST(Exam_Result_of_Student.Position_InExam_Class AS int) AS Position_InExam_Class, CAST(Exam_Result_of_Student.Position_InExam_Subsection AS int) AS Position_InExam_Subsection, Student.SMSPhoneNo, Exam_Result_of_Student.ExamID, Exam_Result_of_Student.PassStatus_InSubject FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Exam_Result_of_Student ON StudentsClass.StudentClassID = Exam_Result_of_Student.StudentClassID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (Exam_Result_of_Student.ExamID = @ExamID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub') AND (Exam_Result_of_Student.PassStatus_InSubject LIKE @PassStatus) ORDER BY Position_InExam_Class">
        <SelectParameters>
            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
            <asp:Parameter DefaultValue="Active" Name="Status" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
            <asp:ControlParameter ControlID="Resultstatus_RBList" Name="PassStatus" PropertyName="SelectedValue" />
        </SelectParameters>
    </asp:SqlDataSource>

    <%if (StudentsGridView.Rows.Count > 0)
        { %>
    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:Button ID="ExportWordButton" runat="server" CssClass="btn btn-primary" OnClick="ExportWordButton_Click" Text="Export To Word" />
        </div>
        <div class="form-group">
            <input type="button" value="Print" class="btn btn-primary" onclick="window.print()" />
        </div>
    </div>
    <%}%>


    <script>
        $(function () {
            $(".PassStatus_Subject").each(function () {
                if ($(this).val() === 'F') {
                    $(this).parent('.Is_Subject').css({ "color": "red" });
                }
            });

            $(".SubjectType").each(function () {
                if ($(this).val() === 'Optional') {
                    $(this).parent('.Is_Subject').append('<span class="FthSub">4th</span>');
                }
            });
        });
    </script>
</asp:Content>
