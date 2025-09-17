<%@ Page Title="Update Info" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Update_Info.aspx.cs" Inherits="EDUCATION.COM.Profile.Update_Info" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Update Info</h3>

    <div class="row">
        <div class="col-sm-8">
            <div class="card">
                <div class="card-body">
                    <asp:FormView ID="AdminFormView" DefaultMode="Edit" runat="server" DataKeyNames="AdminID" DataSourceID="AdminInfoSQL" OnItemUpdated="AdminFormView_ItemUpdated" Width="100%">
                        <EditItemTemplate>
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
                                <label>Gender</label>
                                <asp:TextBox ID="GenderTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Gender") %>' />
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
                                <label>Address</label>
                                <asp:TextBox ID="AddressTextBox" runat="server" CssClass="form-control" Text='<%# Bind("Address") %>' TextMode="MultiLine" />
                            </div>
                            <div class="form-group">
                                <label>Image</label>
                                <asp:FileUpload ID="ImageFileUpload" runat="server" />
                            </div>

                            <asp:LinkButton ID="UpdateButton" runat="server" CausesValidation="True" CommandName="Update" Text="Update" CssClass="btn btn-default" />
                            &nbsp;<asp:LinkButton ID="UpdateCancelButton" runat="server" CausesValidation="False" CommandName="Cancel" Text="Cancel" CssClass="btn btn-default" />
                        </EditItemTemplate>
                    </asp:FormView>
                </div>
            </div>
        </div>
    </div>
    <asp:SqlDataSource ID="AdminInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT FirstName + ' ' + LastName AS Name, Admin.* FROM Admin WHERE (SchoolID = @SchoolID) AND (RegistrationID = @RegistrationID)"
        UpdateCommand="UPDATE Admin SET FirstName = @FirstName, LastName = @LastName, FatherName = @FatherName, Gender = @Gender, Designation = @Designation, Address = @Address, City = @City, PostalCode = @PostalCode, State = @State, Phone = @Phone, Email = @Email, DateofBirth = @DateofBirth, Age = @Age WHERE (AdminID = @AdminID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
        </SelectParameters>
        <UpdateParameters>
            <asp:Parameter Name="FirstName" Type="String" />
            <asp:Parameter Name="LastName" Type="String" />
            <asp:Parameter Name="FatherName" Type="String" />
            <asp:Parameter Name="Gender" Type="String" />
            <asp:Parameter Name="Designation" Type="String" />
            <asp:Parameter Name="Address" Type="String" />
            <asp:Parameter Name="City" Type="String" />
            <asp:Parameter Name="PostalCode" Type="String" />
            <asp:Parameter Name="State" Type="String" />
            <asp:Parameter Name="Phone" Type="String" />
            <asp:Parameter Name="Email" Type="String" />
            <asp:Parameter Name="DateofBirth" />
            <asp:Parameter Name="Age" />
            <asp:Parameter Name="AdminID" Type="Int32" />
        </UpdateParameters>
    </asp:SqlDataSource>
</asp:Content>
