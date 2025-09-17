<%@ Page Title="Update Info" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Update_Info.aspx.cs" Inherits="EDUCATION.COM.Authority.Basic.Update_Info" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Update Info</h3>

    <div class="row">
        <div class="col-sm-8">
            <div class="card">
                <div class="card-body">
                    <asp:FormView ID="AdminFormView" DefaultMode="Edit" runat="server" DataKeyNames="AuthorityID" DataSourceID="AdminInfoSQL" OnItemUpdated="AdminFormView_ItemUpdated" Width="100%">
                        <EditItemTemplate>
                            <div class="form-group">
                                <label>Name</label>
                                <asp:TextBox ID="NameTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Name") %>' />
                            </div>
                            <div class="form-group">
                                <label>Father&#39;s Name</label>
                                <asp:TextBox ID="FatherNameTextBox" runat="server" CssClass="form-control" Text='<%# Bind("FatherName") %>' />
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
                                <label>Mobile</label>
                                <asp:TextBox ID="PhoneTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Phone") %>' />
                            </div>
                            <div class="form-group">
                                <label>Email</label>
                                <asp:TextBox ID="EmailTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Email") %>' />
                            </div>
                            <div class="form-group">
                                <label>Address</label>
                                <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Address") %>' TextMode="MultiLine" />
                            </div>
                            <div class="form-group">
                                <label>Date of Birth</label>
                                <asp:TextBox ID="DateofBirthTextBox" runat="server" CssClass="form-control Datetime" Text='<%# Bind("DateofBirth","{0:d MMM yyyy}") %>' />
                            </div>
                            <div class="form-group">
                                <label>Image</label>
                                <asp:FileUpload ID="ImageFileUpload" runat="server" />
                            </div>

                            <asp:LinkButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" Text="Update" CssClass="btn btn-default" />
                        </EditItemTemplate>
                    </asp:FormView>
                </div>
            </div>
        </div>
    </div>
    <asp:SqlDataSource ID="AdminInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT * FROM Authority_Info WHERE (RegistrationID = @RegistrationID)"
        UpdateCommand="UPDATE Authority_Info SET Name = @Name, FatherName = @FatherName, Gender = @Gender, Designation = @Designation, City = @City, Phone = @Phone, Email = @Email, Address = @Address, DateofBirth = @DateofBirth WHERE (AuthorityID = @AuthorityID)">
        <SelectParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="Name" />
            <asp:Parameter Name="FatherName" Type="String" />
            <asp:Parameter Name="Gender" Type="String" />
            <asp:Parameter Name="Designation" Type="String" />
            <asp:Parameter Name="Address" Type="String" />
            <asp:Parameter Name="City" Type="String" />
            <asp:Parameter Name="Phone" Type="String" />
            <asp:Parameter Name="Email" Type="String" />
            <asp:Parameter Name="DateofBirth" />
            <asp:Parameter Name="Age" />
            <asp:Parameter Name="AuthorityID" />
        </UpdateParameters>
    </asp:SqlDataSource>

    <script>
        $(function () {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        })
    </script>
</asp:Content>
