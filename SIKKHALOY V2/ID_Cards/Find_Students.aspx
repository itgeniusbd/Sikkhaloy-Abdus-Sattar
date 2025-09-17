<%@ Page Title="Find Students" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Find_Students.aspx.cs" Inherits="EDUCATION.COM.ID_CARDS.Find_Students" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Students Records</h3>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="form-inline d-print-none">
                <div class="form-group">
                    <asp:DropDownList ID="OldNewDropDownList" CssClass="form-control" runat="server">
                        <asp:ListItem Value="%">All Student</asp:ListItem>
                        <asp:ListItem Value="1">New Student</asp:ListItem>
                        <asp:ListItem Value="0">Old Student</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="form-group">
                    <asp:TextBox ID="SearchTextBox" runat="server" CssClass="form-control" placeholder="Enter search keywords here"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" OnClick="FindButton_Click" Text="Find" />
                </div>
            </div>

            <div class=" alert alert-info">
                <asp:Label ID="StudentCountLabel" runat="server"></asp:Label>
            </div>

            <div class="table-responsive">
                <asp:GridView ID="StudentGridView" runat="server" AutoGenerateColumns="False" DataSourceID="AllStudentSQL"
                    CssClass="mGrid" AllowPaging="True" PageSize="70" AllowSorting="True">
                    <Columns>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" />
                        <asp:HyperLinkField Target="_blank" DataNavigateUrlFields="StudentID,StudentClassID" DataTextField="StudentsName" HeaderText="Student's Name"
                            DataNavigateUrlFormatString="/Admission/Student_Report/Report.aspx?Student={0}&Student_Class={1}" />
                        <asp:BoundField DataField="FathersName" HeaderText="Father Name" SortExpression="FathersName" />
                        <asp:BoundField DataField="MothersName" HeaderText="Mother Name" SortExpression="MothersName" />
                        <asp:BoundField DataField="BloodGroup" HeaderText="Blood Group" SortExpression="BloodGroup" />
                        <asp:BoundField DataField="RollNo" HeaderText="Roll" SortExpression="RollNo" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />
                        <asp:BoundField DataField="SMSPhoneNo" HeaderText="SMS Phone" SortExpression="SMSPhoneNo" />
                        <asp:BoundField DataField="Religion" HeaderText="Religion" SortExpression="Religion" />
                        <asp:BoundField DataField="Class" HeaderText="Class" SortExpression="Class" />
                        <asp:BoundField DataField="SubjectGroup" HeaderText="Group" SortExpression="SubjectGroup" ReadOnly="True" />
                        <asp:BoundField DataField="Section" HeaderText="Section" SortExpression="Section" />
                        <asp:BoundField DataField="Shift" HeaderText="Shift" SortExpression="Shift" />
                        <asp:BoundField DataField="AdmissionDate" DataFormatString="{0:d MMM yyyy}" HeaderText="Admission Date" SortExpression="AdmissionDate" />
                        <asp:TemplateField HeaderText="Image">
                            <ItemTemplate>
                                <img src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" style="width: 35px;" />
                            </ItemTemplate>
                            <ItemStyle Width="35px" />
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pgr" />
                    <EmptyDataTemplate>
                        No result found!
                    </EmptyDataTemplate>
                </asp:GridView>

                <asp:SqlDataSource ID="AllStudentSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT Student.StudentsName, Student.ID, Student.FathersName,Student.MothersName,Student.BloodGroup, CreateSection.Section, CreateClass.Class, CreateShift.Shift, CreateSubjectGroup.SubjectGroup, StudentsClass.RollNo, Student.Gender, Student.Religion, Student.StudentsLocalAddress, Student.PrevClass, Student.SMSPhoneNo, Student.AdmissionDate, StudentsClass.StudentClassID, StudentsClass.StudentID, Student.StudentImageID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE (Student.Status = 'Active') AND (Student.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.Is_New LIKE @Is_New)"
                    FilterExpression="StudentsName LIKE '{0}%' or ID LIKE '{0}%' or FathersName LIKE '{0}%' or BloodGroup LIKE '{0}%' or Section LIKE '{0}%' or Class LIKE '{0}%' or Shift LIKE '{0}%' or SubjectGroup LIKE '{0}%' or RollNo LIKE '{0}%' or Gender LIKE '{0}%' or Religion LIKE '{0}%' or StudentsLocalAddress LIKE '{0}%' or SMSPhoneNo LIKE '{0}%'" CancelSelectOnNullParameter="False">
                    <FilterParameters>
                        <asp:ControlParameter ControlID="SearchTextBox" Name="Find" PropertyName="Text" />
                    </FilterParameters>
                    <SelectParameters>
                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        <asp:ControlParameter ControlID="OldNewDropDownList" Name="Is_New" PropertyName="SelectedValue" />
                    </SelectParameters>
                </asp:SqlDataSource>
            </div>
        </ContentTemplate>
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
</asp:Content>
