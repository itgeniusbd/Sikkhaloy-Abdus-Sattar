<%@ Page Title="Authority Profile" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Auth_Profile.aspx.cs" Inherits="EDUCATION.COM.Authority.Auth_Profile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .mGrid { text-align: left; }
        .Invaid_Ins td { color: #ff2b2b; }
        .Invaid_Ins td a { color: #ff2b2b; }

        /* Online institution row highlight */
        .online-now-row {
            background: linear-gradient(90deg, #dcfce7 0%, #f0fdf4 100%) !important;
            border-left: 4px solid #22c55e;
        }

        .online-now-row td {
            background: transparent !important;
        }

        .online-active-row {
            background: linear-gradient(90deg, #fef9c3 0%, #fefce8 100%) !important;
            border-left: 4px solid #eab308;
        }

        .online-active-row td {
            background: transparent !important;
        }

        .online-badge {
            display: inline-flex;
            align-items: center;
            gap: 4px;
            padding: 2px 7px;
            border-radius: 20px;
            font-size: 10px;
            font-weight: 600;
            white-space: nowrap;
        }

        .online-badge.online-now {
            background: #dcfce7;
            color: #15803d;
            border: 1px solid #86efac;
        }

        .online-badge.online-active {
            background: #fef9c3;
            color: #a16207;
            border: 1px solid #fde047;
        }

        .online-badge .fa-circle {
            font-size: 8px;
        }

        .online-badge.online-now .fa-circle {
            color: #22c55e;
            animation: onlinePulse 1.5s ease-in-out infinite;
        }

        @keyframes onlinePulse {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.35; }
        }

        .session-live-row {
            background: #f0fdf4 !important;
        }

        .session-active-row {
            background: #fefce8 !important;
        }
        
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

        /* Live Login Monitor - compact toolbar */
        .live-stats-panel {
            background: #fff;
            border: 1px solid #e2e8f0;
            border-radius: 10px;
            padding: 10px 14px;
            margin-bottom: 16px;
            box-shadow: 0 2px 8px rgba(15, 23, 42, 0.05);
        }

        .live-stats-toolbar {
            display: flex;
            align-items: center;
            gap: 10px;
            flex-wrap: wrap;
        }

        .live-stats-title {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            font-size: 13px;
            font-weight: 700;
            color: #1e293b;
            white-space: nowrap;
            margin: 0;
        }

        .live-stats-title i {
            color: #22c55e;
            font-size: 14px;
        }

        .live-filter-group {
            display: flex;
            align-items: center;
            gap: 6px;
            flex: 1;
            flex-wrap: wrap;
        }

        .live-filter-item {
            position: relative;
            display: inline-flex;
            align-items: center;
            gap: 8px;
            padding: 5px 10px 5px 5px;
            border: 1px solid #dbe3ef;
            border-radius: 8px;
            background: #f8fafc;
            white-space: nowrap;
            line-height: 1.2;
            transition: all 0.2s ease;
        }

        .live-filter-item:hover {
            border-color: #93c5fd;
            background: #eff6ff;
        }

        .live-filter-item.selected {
            border-color: #2563eb;
            background: #eff6ff;
            box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.15);
        }

        .live-filter-item.live-filter-all.selected {
            border-color: #64748b;
            background: #f1f5f9;
            box-shadow: 0 0 0 2px rgba(100, 116, 139, 0.15);
        }

        .live-filter-hitarea {
            position: absolute;
            inset: 0;
            width: 100%;
            height: 100%;
            opacity: 0;
            border: none !important;
            background: transparent !important;
            cursor: pointer;
            padding: 0 !important;
            margin: 0;
            z-index: 2;
            min-width: 0;
            min-height: 0;
            box-shadow: none !important;
        }

        .live-filter-hitarea:focus {
            outline: none;
        }

        .live-filter-icon {
            width: 26px;
            height: 26px;
            border-radius: 6px;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            font-size: 12px;
            color: #fff !important;
            flex-shrink: 0;
        }

        .live-filter-icon.icon-all { background: #64748b; }
        .live-filter-icon.icon-active { background: #2563eb; }
        .live-filter-icon.icon-today { background: #7c3aed; }
        .live-filter-icon.icon-hour { background: #d97706; }
        .live-filter-icon.icon-live { background: #16a34a; animation: liveIconPulse 2s ease-in-out infinite; }

        @keyframes liveIconPulse {
            0%, 100% { box-shadow: 0 0 0 0 rgba(22, 163, 74, 0.4); }
            50% { box-shadow: 0 0 0 6px rgba(22, 163, 74, 0); }
        }

        .live-filter-text {
            display: inline-flex;
            align-items: baseline;
            gap: 5px;
        }

        .live-filter-count {
            font-size: 14px;
            font-weight: 800;
            color: #0f172a !important;
            margin-bottom: 0;
            display: inline;
        }

        .live-filter-name {
            font-size: 11px;
            font-weight: 600;
            color: #64748b !important;
        }

        .live-filter-active-tag {
            margin-left: auto;
            font-size: 11px;
            color: #64748b;
            white-space: nowrap;
        }

        .live-filter-active-tag strong {
            color: #2563eb;
            font-weight: 700;
        }

        /* Compact institution grid */
        .auth-grid-wrap .mGrid th {
            padding: 5px 7px;
            font-size: 11px;
            font-weight: 600;
            white-space: nowrap;
            line-height: 1.2;
            vertical-align: middle;
        }

        .auth-grid-wrap .mGrid th a {
            font-size: 11px;
            white-space: nowrap;
        }

        .auth-grid-wrap .mGrid th a:after {
            font-size: 9px;
            padding-left: 2px;
        }

        .auth-grid-wrap .mGrid td {
            padding: 4px 7px;
            font-size: 12px;
            line-height: 1.3;
            vertical-align: middle;
        }

        .auth-grid-wrap .mGrid .list-group-item {
            font-size: 11px;
            padding: 1px 0;
        }

        @media (max-width: 992px) {
            .live-filter-active-tag {
                margin-left: 0;
                width: 100%;
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

    <!-- Live Login Monitor -->
    <asp:HiddenField ID="OnlineFilterValue" runat="server" Value="" />
    <div class="live-stats-panel">
        <div class="live-stats-toolbar">
            <h6 class="live-stats-title"><i class="fa fa-signal"></i> Live Monitor</h6>
            <div class="live-filter-group">
                <asp:Panel ID="FilterAllPanel" runat="server" CssClass="live-filter-item live-filter-all selected">
                    <span class="live-filter-icon icon-all"><i class="fa fa-th-list"></i></span>
                    <span class="live-filter-text">
                        <asp:Label ID="AllInstitutionCountLabel" runat="server" CssClass="live-filter-count" Text="0"></asp:Label>
                        <span class="live-filter-name">All</span>
                    </span>
                    <asp:Button ID="FilterAllBtn" runat="server" CssClass="live-filter-hitarea" CausesValidation="false" OnClick="FilterAllBtn_Click" />
                </asp:Panel>

                <asp:Panel ID="FilterActivePanel" runat="server" CssClass="live-filter-item live-filter-active">
                    <span class="live-filter-icon icon-active"><i class="fa fa-users"></i></span>
                    <span class="live-filter-text">
                        <asp:Label ID="LoggedInUsersCountLabel" runat="server" CssClass="live-filter-count" Text="0"></asp:Label>
                        <span class="live-filter-name">Active · 15m</span>
                    </span>
                    <asp:Button ID="FilterActiveBtn" runat="server" CssClass="live-filter-hitarea" CausesValidation="false" OnClick="FilterActiveBtn_Click" />
                </asp:Panel>

                <asp:Panel ID="FilterTodayPanel" runat="server" CssClass="live-filter-item live-filter-today">
                    <span class="live-filter-icon icon-today"><i class="fa fa-calendar"></i></span>
                    <span class="live-filter-text">
                        <asp:Label ID="TodayLoginsLabel" runat="server" CssClass="live-filter-count" Text="0"></asp:Label>
                        <span class="live-filter-name">Today</span>
                    </span>
                    <asp:Button ID="FilterTodayBtn" runat="server" CssClass="live-filter-hitarea" CausesValidation="false" OnClick="FilterTodayBtn_Click" />
                </asp:Panel>

                <asp:Panel ID="FilterLastHourPanel" runat="server" CssClass="live-filter-item live-filter-hour">
                    <span class="live-filter-icon icon-hour"><i class="fa fa-clock-o"></i></span>
                    <span class="live-filter-text">
                        <asp:Label ID="LastHourLoginsLabel" runat="server" CssClass="live-filter-count" Text="0"></asp:Label>
                        <span class="live-filter-name">Last Hour</span>
                    </span>
                    <asp:Button ID="FilterLastHourBtn" runat="server" CssClass="live-filter-hitarea" CausesValidation="false" OnClick="FilterLastHourBtn_Click" />
                </asp:Panel>

                <asp:Panel ID="FilterLiveNowPanel" runat="server" CssClass="live-filter-item live-filter-live">
                    <span class="live-filter-icon icon-live"><i class="fa fa-bolt"></i></span>
                    <span class="live-filter-text">
                        <asp:Label ID="OnlineNowLabel" runat="server" CssClass="live-filter-count" Text="0"></asp:Label>
                        <span class="live-filter-name">Online · 5m</span>
                    </span>
                    <asp:Button ID="FilterLiveNowBtn" runat="server" CssClass="live-filter-hitarea" CausesValidation="false" OnClick="FilterLiveNowBtn_Click" />
                </asp:Panel>
            </div>
            <span class="live-filter-active-tag">Filter: <strong><asp:Label ID="ActiveFilterLabel" runat="server" Text="All"></asp:Label></strong></span>
        </div>
    </div>

    <div class="table-responsive auth-grid-wrap">
        <asp:GridView ID="SchoolGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="SchoolID" DataSourceID="InstitutionSQL" AllowSorting="True" OnRowDataBound="SchoolGridView_RowDataBound">
            <Columns>
                <asp:BoundField DataField="SchoolID" HeaderText="ID" SortExpression="SchoolID" ItemStyle-Width="50px" />
                <asp:HyperLinkField SortExpression="SchoolName" DataNavigateUrlFields="SchoolID" DataNavigateUrlFormatString="Institutions/Institution_Details.aspx?SchoolID={0}" DataTextField="SchoolName" HeaderText="Institution" />
                <asp:BoundField DataField="UserName" HeaderText="User ID" SortExpression="UserName" ItemStyle-Width="90px" />
                <asp:BoundField DataField="Phone" HeaderText="Phone" SortExpression="Phone" ItemStyle-Width="100px" />
                <asp:BoundField DataField="Validation" HeaderText="Valid" SortExpression="Validation" ItemStyle-Width="55px" />
                <asp:TemplateField HeaderText="Online" ItemStyle-Width="70px">
                    <ItemTemplate>
                        <%# GetOnlineStatusBadge(Eval("LastActivity")) %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="LoggedInUser" HeaderText="User" SortExpression="LoggedInUser" NullDisplayText="-" ItemStyle-Width="90px" />
                <asp:BoundField DataField="LoginRole" HeaderText="Role" SortExpression="LoginRole" NullDisplayText="-" ItemStyle-Width="70px" />
                <asp:BoundField DataField="LoginTime" HeaderText="Login" SortExpression="LoginTime" DataFormatString="{0:dd MMM hh:mm tt}" NullDisplayText="-" ItemStyle-Width="110px" />
                <asp:BoundField DataField="LastActivity" HeaderText="Last Act." SortExpression="LastActivity" DataFormatString="{0:dd MMM hh:mm tt}" NullDisplayText="-" ItemStyle-Width="110px" />
                <asp:BoundField DataField="Date" HeaderText="Reg. Date" SortExpression="Date" DataFormatString="{0:dd MMM yyyy}" ItemStyle-Width="90px" />
                <asp:TemplateField HeaderText="Session" SortExpression="EducationYear" ItemStyle-Width="80px">
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
        <asp:SqlDataSource ID="InstitutionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Sch.SchoolID, Sch.SchoolName, Sch.Phone, Sch.Validation, Sch.Date, Sch.UserName, ses.LoggedInUser, ses.LoginRole, ses.LoginTime, ses.LastActivity FROM SchoolInfo AS Sch OUTER APPLY (SELECT TOP 1 u.UserName AS LoggedInUser, u.Category AS LoginRole, u.LoginTime, u.LastActivity FROM User_Active_Sessions u WHERE u.SchoolID = Sch.SchoolID AND (u.LastActivity >= DATEADD(HOUR, -1, GETDATE()) OR CAST(u.LoginTime AS DATE) = CAST(GETDATE() AS DATE)) ORDER BY u.LastActivity DESC) ses ORDER BY ses.LastActivity DESC, Sch.Date DESC, Sch.SchoolID">
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
