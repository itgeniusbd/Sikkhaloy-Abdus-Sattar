<%@ Page Title="Subject Wise Marks Check" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Subject_Wise_Marks_Check.aspx.cs" Inherits="EDUCATION.COM.Exam.ExamSetting.Subject_Wise_Marks_Check" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid td:nth-child(2) { text-align: left; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="d-print-none">Subject Wise Marks</h3>

    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
        <ContentTemplate>
            <div class="text-center black-text d-print-block d-none">
                <h4 id="ExmName" class="mt-2"></h4>
                <h5 id="SubjectName"></h5>
                <h6 id="ClassName"></h6>
            </div>

            <div class=" form-inline d-print-none">
                <div class="form-group">
                    <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
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
                    <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.Class,CreateClass.SN, CreateClass.ClassID FROM CreateClass INNER JOIN Exam_Full_Marks ON CreateClass.ClassID = Exam_Full_Marks.ClassID WHERE (CreateClass.SchoolID = @SchoolID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ExamID = @ExamID)  ORDER BY CreateClass.SN">
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
                    <asp:DropDownList ID="SubjectDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubjectSQL" DataTextField="SubjectName" DataValueField="SubjectID" OnDataBound="SubjectDropDownList_DataBound">
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
                    <button type="button" onclick="window.print();" class="btn btn-blue">Print</button>
                </div>
            </div>

            <asp:GridView ID="SubjectMarksGridView" AllowSorting="true" runat="server" CssClass="mGrid" AutoGenerateColumns="True" DataSourceID="SubjectMarksSQL">
            </asp:GridView>
            <asp:SqlDataSource ID="SubjectMarksSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SP_SP_Exam_Subject_MarkCheck" SelectCommandType="StoredProcedure">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="String" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="String" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="String" />
                    <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" Type="String" />
                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" Type="String" />
                    <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" Type="String" />
                    <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" Type="String" />
                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="String" />
                </SelectParameters>
            </asp:SqlDataSource>
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


    <script>
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (e, f) {
            $(".mGrid tr td").each(function () {
                var marks = $(this);
                if (!marks.index()) return;

                if (marks.html() == '&nbsp;')
                    $(this).html('Abs').css("color","red");

            });

            var Exam = "";
            if ($('[id*=ExamDropDownList] :selected').index() > 0) {
                Exam = $('[id*=ExamDropDownList] :selected').text();
            }

            var Class = "";
            if ($('[id*=ClassDropDownList] :selected').index() > 0) {
                Class = " Class: " + $('[id*=ClassDropDownList] :selected').text();
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
                Subject = " Subject: " + $('[id*=SubjectDropDownList] :selected').text();
            }

            $('#ExmName').text(Exam);
            $('#ClassName').text(Class + Group + Section + Shift);
            $('#SubjectName').text(Subject);
        })
    </script>
</asp:Content>
