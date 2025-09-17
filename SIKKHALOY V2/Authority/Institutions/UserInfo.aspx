<%@ Page Title="User Info" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="UserInfo.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.UserInfo" %>

<asp:Content ID="Content3" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid { text-align: left; }
        .Invaid_Ins td { color: #ff2b2b; }
        .Invaid_Ins td a { color: #ff2b2b; }
    </style>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">
    <h3>User Info</h3>
    <a href="User_Login_Info.aspx">User Login Info >></a>

    <div class="form-inline">
        <div class="md-form">
            <asp:TextBox ID="SearchTextBox" placeholder="Institution, Username" CssClass="form-control" runat="server"></asp:TextBox>
        </div>
        <div class="md-form">
            <asp:Button ID="FIndButton" runat="server" Text="Find" CssClass="btn btn-primary btn-sm" />
        </div>
    </div>



    <div class="table-responsive">
        <asp:GridView ID="SchoolGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="SchoolID" DataSourceID="InstitutionSQL" AllowSorting="True">
            <Columns>
                <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" />
                <asp:TemplateField HeaderText="Institution" SortExpression="SchoolName">
                    <ItemTemplate>
                        <asp:LinkButton OnCommand="Ins_LinkButton_Command" CommandArgument='<%#Eval("SchoolName") %>' CommandName='<%# Bind("SchoolID") %>' ID="Ins_LinkButton" runat="server"><%# Eval("SchoolName") %></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="UserName" HeaderText="User id" SortExpression="UserName" />
                <asp:BoundField DataField="Password" HeaderText="Password" SortExpression="Password" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Validation" HeaderText="Validation" SortExpression="Validation" />
                <asp:TemplateField HeaderText="Act. Session" SortExpression="EducationYear">
                    <ItemTemplate>
                        <asp:HiddenField ID="SchoolIDHF" runat="server" Value='<%#Eval("SchoolID") %>' />
                        <asp:Repeater ID="SessionRepeater" runat="server" DataSourceID="AcSessionSQL">
                            <HeaderTemplate>
                                <ul class="list-group">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <li class="list-group-item p-0 border-0">
                                    <i class="fa fa-check-square-o" aria-hidden="true"></i>
                                    <%#Eval("EducationYear") %></li>
                            </ItemTemplate>
                            <FooterTemplate>
                                </ul>
                            </FooterTemplate>
                        </asp:Repeater>
                        <asp:SqlDataSource ID="AcSessionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT EducationYear FROM Education_Year WHERE (SchoolID = @SchoolID) AND (IsActive = 1)">
                            <SelectParameters>
                                <asp:ControlParameter ControlID="SchoolIDHF" Name="SchoolID" PropertyName="Value" />
                            </SelectParameters>
                        </asp:SqlDataSource>
                    </ItemTemplate>
                </asp:TemplateField>

            </Columns>
            <EmptyDataTemplate>
                No Found !
            </EmptyDataTemplate>
        </asp:GridView>
        <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SchoolID, SchoolName, (SELECT UserName FROM AST WHERE (Category = N'admin') AND (SchoolID = Sch.SchoolID)) AS UserName, (SELECT Password FROM AST AS AST_1 WHERE (Category = N'admin') AND (SchoolID = Sch.SchoolID)) AS Password, Phone, Validation, Date FROM SchoolInfo AS Sch ORDER BY SchoolID"
            FilterExpression="SchoolName like '{0}%' OR UserName like '{0}%'">
            <FilterParameters>
                <asp:ControlParameter ControlID="SearchTextBox" Name="Find" PropertyName="Text" />
            </FilterParameters>
        </asp:SqlDataSource>
    </div>

    <div class="modal fade" id="myModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">
                        <asp:UpdatePanel ID="UpdatePanel6" runat="server">
                            <ContentTemplate>
                                <asp:Label ID="Institution_Label" runat="server"></asp:Label>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                        <ContentTemplate>
                            <div class="form-inline mb-3">
                                <div class="form-group">
                                    <asp:DropDownList ID="UserRoleDropDownList" runat="server" AutoPostBack="True" CssClass="form-control">
                                        <asp:ListItem Value="%">[ SELECT ROLE ]</asp:ListItem>
                                        <asp:ListItem>Admin</asp:ListItem>
                                        <asp:ListItem>Sub-Admin</asp:ListItem>
                                        <asp:ListItem>Teacher</asp:ListItem>
                                        <asp:ListItem>Student</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                            </div>
                            <div class="table-responsive">
                                <asp:GridView ID="UserGridView" runat="server" AutoGenerateColumns="False" DataKeyNames="RegistrationID,UserName" DataSourceID="UserSQL" AllowPaging="True" AllowSorting="True" CssClass="mGrid" PageSize="20">
                                    <Columns>
                                        <asp:BoundField DataField="UserName" HeaderText="Username" SortExpression="UserName" />
                                        <asp:BoundField DataField="Password" HeaderText="Password" SortExpression="Password" />
                                        <asp:TemplateField HeaderText="IsApproved" SortExpression="IsApproved">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="ISApprovedCheckBox" runat="server" Checked='<%# Bind("IsApproved") %>' Text=" " AutoPostBack="True" OnCheckedChanged="ISApprovedCheckBox_CheckedChanged" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="IsLockedOut" SortExpression="IsLockedOut">

                                            <ItemTemplate>
                                                <asp:CheckBox ID="IsLockedOutCheckBox" runat="server" Checked='<%# Bind("IsLockedOut") %>' Text=" " AutoPostBack="True" OnCheckedChanged="IsLockedOutCheckBox_CheckedChanged" />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                                    </Columns>
                                    <PagerStyle CssClass="pgr" />
                                </asp:GridView>
                                <asp:SqlDataSource ID="UserSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Registration.RegistrationID, Registration.SchoolID, Registration.UserName, Registration.Validation, Registration.CreateDate, aspnet_Membership.IsApproved, aspnet_Membership.IsLockedOut, aspnet_Membership.Email, AST.Password, AST.PasswordAnswer FROM aspnet_Users INNER JOIN aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId INNER JOIN Registration INNER JOIN AST ON Registration.RegistrationID = AST.RegistrationID ON aspnet_Users.UserName = Registration.UserName WHERE (Registration.SchoolID = @SchoolID) AND (Registration.Category = @Category)">
                                    <SelectParameters>
                                        <asp:Parameter Name="SchoolID" Type="Int32" />
                                        <asp:ControlParameter ControlID="UserRoleDropDownList" Name="Category" PropertyName="SelectedValue" Type="String" />
                                    </SelectParameters>
                                </asp:SqlDataSource>
                                <asp:SqlDataSource ID="UpdateRegSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT * FROM [Registration]" UpdateCommand="UPDATE SchoolInfo SET Validation = @Validation WHERE (UserName = @UserName)">
                                    <UpdateParameters>
                                        <asp:Parameter Name="Validation" />
                                        <asp:Parameter Name="UserName" Type="String" />
                                    </UpdateParameters>
                                </asp:SqlDataSource>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>


    <script type='text/javascript'>
        function openModal() {
            $('#myModal').modal('show');
        }

        $(function () {
            $('.mGrid tr').each(function () {
                if ($(this).find('td:nth-child(6)').text().trim() === "Invalid") {
                    $(this).addClass("Invaid_Ins");
                }
            });

            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });
    </script>
</asp:Content>

