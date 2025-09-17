<%@ Page Title="" Language="C#" MasterPageFile="~/Design.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="EDUCATION.COM.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Default.css?v=1.5" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="view" style="background-image: url('/Home/Features/bg.png'); background-repeat: no-repeat; background-position: center center; /*background-size: cover; */">
        <div class="mask rgba-gradient d-flex justify-content-center align-items-center">
            <div class="container">
                <div class="row">
                    <div class="col-md-6 text-center text-md-left wow fadeInLeft mt-xl-5 mb-5" data-wow-delay="0.3s">
                        <h1 class="h1-responsive font-weight-bold mt-sm-5 mb-0"><strong>Sikkhaloy.com</strong></h1>
                        <p><strong>Educational institution management service</strong></p>
                        <hr />
                        <h6 class="mb-4 text-justify black-text">Sikkhaloy.com is an educational institution management service to help institutes for save their time and improve management system. It is a simple yet powerful one point integrated platform that connects all the departments with its technologically advanced features and software modules such as Daily attendance which integrated with RFID and fingerprint device with automated SMS notification system, User friendly student fees & payroll management system, fully customable exam & results with graphical explanation and many more solutions.</h6>
                        <a href="#Contact" class="btn btn-white link-b">Contact Us</a>
                    </div>
                    <div class="col-md-6 wow fadeInRight col-xl-5 mt-xl-5" data-wow-delay="0.3s">
                        <img src="/Home/Features/Intro.png" alt="" class="img-fluid" />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="wow fadeIn py-5" data-wow-delay="0.5s">
        <div class="flex-center">
            <div class="container text-center">
                <asp:FormView ID="CountFormView" Width="100%" runat="server" DataSourceID="CountSQL">
                    <ItemTemplate>
                        <div class="row">
                            <div class="col-md-4 counter">
                                <i class="fa fa-building" aria-hidden="true"></i>
                                <div class="number"><%# Eval("Total_Institution") %></div>
                                <p>Institutions</p>
                            </div>
                            <div class="col-md-4 counter">
                                <i class="fa fa-users" aria-hidden="true"></i>
                                <div class="number"><%# Eval("Total_Student") %></div>
                                <p>Students</p>
                            </div>
                            <div class="col-md-4 counter">
                                <i class="fa fa-user" aria-hidden="true"></i>
                                <div class="number"><%# Eval("Total_Teacher") %></div>
                                <p>Teachers</p>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="CountSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT (SELECT COUNT(SchoolID) FROM SchoolInfo)AS Total_Institution,(SELECT COUNT(StudentID)  FROM Student)AS Total_Student, (SELECT COUNT(EmployeeID)  FROM Employee_Info)AS Total_Teacher"></asp:SqlDataSource>
            </div>
        </div>
    </div>

    <div class="bg-features wow fadeIn" data-wow-delay="0.5s">
        <div class="container">
            <div class="row">
                <div class="col-lg-5 col-md-12 mb-3 wow fadeIn">
                    <img src="Home/Features/Attendance.png" class="img-fluid" alt="Digital Attendance System" />
                </div>
                <div class="col-lg-6 ml-auto col-md-12 wow fadeIn">
                    <h3 class="h3 mb-3"><i class="fa fa-bell" aria-hidden="true"></i>
                        Digital Attendance System</h3>

                    <ul class="List-Fetures">
                        <li>Attendance Display System</li>
                        <li>Student, Teacher & Staff Report</li>
                        <li>Entry SMS Confirmation</li>
                        <li>Exit SMS Confirmation</li>
                        <li>Late SMS</li>
                        <li>Absent And Bunk SMS</li>
                        <li>Manage Schedule</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <div class="non-features wow fadeIn" data-wow-delay="0.5s">
        <div class="container">
            <div class="row">
                <div class="col-md-6">
                    <h3 class="h3 mb-3"><i class="fa fa-book" aria-hidden="true"></i>
                        Exam Management</h3>
                    <ul class="List-Fetures">
                        <li>Flexible Setting</li>
                        <li>Easy to Modify</li>
                        <li>Result By SMS</li>
                        <li>Smart Result Card</li>
                    </ul>
                </div>
                <div class="col-md-6">
                    <img src="Home/Features/Exam.png" class="img-fluid" alt="Exam Management" />
                </div>

            </div>
        </div>
    </div>

    <div class="bg-features wow fadeIn" data-wow-delay="0.5s">
        <div class="container">
            <div class="row">
                <div class="col-md-6">
                    <img src="Home/Features/Accounts.png" class="img-fluid" alt="Accounts Management" />
                </div>
                <div class="col-md-6">
                    <h3 class="h3 mb-3"><i class="fa fa-money" aria-hidden="true"></i>
                        Accounts Management</h3>
                    <ul class="List-Fetures">
                        <li>Easy Step Payment Collection</li>
                        <li>Income & Expense Report</li>
                        <li>Accounts Summary</li>
                        <li>Daily,Monthly,Yearly Analysis</li>
                        <li>Paid Notification by SMS</li>
                        <li>Due Notification by SMS</li>
                        <li>Online Money Receipt</li>
                    </ul>
                </div>
            </div>
        </div>
    </div>

    <div class="non-features wow fadeIn" data-wow-delay="0.5s">
        <div class="container">
            <div class="row">
                <div class="col-md-6">
                    <h3 class="h3 mb-3"><i class="fa fa-address-card" aria-hidden="true"></i>
                        Student Report</h3>
                    <ul class="List-Fetures">
                        <li>Attendance Activities</li>
                        <li>Exam Report</li>
                        <li>Accounts Activities</li>
                        <li>Reporting</li>
                    </ul>
                </div>
                <div class="col-md-6">
                    <img src="Home/Features/StudentReport.png" class="img-fluid" alt="Student Report" />
                </div>
            </div>
        </div>
    </div>

    <div class="bg-features wow fadeIn" data-wow-delay="0.5s">
        <div class="container">
            <div id="quote-carousel" class="carousel slide carousel-fade" data-interval="10000" data-ride="carousel">
                <div class="carousel-inner text-center" role="listbox">
                    <asp:Repeater ID="TestimonialRepeater" DataSourceID="TestimonialSQL" runat="server">
                        <ItemTemplate>
                            <div class="carousel-item">
                                <blockquote>
                                    <img class="rounded-circle Testimonial img-thumbnail" src="Handeler/Admin_Photo.ashx?Img=<%# Eval("AdminID") %>" />
                                    <p><%# Eval("Testimonial_Text") %></p>
                                    <small><%# Eval("FirstName") %> <%# Eval("LastName") %></small>
                                    <b class="d-block"><%# Eval("SchoolName") %></b>
                                </blockquote>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:SqlDataSource ID="TestimonialSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Public_Testimonial.Testimonial_Text, Admin.FirstName, Admin.LastName, Admin.AdminID, SchoolInfo.SchoolName FROM Public_Testimonial INNER JOIN Admin ON Public_Testimonial.RegistrationID = Admin.RegistrationID INNER JOIN SchoolInfo ON Public_Testimonial.SchoolID = SchoolInfo.SchoolID WHERE (Public_Testimonial.Is_Show = 1) ORDER BY Public_Testimonial.Show_SN"></asp:SqlDataSource>
                </div>

                <a class="carousel-control-prev" href="#quote-carousel" role="button" data-slide="prev">
                    <span class="carousel-control-prev-icon"></span>
                    <span class="sr-only">Previous</span>
                </a>
                <a class="carousel-control-next" href="#quote-carousel" role="button" data-slide="next">
                    <span class="carousel-control-next-icon"></span>
                    <span class="sr-only">Next</span>
                </a>
            </div>
        </div>
    </div>


    <section id="Contact" class="mt-5">
        <div class="container-fluid">
            <div class="row">
                <div class="col-md-12">
                    <div class="card pb-5">
                        <div class="card-body">
                            <iframe class="z-depth-1" style="height: 400px; width: 100%; border: none; position: relative; overflow: hidden;" src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3651.3167032247748!2d90.39618371538356!3d23.77173408457886!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3755c7667ffb04b3%3A0x4c6a302f46319275!2sLoops+IT!5e0!3m2!1sen!2sbd!4v1521971087576"></iframe>
                            <div class="container">
                                <section class="section">
                                    <h2 class="section-heading font-weight-bold h1 pt-4">Contact us</h2>
                                    <asp:Label ID="MsgLabel" runat="server" Font-Bold="True" ForeColor="#00CC00"></asp:Label>

                                    <div class="row pt-5">
                                        <div class="col-md-8 col-xl-9">
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="md-form">
                                                        <div class="md-form">
                                                            <asp:TextBox ID="NameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                                            <label for="body_NameTextBox">Your name<asp:RequiredFieldValidator ControlToValidate="NameTextBox" ValidationGroup="1" ID="Rv1" runat="server" ErrorMessage="*" ForeColor="Red" /></label>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                    <div class="md-form">
                                                        <div class="md-form">
                                                            <asp:TextBox ID="EmailTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                                            <label for="body_EmailTextBox">
                                                                Your email
                                                                <asp:RegularExpressionValidator ControlToValidate="EmailTextBox" ValidationGroup="1" ID="ERex" runat="server" ErrorMessage="Invalid" ForeColor="Red" SetFocusOnError="True" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                                                            </label>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>

                                            <div class="row">
                                                <div class="col-md-6">
                                                    <div class="md-form">
                                                        <asp:TextBox ID="SubjectTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                                        <label for="body_SubjectTextBox">
                                                            Subject
                                                            <asp:RequiredFieldValidator ControlToValidate="SubjectTextBox" ValidationGroup="1" ID="ReldVal1" runat="server" ErrorMessage="*" ForeColor="Red" /></label>
                                                    </div>
                                                </div>
                                                <div class="col-md-6">
                                                    <div class="md-form">
                                                        <asp:TextBox ID="MobileTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                                        <label for="body_MobileTextBox">
                                                            Mobile number
                                                            <asp:RegularExpressionValidator ControlToValidate="MobileTextBox" ValidationGroup="1" ID="RegularExpressionValidator1" runat="server" ErrorMessage="Invalid" ForeColor="Red" SetFocusOnError="True" ValidationExpression="\+?(88)?0?1[3456789][0-9]{8}\b"></asp:RegularExpressionValidator>
                                                            <asp:RequiredFieldValidator ControlToValidate="MobileTextBox" ValidationGroup="1" ID="Rv3" runat="server" ErrorMessage="*" ForeColor="Red" /></label>
                                                    </div>
                                                </div>
                                            </div>

                                            <div class="row">
                                                <div class="col-md-12">
                                                    <div class="md-form">
                                                        <asp:TextBox ID="MessageTextBox" runat="server" CssClass="md-textarea form-control" MaxLength="4000" Rows="3" TextMode="MultiLine"></asp:TextBox>
                                                        <label for="body_MessageTextBox">Your message<asp:RequiredFieldValidator ControlToValidate="MessageTextBox" ValidationGroup="1" ID="Rv4" runat="server" ErrorMessage="*" ForeColor="Red" /></label>
                                                    </div>
                                                </div>
                                            </div>

                                            <div class="text-center text-md-left my-4">
                                                <asp:TextBox ID="txtCaptcha" runat="server" Style="display: none" />
                                                <asp:RequiredFieldValidator ID="rfvCaptcha" ErrorMessage="Validation is required." ControlToValidate="txtCaptcha" runat="server" ForeColor="Red" Display="Dynamic" ValidationGroup="1"/>
                                                <asp:Button ID="SendButton" ValidationGroup="1" runat="server" Text="Send" CssClass="btn btn-success waves-effect waves-light" OnClick="SendButton_Click" />
                                                <asp:SqlDataSource ID="ContactSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Public_Contact_US(Name, Email, MobileNo, Subject, Message) VALUES (@Name, @Email, @MobileNo, @Subject, @Message)" SelectCommand="SELECT * FROM [Public_Contact_US]">
                                                    <InsertParameters>
                                                        <asp:ControlParameter ControlID="NameTextBox" Name="Name" PropertyName="Text" Type="String" />
                                                        <asp:ControlParameter ControlID="EmailTextBox" Name="Email" PropertyName="Text" Type="String" />
                                                        <asp:ControlParameter ControlID="MobileTextBox" Name="MobileNo" PropertyName="Text" Type="String" />
                                                        <asp:ControlParameter ControlID="SubjectTextBox" Name="Subject" PropertyName="Text" />
                                                        <asp:ControlParameter ControlID="MessageTextBox" Name="Message" PropertyName="Text" Type="String" />
                                                    </InsertParameters>
                                                </asp:SqlDataSource>
                                            </div>
                                        </div>
                                        <div class="col-md-4 col-xl-3">
                                            <ul class="contact-icons list-unstyled">
                                                <li><i class="fa fa-building-o fa-2x green-text"></i>
                                                    <p><b>Loops IT Ltd.</b></p>
                                                </li>

                                                <li><i class="fa fa-map-marker fa-2x green-text"></i>
                                                    <p>328, East Nakhal Para, Tejgaon, Dhaka 1215</p>
                                                </li>

                                                <li><i class="fa fa-phone fa-2x green-text"></i>
                                                    <p class="mb-0">09638 66 99 66</p>
                                                    <p>+88 01739144141</p>
                                                </li>

                                                <li><i class="fa fa-envelope fa-2x green-text"></i>
                                                    <p>info@loopsit.com</p>
                                                </li>
                                                <li><i class="fa fa-globe fa-2x green-text"></i>
                                                    <p>
                                                        <a class="black-text" href="http://loopsit.com/" target="_blank">www.loopsit.com</a>
                                                    </p>
                                                </li>
                                                <li>
                                                    <a target="_blank" href='https://play.google.com/store/apps/details?id=com.loopsit.sikkhaloy&hl=en&pcampaignid=MKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1'>
                                                        <img style="width: 165px;" alt='Get it on Google Play' src='https://play.google.com/intl/en_us/badges/images/generic/en_badge_web_generic.png' />
                                                        <img style="width: 165px;" src="CSS/Image/googlePlay.png" />
                                                    </a>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>
                                </section>
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    </section>

    <script src='https://www.google.com/recaptcha/api.js?render=6LdXPY0UAAAAAHg7W3SLGr_MQt7wRvV0HLZ8JTBi'></script>
    <script>
        //Google recapcha
        grecaptcha.ready(function () {
            grecaptcha.execute('6LdXPY0UAAAAAHg7W3SLGr_MQt7wRvV0HLZ8JTBi', { action: 'homepage' })
            .then(function (token) {
                $.ajax({
                    type: "POST",
                    url: "Default.aspx/VerifyCaptcha",
                    data: "{response: '" + token + "'}",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (r) {
                        var captchaResponse = jQuery.parseJSON(r.d);
                        var success = captchaResponse.success;
                        var score = captchaResponse.score;

                        if (success == true && score >= 0.5) {
                            $("[id*=txtCaptcha]").val(success);
                            $("[id*=rfvCaptcha]").hide();
                        } else {
                            $("[id*=txtCaptcha]").val("");
                            $("[id*=rfvCaptcha]").show();
                            var error = captchaResponse["error-codes"][0];
                            $("[id*=rfvCaptcha]").html("RECaptcha error. " + error);
                        }
                    }
                });
            });
        });


        $(function () {
            $('.link-b').on('click', function (e) {
                e.preventDefault();

                $('html, body').animate({
                    scrollTop: $($(this).attr('href')).offset().top
                }, 800);
            });


            $('#quote-carousel').find('.carousel-item').first().addClass('active');

            $('.number').each(function () {
                $(this).prop('Counter', 0).animate({
                    Counter: $(this).text()
                }, {
                    duration: 4000,
                    easing: 'swing',
                    step: function (now) {
                        $(this).text(Math.ceil(now));
                    }
                });
            });
        });
    </script>
</asp:Content>
