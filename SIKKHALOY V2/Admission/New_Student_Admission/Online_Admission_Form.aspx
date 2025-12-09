<%@ Page Title="Online Admission Form" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Online_Admission_Form.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Online_Admission_Form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <link href="../CSS/OnlineAdmissionForm.css?v=3.2" rel="stylesheet" />
    <style>
        /* Force bold font with highest priority */
        body, .print-container, .print-container * {
            font-weight: 700 !important;
        }
   
        .label {
            font-weight: 900 !important;
        }
        
        legend {
            font-weight: 900 !important;
        }
        
        .form-title {
            font-weight: 900 !important;
        }
  
        /* Navigation Bar Button Hover Effects */
        .d-print-none a:hover,
        .d-print-none button:hover {
            transform: translateY(-3px);
            box-shadow: 0 6px 20px rgba(0, 0, 0, 0.25) !important;
        }
        
        .d-print-none a:active,
        .d-print-none button:active {
            transform: translateY(-1px);
        }
        
        /* Responsive design for navigation bar */
        @media (max-width: 768px) {
            .d-print-none > div > div:last-child {
                flex-direction: column !important;
            }
            
            .d-print-none a,
            .d-print-none button {
                width: 100%;
                justify-content: center;
            }
        }
        
        /* Print styles - hide navigation */
        @media print {
            .d-print-none {
                display: none !important;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <!-- Modern Navigation Bar at Top -->
    <div class="d-print-none" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 20px 40px; margin: -20px -20px 30px -20px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); border-radius: 0 0 15px 15px;">
        <div style="max-width: 1200px; margin: 0 auto;">
            <!-- Header Title -->
          
            <!-- Navigation Buttons -->
            <div style="display: flex; gap: 12px; justify-content: center; align-items: center; flex-wrap: wrap;">
                <button class="btn btn-blue" onclick="window.print();" style="background: white; color: #667eea; border: none; padding: 12px 25px; border-radius: 8px; font-weight: 700; box-shadow: 0 4px 12px rgba(0,0,0,0.15); transition: all 0.3s ease; cursor: pointer; display: inline-flex; align-items: center; gap: 8px;">
                    <i class="fa fa-print" style="font-size: 16px;"></i>
                    <span>প্রিন্ট করুন</span>
                </button>
                
                <a href="Online_Admission_Form_EN.aspx" style="background: white; color: #000; text-decoration: none; padding: 12px 25px; border-radius: 8px; font-weight: 700; box-shadow: 0 4px 12px rgba(0,0,0,0.15); transition: all 0.3s ease; display: inline-flex; align-items: center; gap: 8px;">
                    <i class="fa fa-language" style="font-size: 18px;"></i>
                    <span>English Form</span>
                </a>
                
                <a href="Admission_Form.aspx" style="background: white; color: #000; text-decoration: none; padding: 12px 25px; border-radius: 8px; font-weight: 700; box-shadow: 0 4px 12px rgba(0,0,0,0.15); transition: all 0.3s ease; display: inline-flex; align-items: center; gap: 8px;">
                    <i class="fa fa-file-text-o" style="font-size: 18px;"></i>
                    <span>Information Form</span>
                </a>
                
                <a href="Form_Bangla.aspx" style="background: white; color: #000; text-decoration: none; padding: 12px 25px; border-radius: 8px; font-weight: 700; box-shadow: 0 4px 12px rgba(0,0,0,0.15); transition: all 0.3s ease; display: inline-flex; align-items: center; gap: 8px;">
                    <i class="fa fa-file-text" style="font-size: 18px;"></i>
                    <span>তথ্যসহ ফরম</span>
                </a>
            </div>
        </div>
    </div>

    <div class="print-container" style="font-weight: 700 !important;">
        <!-- Watermark -->
        <div class="watermark">ভর্তি ফরম</div>

        <!-- Form Header -->
        <div class="form-header">
            <!-- Left: Admission Date -->
            <div class="admission-date" style="font-weight: 700 !important;">
                ভর্তির তারিখ<br/>___/___/___
            </div>
     
            <!-- Center: Title -->
            <div class="header-center">
                <div class="form-title" style="font-weight: 900 !important;">ভর্তি ফরম</div>
            </div>
            
            <!-- Right: Photo Box -->
            <div class="student-photo-box" style="font-weight: 700 !important;">
                ছবি<br/>স্থান
            </div>
        </div>

        <!-- Content Wrapper -->
    <div class="content-wrapper" style="font-weight: 700 !important;">
      <!-- Student Information -->
            <fieldset>
 <legend style="font-weight: 900 !important;">শিক্ষার্থীর তথ্য</legend>
      <table class="info-table" style="font-weight: 700 !important;">
        <tr>
   <td style="width: 25%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">আইডি:</span><div class="blank-line"></div></td>
        <td style="width: 45%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">শিক্ষার্থীর নাম:</span><div class="blank-line"></div></td>
         <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">মোবাইল:</span><div class="blank-line"></div></td>
          </tr>
        <tr>
          <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">লিঙ্গ:</span><div class="blank-line"></div></td>
     <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">জন্ম তারিখ:</span><div class="blank-line"></div></td>
     <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">রক্তের গ্রুপ:</span><div class="blank-line"></div></td>
         </tr>
     <tr>
       <td colspan="3" style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">ধর্ম:</span><div class="blank-line"></div></td>
         </tr>
      <tr>
           <td colspan="3" style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">স্থায়ী ঠিকানা:</span><div class="blank-line"></div></td>
     </tr>
           <tr>
              <td colspan="3" style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">বর্তমান ঠিকানা:</span><div class="blank-line"></div></td>
         </tr>
  </table>
   </fieldset>

            <!-- Parents Information -->
    <fieldset>
        <legend style="font-weight: 900 !important;">পিতা-মাতার তথ্য</legend>
     <table class="info-table" style="font-weight: 700 !important;">
  <tr>
        <td style="width: 40%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">পিতার নাম:</span><div class="blank-line"></div></td>
   <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">মোবাইল:</span><div class="blank-line"></div></td>
      <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">পেশা:</span><div class="blank-line"></div></td>
   </tr>
            <tr>
    <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">মাতার নাম:</span><div class="blank-line"></div></td>
  <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">মোবাইল:</span><div class="blank-line"></div></td>
           <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">পেশা:</span><div class="blank-line"></div></td>
  </tr>
             </table>
            </fieldset>

            <!-- Guardian Information -->
  <fieldset>
       <legend style="font-weight: 900 !important;">অভিভাবকের তথ্য</legend>
     <table class="info-table" style="font-weight: 700 !important;">
    <tr>
           <td style="width: 40%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">অভিভাবকের নাম:</span><div class="blank-line"></div></td>
         <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">সম্পর্ক:</span><div class="blank-line"></div></td>
              <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">মোবাইল:</span><div class="blank-line"></div></td>
   </tr>
                </table>
 </fieldset>

            <!-- Previous School Information -->
            <fieldset>
            <legend style="font-weight: 900 !important;">পূর্বের শিক্ষা প্রতিষ্ঠানের তথ্য</legend>
     <table class="info-table" style="font-weight: 700 !important;">
            <tr>
            <td style="width: 40%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">প্রতিষ্ঠানের নাম:</span><div class="blank-line"></div></td>
      <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">শ্রেণি:</span><div class="blank-line"></div></td>
                <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">সাল:</span><div class="blank-line"></div></td>
            <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">গ্রেড:</span><div class="blank-line"></div></td>
           </tr>
    </table>
   </fieldset>

       <!-- Institutional Information -->
  <fieldset>
     <legend style="font-weight: 900 !important;">প্রাতিষ্ঠানিক তথ্য</legend>
        <table class="info-table" style="font-weight: 700 !important;">
            <tr>
<td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">শ্রেণি:</span><div class="blank-line"></div></td>
              <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">রোল নং:</span><div class="blank-line"></div></td>
 <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">শাখা:</span><div class="blank-line"></div></td>
             <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">শিফট:</span><div class="blank-line"></div></td>
      <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">গ্রুপ:</span><div class="blank-line"></div></td>
         </tr>
      </table>
            </fieldset>
   </div>

        <!-- Signature Section -->
        <div class="signature-section" style="font-weight: 700 !important;">
       <div class="signature-box">
        <div class="signature-line" style="font-weight: 700 !important;">শিক্ষার্থীর স্বাক্ষর</div>
     <div class="signature-date" style="font-weight: 700 !important;">তারিখ: ______</div>
        </div>
            <div class="signature-box">
          <div class="signature-line" style="font-weight: 700 !important;">অভিভাবকের স্বাক্ষর</div>
           <div class="signature-date" style="font-weight: 700 !important;">তারিখ: ______</div>
 </div>
            <div class="signature-box">
    <div class="signature-line" style="font-weight: 700 !important;">প্রধান শিক্ষকের স্বাক্ষর</div>
          <div class="signature-date" style="font-weight: 700 !important;">তারিখ: ______</div>
         </div>
   </div>
    </div>
</asp:Content>
