<%@ Page Title="Position Of Subject" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="ExamPosition_Subject.aspx.cs" Inherits="EDUCATION.COM.Exam.ExamPosition_Subject" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/ExamPosition.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
        <ContentTemplate>
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
                <div class="form-group">
                    <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" OnDataBound="ExamDropDownList_DataBound" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
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
                    <asp:DropDownList ID="SubjectDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubjectSQL" DataTextField="SubjectName" DataValueField="SubjectID" OnDataBound="SubjectDropDownList_DataBound" OnSelectedIndexChanged="SubjectDropDownList_SelectedIndexChanged">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Subject.SubjectID, Subject.SubjectName, Exam_Result_of_Student.StudentPublishStatus FROM Subject INNER JOIN Exam_Result_of_Subject ON Subject.SubjectID = Exam_Result_of_Subject.SubjectID INNER JOIN StudentsClass ON Exam_Result_of_Subject.StudentClassID = StudentsClass.StudentClassID INNER JOIN Exam_Result_of_Student ON Exam_Result_of_Subject.StudentResultID = Exam_Result_of_Student.StudentResultID WHERE (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub')">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                            <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" PagerStyle-CssClass="pgr" CssClass="mGrid" AllowSorting="True" DataSourceID="ShowStudentClassSQL"
                    OnRowDataBound="StudentsGridView_RowDataBound" DataKeyNames="PassStatus_Subject,ObtainedMark_ofSubject,SubjectGrades,SubjectPoint,SMSPhoneNo,StudentsName,StudentID">
                    <AlternatingRowStyle CssClass="alt" />
                    <RowStyle CssClass="RowStyle" />
                    <Columns>
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <asp:CheckBox ID="AllIteamCheckBox" runat="server" Text=" " />
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="SingleCheckBox" runat="server" Text=" " />
                            </ItemTemplate>
                            <HeaderStyle CssClass="NoPrint" />
                            <ItemStyle Width="50px" CssClass="NoPrint" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone" SortExpression="SMSPhoneNo" />
                        <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                        <asp:TemplateField>
                            <HeaderTemplate>
                                <label id="SubjectName"></label>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <asp:DataList ID="SMarksDataList" runat="server" DataSourceID="SUB_markSQL" RepeatDirection="Horizontal" Width="100%">
                                    <ItemTemplate>
                                        <asp:Label ID="SubjectNameLabel" runat="server" Text='<%# Eval("SubExamName") %>' />
                                        <br />
                                        <asp:Label ID="MarkLabel" runat="server" Text='<%# Eval("MarksObtained") %>' Font-Bold="True" />
                                    </ItemTemplate>
                                </asp:DataList>
                                <asp:HiddenField ID="StudentResultIDHiddenField" runat="server" Value='<%# Eval("StudentResultID") %>' />
                                <asp:SqlDataSource ID="SUB_markSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Exam_SubExam_Name.SubExamName, ISNULL(CAST(Exam_Obtain_Marks.MarksObtained AS NVARCHAR(50)),'A') AS MarksObtained  FROM Exam_Obtain_Marks LEFT OUTER JOIN Exam_SubExam_Name ON Exam_Obtain_Marks.SubExamID = Exam_SubExam_Name.SubExamID WHERE (Exam_Obtain_Marks.StudentResultID = @StudentResultID) AND (Exam_Obtain_Marks.SubjectID = @SubjectID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="StudentResultIDHiddenField" Name="StudentResultID" PropertyName="Value" />
                                        <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="ObtainedMark_ofSubject" HeaderText="Total" DataFormatString="{0:n2}" SortExpression="ObtainedMark_ofSubject" />
                        <asp:BoundField DataField="SubjectGrades" HeaderText="Grade" SortExpression="SubjectGrades" />
                        <asp:BoundField DataField="SubjectPoint" HeaderText="Point" SortExpression="SubjectPoint" />
                        <asp:BoundField DataField="Position_InSubject_Class" HeaderText="P.C" SortExpression="Position_InSubject_Class" />
                        <asp:BoundField DataField="Position_InSubject_Subsection" HeaderText="P.S" SortExpression="Position_InSubject_Subsection" />
                    </Columns>

                    <EmptyDataTemplate>
                        Empty
                    </EmptyDataTemplate>

                    <PagerStyle CssClass="pgr" />
                    <SelectedRowStyle CssClass="Selected" />
                </asp:GridView>
                <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT StudentsClass.StudentClassID, Student.StudentID, Student.ID, Student.StudentsName, Student.SMSPhoneNo, StudentsClass.RollNo, Subject.SubjectName, Exam_Result_of_Subject.ObtainedMark_ofSubject, Exam_Result_of_Subject.SubjectGrades, Exam_Result_of_Subject.SubjectPoint, CAST(Exam_Result_of_Subject.Position_InSubject_Class AS int) AS Position_InSubject_Class, CAST(Exam_Result_of_Subject.Position_InSubject_Subsection AS int) AS Position_InSubject_Subsection, Exam_Result_of_Subject.PassStatus_Subject, Exam_Result_of_Subject.StudentResultID, Exam_Result_of_Subject.SubjectID FROM Exam_Result_of_Student INNER JOIN Subject INNER JOIN Exam_Result_of_Subject ON Subject.SubjectID = Exam_Result_of_Subject.SubjectID ON Exam_Result_of_Student.StudentResultID = Exam_Result_of_Subject.StudentResultID INNER JOIN StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID ON Exam_Result_of_Student.StudentClassID = StudentsClass.StudentClassID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.ExamID = @ExamID) AND (Exam_Result_of_Subject.SubjectID = @SubjectID) AND (Exam_Result_of_Student.StudentPublishStatus = N'Pub') ORDER BY StudentsClass.RollNo">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                        <asp:Parameter DefaultValue="Active" Name="Status" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SubjectDropDownList" DefaultValue="" Name="SubjectID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:SqlDataSource ID="SMS_OtherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SMS_OtherInfo(SMS_Send_ID, SchoolID, StudentID, TeacherID, EducationYearID) VALUES (@SMS_Send_ID, @SchoolID, @StudentID, @TeacherID, @EducationYearID)" SelectCommand="SELECT * FROM [SMS_OtherInfo]">
                    <InsertParameters>
                        <asp:Parameter Name="SMS_Send_ID" DbType="Guid" />
                        <asp:Parameter Name="SchoolID" />
                        <asp:Parameter Name="StudentID" />
                        <asp:Parameter Name="TeacherID" />
                        <asp:Parameter Name="EducationYearID" />
                    </InsertParameters>
                </asp:SqlDataSource>
            </div>

            <%if (StudentsGridView.Rows.Count > 0)
                { %>
            <asp:CheckBox ID="Send_DetailsCheckBox" runat="server" Text="Send Sub-Exam Details SMS" />
            <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
            <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="Validate" ErrorMessage="You do not select any student from student list." ForeColor="Red" ValidationGroup="1"></asp:CustomValidator>

            <div class="form-inline">
                <div class="form-group">
                    <asp:Button ID="SMSButton" runat="server" CssClass="btn btn-primary" Text="Send Result By SMS" OnClick="SMSButton_Click" ValidationGroup="1" />
                </div>
                <div class="form-group">
                    <input type="button" value="Print" class="btn btn-primary" onclick="window.print()" />
                </div>
            </div>
            <%}%>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="SMSButton" />
        </Triggers>
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
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($('[id*=SubjectDropDownList] :selected').index() > 0) {
                $("#SubjectName").text($('[id*=SubjectDropDownList] :selected').text());
            }

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

        //Select at least one Checkbox From GridView
        function Validate(d, c) {
            for (var b = document.getElementById("<%=StudentsGridView.ClientID %>").getElementsByTagName("input"), a = 0; a < b.length; a++) {
                if ("checkbox" == b[a].type && b[a].checked) {
                    c.IsValid = !0;
                    return;
                }
            }
            c.IsValid = !1;
        };
    </script>
</asp:Content>
