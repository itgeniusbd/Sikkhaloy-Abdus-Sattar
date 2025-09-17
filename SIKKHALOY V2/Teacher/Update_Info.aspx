<%@ Page Title="Update Info" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="Update_Info.aspx.cs" Inherits="EDUCATION.COM.Teacher.Update_Info" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Personal Info</h3>
    <asp:FormView ID="TeacherFormView" runat="server" DataKeyNames="TeacherID" DataSourceID="TeacherInfoSQL" DefaultMode="Edit" Width="100%">
        <EditItemTemplate>
            <div class="row">
                <div class="col-lg-8">
                    <div class="card">
                        <div class="card-body">
                            <div class="form-group">
                                <label>First Name</label>
                                <asp:TextBox ID="NameTextBox" runat="server" CssClass="form-control" Text='<%# Bind("FirstName") %>' />
                            </div>
                            <div class="form-group">
                                <label>Last Name</label>
                                <asp:TextBox ID="LastNameTextBox" runat="server" CssClass="form-control" Text='<%# Bind("LastName") %>' />
                            </div>
                            <div class="form-group">
                                <label>Father&#39;s Name</label>
                                <asp:TextBox ID="FatherNameTextBox" runat="server" CssClass="form-control" Text='<%# Bind("FatherName") %>' />
                            </div>
                             <div class="form-group">
                                <label>Mother&#39;s Name</label>
                                <asp:TextBox ID="MotherTextBox" runat="server" CssClass="form-control" Text='<%# Bind("MothersName") %>' />
                            </div>
                            <div class="form-group">
                                <label>Gender</label>
                                <asp:RadioButtonList ID="GenderRadioButtonList" runat="server" RepeatDirection="Horizontal" SelectedValue='<%#Bind("Gender") %>'>
                                    <asp:ListItem>Male</asp:ListItem>
                                    <asp:ListItem>Female</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>
                            <div class="form-group">
                                <label>Designation</label>
                                <asp:TextBox ID="DesignationTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Designation") %>' />
                            </div>
                            <div class="form-group">
                                <label>City</label>
                                <asp:TextBox ID="CityTextBox" runat="server" CssClass="form-control" Text='<%# Bind("City") %>' />
                            </div>
                            <div class="form-group">
                                <label>Postal Code</label>
                                <asp:TextBox ID="PostalCodeTextBox" runat="server" CssClass="form-control" Text='<%# Bind("PostalCode") %>' />
                            </div>
                            <div class="form-group">
                                <label>Mobile</label>
                                <asp:TextBox ID="PhoneTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Phone") %>' />
                            </div>
                            <div class="form-group">
                                <label>Email</label>
                                <asp:TextBox ID="EmailTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Email") %>' />
                            </div>
                            <div class="form-group">
                                <label>Present Address</label>
                                <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Address") %>' TextMode="MultiLine" />
                            </div>
                            <div class="form-group">
                                <label>Permanent Address</label>
                                <asp:TextBox ID="PermanentAddressTextBox" runat="server" CssClass="form-control" Text='<%# Bind("PermanentAddress") %>' TextMode="MultiLine" />
                            </div>
                            <img alt="No Image" src="/Handeler/Teachers.ashx?Img=<%#Eval("TeacherID") %>" class="rounded-circle img-thumbnail z-depth-1 Prev_img" style="width: 150px; height: 150px;" />
                            <div class="form-group">
                                <label>Upload</label><br />
                                <input name="Teacher_photo" type="file" accept=".png,.jpg" />
                            </div>

                            <asp:LinkButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" Text="Update" CssClass="btn btn-default" />
                        </div>
                    </div>
                </div>
            </div>
        </EditItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TeacherInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT * FROM Teacher WHERE (SchoolID = @SchoolID) AND (TeacherID = @TeacherID)"
        UpdateCommand="UPDATE Teacher SET FirstName = @FirstName, LastName = @LastName, FatherName = @FatherName, MothersName = @MothersName, Gender = @Gender, Designation = @Designation, Address = @Address, PermanentAddress = @PermanentAddress, City = @City, PostalCode = @PostalCode, State = @State, Phone = @Phone, Email = @Email, DateofBirth = @DateofBirth, Age = @Age WHERE (TeacherID = @TeacherID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="TeacherID" SessionField="TeacherID" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="FirstName" Type="String" />
            <asp:Parameter Name="LastName" Type="String" />
            <asp:Parameter Name="FatherName" Type="String" />
            <asp:Parameter Name="MothersName" />
            <asp:Parameter Name="Gender" Type="String" />
            <asp:Parameter Name="Designation" Type="String" />
            <asp:Parameter Name="Address" Type="String" />
            <asp:Parameter Name="PermanentAddress" />
            <asp:Parameter Name="City" Type="String" />
            <asp:Parameter Name="PostalCode" Type="String" />
            <asp:Parameter Name="State" Type="String" />
            <asp:Parameter Name="Phone" Type="String" />
            <asp:Parameter Name="Email" Type="String" />
            <asp:Parameter Name="DateofBirth" />
            <asp:Parameter Name="Age" />
            <asp:Parameter Name="TeacherID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>


    <script>
        //Student Photo
        $('input[name=Teacher_photo]').change(function (input) {
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
                        $('.Prev_img').attr('src', idata);

                        $.ajax({
                            url: "Update_Info.aspx/Update_Teacher_Image",
                            data: JSON.stringify({ 'Image': idata.split(",")[1] }),
                            dataType: "json",
                            type: "POST",
                            contentType: "application/json; charset=utf-8",
                            success: function (response) { },
                            error: function (xhr) {
                                var err = JSON.parse(xhr.responseText);
                                alert(err.message);
                            }
                        });
                    }
                });
            }
        });
    </script>
</asp:Content>
