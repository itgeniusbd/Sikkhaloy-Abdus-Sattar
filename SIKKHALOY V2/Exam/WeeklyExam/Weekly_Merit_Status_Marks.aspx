<%@ Page Title="Input Weekly Merit Status marks" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Weekly_Merit_Status_Marks.aspx.cs" Inherits="EDUCATION.COM.EXAM.WeeklyExam.Weekly_Merit_Status_Marks" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="Css/Check-Weekly-Results.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
      <ContentTemplate>
         <div class="Contain">
            <!--Contain-->
            <h3>Students Weekly Merit Status Marks<asp:Label ID="DateLabel" runat="server" /></h3>

            <asp:MultiView ID="MultiView1" runat="server">
               <asp:View ID="View1" runat="server">
                  <p>Select Date To Input Weekly Merit Status marks:</p>
                  <asp:Calendar ID="ExamDateCalendar" runat="server" BackColor="White" BorderColor="White" BorderWidth="1px" FirstDayOfWeek="Saturday" Font-Names="Verdana" Font-Size="9pt" ForeColor="Black" Height="190px" NextPrevFormat="FullMonth" OnSelectionChanged="ExamDateCalendar_SelectionChanged" Width="100%">
                     <DayHeaderStyle Font-Bold="True" Font-Size="8pt" />
                     <NextPrevStyle Font-Bold="True" Font-Size="8pt" ForeColor="#333333" VerticalAlign="Bottom" />
                     <OtherMonthDayStyle ForeColor="#999999" />
                     <SelectedDayStyle BackColor="#333399" ForeColor="White" />
                     <TitleStyle BackColor="White" Font-Bold="True" Font-Size="12pt" ForeColor="#333399" BorderColor="Black" BorderWidth="4px" />
                     <TodayDayStyle BackColor="#CCCCCC" />
                  </asp:Calendar>
               </asp:View>

               <asp:View ID="View2" runat="server">
                  <div class="Category">
                     <table>
                        <tr>
                           <td>Select Exam </td>
                           <td>
                              <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorStar" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1">*</asp:RequiredFieldValidator>
                           </td>
                        </tr>
                        <tr>
                           <td>
                              <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" Width="147px">
                              </asp:DropDownList>
                           </td>
                           <td>&nbsp;</td>
                        </tr>
                     </table>

                     <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT ExamID, RegistrationID, ExamName, Date, SchoolID FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                        <SelectParameters>
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>

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
                        <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" Width="150px">
                           <asp:ListItem Value="0">-Select-</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                           <SelectParameters>
                              <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
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
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                     <asp:SqlDataSource ID="Group" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.SubjectGroupID = @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                          <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                     <asp:SqlDataSource ID="Section" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.SectionID = @SectionID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                          <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                     <asp:SqlDataSource ID="Shift" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.ShiftID = @ShiftID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                          <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                        </SelectParameters>
                     </asp:SqlDataSource>

                     <br />
                     <asp:LinkButton ID="BackLinkButton" runat="server" OnClick="BackLinkButton_Click"> << Back To Date</asp:LinkButton><br />


                  </div>
                  <!--End Category-->

                  <div id="AllStudents">

                     <asp:GridView ID="StudentsGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False" AllowPaging="True" PageSize="60"
                        DataKeyNames="StudentClassID,EducationYearID,StudentID" OnRowDataBound="StudentsGridView_RowDataBound" CssClass="mGrid" DataSourceID="ShowStudentClassSQL" AllowSorting="True">
                        <AlternatingRowStyle CssClass="alt" />

                        <Columns>
                           <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                           <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" />
                           <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo">
                              <ItemStyle HorizontalAlign="Center" Width="80px" />
                           </asp:BoundField>
                           <asp:TemplateField HeaderText="Marks">
                              <ItemTemplate>
                                 <asp:TextBox ID="MarksTextBox" runat="server" CssClass="form-control tb" onkeypress="return isNumberKey(event)"></asp:TextBox>
                              </ItemTemplate>
                              <ItemStyle Width="50px" />
                           </asp:TemplateField>
                           <asp:TemplateField HeaderText="Absence">
                              <ItemTemplate>
                                 <asp:CheckBox ID="AbsenceCheckBox" runat="server" Text=" " />
                              </ItemTemplate>
                              <HeaderStyle HorizontalAlign="Center" />
                              <ItemStyle HorizontalAlign="Center" Width="80px" />
                           </asp:TemplateField>
                        </Columns>

                        <EmptyDataTemplate>
                           Empty
                        </EmptyDataTemplate>

                        <PagerStyle CssClass="pgr" />
                        <SelectedRowStyle CssClass="Selected" />
                     </asp:GridView>

                     <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT Student.StudentID, Student.StudentImageID, Student.ID, Student.StudentsName, Student.Gender, StudentsClass.RollNo, StudentsClass.StudentClassID, StudentsClass.ClassID, StudentsClass.SectionID, StudentsClass.ShiftID, StudentsClass.RollNo, StudentsClass.Date, StudentsClass.SubjectGroupID, StudentsClass.EducationYearID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) ORDER BY StudentsClass.RollNo">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>

                     <asp:SqlDataSource ID="WeeklyExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        InsertCommand="INSERT INTO WeeklyExam(SchoolID, RegistrationID, StudentID, ClassID, EducationYearID, StudentClassID, ExamID, MarksObtained, ExamDate, Date) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @EducationYearID, @StudentClassID, @ExamID, @MarksObtained, @ExamDate, GETDATE())"
                        SelectCommand="SELECT WeeklyExamID, SchoolID, RegistrationID, StudentID, ClassID, ExamID, StudentClassID, MarksObtained, ExamDate, EducationYearID, Date FROM WeeklyExam WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                        <InsertParameters>
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                           <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                           <asp:SessionParameter Name="StudentID" SessionField="StudentID" Type="Int32" />
                           <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                           <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" Type="Int32" />
                           <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                           <asp:SessionParameter Name="MarksObtained" SessionField="MarksObtained" Type="Double" />
                           <asp:ControlParameter ControlID="ExamDateCalendar" DbType="Date" Name="ExamDate" PropertyName="SelectedDate" />
                        </InsertParameters>
                        <SelectParameters>
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>

                     <%if (StudentsGridView.Rows.Count > 0)
                       { %>
                     <br />
                     <asp:Button ID="SubmitButton" runat="server" OnClick="SubmitButton_Click" Text="Submit" CssClass="btn btn-primary"
                        OnClientClick="this.disabled = true; this.value = 'Submitting...';" UseSubmitBehavior="false" />
                     <%} %>
                  </div>
                  <!--AllStudents-->
               </asp:View>
            </asp:MultiView>

         </div>
         <!--End Contain-->

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
   <script>
      function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
      $(document).ready(function () {
         Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $(":checkbox").change(function () {
               $(this).parents("tr:eq(0)").find(".tb").prop("disabled", this.checked).val("");
            });
         });
      });
      function DisableButton() { document.getElementById("<%=SubmitButton.ClientID %>").disabled = true; }
        window.onbeforeunload = DisableButton;
    </script>
</asp:Content>
