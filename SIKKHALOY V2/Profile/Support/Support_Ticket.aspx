<%@ Page Title="Support Ticket" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Support_Ticket.aspx.cs" Inherits="EDUCATION.COM.Profile.Support.Support_Ticket" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" crossorigin="anonymous" />
    
    <style>
        /* Support Contact Section Styles */
        .support-contact-section {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 25px 20px;
            margin: -15px -15px 30px -15px;
            border-radius: 15px;
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.3);
            position: relative;
            overflow: hidden;
            animation: fadeInUp 0.8s ease-out;
        }

        .support-contact-section::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: 
                radial-gradient(circle at 20% 20%, rgba(255, 255, 255, 0.1) 0%, transparent 50%),
                radial-gradient(circle at 80% 80%, rgba(255, 255, 255, 0.1) 0%, transparent 50%);
            pointer-events: none;
        }

        .support-header {
            text-align: center;
            margin-bottom: 25px;
            position: relative;
            z-index: 2;
        }

        .support-header h2 {
            margin: 0;
            font-size: 28px;
            font-weight: 700;
            text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 10px;
        }

        .support-header .header-icon {
            font-size: 32px;
            animation: bounce 2s infinite;
        }

        .support-numbers {
            display: flex;
            justify-content: center;
            gap: 20px;
            flex-wrap: wrap;
            position: relative;
            z-index: 2;
        }

        .contact-card {
            background: rgba(255, 255, 255, 0.15);
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.2);
            border-radius: 15px;
            padding: 20px;
            text-align: center;
            transition: all 0.3s ease;
            position: relative;
            overflow: hidden;
            min-width: 250px;
            animation: slideInUp 0.6s ease-out;
        }

        .contact-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 15px 35px rgba(0, 0, 0, 0.2);
            background: rgba(255, 255, 255, 0.25);
            border-color: rgba(255, 255, 255, 0.4);
        }

        .contact-card::before {
            content: '';
            position: absolute;
            top: 0;
            left: -100%;
            width: 100%;
            height: 100%;
            background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
            transition: left 0.5s ease;
        }

        .contact-card:hover::before {
            left: 100%;
        }

        .contact-icon {
            font-size: 40px;
            margin-bottom: 15px;
            display: block;
            animation: pulse 2s infinite;
        }

        .contact-type {
            font-size: 14px;
            font-weight: 600;
            margin-bottom: 8px;
            opacity: 0.9;
            text-transform: uppercase;
            letter-spacing: 1px;
        }

        .contact-number {
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 10px;
            text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
        }

        .contact-label {
            font-size: 12px;
            opacity: 0.8;
            font-style: italic;
        }

        .call-action {
            margin-top: 15px;
        }

        .call-btn {
            background: rgba(255, 255, 255, 0.2);
            border: 1px solid rgba(255, 255, 255, 0.3);
            color: white;
            padding: 8px 16px;
            border-radius: 25px;
            text-decoration: none;
            font-size: 12px;
            font-weight: 600;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 5px;
        }

        .call-btn:hover {
            background: rgba(255, 255, 255, 0.3);
            color: white;
            text-decoration: none;
            transform: scale(1.05);
        }

        /* Support Ticket Form Section */
        .support-ticket-section {
            background: white;
            padding: 30px;
            border-radius: 15px;
            box-shadow: 0 5px 20px rgba(0, 0, 0, 0.1);
            margin-bottom: 30px;
        }

        .support-ticket-header {
            display: flex;
            align-items: center;
            gap: 10px;
            margin-bottom: 25px;
            padding-bottom: 15px;
            border-bottom: 2px solid #f0f0f0;
        }

        .support-ticket-header h3 {
            margin: 0;
            color: #333;
            font-size: 24px;
            font-weight: 600;
        }

        .ticket-icon {
            font-size: 28px;
            color: #667eea;
        }

        /* Form Enhancements */
        .enhanced-form {
            display: flex;
            flex-direction: column;
            gap: 20px;
            width: 100%;
        }

        .form-row {
            display: flex;
            gap: 15px;
            align-items: flex-start;
            flex-wrap: wrap;
            width: 100%;
        }

        .form-group {
            flex: 1;
            min-width: 200px;
            width: 100%;
        }

        .form-group label {
            font-weight: 600;
            color: #555;
            margin-bottom: 8px;
            display: block;
            width: 100%;
        }

        .form-control {
            border: 2px solid #e1e5e9;
            border-radius: 10px;
            padding: 12px 15px;
            transition: all 0.3s ease;
            width: 100% !important;
            box-sizing: border-box;
            display: block;
        }

        .form-control:focus {
            border-color: #667eea;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
            outline: none;
        }

        /* Specific styling for dropdown */
        select.form-control {
            height: 45px;
            background-color: #fff;
            background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 16 16'%3e%3cpath fill='none' stroke='%23343a40' stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M2 5l6 6 6-6'/%3e%3c/svg%3e");
            background-repeat: no-repeat;
            background-position: right 12px center;
            background-size: 16px 12px;
            padding-right: 40px;
            appearance: none;
            -webkit-appearance: none;
            -moz-appearance: none;
        }

        /* Textarea specific styling */
        textarea.form-control {
            resize: vertical;
            min-height: 100px;
        }

        .btn-primary {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: none;
            padding: 12px 30px;
            border-radius: 25px;
            font-weight: 600;
            transition: all 0.3s ease;
            width: auto;
            min-width: 150px;
        }

        .btn-primary:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
        }

        /* WhatsApp Card Specific Styling */
        .whatsapp-card {
            background: rgba(37, 211, 102, 0.2) !important;
            border-color: rgba(37, 211, 102, 0.3) !important;
        }

        .whatsapp-card:hover {
            background: rgba(37, 211, 102, 0.3) !important;
            border-color: rgba(37, 211, 102, 0.5) !important;
        }

        .whatsapp-card .contact-icon {
            color: #25d366 !important;
        }

        .whatsapp-btn {
            background: rgba(37, 211, 102, 0.3) !important;
            border-color: rgba(37, 211, 102, 0.4) !important;
        }

        .whatsapp-btn:hover {
            background: rgba(37, 211, 102, 0.5) !important;
            border-color: rgba(37, 211, 102, 0.6) !important;
        }

        /* Animations */
        @keyframes fadeInUp {
            from {
                opacity: 0;
                transform: translateY(30px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        @keyframes slideInUp {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        @keyframes bounce {
            0%, 20%, 50%, 80%, 100% {
                transform: translateY(0);
            }
            40% {
                transform: translateY(-10px);
            }
            60% {
                transform: translateY(-5px);
            }
        }

        @keyframes pulse {
            0% {
                transform: scale(1);
            }
            50% {
                transform: scale(1.1);
            }
            100% {
                transform: scale(1);
            }
        }

        /* Responsive Design */
        @media (max-width: 768px) {
            .support-contact-section {
                margin: -15px -30px 30px -30px;
                padding: 30px 15px;
                border-radius: 0;
            }

            .support-numbers {
                flex-direction: column;
                align-items: center;
                gap: 15px;
            }

            .contact-card {
                min-width: 100%;
                max-width: 100%;
                margin: 0;
                animation-delay: 0.2s;
            }

            .contact-card:nth-child(2) {
                animation-delay: 0.4s;
            }

            .contact-card:nth-child(3) {
                animation-delay: 0.6s;
            }

            /* Form specific mobile fixes */
            .form-row {
                flex-direction: column;
                gap: 15px;
                width: 100%;
            }

            .form-group {
                min-width: 100% !important;
                width: 100% !important;
                flex: none;
            }

            .form-control {
                width: 100% !important;
                max-width: 100% !important;
                margin: 0 !important;
                padding: 12px 15px !important;
                font-size: 16px !important; /* Prevents zoom on iOS */
            }

            select.form-control {
                height: 50px !important;
                padding-right: 40px !important;
            }

            textarea.form-control {
                min-height: 120px !important;
            }

            .support-header h2 {
                font-size: 20px;
                flex-direction: column;
                gap: 5px;
            }

            .support-header .header-icon {
                font-size: 28px;
            }

            .contact-number {
                font-size: 18px !important;
            }

            .support-ticket-section {
                margin: 0 -30px 20px -30px;
                border-radius: 0;
                padding: 20px 15px;
            }

            .support-ticket-header {
                padding-bottom: 10px;
            }

            .enhanced-form {
                gap: 15px;
            }

            .btn-primary {
                width: 100% !important;
                min-width: 100% !important;
                padding: 15px 20px !important;
                font-size: 16px !important;
            }
        }

        @media (max-width: 576px) {
            .support-contact-section {
                margin: -15px -20px 20px -20px;
                padding: 25px 10px;
            }

            .contact-card {
                padding: 15px;
                min-height: 180px;
            }

            .contact-icon {
                font-size: 35px !important;
            }

            .contact-number {
                font-size: 16px !important;
            }

            .support-ticket-section {
                margin: 0 -20px 15px -20px;
                padding: 15px 10px;
            }

            .support-ticket-header h3 {
                font-size: 18px;
            }

            /* Extra small screen form fixes */
            .form-control {
                padding: 10px 12px !important;
                border-radius: 8px !important;
            }

            select.form-control {
                height: 48px !important;
                font-size: 15px !important;
            }

            textarea.form-control {
                min-height: 100px !important;
                font-size: 15px !important;
            }

            .form-group label {
                font-size: 14px !important;
                margin-bottom: 6px !important;
            }
        }

        /* Full-width container adjustments */
        @media (max-width: 991px) {
            .container-fluid {
                padding-left: 0 !important;
                padding-right: 0 !important;
            }
            
            /* Ensure all form elements are full width on tablets and below */
            .support-ticket-section .form-control,
            .support-ticket-section select,
            .support-ticket-section textarea {
                width: 100% !important;
                max-width: 100% !important;
                box-sizing: border-box !important;
            }
        }

        /* GridView Enhancement */
        .mGrid {
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <!-- Enhanced Support Contact Section -->
    <div class="support-contact-section">
        <div class="support-header">
            <h2>
                <i class="fas fa-headset header-icon"></i>
                10:00 AM To 5:00 PM Support Available
            </h2>
        </div>
        
        <div class="support-numbers">
            <div class="contact-card">
                <i class="fas fa-phone contact-icon"></i>
                <div class="contact-type">Office Phone</div>
                <div class="contact-number">09638669966</div>
                <div class="contact-label">Business Hours: 10AM - 5PM</div>
                <div class="call-action">
                    <a href="tel:09638669966" class="call-btn">
                        <i class="fas fa-phone-alt"></i>
                        Call Now
                    </a>
                </div>
            </div>
            
            <div class="contact-card">
                <i class="fas fa-mobile-alt contact-icon"></i>
                <div class="contact-type">Mobile Support</div>
                <div class="contact-number">01739144141</div>
                <div class="contact-label">Available 10 AM TO 5 PM</div>
                <div class="call-action">
                    <a href="tel:01739144141" class="call-btn">
                        <i class="fas fa-mobile-alt"></i>
                        Call Mobile
                    </a>
                </div>
            </div>

            <div class="contact-card whatsapp-card">
                <i class="fab fa-whatsapp contact-icon"></i>
                <div class="contact-type">WhatsApp Support</div>
                <div class="contact-number">01739144141</div>
                <div class="contact-label">Available 10 AM TO 5 PM</div>
                <div class="call-action">
                    <a href="https://wa.me/8801739144141?text=Hello,%20I%20need%20support" class="call-btn whatsapp-btn">
                        <i class="fab fa-whatsapp"></i>
                        Chat Now
                    </a>
                </div>
            </div>
        </div>
    </div>

    <!-- Enhanced Support Ticket Section -->
    <div class="support-ticket-section">
        <div class="support-ticket-header">
           
            <h3> <i class="fas fa-ticket-alt ticket-icon"></i> Submit Support Ticket</h3>
        </div>
        
        <div class="enhanced-form">
            <div class="form-row">
                <div class="form-group">
                    <label for="<%=TitleDropDownList.ClientID%>">Subject Category</label>
                    <asp:DropDownList ID="TitleDropDownList" CssClass="form-control" runat="server" AppendDataBoundItems="True" DataSourceID="TitleSQL" DataTextField="Support_Title" DataValueField="SupportTitleID">
                        <asp:ListItem Value="0">[ SELECT SUBJECT ]</asp:ListItem>
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="TitleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Public_Support_Title]"></asp:SqlDataSource>
                    <asp:RequiredFieldValidator ControlToValidate="TitleDropDownList" InitialValue="0" CssClass="EroorStar" ValidationGroup="S" ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
                </div>
            </div>
            
            <div class="form-row">
                <div class="form-group">
                    <label for="<%=MessageTextBox.ClientID%>">Describe Your Issue</label>
                    <asp:TextBox ID="MessageTextBox" Rows="4" placeholder="Please describe your issue in detail..." TextMode="MultiLine" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ControlToValidate="MessageTextBox" CssClass="EroorStar" ValidationGroup="S" ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"></asp:RequiredFieldValidator>
                </div>
            </div>
            
            <div class="form-row">
                <div class="form-group">
                    <asp:Button ID="SubmitButton" ValidationGroup="S" runat="server" Text="Submit Ticket" CssClass="btn btn-primary" OnClick="SubmitButton_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- Support History -->
    <div class="support-ticket-section">
        <div class="support-ticket-header">
           
            <h3> <i class="fas fa-history ticket-icon"></i> Your Support History</h3>
        </div>
        
        <asp:GridView ID="SupportGridView" CssClass="mGrid table table-striped" runat="server" AutoGenerateColumns="False" DataKeyNames="SupportID" DataSourceID="SupportSQL">
            <Columns>
                <asp:BoundField DataField="Support_Title" HeaderText="Subject" SortExpression="Support_Title" />
                <asp:BoundField DataField="Message" HeaderText="Message" SortExpression="Message" />
                <asp:BoundField DataField="Sent_Date" DataFormatString="{0:d MMM yyyy}" HeaderText="Date" SortExpression="Sent_Date" />
            </Columns>
        </asp:GridView>
    </div>
    
    <asp:SqlDataSource ID="SupportSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Public_Support(SchoolID, RegistrationID, SupportTitleID, Message) VALUES (@SchoolID, @RegistrationID, @SupportTitleID, @Message)" SelectCommand="SELECT Public_Support.SupportID, Public_Support.Message, Public_Support.Sent_Date, Public_Support_Title.Support_Title FROM Public_Support INNER JOIN Public_Support_Title ON Public_Support.SupportTitleID = Public_Support_Title.SupportTitleID WHERE (Public_Support.SchoolID = @SchoolID) AND (Public_Support.RegistrationID = @RegistrationID) ORDER BY Public_Support.Sent_Date DESC">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:ControlParameter ControlID="TitleDropDownList" Name="SupportTitleID" PropertyName="SelectedValue" Type="Int32" />
            <asp:ControlParameter ControlID="MessageTextBox" Name="Message" PropertyName="Text" Type="String" />
        </InsertParameters>
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(document).ready(function() {
            // Mobile form optimization
            function optimizeForMobile() {
                if (window.innerWidth <= 768) {
                    // Ensure all form controls are properly sized
                    $('.form-control').css({
                        'width': '100%',
                        'max-width': '100%',
                        'box-sizing': 'border-box'
                    });
                    
                    // Fix dropdown styling on mobile
                    $('select.form-control').css({
                        'width': '100%',
                        'height': '50px',
                        'font-size': '16px'
                    });
                    
                    // Fix textarea on mobile
                    $('textarea.form-control').css({
                        'width': '100%',
                        'min-height': '120px',
                        'font-size': '16px'
                    });
                }
            }

            // Run on page load
            optimizeForMobile();
            
            // Run on window resize
            $(window).resize(function() {
                setTimeout(optimizeForMobile, 100);
            });

            // Add some interactive effects
            $('.contact-card').hover(
                function() {
                    $(this).find('.contact-icon').addClass('animated');
                },
                function() {
                    $(this).find('.contact-icon').removeClass('animated');
                }
            );

            // WhatsApp specific interactions
            $('.whatsapp-card').hover(
                function() {
                    $(this).find('.contact-icon').css('transform', 'scale(1.2) rotate(15deg)');
                },
                function() {
                    $(this).find('.contact-icon').css('transform', 'scale(1) rotate(0deg)');
                }
            );

            // Enhanced WhatsApp button click tracking
            $('.whatsapp-btn').click(function() {
                // Add a small vibration effect if supported
                if (navigator.vibrate) {
                    navigator.vibrate(100);
                }
                
                // Log the click for analytics
                console.log('WhatsApp support clicked');
                
                // Optional: Show a brief loading state
                var $btn = $(this);
                var originalText = $btn.html();
                $btn.html('<i class="fas fa-spinner fa-spin"></i> Opening...');
                
                setTimeout(function() {
                    $btn.html(originalText);
                }, 2000);
            });

            // Enhanced form validation
            $('.form-control').on('focus', function() {
                $(this).parent().addClass('focused');
                $(this).removeClass('error');
            }).on('blur', function() {
                if ($(this).val() === '') {
                    $(this).parent().removeClass('focused');
                }
                
                // Mobile specific: ensure proper width after focus/blur
                if (window.innerWidth <= 768) {
                    $(this).css('width', '100%');
                }
            });

            // Form submission enhancement
            $('#<%=SubmitButton.ClientID%>').click(function(e) {
                var isValid = true;
                
                // Check dropdown
                if ($('#<%=TitleDropDownList.ClientID%>').val() === '0') {
                    $('#<%=TitleDropDownList.ClientID%>').addClass('error');
                    isValid = false;
                }
                
                // Check message
                if ($('#<%=MessageTextBox.ClientID%>').val().trim() === '') {
                    $('#<%=MessageTextBox.ClientID%>').addClass('error');
                    isValid = false;
                }
                
                if (!isValid) {
                    e.preventDefault();
                    showErrorMessage();
                }
            });

            // Success message for form submission
            if (window.location.hash === '#success') {
                showSuccessMessage();
            }

            // Responsive behavior for mobile
            function handleMobileLayout() {
                if (window.innerWidth <= 768) {
                    $('.support-numbers').addClass('mobile-layout');
                    $('.contact-card').addClass('full-width-mobile');
                    
                    // Force form elements to be full width
                    $('.form-control').each(function() {
                        $(this).css({
                            'width': '100%',
                            'max-width': '100%'
                        });
                    });
                } else {
                    $('.support-numbers').removeClass('mobile-layout');
                    $('.contact-card').removeClass('full-width-mobile');
                }
            }

            // Initial check and resize handler
            handleMobileLayout();
            $(window).resize(handleMobileLayout);

            // Add stagger animation for cards on page load
            $('.contact-card').each(function(index) {
                $(this).css('animation-delay', (index * 0.2) + 's');
            });

            // Prevent iOS zoom on form focus
            if (/iPad|iPhone|iPod/.test(navigator.userAgent)) {
                $('.form-control').css('font-size', '16px');
            }
        });

        function showSuccessMessage() {
            var successAlert = $('<div class="alert alert-success alert-dismissible fade show" role="alert" style="margin: 20px 0;">' +
                '<i class="fas fa-check-circle"></i> Your support ticket has been submitted successfully!' +
                '<button type="button" class="close" data-dismiss="alert" aria-label="Close">' +
                '<span aria-hidden="true">&times;</span></button></div>');
            
            $('.support-ticket-section').first().prepend(successAlert);
            
            setTimeout(function() {
                successAlert.alert('close');
            }, 5000);
        }

        function showErrorMessage() {
            var errorAlert = $('<div class="alert alert-danger alert-dismissible fade show" role="alert" style="margin: 20px 0;">' +
                '<i class="fas fa-exclamation-triangle"></i> Please fill in all required fields!' +
                '<button type="button" class="close" data-dismiss="alert" aria-label="Close">' +
                '<span aria-hidden="true">&times;</span></button></div>');
            
            $('.support-ticket-section').first().prepend(errorAlert);
            
            setTimeout(function() {
                errorAlert.alert('close');
            }, 3000);
        }

        // Add some CSS classes dynamically for mobile
        function addMobileStyles() {
            if (!document.getElementById('mobile-dynamic-styles')) {
                var mobileStyles = document.createElement('style');
                mobileStyles.id = 'mobile-dynamic-styles';
                mobileStyles.innerHTML = `
                    .full-width-mobile {
                        animation: slideInLeft 0.6s ease-out;
                    }
                    
                    .error {
                        border-color: #dc3545 !important;
                        box-shadow: 0 0 0 3px rgba(220, 53, 69, 0.1) !important;
                    }
                    
                    @keyframes slideInLeft {
                        from {
                            opacity: 0;
                            transform: translateX(-50px);
                        }
                        to {
                            opacity: 1;
                            transform: translateX(0);
                        }
                    }
                    
                    /* Additional mobile form fixes */
                    @media (max-width: 768px) {
                        .form-control, 
                        .form-control:focus,
                        .form-control:active {
                            width: 100% !important;
                            max-width: 100% !important;
                            box-sizing: border-box !important;
                        }
                    }
                `;
                document.head.appendChild(mobileStyles);
            }
        }

        // Initialize mobile styles
        addMobileStyles();
    </script>
</asp:Content>
