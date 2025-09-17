<%@ Page Title="Weekly Merit Status" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Weekly_Merit_Status_Results.aspx.cs" Inherits="EDUCATION.COM.EXAM.WeeklyExam.Weekly_Merit_Status_Results" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="CSS/Check-Weekly-Results.css" rel="stylesheet" />
   <link href="CSS/Print.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
      <ContentTemplate>
         <div class="Contain">
            <!--Contain-->
            <h3>Students Weekly Merit Status: </h3>

            <div class="Category">
               <table>
                  <tr>
                     <td>Select Exam </td>
                  </tr>
                  <tr>
                     <td>
                        <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" Width="147px" AppendDataBoundItems="True" AutoPostBack="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
                           <asp:ListItem Value="0">[ Select ]</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                           SelectCommand="SELECT ExamID, RegistrationID, ExamName, Date, SchoolID FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                           <SelectParameters>
                              <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                              <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                           </SelectParameters>
                        </asp:SqlDataSource>
                     </td>
                  </tr>
                  <tr>
                     <td>Select Week</td>
                  </tr>
                  <tr>
                     <td>
                        <asp:DropDownList ID="WeekDropDownList" runat="server" CssClass="form-control" Width="150px" AutoPostBack="True" OnSelectedIndexChanged="WeekDropDownList_SelectedIndexChanged">
                        </asp:DropDownList>
                     </td>
                  </tr>
               </table>
               <div class="Class">
                  Class
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
               </div>
               <div class="ClassCount">
                  <asp:FormView ID="StudentsFormView" runat="server" DataSourceID="Class">
                     <ItemTemplate>
                        (<asp:Label ID="TotalLabel" runat="server" Text='<%# Bind("Total") %>' />)
                     </ItemTemplate>
                  </asp:FormView>
               </div>
               <div>
                  <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" Width="150px" OnDataBound="ClassDropDownList_DataBound">
                  </asp:DropDownList>
                  <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                     SelectCommand="SET DATEFIRST 6;
SELECT DISTINCT CreateClass.Class, CreateClass.ClassID
FROM            CreateClass INNER JOIN
                         WeeklyExam ON CreateClass.ClassID = WeeklyExam.ClassID
