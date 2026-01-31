<%@ Page Title="My Details" Language="C#" MasterPageFile="~/Basic_Donor.Master" AutoEventWireup="true" CodeBehind="Donor_Details.aspx.cs" Inherits="EDUCATION.COM.Committee.Donor_Details" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <style>
        .profile-card { background: #fff; border-radius: 15px; box-shadow: 0 6px 20px rgba(0,0,0,0.1); overflow: hidden; margin-bottom: 20px; }
        .profile-header { background: linear-gradient(135deg, #00c853 0%, #00897b 100%); padding: 30px; text-align: center; color: white; }
        .profile-photo-container { position: relative; width: 150px; height: 150px; margin: 0 auto 20px; }
        .profile-photo { width: 150px; height: 150px; border-radius: 50%; border: 5px solid white; box-shadow: 0 4px 10px rgba(0,0,0,0.2); object-fit: cover; }
        .photo-upload-btn { position: absolute; bottom: 5px; right: 5px; background: #00c853; color: white; border: 3px solid white; border-radius: 50%; width: 45px; height: 45px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all 0.3s; }
        .photo-upload-btn:hover { background: #00897b; transform: scale(1.1); }
        .photo-upload-btn i { font-size: 18px; }
        .profile-body { padding: 30px; }
        .form-section { background: #f8f9fa; padding: 20px; border-radius: 10px; margin-bottom: 20px; }
        .form-section h5 { color: #00c853; font-weight: 600; margin-bottom: 15px; padding-bottom: 10px; border-bottom: 2px solid #00c853; }
        .form-group label { font-weight: 600; color: #555; }
        .btn-update { background: linear-gradient(135deg, #00c853 0%, #00897b 100%); border: none; padding: 12px 40px; color: white; font-weight: bold; border-radius: 25px; }
        .btn-update:hover { transform: translateY(-2px); color: white; }
        .alert-info { background-color: #e7f3ff; border-color: #b3d9ff; color: #004085; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="row justify-content-center">
        <div class="col-lg-10">
            <asp:FormView ID="DonorInfoFV" runat="server" DataSourceID="DonorDetailsSQL" Width="100%">
                <ItemTemplate>
                    <div class="profile-card">
                        <div class="profile-header">
                            <div class="profile-photo-container mx-auto">
                                <asp:Image ID="DonorPhotoImg" runat="server" 
                                    CssClass="profile-photo"
                                    ImageUrl='<%# "~/Handeler/Commeti_Photo.ashx?CID=" + Eval("CommitteeMemberId") %>' 
                                    AlternateText="Donor Photo" />
                                
                                <label for="<%= PhotoUploadControl.ClientID %>" class="photo-upload-btn" title="Upload Photo">
                                    <i class="fa fa-camera"></i>
                                </label>
                            </div>
                            <div class="text-center">
                                <h4 class="mb-2"><%# Eval("MemberName") %></h4>
                                <p class="mb-0"><%# Eval("CommitteeMemberType") %></p>
                                <p class="mb-0">Member Since: <%# Eval("InsertDate", "{0:d MMM yyyy}") %></p>
                            </div>
                        </div>

                        <div class="profile-body">
                            <asp:Label ID="MessageLabel" runat="server" CssClass="alert d-none" role="alert"></asp:Label>

                            <div class="alert alert-info mb-4">
                                <i class="fa fa-info-circle mr-2"></i>
                                <strong>Note:</strong> You can update your photo, contact details and address.
                            </div>

                            <div class="form-section">
                                <h5><i class="fa fa-user mr-2"></i>Personal Information</h5>
                                <div class="row">
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Full Name</label>
                                            <asp:TextBox ID="MemberNameTB" runat="server" CssClass="form-control" 
                                                Text='<%# Eval("MemberName") %>' />
                                        </div>
                                    </div>
                                    <div class="col-md-6">
                                        <div class="form-group">
                                            <label>Mobile Number</label>
                                            <asp:TextBox ID="SmsNumberTB" runat="server" CssClass="form-control" 
                                                Text='<%# Eval("SmsNumber") %>' MaxLength="11" />
                                        </div>
                                    </div>
                                    <div class="col-md-12">
                                        <div class="form-group">
                                            <label>Email Address</label>
                                            <asp:TextBox ID="EmailTB" runat="server" CssClass="form-control" 
                                                Text='<%# Eval("Email") %>' TextMode="Email" placeholder="example@email.com" />
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div class="form-section">
                                <h5><i class="fa fa-map-marker mr-2"></i>Address Information</h5>
                                <div class="row">
                                    <div class="col-md-12">
                                        <div class="form-group">
                                            <label>Address</label>
                                            <asp:TextBox ID="AddressTB" runat="server" CssClass="form-control" 
                                                Text='<%# Eval("Address") %>' TextMode="MultiLine" Rows="3" />
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

    <asp:FileUpload ID="PhotoUploadControl" runat="server" Style="display:none;" accept="image/*" />

    <asp:SqlDataSource ID="DonorDetailsSQL" runat="server" 
        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CM.CommitteeMemberId, CM.MemberName, CM.Address, CM.SmsNumber, ISNULL(CM.Email, '') AS Email, CM.InsertDate, CMT.CommitteeMemberType 
                       FROM CommitteeMember CM 
                       INNER JOIN CommitteeMemberType CMT ON CM.CommitteeMemberTypeId = CMT.CommitteeMemberTypeId
                       INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.CommitteeMemberId = CM.CommitteeMemberId
                       WHERE R.RegistrationID = @RegistrationID AND R.SchoolID = @SchoolID">
        <SelectParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <script type="text/javascript">
        function setupPhotoUpload() {
            console.log('Setting up photo upload...');
            
            var photoFileInput = document.getElementById('<%= PhotoUploadControl.ClientID %>');
            var photoImg = document.querySelector('img.profile-photo[alt="Donor Photo"]');
            var cameraBtn = document.querySelector('.photo-upload-btn');

            console.log('File input:', photoFileInput);
            console.log('Photo img:', photoImg);
            console.log('Camera btn:', cameraBtn);

            if (!photoFileInput || !photoImg) {
                console.error('Required elements not found!');
                return;
            }

            // Camera button click handler
            if (cameraBtn) {
                cameraBtn.onclick = function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    console.log('Camera button clicked');
                    photoFileInput.click();
                    return false;
                };
            }

            // File input change handler
            photoFileInput.onchange = function() {
                console.log('File selected');
                
                if (this.files && this.files[0]) {
                    var file = this.files[0];
                    console.log('File:', file.name, file.type, file.size);
                    
                    // Validate file type
                    var validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
                    if (validTypes.indexOf(file.type) === -1) {
                        alert('Please select a valid image file (JPG, PNG, or GIF)');
                        this.value = '';
                        return;
                    }
                    
                    // Validate file size (max 2MB)
                    if (file.size > 2097152) {
                        alert('File size too large. Maximum allowed size is 2MB.');
                        this.value = '';
                        return;
                    }
                    
                    // Show preview
                    var reader = new FileReader();
                    reader.onload = function (e) {
                        console.log('FileReader loaded, updating image...');
                        photoImg.src = e.target.result;
                    };
                    reader.readAsDataURL(file);
                }
            };
        }

        // Try multiple methods to ensure script runs after page load
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', setupPhotoUpload);
        } else {
            setupPhotoUpload();
        }

        // Also try with jQuery if available
        if (typeof jQuery !== 'undefined') {
            jQuery(document).ready(function() {
                setTimeout(setupPhotoUpload, 500);
            });
        }

        // Also try with window.onload as fallback
        window.addEventListener('load', function() {
            setTimeout(setupPhotoUpload, 1000);
        });
    </script>
</asp:Content>
