<%@ Page Title="Authority Profile" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Auth_Profile.aspx.cs" Inherits="EDUCATION.COM.Authority.Auth_Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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

        /* Form Row Styles */
        .filter-row {
            margin-bottom: 15px;
        }
        
        .filter-row:last-child {
            margin-bottom: 0;
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
            min-width: 280px;
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

        /* Date Input Enhancement */
        .date-input-group {
            position: relative;
        }
        
        .date-input-group .form-control {
            padding-left: 40px;
            background-image: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="%23666" viewBox="0 0 16 16"><path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5 0zM1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4H1z"/></svg>');
            background-repeat: no-repeat;
            background-position: 12px center;
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
        
        .btn-cyan {
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

        .date-range-info {
            margin-top: 15px;
            padding: 10px 15px;
            background-color: #f8f9fa;
            border-radius: 6px;
            border-left: 4px solid #17a2b8;
        }
        
        .date-range-info small {
            color: #6c757d;
            font-weight: 500;
        }

        /* Compact Active Users Bar */
        .active-users-bar {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }

        .users-bar-content {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 15px 25px;
            gap: 30px;
        }

        .users-main-info {
            display: flex;
            align-items: center;
            gap: 15px;
        }

        .users-icon {
            font-size: 28px;
            color: white;
            animation: pulse 2s ease-in-out infinite;
        }

        @keyframes pulse {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.1); }
        }

        .users-text {
            color: white;
            display: flex;
            align-items: center;
            gap: 8px;
            font-size: 14px;
        }

        .users-text strong {
            font-weight: 600;
        }

        .users-count {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            background: rgba(255, 255, 255, 0.25);
            color: white;
            font-size: 24px;
            font-weight: 700;
            padding: 2px 16px;
            border-radius: 20px;
            min-width: 50px;
            backdrop-filter: blur(10px);
        }

        .users-label {
            color: rgba(255, 255, 255, 0.9);
            font-size: 13px;
        }

        .users-stats {
            display: flex;
            align-items: center;
            gap: 20px;
        }

        .stat-item {
            display: flex;
            align-items: center;
            gap: 8px;
            color: white;
            font-size: 13px;
            white-space: nowrap;
        }

        .stat-item i {
            font-size: 16px;
            opacity: 0.9;
        }

        .stat-item strong {
            font-size: 16px;
            font-weight: 700;
        }

        .stat-online strong {
            color: #4ade80;
            animation: numberPulse 2s ease-in-out infinite;
        }

        @keyframes numberPulse {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.08); }
        }

        .stat-divider {
            width: 1px;
            height: 30px;
            background: rgba(255, 255, 255, 0.3);
        }

        /* Responsive */
        @media (max-width: 1200px) {
            .users-bar-content {
                flex-direction: column;
                gap: 15px;
                padding: 20px;
            }

            .users-stats {
                width: 100%;
                justify-content: space-around;
                flex-wrap: wrap;
                gap: 15px;
            }

            .stat-divider {
                display: none;
            }
        }

        @media (max-width: 768px) {
            .users-text {
                flex-direction: column;
                align-items: flex-start;
                gap: 5px;
            }

            .stat-item {
                font-size: 12px;
            }

            .stat-item strong {
                font-size: 14px;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
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
                <label class="form-label">Registration Date From</label>
                <div class="date-input-group">
                    <asp:TextBox ID="StartDateTextBox" placeholder="📅 Start Date (e.g., 01 Jan 2025)" autocomplete="off" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                </div>
            </div>
            <div class="search-col">
                <label class="form-label">Registration Date To</label>
                <div class="date-input-group">
                    <asp:TextBox ID="EndDateTextBox" placeholder="📅 End Date (e.g., 31 Dec 2025)" autocomplete="off" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                </div>
            </div>
            <div class="search-col-auto">
                <label class="form-label" style="visibility: hidden;">Actions</label>
                <div style="display: flex; gap: 10px;">
                    <asp:Button ID="FIndButton" runat="server" Text="🔍 Search" CssClass="btn btn-primary" OnClick="FIndButton_Click" />
                    <asp:Button ID="ClearButton" runat="server" Text="🗑️ Clear All" CssClass="btn btn-secondary" OnClick="ClearButton_Click" />
                    <button type="button" class="btn btn-cyan" data-toggle="modal" data-target="#exampleModal">
                        <i class="fa fa-bullhorn mr-1" aria-hidden="true"></i> Add Notice
                    </button>
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
                <strong>Total Institution Found: <asp:Label ID="TotalCountLabel" runat="server" Text="0"></asp:Label></strong>
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
        <div class="date-range-info">
            <small>
                <i class="fa fa-calendar" aria-hidden="true"></i>
                <strong>Date Range:</strong> <asp:Label ID="DateRangeLabel" runat="server" Text="All Time"></asp:Label>
            </small>
        </div>
    </div>

    <!-- Compact Active Users Bar -->
    <div class="active-users-bar mb-3">
        <div class="users-bar-content">
            <div class="users-main-info">
                <i class="fa fa-users users-icon"></i>
                <div class="users-text">
                    <strong>Currently Active:</strong>
                    <span class="users-count">
                        <asp:Label ID="LoggedInUsersCountLabel" runat="server" Text="0"></asp:Label>
                    </span>
                    <span class="users-label">users online (last 15 min)</span>
                </div>
            </div>
            <div class="users-stats">
                <div class="stat-item">
                    <i class="fa fa-calendar"></i>
                    <span>Today: <strong><asp:Label ID="TodayLoginsLabel" runat="server" Text="0"></asp:Label></strong></span>
                </div>
                <div class="stat-divider"></div>
                <div class="stat-item">
                    <i class="fa fa-clock-o"></i>
                    <span>Last Hour: <strong><asp:Label ID="LastHourLoginsLabel" runat="server" Text="0"></asp:Label></strong></span>
                </div>
                <div class="stat-divider"></div>
                <div class="stat-item stat-online">
                    <i class="fa fa-bolt"></i>
                    <span>Online Now: <strong><asp:Label ID="OnlineNowLabel" runat="server" Text="0"></asp:Label></strong> (5 min)</span>
                </div>
            </div>
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="SchoolGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="SchoolID" DataSourceID="InstitutionSQL" AllowSorting="True">
            <Columns>
                <asp:BoundField DataField="SchoolID" HeaderText="School ID" SortExpression="SchoolID" />
                <asp:HyperLinkField SortExpression="SchoolName" DataNavigateUrlFields="SchoolID" DataNavigateUrlFormatString="Institutions/Institution_Details.aspx?SchoolID={0}" DataTextField="SchoolName" HeaderText="Select" />
                <asp:BoundField DataField="UserName" HeaderText="User id" SortExpression="UserName" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" />
                <asp:BoundField DataField="Validation" HeaderText="Validation" SortExpression="Validation" />
                <asp:BoundField DataField="Date" HeaderText="Registration Date" SortExpression="Date" DataFormatString="{0:dd MMM yyyy}" />
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
        <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SchoolID, SchoolName, Phone, Validation, Date, UserName FROM SchoolInfo AS Sch ORDER BY SchoolID">
        </asp:SqlDataSource>
    </div>

    <!-- Modal -->
    <div class="modal fade" id="exampleModal" tabindex="-1" role="dialog" aria-labelledby="exampleModalLabel" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">Add Notice For All Isntitution</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                        <ContentTemplate>
                            <div class="form-group">
                                <label>Notice Title<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="Notice_TitleTextBox" CssClass="EroorStar" ErrorMessage="Required" ValidationGroup="N"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="Notice_TitleTextBox" placeholder="Notice Title" runat="server" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Show From Date<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ShowFromDateTextBox" CssClass="EroorStar" ErrorMessage="Required" ValidationGroup="N"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="ShowFromDateTextBox" placeholder="From Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Show To Date<asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="ShowToDateTextBox" CssClass="EroorStar" ErrorMessage="Required" ValidationGroup="N"></asp:RequiredFieldValidator></label>
                                <asp:TextBox ID="ShowToDateTextBox" placeholder="To Date" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" runat="server" CssClass="form-control datepicker"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <label>Notice (Text)</label>
                                <asp:TextBox ID="NoticeTextBox" placeholder="Notice Text" runat="server" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                            </div>

                            <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Submit" OnClick="SubmitButton_Click" ValidationGroup="N" />
                            <asp:SqlDataSource ID="NoticeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" DeleteCommand="DELETE FROM Notice_Admin WHERE [AdminNoticeID] = @AdminNoticeID" InsertCommand="INSERT INTO Notice_Admin(Notice_Title, Notice, Show_Date, End_Date, RegistrationID) VALUES (@Notice_Title, @Notice, @Show_Date, @End_Date, @RegistrationID)" SelectCommand="SELECT * FROM Notice_Admin" UpdateCommand="UPDATE Notice_Admin SET Notice_Title = @Notice_Title, Notice = @Notice, Show_Date = @Show_Date, End_Date = @End_Date WHERE (AdminNoticeID = @AdminNoticeID)">
                                <DeleteParameters>
                                    <asp:Parameter Name="AdminNoticeID" Type="Int32" />
                                </DeleteParameters>
                                <InsertParameters>
                                    <asp:ControlParameter ControlID="Notice_TitleTextBox" Name="Notice_Title" PropertyName="Text" Type="String" />
                                    <asp:ControlParameter ControlID="NoticeTextBox" Name="Notice" PropertyName="Text" Type="String" />
                                    <asp:ControlParameter ControlID="ShowFromDateTextBox" DbType="Date" Name="Show_Date" PropertyName="Text" />
                                    <asp:ControlParameter ControlID="ShowToDateTextBox" DbType="Date" Name="End_Date" PropertyName="Text" />
                                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                                </InsertParameters>
                                <UpdateParameters>
                                    <asp:Parameter Name="Notice_Title" Type="String" />
                                    <asp:Parameter Name="Notice" Type="String" />
                                    <asp:Parameter DbType="Date" Name="Show_Date" />
                                    <asp:Parameter DbType="Date" Name="End_Date" />
                                    <asp:Parameter Name="AdminNoticeID" Type="Int32" />
                                </UpdateParameters>
                            </asp:SqlDataSource>

                            <div class="table-responsive">
                                <asp:GridView ID="Notice_GridView" runat="server" CssClass="mGrid" AutoGenerateColumns="False" DataKeyNames="AdminNoticeID" DataSourceID="NoticeSQL">
                                    <Columns>
                                        <asp:TemplateField HeaderText="Notice">
                                            <ItemTemplate>
                                                <div>
                                                    <h4>
                                                        <asp:Label ID="Label3" runat="server" Text='<%# Bind("Notice_Title") %>'></asp:Label></h4>
                                                </div>

                                                <asp:Label ID="Label4" runat="server" Text='<%# Bind("Notice") %>'></asp:Label>

                                                <div>
                                                    <div><strong>Display Date</strong></div>
                                                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("Show_Date", "{0:d MMM yyyy}") %>'></asp:Label>
                                                    TO
                            <asp:Label ID="Label2" runat="server" Text='<%# Bind("End_Date", "{0:d MMM yyyy}") %>'></asp:Label>
                                                </div>

                                                Add Date:
                            <asp:Label ID="Label5" runat="server" Text='<%# Bind("Insert_Date", "{0:d MMM yyyy}") %>'></asp:Label>

                                                <div>
                                                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="Edit" Text="Edit Notice"></asp:LinkButton>
                                                    |
                            <asp:LinkButton ID="LinkButton4" runat="server" CausesValidation="False" CommandName="Delete" Text="Delete" OnClientClick="return confirm('are you sure want to delete?')"></asp:LinkButton>
                                                </div>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <div class="form-group">
                                                    <label>Notice Title</label>
                                                    <asp:TextBox ID="TextBox3" CssClass="form-control" runat="server" Text='<%# Bind("Notice_Title") %>'></asp:TextBox>
                                                </div>
                                                <div class="form-group">
                                                    <label>Notice</label>
                                                    <asp:TextBox ID="TextBox4" CssClass="form-control" runat="server" TextMode="MultiLine" Text='<%# Bind("Notice") %>'></asp:TextBox>
                                                </div>
                                                <div class="form-group">
                                                    <label>Display From Date</label>
                                                    <asp:TextBox ID="TextBox1" CssClass="form-control datepicker" runat="server" Text='<%# Bind("Show_Date", "{0:d MMM yyyy}") %>'></asp:TextBox>
                                                </div>
                                                <div class="form-group">
                                                    <label>Display To Date</label>
                                                    <asp:TextBox ID="TextBox2" CssClass="form-control datepicker" runat="server" Text='<%# Bind("End_Date", "{0:d MMM yyyy}") %>'></asp:TextBox>
                                                </div>

                                                <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="True" CommandName="Update" Text="Update"></asp:LinkButton>
                                                <asp:LinkButton ID="LinkButton3" runat="server" CausesValidation="True" CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                                            </EditItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>


    <script type='text/javascript'>
        $(function () {
            $('.mGrid tr').each(function () {
                if ($(this).find('td:nth-child(5)').text().trim() === "Invalid") {
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

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            $('.datepicker').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });
    </script>
</asp:Content>
