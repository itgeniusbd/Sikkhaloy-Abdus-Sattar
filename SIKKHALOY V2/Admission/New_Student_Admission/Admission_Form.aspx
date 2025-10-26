<%@ Page Title="Admission Form" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Admission_Form.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Admission_Form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        * {
            font-family: Arial, sans-serif !important;
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            line-height: 1.4;
            color: #000;
        }

        @page {
            size: A4;
            margin: 8mm 10mm;
        }

        @media print {
            .d-print-none {
                display: none !important;
            }

            body {
                margin: 0 !important;
                padding: 0 !important;
            }

            .print-container {
                padding: 0 !important;
            }

            fieldset {
                page-break-inside: avoid;
                break-inside: avoid;
            }

            .form-header {
                margin-top: 0 !important;
                padding-top: 5px !important;
            }

            .signature-section {
                page-break-inside: avoid;
                margin-top: 15px !important;
            }
        }

        .print-container {
            max-width: 210mm;
            margin: 0 auto;
            padding: 10px;
            background: white;
            position: relative;
        }

        /* Form Header with Photo and Date on sides */
        .form-header {
            position: relative;
            text-align: center;
            border-top: 3px solid #667eea;
            padding-top: 5px;
            margin-top: 0;
            display: flex;
            align-items: center;
            justify-content: space-between;
            min-height: 110px;
        }

        .admission-date {
            width: 90px;
            font-size: 13px;
            font-weight: bold;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 5px 8px;
            border-radius: 6px;
            border: 2px solid #5a67d8;
            box-shadow: 0 2px 6px rgba(0,0,0,0.2);
            text-align: center;
            line-height: 1.3;
        }

        .header-center {
            flex: 1;
            text-align: center;
        }

        .school-logo {
            width: 60px;
            height: 60px;
            margin: 0 auto 5px;
        }

        .school-name {
            font-size: 20px;
            font-weight: 900;
            color: #333;
            margin-bottom: 3px;
        }

        .school-address {
            font-size: 12px;
            color: #666;
            margin-bottom: 5px;
        }

        .form-title {
            font-size: 18px;
            font-weight: 900;
            color: #fff;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            display: inline-block;
            padding: 6px 24px;
            border-radius: 6px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.2);
            border: 2px solid #5a67d8;
        }

        .student-photo {
            width: 90px;
        }

        .img-thumbnail {
            padding: 3px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: 2px solid #5a67d8;
            border-radius: 6px;
            width: 90px;
            height: 110px;
            object-fit: cover;
            box-shadow: 0 3px 8px rgba(0,0,0,0.2);
            display: block;
        }

        /* Action Buttons */
        .action-buttons {
            text-align: center;
            margin-bottom: 10px;
        }

        .btn {
            padding: 8px 16px;
            margin: 3px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 14px;
            font-weight: bold;
            text-decoration: none;
            display: inline-block;
            transition: all 0.3s ease;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }

        .btn-blue {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }

        .btn-blue:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0,0,0,0.3);
        }

        /* Content Wrapper */
        .content-wrapper {
            margin-bottom: 15px;
        }

        /* Fieldset Styles - LARGER */
        fieldset {
            border: 2px solid #667eea;
            border-radius: 8px;
            padding: 10px 12px;
            margin-bottom: 10px;
            background: linear-gradient(to right, #f8f9ff 0%, #ffffff 100%);
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        legend {
            font-size: 17px;
            font-weight: 900;
            padding: 3px 12px;
            color: #fff;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 5px;
            border: 2px solid #5a67d8;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }

        /* Table Styles - LARGER */
        .info-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 15px;
        }

        .info-table td {
            padding: 6px 8px;
            vertical-align: top;
        }

        .label {
            font-weight: 700;
            color: #5a67d8;
            display: inline-block;
            font-size: 15px;
        }

        .value {
            color: #1a1a1a;
            font-weight: 600;
            font-size: 14px;
        }

        /* Signature Section - Normal flow, stays on first page */
        .signature-section {
            display: flex;
            justify-content: space-between;
            padding: 10px 5px;
            margin-top: 20px;
    
           
        }

        .signature-box {
            text-align: center;
            flex: 1;
        }

        .signature-line {
            border-top: 2px solid #000;
            width: 145px;
            margin: 35px auto 5px;
            padding-top: 5px;
            font-weight: 700;
            font-size: 14px;
            color: #333;
        }

        .signature-date {
            font-size: 11px;
            color: #666;
            margin-top: 3px;
        }

        /* Decorative Elements */
        .corner-decoration {
            position: absolute;
            width: 35px;
            height: 35px;
            border: 2px solid #667eea;
        }

        .corner-decoration.top-left {
            top: 0;
            left: 0;
            border-right: none;
            border-bottom: none;
            border-radius: 6px 0 0 0;
        }

        .corner-decoration.top-right {
            top: 0;
            right: 0;
            border-left: none;
            border-bottom: none;
            border-radius: 0 6px 0 0;
        }

        .corner-decoration.bottom-left {
            bottom: 0;
            left: 0;
            border-right: none;
            border-top: none;
            border-radius: 0 0 0 6px;
        }

        .corner-decoration.bottom-right {
            bottom: 0;
            right: 0;
            border-left: none;
            border-top: none;
            border-radius: 0 0 6px 0;
        }

        /* Print Optimizations */
        @media print {
            .content-wrapper {
                margin-bottom: 12px;
            }

            fieldset {
                margin-bottom: 8px;
                padding: 8px 10px;
            }

            legend {
                font-size: 16px;
                padding: 2px 10px;
            }

            .info-table {
                font-size: 14px;
            }

            .info-table td {
                padding: 5px 6px;
            }

            .label, .value {
                font-size: 14px;
            }

            .form-header {
                margin-bottom: 10px;
                padding-top: 5px;
                min-height: 105px;
            }

            .form-title {
                font-size: 17px;
                padding: 5px 20px;
            }

            .signature-section {
                padding: 10px 20px;
                margin-top: 15px;
            }

            .signature-line {
                 width: 145px;
                margin: 30px auto 5px;
                font-size: 13px;
            }

            .corner-decoration {
                display: none;
            }

            .img-thumbnail {
                width: 85px;
                height: 105px;
            }

            .admission-date {
                font-size: 12px;
                padding: 4px 6px;
                width: 85px;
            }

            .student-photo {
                width: 85px;
            }
        }

        /* Watermark */
        .watermark {
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%) rotate(-45deg);
            font-size: 70px;
            color: rgba(102, 126, 234, 0.03);
            font-weight: 900;
            z-index: -1;
            pointer-events: none;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="print-container">
        <!-- Decorative Corners -->
        <div class="corner-decoration top-left d-print-none"></div>
        <div class="corner-decoration top-right d-print-none"></div>
        <div class="corner-decoration bottom-left d-print-none"></div>
        <div class="corner-decoration bottom-right d-print-none"></div>

        <!-- Watermark -->
        <div class="watermark">ADMISSION FORM</div>

        <!-- Action Buttons (Print/Edit) -->
        <asp:FormView ID="FormView1" runat="server" DataSourceID="FormSQL" Width="100%">
            <ItemTemplate>
                <div class="action-buttons d-print-none">
                    <a class="btn btn-blue" href="../Edit_Student_Info/Edit_Student_information.aspx?Student=<%# Eval("StudentID") %>&Student_Class=<%# Eval("StudentClassID") %>">
                        <i class="fa fa-pencil-square" aria-hidden="true"></i> Update Info
                    </a>
                    <input class="btn btn-blue" onclick="window.print();" type="button" value="🖨️ Print" />
                    <a class="btn btn-blue" href="Admission_New_Student.aspx">➕ New Admission</a>
                </div>

                <!-- Form Header with Photo and Date on both sides -->
                <div class="form-header">
                    <!-- Left: Admission Date -->
                    <div class="admission-date">
                        📅 Admission<br/><%# Eval("AdmissionDate","{0:dd/MM/yyyy}") %>
                    </div>
                    
                    <!-- Center: School Info and Title -->
                    <div class="header-center">
                        <div class="form-title">✨ ADMISSION FORM ✨</div>
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
                        <legend>👤 Student Information</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 25%;"><span class="label">ID:</span> <span class="value"><%# Eval("ID") %></span></td>
                                <td style="width: 45%;"><span class="label">Student Name:</span> <span class="value"><%# Eval("StudentsName") %></span></td>
                                <td style="width: 30%;"><span class="label">📱 Mobile:</span> <span class="value"><%# Eval("SMSPhoneNo") %></span></td>
                            </tr>
                            <tr>
                                <td><span class="label">Gender:</span> <span class="value"><%# Eval("Gender") %></span></td>
                                <td><span class="label">🎂 Date of Birth:</span> <span class="value"><%# Eval("DateofBirth","{0:dd/MM/yyyy}") %></span></td>
                                <td><span class="label">🩸 Blood Group:</span> <span class="value"><%# Eval("BloodGroup") %></span></td>
                            </tr>
                            <tr>
                                <td colspan="3"><span class="label">🕌 Religion:</span> <span class="value"><%# Eval("Religion") %></span></td>
                            </tr>
                            <tr>
                                <td colspan="3"><span class="label">🏠 Permanent Address:</span> <span class="value"><%# Eval("StudentPermanentAddress") %></span></td>
                            </tr>
                            <tr>
                                <td colspan="3"><span class="label">📍 Present Address:</span> <span class="value"><%# Eval("StudentsLocalAddress") %></span></td>
                            </tr>

                        </table>
                    </fieldset>

                    <!-- Parents Information -->
                    <fieldset>
                        <legend>👨‍👩‍👦 Parents Information</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 40%;"><span class="label">👨 Father's Name:</span> <span class="value"><%# Eval("FathersName") %></span></td>
                                <td style="width: 30%;"><span class="label">📱 Mobile:</span> <span class="value"><%# Eval("FatherPhoneNumber") %></span></td>
                                <td style="width: 30%;"><span class="label">💼 Occupation:</span> <span class="value"><%# Eval("FatherOccupation") %></span></td>
                            </tr>
                            <tr>
                                <td><span class="label">👩 Mother's Name:</span> <span class="value"><%# Eval("MothersName") %></span></td>
                                <td><span class="label">📱 Mobile:</span> <span class="value"><%# Eval("MotherPhoneNumber") %></span></td>
                                <td><span class="label">💼 Occupation:</span> <span class="value"><%# Eval("MotherOccupation") %></span></td>
                            </tr>
                        </table>
                    </fieldset>

                    <!-- Guardian Information -->
                    <fieldset>
                        <legend>🤝 Guardian Information</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 40%;"><span class="label">Guardian Name:</span> <span class="value"><%# Eval("GuardianName") %></span></td>
                                <td style="width: 30%;"><span class="label">🔗 Relationship:</span> <span class="value"><%# Eval("GuardianRelationshipwithStudent") %></span></td>
                                <td style="width: 30%;"><span class="label">📱 Mobile:</span> <span class="value"><%# Eval("GuardianPhoneNumber") %></span></td>
                            </tr>
                            <tr>
                                <td colspan="3"><span class="label">Other Details:</span> <span class="value"><%# Eval("OtherDetails") %></span></td>
                            </tr>
                        </table>
                    </fieldset>

                    <!-- Previous School Information -->
                    <fieldset>
                        <legend>🏫 Previous Institution Information</legend>
                        <table class="info-table">
                            <tr>
                                <td style="width: 40%;"><span class="label">Institution Name:</span> <span class="value"><%# Eval("PrevSchoolName") %></span></td>
                                <td style="width: 20%;"><span class="label">Class:</span> <span class="value"><%# Eval("PrevClass") %></span></td>
                                <td style="width: 20%;"><span class="label">Year:</span> <span class="value"><%# Eval("PrevExamYear") %></span></td>
                                <td style="width: 20%;"><span class="label">🏆 Grade:</span> <span class="value"><%# Eval("PrevExamGrade") %></span></td>
                            </tr>
                        </table>
                    </fieldset>

                    <!-- Institutional Information -->
                    <fieldset>
                        <legend>🎓 Academic Information</legend>
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

        <!-- Signature Section - Normal flow, stays on first page -->
        <div class="signature-section">
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
            <asp:QueryStringParameter Name="StudentClassID" QueryStringField="StudentClass" />
            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
