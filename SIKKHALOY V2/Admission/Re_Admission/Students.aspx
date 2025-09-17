<%@ Page Title="Single Re-Admission" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Students.aspx.cs" Inherits="EDUCATION.COM.Admission.Re_Admission.Students" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
      <ContentTemplate>
         <h3>Re-Admission</h3>

          <div class="form-inline NoPrint">
              <div class="form-group">
                  <asp:DropDownList ID="SessionYearDropDownList" runat="server" CssClass="form-control" DataSourceID="Edu_YearSQL" DataTextField="EducationYear" DataValueField="EducationYearID" AppendDataBoundItems="True" AutoPostBack="True">
                      <asp:ListItem Value="0">[ SELECT SESSION ]</asp:ListItem>
                  </asp:DropDownList>
                  <asp:SqlDataSource ID="Edu_YearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYearID, SchoolID, RegistrationID, EducationYear, Status FROM Education_Year WHERE (SchoolID = @SchoolID)">
                      <SelectParameters>
                          <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                      </SelectParameters>
                  </asp:SqlDataSource>
              </div>

              <div class="form-group mx-1">
                  <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                      <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                  </asp:DropDownList>
                  <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                      SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                      <SelectParameters>
                          <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                      </SelectParameters>
                  </asp:SqlDataSource>
              </div>

              <div class="form-group mx-1">
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

              <div class="form-group mx-1">
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

              <div class="form-group mx-1">
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
          </div>

          <div class="table-responsive">
              <asp:GridView ID="StudentsGridView" runat="server" AlternatingRowStyle-CssClass="alt" AutoGenerateColumns="False"
                  DataKeyNames="StudentClassID,StudentID,EducationYearID" ToolTip="Click here for next step"
                  PagerStyle-CssClass="pgr" CssClass="mGrid" AllowSorting="True" DataSourceID="ShowStudentClassSQL">
                  <AlternatingRowStyle CssClass="alt" />
                  <Columns>
                      <asp:HyperLinkField DataNavigateUrlFields="StudentID,EducationYearID,StudentClassID"
                          DataNavigateUrlFormatString="Assign_Class_And_Subjects.aspx?Student={0}&Year={1}&StudentClass={2}"
                          DataTextField="StudentsName" HeaderText="Select Name" />

                      <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                      <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                      <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                      <asp:BoundField DataField="FatherPhoneNumber" HeaderText="F.Mobile" SortExpression="FatherPhoneNumber" />
                      <asp:BoundField DataField="MotherPhoneNumber" HeaderText="M.Mobile" SortExpression="MotherPhoneNumber" />
                      <asp:BoundField DataField="GuardianPhoneNumber" HeaderText="G.Mobile" SortExpression="GuardianPhoneNumber" />
                  </Columns>

                  <EmptyDataTemplate>
                      Empty
                  </EmptyDataTemplate>

                  <PagerStyle CssClass="pgr" />
                  <SelectedRowStyle CssClass="Selected" />
              </asp:GridView>
              <asp:SqlDataSource ID="ShowStudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                  SelectCommand="SELECT        Student.StudentID, Student.SMSPhoneNo, Student.StudentsName, Student.Gender, Student.StudentsLocalAddress, Student.MothersName, Student.FathersName, Student.FatherPhoneNumber, 
                         Student.GuardianName, StudentsClass.RollNo, Student.ID, Student.MotherPhoneNumber, Student.FatherOccupation, Student.GuardianPhoneNumber, StudentsClass.StudentClassID, 
                         StudentsClass.EducationYearID
FROM            StudentsClass INNER JOIN
                         Student ON StudentsClass.StudentID = Student.StudentID
WHERE        (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.SubjectGroupID LIKE @SubjectGroupID) AND (StudentsClass.ShiftID LIKE @ShiftID) AND 
                         (Student.Status = @Status) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.Class_Status IS NULL)
ORDER BY CASE WHEN ISNUMERIC(StudentsClass.RollNo) = 1 THEN CAST(REPLACE(REPLACE(StudentsClass.RollNo, '$', ''), ',', '') AS INT) ELSE 0 END">
                  <SelectParameters>
                      <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                      <asp:ControlParameter ControlID="SessionYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" />
                      <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                      <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                      <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                      <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                      <asp:Parameter DefaultValue="Active" Name="Status" />
                  </SelectParameters>
              </asp:SqlDataSource>
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
</asp:Content>
