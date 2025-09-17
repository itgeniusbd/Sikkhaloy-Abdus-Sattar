<%@ Page Title="Signup Teacher" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Teacher.aspx.cs" Inherits="EDUCATION.COM.ADMINISTRATION_BASIC_SETTING.Create_Edit_Delete_Teacher" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>New Registration For Teacher</h3>

    <div class="row">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-body">
                    <asp:CreateUserWizard ID="TeacherCreateUserWizard" runat="server" OnCreatedUser="TeacherCreateUserWizard_CreatedUser" LoginCreatedUser="False" DuplicateUserNameErrorMessage="This username is already taken, please choose another." Width="100%">
                        <WizardSteps>
                            <asp:WizardStep ID="TeacherInfoW" runat="server" Title="Teacher Info">
                                <div class="form-group">
                                    <label>First Name*</label><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="FirstNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="S"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="FirstNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Last Name*</label>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="LastNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="S"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="LastNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Father&#39;s Name</label>
                                    <asp:TextBox ID="FatherTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Designation*</label>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="DesignationTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="S"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="DesignationTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Phone Number*</label>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="PhoneNumberTextBox" CssClass="EroorStar" ErrorMessage="*" ValidationGroup="S"></asp:RequiredFieldValidator>
                                    <asp:TextBox ID="PhoneNumberTextBox" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="GenderRadioButtonList" CssClass="EroorSummer" ErrorMessage="Pick Gender" ValidationGroup="S"></asp:RequiredFieldValidator>
                                    <asp:RadioButtonList ID="GenderRadioButtonList" runat="server" RepeatDirection="Horizontal">
                                        <asp:ListItem>Male</asp:ListItem>
                                        <asp:ListItem>Female</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>

                                <div class="form-group">
                                    <label>Age</label>
                                    <asp:TextBox ID="AgeTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Date of Birth</label>
                                    <asp:RegularExpressionValidator ID="RegularExpressionValidator6" runat="server" ControlToValidate="BirthDayTextBox" CssClass="EroorSummer" ErrorMessage="Invalid Date"
                                        ValidationExpression="^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$" ValidationGroup="S"></asp:RegularExpressionValidator>
                                    <asp:TextBox ID="BirthDayTextBox" runat="server" CssClass="form-control Datetime"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Religion</label>
                                    <asp:DropDownList ID="ReligionDropDownList" runat="server" CssClass="form-control">
                                        <asp:ListItem>Islam</asp:ListItem>
                                        <asp:ListItem>Hinduism</asp:ListItem>
                                        <asp:ListItem>Buddhism</asp:ListItem>
                                        <asp:ListItem>Christianity</asp:ListItem>
                                    </asp:DropDownList>
                                </div>

                                <div class="form-group">
                                    <label>Nationality</label>
                                    <asp:TextBox ID="NationalityTextBox" Text="BANGLADESHI" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Address</label>
                                    <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>City</label>
                                    <asp:TextBox ID="CityTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>State</label>
                                    <asp:TextBox ID="StateTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Postal code</label>
                                    <asp:TextBox ID="PostalCodeTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <asp:Button ID="TeacherInfoButton" runat="server" Text="Save & Next" OnClick="TeacherInfoButton_Click" ValidationGroup="S" CssClass="btn btn-primary" />
                                </div>

                                <asp:SqlDataSource ID="TeacherSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Teacher]" InsertCommand="INSERT INTO Teacher(TeacherRegistrationID, RegistrationID, SchoolID, FirstName, LastName, Date, T_SN,Designation, FatherName, Gender, Age, DateofBirth, Religion, Nationality, Address, City, PostalCode, State, Phone, Email) VALUES (@TeacherRegistrationID, @RegistrationID, @SchoolID, @FirstName, @LastName, GETDATE(), dbo.Teacher_SerialNumber(@SchoolID),@Designation,@FatherName,@Gender,@Age,@DateofBirth,@Religion,@Nationality,@Address,@City,@PostalCode,@State,@Phone,@Email) SELECT @TeacherID = SCOPE_IDENTITY()"
                                    OnInserted="TeacherSQL_Inserted">
                                    <InsertParameters>
                                        <asp:Parameter Name="TeacherRegistrationID" />
                                        <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                        <asp:ControlParameter ControlID="FirstNameTextBox" Name="FirstName" PropertyName="Text" Type="String" />
                                        <asp:ControlParameter ControlID="LastNameTextBox" Name="LastName" PropertyName="Text" Type="String" />
                                        <asp:ControlParameter ControlID="DesignationTextBox" Name="Designation" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="FatherTextBox" Name="FatherName" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="GenderRadioButtonList" Name="Gender" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="AgeTextBox" Name="Age" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="BirthDayTextBox" Name="DateofBirth" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="ReligionDropDownList" Name="Religion" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="NationalityTextBox" Name="Nationality" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="AddressTextBox" Name="Address" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="CityTextBox" Name="City" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="PostalCodeTextBox" Name="PostalCode" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="StateTextBox" Name="State" PropertyName="Text" />
                                        <asp:ControlParameter ControlID="PhoneNumberTextBox" Name="Phone" PropertyName="Text" />
                                        <asp:Parameter Name="Email" />
                                        <asp:Parameter Direction="Output" Name="TeacherID" Size="50" />
                                    </InsertParameters>
                                </asp:SqlDataSource>
                            </asp:WizardStep>

                            <asp:WizardStep ID="EmployeeInfo" runat="server">
                                <div class="form-group">
                                    <label>Job Type</label>
                                    <asp:RadioButtonList ID="JobTypeRadioButtonList" runat="server" CssClass="form-control" RepeatDirection="Horizontal">
                                        <asp:ListItem Selected="True">Permanent</asp:ListItem>
                                        <asp:ListItem>Temporary</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>

                                <div class="form-group" style="display: none;">
                                    <label>Salary Type</label>
                                    <asp:RadioButtonList ID="SalaryTypeRadioButtonList" runat="server" CssClass="form-control" RepeatDirection="Horizontal">
                                        <asp:ListItem>Work Basis</asp:ListItem>
                                        <asp:ListItem Selected="True">Time Basis</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>

                                <div class="form-group" id="DR" style="display: none;">
                                    <asp:RadioButtonList ID="JobperiodRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control">
                                        <asp:ListItem Selected="True">Monthly</asp:ListItem>
                                        <asp:ListItem>Weekly</asp:ListItem>
                                        <asp:ListItem>Daily</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>

                                <div class="form-group">
                                    <label>
                                        Salary*
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="SalaryTextBox" CssClass="EroorSummer" ErrorMessage="Input Salary" ValidationGroup="JI"></asp:RequiredFieldValidator>
                                    </label>
                                    <asp:TextBox ID="SalaryTextBox" autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" placeholder="Total Salary" runat="server"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Abs Deduction</label>
                                    <asp:RadioButtonList ID="Abs_DeductedRadioButtonList" runat="server" CssClass="form-control" RepeatDirection="Horizontal">
                                        <asp:ListItem Value="True">Yes</asp:ListItem>
                                        <asp:ListItem Selected="True" Value="False">No</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>

                                <div class="form-group" id="AHD" style="display: none">
                                    <label>Deduction Amount</label>
                                    <asp:TextBox ID="Abs_DeductedAmount_TextBox" autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" runat="server" placeholder="Amount"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>Late Count As Abs</label>
                                    <asp:RadioButtonList ID="Late_Count_As_AbsRadioButtonList" CssClass="form-control" runat="server" RepeatDirection="Horizontal">
                                        <asp:ListItem Value="True">Yes</asp:ListItem>
                                        <asp:ListItem Selected="True" Value="False">No</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>

                                <div class="form-group" id="LateDay" style="display: none">
                                    <label>Number of days</label>
                                    <asp:TextBox ID="LateDaysTextBox" autocomplete="off" CssClass="form-control" onDrop="blur();return false;" onkeypress="return isNumberKey(event)" onpaste="return false" runat="server" placeholder="Number of late days"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <asp:SqlDataSource ID="TeacherJobSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Employee_Info(SchoolID, RegistrationID, ID, EmployeeType, Permanent_Temporary, Work_Time_Basis, Time_Basis_Type, Salary, IS_Abs_Deducted, Abs_Deduction, IS_Late_Count_As_Abs,Late_Days, Job_Status) VALUES (@SchoolID, @RegistrationID, dbo.Employee_Teacher_ID(@TeacherID), @EmployeeType, @Permanent_Temporary, @Work_Time_Basis, @Time_Basis_Type, ISNULL(@Salary, 0), @IS_Abs_Deducted, ISNULL(@Abs_Deduction, 0), @IS_Late_Count_As_Abs,@Late_Days, @Job_Status)

