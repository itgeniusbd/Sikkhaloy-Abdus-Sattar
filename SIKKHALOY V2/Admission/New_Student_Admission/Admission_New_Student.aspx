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
                        <div class="autocomplete-dropdown" id="idAutocompleteDropdown"></div>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="IDTextBox" CssClass="EroorSummer" ErrorMessage="Student ID is required" ValidationGroup="1"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="IDTextBox" CssClass="EroorSummer" ErrorMessage="Invalid ID format" ValidationExpression="^[A-Z0-9](?!.*?[^\nA-Z0-9]{2}).*?[A-Z0-9]$" ValidationGroup="1"></asp:RegularExpressionValidator>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">
                            SMS Mobile Number <span class="required-mark">*</span>
                        </label>
                        <asp:TextBox ID="SMSPhoneNoTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" MaxLength="13" placeholder="01XXXXXXXXX"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="Mobile number is required" ValidationGroup="1" Display="Dynamic"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="Invalid mobile number. Use 11 digits (01XXXXXXXXX)" ValidationExpression="^(01[3-9]\d{8})$" ValidationGroup="1" Display="Dynamic"></asp:RegularExpressionValidator>
                        <asp:CustomValidator ID="CustomValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="Mobile number must be exactly 11 digits" ClientValidationFunction="validatePhoneLength" ValidationGroup="1" Display="Dynamic"></asp:CustomValidator>
                    </div>
                    
                    <div class="col-md-4 form-group">
                        <label class="form-label">
                            Session Year <span class="required-mark">*</span>
                        </label>
                        <asp:DropDownList ID="EducationYearDropDownList" runat="server" DataSourceID="EduYearSQL" DataTextField="EducationYear" DataValueField="EducationYearID" CssClass="form-control custom-select">
                        </asp:DropDownList>
                        <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYear, EducationYearID FROM Education_Year WHERE (SchoolID = @SchoolID) ORDER BY SN DESC">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="EducationYearDropDownList" CssClass="EroorSummer" ErrorMessage="Session year is required" InitialValue="0" ValidationGroup="1"></asp:RequiredFieldValidator>
                    </div>
                </div>
                
                <div class="row">
                <div class="col-md-5 form-group">
       <label class="form-label">
         Student's Name <span class="required-mark">*</span>
      </label>
 <asp:TextBox ID="StudentNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
          <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="StudentNameTextBox" CssClass="EroorSummer" ErrorMessage="Student name is required" ValidationGroup="1"></asp:RequiredFieldValidator>
       </div>
        
        <div class="col-md-2 form-group">
            <label class="form-label">
                 Gender <span class="required-mark">*</span>
             </label>
      <div class="gender-radio-wrapper">
       <asp:RadioButtonList ID="GenderRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="gender-radio">
          <asp:ListItem Selected="True">Male</asp:ListItem>
   <asp:ListItem>Female</asp:ListItem>
  </asp:RadioButtonList>
    </div>
   <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="GenderRadioButtonList" CssClass="EroorSummer" ErrorMessage="Gender is required" ValidationGroup="1"></asp:RequiredFieldValidator>
  </div>
       
       <div class="col-md-5 form-group">
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
                        <asp:TextBox ID="FatherPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control phone-field" MaxLength="13" placeholder="01XXXXXXXXX"></asp:TextBox>
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
                        <asp:TextBox ID="MotherPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control phone-field" MaxLength="13" placeholder="01XXXXXXXXX"></asp:TextBox>
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
                        <asp:TextBox ID="SecondGuardianPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control phone-field" MaxLength="13" placeholder="01XXXXXXXXX"></asp:TextBox>
                    </div>
                </div>
            </div>
        </div>

        <!-- Section 4: Previous Institution -->
        <div class="section-card">
            <div class="section-header optional collapsed">
                <h4>
                    <i class="fas fa-school"></i>
                    Previous Education/Institution Information
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
                <asp:UpdatePanel ID="ContainUpdatePanel" runat="server" UpdateMode="Conditional">
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
                                <asp:ControlParameter ControlID="EducationYearDropDownList" Name="Education_YearID" PropertyName="SelectedValue" />
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
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="ClassDropDownList" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="GroupDropDownList" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="SectionDropDownList" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="ShiftDropDownList" EventName="SelectedIndexChanged" />
                    </Triggers>
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
   <asp:ControlParameter ControlID="EducationYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" Type="String" />
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
                <asp:ControlParameter ControlID="EducationYearDropDownList" Name="EducationYearID" PropertyName="SelectedValue" Type="String" />
                <asp:SessionParameter Name="StudentClassID" SessionField="StudentClassID" Type="Int32" />
                <asp:Parameter Name="SubjectID" Type="Int32" />
                <asp:Parameter Name="SubjectType" />
            </InsertParameters>
        </asp:SqlDataSource>

        <!-- Action Buttons -->
        <div class="action-buttons">
   <!-- SMS & Print Options -->
         <div class="checkbox-container">
      <!-- SMS Checkboxes -->
          <label class="sms-checkbox">
         <asp:CheckBox ID="SendAdmissionSMSCheckBox" runat="server" />
          <span>📱 Send admission SMS</span>
       </label>
 
         <!-- Print Checkboxes -->
       <label>
       <asp:CheckBox ID="PrintCheckBox" runat="server" />
           <span>🖨️ Print form (English)</span>
  </label>
    <label>
       <asp:CheckBox ID="BanglaPrintCheckBox" runat="server" />
         <span>🖨️ Print form (বাংলা)</span>
          </label>
            </div>
        
            <!-- SMS Template Link -->
    <div class="sms-template-link-container">
       <div class="alert alert-info d-flex align-items-center justify-content-between">
        <div>
          <i class="fas fa-info-circle"></i>
     <strong>📋 SMS Notification:</strong> Configure admission SMS templates for better communication
       </div>
                 <a href="../../SMS/SMS_Template.aspx"" target="_blank" class="btn btn-sm btn-primary">
                   <i class="fas fa-cog"></i> Manage SMS Templates
 </a>
     </div>
