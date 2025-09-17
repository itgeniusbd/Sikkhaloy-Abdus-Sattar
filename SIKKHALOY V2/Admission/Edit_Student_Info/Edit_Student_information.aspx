<%@ Page Title="Update Student's Info" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Edit_Student_information.aspx.cs" Inherits="EDUCATION.COM.ADMISSION_REGISTER.Edit_Student_information" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .badge { font-size: 18px; padding: 8px 20px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="card">
        <div class="card-header">
            <div class="form-inline">
                <div class="form-group mb-0">
                    <asp:TextBox ID="IDTextBox" runat="server" autocomplete="off" CssClass="form-control" placeholder="Enter Student ID" />
                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ControlToValidate="IDTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="id" />
                    <asp:SqlDataSource ID="ShowIDSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT StudentsClass.StudentClassID, StudentsClass.StudentID, Student.StudentImageID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID WHERE (Student.ID = @ID) AND (Student.Status = N'Active') AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.SchoolID = @SchoolID)">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" Type="String" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                    <asp:Button ID="IDFindButton" runat="server" CssClass="btn btn-primary" OnClick="IDFindButton_Click" Text="Find" ValidationGroup="id" />
                </div>
            </div>
        </div>
        <asp:FormView ID="StudentInfoFormView" DefaultMode="Edit" runat="server" DataKeyNames="StudentClassID" DataSourceID="StudentInfoSQL" Width="100%">
            <EditItemTemplate>
                <div class="card-body">
                    <h4 class="badge stylish-color-dark"><%# Eval("StudentsName") %></h4>
                    <h4 class="badge stylish-color-dark mx-2">ID:<%#Eval("ID") %></h4>
                    <h4 class="badge stylish-color-dark">CLASS: <%#Eval("Class") %></h4>

                    <input id="ImgID" type="hidden" value="<%#Eval("StudentImageID") %>" />
                    <br />
                    <img src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="rounded-circle img-thumbnail z-depth-1 S_img" style="width: 150px; height: 150px;" />
                    <div class="NoPrint form-group">
                        <label>Student&#39;s photo</label><br />
                        <input name="Student_photo" type="file" accept=".png,.jpg" />
                    </div>

                    <div class="form-group">
                        <label>SMS Phone Number</label>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="Enter SMS Phone Number" ValidationGroup="1">*</asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="SMS মোবাইল নাম্বার সঠিক নয়" ValidationExpression="(88)?((011)|(015)|(016)|(017)|(018)|(019)|(013)|(014))\d{8,8}" ValidationGroup="1">*</asp:RegularExpressionValidator>
                        <asp:TextBox ID="SMSPhoneNoTextBox" onkeypress="return isNumberKey(event)" runat="server" Text='<%# Bind("SMSPhoneNo") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Student's Name</label>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="StudentsNameTextBox" CssClass="EroorSummer" ErrorMessage="Enter Student Name" ValidationGroup="1"></asp:RequiredFieldValidator>
                        <asp:TextBox ID="StudentsNameTextBox" runat="server" Text='<%# Bind("StudentsName") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Gender</label>
                        <asp:RadioButtonList ID="GenderRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%#Bind("Gender") %>'>
                            <asp:ListItem>Male</asp:ListItem>
                            <asp:ListItem>Female</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="form-group">
                        <label>Date of Birth</label>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator6" runat="server" ControlToValidate="DateofBirthTextBox" CssClass="EroorSummer" ErrorMessage="Invalid Format" ValidationExpression="^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$" ValidationGroup="1"></asp:RegularExpressionValidator>
                        <asp:TextBox ID="DateofBirthTextBox" runat="server" Text='<%# Bind("DateofBirth") %>' CssClass="form-control" />
                    </div>
                        <div class="form-group">
                        <label>Legal Identity No.</label>
                       
                            <asp:TextBox ID="Legal_IdentityTextBox" runat="server" Text='<%# Bind("Legal_Identity") %>' placeholder="NID : 20155485444/ BRN : 2012545544554" CssClass="form-control" />
                    </div>

                    <div class="form-group">
                        <label>Blood Group</label>
                        <asp:DropDownList ID="BloodGroupDropDownList"  SelectedValue='<%#Bind("BloodGroup") %>' runat="server" CssClass="form-control">
                        <asp:ListItem>Unknown</asp:ListItem>
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
                    <div class="form-group">
                        <label>Religion</label>
                        <asp:DropDownList ID="ReligionDropDownList" SelectedValue='<%#Bind("Religion") %>' runat="server" CssClass="form-control">
                            <asp:ListItem>Islam</asp:ListItem>
                            <asp:ListItem>Hinduism</asp:ListItem>
                            <asp:ListItem>Buddhism</asp:ListItem>
                            <asp:ListItem>Christianity</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="form-group">
                        <label>Student's Local Address</label>
                        <asp:TextBox ID="StudentsLocalAddressTextBox" runat="server" Text='<%# Bind("StudentsLocalAddress") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Student's Permanent Address</label>
                        <asp:TextBox ID="StudentPermanentAddressTextBox" runat="server" Text='<%# Bind("StudentPermanentAddress") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Student's Email Address</label>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator5" runat="server" ControlToValidate="StudentEmailAddressTextBox" CssClass="EroorSummer" ErrorMessage="Invalid Email" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ValidationGroup="1"></asp:RegularExpressionValidator>
                        <asp:TextBox ID="StudentEmailAddressTextBox" runat="server" Text='<%# Bind("StudentEmailAddress") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Other Details of Student(If any)</label>
                        <asp:TextBox ID="OtherDetailsTextBox" runat="server" Text='<%# Bind("OtherDetails") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Prev. Institution Name</label>
                        <asp:TextBox ID="PrevSchoolNameTextBox" runat="server" Text='<%# Bind("PrevSchoolName") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Prev. Class</label>
                        <asp:TextBox ID="PrevClassTextBox" runat="server" Text='<%# Bind("PrevClass") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Prev. Exam Year</label>
                        <asp:TextBox ID="PrevExamYearTextBox" onkeypress="return isNumberKey(event)" runat="server" Text='<%# Bind("PrevExamYear") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Prev.Exam Grade</label>
                        <asp:TextBox ID="PrevExamGradeTextBox" runat="server" Text='<%# Bind("PrevExamGrade") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Mother's Name</label>
                        <asp:TextBox ID="MothersNameTextBox" runat="server" Text='<%# Bind("MothersName") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Mother's Occupation</label>
                        <asp:TextBox ID="MotherOccupationTextBox" runat="server" Text='<%# Bind("MotherOccupation") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Mother's Phone Number</label><asp:TextBox ID="MotherPhoneNumberTextBox" onkeypress="return isNumberKey(event)" runat="server" Text='<%# Bind("MotherPhoneNumber") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Father's Name</label>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="FathersNameTextBox" CssClass="EroorSummer" ErrorMessage="Enter Father's  Name" ValidationGroup="1"></asp:RequiredFieldValidator>
                        <asp:TextBox ID="FathersNameTextBox" runat="server" Text='<%# Bind("FathersName") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Father's Occupation</label>
                        <asp:TextBox ID="FatherOccupationTextBox" runat="server" Text='<%# Bind("FatherOccupation") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Father's Phone Number</label><asp:TextBox ID="FatherPhoneNumberTextBox" onkeypress="return isNumberKey(event)" runat="server" Text='<%# Bind("FatherPhoneNumber") %>' CssClass="form-control" />
                    </div>

                    <img src="/Handeler/Guardian_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="rounded-circle img-thumbnail z-depth-1 g_img" style="width: 150px; height: 150px;" />
                    <div class="form-group NoPrint">
                        <label>Guardian&#39;s photo</label><br />
                        <input name="Guardian_photo" type="file" accept=".png,.jpg" />
                    </div>

                    <div class="form-group">
                        <label>2nd Guardian Name</label>
                        <asp:TextBox ID="GuardianNameTextBox" runat="server" Text='<%# Bind("GuardianName") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Relationship with Student</label>
                        <asp:TextBox ID="GuardianRelationshipwithStudentTextBox" runat="server" Text='<%# Bind("GuardianRelationshipwithStudent") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>2nd Guardian Phone Number</label><asp:TextBox ID="GuardianPhoneNumberTextBox" onkeypress="return isNumberKey(event)" runat="server" Text='<%# Bind("GuardianPhoneNumber") %>' CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <asp:LinkButton ID="UpdateButton" runat="server" CssClass="btn btn-primary" CausesValidation="True" CommandName="Update" Text="Update" ValidationGroup="1" />
                        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="EroorSummer" DisplayMode="List" ShowMessageBox="True" ValidationGroup="1" />
                    </div>
                </div>
            </EditItemTemplate>
        </asp:FormView>
    </div>
    <asp:SqlDataSource ID="StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT Student.ID, Student.StudentsName, Student.Gender,CONVERT(varchar,Student.DateofBirth, 103) AS DateofBirth ,Legal_Identity,Student.BloodGroup, Student.Religion, Student.StudentsLocalAddress, Student.PrevSchoolName, Student.PrevClass, Student.PrevExamYear, Student.PrevExamGrade, Student.MothersName, Student.FathersName, Student.GuardianName, Student.SMSPhoneNo, Student.MotherOccupation, Student.MotherPhoneNumber, Student.FatherOccupation, Student.FatherPhoneNumber, Student.GuardianRelationshipwithStudent, Student.GuardianPhoneNumber, Student.OtherDetails, Student.StudentPermanentAddress, Student.StudentEmailAddress, StudentsClass.StudentClassID, StudentsClass.StudentID, CreateClass.Class, Student.StudentImageID FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID INNER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.SchoolID = @SchoolID) AND (StudentsClass.StudentClassID = @StudentClassID) AND (StudentsClass.StudentID = @StudentID)"
        UpdateCommand="UPDATE Student SET SMSPhoneNo = @SMSPhoneNo, StudentsName = @StudentsName, StudentEmailAddress = @StudentEmailAddress, Gender = @Gender, DateofBirth = CONVERT(date,@DateofBirth,105),Legal_Identity=@Legal_Identity, BloodGroup = @BloodGroup, Religion = @Religion, StudentPermanentAddress = @StudentPermanentAddress, StudentsLocalAddress = @StudentsLocalAddress, PrevSchoolName = @PrevSchoolName, PrevClass = @PrevClass, PrevExamYear = @PrevExamYear, PrevExamGrade = @PrevExamGrade, MothersName = @MothersName, MotherOccupation = @MotherOccupation, MotherPhoneNumber = @MotherPhoneNumber, FathersName = @FathersName, FatherOccupation = @FatherOccupation, FatherPhoneNumber = @FatherPhoneNumber, GuardianName = @GuardianName, GuardianRelationshipwithStudent = @GuardianRelationshipwithStudent, GuardianPhoneNumber = @GuardianPhoneNumber, OtherDetails = @OtherDetails WHERE (StudentID = @StudentID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:QueryStringParameter Name="StudentClassID" QueryStringField="Student_Class" />
            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="SMSPhoneNo" />
            <asp:Parameter Name="StudentsName" Type="String" />
            <asp:Parameter Name="StudentEmailAddress" />
            <asp:Parameter Name="Gender" Type="String" />
            <asp:Parameter Name="DateofBirth" Type="String" />
            <asp:Parameter Name="Legal_Identity" Type="String" />
            <asp:Parameter Name="BloodGroup" Type="String" />
            <asp:Parameter Name="Religion" Type="String" />
            <asp:Parameter Name="StudentPermanentAddress" />
            <asp:Parameter Name="StudentsLocalAddress" Type="String" />
            <asp:Parameter Name="PrevSchoolName" Type="String" />
            <asp:Parameter Name="PrevClass" Type="String" />
            <asp:Parameter Name="PrevExamYear" Type="String" />
            <asp:Parameter Name="PrevExamGrade" Type="String" />
            <asp:Parameter Name="MothersName" Type="String" />
            <asp:Parameter Name="MotherOccupation" />
            <asp:Parameter Name="MotherPhoneNumber" />
            <asp:Parameter Name="FathersName" Type="String" />
            <asp:Parameter Name="FatherOccupation" />
            <asp:Parameter Name="FatherPhoneNumber" />
            <asp:Parameter Name="GuardianName" Type="String" />
            <asp:Parameter Name="GuardianRelationshipwithStudent" />
            <asp:Parameter Name="GuardianPhoneNumber" />
            <asp:Parameter Name="OtherDetails" />
            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
        </UpdateParameters>
    </asp:SqlDataSource>

    <script src="/JS/DateMask.js"></script>
    <script>
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
        $(function () {
            $('[id*=DateofBirthTextBox]').mask("99/99/9999", { placeholder: 'dd/mm/yyyy' });

            //Autocomplete
            $('[id*=IDTextBox]').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "/Handeler/Student_IDs.asmx/GetStudentID",
                        data: JSON.stringify({ 'ids': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            result($.map(JSON.parse(response.d), function (item) {
                                return item;
                            }));
                        }
                    });
                }
            });

            //Student Photo
            $('input[name=Student_photo]').change(function (input) {
                var file = input.target.files[0];
                var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
                    alert('Please upload file having extensions .jpeg/.jpg/.png only');
                    return false;
                }
                else {
                    canvasResize(file, {
                        width: 250,
                        quality: 70,
                        callback: function (idata) {
                            $('.S_img').attr('src', idata);

                            $.ajax({
                                url: "Edit_Student_information.aspx/Update_Student_Image",
                                data: JSON.stringify({ 'StudentImageID': $('#ImgID').val(), 'Image': idata.split(",")[1] }),
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8"
                            });
                        }
                    });
                }
            });

            //Guardian Photo
            $('input[name=Guardian_photo]').change(function (input) {
                var file = input.target.files[0];
                var Valid = ["image/jpg", "image/jpeg", "image/png"];

                if ($.inArray(file["type"], Valid) < 0) {
                    alert('Please upload file having extensions .jpeg/.jpg/.png only');
                    return false;
                }
                else {
                    canvasResize(file, {
                        width: 250,
                        quality: 70,
                        callback: function (idata) {
                            $('.g_img').attr('src', idata);
                            $.ajax({
                                url: "Edit_Student_information.aspx/Update_Guardian_Image",
                                data: JSON.stringify({ 'StudentImageID': $('#ImgID').val(), 'Image': idata.split(",")[1] }),
                                dataType: "json",
                                type: "POST",
                                contentType: "application/json; charset=utf-8"
                            });
                        }
                    });
                }
            });
        });
    </script>
</asp:Content>

