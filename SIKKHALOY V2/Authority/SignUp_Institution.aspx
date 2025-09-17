<%@ Page Title="Sign Up Institution" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="SignUp_Institution.aspx.cs" Inherits="EDUCATION.COM.Authority.SignUp_Institution" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

        <h3>Sign Up Institution</h3>

        <asp:CreateUserWizard ID="InstitutionCW" runat="server" LoginCreatedUser="False" OnCreatedUser="InstitutionCW_CreatedUser" Width="100%">
            <WizardSteps>
                <asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server">
                    <ContentTemplate>
                        <div class="row">
                            <div class="col-lg-8">
                                <div class="card">
                                    <div class="card-body">
                                        <div class="form-group">
                                            <label>
                                                User Name
                            <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" CssClass="EroorStar" ErrorMessage="User Name is required." ToolTip="User Name is required." ValidationGroup="InstitutionCW">!</asp:RequiredFieldValidator>
                                                <asp:RegularExpressionValidator Display="Dynamic" ControlToValidate="UserName" ID="RegularExpressionValidator3" ValidationExpression="^[\s\S]{8,30}$" runat="server" ErrorMessage="Minimum 8 and Maximum 30 characters required." CssClass="EroorSummer" ValidationGroup="InstitutionCW" />
                                            </label>
                                            <asp:TextBox ID="UserName" runat="server" CssClass="form-control"></asp:TextBox>
                                        </div>
                                        <div class="form-group">
                                            <label>
                                                Password
                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" CssClass="EroorStar" ErrorMessage="Password is required." ToolTip="Password is required." ValidationGroup="InstitutionCW">!</asp:RequiredFieldValidator>
                                                <asp:RegularExpressionValidator ID="RegularExpressionValidator4" runat="server" ControlToValidate="Password" CssClass="EroorSummer" Display="Dynamic" ErrorMessage="Minimum 8 and Maximum 30 characters required." ValidationExpression="^[\s\S]{8,30}$" ValidationGroup="InstitutionCW" />
                                            </label>
                                            <asp:TextBox ID="Password" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                        </div>
                                        <div class="form-group">
                                            <label>
                                                Confirm Password
                            <asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" ControlToValidate="ConfirmPassword" CssClass="EroorStar" ErrorMessage="Confirm Password is required." ToolTip="Confirm Password is required." ValidationGroup="InstitutionCW" />
                                            </label>
                                            <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                        </div>
                                        <div class="form-group">
                                            <label>
                                                E-mail
                            <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email" CssClass="EroorStar" ErrorMessage="E-mail is required." ToolTip="E-mail is required." ValidationGroup="InstitutionCW" />
                                            </label>
                                            <asp:TextBox ID="Email" runat="server" CssClass="form-control"></asp:TextBox>
                                        </div>
                                        <div class="form-group">
                                            <label>
                                                Security Question
                            <asp:RequiredFieldValidator ID="QuestionRequired" runat="server" ControlToValidate="Question" CssClass="EroorStar" ErrorMessage="Security question is required." ToolTip="Security question is required." ValidationGroup="InstitutionCW" />
                                            </label>
                                            <asp:TextBox ID="Question" runat="server" CssClass="form-control"></asp:TextBox>
                                        </div>
                                        <div class="form-group">
                                            <label>
                                                Security Answer
                            <asp:RequiredFieldValidator ID="AnswerRequired" runat="server" ControlToValidate="Answer" CssClass="EroorStar" ErrorMessage="Security answer is required." ToolTip="Security answer is required." ValidationGroup="InstitutionCW" />
                                            </label>
                                            <asp:TextBox ID="Answer" runat="server" CssClass="form-control"></asp:TextBox>
                                        </div>

                                        <asp:CompareValidator ID="PasswordCompare" runat="server" ControlToCompare="Password" ControlToValidate="ConfirmPassword" CssClass="EroorStar" Display="Dynamic" ErrorMessage="The Password and Confirmation Password must match." ValidationGroup="InstitutionCW" />
                                        <asp:Literal ID="ErrorMessage" runat="server" EnableViewState="False"></asp:Literal>

                                        <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" CssClass="btn btn-success" Text="Create User id" ValidationGroup="InstitutionCW" />
                                    </div>
                                </div>
                            </div>

                        </div>
                    </ContentTemplate>
                    <CustomNavigationTemplate>
                    </CustomNavigationTemplate>
                </asp:CreateUserWizardStep>

                <asp:WizardStep ID="InstitutionInfo" runat="server" Title="Institution Info">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="card">
                                <div class="card-body">
                                    <div class="form-group">
                                        <label>
                                            Per Student tk
                      <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="PerStudentTkTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="SS"></asp:RequiredFieldValidator>
                                        </label>
                                        <asp:TextBox ID="PerStudentTkTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>
                                            Institution Name
                      <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SchoolNameTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="SS"></asp:RequiredFieldValidator>
                                        </label>
                                        <asp:TextBox ID="SchoolNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Established</label>
                                        <asp:TextBox ID="EstablishedTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Principal</label>
                                        <asp:TextBox ID="PrincipalTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Academic Staff</label>
                                        <asp:TextBox ID="AcadamicStaffTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Students</label>
                                        <asp:TextBox ID="StudentsTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>
                                            City
                       <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="CityTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="SS"></asp:RequiredFieldValidator>
                                        </label>
                                        <asp:TextBox ID="CityTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>State</label>
                                        <asp:TextBox ID="StateTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Local Area</label>
                                        <asp:TextBox ID="LocalAreaTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Postal Code</label>
                                        <asp:TextBox ID="PostalCodeTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Phone </label>
                                        <asp:TextBox ID="Phone1TextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Email</label>

                                        <asp:TextBox ID="EmailTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Website</label>
                                        <asp:TextBox ID="WebsiteTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Address</label>
                                        <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label>Logo</label>
                                        <asp:FileUpload ID="LogoUpload" runat="server" />
                                    </div>
                                    <div class="form-group">
                                        <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-success" Text="Submit" OnClick="SubmitButton_Click" ValidationGroup="SS" />

                                        <asp:SqlDataSource ID="SchoolInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO SchoolInfo(SchoolName, SchoolLogo, Established, Principal, AcadamicStaff, Students, Address, City, State, LocalArea, PostalCode, Phone, Email, Website, UserName, Validation, Date, School_SN, Per_Student_Rate) VALUES (@SchoolName, @SchoolLogo, @Established, @Principal, @AcadamicStaff, @Students, @Address, @City, @State, @LocalArea, @PostalCode, @Phone, @Email, @Website, @UserName, @Validation, GETDATE(), dbo.Institution_SerialNumber(), @Per_Student_Rate)" SelectCommand="SELECT * FROM [SchoolInfo]">
                                            <InsertParameters>
                                                <asp:ControlParameter ControlID="SchoolNameTextBox" Name="SchoolName" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="LogoUpload" Name="SchoolLogo" PropertyName="FileBytes" />
                                                <asp:ControlParameter ControlID="EstablishedTextBox" Name="Established" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="PrincipalTextBox" Name="Principal" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="AcadamicStaffTextBox" Name="AcadamicStaff" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="StudentsTextBox" Name="Students" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="AddressTextBox" Name="Address" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="CityTextBox" Name="City" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="StateTextBox" Name="State" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="LocalAreaTextBox" Name="LocalArea" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="PostalCodeTextBox" Name="PostalCode" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="Phone1TextBox" Name="Phone" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="EmailTextBox" Name="Email" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="WebsiteTextBox" Name="Website" PropertyName="Text" Type="String" />
                                                <asp:ControlParameter ControlID="PerStudentTkTextBox" DefaultValue="" Name="Per_Student_Rate" PropertyName="Text" />
                                                <asp:Parameter Name="Validation" DefaultValue="Valid" />
                                                <asp:Parameter Name="UserName" Type="String" />
                                            </InsertParameters>
                                        </asp:SqlDataSource>
                                        <asp:SqlDataSource ID="RegistrationSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Registration(SchoolID, UserName, Validation, Category, CreateDate) VALUES ((Select IDENT_CURRENT('SchoolInfo')), @UserName, 'Valid', 'Admin', GETDATE())" SelectCommand="SELECT * FROM [Registration]">
                                            <InsertParameters>
                                                <asp:Parameter DefaultValue="" Name="UserName" Type="String" />
                                            </InsertParameters>
                                        </asp:SqlDataSource>
                                        <asp:SqlDataSource ID="AdminSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                            InsertCommand="INSERT INTO Admin(RegistrationID, SchoolID) VALUES ((Select IDENT_CURRENT('Registration')), (Select IDENT_CURRENT('SchoolInfo')))"
                                            SelectCommand="SELECT * FROM [Admin]"></asp:SqlDataSource>

                                        
                                        <asp:SqlDataSource ID="Edu_YearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                                            InsertCommand="INSERT INTO Education_Year(SchoolID, RegistrationID, EducationYear, Status, StartDate, EndDate, SN) VALUES (@SchoolID, @RegistrationID, YEAR(GETDATE()), @Status, '01-01-' + CONVERT (varchar(4), YEAR(GETDATE())), '12-31-' + CONVERT (varchar(4), YEAR(GETDATE())),[dbo].[F_EducationYear_SN](@SchoolID)) 
                                            INSERT INTO Education_Year_User (EducationYearID, SchoolID, RegistrationID) VALUES (SCOPE_IDENTITY(),@SchoolID, @RegistrationID)"
                                            SelectCommand="SELECT * FROM [Education_Year]">
                                            <InsertParameters>
                                                <asp:Parameter Name="SchoolID" Type="Int32" />
                                                <asp:Parameter Name="RegistrationID" Type="Int32" />
                                                <asp:Parameter DefaultValue="True" Name="Status" Type="String" />
                                            </InsertParameters>
                                        </asp:SqlDataSource>
                                        <asp:SqlDataSource ID="SMS_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [SMS] ([SchoolID], [SMS_Balance], [Masking], [Date]) VALUES (@SchoolID, 0, 'Sikkhaloy', getdate())" SelectCommand="SELECT * FROM [SMS]">
                                            <InsertParameters>
                                                <asp:Parameter Name="SchoolID" Type="Int32" />
                                            </InsertParameters>
                                        </asp:SqlDataSource>
                                        <asp:SqlDataSource ID="LIT_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [AST] ([RegistrationID], [SchoolID], [UserName], [Category], [Password], [PasswordAnswer]) VALUES (@RegistrationID, @SchoolID, @UserName, @Category, @Password, @PasswordAnswer)" SelectCommand="SELECT * FROM [AST]" ProviderName="<%$ ConnectionStrings:EducationConnectionString.ProviderName %>">
                                            <InsertParameters>
                                                <asp:Parameter DefaultValue="" Name="RegistrationID" Type="Int32" />
                                                <asp:Parameter DefaultValue="" Name="SchoolID" Type="Int32" />
                                                <asp:Parameter Name="UserName" Type="String" />
                                                <asp:Parameter DefaultValue="Admin" Name="Category" Type="String" />
                                                <asp:Parameter DefaultValue="" Name="Password" Type="String" />
                                                <asp:Parameter Name="PasswordAnswer" Type="String" />
                                            </InsertParameters>
                                        </asp:SqlDataSource>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </asp:WizardStep>

                <asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
                    <ContentTemplate>
                        <div class="alert alert-success">
                            <div class="form-group">
                                <label>Congratulation! Your account has been successfully created.</label>
                            </div>
                            <div class="form-group">
                                <asp:Button ID="ContinueButton" runat="server" CausesValidation="False" CommandName="Continue" CssClass="btn btn-primary" Text="Continue" ValidationGroup="InstitutionCW" PostBackUrl="Auth_Profile.aspx" />
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:CompleteWizardStep>
            </WizardSteps>
            <FinishNavigationTemplate>
                <asp:Button ID="FinishPreviousButton" runat="server" CausesValidation="False" CommandName="MovePrevious" Text="Previous" Visible="false" />
                <asp:Button ID="FinishButton" runat="server" CommandName="MoveComplete" Text="Finish" Visible="false" />
            </FinishNavigationTemplate>
            <StepNavigationTemplate>
                <asp:Button ID="StepPreviousButton" runat="server" CausesValidation="False" CommandName="MovePrevious" Text="Previous" />
                <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" Text="Next" />
            </StepNavigationTemplate>
        </asp:CreateUserWizard>
</asp:Content>
