<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Form_Bangla.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Form_Bangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <link href="CSS/FormBangla.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="print-container">
        <!-- Decorative Corners -->
        <div class="corner-decoration top-left d-print-none"></div>
        <div class="corner-decoration top-right d-print-none"></div>
        <div class="corner-decoration bottom-left d-print-none"></div>
        <div class="corner-decoration bottom-right d-print-none"></div>

        <!-- Watermark -->
        <div class="watermark">ভর্তি ফরম</div>

        <!-- Action Buttons (Print/Edit) -->
        <asp:FormView ID="FormView1" runat="server" DataSourceID="FormSQL" Width="100%">
            <ItemTemplate>
                <div class="action-buttons d-print-none">
                    <a class="btn btn-blue" href="../Edit_Student_Info/Edit_Student_information.aspx?Student=<%# Eval("StudentID") %>&Student_Class=<%# Eval("StudentClassID") %>">
                        <i class="fa fa-pencil-square" aria-hidden="true"></i> তথ্য সংশোধন করুন
                    </a>
                    <input class="btn btn-blue" onclick="window.print();" type="button" value="🖨️ প্রিন্ট করুন" />
                    <a class="btn btn-blue" href="Admission_New_Student.aspx">➕ নতুন ভর্তি</a>
                </div>

                <!-- Form Header with Photo and Date on both sides -->
                <div class="form-header">
                    <!-- Left: Admission Date -->
                    <div class="admission-date">
                        📅 ভর্তি<br/><%# Eval("AdmissionDate","{0:dd/MM/yyyy}") %>
                    </div>
                    
                    <!-- Center: School Info and Title -->
                    <div class="header-center">
                        <div class="form-title">✨ ভর্তি ফরম ✨</div>
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
                        <legend>👤 শিক্ষার্থীর তথ্য</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 25%;"><span class="label">আইডি:</span> <span class="value"><%# Eval("ID") %></span></td>
                                <td style="width: 45%;"><span class="label">শিক্ষার্থীর নাম:</span> <span class="value"><%# Eval("StudentsName") %></span></td>
                                <td style="width: 30%;"><span class="label">📱 মোবাইল:</span> <span class="value"><%# Eval("SMSPhoneNo") %></span></td>
                            </tr>
                            <tr>
                                <td><span class="label">লিঙ্গ:</span> <span class="value"><%# Eval("Gender") %></span></td>
                                <td><span class="label">🎂 জন্ম তারিখ:</span> <span class="value"><%# Eval("DateofBirth","{0:dd/MM/yyyy}") %></span></td>
                                <td><span class="label">🩸 রক্তের গ্রুপ:</span> <span class="value"><%# Eval("BloodGroup") %></span></td>
                            </tr>
                            <tr>
                                <td colspan="3"><span class="label">🕌 ধর্ম:</span> <span class="value"><%# Eval("Religion") %></span></td>
                            </tr>
                            <tr>
                                <td colspan="3"><span class="label">🏠 স্থায়ী ঠিকানা:</span> <span class="value"><%# Eval("StudentPermanentAddress") %></span></td>
                            </tr>
                            <tr>
                                <td colspan="3"><span class="label">📍 বর্তমান ঠিকানা:</span> <span class="value"><%# Eval("StudentsLocalAddress") %></span></td>
                            </tr>
                        </table>
                    </fieldset>

                    <!-- Parents Information -->
                    <fieldset>
                        <legend>👨‍👩‍👦 পিতা-মাতার তথ্য</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 40%;"><span class="label">👨 পিতার নাম:</span> <span class="value"><%# Eval("FathersName") %></span></td>
                                <td style="width: 30%;"><span class="label">📱 মোবাইল:</span> <span class="value"><%# Eval("FatherPhoneNumber") %></span></td>
                                <td style="width: 30%;"><span class="label">💼 পেশা:</span> <span class="value"><%# Eval("FatherOccupation") %></span></td>
                            </tr>
                            <tr>
                                <td><span class="label">👩 মাতার নাম:</span> <span class="value"><%# Eval("MothersName") %></span></td>
                                <td><span class="label">📱 মোবাইল:</span> <span class="value"><%# Eval("MotherPhoneNumber") %></span></td>
                                <td><span class="label">💼 পেশা:</span> <span class="value"><%# Eval("MotherOccupation") %></span></td>
                            </tr>
                        </table>
                    </fieldset>

                    <!-- Guardian Information -->
                    <fieldset>
                        <legend>🤝 অভিভাবকের তথ্য</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 40%;"><span class="label">অভিভাবকের নাম:</span> <span class="value"><%# Eval("GuardianName") %></span></td>
                                <td style="width: 30%;"><span class="label">🔗 সম্পর্ক:</span> <span class="value"><%# Eval("GuardianRelationshipwithStudent") %></span></td>
                                <td style="width: 30%;"><span class="label">📱 মোবাইল:</span> <span class="value"><%# Eval("GuardianPhoneNumber") %></span></td>
                            </tr>
                        </table>
                    </fieldset>

                    <!-- Previous School Information -->
                    <fieldset>
                        <legend>🏫 পূর্বের শিক্ষা প্রতিষ্ঠানের তথ্য</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 40%;"><span class="label">প্রতিষ্ঠানের নাম:</span> <span class="value"><%# Eval("PrevSchoolName") %></span></td>
                                <td style="width: 20%;"><span class="label">শ্রেণি:</span> <span class="value"><%# Eval("PrevClass") %></span></td>
                                <td style="width: 20%;"><span class="label">সাল:</span> <span class="value"><%# Eval("PrevExamYear") %></span></td>
                                <td style="width: 20%;"><span class="label">🏆 গ্রেড:</span> <span class="value"><%# Eval("PrevExamGrade") %></span></td>
                            </tr>
                        </table>
                    </fieldset>

                    <!-- Institutional Information -->
                    <fieldset>
                        <legend>🎓 প্রাতিষ্ঠানিক তথ্য</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 20%;"><span class="label">শ্রেণি:</span> <span class="value"><%# Eval("Class") %></span></td>
                                <td style="width: 20%;"><span class="label">রোল নং:</span> <span class="value"><%# Eval("RollNo") %></span></td>
                                <td style="width: 20%;"><span class="label">শাখা:</span> <span class="value"><%# Eval("Section") %></span></td>
                                <td style="width: 20%;"><span class="label">শিফট:</span> <span class="value"><%# Eval("Shift") %></span></td>
                                <td style="width: 20%;"><span class="label">গ্রুপ:</span> <span class="value"><%# Eval("SubjectGroup") %></span></td>
                            </tr>
                        </table>
                    </fieldset>
                </ItemTemplate>
            </asp:FormView>
        </div>

        <!-- Signature Section - Normal flow, stays on first page -->
        <div class="signature-section">
            <div class="signature-box">
                <div class="signature-line">শিক্ষার্থীর স্বাক্ষর</div>
                <div class="signature-date">তারিখ: _______</div>
            </div>
            <div class="signature-box">
                <div class="signature-line">অভিভাবকের স্বাক্ষর</div>
                <div class="signature-date">তারিখ: _______</div>
            </div>
            <div class="signature-box">
                <div class="signature-line">প্রধান শিক্ষকের স্বাক্ষর</div>
                <div class="signature-date">তারিখ: _______</div>
            </div>
        </div>
    </div>

    <asp:SqlDataSource ID="FormSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, Student.FathersName, CreateClass.Class, StudentsClass.RollNo, CreateSection.Section, CreateSubjectGroup.SubjectGroup, CreateShift.Shift, Student.SMSPhoneNo, Student.StudentImageID, Student.StudentID, Student.SchoolID, Student.StudentEmailAddress, Student.DateofBirth, Student.BloodGroup, Student.Religion, Student.Gender, Student.StudentPermanentAddress, Student.StudentsLocalAddress, Student.PrevSchoolName, Student.PrevClass, Student.PrevExamYear, Student.PrevExamGrade, Student.MothersName, Student.MotherOccupation, Student.MotherPhoneNumber, Student.FatherOccupation, Student.FatherPhoneNumber, Student.GuardianName, Student.GuardianRelationshipwithStudent, Student.GuardianPhoneNumber, Student.OtherDetails, Student.AdmissionDate, StudentsClass.ClassID, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.SchoolID = @SchoolID) AND (Student.StudentID = @StudentID) AND (StudentsClass.StudentClassID = @StudentClassID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:QueryStringParameter Name="StudentClassID" QueryStringField="StudentClass" />
            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
