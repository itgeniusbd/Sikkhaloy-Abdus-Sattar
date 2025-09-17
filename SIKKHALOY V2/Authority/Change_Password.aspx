<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Change_Password.aspx.cs" Inherits="EDUCATION.COM.Authority.Change_Password" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server">
    <h3>Change Password</h3>
    <div class="row">
        <div class="col-lg-8">
            <div class="card">
                <div class="card-body">

                    <asp:ChangePassword Width="100%" ID="ChangePassword" runat="server" ChangePasswordFailureText="Password incorrect or New Password invalid.">
                        <ChangePasswordTemplate>
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label>Old Password</label>
                                    <asp:RequiredFieldValidator ID="CurrentPasswordRequired" runat="server" ControlToValidate="CurrentPassword" CssClass="EroorStar" ErrorMessage="Password is required." ToolTip="Password is required." ValidationGroup="ChangePassword1">*</asp:RequiredFieldValidator>
                                    <asp:TextBox ID="CurrentPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>New Password</label>
                                    <asp:RequiredFieldValidator ID="NewPasswordRequired" runat="server" ControlToValidate="NewPassword" CssClass="EroorStar" ErrorMessage="New Password is required." ToolTip="New Password is required." ValidationGroup="ChangePassword1">*</asp:RequiredFieldValidator>
                                    <asp:RegularExpressionValidator ID="RegularExpressionValidator4" runat="server" ControlToValidate="NewPassword" CssClass="EroorStar" Display="Dynamic" ErrorMessage="Minimum 6 and Maximum 30 characters required." ValidationExpression="^[\s\S]{6,30}$" ValidationGroup="ChangePassword1"></asp:RegularExpressionValidator>
                                    <asp:TextBox ID="NewPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                </div>

                                <div class="form-group">
                                    <label>New Password Again</label>
                                    <asp:CompareValidator ID="NewPasswordCompare" runat="server" ControlToCompare="NewPassword" ControlToValidate="ConfirmNewPassword" CssClass="EroorStar" Display="Dynamic" ErrorMessage="New password does not match" ValidationGroup="ChangePassword1"></asp:CompareValidator>
                                    <asp:RequiredFieldValidator ID="ConfirmNewPasswordRequired" runat="server" ControlToValidate="ConfirmNewPassword" CssClass="EroorStar" ErrorMessage="Confirm New Password is required." ToolTip="Confirm New Password is required." ValidationGroup="ChangePassword1">*</asp:RequiredFieldValidator>
                                    <asp:TextBox ID="ConfirmNewPassword" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                </div>

                                <div class="form-group">


                                    <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>

                                    <asp:Button ID="ChangePasswordPushButton" runat="server" CommandName="ChangePassword" CssClass="btn btn-primary" Text="Change" ValidationGroup="ChangePassword1" />
                                    <asp:Button ID="CancelPushButton" runat="server" CausesValidation="False" CommandName="Cancel" CssClass="btn btn-primary" Text="Cancel" />
                                </div>
                            </div>
                        </ChangePasswordTemplate>
                        <SuccessTemplate>
                            <div class="alert alert-success">
                                <label>Congratulation!!</label>
                                <label>You have Successfully Changed Your Password!</label>
                                <asp:Button ID="ContinuePushButton" runat="server" CausesValidation="False" CommandName="Continue" CssClass="btn btn-primary" PostBackUrl="~/Profile_Redirect.aspx" Text="Continue" />
                            </div>
                        </SuccessTemplate>
                    </asp:ChangePassword>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

