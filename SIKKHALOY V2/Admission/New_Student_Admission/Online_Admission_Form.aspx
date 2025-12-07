<%@ Page Title="Online Admission Form" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Online_Admission_Form.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Online_Admission_Form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <link href="../CSS/OnlineAdmissionForm.css?v=3.1" rel="stylesheet" />
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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="print-container" style="font-weight: 700 !important;">
        <!-- Watermark -->
        <div class="watermark">ভর্তি ফরম</div>

   <!-- Action Buttons -->
 <div class="action-buttons d-print-none">
         <input class="btn btn-blue" onclick="window.print();" type="button" value="প্রিন্ট করুন" style="font-weight: 700 !important;" />
      <a href="Online_Admission_Form_EN.aspx"> Print Admission Form English >> </a> 
        </div>

        <!-- Form Header -->
        <div class="form-header">
            <!-- Left: Admission Date -->
 <div class="admission-date" style="font-weight: 700 !important;">
        ভর্তির তারিখ<br/>___/___/___
       </div>
     
            <!-- Center: Title -->
      <div class="header-center">
          <div class="form-title" style="font-weight: 900 !important;">ভর্তি ফরম</div>
       </div
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
