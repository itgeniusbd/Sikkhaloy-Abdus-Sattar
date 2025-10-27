<%@ Page Title="Online Admission Form" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Online_Admission_Form.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Online_Admission_Form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <link href="../CSS/OnlineAdmissionForm.css" rel="stylesheet" />
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

        <!-- Action Buttons -->
        <div class="action-buttons d-print-none">
            <input class="btn btn-blue" onclick="window.print();" type="button" value="🖨️ প্রিন্ট করুন" />
        </div>

        <!-- Form Header - COMPACT -->
        <div class="form-header">
            <!-- Left: Admission Date (Blank) -->
            <div class="admission-date">
                📅 ভর্তির তারিখ<br/>___/___/___
            </div>
            
            <!-- Center: School Info and Title -->
            <div class="header-center">
                <div class="form-title">✨ ভর্তি ফরম ✨</div>
            </div>
            
            <!-- Right: Photo Box -->
            <div class="student-photo-box">
                ছবি<br/>স্থান
            </div>
        </div>

        <!-- Content Wrapper - COMPACT -->
        <div class="content-wrapper">
            <!-- Student Information -->
            <fieldset>
                <legend>👤 শিক্ষার্থীর তথ্য</legend>
                <table class="info-table">
                    <tr>
                        <td style="width: 25%;"><span class="label">আইডি:</span><div class="blank-line"></div></td>
                        <td style="width: 45%;"><span class="label">শিক্ষার্থীর নাম:</span><div class="blank-line"></div></td>
                        <td style="width: 30%;"><span class="label">📱 মোবাইল:</span><div class="blank-line"></div></td>
                    </tr>
                    <tr>
                        <td><span class="label">লিঙ্গ:</span><div class="blank-line"></div></td>
                        <td><span class="label">🎂 জন্ম তারিখ:</span><div class="blank-line"></div></td>
                        <td><span class="label">🩸 রক্তের গ্রুপ:</span><div class="blank-line"></div></td>
                    </tr>
                    <tr>
                        <td colspan="3"><span class="label">🕌 ধর্ম:</span><div class="blank-line"></div></td>
                    </tr>
                    <tr>
                        <td colspan="3"><span class="label">🏠 স্থায়ী ঠিকানা:</span><div class="blank-line"></div></td>
                    </tr>
                    <tr>
                        <td colspan="3"><span class="label">📍 বর্তমান ঠিকানা:</span><div class="blank-line"></div></td>
                    </tr>
                </table>
            </fieldset>

            <!-- Parents Information -->
            <fieldset>
                <legend>👨‍👩‍👦 পিতা-মাতার তথ্য</legend>
                <table class="info-table">
                    <tr>
                        <td style="width: 40%;"><span class="label">👨 পিতার নাম:</span><div class="blank-line"></div></td>
                        <td style="width: 30%;"><span class="label">📱 মোবাইল:</span><div class="blank-line"></div></td>
                        <td style="width: 30%;"><span class="label">💼 পেশা:</span><div class="blank-line"></div></td>
                    </tr>
                    <tr>
                        <td><span class="label">👩 মাতার নাম:</span><div class="blank-line"></div></td>
                        <td><span class="label">📱 মোবাইল:</span><div class="blank-line"></div></td>
                        <td><span class="label">💼 পেশা:</span><div class="blank-line"></div></td>
                    </tr>
                </table>
            </fieldset>

            <!-- Guardian Information -->
            <fieldset>
                <legend>🤝 অভিভাবকের তথ্য</legend>
                <table class="info-table">
                    <tr>
                        <td style="width: 40%;"><span class="label">অভিভাবকের নাম:</span><div class="blank-line"></div></td>
                        <td style="width: 30%;"><span class="label">🔗 সম্পর্ক:</span><div class="blank-line"></div></td>
                        <td style="width: 30%;"><span class="label">📱 মোবাইল:</span><div class="blank-line"></div></td>
                    </tr>
                </table>
            </fieldset>

            <!-- Previous School Information -->
            <fieldset>
                <legend>🏫 পূর্বের শিক্ষা প্রতিষ্ঠানের তথ্য</legend>
                <table class="info-table">
                    <tr>
                        <td style="width: 40%;"><span class="label">প্রতিষ্ঠানের নাম:</span><div class="blank-line"></div></td>
                        <td style="width: 20%;"><span class="label">শ্রেণি:</span><div class="blank-line"></div></td>
                        <td style="width: 20%;"><span class="label">সাল:</span><div class="blank-line"></div></td>
                        <td style="width: 20%;"><span class="label">🏆 গ্রেড:</span><div class="blank-line"></div></td>
                    </tr>
                </table>
            </fieldset>

            <!-- Institutional Information -->
            <fieldset>
                <legend>🎓 প্রাতিষ্ঠানিক তথ্য</legend>
                <table class="info-table">
                    <tr>
                        <td style="width: 20%;"><span class="label">শ্রেণি:</span><div class="blank-line"></div></td>
                        <td style="width: 20%;"><span class="label">রোল নং:</span><div class="blank-line"></div></td>
                        <td style="width: 20%;"><span class="label">শাখা:</span><div class="blank-line"></div></td>
                        <td style="width: 20%;"><span class="label">শিফট:</span><div class="blank-line"></div></td>
                        <td style="width: 20%;"><span class="label">গ্রুপ:</span><div class="blank-line"></div></td>
                    </tr>
                </table>
            </fieldset>
        </div>

        <!-- Signature Section - COMPACT -->
        <div class="signature-section">
            <div class="signature-box">
                <div class="signature-line">শিক্ষার্থীর স্বাক্ষর</div>
                <div class="signature-date">তারিখ: ______</div>
            </div>
            <div class="signature-box">
                <div class="signature-line">অভিভাবকের স্বাক্ষর</div>
                <div class="signature-date">তারিখ: ______</div>
            </div>
            <div class="signature-box">
                <div class="signature-line">প্রধান শিক্ষকের স্বাক্ষর</div>
                <div class="signature-date">তারিখ: ______</div>
            </div>
        </div>
    </div>
</asp:Content>
