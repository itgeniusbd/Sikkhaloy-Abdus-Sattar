<%@ Page Title="Student Admission" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Admission_New_Student.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Admission_New_Student"%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" rel="stylesheet" />
    <link href="CSS/New_Student_Admission.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="admission-container">
        <div class="page-header">
            <h2><i class="fas fa-user-graduate"></i> Student Admission Form</h2>
            <p>Complete all required fields to proceed with admission</p>
        </div>

        <!-- Progress Indicator -->
        <div class="progress-indicator">
            <div class="progress-step active" id="step1">
                <i class="fas fa-user fa-2x"></i>
                <div>Basic Info</div>
            </div>
            <div class="progress-step" id="step2">
                <i class="fas fa-users fa-2x"></i>
                <div>Parents</div>
            </div>
            <div class="progress-step" id="step3">
                <i class="fas fa-book fa-2x"></i>
                <div>Academic</div>
            </div>
            <div class="progress-step" id="step4">
                <i class="fas fa-check-circle fa-2x"></i>
                <div>Complete</div>
            </div>
        </div>

        <!-- Section 1: Required Student Information (Always Open) -->
        <div class="section-card">
            <div class="section-header required">
                <h4>
                    <i class="fas fa-star"></i>
                    Required Student Information
                    <span class="badge badge-light badge-status">Required</span>
                </h4>
                <i class="fas fa-chevron-down icon"></i>
            </div>
            <div class="section-body show">
                <asp:Label ID="LastIDLabel" runat="server" CssClass="badge badge-info mb-3" Font-Size="Medium"></asp:Label>
                
                <div class="row">
                    <div class="col-md-4 form-group">
                        <label class="form-label">
                            Student ID <span class="required-mark">*</span>
                        </label>
                        <asp:TextBox ID="IDTextBox" runat="server" CssClass="form-control" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="IDTextBox" CssClass="EroorSummer" ErrorMessage="Student ID is required" ValidationGroup="1"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="IDTextBox" CssClass="EroorSummer" ErrorMessage="Invalid ID format" ValidationExpression="^[A-Z0-9](?!.*?[^\nA-Z0-9]{2}).*?[A-Z0-9]$" ValidationGroup="1"></asp:RegularExpressionValidator>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">
                            SMS Mobile Number <span class="required-mark">*</span>
                        </label>
                        <asp:TextBox ID="SMSPhoneNoTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="Mobile number is required" ValidationGroup="1"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="Invalid mobile number" ValidationExpression="(88)?((011)|(015)|(016)|(017)|(018)|(019)|(013)|(014))\d{8,8}" ValidationGroup="1"></asp:RegularExpressionValidator>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">
                            Session Year <span class="required-mark">*</span>
                        </label>
                        <asp:DropDownList ID="EducationYearDropDownList" runat="server" DataSourceID="EduYearSQL" DataTextField="EducationYear" DataValueField="EducationYearID" CssClass="form-control custom-select">
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYear, EducationYearID FROM Education_Year WHERE (SchoolID = @SchoolID) ORDER BY SN">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="EducationYearDropDownList" CssClass="EroorSummer" ErrorMessage="Session year is required" InitialValue="0" ValidationGroup="1"></asp:RequiredFieldValidator>
                    </div>
                </div>
                
                <div class="row">
                    <div class="col-md-6 form-group">
                        <label class="form-label">
                            Student's Name <span class="required-mark">*</span>
                        </label>
                        <asp:TextBox ID="StudentNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="StudentNameTextBox" CssClass="EroorSummer" ErrorMessage="Student name is required" ValidationGroup="1"></asp:RequiredFieldValidator>
                    </div>
                    
                    <div class="col-md-3 form-group">
                        <label class="form-label">
                            Gender <span class="required-mark">*</span>
                        </label>
                        <asp:RadioButtonList ID="GenderRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-check">
                            <asp:ListItem Selected="True">Male</asp:ListItem>
                            <asp:ListItem>Female</asp:ListItem>
                        </asp:RadioButtonList>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="GenderRadioButtonList" CssClass="EroorSummer" ErrorMessage="Gender is required" ValidationGroup="1"></asp:RequiredFieldValidator>
                    </div>
                    
                    <div class="col-md-3 form-group">
                        <label class="form-label">Father's Name <span class="required-mark">*</span></label>
                        <asp:TextBox ID="FatherNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="FatherNameTextBox" CssClass="EroorSummer" ErrorMessage="Father's name is required" ValidationGroup="1"></asp:RequiredFieldValidator>
                    </div>
                </div>
            </div>
        </div>

        <!-- Section 2: Optional Student Details (Collapsible) -->
        <div class="section-card">
            <div class="section-header optional collapsed">
                <h4>
                    <i class="fas fa-id-card"></i>
                    Additional Student Details
                    <span class="badge badge-light badge-status">Optional</span>
                </h4>
                <i class="fas fa-chevron-down icon"></i>
            </div>
            <div class="section-body">
                <div class="row">
                    <div class="col-md-6 form-group">
                        <label class="form-label">Student's Email</label>
                        <asp:TextBox ID="StudentEmailTextBox" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator5" runat="server" ControlToValidate="StudentEmailTextBox" CssClass="EroorSummer" ErrorMessage="Invalid email format" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ValidationGroup="1"></asp:RegularExpressionValidator>
                    </div>
                    
                    <div class="col-md-6 form-group">
                        <label class="form-label">Date of Birth (dd/mm/yyyy)</label>
                        <asp:TextBox ID="BirthDayTextBox" runat="server" CssClass="form-control" placeholder="dd/mm/yyyy"></asp:TextBox>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator6" runat="server" ControlToValidate="BirthDayTextBox" CssClass="EroorSummer" ErrorMessage="Invalid date format" ValidationExpression="^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$" ValidationGroup="1"></asp:RegularExpressionValidator>
                    </div>
                </div>
                
                <div class="row">
                    <div class="col-md-4 form-group">
                        <label class="form-label">Legal Identity No. (NID/Birth Reg.)</label>
                        <asp:TextBox ID="Legal_IdentityTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">Blood Group</label>
                        <asp:DropDownList ID="BloodGroupDropDownList" runat="server" CssClass="form-control custom-select">
                            <asp:ListItem Value="Unknown">[ SELECT ]</asp:ListItem>
                            <asp:ListItem>A+</asp:ListItem>
                            <asp:ListItem>A-</asp:ListItem>
                            <asp:ListItem>B+</asp:ListItem>
                            <asp:ListItem>B-</asp:ListItem>
                            <asp:ListItem>AB+</asp:ListItem>
                            <asp:ListItem>AB-</asp:ListItem>
                            <asp:ListItem>O+</asp:ListItem>
                            <asp:ListItem>O-</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">Religion</label>
                        <asp:DropDownList ID="ReligionDropDownList" runat="server" CssClass="form-control custom-select">
                            <asp:ListItem Selected="True">Islam</asp:ListItem>
                            <asp:ListItem>Hinduism</asp:ListItem>
                            <asp:ListItem>Buddhism</asp:ListItem>
                            <asp:ListItem>Christianity</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                
                <div class="row">
                    <div class="col-md-6 form-group">
                        <label class="form-label">Student's Permanent Address</label>
                        <asp:TextBox ID="StudentPermanentAddressTextBox" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-6 form-group">
                        <label class="form-label">
                            Student's Local Address
                            <button type="button" id="SameAddrs" class="btn btn-sm btn-link" style="padding:0">
                                <i class="fas fa-copy"></i> Same as Permanent
                            </button>
                        </label>
                        <asp:TextBox ID="StudentLocalAddressTextBox" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                
                <div class="form-group">
                    <label class="form-label">Student's Photo</label>
                    <div class="photo-upload">
                        <label for="StudentPhoto" class="custom-file-upload">
                            <i class="fas fa-camera"></i> Choose Photo
                        </label>
                        <input name="Student_photo" id="StudentPhoto" type="file" accept=".png,.jpg,.jpeg" />
                        <p class="mt-2 text-muted">Recommended: 250x250 pixels, Max 2MB</p>
                    </div>
                    <asp:HiddenField ID="Imge_HF" runat="server" />
                </div>
            </div>
        </div>

        <!-- Section 3: Parents Information -->
        <div class="section-card">
            <div class="section-header optional collapsed">
                <h4>
                    <i class="fas fa-users"></i>
                    Parents & Guardian Information
                    <span class="badge badge-light badge-status">Optional</span>
                </h4>
                <i class="fas fa-chevron-down icon"></i>
            </div>
            <div class="section-body">
                <h5 class="mb-3"><i class="fas fa-male"></i> Father's Information</h5>
                <div class="row">
                    <div class="col-md-6 form-group">
                        <label class="form-label">Father's Phone</label>
                        <asp:TextBox ID="FatherPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-6 form-group">
                        <label class="form-label">Father's Occupation</label>
                        <asp:TextBox ID="FatherOccupationTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                
                <h5 class="mb-3 mt-4"><i class="fas fa-female"></i> Mother's Information</h5>
                <div class="row">
                    <div class="col-md-4 form-group">
                        <label class="form-label">Mother's Name</label>
                        <asp:TextBox ID="MothersNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">Mother's Phone</label>
                        <asp:TextBox ID="MotherPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">Mother's Occupation</label>
                        <asp:TextBox ID="MotherOccupationTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                
                <h5 class="mb-3 mt-4"><i class="fas fa-user-shield"></i> Guardian Information</h5>
                <div class="form-group">
                    <label class="form-label">Guardian's Photo</label>
                    <div class="photo-upload">
                        <label for="GuardianPhoto" class="custom-file-upload">
                            <i class="fas fa-camera"></i> Choose Photo
                        </label>
                        <input name="Guardian_photo" id="GuardianPhoto" type="file" accept=".png,.jpg,.jpeg" />
                        <p class="mt-2 text-muted">Recommended: 250x250 pixels, Max 2MB</p>
                    </div>
                    <asp:HiddenField ID="Guardian_Imge_HF" runat="server" />
                </div>
                
                <h5 class="mb-3 mt-4"><i class="fas fa-user-plus"></i> Second Guardian (Optional)</h5>
                <div class="row">
                    <div class="col-md-4 form-group">
                        <label class="form-label">Guardian Name</label>
                        <asp:TextBox ID="SecondGuardianNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">Relationship</label>
                        <asp:TextBox ID="RelationshipwithStudentTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">Mobile No.</label>
                        <asp:TextBox ID="SecondGuardianPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
            </div>
        </div>

        <!-- Section 4: Previous Institution -->
        <div class="section-card">
            <div class="section-header optional collapsed">
                <h4>
                    <i class="fas fa-school"></i>
                    Previous Institution Information
                    <span class="badge badge-light badge-status">Optional</span>
                </h4>
                <i class="fas fa-chevron-down icon"></i>
            </div>
            <div class="section-body">
                <div class="form-group">
                    <label class="form-label">Institution Name</label>
                    <asp:TextBox ID="PreviousSchoolNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                
                <div class="row">
                    <div class="col-md-4 form-group">
                        <label class="form-label">Class</label>
                        <asp:TextBox ID="PreviousClassTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">Exam Year</label>
                        <asp:TextBox ID="PrevExamYearTextBox" onkeypress="return isNumberKey(event)" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">Grade</label>
                        <asp:TextBox ID="PrevGradeTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
            </div>
        </div>

        <!-- Section 5: Academic Information (Required) -->
        <div class="section-card">
            <div class="section-header required">
                <h4>
                    <i class="fas fa-book-open"></i>
                    Academic Information & Subject Selection
                    <span class="badge badge-light badge-status">Required</span>
                </h4>
                <i class="fas fa-chevron-down icon"></i>
            </div>
            <div class="section-body show">
                <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
                    <ContentTemplate>
                        <asp:FormView ID="RollNo_FormView" DataSourceID="RollShowSQL" runat="server" Width="100%">
                            <ItemTemplate>
                                <div class="alert alert-info">
                                    <i class="fas fa-info-circle"></i> Last Entry Roll No: <strong><%# Eval("RollNo") %></strong>
                                </div>
                            </ItemTemplate>
                        </asp:FormView>
                        <asp:SqlDataSource ID="RollShowSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT TOP (1) StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (StudentsClass.SchoolID = @SchoolID) AND (StudentsClass.ClassID = @ClassID) AND (StudentsClass.SectionID LIKE @SectionID) AND (StudentsClass.EducationYearID = @Education_YearID) AND (Student.Status = N'Active') ORDER BY StudentsClass.StudentClassID DESC">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                                <asp:SessionParameter Name="Education_YearID" SessionField="Edu_Year" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        
                        <div class="row">
                            <div class="col-md-3 form-group">
                                <label class="form-label">Class <span class="required-mark">*</span></label>
                                <asp:DropDownList ID="ClassDropDownList" runat="server" CssClass="form-control custom-select" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                                    <asp:ListItem Value="0">[ SELECT CLASS ]</asp:ListItem>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [CreateClass] WHERE ([SchoolID] = @SchoolID) ORDER BY SN">
                                    <SelectParameters>
                                        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="ClassDropDownList" CssClass="EroorSummer" ErrorMessage="Select class" InitialValue="0" ValidationGroup="1"></asp:RequiredFieldValidator>
                            </div>
                            
                            <div class="col-md-3 form-group group-dropdown-wrapper">
                                <label class="form-label">Group</label>
                                <asp:DropDownList ID="GroupDropDownList" runat="server" CssClass="form-control custom-select" AutoPostBack="True" DataSourceID="GroupSQL" DataTextField="SubjectGroup" DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE N'%' + @SectionID + N'%')">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            
                            <div class="col-md-3 form-group section-dropdown-wrapper">
                                <label class="form-label">Section</label>
                                <asp:DropDownList ID="SectionDropDownList" runat="server" CssClass="form-control custom-select" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" AutoPostBack="True" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].ShiftID LIKE N'%' + @ShiftID + N'%')">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                            
                            <div class="col-md-3 form-group shift-dropdown-wrapper">
                                <label class="form-label">Shift</label>
                                <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control custom-select" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE N'%' + @SubjectGroupID + N'%') AND ([Join].SectionID LIKE N'%' + @SectionID + N'%') AND ([Join].ClassID = @ClassID)">
                                    <SelectParameters>
                                        <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                                        <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                            </div>
                        </div>
                        
                        <div class="form-group">
                            <label class="form-label">Roll Number</label>
                            <asp:TextBox ID="RollNumberTextBox" runat="server" CssClass="form-control" placeholder="Enter Roll Number"></asp:TextBox>
                        </div>
                        
                        <h5 class="mb-3 mt-4">
                            <i class="fas fa-book-reader"></i> Select Subjects
                            <small class="text-muted">(Check the box to select and choose subject type)</small>
                        </h5>
                        
                        <div class="table-responsive">
                            <asp:GridView ID="GroupGridView" runat="server" AutoGenerateColumns="False" CssClass="table table-hover table-bordered" DataKeyNames="SubjectID,SubjectType" DataSourceID="SubjectGroupSQL">
                                <Columns>
                                    <asp:BoundField DataField="SubjectName" HeaderText="Subject Name" SortExpression="SubjectName">
                                        <HeaderStyle HorizontalAlign="Left" />
                                        <ItemStyle HorizontalAlign="Left" />
                                    </asp:BoundField>
                                    <asp:TemplateField HeaderText="Select">
                                        <ItemTemplate>
                                            <asp:CheckBox ID="SubjectCheckBox" runat="server" Checked='<%# Bind("Check") %>' />
                                        </ItemTemplate>
                                        <ItemStyle HorizontalAlign="Center" Width="80px" />
                                        <HeaderStyle HorizontalAlign="Center" />
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Subject Type">
                                        <ItemTemplate>
                                            <asp:RadioButtonList ID="SubjectTypeRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%# Bind("SubjectType") %>'>
                                                <asp:ListItem>Compulsory</asp:ListItem>
                                                <asp:ListItem>Optional</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </ItemTemplate>
                                        <ItemStyle Width="220px" HorizontalAlign="Left" />
                                        <HeaderStyle HorizontalAlign="Left" />
                                    </asp:TemplateField>
                                </Columns>
                                <HeaderStyle BackColor="#667eea" ForeColor="White" />
                                <RowStyle BackColor="White" />
                                <AlternatingRowStyle BackColor="#f0f0ff" />
                            </asp:GridView>
                            <asp:SqlDataSource ID="SubjectGroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Subject.SubjectID, Subject.SubjectName, SubjectForGroup.SubjectType, CAST((CASE WHEN SubjectForGroup.SubjectType = 'Compulsory' THEN 1 ELSE 0 END)AS BIT) AS [Check] FROM Subject INNER JOIN SubjectForGroup ON Subject.SubjectID = SubjectForGroup.SubjectID WHERE (Subject.SchoolID = @SchoolID) AND (SubjectForGroup.ClassID = @ClassID) AND (SubjectForGroup.SubjectGroupID = (CASE WHEN @SubjectGroupID = '%' THEN 0 ELSE @SubjectGroupID END)) ORDER BY SubjectForGroup.SubjectType, Subject.SubjectName">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                    <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                                    <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </div>
        </div>

        <!-- Section 6: Additional Notes -->
        <div class="section-card">
            <div class="section-header optional collapsed">
                <h4>
                    <i class="fas fa-sticky-note"></i>
                    Additional Information
                    <span class="badge badge-light badge-status">Optional</span>
                </h4>
                <i class="fas fa-chevron-down icon"></i>
            </div>
            <div class="section-body">
                <div class="form-group">
                    <label class="form-label">Other Details/Notes</label>
                    <asp:TextBox ID="OthersDetailsTextBox" runat="server" TextMode="MultiLine" Rows="4" CssClass="form-control" placeholder="Any additional information..."></asp:TextBox>
                </div>
            </div>
        </div>

        <!-- Hidden Data Sources -->
        <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Student(RegistrationID, SchoolID, StudentRegistrationID, StudentImageID, ID, SMSPhoneNo, StudentEmailAddress, StudentsName, Gender, DateofBirth,Legal_Identity, BloodGroup, Religion, StudentPermanentAddress, StudentsLocalAddress, PrevSchoolName, PrevClass, PrevExamYear, PrevExamGrade, MothersName, MotherOccupation, MotherPhoneNumber, FathersName, FatherOccupation, FatherPhoneNumber, GuardianName, GuardianRelationshipwithStudent, GuardianPhoneNumber, Status, OtherDetails, AdmissionDate) VALUES (@RegistrationID, @SchoolID, @StudentRegistrationID, @StudentImageID, @ID, @SMSPhoneNo, @StudentEmailAddress, @StudentsName, @Gender, CONVERT(date,@DateofBirth,105) ,@Legal_Identity, @BloodGroup, @Religion, @StudentPermanentAddress, @StudentsLocalAddress, @PrevSchoolName, @PrevClass, @PrevExamYear, @PrevExamGrade, @MothersName, @MotherOccupation, @MotherPhoneNumber, @FathersName, @FatherOccupation, @FatherPhoneNumber, @GuardianName, @GuardianRelationshipwithStudent, @GuardianPhoneNumber, @Status, @OtherDetails, GETDATE())" SelectCommand="SELECT * FROM [Student]">
            <InsertParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:Parameter Name="StudentImageID" Type="Int32" />
                <asp:Parameter Name="StudentRegistrationID" Type="Int32" />
                <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="SMSPhoneNoTextBox" Name="SMSPhoneNo" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="StudentEmailTextBox" Name="StudentEmailAddress" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="StudentNameTextBox" Name="StudentsName" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="GenderRadioButtonList" Name="Gender" PropertyName="SelectedValue" Type="String" />
                <asp:ControlParameter ControlID="BirthDayTextBox" Name="DateofBirth" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="Legal_IdentityTextBox" Name="Legal_Identity" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="BloodGroupDropDownList" Name="BloodGroup" PropertyName="SelectedValue" Type="String" />
                <asp:ControlParameter ControlID="ReligionDropDownList" Name="Religion" PropertyName="SelectedValue" Type="String" />
                <asp:ControlParameter ControlID="StudentPermanentAddressTextBox" Name="StudentPermanentAddress" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="StudentLocalAddressTextBox" Name="StudentsLocalAddress" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="PreviousSchoolNameTextBox" Name="PrevSchoolName" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="PreviousClassTextBox" Name="PrevClass" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="PrevExamYearTextBox" Name="PrevExamYear" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="PrevGradeTextBox" Name="PrevExamGrade" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="MothersNameTextBox" Name="MothersName" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="MotherOccupationTextBox" Name="MotherOccupation" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="MotherPhoneTextBox" Name="MotherPhoneNumber" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="FatherNameTextBox" Name="FathersName" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="FatherOccupationTextBox" Name="FatherOccupation" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="FatherPhoneTextBox" Name="FatherPhoneNumber" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="SecondGuardianNameTextBox" Name="GuardianName" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="RelationshipwithStudentTextBox" Name="GuardianRelationshipwithStudent" PropertyName="Text" Type="String" />
                <asp:ControlParameter ControlID="SecondGuardianPhoneTextBox" Name="GuardianPhoneNumber" PropertyName="Text" Type="String" />
                <asp:Parameter Name="Status" Type="String" DefaultValue="Active" />
                <asp:ControlParameter ControlID="OthersDetailsTextBox" DefaultValue="" Name="OtherDetails" PropertyName="Text" Type="String" />
            </InsertParameters>
        </asp:SqlDataSource>
        
        <asp:SqlDataSource ID="StudentImageSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="INSERT INTO Student_Image(Image, Guardian_Photo) VALUES (CAST(N'' AS xml).value('xs:base64Binary(sql:variable(&quot;@Image&quot;))', 'varbinary(max)'), CAST(N'' AS xml).value('xs:base64Binary(sql:variable(&quot;@Guardian_Photo&quot;))', 'varbinary(max)'))" SelectCommand="SELECT * FROM [Student_Image]">
            <InsertParameters>
                <asp:ControlParameter ControlID="Imge_HF" Name="Image" PropertyName="Value" />
                <asp:ControlParameter ControlID="Guardian_Imge_HF" Name="Guardian_Photo" PropertyName="Value" />
            </InsertParameters>
        </asp:SqlDataSource>
        
        <asp:SqlDataSource ID="StudentClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS (SELECT * FROM  StudentsClass WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID))
