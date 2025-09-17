<%@ Page Title="Online Admission Form" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Online_Admission_Form.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Online_Admission_Form" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
     <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <style>
        .online-form { color: #000;font-family: 'Kalpurush', Arial, sans-serif !important;}
        .online-form h3{border:none; font-family: 'Kalpurush', Arial, sans-serif !important;}
        .online-form .border-bottom { margin-bottom:20px; border-bottom: 1px solid #777 !important; }
        .online-form .cb label { margin-bottom: 0; font-family: 'Kalpurush', Arial, sans-serif !important;}
          .C-title {
          font-size: 2rem;
          font-weight: 800;
          width: 290px;
          margin: auto;
          border-radius:15px;
          font-family: 'Kalpurush', Arial, sans-serif !important;
          color: #333;
          text-align:center;
      }
          .c-body {
             color: #000;
            font-size: 18px;
            text-align: justify;
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }
       .online-form fieldset { border-radius:3px;font-family: 'Kalpurush', Arial, sans-serif !important;}
         .online-form legend { padding: 0 7px; font-size: 1rem; font-weight:bold;font-family: 'Kalpurush', Arial, sans-serif !important; }
        .online-form .border{border: 0.5px solid #333 !important;font-family: 'Kalpurush', Arial, sans-serif !important;}
        
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="online-form">
       
        <div class="C-title">ভর্তি ফরম</div>
        
           
          <%--<label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:30px">তারিখ: ......../........../.........</label>--%>
      
        <input type="button" value="Print" class="btn btn-sm btn-blue d-print-none" onclick="window.print();"/>

        <fieldset class="border p-2 mb-3 c-body">
            <legend class="w-auto">শিক্ষার্থীর তথ্য</legend>
            
            <div class="row ">
                
                <div class="col">
                    *শিক্ষার্থীর নাম:<div class="border-bottom"></div>
                </div>
                   <div class="col">
                    *SMS মোবাইল নম্বর:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>

                <div class="col-4">*লিঙ্গ:<div class="border-bottom"></div></div>
                <div class="col">
                    জন্ম তারিখ (দিন/মাস/বছর):<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>

                <div class="col">
                    রক্তের গ্রুপ:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    ধর্ম:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>

                <div class="col">
                    শিক্ষার্থীর স্থায়ী ঠিকানা:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>
                <div class="col">
                     শিক্ষার্থীর অস্থায়ী ঠিকানা:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>

        <fieldset class="border p-2 mb-3 c-body">
            <legend class="w-auto">পিতা-মাতার তথ্য</legend>
            <div class="row">
                <div class="col">
                    *পিতার নাম:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    মোবাইল নম্বর:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    পিতার পেশা:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>

                <div class="col">
                    *মাতার নাম:<div class="border-bottom"></div>
                </div>
                <div class="col">
                   মোবাইল নম্বর:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    মাতার পেশা:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>

        <fieldset class="border p-2 mb-3 c-body" >
            <legend class="w-auto">গার্ডিয়ানের তথ্য (যদি থাকে)</legend>
            <div class="row">
                <div class="col">
                    গার্ডিয়ানের নাম:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    সম্পর্ক:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    মোবাইল নম্বর:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>

        <fieldset class="border p-2 c-body">
            <legend class="w-auto">একাডেমিক তথ্য</legend>
            <div class="row">
                <div class="col">
                    *শ্রেণি:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    শাখা:<div class="border-bottom"></div>
                </div>
                <div class="col">
                    গ্রুপ:<div class="border-bottom"></div>
                </div>
                 <div class="col">
                    রোল:<div class="border-bottom"></div>
                </div>
                <div class="w-100"></div>
                 <div class="col">
                    সাবজেক্ট:<div class="border-bottom"></div>
                </div>
            </div>
        </fieldset>
    </div>
       <div>
       <p style="float:right; font-family: 'Kalpurush', Arial, sans-serif !important;border-top : solid 1px #808080;margin-top:190px">মুহতামীমের স্বাক্ষর</p>
       <p style="float:left; font-family: 'Kalpurush', Arial, sans-serif !important;border-top : solid 1px #808080;margin-top:190px">শিক্ষার্থীর স্বাক্ষর</p>
   </div>
</asp:Content>
