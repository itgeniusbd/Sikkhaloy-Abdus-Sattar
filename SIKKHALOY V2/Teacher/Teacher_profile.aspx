<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Basic_Teacher.Master" AutoEventWireup="true" CodeBehind="Teacher_profile.aspx.cs" Inherits="EDUCATION.COM.Teacher.Teacher_profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .card-body ul { margin: 0; padding: 0; }
            .card-body ul li { list-style: none; border-bottom: 1px dotted #bcbcbc; padding: 0.5rem 0; margin: 0.3rem 0; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:FormView ID="TeacherFormView" runat="server" Width="100%" DataSourceID="TeacherSQL">
        <ItemTemplate>
            <div class="card">
                <div class="card-header">
                    <i class="fa fa-user-circle" aria-hidden="true"></i>
                    <%# Eval("FirstName") %> <%# Eval("LastName") %>
                </div>

                <div class="card-body">
                    <ul>
                        <li>
                            <b><i class="fa fa-user mr-1" aria-hidden="true"></i></b>
                            <%# Eval("Designation") %></li>
                        <li>
                            <b><i class="fa fa-phone-square mr-1" aria-hidden="true"></i></b>
                            <%# Eval("Phone") %></li>
                        <li>
                            <b><i class="fa fa-map-marker mr-1" aria-hidden="true"></i></b>
                            <%# Eval("Address") %></li>
                    </ul>
                </div>
            </div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="TeacherSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT [FirstName], [LastName], [Designation], [FatherName], [Address], [Phone] FROM [Teacher] WHERE ([TeacherRegistrationID] = @TeacherRegistrationID)">
        <SelectParameters>
            <asp:SessionParameter Name="TeacherRegistrationID" SessionField="RegistrationID" Type="Int32" />
        </SelectParameters>
    </asp:SqlDataSource>
</asp:Content>
