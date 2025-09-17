<%@ Page Title="Marks Collect Paper" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Marks_Collect_Paper.aspx.cs" Inherits="EDUCATION.COM.Exam.ExamSetting.Marks_Collect_Paper" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid th { border: 1px solid #000; }
        .mGrid td { border: 1px solid #000 !important; }
        .Ex_Cls { font-size:18px; font-weight:bold; text-align: center; display: none; margin-bottom: 8px; border-bottom: 1px solid #000; color:#000}
        .Class_GSS { font-size: 16px; font-weight:bold; text-align:center; display: none; color:#000}
        .Ins_Name { display:none;}
        @media print {
            .Ex_Cls, .Class_GSS { display:block !important;}
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
            <h3 class="d-print-none">Exam Marks Collect Paper</h3>

            <div class=" form-inline d-print-none">
                <div class="form-group">
                    <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT ExamID, ExamName FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="Select Exam" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" OnDataBound="ClassDropDownList_DataBound">
                        <asp:ListItem Value="0">- Select -</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM CreateClass INNER JOIN Exam_Full_Marks ON CreateClass.ClassID = Exam_Full_Marks.ClassID WHERE (CreateClass.SchoolID = @SchoolID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ExamID = @ExamID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="GroupDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="SectionDropDownList" runat="server" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" AutoPostBack="True" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
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
                    <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
                <div class="form-group">
                    <asp:DropDownList ID="SubjectDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubjectSQL" DataTextField="SubjectName" DataValueField="SubjectID" OnDataBound="SubjectDropDownList_DataBound" OnSelectedIndexChanged="SubjectDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Subject.SubjectID, Subject.SubjectName FROM Subject INNER JOIN StudentRecord ON Subject.SubjectID = StudentRecord.SubjectID INNER JOIN StudentsClass ON StudentRecord.StudentClassID = StudentsClass.StudentClassID INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = N'Active') AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="SubjectDropDownList" CssClass="EroorStar" ErrorMessage="Select subject" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                </div>
                <div class="form-group">
                    <button type="button" class="btn btn-primary" onclick="window.print()"><span class="glyphicon glyphicon-print" aria-hidden="true"></span> Print</button>
                </div>
                <div class="form-group">
                    <input type="button" id="ExportWord" class="btn btn-primary" value="Export To Word" />
                </div>
            </div>

            <div id="Ex-word-data">
                <h2 class="Ins_Name"></h2>
                <div class="Ex_Cls">
                    <label id="Exam_Name" class="d-block mb-0 mt-1"></label>
                    <label id="Class_Name" class="d-block"></label>
                </div>
                <div class="Class_GSS">
                    <label id="Subject"></label>
                    <label id="number-of-student"></label>
                </div>
                <div class="clearfix"></div>

                <div class="table-responsive">
                    <asp:GridView ID="StudentsGridView" AllowSorting="True" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="ShowStudentClassSQL" DataKeyNames="StudentID,StudentClassID" OnRowDataBound="StudentsGridView_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                            <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" >
                            <ItemStyle HorizontalAlign="Left" />
                            </asp:BoundField>
                            <asp:BoundField DataField="RollNo" HeaderText="Roll No." SortExpression="RollNo" />
                        </Columns>
                        <PagerStyle CssClass="pgr" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.StudentsName, Student.FathersName, StudentsClass.StudentID, StudentsClass.StudentClassID, Student.ID, StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN StudentRecord ON StudentsClass.StudentClassID = StudentRecord.StudentClassID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentRecord.SubjectID = @SubjectID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (StudentsClass.SchoolID = @SchoolID) AND (Student.Status = N'Active')  
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:SqlDataSource ID="SubExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT        Exam_SubExam_Name.SubExamName
FROM            Exam_Full_Marks LEFT OUTER JOIN
                         Exam_SubExam_Name ON Exam_Full_Marks.SubExamID = Exam_SubExam_Name.SubExamID
WHERE        (Exam_Full_Marks.SchoolID = @SchoolID) AND (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.ClassID = @ClassID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND 
                         (Exam_Full_Marks.SubjectID = @SubjectID)
ORDER BY Exam_SubExam_Name.Sub_ExamSN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>

                    <%if (StudentsGridView.Rows.Count > 0)
                        {%>
                    <div class="mt-4">
                        Teacher Signature
                    </div>
                    <%} %>
                </div>
            </div>
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

    <script src="/JS/ExportWord/FileSaver.js"></script>
    <script src="/JS/ExportWord/jquery.wordexport.js"></script>
    <script>
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
            var Class = "";
            if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                Class = "Class: " + $('[id*=ClassDropDownList] :selected').text();
            }
            var Group = "";
            if ($('[id*=GroupDropDownList] :selected').index() > 0) {
                Group = ". Group: " + $('[id*=GroupDropDownList] :selected').text();
            }
            var Section = "";
            if ($('[id*=SectionDropDownList] :selected').index() > 0) {
                Section = ". Section: " + $('[id*=SectionDropDownList] :selected').text();
            }
            var Shift = "";
            if ($('[id*=ShiftDropDownList] :selected').index() > 0) {
                Shift = ". Shift: " + $('[id*=ShiftDropDownList] :selected').text();
            }
            var Subject = "";
            if ($('[id*=SubjectDropDownList] :selected').index() > 0) {
                Subject = $('[id*=SubjectDropDownList] :selected').text();
            }

            $('#Exam_Name').text($('[id*=ExamDropDownList] :selected').text());
            $('#Class_Name').text(Class + Group + Section + Shift);
            $('#Subject').text(`Subject: ${Subject}. `);

            $("#ExportWord").click(function () {
                $(".Class_GSS").show();
                $(".Ex_Cls").show();
                $(".Ins_Name").text($("#InstitutionName").text()).show();

                $("#Ex-word-data").wordExport("CLASS-" + $('[id*=ClassDropDownList] :selected').text());
                $(".Class_GSS").hide();
                $(".Ex_Cls").hide();
                $(".Ins_Name").hide();
            });

            //count number of student
            const gridViewRowCount = document.getElementById("<%= StudentsGridView.ClientID %>");
            

            if (gridViewRowCount) {
                document.getElementById("number-of-student").textContent = ` Total Student: ${gridViewRowCount.rows.length - 1}`;
            }
        });
    </script>
</asp:Content>