UPDATE  Teacher SET  EmployeeID = (SELECT SCOPE_IDENTITY()) WHERE (TeacherID = @TeacherID)"
                                        SelectCommand="SELECT * FROM [Employee_Info]">
                                        <InsertParameters>
                                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                                            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                                            <asp:Parameter DefaultValue="Teacher" Name="EmployeeType" Type="String" />
                                            <asp:ControlParameter ControlID="JobTypeRadioButtonList" DefaultValue="" Name="Permanent_Temporary" PropertyName="SelectedValue" Type="String" />
                                            <asp:ControlParameter ControlID="SalaryTypeRadioButtonList" Name="Work_Time_Basis" PropertyName="SelectedValue" Type="String" />
                                            <asp:ControlParameter ControlID="JobperiodRadioButtonList" Name="Time_Basis_Type" PropertyName="SelectedValue" Type="String" />
                                            <asp:ControlParameter ControlID="SalaryTextBox" Name="Salary" PropertyName="Text" Type="Double" />
                                            <asp:ControlParameter ControlID="Abs_DeductedRadioButtonList" Name="IS_Abs_Deducted" PropertyName="SelectedValue" Type="Boolean" />
                                            <asp:ControlParameter ControlID="Abs_DeductedAmount_TextBox" Name="Abs_Deduction" PropertyName="Text" Type="Double" />
                                            <asp:ControlParameter ControlID="Late_Count_As_AbsRadioButtonList" Name="IS_Late_Count_As_Abs" PropertyName="SelectedValue" Type="Boolean" />
                                            <asp:Parameter DefaultValue="Active" Name="Job_Status" Type="String" />
                                            <asp:ControlParameter ControlID="LateDaysTextBox" Name="Late_Days" PropertyName="Text" />
                                            <asp:Parameter Name="TeacherID" DefaultValue="" />
                                        </InsertParameters>
                                    </asp:SqlDataSource>
                                    <asp:Button ID="TeacherJobsButton" runat="server" CssClass="btn btn-primary" OnClick="TeacherJobsButton_Click" Text="Save & Next" ValidationGroup="JI" />
                                </div>
                            </asp:WizardStep>

                            <asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server">
                                <ContentTemplate>
                                    <div class="form-group">
                                        <label>Login Username*</label>
                                        <asp:RequiredFieldValidator ID="UsernameRequired" runat="server" ControlToValidate="UserName" ErrorMessage="Username is required." ToolTip="Username is required." ValidationGroup="CreateUserWizard1" CssClass="EroorSummer">*</asp:RequiredFieldValidator>
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="UserName" CssClass="EroorSummer" Display="Dynamic" ErrorMessage="Minimum 8 and Maximum 30 characters required." ValidationExpression="^[\s\S]{8,30}$" ValidationGroup="CreateUserWizard1"></asp:RegularExpressionValidator>
                                        <asp:TextBox ID="UserName" runat="server" onDrop="blur();return false;" onpaste="return false" CssClass="form-control LoginUserName" placeholder="Input User Name" tooltipText="UserName must be minimum of 6 characters or digites, first 1 must be letter, Only use (- _ ) after 5 digites"></asp:TextBox>
                                        <i id="SpaceError" class="EroorSummer"></i>
                                    </div>

                                    <div class="form-group">
                                        <label>Password*</label>
                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" ErrorMessage="Enter Password" ToolTip="Password is required." ValidationGroup="CreateUserWizard1" CssClass="EroorSummer">*</asp:RequiredFieldValidator>
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator4" runat="server" ControlToValidate="Password" CssClass="EroorSummer" Display="Dynamic" ErrorMessage="Minimum 8 and Maximum 30 characters required." ValidationExpression="^[\s\S]{8,30}$" ValidationGroup="CreateUserWizard1"></asp:RegularExpressionValidator>
                                        <asp:TextBox ID="Password" runat="server" CssClass="form-control" placeholder="Input Password" TextMode="Password"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Confirm Password*</label>
                                        <asp:RequiredFieldValidator ID="ConfirmPasswordRequired" runat="server" ControlToValidate="ConfirmPassword" ErrorMessage="Confirm Password" ToolTip="Confirm Password is required." ValidationGroup="CreateUserWizard1" CssClass="EroorSummer">*</asp:RequiredFieldValidator>
                                        <asp:TextBox ID="ConfirmPassword" runat="server" CssClass="form-control" placeholder="Password Again" TextMode="Password"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>E-mail*</label>
                                        <asp:RequiredFieldValidator ID="EmailRequired" runat="server" ControlToValidate="Email" ErrorMessage="E-mail is required." ToolTip="E-mail is required." ValidationGroup="CreateUserWizard1" CssClass="EroorSummer">*</asp:RequiredFieldValidator>
                                        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="Email" ErrorMessage="Email not valid" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" CssClass="EroorSummer"></asp:RegularExpressionValidator>
                                        <asp:TextBox ID="Email" runat="server" CssClass="form-control" placeholder="Write@mail.com"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Security Question*</label>
                                        <asp:RequiredFieldValidator ID="QuestionRequired" runat="server" ControlToValidate="Question" ErrorMessage="Security question is required." InitialValue="Select your security question" ToolTip="Security question is required." ValidationGroup="CreateUserWizard1" CssClass="EroorSummer">*</asp:RequiredFieldValidator>
                                        <asp:DropDownList ID="Question" runat="server" CssClass="form-control" EnableViewState="False" ValidationGroup="CreateUserWizard1">
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
                                        <label>Security Answer*</label>
                                        <asp:RequiredFieldValidator ID="AnswerRequired" runat="server" ControlToValidate="Answer" ErrorMessage="Security answer is required." ToolTip="Security answer is required." ValidationGroup="CreateUserWizard1" CssClass="EroorSummer">*</asp:RequiredFieldValidator>
                                        <asp:TextBox ID="Answer" runat="server" CssClass="form-control" placeholder="Answer the Question"></asp:TextBox>
                                    </div>

                                    <div class="form-group">
                                        <label>Upload photo</label>
                                        <asp:FileUpload ID="ImageFileUpload" runat="server" />
                                    </div>

                                    <div class="form-group">
                                        <asp:CompareValidator ID="PasswordCompare" runat="server"
                                            ControlToCompare="Password" ControlToValidate="ConfirmPassword"
                                            Display="Dynamic"
                                            ErrorMessage="The Password and Confirmation Password must match."
                                            ValidationGroup="CreateUserWizard1" CssClass="EroorSummer"></asp:CompareValidator>
                                        <div class="alert-danger">
                                            <asp:Literal ID="ErrorMessage" runat="server" EnableViewState="False"></asp:Literal>
                                        </div>

                                        <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" CssClass="btn btn-primary" Text="Signup Teacher" ValidationGroup="CreateUserWizard1" />
                                    </div>
                                </ContentTemplate>
                                <CustomNavigationTemplate>
                                </CustomNavigationTemplate>
                            </asp:CreateUserWizardStep>

                            <asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
                                <ContentTemplate>
                                    <div class="alert alert-success">
                                        Congratulation! Teacher Account has been successfully created.
                                    </div>
                                    <div class="form-group">
                                        <asp:Button ID="ContinueButton" runat="server" CausesValidation="False" CommandName="Continue" Text="Continue" ValidationGroup="CreateUserWizard1" CssClass="btn btn-primary" OnClick="ContinueButton_Click" />
                                    </div>
                                </ContentTemplate>
                            </asp:CompleteWizardStep>
                        </WizardSteps>

                        <FinishNavigationTemplate>
                            <asp:Button ID="FinishPreviousButton" runat="server" CausesValidation="False" Visible="false" CommandName="MovePrevious" Text="Previous" />
                            <asp:Button ID="FinishButton" runat="server" CommandName="MoveComplete" Visible="false" Text="Finish" />
                        </FinishNavigationTemplate>
                        <StartNavigationTemplate>
                            <asp:Button ID="StartNextButton" runat="server" CommandName="MoveNext" Text="Next" Visible="false" />
                        </StartNavigationTemplate>
                        <StepNavigationTemplate>
                            <asp:Button ID="StepPreviousButton" runat="server" CausesValidation="False" CommandName="MovePrevious" Text="Previous" Visible="false" />
                            <asp:Button ID="StepNextButton" runat="server" CommandName="MoveNext" Text="Next" Visible="false" />
                        </StepNavigationTemplate>
                    </asp:CreateUserWizard>
                </div>
            </div>
        </div>
        <asp:SqlDataSource ID="LITSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [AST] ([RegistrationID], [SchoolID], [UserName], [Category], [Password], [PasswordAnswer]) VALUES (@RegistrationID, @SchoolID, @UserName, @Category, @Password, @PasswordAnswer)" SelectCommand="SELECT * FROM [AST]">
            <InsertParameters>
                <asp:Parameter DefaultValue="Teacher" Name="Category" Type="String" />
                <asp:Parameter DefaultValue="" Name="RegistrationID" Type="Int32" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:Parameter Name="UserName" Type="String" />
                <asp:Parameter Name="Password" Type="String" />
                <asp:Parameter Name="PasswordAnswer" Type="String" />
            </InsertParameters>
        </asp:SqlDataSource>
        <asp:SqlDataSource ID="Edu_YearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO [Education_Year_User] ([RegistrationID], [EducationYearID], [SchoolID]) VALUES (@RegistrationID, @EducationYearID, @SchoolID)" SelectCommand="SELECT * FROM [Education_Year_User]">
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
                <asp:Parameter Name="RegistrationID" Type="Int32" />
            </InsertParameters>
        </asp:SqlDataSource>

            <asp:SqlDataSource ID="Device_DataUpdateSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS(SELECT DateUpdateID FROM  Attendance_Device_DataUpdateList WHERE (SchoolID = @SchoolID) AND (UpdateType = @UpdateType))
