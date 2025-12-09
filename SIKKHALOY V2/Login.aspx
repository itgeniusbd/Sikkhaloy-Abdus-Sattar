<%@ Page Title="Login" Language="C#" MasterPageFile="~/Design.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="EDUCATION.COM.Login1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        header { display: none; }
        #toTop { display:none !important}
   
        /* Modern Login Page Styling - Dark Theme */
        body {
            background: linear-gradient(135deg, #2c3e50 0%, #1a252f 100%);
            min-height: 100vh;
        }
     
        .login-wrapper {
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
            background: linear-gradient(135deg, #273A28  0%, #2c3e50 100%);
        }
        
        .login-card {
            background: #2c3e50;
            border-radius: 20px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.5);
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
            background: linear-gradient(135deg, #00c851 0%, #007E33 100%);
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
            background: linear-gradient(180deg, #34495e 0%, #2c3e50 100%);
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
            color: #00c851;
            font-size: 18px;
            z-index: 10;
            transition: all 0.3s ease;
        }
        
        .input-group-modern input {
            width: 100%;
            padding: 15px 15px 15px 45px;
            border: 2px solid #4a5f7f;
            border-radius: 10px;
            font-size: 16px;
            transition: all 0.3s ease;
            background: #3d5568;
            color: #fff;
        }
 
        .input-group-modern input:focus {
            outline: none;
            border-color: #00c851;
            background: #4a5f7f;
            box-shadow: 0 5px 15px rgba(0, 200, 81, 0.2);
        }
        
        .input-group-modern input:focus + i {
            color: #00ff6a;
            transform: translateY(-50%) scale(1.1);
        }
        
        .input-group-modern input::placeholder {
            color: #95a5a6;
        }
        
        .btn-login {
            width: 100%;
            padding: 15px;
            background: linear-gradient(135deg, #00c851 0%, #007E33 100%);
            border: none;
            border-radius: 10px;
            color: white;
            font-size: 18px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
            text-transform: uppercase;
            letter-spacing: 1px;
            box-shadow: 0 5px 15px rgba(0, 200, 81, 0.4);
        }
        
        .btn-login:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 25px rgba(0, 200, 81, 0.5);
            background: linear-gradient(135deg, #00a32a 0%, #007E33 100%);
        }
        
        .btn-login:active {
            transform: translateY(0);
        }
        
        .error-message {
            background: #2c1f1f;
            border-left: 4px solid #f44336;
            color: #ff6b6b !important;
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
            color: #ff6b6b;
            font-size: 12px;
            margin-top: 5px;
            display: block;
        }

        /* Loading Animation Overlay for Login Page */
        .login-loading-overlay {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: linear-gradient(135deg, #273A28 0%, #2c3e50 100%);
            z-index: 9999;
            display: none;
            align-items: center;
            justify-content: center;
        }

        .login-loading-overlay.show {
            display: flex;
        }

        .login-animation-content {
            position: relative;
            width: 100%;
            max-width: 400px;
            height: 350px;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
        }

        /* Logo Animation */
        .login-logo-container {
            position: relative;
            text-align: center;
            animation: logoEntrance 1s ease-out;
            z-index: 10;
        }

        @keyframes logoEntrance {
            0% {
                transform: scale(0) rotate(-180deg);
                opacity: 0;
            }
            60% {
                transform: scale(1.1) rotate(10deg);
            }
            100% {
                transform: scale(1) rotate(0deg);
                opacity: 1;
            }
        }

        .login-sikkhaloy-logo {
            width: 140px;
            height: auto;
            filter: drop-shadow(0 10px 30px rgba(0, 0, 0, 0.3));
            animation: logoPulse 2s ease-in-out infinite;
        }

        @keyframes logoPulse {
            0%, 100% {
                transform: scale(1);
                filter: drop-shadow(0 10px 30px rgba(0, 0, 0, 0.3));
            }
            50% {
                transform: scale(1.05);
                filter: drop-shadow(0 15px 40px rgba(0, 0, 0, 0.4));
            }
        }

        /* Circular Loading Ring */
        .login-loading-ring {
            position: absolute;
            width: 250px;
            height: 250px;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }

        .login-loading-ring-circle {
            position: absolute;
            width: 100%;
            height: 100%;
            border-radius: 50%;
            border: 4px solid rgba(255, 255, 255, 0.2);
            border-top-color: #fff;
            animation: ringRotate 1.5s linear infinite;
            top: 0;
            left: 0;
        }

        @keyframes ringRotate {
            0% {
                transform: rotate(0deg);
            }
            100% {
                transform: rotate(360deg);
            }
        }

        /* Floating Particles */
        .login-particle {
            position: absolute;
            width: 8px;
            height: 8px;
            background: rgba(255, 255, 255, 0.6);
            border-radius: 50%;
            animation: particleFloat 3s ease-in-out infinite;
        }

        .login-particle:nth-child(1) {
            left: 10%;
            top: 20%;
            animation-delay: 0s;
        }

        .login-particle:nth-child(2) {
            left: 85%;
            top: 30%;
            animation-delay: 0.5s;
        }

        .login-particle:nth-child(3) {
            left: 20%;
            top: 70%;
            animation-delay: 1s;
        }

        .login-particle:nth-child(4) {
            left: 80%;
            top: 65%;
            animation-delay: 1.5s;
        }

        .login-particle:nth-child(5) {
            left: 50%;
            top: 15%;
            animation-delay: 2s;
        }

        @keyframes particleFloat {
            0%, 100% {
                transform: translateY(0) scale(1);
                opacity: 0.6;
            }
            50% {
                transform: translateY(-20px) scale(1.2);
                opacity: 1;
            }
        }

        /* Progress Bar */
        .login-progress-bar-container {
            position: relative;
            width: 160px;
            height: 4px;
            background: rgba(255, 255, 255, 0.2);
            border-radius: 2px;
            overflow: hidden;
            margin-top: 80px;
            z-index: 10;
        }

        .login-progress-bar-fill {
            height: 100%;
            background: linear-gradient(90deg, #fff, #ffeb3b, #fff);
            background-size: 200% 100%;
            animation: progressMove 1.5s ease-in-out infinite;
            border-radius: 2px;
        }

        @keyframes progressMove {
            0% {
                width: 0%;
                background-position: 0% 0%;
            }
            50% {
                width: 70%;
                background-position: 100% 0%;
            }
            100% {
                width: 100%;
                background-position: 200% 0%;
            }
        }

        /* Loading Text */
        .login-loading-text {
            position: relative;
            text-align: center;
            color: #fff;
            font-size: 16px;
            font-weight: 700;
            letter-spacing: 1px;
            text-shadow: 0 2px 10px rgba(0, 0, 0, 0.5);
            animation: textFade 1.5s ease-in-out infinite;
            margin-top: 15px;
            z-index: 10;
        }

        @keyframes textFade {
            0%, 100% { 
                opacity: 1;
            }
            50% { 
                opacity: 0.6;
            }
        }

        /* Loading Dots */
        .login-loading-dots {
            display: inline-block;
            margin-left: 5px;
        }

        .login-loading-dots span {
            display: inline-block;
            width: 6px;
            height: 6px;
            margin: 0 2px;
            background: #fff;
            border-radius: 50%;
            animation: dotBounce 1.4s ease-in-out infinite;
        }

        .login-loading-dots span:nth-child(1) {
            animation-delay: 0s;
        }

        .login-loading-dots span:nth-child(2) {
            animation-delay: 0.2s;
        }

        .login-loading-dots span:nth-child(3) {
            animation-delay: 0.4s;
        }

        @keyframes dotBounce {
            0%, 80%, 100% {
                transform: scale(0.8);
                opacity: 0.5;
            }
            40% {
                transform: scale(1.2);
                opacity: 1;
            }
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

            .login-sikkhaloy-logo {
                width: 100px;
            }
            
            .login-loading-ring {
                width: 180px;
                height: 180px;
                padding: 15px;
            }
            
            .login-progress-bar-container {
                width: 120px;
                margin-top: 60px;
            }
            
            .login-loading-text {
                font-size: 14px;
                margin-top: 12px;
            }
            
            .login-loading-dots span {
                width: 5px;
                height: 5px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <!-- Loading Animation Overlay -->
    <div id="loginPageLoadingOverlay" class="login-loading-overlay">
        <div class="login-animation-content">
            <!-- Floating Particles -->
            <div class="login-particle"></div>
            <div class="login-particle"></div>
            <div class="login-particle"></div>
            <div class="login-particle"></div>
            <div class="login-particle"></div>
            
            <!-- Loading Ring with Logo, Progress Bar and Text inside -->
            <div class="login-loading-ring">
                <div class="login-loading-ring-circle"></div>
                
                <!-- Logo Container -->
                <div class="login-logo-container">
                    <img src="/CSS/Image/SikkhaloyLogo.png" alt="Sikkhaloy Logo" class="login-sikkhaloy-logo" />
                </div>
                
                <!-- Progress Bar inside the ring -->
                <div class="login-progress-bar-container">
                    <div class="login-progress-bar-fill"></div>
                </div>
                
                <!-- Loading Text inside the ring -->
                <div class="login-loading-text">
                    লগইন হচ্ছে<span class="login-loading-dots"><span></span><span></span><span></span></span>
                </div>
            </div>
        </div>
    </div>

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
                            <asp:Button ID="LoginButton" runat="server" CommandName="Login" class="btn-login" Text="লগইন করুন" ValidationGroup="Login2" OnClientClick="showLoginPageAnimation(); return true;" />
                        </div>
      
                        <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                    </LayoutTemplate>
                </asp:Login>
        
                <asp:Label ID="InvalidErrorLabel" runat="server" CssClass="error-message"></asp:Label>
            </div>
        </div>
    </div>

    <script>
        function showLoginPageAnimation() {
            document.getElementById('loginPageLoadingOverlay').classList.add('show');
        }

        function hideLoginPageAnimation() {
            document.getElementById('loginPageLoadingOverlay').classList.remove('show');
        }

        // Hide animation on page load if there's an error
        $(function() {
            var errorLabel = $('[id*=InvalidErrorLabel]').text();
            var failureText = $('[id*=FailureText]').text();
            
            if ((errorLabel && errorLabel.length > 0) || (failureText && failureText.length > 0)) {
                hideLoginPageAnimation();
            }

            // Handle UpdatePanel postback completion
            var prm = Sys.WebForms.PageRequestManager.getInstance();
            if (prm) {
                prm.add_endRequest(function() {
                    var errorLabel = $('[id*=InvalidErrorLabel]').text();
                    var failureText = $('[id*=FailureText]').text();
                    
                    if ((errorLabel && errorLabel.length > 0) || (failureText && failureText.length > 0)) {
                        hideLoginPageAnimation();
                    }
                });
            }
        });
    </script>
</asp:Content>
