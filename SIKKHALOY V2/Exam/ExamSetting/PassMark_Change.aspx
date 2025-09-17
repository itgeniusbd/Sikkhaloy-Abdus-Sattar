<%@ Page Title="Pass Marks Change" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="PassMark_Change.aspx.cs" Inherits="EDUCATION.COM.Exam.ExamSetting.PassMark_Change" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid th:last-child { text-align:left; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="Container">
        <h3>Exam Pass Marks Change</h3>

        <div class="form-inline NoPrint">
            <div class="form-group">
                <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID">
                    <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                </asp:DropDownList>
                <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="ExamName" DataValueField="ExamID" OnDataBound="ExamDropDownList_DataBound">
                </asp:DropDownList>
                <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Full_Marks ON Exam_Name.ExamID = Exam_Full_Marks.ExamID WHERE (Exam_Full_Marks.ClassID = @ClassID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.SchoolID = @SchoolID)">
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="Ex"></asp:RequiredFieldValidator>
            </div>

            <div class="form-group">
                <asp:DropDownList ID="SubExamDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubExamSQL" DataTextField="SubExamName" DataValueField="SubExamID" OnDataBound="SubExamDownList_DataBound">
                </asp:DropDownList>
                <asp:SqlDataSource ID="SubExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT ISNULL(Exam_SubExam_Name.SubExamID, - 1) AS SubExamID, ISNULL(Exam_SubExam_Name.SubExamName, 'No Sub Exam') AS SubExamName FROM Exam_SubExam_Name RIGHT OUTER JOIN Exam_Full_Marks ON Exam_SubExam_Name.SubExamID = Exam_Full_Marks.SubExamID WHERE (Exam_Full_Marks.ClassID = @ClassID) AND (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.SchoolID = @SchoolID)">
                    <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </div>

        <div class="table-responsive">
            <asp:GridView ID="SubExamGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="PassMarkSQL" DataKeyNames="ExamFullMarksID,FullMarks">
<AlternatingRowStyle CssClass="alt"></AlternatingRowStyle>
                <Columns>
                    <asp:BoundField DataField="SubjectName" HeaderText="Subject" SortExpression="SubjectName"/>
                    <asp:BoundField DataField="SubExamName" HeaderText="Sub Exam" SortExpression="SubExamName"/>
                    <asp:BoundField DataField="FullMarks" HeaderText="Full Mark" SortExpression="FullMarks"/>
                    <asp:TemplateField HeaderText="Pass Mark" SortExpression="Sub_PassMarks">
                        <HeaderTemplate>
                          Pass Marks  <input id="Round_PM" class="btn btn-primary btn-sm" type="button" value="Round Pass Marks (Ex: from 17.5 to 17)" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:TextBox ID="PassMarkTextBox" runat="server" Text='<%# Bind("Sub_PassMarks") %>' CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false" onkeypress="return isNumberKey(event)"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <PagerStyle CssClass="pgr" />
            </asp:GridView>
            <asp:SqlDataSource ID="PassMarkSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Exam_Full_Marks.ExamFullMarksID, Subject.SubjectName, Exam_SubExam_Name.SubExamName, Exam_Full_Marks.FullMarks, Exam_Full_Marks.Sub_PassMarks
FROM            Exam_Full_Marks INNER JOIN
                         Subject ON Exam_Full_Marks.SubjectID = Subject.SubjectID  LEFT OUTER JOIN
                         Exam_SubExam_Name ON Exam_Full_Marks.SubExamID = Exam_SubExam_Name.SubExamID
WHERE        (Exam_Full_Marks.SchoolID = @SchoolID) AND (Exam_Full_Marks.ExamID = @ExamID) AND (Exam_Full_Marks.EducationYearID = @EducationYearID) AND (Exam_Full_Marks.ClassID = @ClassID) AND (ISNULL(Exam_Full_Marks.SubExamID,-1) = @SubExamID OR @SubExamID = 0)"
                UpdateCommand="UPDATE Exam_Full_Marks SET Sub_PassMarks = @Sub_PassMarks WHERE (ExamFullMarksID = @ExamFullMarksID)" InsertCommand="Exam_Mark_Re_Submit" InsertCommandType="StoredProcedure">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" Type="Int32" />
                </InsertParameters>
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                    <asp:ControlParameter ControlID="SubExamDownList" DefaultValue="" Name="SubExamID" PropertyName="SelectedValue" />
                </SelectParameters>
                <UpdateParameters>
                    <asp:Parameter Name="Sub_PassMarks" />
                    <asp:Parameter Name="ExamFullMarksID" />
                </UpdateParameters>
            </asp:SqlDataSource>
        </div>

        <%if (SubExamGridView.Rows.Count > 0)
            {%>
        <div class="alert alert-danger mt-3">If You Modify Subject Pass Marks After Publish Result, You have to Publish Result Again.</div>
        <asp:Button ID="ChangeButton" runat="server" CssClass="btn btn-primary" OnClick="ChangeButton_Click" Text="Change Pass Mark" />
        <%} %>
    </div>

    <script type="text/javascript">
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };

        $("#Round_PM").on("click", function () {
            $('[id*=PassMarkTextBox]').each(function () {
                var val = Math.floor($(this).val());
                $(this).val(val);
            });
        });
    </script>
</asp:Content>
