<%@ Page Title="Edit My Profile" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Student_Edit_Profile.aspx.cs" Inherits="EDUCATION.COM.Student.Student_Edit_Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .profile-card { background: #fff; border-radius: 15px; box-shadow: 0 6px 20px rgba(0,0,0,0.1); overflow: hidden; margin-bottom: 20px; }
        .profile-header { background: linear-gradient(135deg, #707070 0%, #044b08 100%); padding: 30px; text-align: center; color: white; }
        .profile-photo-container { position: relative; width: 150px; height: 150px; margin: 0 auto 20px; }
        .profile-photo { width: 150px; height: 150px; border-radius: 50%; border: 5px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.2); object-fit: cover; }
        .photo-upload-btn { position: absolute; bottom: 5px; right: 5px; background: #11998e; color: white; border: 3px solid white; border-radius: 50%; width: 45px; height: 45px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all 0.3s; box-shadow: 0 3px 10px rgba(0,0,0,0.3); }
        .photo-upload-btn:hover { background: #0e8478; transform: scale(1.1); }
        .photo-upload-btn i { font-size: 18px; }
        .profile-body { padding: 30px; }
        .form-section { background: #f8f9fa; padding: 20px; border-radius: 10px; margin-bottom: 20px; }
        .form-section h5 { color: #667eea; font-weight: 600; margin-bottom: 15px; padding-bottom: 10px; border-bottom: 2px solid #667eea; }
        .form-group label { font-weight: 600; color: #555; }
        .btn-update { background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%); border: none; padding: 12px 40px; color: white; font-weight: bold; border-radius: 25px; }
        .btn-update:hover { transform: translateY(-2px); color: white; }
        .alert-info { background-color: #e7f3ff; border-color: #b3d9ff; color: #004085; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container mt-4">
        <div class="row justify-content-center">
            <div class="col-lg-10">
                <asp:FormView ID="StudentInfoFV" runat="server" DataSourceID="StudentInfoSQL" Width="100%">
                    <ItemTemplate>
                        <div class="profile-card">
                            <div class="profile-header">
                                <div class="row">
                                    <div class="col-md-6 text-center">
                                        <h6 class="text-white mb-2">Student Photo</h6>
                                        <div class="profile-photo-container mx-auto">
                                            <asp:Image ID="StudentPhotoImg" runat="server" 
                                                CssClass="profile-photo"
                                                ImageUrl='<%# "~/Handeler/Student_Photo.ashx?SID=" + Eval("StudentImageID") %>' 
                                                AlternateText="Student Photo" />
                                            
                                            <label for="<%= PhotoUploadControl.ClientID %>" class="photo-upload-btn" title="Upload Student Photo">
                                                <i class="fa fa-camera"></i>
                                            </label>
                                        </div>
                                        <small class="text-white-50">Click camera to upload</small>
                                    </div>
                                    <div class="col-md-6 text-center">
                                        <h6 class="text-white mb-2">Guardian Photo</h6>
                                        <div class="profile-photo-container mx-auto">
                                            <asp:Image ID="GuardianPhotoImg" runat="server" 
                                                CssClass="profile-photo"
                                                ImageUrl='<%# "~/Handeler/Guardian_Photo.ashx?SID=" + Eval("StudentImageID") %>' 
                                                AlternateText="Guardian Photo" />
                                            
                                            <label for="<%= GuardianPhotoUploadControl.ClientID %>" class="photo-upload-btn" title="Upload Guardian Photo">
                                                <i class="fa fa-camera"></i>
                                            </label>
                                        </div>
                                        <small class="text-white-50">Click camera to upload</small>
                                    </div>
                                </div>
                                <div class="text-center mt-3">
                                    <h4 class="mb-2"><%# Eval("StudentsName") %></h4>
                                    <p class="mb-0">ID: <%# Eval("ID") %></p>
                                    <p class="mb-0">Class: <%# Eval("Class") %></p>
                                </div>
                            </div>

                            <div class="profile-body">
                                <asp:Label ID="MessageLabel" runat="server" CssClass="alert d-none" role="alert"></asp:Label>

                                <div class="alert alert-info mb-4">
                                    <i class="fa fa-info-circle mr-2"></i>
                                    <strong>Note:</strong> You can update your photo, contact details and address.
                                </div>

                                <div class="form-section">
                                    <h5><i class="fa fa-user mr-2"></i>Basic Information</h5>
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label>Full Name</label>
                                                <input type="text" class="form-control" value='<%# Eval("StudentsName") %>' readonly />
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label>Email Address</label>
                                                <asp:TextBox ID="StudentEmailAddressTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("StudentEmailAddress") %>' TextMode="Email" />
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label>Date of Birth</label>
                                                <asp:TextBox ID="DateofBirthTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("DateofBirth", "{0:yyyy-MM-dd}") %>' TextMode="Date" />
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label>Birth Certificate / NID</label>
                                                <asp:TextBox ID="Legal_IdentityTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("Legal_Identity") %>' />
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label>Blood Group</label>
                                                <asp:DropDownList ID="BloodGroupDDL" runat="server" CssClass="form-control">
                                                    <asp:ListItem Value="">Select Blood Group</asp:ListItem>
                                                    <asp:ListItem Value="A+">A+</asp:ListItem>
                                                    <asp:ListItem Value="A-">A-</asp:ListItem>
                                                    <asp:ListItem Value="B+">B+</asp:ListItem>
                                                    <asp:ListItem Value="B-">B-</asp:ListItem>
                                                    <asp:ListItem Value="O+">O+</asp:ListItem>
                                                    <asp:ListItem Value="O-">O-</asp:ListItem>
                                                    <asp:ListItem Value="AB+">AB+</asp:ListItem>
                                                    <asp:ListItem Value="AB-">AB-</asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label>Religion</label>
                                                <asp:DropDownList ID="ReligionDDL" runat="server" CssClass="form-control">
                                                    <asp:ListItem Value="">Select Religion</asp:ListItem>
                                                    <asp:ListItem Value="Islam">Islam</asp:ListItem>
                                                    <asp:ListItem Value="Hinduism">Hinduism</asp:ListItem>
                                                    <asp:ListItem Value="Buddhism">Buddhism</asp:ListItem>
                                                    <asp:ListItem Value="Christianity">Christianity</asp:ListItem>
                                                    <asp:ListItem Value="Other">Other</asp:ListItem>
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="form-section">
                                    <h5><i class="fa fa-map-marker mr-2"></i>Address Information</h5>
                                    <div class="row">
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label>Present Address</label>
                                                <asp:TextBox ID="StudentsLocalAddressTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("StudentsLocalAddress") %>' TextMode="MultiLine" Rows="3" />
                                            </div>
                                        </div>
                                        <div class="col-md-6">
                                            <div class="form-group">
                                                <label>Permanent Address</label>
                                                <asp:TextBox ID="StudentPermanentAddressTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("StudentPermanentAddress") %>' TextMode="MultiLine" Rows="3" />
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="form-section">
                                    <h5><i class="fa fa-users mr-2"></i>Parents Information</h5>
                                    <div class="row">
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Father's Name</label>
                                                <asp:TextBox ID="FathersNameTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("FathersName") %>' />
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Father's Occupation</label>
                                                <asp:TextBox ID="FatherOccupationTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("FatherOccupation") %>' />
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Father's Phone</label>
                                                <asp:TextBox ID="FatherPhoneNumberTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("FatherPhoneNumber") %>' MaxLength="11" />
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Mother's Name</label>
                                                <asp:TextBox ID="MothersNameTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("MothersName") %>' />
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Mother's Occupation</label>
                                                <asp:TextBox ID="MotherOccupationTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("MotherOccupation") %>' />
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Mother's Phone</label>
                                                <asp:TextBox ID="MotherPhoneNumberTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("MotherPhoneNumber") %>' MaxLength="11" />
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="form-section">
                                    <h5><i class="fa fa-user-shield mr-2"></i>Guardian Information</h5>
                                    <div class="row">
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Guardian's Name</label>
                                                <asp:TextBox ID="GuardianNameTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("GuardianName") %>' />
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Relationship</label>
                                                <asp:TextBox ID="GuardianRelationshipTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("GuardianRelationshipwithStudent") %>' />
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <label>Guardian's Phone</label>
                                                <asp:TextBox ID="GuardianPhoneNumberTB" runat="server" CssClass="form-control" 
                                                    Text='<%# Eval("GuardianPhoneNumber") %>' MaxLength="11" />
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div class="text-center mt-4">
                                    <asp:Button ID="UpdateButton" runat="server" 
                                        Text="Update Profile" 
                                        CssClass="btn btn-update btn-lg"
                                        OnClick="UpdateButton_Click"
                                        CausesValidation="false" />
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:FormView>
            </div>
        </div>
    </div>

    <asp:FileUpload ID="PhotoUploadControl" runat="server" Style="display:none;" accept="image/*" />
    <asp:FileUpload ID="GuardianPhotoUploadControl" runat="server" Style="display:none;" accept="image/*" />

    <asp:SqlDataSource ID="StudentInfoSQL" runat="server" 
        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT S.StudentID, ISNULL(S.StudentImageID, 0) AS StudentImageID, 
                       ISNULL(S.StudentsName, '') AS StudentsName, ISNULL(S.ID, '') AS ID, 
                       ISNULL(S.StudentEmailAddress, '') AS StudentEmailAddress,
                       S.DateofBirth, ISNULL(S.Legal_Identity, '') AS Legal_Identity,
                       ISNULL(S.BloodGroup, '') AS BloodGroup, ISNULL(S.Religion, '') AS Religion,
                       ISNULL(S.StudentPermanentAddress, '') AS StudentPermanentAddress,
                       ISNULL(S.StudentsLocalAddress, '') AS StudentsLocalAddress,
                       ISNULL(S.MothersName, '') AS MothersName, ISNULL(S.MotherOccupation, '') AS MotherOccupation,
                       ISNULL(S.MotherPhoneNumber, '') AS MotherPhoneNumber,
                       ISNULL(S.FathersName, '') AS FathersName, ISNULL(S.FatherOccupation, '') AS FatherOccupation,
                       ISNULL(S.FatherPhoneNumber, '') AS FatherPhoneNumber,
                       ISNULL(S.GuardianName, '') AS GuardianName, 
                       ISNULL(S.GuardianRelationshipwithStudent, '') AS GuardianRelationshipwithStudent,
                       ISNULL(S.GuardianPhoneNumber, '') AS GuardianPhoneNumber,
                       ISNULL(CC.Class, 'N/A') AS Class
                       FROM Student S
                       LEFT JOIN StudentsClass SC ON S.StudentID = SC.StudentID 
                       LEFT JOIN CreateClass CC ON SC.ClassID = CC.ClassID
                       WHERE S.StudentRegistrationID = @RegistrationID">
        <SelectParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script>
        $(document).ready(function() {
            var studentPhotoBtn = document.querySelector('label[for="<%= PhotoUploadControl.ClientID %>"]');
            var studentFileInput = document.getElementById('<%= PhotoUploadControl.ClientID %>');
            var studentPhotoImg = document.querySelector('#<%= StudentInfoFV.ClientID %> img[alt="Student Photo"]');

            if (studentPhotoBtn && studentFileInput) {
                studentPhotoBtn.addEventListener('click', function(e) {
                    e.preventDefault();
                    studentFileInput.click();
                });
            }

            if (studentFileInput && studentPhotoImg) {
                studentFileInput.addEventListener('change', function() {
                    if (this.files && this.files[0]) {
                        var reader = new FileReader();
                        reader.onload = function (e) {
                            studentPhotoImg.src = e.target.result;
                        };
                        reader.readAsDataURL(this.files[0]);
                    }
                });
            }

            var guardianPhotoBtn = document.querySelector('label[for="<%= GuardianPhotoUploadControl.ClientID %>"]');
            var guardianFileInput = document.getElementById('<%= GuardianPhotoUploadControl.ClientID %>');
            var guardianPhotoImg = document.querySelector('#<%= StudentInfoFV.ClientID %> img[alt="Guardian Photo"]');

            if (guardianPhotoBtn && guardianFileInput) {
                guardianPhotoBtn.addEventListener('click', function(e) {
                    e.preventDefault();
                    guardianFileInput.click();
                });
            }

            if (guardianFileInput && guardianPhotoImg) {
                guardianFileInput.addEventListener('change', function() {
                    if (this.files && this.files[0]) {
                        var reader = new FileReader();
                        reader.onload = function (e) {
                            guardianPhotoImg.src = e.target.result;
                        };
                        reader.readAsDataURL(this.files[0]);
                    }
                });
            }
        });
    </script>
</asp:Content>