WHERE        (WeeklyExam.ExamID = @ExamID) AND (DATEPART(WEEK, WeeklyExam.ExamDate) = @WeekNo) AND (WeeklyExam.EducationYearID = @EducationYearID)">
                     <SelectParameters>
                        <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="WeekDropDownList" Name="WeekNo" PropertyName="SelectedValue" />
                       <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                     </SelectParameters>
                  </asp:SqlDataSource>
               </div>

               <div class="Class">
                  <asp:Label ID="GroupLabel" runat="server" Text="Group"></asp:Label>
                  <asp:RequiredFieldValidator ID="GRequiredFieldValidator" runat="server" ControlToValidate="GroupDropDownList" CssClass="EroorStar" ErrorMessage="Select group" InitialValue="%" ValidationGroup="1">*</asp:RequiredFieldValidator>
               </div>
               <div class="ClassCount">
                  <asp:FormView ID="GroupFormView" runat="server" DataSourceID="Group">
                     <ItemTemplate>
                        (<asp:Label ID="TotalLabel" runat="server" Text='<%# Bind("Total") %>' />)
                     </ItemTemplate>
                  </asp:FormView>
               </div>
               <div>
                  <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup"
                     DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged" Width="150px">
                  </asp:DropDownList>
                  <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%')">
                     <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                     </SelectParameters>
                  </asp:SqlDataSource>
               </div>

               <div class="Class">
                  <asp:Label ID="SectionLabel" runat="server" Text="Section"></asp:Label>
                  <asp:RequiredFieldValidator ID="SeRequiredFieldValidator" runat="server" ControlToValidate="SectionDropDownList" CssClass="EroorStar" ErrorMessage="Select section" InitialValue="%" ValidationGroup="1">*</asp:RequiredFieldValidator>
               </div>
               <div class="ClassCount">
                  <asp:FormView ID="SectionFormView" runat="server" DataSourceID="Section">
                     <ItemTemplate>
                        (<asp:Label ID="TotalLabel" runat="server" Text='<%# Bind("Total") %>' />)               
                     </ItemTemplate>
                  </asp:FormView>
               </div>
               <div>
                  <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged" Width="150px">
                  </asp:DropDownList>
                  <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%')">
                     <SelectParameters>
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                     </SelectParameters>
                  </asp:SqlDataSource>
               </div>

               <div class="Class">
                  <asp:Label ID="ShiftLabel" runat="server" Text="Shift"></asp:Label>
                  <asp:RequiredFieldValidator ID="ShRequiredFieldValidator" runat="server" ControlToValidate="SectionDropDownList" CssClass="EroorStar" ErrorMessage="Select shift" InitialValue="%" ValidationGroup="1">*</asp:RequiredFieldValidator>
               </div>
               <div class="ClassCount">
                  <asp:FormView ID="ShiftFormView" runat="server" DataSourceID="Shift">
                     <ItemTemplate>
                        (<asp:Label ID="TotalLabel" runat="server" Text='<%# Bind("Total") %>' />)                          
                     </ItemTemplate>
                  </asp:FormView>
               </div>
               <div>
                  <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged" Width="150px">
                  </asp:DropDownList>
                  <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ClassID = @ClassID)">
                     <SelectParameters>
                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                     </SelectParameters>
                  </asp:SqlDataSource>
               </div>

               <asp:SqlDataSource ID="Class" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                  <SelectParameters>
                     <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                     <asp:Parameter DefaultValue="Active" Name="Status" />
                     <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                  </SelectParameters>
               </asp:SqlDataSource>
               <asp:SqlDataSource ID="Group" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.SubjectGroupID = @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                  <SelectParameters>
                     <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                     <asp:Parameter DefaultValue="Active" Name="Status" />
                   <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                  </SelectParameters>
               </asp:SqlDataSource>
               <asp:SqlDataSource ID="Section" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                  SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.SectionID = @SectionID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                  <SelectParameters>
                     <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                     <asp:Parameter DefaultValue="Active" Name="Status" />
                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                  </SelectParameters>
               </asp:SqlDataSource>
               <asp:SqlDataSource ID="Shift" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                  SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.ShiftID = @ShiftID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                  <SelectParameters>
                     <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                     <asp:Parameter DefaultValue="Active" Name="Status" />
              <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                  </SelectParameters>
               </asp:SqlDataSource>

            </div>
            <!--End Category-->

            <div id="AllStudents">
               <div class="ClassName">
                  <asp:Label ID="CGSSLabel" runat="server"></asp:Label>
               </div>

               <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" PageSize="20" OnRowDataBound="StudentsGridView_RowDataBound"
                  DataSourceID="ShowStudentClassSQL" AllowSorting="True" DataKeyNames="SMSPhoneNo,StudentID"
                  CssClass="mGrid">
                  <AlternatingRowStyle CssClass="alt" />
                  <Columns>
                     <asp:TemplateField HeaderText="SMS">
                        <HeaderTemplate>
                           <asp:CheckBox ID="SelectAllCheckBox" runat="server" Text="SMS" />
                        </HeaderTemplate>
                        <ItemTemplate>
                           <asp:CheckBox ID="SMSCheckBox" runat="server" Text=" " />
                        </ItemTemplate>
                        <FooterStyle CssClass="HideSMSCheckbox" />
                        <HeaderStyle CssClass="HideSMSCheckbox" />
                        <ItemStyle HorizontalAlign="Center" CssClass="HideSMSCheckbox" Width="50px" />

                     </asp:TemplateField>
                     <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID">
                        <ItemStyle HorizontalAlign="Center" />
                     </asp:BoundField>
                     <asp:BoundField DataField="RollNo" HeaderText="Roll">
                        <ItemStyle HorizontalAlign="Center" />
                     </asp:BoundField>
                     <asp:BoundField DataField="StudentsName" HeaderText="Name">
                        <ItemStyle HorizontalAlign="Left" />
                     </asp:BoundField>
                     <asp:BoundField DataField="SMSPhoneNo" HeaderText="Phone" SortExpression="SMSPhoneNo" />
                     <asp:BoundField DataField="Saturday" HeaderText="Sat" ReadOnly="True" SortExpression="Saturday">
                        <ItemStyle HorizontalAlign="Center" />
                     </asp:BoundField>
                     <asp:BoundField HeaderText="Sun" DataField="Sunday" SortExpression="Sunday" ReadOnly="True">
                        <ItemStyle HorizontalAlign="Center" />
                     </asp:BoundField>
                     <asp:BoundField DataField="Monday" HeaderText="Mon" SortExpression="Monday" ReadOnly="True">
                        <ItemStyle HorizontalAlign="Center" />
                     </asp:BoundField>
                     <asp:BoundField DataField="Tuesday" HeaderText="Tue" ReadOnly="True" SortExpression="Tuesday">
                        <ItemStyle HorizontalAlign="Center" />
                     </asp:BoundField>
                     <asp:BoundField DataField="Wednesday" HeaderText="Wed" ReadOnly="True" SortExpression="Wednesday">
                        <ItemStyle HorizontalAlign="Center" />
                     </asp:BoundField>
                     <asp:BoundField DataField="Thursday" HeaderText="Thu" ReadOnly="True" SortExpression="Thursday">
                        <ItemStyle HorizontalAlign="Center" />
                     </asp:BoundField>
                     <asp:BoundField DataField="Total Mark" HeaderText="Total" ReadOnly="True" SortExpression="Total Mark">
                        <ItemStyle HorizontalAlign="Center" Font-Bold="True" />
                     </asp:BoundField>
                     <asp:BoundField DataField="PositionRank" HeaderText="Position" ReadOnly="True" SortExpression="Position">
                        <ItemStyle HorizontalAlign="Center" Font-Bold="True" />
                     </asp:BoundField>
                     <asp:TemplateField HeaderText="Gur.Signature" />
                  </Columns>
                  <EmptyDataTemplate>
                     Empty
                  </EmptyDataTemplate>

               </asp:GridView>


               <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                  SelectCommand="SET DATEFIRST 6;
