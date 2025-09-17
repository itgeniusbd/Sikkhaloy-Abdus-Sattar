<%@ Page Title="Login" Language="C#" MasterPageFile="~/Design.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="EDUCATION.COM.Login1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        header { display: none; }
        .Error { color: red; font-weight: bold; }
        #toTop { display:none !important}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container my-5">
        <div class="row">
            <div class="col-lg-6 offset-lg-3">
                <div class="modal-dialog cascading-modal">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h4 class="title"><i class="fa fa-lock"></i>
                                Login</h4>
                        </div>
                        <div class="modal-body">
                            <asp:Login ID="UserLogin2" runat="server" OnLoginError="UserLogin_LoginError" OnLoggedIn="UserLogin_LoggedIn" DestinationPageUrl="~/Profile_Redirect.aspx" Width="100%">
                                <LayoutTemplate>
                                    <div class="md-form">
                                        <i class="fa fa-envelope prefix grey-text"></i>
                                        <asp:TextBox ID="UserName" runat="server" class="form-control" placeholder="username"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName" ErrorMessage="User Name is required." ForeColor="#CC0000" ToolTip="User Name is required." ValidationGroup="Login2">*</asp:RequiredFieldValidator>
                                    </div>
                                    <div class="md-form">
                                        <i class="fa fa-lock prefix grey-text"></i>
                                        <asp:TextBox ID="Password" runat="server" class="form-control" TextMode="Password" placeholder="password"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password" ErrorMessage="Password is required." ForeColor="Red" ToolTip="Password is required." ValidationGroup="Login2">*</asp:RequiredFieldValidator>
                                    </div>
                                    <div class="md-form">
                                        <div class="text-center">
                                            <asp:Button ID="LoginButton" runat="server" CommandName="Login" class="btn btn-default waves-effect waves-light" Text="Log In" ValidationGroup="Login2" />
                                        </div>
                                    </div>
                                    <div class="alert-danger">
                                        <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                                    </div>
                                </LayoutTemplate>
                            </asp:Login>
                            <asp:Label ID="InvalidErrorLabel" runat="server" CssClass="Error"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
