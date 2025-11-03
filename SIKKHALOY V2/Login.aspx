<%@ Page Title="Login" Language="C#" MasterPageFile="~/Design.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="EDUCATION.COM.Login1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
     header { display: none; }
        #toTop { display:none !important}
   
        /* Modern Login Page Styling */
        body {
  background: linear-gradient(135deg, #ee6a6a 0%, #1a4d2e 100%);
         min-height: 100vh;
        }
     
        .login-wrapper {
  min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
       padding: 20px;
    background: linear-gradient(135deg, #bf8383 0%, #11d75e 100%);
     }
        
        .login-card {
  background: #ffffff;
            border-radius: 20px;
    box-shadow: 0 20px 60px rgba(0,0,0,0.3);
    overflow: hidden;
    max-width: 450px;
         width: 100%;
            animation: slideUp 0.5s ease-out;
        }
        
        @keyframes slideUp {
            from {
                opacity: 0;
    transform: translateY(30px);
  }
    to {
     opacity: 1;
              transform: translateY(0);
     }
        }
        
        .login-header {
            background: linear-gradient(135deg, #dd6e6e 0%, #4aff91 100%);
        padding: 40px 30px;
   text-align: center;
            color: white;
        }
        
     .login-header i {
            font-size: 50px;
       margin-bottom: 15px;
  animation: pulse 2s infinite;
   }
        
        @keyframes pulse {
        0%, 100% {
                transform: scale(1);
            }
       50% {
        transform: scale(1.1);
   }
        }
        
        .login-header h4 {
        margin: 0;
            font-size: 28px;
            font-weight: 600;
    letter-spacing: 1px;
     }
        
        .login-body {
padding: 40px 35px;
        }
        
        .input-group-modern {
     position: relative;
       margin-bottom: 30px;
        }
     
        .input-group-modern i {
      position: absolute;
       left: 15px;
   top: 50%;
     transform: translateY(-50%);
color: #1a4d2e;
 font-size: 18px;
   z-index: 10;
            transition: all 0.3s ease;
        }
        
        .input-group-modern input {
  width: 100%;
            padding: 15px 15px 15px 45px;
            border: 2px solid #e0e0e0;
   border-radius: 10px;
     font-size: 16px;
          transition: all 0.3s ease;
  background: #f8f9fa;
        }
 
        .input-group-modern input:focus {
            outline: none;
   border-color: #1a4d2e;
         background: #ffffff;
     box-shadow: 0 5px 15px rgba(26, 77, 46, 0.2);
        }
        
     .input-group-modern input:focus + i {
        color: #ee6a6a;
      transform: translateY(-50%) scale(1.1);
        }
        
    .input-group-modern input::placeholder {
      color: #999;
}
        
        .btn-login {
         width: 100%;
            padding: 15px;
      background: linear-gradient(135deg, #ee6a6a 0%, #1a4d2e 100%);
            border: none;
   border-radius: 10px;
 color: white;
            font-size: 18px;
            font-weight: 600;
  cursor: pointer;
            transition: all 0.3s ease;
            text-transform: uppercase;
     letter-spacing: 1px;
        box-shadow: 0 5px 15px rgba(26, 77, 46, 0.4);
        }
        
   .btn-login:hover {
    transform: translateY(-2px);
            box-shadow: 0 8px 25px rgba(26, 77, 46, 0.5);
        }
        
        .btn-login:active {
            transform: translateY(0);
        }
        
   .error-message {
            background: #fee;
            border-left: 4px solid #f44336;
 color: #f44336 !important;
            padding: 12px 15px;
    border-radius: 5px;
            margin-top: 20px;
     font-size: 14px;
            animation: shake 0.5s;
            display:none;
        }
        
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-10px); }
         75% { transform: translateX(10px); }
        }
        
        .validator-text {
       color: #f44336;
     font-size: 12px;
 margin-top: 5px;
            display: block;
        }
        
     /* Responsive Design */
        @media (max-width: 576px) {
    .login-card {
        border-radius: 15px;
            }

       .login-header {
  padding: 30px 20px;
      }
   
          .login-header h4 {
                font-size: 24px;
            }
   
            .login-body {
      padding: 30px 25px;
      line-height:40px;
   }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="login-wrapper">
        <div class="login-card">
            <div class="login-header">
      <i class="fa fa-lock"></i>
       <h4>স্বাগতম</h4>
        <p style="margin: 10px 0 0 0; font-size: 14px; opacity: 0.9;">আপনার অ্যাকাউন্টে লগইন করুন</p>
            </div>
            
 <div class="login-body">
     <asp:Login ID="UserLogin2" runat="server" OnLoginError="UserLogin_LoginError" OnLoggedIn="UserLogin_LoggedIn" DestinationPageUrl="~/Profile_Redirect.aspx" Width="100%">
  <LayoutTemplate>
       <div class="input-group-modern">
      <asp:TextBox ID="UserName" runat="server" class="form-control" placeholder="ইউজারনেম লিখুন"></asp:TextBox>
          <i class="fa fa-user"></i>
       <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" ErrorMessage="ইউজারনেম প্রয়োজন" CssClass="validator-text" ToolTip="User Name is required." ValidationGroup="Login2" Display="Dynamic">*</asp:RequiredFieldValidator>
    </div>
       
     <div class="input-group-modern">
        <asp:TextBox ID="Password" runat="server" class="form-control" TextMode="Password" placeholder="পাসওয়ার্ড লিখুন"></asp:TextBox>
         <i class="fa fa-lock"></i>
    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" ErrorMessage="পাসওয়ার্ড প্রয়োজন" CssClass="validator-text" ToolTip="Password is required." ValidationGroup="Login2" Display="Dynamic">*</asp:RequiredFieldValidator>
            </div>

            <div class="text-center" style="margin-top: 30px;">
     <asp:Button ID="LoginButton" runat="server" CommandName="Login" class="btn-login" Text="লগইন করুন" ValidationGroup="Login2" />
        </div>
      
         <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
           </LayoutTemplate>
     </asp:Login>
        
   <asp:Label ID="InvalidErrorLabel" runat="server" CssClass="error-message"></asp:Label>
       </div>
        </div>
    </div>
</asp:Content>
