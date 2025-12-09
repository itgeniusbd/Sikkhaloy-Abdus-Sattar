<%@ Page Title="Admission Form" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Admission_Form.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Admission_Form" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Admission_Form.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <!-- Student ID Search Section -->
    <div class="d-print-none" style="margin-bottom: 20px; padding: 15px; background-color: #f8f9fa; border-radius: 5px;">
        <h5>Search Student by ID</h5>
        <div style="display: flex; gap: 10px; align-items: center;">
            <asp:TextBox ID="StudentIDTextBox" runat="server" placeholder="Enter Student ID" CssClass="form-control" style="width: 250px;"></asp:TextBox>
            <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="SearchButton_Click" />
            <asp:Label ID="MessageLabel" runat="server" CssClass="text-danger"></asp:Label>
        </div>
    </div>

    <div class="print-container">
        <!-- Action Buttons (Print/Edit) -->
        <asp:FormView ID="FormView1" runat="server" DataSourceID="FormSQL" Width="100%">
   <ItemTemplate>
           <div class="action-buttons d-print-none">
    <a class="btn btn-blue" href="../Edit_Student_Info/Edit_Student_information.aspx?Student=<%# Eval("StudentID") %>&Student_Class=<%# Eval("StudentClassID") %>">
             Update Info
          </a>
         <input class="btn btn-blue" onclick="window.print();" type="button" value="Print" />
          <a class="btn btn-blue" href="Admission_New_Student.aspx">New Admission</a>
   </div>

            <!-- Form Header with Photo and Date on both sides -->
  <div class="form-header">
 <!-- Left: Admission Date -->
        <div class="admission-date">
   Admission<br/><%# Eval("AdmissionDate","{0:dd/MM/yyyy}") %>
  </div>
              
         <!-- Center: School Info and Title -->
                <div class="header-center">
  <div class="form-title">ADMISSION FORM</div>
   </div>
        
               <!-- Right: Student Photo -->
           <div class="student-photo">
    <img src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail" alt="Student Photo" />
       </div>
                </div>
          </ItemTemplate>
    </asp:FormView>

        <!-- Content Wrapper -->
        <div class="content-wrapper">
         <!-- Form Content -->
   <asp:FormView ID="FormFormView" runat="server" DataSourceID="FormSQL" Width="100%">
                <ItemTemplate>
       <!-- Student Information -->
   <fieldset>
 <legend>Student Information</legend>
  <table class="info-table">
      <tr>
   <td style="width: 25%;"><span class="label">ID:</span> <span class="value"><%# Eval("ID") %></span></td>
