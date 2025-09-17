<%@ Page Title="Character Certificate" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Character_Certificate.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.Character_Certificate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
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
      
.link-design {
padding: 3rem;
    padding-top: 3rem;
  border-radius: .25rem;
  margin-top: 3px;
  text-transform: uppercase;
  color: #3e4551;
  font-size: 16px;
  font-weight: 400;
  background-color: #f0f0f0;
  box-shadow: 0 2px 5px 0 rgba(0,0,0,.16),0 2px 10px 0 rgba(0,0,0,.12);
  margin-bottom: 1.5rem !important;
  margin-left: 20px;
  width: 360px;
  text-align: center;
  height: 150px;
  vertical-align: middle;
  padding-top: 58px;
  
        }

.link-design a{color:#fff;}
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <div class="form-inline">

        <div class="link-design bg">

            <a href="../Administration_Basic_Settings/AllCertificate/CharecterCertificate_English.aspx"><b>Charecter Cerificate (English)</b></a>
        </div>
        <div class="link-design bg">

            <a href="../Administration_Basic_Settings/AllCertificate/CharecterCertificate_Bangla.aspx"><b>চারিত্রিক সনদ (বাংলা)</b></a>
        </div>
        <div class="link-design bg">

            <a href="../Administration_Basic_Settings/AllCertificate/Testimonial_English.aspx"><b>Testimonial (English)</b></a>
        </div>
        <div class="link-design bg">

            <a href="../Administration_Basic_Settings/AllCertificate/Testimonial_Bangla.aspx"><b>প্রশংসা পত্র (বাংলা)</b></a>
        </div>
    </div>

 


</asp:Content>
