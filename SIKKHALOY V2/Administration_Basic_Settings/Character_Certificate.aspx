<%@ Page Title="Character Certificate" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Character_Certificate.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.Character_Certificate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .cert-page-wrapper { padding: 30px 20px; }
        .cert-page-title {
            font-size: 1.3rem; font-weight: 700; color: #333;
            margin-bottom: 25px; padding-bottom: 10px;
            border-bottom: 3px solid #512bd4; display: inline-block;
        }
        .cert-grid { display: flex; flex-wrap: wrap; gap: 20px; }
        .cert-card {
            width: 200px; border-radius: 16px; padding: 30px 20px;
            text-align: center; text-decoration: none; color: #fff;
            font-weight: 600; position: relative; overflow: hidden;
            box-shadow: 0 8px 25px rgba(0,0,0,0.18);
            transition: transform 0.25s ease, box-shadow 0.25s ease;
            display: flex; flex-direction: column; align-items: center;
            justify-content: center; gap: 12px; cursor: pointer;
        }
        .cert-card:hover { transform: translateY(-6px); box-shadow: 0 16px 35px rgba(0,0,0,0.28); color: #fff; text-decoration: none; }
        .cert-card::before {
            content: ''; position: absolute; top: -40px; right: -40px;
            width: 100px; height: 100px; border-radius: 50%; background: rgba(255,255,255,0.12);
        }
        .cert-card::after {
            content: ''; position: absolute; bottom: -30px; left: -30px;
            width: 80px; height: 80px; border-radius: 50%; background: rgba(255,255,255,0.08);
        }
        .cert-icon { font-size: 2.4rem; line-height: 1; z-index: 1; }
        .cert-label { font-size: 13px; font-weight: 700; line-height: 1.4; z-index: 1; text-transform: uppercase; letter-spacing: 0.5px; }
        .cert-sublabel { font-size: 11px; font-weight: 400; opacity: 0.85; z-index: 1; }
        .card-en-char { background: linear-gradient(135deg, #1a73e8 0%, #0d47a1 100%); }
        .card-bn-char { background: linear-gradient(135deg, #e91e63 0%, #880e4f 100%); }
        .card-en-test { background: linear-gradient(135deg, #00897b 0%, #004d40 100%); }
        .card-bn-test { background: linear-gradient(135deg, #f57c00 0%, #bf360c 100%); }
        .card-bn-prot { background: linear-gradient(135deg, #7b1fa2 0%, #4a148c 100%); }

        /* OLD - kept for reference */
        .C-title {
            font-size: 1.5rem;
            font-weight: 700;
            width: 290px;
            margin: auto;
            margin-top: 3rem;
            border-bottom: 2px solid #000;
            color: #333;
        }
.bg {
  width: 100%;
  height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-size: 300% 300%;
  /*background-image: linear-gradient(-45deg, Gainsboro 0%, LimeGreen 25%, Gainsboro 51%, #ff357f 100%);*/
  -webkit-animation: AnimateBG 20s ease infinite;
          animation: AnimateBG 20s ease infinite;
    background: linear-gradient(128.87deg, #512bd4 14.05%, #d600aa 89.3%);
}


@-webkit-keyframes AnimateBG {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}

@keyframes AnimateBG {
  0% {
    background-position: 0% 50%;
  }
  50% {
    background-position: 100% 50%;
  }
  100% {
    background-position: 0% 50%;
  }
}
      

    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="cert-page-wrapper">
        <div class="cert-page-title">&#127891; সকল সার্টিফিকেট</div>
        <div class="cert-grid">

            <a href="../Administration_Basic_Settings/AllCertificate/CharecterCertificate_English.aspx" class="cert-card card-en-char">
                <div class="cert-icon">&#128196;</div>
                <div class="cert-label">Character Certificate</div>
                <div class="cert-sublabel">(English)</div>
            </a>

            <a href="../Administration_Basic_Settings/AllCertificate/CharecterCertificate_Bangla.aspx" class="cert-card card-bn-char">
                <div class="cert-icon">&#128196;</div>
                <div class="cert-label">চারিত্রিক সনদ</div>
                <div class="cert-sublabel">(বাংলা)</div>
            </a>

            <a href="../Administration_Basic_Settings/AllCertificate/Testimonial_English.aspx" class="cert-card card-en-test">
                <div class="cert-icon">&#127942;</div>
                <div class="cert-label">Testimonial</div>
                <div class="cert-sublabel">(English)</div>
            </a>

            <a href="../Administration_Basic_Settings/AllCertificate/Testimonial_Bangla.aspx" class="cert-card card-bn-test">
                <div class="cert-icon">&#127942;</div>
                <div class="cert-label">প্রশংসা পত্র</div>
                <div class="cert-sublabel">(বাংলা)</div>
            </a>

            <a href="../Administration_Basic_Settings/AllCertificate/Prottoyon_Bangla.aspx" class="cert-card card-bn-prot">
                <div class="cert-icon">&#128221;</div>
                <div class="cert-label">প্রত্যয়ন পত্র</div>
                <div class="cert-sublabel">(বাংলা)</div>
            </a>

        </div>
    </div>

</asp:Content>
