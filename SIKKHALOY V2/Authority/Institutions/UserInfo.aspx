<%@ Page Title="User Info" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="UserInfo.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.UserInfo" %>

<asp:Content ID="Content3" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid { text-align: left; }
        .Invaid_Ins td { color: #ff2b2b; }
        .Invaid_Ins td a { color: #ff2b2b; }
        
        /* Enhanced Search Panel Styles */
        .search-filters {
            background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
            border: 1px solid #dee2e6;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 25px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }
        
        .search-filters h6 {
            color: #495057;
            font-weight: 600;
            margin-bottom: 20px;
            display: flex;
            align-items: center;
        }
        
        .search-filters h6 i {
            margin-right: 8px;
            color: #007bff;
        }

        /* Enhanced Form Controls */
        .search-row {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            align-items: end;
            margin-bottom: 15px;
        }
        
        .search-col {
            flex: 1;
            min-width: 180px;
        }
        
        .search-col-auto {
            flex: 0 0 auto;
            min-width: 200px;
        }

        /* Enhanced Input Styles */
        .form-control {
            border: 2px solid #e9ecef;
            border-radius: 8px;
            padding: 8px 15px;
            font-size: 14px;
            transition: all 0.3s ease;
            background-color: #fff;
        }
        
        .form-control:focus {
            border-color: #007bff;
            box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
            outline: none;
        }

        /* Label Styles */
        .form-label {
            display: block;
            margin-bottom: 5px;
            font-weight: 500;
            color: #495057;
            font-size: 13px;
        }

        /* Button Enhancements */
        .btn {
            border-radius: 8px;
            padding: 10px 20px;
            font-weight: 500;
            text-transform: uppercase;
            font-size: 12px;
            letter-spacing: 0.5px;
            transition: all 0.3s ease;
            border: none;
            cursor: pointer;
        }
        
        .btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
        }
        
        .btn-primary {
            background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
            color: white;
        }
        
        .btn-secondary {
            background: linear-gradient(135deg, #6c757d 0%, #495057 100%);
            color: white;
        }
        
        .btn-info {
            background: linear-gradient(135deg, #17a2b8 0%, #117a8b 100%);
            color: white;
        }

        /* Summary Styles */
        .search-summary {
            background: linear-gradient(135deg, #fff 0%, #f8f9fa 100%);
            border: 1px solid #dee2e6;
            border-radius: 12px;
            padding: 20px;
            margin-bottom: 25px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }
        
        .search-summary h6 {
            color: #495057;
            font-weight: 600;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
        }
        
        .search-summary h6 i {
            margin-right: 8px;
            color: #17a2b8;
        }

        .summary-row {
            display: flex;
            flex-wrap: wrap;
            gap: 15px;
            margin-bottom: 10px;
        }
        
        .summary-item {
            display: inline-flex;
            align-items: center;
            padding: 12px 18px;
            background-color: #fff;
            border-radius: 8px;
            border: 2px solid #dee2e6;
            font-weight: 500;
            min-width: 120px;
            justify-content: center;
            transition: all 0.3s ease;
        }
        
        .summary-item:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }
        
        .summary-item i {
            margin-right: 6px;
        }
        
        .summary-item.valid {
            border-color: #28a745;
            color: #28a745;
            background: linear-gradient(135deg, #fff 0%, #f8fff9 100%);
        }
        
        .summary-item.invalid {
            border-color: #dc3545;
            color: #dc3545;
            background: linear-gradient(135deg, #fff 0%, #fff8f8 100%);
        }
        
        .summary-item.total {
            border-color: #007bff;
            color: #007bff;
            font-weight: 600;
            background: linear-gradient(135deg, #fff 0%, #f8fbff 100%);
        }

        /* Page Header Styles */
        .page-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding-bottom: 15px;
            border-bottom: 2px solid #e9ecef;
        }
        
        .page-header h3 {
            margin: 0;
            color: #495057;
            font-weight: 600;
        }
        
        .page-header a {
            background: linear-gradient(135deg, #17a2b8 0%, #117a8b 100%);
            color: white;
            text-decoration: none;
            padding: 8px 16px;
            border-radius: 6px;
            font-weight: 500;
            transition: all 0.3s ease;
        }
        
        .page-header a:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);
            text-decoration: none;
            color: white;
        }

        /* Responsive Design */
        @media (max-width: 1200px) {
            .search-col {
                min-width: 150px;
            }
            
            .search-col-auto {
                min-width: 180px;
            }
        }
        
        @media (max-width: 992px) {
            .search-row {
                flex-direction: column;
            }
            
            .search-col, .search-col-auto {
                min-width: 100%;
            }
            
            .search-col-auto div {
                justify-content: center !important;
            }
            
            .summary-row {
                flex-direction: column;
            }
            
            .summary-item {
                min-width: 100%;
            }
            
            .page-header {
                flex-direction: column;
                gap: 15px;
                text-align: center;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="body" runat="server">
    <!-- Enhanced Page Header -->
    <div class="page-header">
        <h3><i class="fa fa-users" aria-hidden="true"></i> User Information Management</h3>
        <a href="User_Login_Info.aspx">
            <i class="fa fa-sign-in" aria-hidden="true"></i> User Login Info
        </a>
    </div>

    <!-- Enhanced Search Filters Panel -->
    <div class="search-filters">
        <h6><i class="fa fa-search" aria-hidden="true"></i> Search & Filter Options</h6>
        
        <!-- Single Row with All Controls -->
        <div class="search-row">
            <div class="search-col">
                <label class="form-label">Search Text</label>
                <asp:TextBox ID="SearchTextBox" placeholder="🔍 Institution, Username, Phone, School ID" CssClass="form-control" runat="server"></asp:TextBox>
            </div>
            <div class="search-col">
                <label class="form-label">Validation Status</label>
                <asp:DropDownList ID="ValidationFilter" runat="server" CssClass="form-control">
                    <asp:ListItem Value="" Text="📋 All Status"></asp:ListItem>
                    <asp:ListItem Value="Valid" Text="✅ Valid Only"></asp:ListItem>
                    <asp:ListItem Value="Invalid" Text="❌ Invalid Only"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-col">
                <label class="form-label">Has Password</label>
                <asp:DropDownList ID="PasswordFilter" runat="server" CssClass="form-control">
                    <asp:ListItem Value="" Text="🔑 All Users"></asp:ListItem>
                    <asp:ListItem Value="HasPassword" Text="🔒 Has Password"></asp:ListItem>
                    <asp:ListItem Value="NoPassword" Text="🔓 No Password"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-col-auto">
                <label class="form-label" style="visibility: hidden;">Actions</label>
                <div style="display: flex; gap: 10px;">
                    <asp:Button ID="FIndButton" runat="server" Text="🔍 Search" CssClass="btn btn-primary" OnClick="FIndButton_Click" />
                    <asp:Button ID="ClearButton" runat="server" Text="🗑️ Clear All" CssClass="btn btn-secondary" OnClick="ClearButton_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- Enhanced Search Results Summary -->
    <div class="search-summary" id="searchSummary" runat="server" visible="false">
        <h6><i class="fa fa-chart-bar" aria-hidden="true"></i> Search Results Summary</h6>
        <div class="summary-row">
            <div class="summary-item total">
                <i class="fa fa-database" aria-hidden="true"></i>
                <strong>Total Found: <asp:Label ID="TotalCountLabel" runat="server" Text="0"></asp:Label></strong>
            </div>
            <div class="summary-item valid">
                <i class="fa fa-check-circle" aria-hidden="true"></i>
                Valid: <asp:Label ID="ValidCountLabel" runat="server" Text="0"></asp:Label>
            </div>
            <div class="summary-item invalid">
                <i class="fa fa-times-circle" aria-hidden="true"></i>
                Invalid: <asp:Label ID="InvalidCountLabel" runat="server" Text="0"></asp:Label>
            </div>
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
                <div style="text-align: center; padding: 20px; color: #6c757d;">
                    <i class="fa fa-search fa-3x" aria-hidden="true"></i>
                    <h4>No Records Found!</h4>
                    <p>Try adjusting your search criteria.</p>
                </div>
            </EmptyDataTemplate>
        </asp:GridView>
        <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Sch.SchoolID, Sch.SchoolName, AdminUser.UserName, AdminUser.Password, Sch.Phone, Sch.Validation, Sch.Date FROM SchoolInfo AS Sch LEFT JOIN AST AS AdminUser ON AdminUser.SchoolID = Sch.SchoolID AND AdminUser.Category = N'admin' ORDER BY Sch.SchoolID">
        </asp:SqlDataSource>
    </div>

    <!-- Modal remains the same -->
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