BEGIN
INSERT INTO [StudentsClass] ([SchoolID], [RegistrationID], [StudentID], [ClassID], [SectionID], [ShiftID], [SubjectGroupID], [RollNo], [EducationYearID], [Date]) VALUES (@SchoolID, @RegistrationID, @StudentID, @ClassID, @SectionID, @ShiftID, @SubjectGroupID, @RollNo, @EducationYearID, Getdate())
END" SelectCommand="SELECT * FROM StudentsClass ">
            <InsertParameters>
                <asp:Parameter Name="StudentID" Type="Int32" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="String" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
                <asp:SessionParameter Name="SectionID" SessionField="SectionID" Type="String" />
                <asp:SessionParameter Name="ShiftID" SessionField="ShiftID" Type="String" />
                <asp:SessionParameter Name="SubjectGroupID" SessionField="GroupID" Type="String" />
                <asp:ControlParameter ControlID="RollNumberTextBox" Name="RollNo" PropertyName="Text" Type="String" />
            </InsertParameters>
        </asp:SqlDataSource>
        
        <asp:SqlDataSource ID="StudentRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" InsertCommand="IF NOT EXISTS (SELECT  * FROM  StudentRecord WHERE (StudentID = @StudentID) AND (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) AND (StudentClassID = @StudentClassID) AND (SubjectID = @SubjectID))