BEGIN
INSERT INTO Attendance_Device_DataUpdateList(SchoolID, RegistrationID, UpdateType, UpdateDescription) VALUES (@SchoolID, @RegistrationID, @UpdateType, @UpdateDescription)
END" SelectCommand="SELECT * FROM [Attendance_Device_DataUpdateList]">
                <InsertParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
                    <asp:Parameter DefaultValue="New Employee" Name="UpdateType" Type="String" />
                    <asp:Parameter DefaultValue="Add new Teacher" Name="UpdateDescription" Type="String" />
                </InsertParameters>
            </asp:SqlDataSource>

    </div>

    <script src="/JS/DateMask.js"></script>
    <script>
        $(function () {
            $("[id*=BirthDayTextBox]").mask("99/99/9999", { placeholder: 'dd/mm/yyyy' });

            //$("#body_TeacherCreateUserWizard_SalaryTypeRadioButtonList_0").click(function () { $('#DR').hide('slow') });
            //$("#body_TeacherCreateUserWizard_SalaryTypeRadioButtonList_1").click(function () { $('#DR').show('slow') });

            $("#body_TeacherCreateUserWizard_Abs_DeductedRadioButtonList_0").click(function () {
                $('#AHD').show('slow');
                $("[id*=Abs_DeductedAmount_TextBox]").attr("required", "required");
            });
            $("#body_TeacherCreateUserWizard_Abs_DeductedRadioButtonList_1").click(function () {
                $('#AHD').hide('slow');
                $("[id*=Abs_DeductedAmount_TextBox]").removeAttr("required");
            });

            $("#body_TeacherCreateUserWizard_Late_Count_As_AbsRadioButtonList_0").click(function () {
                $('#LateDay').show('slow');
                $("[id*=LateDaysTextBox]").attr("required", "required");
            });
            $("#body_TeacherCreateUserWizard_Late_Count_As_AbsRadioButtonList_1").click(function () {
                $('#LateDay').hide('slow');
                $("[id*=LateDaysTextBox]").removeAttr("required");
            });

            //Space not allow
            $('.LoginUserName').on("keypress", function (e) {
                if (e.which === 32) {
                    $("#SpaceError").text("Space not allow");
                    return false;
                } else {
                    $("#SpaceError").text("");
                }
            });
        })

        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
