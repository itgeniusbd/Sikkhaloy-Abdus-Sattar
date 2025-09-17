<%@ Page Title="Edit Weekly Merit Status" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Edit_Weekly_Merit_Status_Marks.aspx.cs" Inherits="EDUCATION.COM.EXAM.WeeklyExam.Edit_Weekly_Merit_Status_Marks" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="CSS/Check-Weekly-Results.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
      <ContentTemplate>
         <div class="Contain">
            <!--Contain-->
            <h3>Edit Weekly Merit Status Marks: </h3>
            <asp:MultiView ID="MultiView1" runat="server">
               <asp:View ID="View1" runat="server">
                  <p>Select exam date to edit marks</p>
                  <asp:Calendar ID="Calendar" runat="server" BackColor="White" BorderColor="White" BorderWidth="1px" Font-Names="Verdana" Font-Size="9pt" ForeColor="Black" Height="190px" NextPrevFormat="FullMonth" Width="100%" OnSelectionChanged="Calendar_SelectionChanged">
                     <DayHeaderStyle Font-Bold="True" Font-Size="8pt" />
                     <NextPrevStyle Font-Bold="True" Font-Size="8pt" ForeColor="#333333" VerticalAlign="Bottom" />
                     <OtherMonthDayStyle ForeColor="#999999" />
                     <SelectedDayStyle BackColor="#333399" ForeColor="White" />
                     <TitleStyle BackColor="White" BorderColor="Black" BorderWidth="4px" Font-Bold="True" Font-Size="12pt" ForeColor="#333399" />
                     <TodayDayStyle BackColor="#CCCCCC" />
                  </asp:Calendar>
               </asp:View>

               <asp:View ID="View2" runat="server">
                  <div class="Category">
                     <table>
                        <tr>
                           <td>Select Exam </td>
                           <td>&nbsp;</td>
                        </tr>
                        <tr>
                           <td>
                              <asp:DropDownList ID="ExamDropDownList" runat="server" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" Width="147px" AutoPostBack="True" AppendDataBoundItems="True">
                                 <asp:ListItem Value="0">-- Select --</asp:ListItem>
                              </asp:DropDownList>

                              <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                 SelectCommand="SELECT ExamID, RegistrationID, ExamName, Date, SchoolID FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
                                 <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                                 </SelectParameters>
                              </asp:SqlDataSource>
                           </td>
                           <td>&nbsp;</td>
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
                        <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged" Width="150px">
                           <asp:ListItem Value="0">-Select-</asp:ListItem>
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                           SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
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
                        <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                           SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
                           <SelectParameters>
                              <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                              <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                              <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
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
                        <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                           SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
                           <SelectParameters>
                              <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                              <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                              <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
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
                        <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                           SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
                           <SelectParameters>
                              <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                              <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                              <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                           </SelectParameters>
                        </asp:SqlDataSource>
                     </div>

                     <asp:SqlDataSource ID="Class" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.ClassID = @ClassID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                     <asp:SqlDataSource ID="Group" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.SubjectGroupID = @SubjectGroupID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                       <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                     <asp:SqlDataSource ID="Section" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.SectionID = @SectionID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                     <asp:SqlDataSource ID="Shift" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Student.StudentID) AS Total FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID WHERE (StudentsClass.ShiftID = @ShiftID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (Student.Status = @Status)">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                           <asp:Parameter DefaultValue="Active" Name="Status" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                     <br />
                     <asp:LinkButton ID="BackLinkButton" runat="server" OnClick="BackLinkButton_Click"> << Back To Date</asp:LinkButton>
                  </div>
                  <!--End Category-->

                  <div id="AllStudents">

                     <asp:GridView ID="StudentsGridView" runat="server" AutoGenerateColumns="False" PageSize="25" DataSourceID="ShowStudentClassSQL" AllowSorting="True"
                        DataKeyNames="WeeklyExamID" CssClass="mGrid">
                        <AlternatingRowStyle CssClass="alt" />
                        <Columns>
                           <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" ReadOnly="True" />
                           <asp:BoundField DataField="StudentsName" HeaderText="Name" SortExpression="StudentsName" ReadOnly="True" />
                           <asp:BoundField DataField="RollNo" HeaderText="Roll No" SortExpression="RollNo" ReadOnly="True"></asp:BoundField>
                           <asp:TemplateField HeaderText="Obt.Marks" SortExpression="MarksObtained">
                              <EditItemTemplate>
                                 <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" Text='<%# Bind("MarksObtained") %>' Width="80px" onkeypress="return isNumberKey(event)"></asp:TextBox>
                              </EditItemTemplate>
                              <ItemTemplate>
                                 <asp:Label ID="Label1" runat="server" Text='<%# Bind("MarksObtained") %>'></asp:Label>
                              </ItemTemplate>
                           </asp:TemplateField>
                           <asp:BoundField DataField="ExamDate" HeaderText="Exam Date" SortExpression="ExamDate" DataFormatString="{0:d MMM yyyy}" InsertVisible="False" ReadOnly="True"></asp:BoundField>
                           <asp:CommandField ShowEditButton="True" />
                           <asp:TemplateField ShowHeader="False">
                              <ItemTemplate>
                                 <asp:LinkButton ID="DeleteLinkButton" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are You sure want delete?')"></asp:LinkButton>
                              </ItemTemplate>
                           </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                           Empty
                        </EmptyDataTemplate>

                     </asp:GridView>

                     <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                        SelectCommand="SELECT WeeklyExam.MarksObtained, StudentsClass.RollNo, Student.ID, Student.StudentsName, WeeklyExam.WeeklyExamID, WeeklyExam.ExamDate FROM WeeklyExam INNER JOIN StudentsClass ON WeeklyExam.StudentClassID = StudentsClass.StudentClassID INNER JOIN Student ON WeeklyExam.StudentID = Student.StudentID WHERE (Student.Status = 'Active') AND (WeeklyExam.ExamID = @ExamID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND (WeeklyExam.ExamDate = @ExamDate) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) ORDER BY StudentsClass.RollNo"
                        UpdateCommand="UPDATE WeeklyExam SET MarksObtained = @MarksObtained WHERE (WeeklyExamID = @WeeklyExamID)" DeleteCommand="DELETE FROM WeeklyExam WHERE (WeeklyExamID = @WeeklyExamID)">
                        <DeleteParameters>
                           <asp:Parameter Name="WeeklyExamID" />
                        </DeleteParameters>
                        <SelectParameters>
                           <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="Calendar" Name="ExamDate" PropertyName="SelectedDate" />
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                        <UpdateParameters>
                           <asp:Parameter Name="MarksObtained" Type="Double" />
                           <asp:Parameter Name="WeeklyExamID" Type="Int32" />
                        </UpdateParameters>
                     </asp:SqlDataSource>

                  </div>
                  <!--AllStudents-->
               </asp:View>
            </asp:MultiView>
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
      function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
   </script>
</asp:Content>