<td style="width: 45%;"><span class="label">Student Name:</span> <span class="value"><%# Eval("StudentsName") %></span></td>
             <td style="width: 30%;"><span class="label">Mobile:</span> <span class="value"><%# Eval("SMSPhoneNo") %></span></td>
             </tr>
         <tr>
    <td><span class="label">Gender:</span> <span class="value"><%# Eval("Gender") %></span></td>
        <td><span class="label">Date of Birth:</span> <span class="value"><%# Eval("DateofBirth","{0:dd/MM/yyyy}") %></span></td>
    <td><span class="label">Blood Group:</span> <span class="value"><%# Eval("BloodGroup") %></span></td>
          </tr>
         <tr>
          <td colspan="3"><span class="label">Religion:</span> <span class="value"><%# Eval("Religion") %></span></td>
       </tr>
 <tr>
    <td colspan="3"><span class="label">Permanent Address:</span> <span class="value"><%# Eval("StudentPermanentAddress") %></span></td>
       </tr>
         <tr>
              <td colspan="3"><span class="label">Present Address:</span> <span class="value"><%# Eval("StudentsLocalAddress") %></span></td>
         </tr>
      </table>
          </fieldset>

 <!-- Parents Information -->
      <fieldset>
       <legend>Parents Information</legend>
   <table class="info-table">
     <tr>
             <td style="width: 40%;"><span class="label">Father's Name:</span> <span class="value"><%# Eval("FathersName") %></span></td>
 <td style="width: 30%;"><span class="label">Mobile:</span> <span class="value"><%# Eval("FatherPhoneNumber") %></span></td>
           <td style="width: 30%;"><span class="label">Occupation:</span> <span class="value"><%# Eval("FatherOccupation") %></span></td>
  </tr>
              <tr>
       <td><span class="label">Mother's Name:</span> <span class="value"><%# Eval("MothersName") %></span></td>
 <td><span class="label">Mobile:</span> <span class="value"><%# Eval("MotherPhoneNumber") %></span></td>
           <td><span class="label">Occupation:</span> <span class="value"><%# Eval("MotherOccupation") %></span></td>
        </tr>
         </table>
   </fieldset>

               <!-- Guardian Information -->
         <fieldset>
             <legend>Guardian Information</legend>
 <table class="info-table">
        <tr>
          <td style="width: 40%;"><span class="label">Guardian Name:</span> <span class="value"><%# Eval("GuardianName") %></span></td>
  <td style="width: 30%;"><span class="label">Relationship:</span> <span class="value"><%# Eval("GuardianRelationshipwithStudent") %></span></td>
                <td style="width: 30%;"><span class="label">Mobile:</span> <span class="value"><%# Eval("GuardianPhoneNumber") %></span></td>
     </tr>
            </table>
        </fieldset>

   <!-- Previous School Information -->
   <fieldset>
        <legend>Previous Institution Information</legend>
      <table class="info-table">
            <tr>
                <td style="width: 40%;"><span class="label">Institution Name:</span> <span class="value"><%# Eval("PrevSchoolName") %></span></td>
       <td style="width: 20%;"><span class="label">Class:</span> <span class="value"><%# Eval("PrevClass") %></span></td>
        <td style="width: 20%;"><span class="label">Year:</span> <span class="value"><%# Eval("PrevExamYear") %></span></td>
   <td style="width: 20%;"><span class="label">Grade:</span> <span class="value"><%# Eval("PrevExamGrade") %></span></td>
          </tr>
          </table>
        </fieldset>

   <!-- Institutional Information -->
                    <fieldset>
   <legend>Academic Information</legend>
            <table class="info-table">
              <tr>
          <td style="width: 20%;"><span class="label">Class:</span> <span class="value"><%# Eval("Class") %></span></td>
      <td style="width: 20%;"><span class="label">Roll No:</span> <span class="value"><%# Eval("RollNo") %></span></td>
    <td style="width: 20%;"><span class="label">Section:</span> <span class="value"><%# Eval("Section") %></span></td>
          <td style="width: 20%;"><span class="label">Shift:</span> <span class="value"><%# Eval("Shift") %></span></td>
                  <td style="width: 20%;"><span class="label">Group:</span> <span class="value"><%# Eval("SubjectGroup") %></span></td>
               </tr>
            </table>
          </fieldset>
         </ItemTemplate>
       </asp:FormView>
        </div>

        <!-- Signature Section -->
        <div class="signature-section" id="signatureSection" style="display: none;">
            <div class="signature-box">
                <div class="signature-line">Student's Signature</div>
                <div class="signature-date">Date: _______</div>
            </div>
    <div class="signature-box">
         <div class="signature-line">Guardian's Signature</div>
   <div class="signature-date">Date: _______</div>
            </div>
   <div class="signature-box">
    <div class="signature-line">Principal's Signature</div>
                <div class="signature-date">Date: _______</div>
   </div>
        </div>
    </div>

<asp:SqlDataSource ID="FormSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, Student.FathersName, CreateClass.Class, StudentsClass.RollNo, CreateSection.Section, CreateSubjectGroup.SubjectGroup, CreateShift.Shift, Student.SMSPhoneNo, Student.StudentImageID, Student.StudentID, Student.SchoolID, Student.StudentEmailAddress, Student.DateofBirth, Student.BloodGroup, Student.Religion, Student.Gender, Student.StudentPermanentAddress, Student.StudentsLocalAddress, Student.PrevSchoolName, Student.PrevClass, Student.PrevExamYear, Student.PrevExamGrade, Student.MothersName, Student.MotherOccupation, Student.MotherPhoneNumber, Student.FatherOccupation, Student.FatherPhoneNumber, Student.GuardianName, Student.GuardianRelationshipwithStudent, Student.GuardianPhoneNumber, Student.OtherDetails, Student.AdmissionDate, StudentsClass.ClassID, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.SchoolID = @SchoolID) AND (Student.StudentID = @StudentID) AND (StudentsClass.StudentClassID = @StudentClassID)">
        <SelectParameters>
     <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
  <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>