</div>
        
    <!-- Button Container -->
       <div class="button-container">
             <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-success" OnClick="SubmitButton_Click" Text="✓ COMPLETE ADMISSION" ValidationGroup="1" />
    <asp:Button ID="GoPayorderButton" runat="server" CssClass="btn btn-primary" OnClick="GoPayorderButton_Click" Text="💳 SAVE & GO TO PAYMENT" ValidationGroup="1" />
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
    
    <script type="text/javascript">
        // Re-initialize master page functions after this page loads
$(document).ready(function() {
 // Re-bind header color picker and language toggle
        if (typeof initializeHeaderColorPicker === 'function') {
       initializeHeaderColorPicker();
    }
        if (typeof initializeLanguageToggle === 'function') {
     initializeLanguageToggle();
     }
  
   // Initialize collapsible sections
        initializeCollapsibleSections();
    
 // Set initial states for sections
    console.log('Setting initial section states...');
    $('.section-header.required').removeClass('collapsed').find('.icon').css('transform', 'rotate(180deg)');
        $('.section-header.required').next('.section-body').addClass('show');
    
    $('.section-header.optional').addClass('collapsed').find('.icon').css('transform', 'rotate(0deg)');
    $('.section-header.optional').next('.section-body').removeClass('show');
  
  console.log('Initial states set. Required sections:', $('.section-header.required').length);
        console.log('Optional sections:', $('.section-header.optional').length);
        
        // ✅ IMPORTANT: Initialize form events on page load
        console.log('Calling bindClassChangeEvent and bindFormEvents on page load...');
  bindClassChangeEvent();
        bindFormEvents();
   console.log('Form events initialized successfully');
    });
    
    // Initialize collapsible sections - works with UpdatePanel
    function initializeCollapsibleSections() {
        console.log('Initializing collapsible sections...');
        
 // Use event delegation to handle dynamically loaded content
    $(document).off('click.sectionHeader').on('click.sectionHeader', '.section-header', function (e) {
          e.preventDefault();
            e.stopPropagation();

   var $header = $(this);
        var $body = $header.next('.section-body');
      
   console.log('Section header clicked:', $header.find('h4').text());
        console.log('Current collapsed state:', $header.hasClass('collapsed'));
            console.log('Current show state:', $body.hasClass('show'));
            
     // Toggle collapsed class on header
            $header.toggleClass('collapsed');
        
          // Toggle show class on body
            $body.toggleClass('show');
          
            // Rotate chevron icon
      var $icon = $header.find('.icon');
        if ($header.hasClass('collapsed')) {
                $icon.css('transform', 'rotate(0deg)');
     console.log('Section collapsed');
        } else {
    $icon.css('transform', 'rotate(180deg)');
        console.log('Section expanded');
          }
        });
        
 console.log('Collapsible sections initialized. Found', $('.section-header').length, 'headers');
  }

    // Re-initialize after UpdatePanel partial postback
    var prm = Sys.WebForms.PageRequestManager.getInstance();
    prm.add_endRequest(function (sender, args) {
  console.log('UpdatePanel postback completed - rebinding events');
        
        // Re-initialize collapsible sections
        initializeCollapsibleSections();
 
  // Re-set initial states
    $('.section-header.required').removeClass('collapsed').find('.icon').css('transform', 'rotate(180deg)');
  $('.section-header.required').next('.section-body').addClass('show');
     
 // Re-bind class dropdown event
        bindClassChangeEvent();
     
   // ✅ Re-bind all form event handlers after UpdatePanel update
  bindFormEvents();
    });

    function isNumberKey(a) { 
        a = a.which ? a.which : event.keyCode; 
 return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 
    }

    // Validate phone number length
    function validatePhoneLength(source, args) {
        var phoneNumber = args.Value.trim();
 
   // Remove any non-digit characters
        phoneNumber = phoneNumber.replace(/\D/g, '');
        
  // Check if it's exactly 11 digits starting with 01
      if (phoneNumber.length === 11 && phoneNumber.substring(0, 2) === '01') {
  // Check if 3rd digit is valid (3-9)
            var thirdDigit = parseInt(phoneNumber.charAt(2));
    if (thirdDigit >= 3 && thirdDigit <= 9) {
     args.IsValid = true;
      return;
            }
        }
    
  // Check if it's 13 digits starting with 88
      if (phoneNumber.length === 13 && phoneNumber.substring(0, 2) === '88') {
        // Check if after 88, it starts with 01 and 5th digit is 3-9
            if (phoneNumber.substring(2, 4) === '01') {
   var fifthDigit = parseInt(phoneNumber.charAt(4));
         if (fifthDigit >= 3 && fifthDigit <= 9) {
           args.IsValid = true;
    return;
 }
 }
        }
  
        args.IsValid = false;
    }

    // Auto-format phone number as user types
    function formatPhoneNumber(input) {
   var value = input.value.replace(/\D/g, '');
  
        // Limit to 13 digits max
        if (value.length > 13) {
  value = value.substring(0, 13);
        }
        
        input.value = value;
    }

    // Show/Hide Group, Section, Shift dropdowns based on Class selection and data availability
 function toggleAcademicDropdowns() {
        var classValue = $('[id$=ClassDropDownList]').val();
  
        console.log('toggleAcademicDropdowns called, classValue:', classValue);
        
        if (classValue && classValue !== '0') {
// Check Group dropdown - show only if has actual data items
        // Use more specific selector to avoid BloodGroupDropDownList
      var groupDropdown = $('[id$=GroupDropDownList]').not('[id*=Blood]');
            var groupOptions = groupDropdown.find('option');
     var hasGroupData = false;
  
          console.log('Group options count:', groupOptions.length);
       
       // Check if there are options other than [ ALL ] or placeholder
   groupOptions.each(function() {
    var optionText = $(this).text().trim();
      var optionValue = $(this).val();
           console.log('Group option - Text:', optionText, 'Value:', optionValue);
     
            // If option is not [ ALL ] or % (wildcard), then we have real data
      if (optionValue !== '%' && optionText !== '[ ALL ]' && optionValue !== '' && optionText !== '') {
     hasGroupData = true;
          console.log('Group has real data:', optionText);
     return false; // break the loop
   }
   });
      
            console.log('hasGroupData:', hasGroupData);
 
          if (hasGroupData) {
    console.log('Showing Group dropdown');
             $('.group-dropdown-wrapper').show().css('display', 'block');
      } else {
console.log('Hiding Group dropdown');
 $('.group-dropdown-wrapper').hide().css('display', 'none');
            }
            
            // Check Section dropdown - show only if has actual data items
            var sectionDropdown = $('[id$=SectionDropDownList]');
          var sectionOptions = sectionDropdown.find('option');
       var hasSectionData = false;
    
      console.log('Section options count:', sectionOptions.length);
       
            sectionOptions.each(function() {
          var optionText = $(this).text().trim();
   var optionValue = $(this).val();
          console.log('Section option - Text:', optionText, 'Value:', optionValue);
       
                if (optionValue !== '%' && optionText !== '[ ALL ]' && optionValue !== '' && optionText !== '') {
  hasSectionData = true;
  console.log('Section has real data:', optionText);
   return false;
    }
            });

    console.log('hasSectionData:', hasSectionData);
 
if (hasSectionData) {
          console.log('Showing Section dropdown');
      $('.section-dropdown-wrapper').show().css('display', 'block');
            } else {
          console.log('Hiding Section dropdown');
    $('.section-dropdown-wrapper').hide().css('display', 'none');
   }
    
        // Check Shift dropdown - show only if has actual data items
var shiftDropdown = $('[id$=ShiftDropDownList]');
    var shiftOptions = shiftDropdown.find('option');
            var hasShiftData = false;
  
   console.log('Shift options count:', shiftOptions.length);
      
         shiftOptions.each(function() {
    var optionText = $(this).text().trim();
           var optionValue = $(this).val();
console.log('Shift option - Text:', optionText, 'Value:', optionValue);
      
  if (optionValue !== '%' && optionText !== '[ ALL ]' && optionValue !== '' && optionText !== '') {
     hasShiftData = true;
 console.log('Shift has real data:', optionText);
           return false;
    }
 });
    
   console.log('hasShiftData:', hasShiftData);
    
            if (hasShiftData) {
          console.log('Showing Shift dropdown');
    $('.shift-dropdown-wrapper').show().css('display', 'block');
     } else {
         console.log('Hiding Shift dropdown');
    $('.shift-dropdown-wrapper').hide().css('display', 'none');
            }
        } else {
       console.log('No class selected, hiding all dropdowns');
         // Hide all dropdowns when no class is selected
     $('.group-dropdown-wrapper').hide().css('display', 'none');
            $('.section-dropdown-wrapper').hide().css('display', 'none');
   $('.shift-dropdown-wrapper').hide().css('display', 'none');
        }
    }

    // Function to bind events - called on page load and after UpdatePanel updates
    function bindClassChangeEvent() {
    console.log('bindClassChangeEvent called');
 
        // Check on page load
        toggleAcademicDropdowns();
    
        // Unbind first to prevent multiple bindings
        $('[id$=ClassDropDownList]').off('change').on('change', function() {
            console.log('Class dropdown changed');
  toggleAcademicDropdowns();
        });
        
        // Also bind to Group, Section, Shift changes to re-check visibility
        $('[id$=GroupDropDownList]').not('[id*=Blood]').off('change').on('change', function() {
    console.log('Group dropdown changed');
          toggleAcademicDropdowns();
      });
        
        $('[id$=SectionDropDownList]').off('change').on('change', function() {
    console.log('Section dropdown changed');
    toggleAcademicDropdowns();
        });
     
        $('[id$=ShiftDropDownList]').off('change').on('change', function() {
    console.log('Shift dropdown changed');
   toggleAcademicDropdowns();
        });
    }
    
    // Bind all form events
    function bindFormEvents() {
        console.log('bindFormEvents called - setting up date formatting...');
      
    // Progress indicator
   function updateProgress() {
     var filledCount = 0;
 var totalRequired = 5;
     
    if ($('[id$=IDTextBox]').val()) filledCount++;
       if ($('[id$=SMSPhoneNoTextBox]').val()) filledCount++;
  if ($('[id$=StudentNameTextBox]').val()) filledCount++;
     if ($('[id$=FatherNameTextBox]').val()) filledCount++;
      if ($('[id$=ClassDropDownList]').val() !== '0') filledCount++;
    
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
 
        $('input, select, textarea').off('change.progress keyup.progress').on('change.progress keyup.progress', updateProgress);

        // Space not allow in ID
 $('[id$=IDTextBox]').off('keypress.idspace keyup.idspace').on("keypress.idspace keyup.idspace", function (e) {
      if (e.which === 32) return false;
        });

        // Student ID Autocomplete
        var idTimeout;
        var currentIdIndex = -1;
  
        $('[id$=IDTextBox]').off('keyup.autocomplete').on("keyup.autocomplete", function(e) {
            // Prevent space
     if (e.which === 32) {
     $(this).val($(this).val().replace(/\s/g, ''));
  return false;
     }
      
    // Handle arrow keys and enter for autocomplete
          var dropdown = $('#idAutocompleteDropdown');
      var items = dropdown.find('.autocomplete-item');
 
 if (e.which === 40) { // Arrow Down
     e.preventDefault();
    if (items.length > 0) {
     currentIdIndex = (currentIdIndex + 1) % items.length;
              items.removeClass('active');
       items.eq(currentIdIndex).addClass('active');
       }
      return;
  } else if (e.which === 38) { // Arrow Up
              e.preventDefault();
      if (items.length > 0) {
   currentIdIndex = currentIdIndex <= 0 ? items.length - 1 : currentIdIndex - 1;
    items.removeClass('active');
        items.eq(currentIdIndex).addClass('active');
   }
    return;
       } else if (e.which === 13) { // Enter
    e.preventDefault();
       if (currentIdIndex >= 0 && items.length > 0) {
  $(this).val(items.eq(currentIdIndex).text());
      dropdown.hide();
       currentIdIndex = -1;
  }
    return;
    } else if (e.which === 27) { // Escape
    dropdown.hide();
     currentIdIndex = -1;
   return;
 }
          
  var searchText = $(this).val().trim();
 
       // Clear previous timeout
       clearTimeout(idTimeout);
     
      // Hide dropdown if empty
   if (searchText.length < 1) {
    dropdown.hide();
  // Remove validation styling
   $(this).removeClass('is-invalid is-valid id-duplicate');
      return;
      }

            // Set new timeout for duplicate check and AJAX call
            idTimeout = setTimeout(function() {
   var $idTextBox = $('[id$=IDTextBox]');
     
    // First check for duplicates
     $.ajax({
      type: "POST",
          url: "Admission_New_Student.aspx/CheckIDExists",
   data: JSON.stringify({ studentID: searchText }),
  contentType: "application/json; charset=utf-8",
     dataType: "json",
         success: function(response) {
  var result = JSON.parse(response.d);
    
          if (result.exists) {
       // ID already exists - show error
            $idTextBox.addClass('is-invalid id-duplicate').removeClass('is-valid');
      
      // Show warning in dropdown
      var warningHtml = '<div class="autocomplete-item autocomplete-warning" style="background-color: #f8d7da; color: #721c24; font-weight: bold; cursor: default;">';
      warningHtml += '<i class="fas fa-exclamation-triangle"></i> ⚠️ This ID already exists!';
       warningHtml += '</div>';
dropdown.html(warningHtml).show();
  } else {
  // ID is available
    $idTextBox.removeClass('is-invalid id-duplicate').addClass('is-valid');
  
   // Now show autocomplete suggestions
         $.ajax({
       type: "POST",
       url: "Admission_New_Student.aspx/GetAllID",
            data: JSON.stringify({ ids: searchText }),
      contentType: "application/json; charset=utf-8",
     dataType: "json",
 success: function(response) {
      var ids = JSON.parse(response.d);
 
     if (ids && ids.length > 0) {
 var html = '';
      $.each(ids, function(index, id) {
 html += '<div class="autocomplete-item" data-value="' + id + '">' + id + '</div>';
          });
      
       dropdown.html(html).show();
     currentIdIndex = -1;
 
          // Click handler for items
      dropdown.find('.autocomplete-item').on('click', function() {
  $('[id$=IDTextBox]').val($(this).text());
     dropdown.hide();
       currentIdIndex = -1;
        // Re-check if selected ID exists
        $('[id$=IDTextBox]').trigger('keyup');
   });
        
    // Hover handler
   dropdown.find('.autocomplete-item').on('mouseenter', function() {
dropdown.find('.autocomplete-item').removeClass('active');
         $(this).addClass('active');
   currentIdIndex = $(this).index();
     });
   } else {
    dropdown.hide();
  }
 },
      error: function() {
       dropdown.hide();
     }
           });
     }
       },
   error: function() {
     $idTextBox.removeClass('is-invalid is-valid id-duplicate');
       dropdown.hide();
          }
   });
}, 500);
      });
          
        // Hide dropdown when clicking outside
        $(document).off('click.autocomplete').on('click.autocomplete', function(e) {
         if (!$(e.target).closest('[id$=IDTextBox], #idAutocompleteDropdown').length) {
 $('#idAutocompleteDropdown').hide();
   currentIdIndex = -1;
   }
     });

        // Validate ID before form submission
        $('form').off('submit.idvalidation').on('submit.idvalidation', function(e) {
   var $idTextBox = $('[id$=IDTextBox]');
     var studentID = $idTextBox.val().trim();
   
     if (studentID === '') {
      return true;
      }
        
    // Check if ID has duplicate class
     if ($idTextBox.hasClass('id-duplicate')) {
  e.preventDefault();
   alert('⚠️ Student ID "' + studentID + '" already exists!\n\nPlease use a different ID.');
   $idTextBox.focus();
      return false;
          }
  
     return true;
 });
        
        // Copy permanent address to local address
  $('#SameAddrs').off('click.copyaddress').on('click.copyaddress', function(e) {
e.preventDefault();
            var permanentAddress = $('[id$=StudentPermanentAddressTextBox]').val();
 $('[id$=StudentLocalAddressTextBox]').val(permanentAddress);
   });

        // ✅ Date of Birth auto-formatting
        console.log('Setting up Date of Birth formatting...');
   var $birthDayTextBox = $('[id$=BirthDayTextBox]');
        console.log('Found BirthDayTextBox:', $birthDayTextBox.length);
 console.log('BirthDayTextBox ID:', $birthDayTextBox.attr('id'));
        console.log('BirthDayTextBox type:', $birthDayTextBox.attr('type'));
        
  // Auto-format Date of Birth with "/" separator (dd/mm/yyyy)
        $birthDayTextBox.off('input.dateformat').on('input.dateformat', function(e) {
     console.log('✅ Date input event triggered! Value:', $(this).val());
 var input = $(this);
      var value = input.val();
     var numbers = value.replace(/[^0-9]/g, ''); // Keep only numbers
      var cursorPosition = this.selectionStart;
      var oldValue = value;
   var oldLength = oldValue.length;
       
            console.log('Numbers extracted:', numbers);
         console.log('Cursor position before:', cursorPosition);
            
  // Build formatted date
            var formatted = '';
  if (numbers.length > 0) {
  // DD
         formatted = numbers.substring(0, 2);
    console.log('After DD:', formatted);
         
     // Add first slash after DD if we have month digits
 if (numbers.length >= 3) {
     formatted += '/' + numbers.substring(2, 4); // MM
          console.log('After MM:', formatted);
                }
             
   // Add second slash after MM if we have year digits
            if (numbers.length >= 5) {
          formatted += '/' + numbers.substring(4, 8); // YYYY
console.log('After YYYY:', formatted);
   }
            }
          
            // Limit to dd/mm/yyyy format (10 characters max)
if (formatted.length > 10) {
         formatted = formatted.substring(0, 10);
            }
            
     console.log('Final formatted value:', formatted);
     
        // Update input value
   input.val(formatted);
   
       // Fix cursor position
    var newLength = formatted.length;
            var newCursorPosition = cursorPosition;
        
  // If we just added a slash at position 2 or 5, move cursor after the slash
      if (formatted.length > oldLength) {
     // A slash was added
       if (cursorPosition === 2 && formatted.charAt(2) === '/') {
             // First slash was added after DD
newCursorPosition = 3; // Move cursor after the slash
     } else if (cursorPosition === 5 && formatted.charAt(5) === '/') {
        // Second slash was added after MM
    newCursorPosition = 6; // Move cursor after the slash
  } else {
    // Normal case - maintain cursor position accounting for new length
newCursorPosition = cursorPosition + (newLength - oldLength);
  }
            } else if (formatted.length < oldLength) {
       // User deleted something - adjust cursor back
            newCursorPosition = Math.max(0, cursorPosition);
   }
    
       console.log('Cursor position after:', newCursorPosition);
    
     // Set cursor position
            this.selectionStart = this.selectionEnd = newCursorPosition;
        });

        // Also try binding with 'keyup' as fallback
        $birthDayTextBox.off('keyup.dateformat').on('keyup.dateformat', function(e) {
        console.log('⚡ Keyup event triggered! Key:', e.which, 'Value:', $(this).val());
         
    // Skip special keys
    if (e.which === 8 || e.which === 46 || e.which === 9 || e.which === 27 || 
   e.which === 13 || e.which === 37 || e.which === 39) {
          return;
 }
   
        // Trigger the input event manually
      $(this).trigger('input');
 });
        
        // Only allow numbers and slashes in Date of Birth (prevent other characters)
        $birthDayTextBox.off('keydown.dateformat').on('keydown.dateformat', function(e) {
   console.log('⌨️ Keydown event! Key code:', e.which);
   var charCode = e.which || e.keyCode;
            
 // Allow: backspace(8), delete(46), tab(9), escape(27), enter(13), left arrow(37), right arrow(39)
            if (charCode === 8 || charCode === 46 || charCode === 9 || charCode === 27 || 
    charCode === 13 || charCode === 37 || charCode === 39) {
      return true;
          }
     
     // Allow Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
            if ((e.ctrlKey || e.metaKey) && (charCode === 65 || charCode === 67 || charCode === 86 || charCode === 88)) {
        return true;
  }
    
            // Prevent slash key (we auto-add it)
          if (charCode === 191 || charCode === 111) {
     e.preventDefault();
    return false;
        }
            
            // Allow only numbers (0-9)
            if ((charCode >= 48 && charCode <= 57) || (charCode >= 96 && charCode <= 105)) {
       return true;
            }
   
       // Block everything else
            console.log('❌ Blocking key:', charCode);
            e.preventDefault();
            return false;
        });

        // Handle paste event
        $birthDayTextBox.off('paste.dateformat').on('paste.dateformat', function(e) {
      console.log('📋 Paste event triggered!');
            e.preventDefault();

          // Get pasted text
   var pastedText = (e.originalEvent || e).clipboardData.getData('text/plain');
            console.log('Pasted text:', pastedText);
    var numbers = pastedText.replace(/[^0-9]/g, ''); // Extract only numbers
       console.log('Numbers from paste:', numbers);
  
 if (numbers.length > 8) {
           numbers = numbers.substring(0, 8);
            }
          
       // Format the pasted numbers
    var formatted = '';
            if (numbers.length > 0) {
                formatted = numbers.substring(0, 2);
 if (numbers.length >= 3) {
        formatted += '/' + numbers.substring(2, 4);
      }
         if (numbers.length >= 5) {
               formatted += '/' + numbers.substring(4, 8);
                }
            }
   
   console.log('Formatted paste result:', formatted);
            $(this).val(formatted);
 
        // Trigger input event to ensure validation runs
     $(this).trigger('input');
  });
      
  // Test if element is actually focused
        $birthDayTextBox.on('focus.dateformat', function() {
     console.log('🎯 Date of Birth field focused!');
        });
        
        console.log('Date of Birth formatting setup complete');
    }

    </script></asp:Content>