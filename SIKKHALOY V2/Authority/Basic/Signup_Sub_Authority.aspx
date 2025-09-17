<%@ Page Title="Signup Sub-Authority" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Signup_Sub_Authority.aspx.cs" Inherits="EDUCATION.COM.Authority.Basic.Signup_Sub_Authority" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>New Registration For Sub-Authority</h3>

    <asp:CreateUserWizard ID="SubAdminCreateUserWizard" runat="server" OnCreatedUser="SubAdminCreateUserWizard_CreatedUser" LoginCreatedUser="False" Width="100%">
        <WizardSteps>
            <asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server">
                <ContentTemplate>
                    <div class="row">
                        <div class="col-lg-6">
                            <div class="card">
                                <div class="card-body">
                                    <div class="form-group">
                                        <label>Name</label>
                                        <asp:RequiredFieldValidator ID="FnameRequired" runat="server" ControlToValidate="NameTextBox" CssClass="EroorStar" ErrorMessage="*" ForeColor="Red" ValidationGroup="CreateUserWizard1"></asp:RequiredFieldValidator>
                                        <asp:TextBox ID="NameTextBox" runat="server" CssClass="form-control" placeholder="Name"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Designation</label>
                                        <asp:RequiredFieldValidator ID="DesignationRequired" runat="server" ControlToValidate="DesignationTextBox" CssClass="EroorStar" ErrorMessage="*" ForeColor="Red" ValidationGroup="CreateUserWizard1"></asp:RequiredFieldValidator>
                                        <asp:TextBox ID="DesignationTextBox" runat="server" CssClass="form-control" placeholder="Input Designation"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Login Username</label>
                                        <asp:RequiredFieldValidator ID="UsernameRequired" runat="server" ControlToValidate="UserName" CssClass="EroorStar" ErrorMessage="*" ForeColor="Red" ValidationGroup="CreateUserWizard1"></asp:RequiredFieldValidator>
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="UserName" CssClass="EroorSummer" Display="Dynamic" ErrorMessage="Minimum 8 and Maximum 30 characters required." ValidationExpression="^[\s\S]{8,30}$" ValidationGroup="CreateUserWizard1"></asp:RegularExpressionValidator>
                                        <asp:TextBox ID="UserName" runat="server" CssClass="form-control" placeholder="Input User Name" tooltipText="UserName must be minimum of 6 characters or digites, first 1 must be letter, Only use (- _ ) after 5 digites"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Password</label>
                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" ErrorMessage="*" ToolTip="Password is required." ValidationGroup="CreateUserWizard1" ForeColor="Red"></asp:RequiredFieldValidator>
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator4" runat="server" ControlToValidate="Password" CssClass="EroorSummer" Display="Dynamic" ErrorMessage="Minimum 8 and Maximum 30 characters required." ValidationExpression="^[\s\S]{8,30}$" ValidationGroup="CreateUserWizard1"></asp:RegularExpressionValidator>
                                        <asp:TextBox ID="Password" runat="server" placeholder="Input Password" TextMode="Password" CssClass="form-control"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Confirm Password</label>
                                        <asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" ControlToValidate="ConfirmPassword" ErrorMessage="*" ValidationGroup="CreateUserWizard1" ForeColor="Red"></asp:RequiredFieldValidator>
                                        <asp:TextBox ID="ConfirmPassword" runat="server" placeholder="Password Again" TextMode="Password" CssClass="form-control"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Email</label>
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="Email" ErrorMessage="Invalid Email" ForeColor="Red" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                                        <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email" ErrorMessage="*" ValidationGroup="CreateUserWizard1" ForeColor="Red"></asp:RequiredFieldValidator>
                                        <asp:TextBox ID="Email" runat="server" placeholder="Write@mail.com" CssClass="form-control"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Security Question</label>
                                        <asp:RequiredFieldValidator ID="QuestionRequired" runat="server" ControlToValidate="Question" ErrorMessage="*" ValidationGroup="CreateUserWizard1" ForeColor="Red" InitialValue="Select your security question"></asp:RequiredFieldValidator>
                                        <asp:DropDownList ID="Question" runat="server" CssClass="form-control" ValidationGroup="CreateUserWizard1">
                                            <asp:ListItem>Select your security question</asp:ListItem>
                                            <asp:ListItem>What is the first name of your favorite uncle?</asp:ListItem>
                                            <asp:ListItem>What is your oldest child&#39;s nick name?</asp:ListItem>
                                            <asp:ListItem>What is the first name of your oldest nephew?</asp:ListItem>
                                            <asp:ListItem>What is the first name of your aunt?</asp:ListItem>
                                            <asp:ListItem>Where did you spend your honeymoon?</asp:ListItem>
                                            <asp:ListItem>What is your favorite game?</asp:ListItem>
                                            <asp:ListItem>what is your favorite food?</asp:ListItem>
                                            <asp:ListItem>What was your favorite sport in high school?</asp:ListItem>
                                            <asp:ListItem>In what city were you born?</asp:ListItem>
                                            <asp:ListItem>What is the country of your ultimate dream vacation?</asp:ListItem>
                                            <asp:ListItem>What is the title and author of your favorite book?</asp:ListItem>
                                            <asp:ListItem>What is your favorite TV program?</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>

                                    <div class="form-group">
                                        <label>Answer the Question</label>
                                        <asp:RequiredFieldValidator ID="AnswerRequired" runat="server" ControlToValidate="Answer" ErrorMessage="*" ValidationGroup="CreateUserWizard1" ForeColor="Red"></asp:RequiredFieldValidator>
                                        <asp:TextBox ID="Answer" runat="server" placeholder="Answer the Question" CssClass="form-control"></asp:TextBox>
                                    </div>


                                    <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" CssClass="btn btn-primary" Text="Save &amp; Continue" ValidationGroup="CreateUserWizard1" />
                                    <asp:CompareValidator ID="PasswordCompare" runat="server"
                                        ControlToCompare="Password" ControlToValidate="ConfirmPassword"
                                        Display="Dynamic"
                                        ErrorMessage="The Password and Confirmation Password must match."
                                        ValidationGroup="CreateUserWizard1" ForeColor="Red"></asp:CompareValidator>

                                    <div class="alert-danger">
                                        <asp:Literal ID="ErrorMessage" runat="server" EnableViewState="False"></asp:Literal>
                                    </div>

                                    <asp:SqlDataSource ID="AdminSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Authority_Info(RegistrationID, Name, Designation, Gender) VALUES (@RegistrationID, @Name, @Designation, N'Male')" SelectCommand="SELECT * FROM [Admin]">
                                        <InsertParameters>
                                            <asp:Parameter Name="RegistrationID" />
                                            <asp:ControlParameter ControlID="NameTextBox" Name="Name" PropertyName="Text" />
                                            <asp:ControlParameter ControlID="DesignationTextBox" Name="Designation" PropertyName="Text" />
                                        </InsertParameters>
                                    </asp:SqlDataSource>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <CustomNavigationTemplate>
                </CustomNavigationTemplate>
            </asp:CreateUserWizardStep>
            <asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
                <ContentTemplate>
                    <div class="alert alert-success">
                        <strong>Congratulation!</strong>
                        <p>Account has been successfully created.</p>
                    </div>

                    <div class="form-group">
                        <asp:Button ID="ContinueButton" runat="server" CausesValidation="False" CommandName="Continue" Text="Continue" ValidationGroup="CreateUserWizard1" CssClass="btn btn-primary" PostBackUrl="~/Profile_Redirect.aspx" />
                    </div>
                </ContentTemplate>
            </asp:CompleteWizardStep>
        </WizardSteps>

        <FinishNavigationTemplate>
            <asp:Button ID="FinishPreviousButton" runat="server" CausesValidation="False" Visible="false" CommandName="MovePrevious" Text="Previous" />
            <asp:Button ID="FinishButton" runat="server" CommandName="MoveComplete" Visible="false" Text="Finish" />
        </FinishNavigationTemplate>
    </asp:CreateUserWizard>

    <asp:SqlDataSource ID="LITQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [AST] ([RegistrationID], [SchoolID], [UserName], [Category], [Password], [PasswordAnswer]) VALUES (@RegistrationID, @SchoolID, @UserName, @Category, @Password, @PasswordAnswer)" SelectCommand="SELECT * FROM [AST]">
        <InsertParameters>
            <asp:Parameter DefaultValue="" Name="RegistrationID" Type="Int32" />
            <asp:Parameter DefaultValue="" Name="SchoolID" Type="Int32" />
            <asp:Parameter Name="UserName" Type="String" />
            <asp:Parameter DefaultValue="Sub-Admin" Name="Category" Type="String" />
            <asp:Parameter Name="Password" Type="String" />
            <asp:Parameter Name="PasswordAnswer" Type="String" />
        </InsertParameters>
    </asp:SqlDataSource>

    
</asp:Content>