SELECT Student.StudentsName,Student.ID,Student.SMSPhoneNo,StudentsClass.RollNo, WeekRank.*,

(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE  datename (dw, ExamDate) ='Saturday' and datepart(WEEK,ExamDate) =@ExamWeek and ExamID =@ExamID and StudentID =StudentsClass.StudentID ) as Saturday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE  datename (dw, ExamDate) ='Sunday' and datepart(WEEK,ExamDate) =@ExamWeek and ExamID =@ExamID and StudentID =StudentsClass.StudentID  ) as Sunday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE  datename (dw, ExamDate) ='Monday' and datepart(WEEK,ExamDate) =@ExamWeek and ExamID =@ExamID and StudentID =StudentsClass.StudentID ) as Monday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE  datename (dw, ExamDate) ='Tuesday' and datepart(WEEK,ExamDate) =@ExamWeek and ExamID =@ExamID and StudentID =StudentsClass.StudentID ) as Tuesday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE  datename (dw, ExamDate) ='Wednesday'and datepart(WEEK,ExamDate) =@ExamWeek and ExamID =@ExamID and StudentID =StudentsClass.StudentID ) as Wednesday,
(SELECT ISNULL( cast((MarksObtained) as varchar(12)),'A') FROM WeeklyExam WHERE  datename (dw, ExamDate) ='Thursday' and datepart(WEEK,ExamDate) =@ExamWeek and ExamID =@ExamID and StudentID =StudentsClass.StudentID ) as Thursday 
FROM StudentsClass  INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID 
LEFT JOIN (select StudentID,Position,[Total Mark],
(case Position
             when  1  then CAST(Position AS varchar(12)) + 'st'
			 when  2  then CAST(Position AS varchar(12)) + 'nd' 
			 when  3  then CAST(Position AS varchar(12)) + 'rd' 
             else CAST(Position AS varchar(12)) + 'th' 
             end) as [PositionRank] 
FROM (SELECT  DENSE_RANK() OVER (ORDER BY [Total Mark] DESC) AS Position,[Total Mark],StudentID

FROM (SELECT  SUM(ISNULL(WeeklyExam.MarksObtained,0)) AS [Total Mark], WeeklyExam.StudentID
FROM            WeeklyExam INNER JOIN
                         StudentsClass ON WeeklyExam.StudentClassID = StudentsClass.StudentClassID
WHERE        (WeeklyExam.ClassID = @ClassID) AND (DATEPART(WEEK, WeeklyExam.ExamDate) = @ExamWeek) AND (WeeklyExam.ExamID = @ExamID) AND 
                         (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND 
                         (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID)
GROUP BY WeeklyExam.StudentID) as RankTable) as StudentRank)as WeekRank 

ON StudentsClass.StudentID = WeekRank.StudentID

WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE  @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) 
AND (StudentsClass.ShiftID LIKE  @ShiftID) AND (Student.Status = 'Active') order by StudentsClass.RollNo">
                  <SelectParameters>
                     <asp:ControlParameter ControlID="WeekDropDownList" Name="ExamWeek" PropertyName="SelectedValue" />
                     <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                     <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                     <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                     <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                     <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                     <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
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



               <%if (StudentsGridView.Rows.Count > 0)
                 { %>
               <br />
               <table>
                  <tr>
                     <td>
                        <asp:Button ID="SmsButton" runat="server" Text="Send SMS" CssClass="btn btn-primary" OnClick="SmsButton_Click" Width="120px" />
                        <asp:Label ID="ErrorLabel" runat="server" CssClass="EroorSummer"></asp:Label>
                     </td>
                  </tr>
                  <tr>
                     <td><a href="#" id="Print" onclick="print_page()">Print</a></td>
                  </tr>
               </table>
               <%} %>
            </div>
            <!--AllStudents-->
         </div>

      </ContentTemplate>
   </asp:UpdatePanel>

   <asp:UpdateProgress ID="UpdateProgress" runat="server">
      <ProgressTemplate>
         <div id="progress_BG"></div>
         <div id="progress">
            <img src="../../CSS/loading.gif" alt="Loading..." />
            <br />
            <b>Loading...</b></div>
      </ProgressTemplate>
   </asp:UpdateProgress>

    <script type="text/javascript">
       function print_page() {
          document.title = "";
          window.print();
       }
       $("[id*=SelectAllCheckBox]").live("click", function () {
          var a = $(this), b = $(this).closest("table");
          $("input[type=checkbox]", b).each(function () {
             a.is(":checked") ? ($(this).attr("checked", "checked"), $("td", $(this).closest("tr")).addClass("selected")) : ($(this).removeAttr("checked"), $("td", $(this).closest("tr")).removeClass("selected"));
          });
       });
       $("[id*=SMSCheckBox]").live("click", function () {
          var a = $(this).closest("table"), b = $("[id*=chkHeader]", a);
          $(this).is(":checked") ? ($("td", $(this).closest("tr")).addClass("selected"), $("[id*=chkRow]", a).length == $("[id*=chkRow]:checked", a).length && b.attr("checked", "checked")) : ($("td", $(this).closest("tr")).removeClass("selected"), b.removeAttr("checked"));
       });
    </script>
</asp:Content>
