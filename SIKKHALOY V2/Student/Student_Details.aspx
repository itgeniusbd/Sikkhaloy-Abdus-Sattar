<%@ Page Title="Full Details" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Student_Details.aspx.cs" Inherits="EDUCATION.COM.Student.Student_Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
      <style>
        body { line-height: initial; color: #333; }
        .FormTitle { text-align: center; font-size: 1.3rem; font-weight: bold; padding-bottom: 0.2rem; margin-top: .5rem; }
        .InfoTitle { margin-bottom: 1rem; border-bottom: 1px solid #777; margin-top: 3px; text-transform: uppercase; color: #3e4551; font-size: 1rem; font-weight: 400; }

        .info { float: left; width: 80%; }
            .info table { width: 100%; }
                .info table tr { }
                    .info table tr td:nth-child(1) { width: 25%; }
                    .info table tr td:nth-child(2) { width: 3%; }

        .p-image { float: right; }
            .p-image img { width: 130px; }

        .OTable { width: 100%; }
            .OTable tr { }
                .OTable tr td:nth-child(1) { width: 20%; }
                .OTable tr table tr td:nth-child(2) { width: 3%; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
        <div class="FormTitle">ADMISSION FORM</div>
    <asp:FormView ID="FormFormView" runat="server" DataSourceID="StudentInfoSQL" Width="100%">
        <ItemTemplate>
           <input class="btn btn-blue btn-sm d-print-none" onclick="window.print();" type="button" value="Print" />

            <div class="InfoTitle">Student Information</div>
            <div class="clearfix">
                <div class="info">
                    <table>
                        <tr>
                            <td>Name</td>
                            <td>:</td>
                            <td><%# Eval("StudentsName") %></td>
                        </tr>
                        <tr>
                            <td>Gender</td>
                            <td>:</td>
                            <td><%# Eval("Gender") %></td>
                        </tr>
                        <tr>
                            <td>SMS Mobile No.</td>
                            <td>:</td>
                            <td><%# Eval("SMSPhoneNo") %></td>
                        </tr>
                        <tr>
                            <td>Date of Birth</td>
                            <td>:</td>
                            <td><%# Eval("DateofBirth") %></td>
                        </tr>
                        <tr>
                            <td>Blood Group</td>
                            <td>:</td>
                            <td><%# Eval("BloodGroup") %></td>
                        </tr>
                        <tr>
                            <td>Religion</td>
                            <td>:</td>
                            <td><%# Eval("Religion") %></td>
                        </tr>
                        <tr>
                            <td>Permanent Address</td>
                            <td>:</td>
                            <td><%# Eval("StudentPermanentAddress") %></td>
                        </tr>
                        <tr>
                            <td>Present Address</td>
                            <td>:</td>
                            <td><%# Eval("StudentsLocalAddress") %></td>
                        </tr>
                        <tr>
                            <td>Email</td>
                            <td>:</td>
                            <td><%# Eval("StudentEmailAddress") %></td>
                        </tr>
                        <tr>
                            <td>Admission Date</td>
                            <td>:</td>
                            <td><%# Eval("AdmissionDate","{0:d MMM yyyy}") %></td>
                        </tr>
                    </table>
                </div>

                <div class="p-image">
                    <img src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail" />
                    <strong class="d-block text-center">ID: <%# Eval("ID") %></strong>
                </div>
            </div>

            <div class="InfoTitle my-3">Parents Information</div>
            <table class="OTable">
                <tr>
                    <td>Father's Name</td>
                    <td>:</td>
                    <td><%# Eval("FathersName") %></td>
                </tr>
                <tr>
                    <td>Father's Phone</td>
                    <td>:</td>
                    <td><%# Eval("FatherPhoneNumber") %></td>
                </tr>
                <tr>
                    <td>Father's Occupation</td>
                    <td>:</td>
                    <td><%# Eval("FatherOccupation") %></td>
                </tr>
                <tr>
                    <td>Mother's Name</td>
                    <td>:</td>
                    <td><%# Eval("MothersName") %></td>
                </tr>
                <tr>
                    <td>Mother's Occupation</td>
                    <td>:</td>
                    <td><%# Eval("MotherOccupation") %></td>
                </tr>
                <tr>
                    <td>Mother's Phone</td>
                    <td>:</td>
                    <td><%# Eval("MotherPhoneNumber") %></td>
                </tr>

            </table>

            <div class="InfoTitle my-3">Previous Institution Information</div>
            <table class="OTable">
                <tr>
                    <td>Prev. Institution Name</td>
                    <td>:</td>
                    <td><%# Eval("PrevSchoolName") %></td>
                </tr>
                <tr>
                    <td>Prev. Class</td>
                    <td>:</td>
                    <td><%# Eval("PrevClass") %></td>
                </tr>
                <tr>
                    <td>Prev. Exam Year</td>
                    <td>:</td>
                    <td><%# Eval("PrevExamYear") %></td>
                </tr>
                <tr>
                    <td>Prev. Grade</td>
                    <td>:</td>
                    <td><%# Eval("PrevExamGrade") %></td>
                </tr>
            </table>

            <div class="InfoTitle my-3">Second Guardian Information</div>
            <table class="OTable">
                <tr>
                    <td>Guardian Name</td>
                    <td>:</td>
                    <td><%# Eval("GuardianName") %></td>
                </tr>
                <tr>
                    <td>Relationship with Student</td>
                    <td>:</td>
                    <td><%# Eval("GuardianRelationshipwithStudent") %></td>
                </tr>
                <tr>
                    <td>Guardian Phone</td>
                    <td>:</td>
                    <td><%# Eval("GuardianPhoneNumber") %></td>
                </tr>
                <tr>
                    <td>Other Details</td>
                    <td>:</td>
                    <td><%# Eval("OtherDetails") %></td>
                </tr>
            </table>

            <div class="InfoTitle mt-2">Academic Information</div>
            <table class="OTable">
                <tr>
                    <td>Class</td>
                    <td>:</td>
                    <td><%# Eval("Class") %></td>
                </tr>
                <tr>
                    <td>Roll No</td>
                    <td>:</td>
                    <td><%# Eval("RollNo") %></td>
                </tr>
                <tr>
                    <td>Section</td>
                    <td>:</td>
                    <td><%# Eval("Section") %></td>
                </tr>
                <tr>
                    <td>Shift</td>
                    <td>:</td>
                    <td><%# Eval("Shift") %></td>
                </tr>
                <tr>
                    <td>Group</td>
                    <td>:</td>
                    <td><%# Eval("SubjectGroup") %></td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
       SelectCommand="SELECT Student.ID, Student.StudentsName, Student.FathersName, CreateClass.Class, StudentsClass.RollNo, CreateSection.Section, CreateSubjectGroup.SubjectGroup, CreateShift.Shift, Student.SMSPhoneNo, Student.StudentImageID, Student.StudentID, Student.SchoolID, Student.StudentEmailAddress, Student.DateofBirth, Student.BloodGroup, Student.Religion, Student.Gender, Student.StudentPermanentAddress, Student.StudentsLocalAddress, Student.PrevSchoolName, Student.PrevClass, Student.PrevExamYear, Student.PrevExamGrade, Student.MothersName, Student.MotherOccupation, Student.MotherPhoneNumber, Student.FatherOccupation, Student.FatherPhoneNumber, Student.GuardianName, Student.GuardianRelationshipwithStudent, Student.GuardianPhoneNumber, Student.OtherDetails, Student.AdmissionDate, StudentsClass.ClassID, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.SchoolID = @SchoolID) AND (Student.StudentID = @StudentID) AND (StudentsClass.StudentClassID = @StudentClassID)">
        <SelectParameters>
            <asp:SessionParameter Name="StudentRegistrationID" SessionField="RegistrationID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
            <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $("#_8").addClass("active");
        });
    </script>
</asp:Content>
