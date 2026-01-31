<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/Basic_Donor.Master" AutoEventWireup="true" CodeBehind="Change_Password.aspx.cs" Inherits="EDUCATION.COM.Committee.Change_Password" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="card p-4">
        <h3 class="mb-4 font-weight-bold ml-0">Change Password</h3>
        <asp:ChangePassword ID="ChangePassword" runat="server" ChangePasswordFailureText="Password incorrect or New Password invalid." OnChangedPassword="ChangePassword1_ChangedPassword" Width="100%">
            <ChangePasswordTemplate>
                <div class="row">
                    <div class="col-md-6 ml-0">
                        <div class="form-group">
                            <label>
                                Old Password
                                <asp:RequiredFieldValidator ID="CurrentPasswordRequired" runat="server" 
                                    ControlToValidate="CurrentPassword" 
                                    CssClass="text-danger" 
                                    ErrorMessage="Password is required." 
                                    ToolTip="Password is required." 
                                    ValidationGroup="ChangePassword1"
                                    Display="Dynamic">*</asp:RequiredFieldValidator>
                            </label>
                            <asp:TextBox ID="CurrentPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                        </div>

                        <div class="form-group">
                            <label>
                                New Password
                                <asp:RequiredFieldValidator ID="NewPasswordRequired" runat="server" 
                                    ControlToValidate="NewPassword" 
                                    CssClass="text-danger" 
                                    ErrorMessage="New Password is required." 
                                    ToolTip="New Password is required." 
                                    ValidationGroup="ChangePassword1"
                                    Display="Dynamic">*</asp:RequiredFieldValidator>
                            </label>
                            <asp:TextBox ID="NewPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator4" runat="server" 
                                ControlToValidate="NewPassword" 
                                CssClass="text-danger small d-block" 
                                Display="Dynamic" 
                                ErrorMessage="Minimum 8 and Maximum 30 characters required." 
                                ValidationExpression="^[\s\S]{8,30}$" 
                                ValidationGroup="ChangePassword1"></asp:RegularExpressionValidator>
                        </div>

                        <div class="form-group">
                            <label>
                                New Password Again
                                <asp:RequiredFieldValidator ID="ConfirmNewPasswordRequired" runat="server" 
                                    ControlToValidate="ConfirmNewPassword" 
                                    CssClass="text-danger" 
                                    ErrorMessage="Confirm New Password is required." 
                                    ToolTip="Confirm New Password is required." 
                                    ValidationGroup="ChangePassword1"
                                    Display="Dynamic">*</asp:RequiredFieldValidator>
                            </label>
                            <asp:TextBox ID="ConfirmNewPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                            <asp:CompareValidator ID="NewPasswordCompare" runat="server" 
                                ControlToCompare="NewPassword" 
                                ControlToValidate="ConfirmNewPassword" 
                                CssClass="text-danger small d-block" 
                                Display="Dynamic" 
                                ErrorMessage="New password does not match" 
                                ValidationGroup="ChangePassword1"></asp:CompareValidator>
                        </div>

                        <div class="form-group">
                            <asp:Button ID="ChangePasswordPushButton" runat="server" 
                                CommandName="ChangePassword" 
                                CssClass="btn btn-primary" 
                                Text="Change Password" 
                                ValidationGroup="ChangePassword1" />
                            <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                        </div>
                    </div>
                </div>
            </ChangePasswordTemplate>
            <SuccessTemplate>
                <div class="alert alert-success">Password Changed successfully!</div>
                <div class="form-group">
                    <asp:Button ID="ContinuePushButton" runat="server" 
                        CausesValidation="False" 
                        CommandName="Continue" 
                        CssClass="btn btn-primary" 
                        PostBackUrl="~/Committee/Donor_Dashboard.aspx" 
                        Text="Continue" />
                </div>
            </SuccessTemplate>
        </asp:ChangePassword>
    </div>
    
    <asp:SqlDataSource ID="LITQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
        SelectCommand="SELECT * FROM [AST]" UpdateCommand="UPDATE AST SET Password = @Password WHERE (RegistrationID = @RegistrationID)">
        <UpdateParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" Type="Int32" />
            <asp:Parameter Name="Password" Type="String" />
        </UpdateParameters>
    </asp:SqlDataSource>
</asp:Content>
