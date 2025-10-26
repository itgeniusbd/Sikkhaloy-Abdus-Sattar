<%@ Page Title="Online Admission Form" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Online_Admission_Form.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Online_Admission_Form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <style>
        * {
            font-family: 'Kalpurush', Arial, sans-serif !important;
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            line-height: 1.3;
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
                margin-top: 5px !important;
                padding-top: 5px !important;
            }

            .signature-section {
                page-break-inside: avoid;
                margin-top: 10px !important;
            }
        }

        .print-container {
            max-width: 210mm;
            margin: 0 auto;
            padding: 8px;
            background: white;
            position: relative;
        }

        /* Form Header - COMPACT */
        .form-header {
            position: relative;
            text-align: center;
            margin-bottom: 10px;
             margin-top: 5px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            min-height: 90px;
        }

.admission-date {
    width: 80px;
    font-size: 13px;
    font-weight: bold;
    /* background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); */
    color: #000;
    padding: 4px -5px;
    border-radius: 5px;
    /* border: 2px solid #5a67d8; */
    /* box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2); */
    text-align: center;
    line-height: 1.2;
}

        .header-center {
            flex: 1;
            text-align: center;
        }

        .school-name {
            font-size: 17px;
            font-weight: 900;
            color: #333;
            margin-bottom: 2px;
        }

        .school-address {
            font-size: 11px;
            color: #666;
            margin-bottom: 4px;
        }

        .form-title {
            font-size: 16px;
            font-weight: 900;
            color: #fff;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            display: inline-block;
            padding: 5px 20px;
            border-radius: 5px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            border: 2px solid #5a67d8;
        }

        .student-photo-box {
            width: 85px;
            height: 90px;
            border: 2px solid #5a67d8;
            border-radius: 5px;
            background: linear-gradient(135deg, #f8f9ff 0%, #ffffff 100%);
            display: flex;
            align-items: center;
            justify-content: center;
            color: #999;
            font-size: 11px;
            text-align: center;
            padding: 4px;
            box-shadow: 0 2px 6px rgba(0,0,0,0.2);
        }

        /* Action Buttons */
        .action-buttons {
            text-align: center;
            margin-bottom: 8px;
        }

        .btn {
            padding: 6px 14px;
            margin: 2px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-size: 13px;
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

        /* Content Wrapper - COMPACT */
        .content-wrapper {
            margin-bottom: 10px;
        }

        /* Fieldset Styles - COMPACT */
        fieldset {
            border: 2px solid #667eea;
            border-radius: 6px;
            padding: 6px 10px;
            margin-bottom: 6px;
            background: linear-gradient(to right, #f8f9ff 0%, #ffffff 100%);
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
        }

        legend {
            font-size: 14px;
            font-weight: 900;
            padding: 2px 10px;
            color: #fff;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 4px;
            border: 2px solid #5a67d8;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
        }

        /* Table Styles - COMPACT */
        .info-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 13px;
        }

        .info-table td {
            padding: 4px 6px;
            vertical-align: top;
        }

        .label {
            font-weight: 700;
            color: #5a67d8;
            display: inline-block;
            font-size: 16px;
        }

        .blank-line {
            border-bottom: 2px dotted #999;
            min-height: 20px;
            margin-top: 3px;
        }

        /* Signature Section - COMPACT */
        .signature-section {
            display: flex;
            justify-content: space-between;
            padding: 15px 15px;
            margin-top: 10px;
          
           
        }

        .signature-box {
            text-align: center;
            flex: 1;
        }

        .signature-line {
            border-top: 2px solid #000;
            width: 142px;
            margin: 25px auto 4px;
            padding-top: 4px;
            font-weight: 700;
            font-size: 15px;
            color: #333;
        }

        .signature-date {
            font-size: 10px;
            color: #666;
            margin-top: 2px;
        }

        /* Decorative Elements */
        .corner-decoration {
            position: absolute;
            width: 30px;
            height: 30px;
            border: 2px solid #667eea;
        }

        .corner-decoration.top-left {
            top: 0;
            left: 0;
            border-right: none;
            border-bottom: none;
            border-radius: 5px 0 0 0;
        }

        .corner-decoration.top-right {
            top: 0;
            right: 0;
            border-left: none;
            border-bottom: none;
            border-radius: 0 5px 0 0;
        }

        .corner-decoration.bottom-left {
            bottom: 0;
            left: 0;
            border-right: none;
            border-top: none;
            border-radius: 0 0 0 5px;
        }

        .corner-decoration.bottom-right {
            bottom: 0;
            right: 0;
            border-left: none;
            border-top: none;
            border-radius: 0 0 5px 0;
        }

        /* Print Optimizations */
        @media print {
            .content-wrapper {
                margin-bottom: 8px;
            }

            fieldset {
                margin-bottom: 5px;
                padding: 5px 8px;
            }

            legend {
                font-size: 13px;
                padding: 2px 8px;
            }

            .info-table {
                font-size: 12px;
            }

            .info-table td {
                padding: 3px 5px;
            }

            .label {
                font-size: 16px;
            }

            .blank-line {
                min-height: 20px;
            }

            .form-header {
                margin-bottom: 8px;
                padding-top: 5px;
                min-height: 85px;
            }

            .form-title {
                font-size: 15px;
                padding: 4px 18px;
            }

            .signature-section {
                padding: 15px 15px;
                margin-top:10px;
            }

            .signature-line {
                width: 142px;
                margin: 20px auto 4px;
                font-size: 15px;
            }

            .corner-decoration {
                display: none;
            }

            .student-photo-box {
                width: 80px;
                height: 85px;
            }

.admission-date {
    width: 80px;
    font-size: 13px;
    font-weight: bold;
    /* background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); */
    color: #000;
    padding: 4px -5px;
    border-radius: 5px;
    /* border: 2px solid #5a67d8; */
    /* box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2); */
    text-align: center;
    line-height: 1.2;
}
        }

        /* Watermark */
        .watermark {
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%) rotate(-45deg);
            font-size: 60px;
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
