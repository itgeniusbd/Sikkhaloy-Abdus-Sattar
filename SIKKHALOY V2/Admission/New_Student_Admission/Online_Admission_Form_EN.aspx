<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Online_Admission_Form_EN.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Online_Admission_Form_Bangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
  <link href="../CSS/OnlineAdmissionForm.css?v=3.1" rel="stylesheet" />

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="print-container" style="font-weight: 700 !important;">
     <!-- Watermark -->
        <div class="watermark">Admission Form</div>

   <!-- Action Buttons -->
 <div class="action-buttons d-print-none">
         <input class="btn btn-blue" onclick="window.print();" type="button" value="Print" style="font-weight: 700 !important;" />
       <a href="Online_Admission_Form.aspx"> << Print Admission Form Bangla </a> 
        </div>

        <!-- Form Header -->
  <div class="form-header">
    <!-- Left: Admission Date -->
 <div class="admission-date" style="font-weight: 700 !important;">
        Admission Date<br/>___/___/___
       </div>
     
            <!-- Center: Title -->
      <div class="header-center">
   <div class="form-title" style="font-weight: 900 !important;">Admission Form</div>
    </div>
    <!-- Right: Photo Box -->
   <div class="student-photo-box" style="font-weight: 700 !important;">
           Photo<br/>Space
  </div>
        </div>

        <!-- Content Wrapper -->
  <div class="content-wrapper" style="font-weight: 700 !important;">
      <!-- Student Information -->
            <fieldset>
 <legend style="font-weight: 900 !important;">Student Information</legend>
      <table class="info-table" style="font-weight: 700 !important;">
        <tr>
   <td style="width: 25%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">ID:</span><div class="blank-line"></div></td>
        <td style="width: 45%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Student Name:</span><div class="blank-line"></div></td>
         <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Mobile:</span><div class="blank-line"></div></td>
          </tr>
        <tr>
          <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Gender:</span><div class="blank-line"></div></td>
     <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Date of Birth:</span><div class="blank-line"></div></td>
     <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Blood Group:</span><div class="blank-line"></div></td>
   </tr>
     <tr>
       <td colspan="3" style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Religion:</span><div class="blank-line"></div></td>
 </tr>
      <tr>
           <td colspan="3" style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Permanent Address:</span><div class="blank-line"></div></td>
     </tr>
         <tr>
   <td colspan="3" style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Present Address:</span><div class="blank-line"></div></td>
         </tr>
  </table>
   </fieldset>

            <!-- Parents Information -->
    <fieldset>
    <legend style="font-weight: 900 !important;">Parents Information</legend>
     <table class="info-table" style="font-weight: 700 !important;">
  <tr>
        <td style="width: 40%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Father's Name:</span><div class="blank-line"></div></td>
   <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Mobile:</span><div class="blank-line"></div></td>
      <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Occupation:</span><div class="blank-line"></div></td>
   </tr>
     <tr>
    <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Mother's Name:</span><div class="blank-line"></div></td>
  <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Mobile:</span><div class="blank-line"></div></td>
           <td style="font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Occupation:</span><div class="blank-line"></div></td>
  </tr>
             </table>
            </fieldset>

       <!-- Guardian Information -->
  <fieldset>
       <legend style="font-weight: 900 !important;">Guardian Information</legend>
     <table class="info-table" style="font-weight: 700 !important;">
    <tr>
           <td style="width: 40%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Guardian's Name:</span><div class="blank-line"></div></td>
         <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Relationship:</span><div class="blank-line"></div></td>
              <td style="width: 30%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Mobile:</span><div class="blank-line"></div></td>
   </tr>
    </table>
 </fieldset>

            <!-- Previous School Information -->
      <fieldset>
            <legend style="font-weight: 900 !important;">Previous School Information</legend>
     <table class="info-table" style="font-weight: 700 !important;">
<tr>
     <td style="width: 40%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">School Name:</span><div class="blank-line"></div></td>
      <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Class:</span><div class="blank-line"></div></td>
          <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Year:</span><div class="blank-line"></div></td>
       <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Grade:</span><div class="blank-line"></div></td>
           </tr>
    </table>
   </fieldset>

     <!-- Institutional Information -->
  <fieldset>
     <legend style="font-weight: 900 !important;">Institutional Information</legend>
        <table class="info-table" style="font-weight: 700 !important;">
  <tr>
<td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Class:</span><div class="blank-line"></div></td>
              <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Roll No:</span><div class="blank-line"></div></td>
 <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Section:</span><div class="blank-line"></div></td>
    <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Shift:</span><div class="blank-line"></div></td>
      <td style="width: 20%; font-weight: 700 !important;"><span class="label" style="font-weight: 900 !important;">Group:</span><div class="blank-line"></div></td>
  </tr>
    </table>
        </fieldset>
   </div>

        <!-- Signature Section -->
      <div class="signature-section" style="font-weight: 700 !important;">
       <div class="signature-box">
      <div class="signature-line" style="font-weight: 700 !important;">Student's Signature</div>
     <div class="signature-date" style="font-weight: 700 !important;">Date: ______</div>
        </div>
        <div class="signature-box">
          <div class="signature-line" style="font-weight: 700 !important;">Guardian's Signature</div>
<div class="signature-date" style="font-weight: 700 !important;">Date: ______</div>
 </div>
     <div class="signature-box">
    <div class="signature-line" style="font-weight: 700 !important;">Headmaster's Signature</div>
     <div class="signature-date" style="font-weight: 700 !important;">Date: ______</div>
     </div>
   </div>
    </div>
</asp:Content>