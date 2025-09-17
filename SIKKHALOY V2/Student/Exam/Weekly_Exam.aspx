<%@ Page Title="Weekly Merit Exam" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Weekly_Exam.aspx.cs" Inherits="EDUCATION.COM.Student.Exam.Weekly_Exam" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Weekly Merit Exam</h3>

    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
        <ContentTemplate>
            <div class="form-inline">
                <div class="form-group">
                    <asp:DropDownList ID="MainExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="MainExamSQL" DataTextField="ExamName" DataValueField="ExamID" AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ SELECT EXAM ]</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <asp:SqlDataSource ID="MainExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                </SelectParameters>
            </asp:SqlDataSource>

            <asp:FormView ID="WeekExamDateFormView" runat="server" CssClass="Show" DataSourceID="WeekExamDateSQL" Width="100%">
                <ItemTemplate>
                    <h4>
                        <span class="badge bg-success"><%# Eval("StartDate","{0:D}") %></span>
                        <span class="badge badge-success"><%# Eval("EndDate","{0:D}") %></span>
                    </h4>
                </ItemTemplate>
            </asp:FormView>
            <asp:SqlDataSource ID="WeekExamDateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT MIN(ExamDate) AS StartDate, MAX(ExamDate) AS EndDate FROM WeeklyExam WHERE (StudentID = @StudentID) AND (ExamID = @ExamID)">
                <SelectParameters>
                    <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                    <asp:ControlParameter ControlID="MainExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>

            <asp:GridView ID="WeeklyExamGridView" runat="server" AutoGenerateColumns="False" DataSourceID="WeeklyExamSQL" CssClass="mGrid" OnRowDataBound="WeeklyExamGridView_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="Week" SortExpression="Week">
                        <ItemTemplate>
                            <%# Eval("Week") %> 
                            (<%# Eval("From", "{0:d MMM}") %>-
                            <%# Eval("To", "{0:d MMM}") %>)
                        </ItemTemplate>
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="Saturday" HeaderText="Saturday" ReadOnly="True" SortExpression="Saturday" />
                    <asp:BoundField DataField="Sunday" HeaderText="Sunday" ReadOnly="True" SortExpression="Sunday" />
                    <asp:BoundField DataField="Monday" HeaderText="Monday" ReadOnly="True" SortExpression="Monday" />
                    <asp:BoundField DataField="Tuesday" HeaderText="Tuesday" ReadOnly="True" SortExpression="Tuesday" />
                    <asp:BoundField DataField="Wednesday" HeaderText="Wednesday" ReadOnly="True" SortExpression="Wednesday" />
                    <asp:BoundField DataField="Thursday" HeaderText="Thursday" ReadOnly="True" SortExpression="Thursday" />
                    <asp:BoundField DataField="Total" HeaderText="Total" ReadOnly="True" SortExpression="Total">
                        <ItemStyle Font-Bold="True" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Rank" HeaderText="Position" ReadOnly="True" SortExpression="Rank">
                        <ItemStyle Font-Bold="True" Width="50px" />
                    </asp:BoundField>
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="WeeklyExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SET DATEFIRST 6;
 SELECT ' Week No. '+ cast((WeekNo) as varchar(12))  as [Week],
(SELECT MIN(ExamDate) FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and StudentID = @StudentID) as [From],
(SELECT MAX(ExamDate) FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and StudentID = @StudentID) as [To],
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and  StudentID = @StudentID and datename (dw, ExamDate) ='Saturday') as Saturday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and  StudentID = @StudentID and datename (dw, ExamDate) ='Sunday') as Sunday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and  StudentID = @StudentID and datename (dw, ExamDate) ='Monday') as Monday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and  StudentID = @StudentID and datename (dw, ExamDate) ='Tuesday') as Tuesday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and  StudentID = @StudentID and datename (dw, ExamDate) ='Wednesday') as Wednesday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and  StudentID = @StudentID and datename (dw, ExamDate) ='Thursday') as Thursday,
(SELECT sum(MarksObtained) FROM WeeklyExam WHERE EducationYearID = @EducationYearID and ExamID =@ExamID and datepart(WEEK,ExamDate) =WeekNo and StudentID = @StudentID) as Total,
(SELECT
(CASE Position
             when  1  then CAST(Position AS varchar(12)) + 'st'
			 when  2  then CAST(Position AS varchar(12)) + 'nd' 
			 when  3  then CAST(Position AS varchar(12)) + 'rd' 
             else CAST(Position AS varchar(12)) + 'th' 
             end) as [PositionRank] 
FROM (SELECT  DENSE_RANK() OVER (ORDER BY [Total Mark] DESC) AS Position,[Total Mark],StudentID

FROM (SELECT sum(MarksObtained) as [Total Mark], StudentID
FROM WeeklyExam WHERE datepart(WEEK,ExamDate) = WeekNo and  EducationYearID = @EducationYearID and ExamID =@ExamID and ClassID =(SELECT ClassID FROM StudentsClass WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID))  group by StudentID) as RankTable) as StudentRank where  StudentID = @StudentID ) as Rank 
from (select distinct datepart(WEEK,ExamDate) as WeekNo from WeeklyExam where ClassID = (SELECT ClassID FROM StudentsClass WHERE (StudentID = @StudentID) AND (EducationYearID = @EducationYearID)) and  ExamID =@ExamID and EducationYearID =@EducationYearID) as WeekTable order by WeekNo">
                <SelectParameters>
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                    <asp:ControlParameter ControlID="MainExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                    <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </ContentTemplate>
    </asp:UpdatePanel>

    <script>
        $(function () {
            $("#_3").addClass("active");
        });
    </script>
</asp:Content>
