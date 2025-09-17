<%@ Page Title="Admission New Student" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Admission_New_Student.aspx.cs" Inherits="EDUCATION.COM.ADMISSION_REGISTER.Admission_New_Student" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../CSS/Admission_New_Student.css" rel="stylesheet" />
    <style>
        body { background-color: #eee !important; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>New Student Admission</h3>

    <div class="card">
        <div class="card-body">
            <div class="alert alert-info">
                Student Information <span class="badge badge-primary">
                    <asp:Label ID="LastIDLabel" Font-Size="Medium" runat="server"></asp:Label></span>
            </div>
            <div class="form-row">
                <div class="col-lg-4 form-group">
                    <label>
                        Student ID*
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ControlToValidate="IDTextBox" CssClass="EroorSummer" ErrorMessage="Invalid" ValidationExpression="^[A-Z0-9](?!.*?[^\nA-Z0-9]{2}).*?[A-Z0-9]$" ValidationGroup="1"></asp:RegularExpressionValidator>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="IDTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator></label>
                    <asp:TextBox ID="IDTextBox" onDrop="blur();return false;" onpaste="return false" autocomplete="off" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg-4 form-group">
                    <label>
                        SMS Mobile Number*
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ControlToValidate="SMSPhoneNoTextBox" CssClass="EroorSummer" ErrorMessage="Invalid" ValidationExpression="(88)?((011)|(015)|(016)|(017)|(018)|(019)|(013)|(014))\d{8,8}" ValidationGroup="1"></asp:RegularExpressionValidator>
                    </label>
                    <asp:TextBox ID="SMSPhoneNoTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg-4 form-group">
                    <label>Session Year*<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="EducationYearDropDownList" CssClass="EroorSummer" ErrorMessage="*" InitialValue="0" ValidationGroup="1"></asp:RequiredFieldValidator></label>
                    <asp:DropDownList ID="EducationYearDropDownList" runat="server" DataSourceID="EduYearSQL" DataTextField="EducationYear" DataValueField="EducationYearID" CssClass="form-control">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="EduYearSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYear, EducationYearID FROM Education_Year WHERE (SchoolID = @SchoolID) ORDER BY SN">
                        <SelectParameters>
                            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>
            <div class="form-row">
                <div class="col-lg form-group">
                    <label>Student's Name*<asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ControlToValidate="StudentNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator></label>
                    <asp:TextBox ID="StudentNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Student&#39;s Email<asp:RegularExpressionValidator ID="RegularExpressionValidator5" runat="server" ControlToValidate="StudentEmailTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ValidationGroup="1"></asp:RegularExpressionValidator></label>
                    <asp:TextBox ID="StudentEmailTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Gender*<asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ControlToValidate="GenderRadioButtonList" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator></label>
                    <asp:RadioButtonList ID="GenderRadioButtonList" runat="server" RepeatDirection="Horizontal">
                        <asp:ListItem>Male</asp:ListItem>
                        <asp:ListItem>Female</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
            </div>
            <div class="form-row">
                <div class="col-lg form-group">
                    <label>
                        Date of Birth (day/month/year)
                <asp:RegularExpressionValidator ID="RegularExpressionValidator6" runat="server" ControlToValidate="BirthDayTextBox" CssClass="EroorSummer" ErrorMessage="Invalid" ValidationExpression="^(?:(?:31(\/|-|\.)(?:0?[13578]|1[02]))\1|(?:(?:29|30)(\/|-|\.)(?:0?[1,3-9]|1[0-2])\2))(?:(?:1[6-9]|[2-9]\d)?\d{2})$|^(?:29(\/|-|\.)0?2\3(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00))))$|^(?:0?[1-9]|1\d|2[0-8])(\/|-|\.)(?:(?:0?[1-9])|(?:1[0-2]))\4(?:(?:1[6-9]|[2-9]\d)?\d{4})$" ValidationGroup="1"></asp:RegularExpressionValidator></label>
                    <asp:TextBox ID="BirthDayTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                   <div class="col-lg form-group">
                    <label>
                   Legal Identity No. (NID:... Or B.R.N:...)
               </label>
                    <asp:TextBox ID="Legal_IdentityTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Blood Group</label>
                    <asp:DropDownList ID="BloodGroupDropDownList" runat="server" CssClass="form-control">
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
                <div class="col-lg form-group">
                    <label>Religion</label>
                    <asp:DropDownList ID="ReligionDropDownList" runat="server" CssClass="form-control">
                        <asp:ListItem>Islam</asp:ListItem>
                        <asp:ListItem>Hinduism</asp:ListItem>
                        <asp:ListItem>Buddhism</asp:ListItem>
                        <asp:ListItem>Christianity</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="form-row">
                <div class="col-md-6 form-group">
                    <label>Student&#39;s Permanent Address</label>
                    <asp:TextBox ID="StudentPermanentAddressTextBox" runat="server" TextMode="MultiLine" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-md-6 form-group">
                    <label>
                        Student&#39;s Local Address 
                        <small id="SameAddrs" style="cursor: pointer; color: #ff0000">
                            <i class="fa fa-hand-o-left" aria-hidden="true"></i>
                            Same As Permanent Address
                        </small>
                    </label>
                    <asp:TextBox ID="StudentLocalAddressTextBox" runat="server" TextMode="MultiLine" CssClass="form-control"></asp:TextBox>
                </div>
            </div>


            <div class="form-group">
                <label>Student&#39;s photo</label><br />
                <input name="Student_photo" type="file" accept=".png,.jpg" />
                <asp:HiddenField ID="Imge_HF" runat="server" />
            </div>


            <div class="alert alert-primary mt-5">Parents Information</div>
            <div class="form-row">
                <div class="col-lg form-group">
                    <label>Father's Name*<asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ControlToValidate="FatherNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="1"></asp:RequiredFieldValidator></label>
                    <asp:TextBox ID="FatherNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Father's Phone</label>
                    <asp:TextBox ID="FatherPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Father's Occupation</label>
                    <asp:TextBox ID="FatherOccupationTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
            </div>
            <div class="form-row">
                <div class="col-lg form-group">
                    <label>Mother's Name</label>
                    <asp:TextBox ID="MothersNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Mother's Phone</label>&nbsp;<asp:TextBox ID="MotherPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Mother's Occupation</label>
                    <asp:TextBox ID="MotherOccupationTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
            </div>
            

            <div class="alert alert-success mt-5">Previous Institution Information (If Any)</div>
            <div class="form-group">
                <label>Institution Name</label>
                <asp:TextBox ID="PreviousSchoolNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
            </div>

            <div class="form-row">
                <div class="col-lg form-group">
                    <label>Class</label>
                    <asp:TextBox ID="PreviousClassTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>

                <div class="col-lg form-group">
                    <label>Exam Year</label>
                    <asp:TextBox ID="PrevExamYearTextBox" onkeypress="return isNumberKey(event)" runat="server" CssClass="form-control"></asp:TextBox>
                </div>

                <div class="col-lg form-group">
                    <label>Grade</label>
                    <asp:TextBox ID="PrevGradeTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
            </div>

            <div class="form-group">
                <label>Guardian&#39;s photo</label><br />
                <input name="Guardian_photo" type="file" accept=".png,.jpg" />
                <asp:HiddenField ID="Guardian_Imge_HF" runat="server" />
            </div>

            <div class="alert alert-secondary mt-5">Second Guardian Information(Optional)</div>
            <div class="form-row">
                <div class="col-lg form-group">
                    <label>Guardian Name</label>
                    <asp:TextBox ID="SecondGuardianNameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Relationship</label>
                    <asp:TextBox ID="RelationshipwithStudentTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="col-lg form-group">
                    <label>Mobile No.</label>
                    <asp:TextBox ID="SecondGuardianPhoneTextBox" runat="server" onkeypress="return isNumberKey(event)" CssClass="form-control"></asp:TextBox>
                </div>
            </div>
            <div class="form-group">
                <label>Others Details(Optional)</label>
                <asp:TextBox ID="OthersDetailsTextBox" runat="server" TextMode="MultiLine" CssClass="form-control"></asp:TextBox>
            </div>


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
            <asp:SqlDataSource ID="StudentImageSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM [Student_Image] WHERE [StudentImageID] = @StudentImageID" InsertCommand="INSERT INTO Student_Image(Image, Guardian_Photo) VALUES (CAST(N'' AS xml).value('xs:base64Binary(sql:variable(&quot;@Image&quot;))', 'varbinary(max)'), CAST(N'' AS xml).value('xs:base64Binary(sql:variable(&quot;@Guardian_Photo&quot;))', 'varbinary(max)'))" SelectCommand="SELECT * FROM [Student_Image]" UpdateCommand="UPDATE [Student_Image] SET [Image] = @Image WHERE [StudentImageID] = @StudentImageID">
                <DeleteParameters>
                    <asp:Parameter Name="StudentImageID" Type="Int32" />
                </DeleteParameters>
                <InsertParameters>
                    <asp:ControlParameter ControlID="Imge_HF" Name="Image" PropertyName="Value" />
                    <asp:ControlParameter ControlID="Guardian_Imge_HF" Name="Guardian_Photo" PropertyName="Value" />
                </InsertParameters>
                <UpdateParameters>
                    <asp:Parameter Name="Image" Type="Object" />
                    <asp:Parameter Name="StudentImageID" Type="Int32" />
                </UpdateParameters>
            </asp:SqlDataSource>

            <asp:Button ID="StudentsInfoButton" runat="server" Text="Save &amp; Continue" OnClick="StudentsInfoButton_Click" UseSubmitBehavior="false" CssClass="btn btn-primary" ValidationGroup="1" />
        </div>
    </div>

    <script src="/JS/DateMask.js"></script>
    <script type="text/javascript">
        function DisableSubmit() { document.getElementById("<%=StudentsInfoButton.ClientID %>").disabled = true }
        window.onbeforeunload = DisableSubmit;

        function noBack() {
            window.history.forward();
        }
        noBack();
        window.onload = noBack;
        window.onpageshow = function (evt) {
            if (evt.persisted) noBack();
        }
        window.onunload = function () { void (0); }
        function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };


        $(function () {
            //Space not allow
            $('[id*=IDTextBox]').on("keypress keyup", function (e) {
                if (e.which === 32) {
                    return false;
                }
            });

            $('[id*=BirthDayTextBox]').mask("99/99/9999", { placeholder: 'dd/mm/yyyy' });

            $('[id*=IDTextBox]').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "Admission_New_Student.aspx/GetAllID",
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

            //Copy Text form another
            $("#SameAddrs").click(function () {
                $("[id*=StudentLocalAddressTextBox]").val($("[id*=StudentPermanentAddressTextBox]").val());
            });

            //Student Photo
            $('input[name=Student_photo]').change(function (e) {
                var file = e.target.files[0];
                canvasResize(file, {
                    width: 250,
                    height: 0,
                    crop: false,
                    quality: 50,
                    callback: function (data) {
                        data.split(",")[1]
                        $("[id*=Imge_HF]").val(data.split(",")[1]);
                    }
                });
            });

            //Guardian Photo
            $('input[name=Guardian_photo]').change(function (e) {
                var file = e.target.files[0];
                canvasResize(file, {
                    width: 250,
                    height: 0,
                    crop: false,
                    quality: 50,
                    callback: function (data) {
                        data.split(",")[1]
                        $("[id*=Guardian_Imge_HF]").val(data.split(",")[1]);
                    }
                });
            });
        });
    </script>
</asp:Content>