BEGIN
INSERT INTO StudentRecord(SchoolID, RegistrationID, StudentID, StudentClassID, SubjectID, EducationYearID, Date, SubjectType) VALUES (@SchoolID, @RegistrationID, @StudentID, @StudentClassID, @SubjectID, @EducationYearID, GETDATE(), @SubjectType)
END" SelectCommand="SELECT * FROM [StudentRecord]">
            <InsertParameters>
                <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                <asp:Parameter Name="StudentID" />
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" Type="Int32" />
                <asp:Parameter Name="SubjectID" Type="Int32" />
                <asp:Parameter Name="SubjectType" />
            </InsertParameters>
        </asp:SqlDataSource>

        <!-- Action Buttons -->
        <div class="action-buttons">
            <!-- Checkbox Container - Upper Section -->
            <div class="checkbox-container">
                <label>
                    <asp:CheckBox ID="SMSCheckBox" runat="server" />
                    <span>📱 Send admission SMS (English)</span>
                </label>
                <label>
                    <asp:CheckBox ID="BanglaSMSCheckBox" runat="server" />
                    <span>📱 Send admission SMS (বাংলা)</span>
                </label>
                <label>
                    <asp:CheckBox ID="PrintCheckBox" runat="server" />
                    <span>🖨️ Print admission form (English)</span>
                </label>
                <label>
                    <asp:CheckBox ID="BanglaPrintCheckBox" runat="server" />
                    <span>🖨️ Print admission form (বাংলা)</span>
                </label>
            </div>
            
            <!-- Button Container - Lower Section -->
            <div class="button-container">
                <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-success" OnClick="SubmitButton_Click" Text="✓ COMPLETE ADMISSION" ValidationGroup="1" UseSubmitBehavior="False" />
                <asp:Button ID="GoPayorderButton" runat="server" CssClass="btn btn-primary" OnClick="GoPayorderButton_Click" Text="💳 SAVE & GO TO PAYMENT" ValidationGroup="1" UseSubmitBehavior="False" />
            </div>
            
            <asp:ValidationSummary ID="ValidationSummary2" runat="server" CssClass="EroorSummer" DisplayMode="List" ShowMessageBox="True" ShowSummary="False" ValidationGroup="1" />
        </div>
    </div>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.7); z-index: 9999; display: flex; align-items: center; justify-content: center;">
                <div style="background: white; padding: 30px; border-radius: 10px; text-align: center;">
                    <div class="spinner-border text-primary" role="status">
                        <span class="sr-only">Loading...</span>
                    </div>
                    <p class="mt-3"><strong>Processing...</strong></p>
                </div>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="/JS/DateMask.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/bgrins-spectrum/1.8.1/spectrum.min.js"></script>
    <script src="../../JS/canvas-to-blob.js"></script>
    <script src="../../JS/canvas-to-blob.min.js"></script>
    <script src="../../JS/canvas-resize.js"></script>
    <script type="text/javascript">
        function isNumberKey(a) { 
            a = a.which ? a.which : event.keyCode; 
            return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 
        }

        $(function () {
            // Collapsible sections
            $('.section-header').click(function () {
                $(this).toggleClass('collapsed');
                $(this).next('.section-body').slideToggle(300).toggleClass('show');
            });

            // Progress indicator
            function updateProgress() {
                var filledCount = 0;
                var totalRequired = 5; // Adjust based on required fields
                
                // Check required fields
                if ($('[id*=IDTextBox]').val()) filledCount++;
                if ($('[id*=SMSPhoneNoTextBox]').val()) filledCount++;
                if ($('[id*=StudentNameTextBox]').val()) filledCount++;
                if ($('[id*=FatherNameTextBox]').val()) filledCount++;
                if ($('[id*=ClassDropDownList]').val() !== '0') filledCount++;
                
                // Update step classes
                $('.progress-step').removeClass('active completed');
                if (filledCount >= 4) {
                    $('#step1, #step2').addClass('completed');
                    $('#step3').addClass('active');
                } else if (filledCount >= 2) {
                    $('#step1').addClass('completed');
                    $('#step2').addClass('active');
                } else {
                    $('#step1').addClass('active');
                }
            }
            
            // Update progress on field change
            $('input, select, textarea').on('change keyup', updateProgress);

            // Space not allow in ID
            $('[id*=IDTextBox]').on("keypress keyup", function (e) {
                if (e.which === 32) return false;
            });

            // Date mask
            $('[id*=BirthDayTextBox]').mask("99/99/9999", { placeholder: 'dd/mm/yyyy' });

            // Copy address
            $("#SameAddrs").click(function (e) {
                e.preventDefault();
                $("[id*=StudentLocalAddressTextBox]").val($("[id*=StudentPermanentAddressTextBox]").val());
            });

            // Student Photo upload
            $('input[name=Student_photo]').change(function (e) {
                var file = e.target.files[0];
                if (file) {
                    canvasResize(file, {
                        width: 250,
                        height: 0,
                        crop: false,
                        quality: 50,
                        callback: function (data) {
                            $("[id*=Imge_HF]").val(data.split(",")[1]);
                        }
                    });
                }
            });

            // Guardian Photo upload
            $('input[name=Guardian_photo]').change(function (e) {
                var file = e.target.files[0];
                if (file) {
                    canvasResize(file, {
                        width: 250,
                        height: 0,
                        crop: false,
                        quality: 50,
                        callback: function (data) {
                            $("[id*=Guardian_Imge_HF]").val(data.split(",")[1]);
                        }
                    });
                }
            });

            // Disable submit button after click
            $('form').submit(function () {
                $(".btn").prop("disabled", true);
                setTimeout(function () {
                    $(".btn").prop('disabled', false);
                }, 5000);
                return true;
            });

            // Prevent back button
            function noBack() {
                window.history.forward();
            }
            noBack();
            window.onload = noBack;
            window.onpageshow = function (evt) {
                if (evt.persisted) noBack();
            }
            window.onunload = function () { void (0); }

            // Show/Hide Group, Section, Shift dropdowns based on Class selection
            function toggleAcademicDropdowns() {
                var classValue = $('[id*=ClassDropDownList]').val();
                
                if (classValue && classValue !== '0') {
                    // Show dropdowns when class is selected
                    $('.group-dropdown-wrapper').fadeIn(300);
                    $('.section-dropdown-wrapper').fadeIn(300);
                    $('.shift-dropdown-wrapper').fadeIn(300);
                } else {
                    // Hide dropdowns when no class is selected
                    $('.group-dropdown-wrapper').fadeOut(300);
                    $('.section-dropdown-wrapper').fadeOut(300);
                    $('.shift-dropdown-wrapper').fadeOut(300);
                }
            }
            
            // Check on page load
            toggleAcademicDropdowns();
            
            // Check when class dropdown changes
            $('[id*=ClassDropDownList]').on('change', function() {
                toggleAcademicDropdowns();
            });
        });
    </script>

    <!-- Custom CSS for Admission Form -->
    <style>
        /* Action Buttons Section Styling */
        .action-buttons {
            position: sticky;
            bottom: 0;
            background: linear-gradient(to bottom, #f8f9fa 0%, #ffffff 100%);
            padding: 25px 30px;
            margin: 30px -30px -30px -30px;
            border-top: 3px solid #667eea;
            border-radius: 0 0 10px 10px;
            box-shadow: 0 -4px 20px rgba(102, 126, 234, 0.15);
        }
        
        /* Checkbox Container - Upper Section */
        .checkbox-container {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            align-items: center;
            gap: 20px;
            padding: 15px 20px;
            background: white;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            margin-bottom: 20px;
        }
        
        .checkbox-container label {
            display: inline-flex;
            align-items: center;
            padding: 10px 15px;
            background: #f8f9fa;
            border: 2px solid #e0e0e0;
            border-radius: 6px;
            cursor: pointer;
            font-size: 14px;
            font-weight: 500;
            color: #333;
            transition: all 0.3s ease;
            margin: 0;
        }
        
        .checkbox-container label:hover {
            background: #e8f0ff;
            border-color: #667eea;
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(102, 126, 234, 0.2);
        }
        
        .checkbox-container label input[type="checkbox"] {
            width: 18px !important;
            height: 18px !important;
            margin-right: 8px !important;
            cursor: pointer !important;
            display: inline-block !important;
            visibility: visible !important;
            opacity: 1 !important;
            position: relative !important;
            accent-color: #667eea;
        }
        
        .checkbox-container label input[type="checkbox"]:checked {
            background-color: #667eea;
        }
        
        /* Button Container - Lower Section */
        .button-container {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 20px;
            flex-wrap: wrap;
        }
        
        .btn-success {
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%) !important;
            border: none !important;
            padding: 14px 35px !important;
            font-weight: 600 !important;
            font-size: 16px !important;
            border-radius: 8px !important;
            color: white !important;
            box-shadow: 0 4px 12px rgba(17, 153, 142, 0.3) !important;
            transition: all 0.3s ease !important;
        }
        
        .btn-success:hover {
            transform: translateY(-3px) !important;
            box-shadow: 0 6px 20px rgba(17, 153, 142, 0.4) !important;
        }
        
        .btn-primary {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
            border: none !important;
            padding: 14px 35px !important;
            font-weight: 600 !important;
            font-size: 16px !important;
            border-radius: 8px !important;
            color: white !important;
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3) !important;
            transition: all 0.3s ease !important;
        }
        
        .btn-primary:hover {
            transform: translateY(-3px) !important;
            box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4) !important;
        }
        
        @media (max-width: 768px) {
            .checkbox-container {
                flex-direction: column;
                gap: 10px;
            }
            
            .checkbox-container label {
                width: 100%;
                justify-content: center;
            }
            
            .button-container {
                flex-direction: column;
                width: 100%;
            }
            
            .button-container .btn {
                width: 100%;
            }
        }
    </style>
</asp:Content>
