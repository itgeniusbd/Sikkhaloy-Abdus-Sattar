<%@ Page Title="SMS Setting" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="SmsSetting.aspx.cs" Inherits="EDUCATION.COM.Authority.SmsSetting" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .nav-tabs .nav-link {
            border-radius: 8px 8px 0 0;
            font-weight: 500;
            color: #495057;
            background-color: #e9ecef;
        }
        .nav-tabs .nav-link:hover {
            color: #667eea;
        }
        .nav-tabs .nav-link.active {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white !important;
            border-color: #667eea #667eea #fff;
        }
        .tab-content {
            border: 1px solid #dee2e6;
            border-top: none;
            border-radius: 0 0 8px 8px;
            padding: 20px;
            background: white;
        }
        .stats-card {
            border-left: 4px solid #667eea;
            background: #f8f9fa;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 15px;
        }
        .stats-card h3 {
            color: #667eea;
            font-weight: bold;
            margin: 0;
        }
        .stats-card p {
            color: #666;
            margin: 5px 0 0 0;
            font-size: 14px;
        }
        .search-panel {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 20px;
        }
        .btn-search {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border: none;
            color: white;
            padding: 8px 25px;
            border-radius: 5px;
            font-weight: 500;
        }
        .btn-search:hover {
            background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
            color: white;
        }
        .card-custom {
            border: none;
            border-radius: 8px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }
        .card-custom .card-header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            font-weight: 600;
            border-radius: 8px 8px 0 0;
        }
        .red-text {
            color: #eb3349;
            font-weight: bold;
        }
        .green-text {
            color: #11998e;
            font-weight: bold;
        }
        .badge-pending {
            background-color: #ffc107;
            color: #000;
            padding: 5px 15px;
            border-radius: 20px;
            font-size: 14px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container-fluid">
        <div class="row mb-3">
            <div class="col-12">
                <h3 class="mb-3">
                    <i class="fas fa-sms"></i> SMS Management System
                </h3>
            </div>
        </div>

        <!-- Tabs Navigation -->
        <ul class="nav nav-tabs" id="smsTab" role="tablist">
            <li class="nav-item" role="presentation">
                <button class="nav-link active" id="settings-tab" data-toggle="tab" data-target="#settings" type="button" role="tab">
                    <i class="fas fa-cog"></i> Settings
                </button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="records-tab" data-toggle="tab" data-target="#records" type="button" role="tab">
                    <i class="fas fa-history"></i> SMS Sender Records
                </button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="failed-tab" data-toggle="tab" data-target="#failed" type="button" role="tab">
                    <i class="fas fa-exclamation-triangle"></i> Failed SMS
                </button>
            </li>
        </ul>

        <!-- Tab Content -->
        <div class="tab-content" id="smsTabContent">
            
            <!-- Settings Tab -->
            <div class="tab-pane fade show active" id="settings" role="tabpanel">
                <div class="row">
                    <div class="col-md-6">
                        <div class="card-custom">
                            <div class="card-header">
                                <i class="fas fa-broadcast-tower"></i> Single SMS Provider
                            </div>
                            <div class="card-body">
                                <asp:RadioButtonList ID="SmsProviderRadioButtonList" runat="server" 
                                    RepeatDirection="Horizontal" CssClass="form-control" 
                                    AutoPostBack="true" OnSelectedIndexChanged="SmsProviderRadioButtonList_SelectedIndexChanged">
                                </asp:RadioButtonList>
                            </div>
                        </div>
                    </div>

                    <div class="col-md-6">
                        <div class="card-custom">
                            <div class="card-header">
                                <i class="fas fa-server"></i> Multiple SMS Provider
                            </div>
                            <div class="card-body">
                                <asp:RadioButtonList ID="SmsProviderMultipleRadioButtonList" runat="server" 
                                    RepeatDirection="Horizontal" CssClass="form-control" 
                                    AutoPostBack="true" OnSelectedIndexChanged="SmsProviderRadioButtonList_SelectedIndexChanged">
                                </asp:RadioButtonList>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="card-custom">
                    <div class="card-header d-flex justify-content-between align-items-center">
                        <span><i class="fas fa-tools"></i> SMS Sender Settings For Server App</span>
                        <asp:FormView ID="SMSPendingFormView" runat="server" DataSourceID="SMSPendingSQL" RenderOuterTable="false">
                            <ItemTemplate>
                                <span class="badge-pending">
                                    <i class="fas fa-clock"></i> Pending SMS: <%# Eval("PendingSMS") %>
                                </span>
                            </ItemTemplate>
                        </asp:FormView>
                        <asp:SqlDataSource ID="SMSPendingSQL" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                            SelectCommand="SELECT COUNT(Attendance_SMSID) AS PendingSMS FROM Attendance_SMS">
                        </asp:SqlDataSource>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-6">
                                <div class="form-group">
                                    <label><i class="fas fa-clock"></i> SMS Sending Interval (Minutes)</label>
                                    <small class="text-muted d-block mb-2">কত মিনিট পর পর মেসেজ পাঠাবে</small>
                                    <asp:TextBox ID="SMSSendingIntervalTextBox" runat="server" 
                                        CssClass="form-control" TextMode="Number" placeholder="e.g., 2">
                                    </asp:TextBox>
                                </div>
                            </div>

                            <div class="col-md-6">
                                <div class="form-group">
                                    <label><i class="fas fa-tachometer-alt"></i> SMS Processing Unit</label>
                                    <small class="text-muted d-block mb-2">মিনিটে কত মেসেজ পাঠাবে (Max: 1000)</small>
                                    <asp:TextBox ID="SMSProcessingUnitTextBox" runat="server" 
                                        CssClass="form-control" TextMode="Number" max="1000" placeholder="e.g., 500">
                                    </asp:TextBox>
                                </div>
                            </div>
                        </div>

                        <div class="form-group text-center mt-3">
                            <asp:Button ID="SMSSettingUpdateButton" runat="server" 
                                CssClass="btn btn-search btn-lg" Text="Update SMS Settings" 
                                OnClick="SMSSettingUpdateButton_Click" />
                        </div>

                        <asp:SqlDataSource ID="SmsSettingSQL" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:EduConnectionString %>" 
                            SelectCommand="SELECT SmsProvider, SmsProviderMultiple, SmsSendInterval, SmsProcessingUnit FROM SikkhaloySetting" 
                            UpdateCommand="UPDATE SikkhaloySetting SET SmsProvider = @SmsProvider, SmsProviderMultiple = @SmsProviderMultiple" 
                            InsertCommand="UPDATE SikkhaloySetting SET SmsSendInterval = @SmsSendInterval, SmsProcessingUnit = @SmsProcessingUnit">
                            <InsertParameters>
                                <asp:ControlParameter ControlID="SMSSendingIntervalTextBox" Name="SmsSendInterval" PropertyName="Text" />
                                <asp:ControlParameter ControlID="SMSProcessingUnitTextBox" Name="SmsProcessingUnit" PropertyName="Text" />
                            </InsertParameters>
                            <UpdateParameters>
                                <asp:ControlParameter ControlID="SmsProviderRadioButtonList" Name="SmsProvider" PropertyName="SelectedValue" />
                                <asp:ControlParameter ControlID="SmsProviderMultipleRadioButtonList" Name="SmsProviderMultiple" PropertyName="SelectedValue" />
                            </UpdateParameters>
                        </asp:SqlDataSource>
                    </div>
                </div>
            </div>

            <!-- SMS Sender Records Tab -->
            <div class="tab-pane fade" id="records" role="tabpanel">
                <div class="search-panel">
                    <div class="row">
                        <div class="col-md-4">
                            <div class="form-group">
                                <label><i class="fas fa-calendar"></i> Start Date</label>
                                <asp:TextBox ID="RecordsStartDateTextBox" runat="server" 
                                    CssClass="form-control" TextMode="Date">
                                </asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label><i class="fas fa-calendar"></i> End Date</label>
                                <asp:TextBox ID="RecordsEndDateTextBox" runat="server" 
                                    CssClass="form-control" TextMode="Date">
                                </asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="form-group">
                                <label>&nbsp;</label>
                                <asp:Button ID="SearchRecordsButton" runat="server" 
                                    CssClass="btn btn-search btn-block" Text="Search Records" 
                                    OnClick="SearchRecordsButton_Click" />
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Button ID="ClearRecordsFilterButton" runat="server" 
                                CssClass="btn btn-secondary btn-sm" Text="Clear Filter" 
                                OnClick="ClearRecordsFilterButton_Click" />
                        </div>
                    </div>
                </div>

                <div class="table-responsive">
                    <asp:GridView ID="SmsSenderGridView" CssClass="mGrid table table-striped table-hover" 
                        runat="server" AutoGenerateColumns="False" 
                        DataKeyNames="AttendanceSmsSenderId" DataSourceID="SmsSenderSQL" 
                        AllowPaging="True" AllowSorting="True" PageSize="15">
                        <Columns>
                            <asp:BoundField DataField="AttendanceSmsSenderId" HeaderText="ID" SortExpression="AttendanceSmsSenderId" />
                            <asp:BoundField DataField="AppStartTime" HeaderText="App Start Time" 
                                SortExpression="AppStartTime" DataFormatString="{0:d MMM, yyyy (hh:mm tt)}" />
                            <asp:BoundField DataField="AppCloseTime" HeaderText="App Close Time" 
                                SortExpression="AppCloseTime" DataFormatString="{0:d MMM, yyyy (hh:mm tt)}" />
                            <asp:BoundField DataField="TotalEventCall" HeaderText="Event Call" 
                                SortExpression="TotalEventCall" ItemStyle-CssClass="text-center" />
                            <asp:BoundField DataField="TotalSmsSend" HeaderText="SMS Sent" 
                                SortExpression="TotalSmsSend" ItemStyle-CssClass="text-center green-text" />
                            <asp:BoundField DataField="TotalSmsFailed" HeaderText="SMS Failed" 
                                SortExpression="TotalSmsFailed" ItemStyle-CssClass="text-center red-text" />
                        </Columns>
                        <PagerStyle CssClass="pgr" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="SmsSenderSQL" runat="server" 
                        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                        SelectCommand="SELECT AttendanceSmsSenderId, AppStartTime, AppCloseTime, TotalEventCall, TotalSmsSend, TotalSmsFailed FROM Attendance_SMS_Sender WHERE (CAST(AppStartTime AS DATE) >= CAST(@StartDate AS DATE) OR @StartDate IS NULL OR @StartDate = '') AND (CAST(AppStartTime AS DATE) <= CAST(@EndDate AS DATE) OR @EndDate IS NULL OR @EndDate = '') ORDER BY AttendanceSmsSenderId DESC">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="RecordsStartDateTextBox" Name="StartDate" PropertyName="Text" Type="String" />
                            <asp:ControlParameter ControlID="RecordsEndDateTextBox" Name="EndDate" PropertyName="Text" Type="String" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

            <!-- Failed SMS Tab -->
            <div class="tab-pane fade" id="failed" role="tabpanel">
                <div class="search-panel">
                    <div class="row">
                        <div class="col-md-2">
                            <div class="form-group">
                                <label><i class="fas fa-calendar"></i> Start Date</label>
                                <asp:TextBox ID="FailedStartDateTextBox" runat="server" 
                                    CssClass="form-control" TextMode="Date">
                                </asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label><i class="fas fa-calendar"></i> End Date</label>
                                <asp:TextBox ID="FailedEndDateTextBox" runat="server" 
                                    CssClass="form-control" TextMode="Date">
                                </asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label><i class="fas fa-exclamation-circle"></i> Failed Reason</label>
                                <asp:DropDownList ID="FailedReasonDropDown" runat="server" 
                                    CssClass="form-control">
                                    <asp:ListItem Value="">All Reasons</asp:ListItem>
                                    <asp:ListItem Value="Not current date">Not current date</asp:ListItem>
                                    <asp:ListItem Value="SMS sending time up">SMS sending time up</asp:ListItem>
                                    <asp:ListItem Value="Insufficient SMS Balance">Insufficient SMS Balance</asp:ListItem>
                                    <asp:ListItem Value="SMS Send Failed">SMS Send Failed</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="form-group">
                                <label><i class="fas fa-school"></i> প্রতিষ্ঠান / Institution</label>
                                <asp:DropDownList ID="InstitutionDropDown" runat="server" 
                                    CssClass="form-control" DataSourceID="InstitutionSQL"
                                    DataTextField="SchoolName" DataValueField="SchoolID"
                                    AppendDataBoundItems="true">
                                    <asp:ListItem Value="">All Institutions</asp:ListItem>
                                </asp:DropDownList>
                                <asp:SqlDataSource ID="InstitutionSQL" runat="server" 
                                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                                    SelectCommand="SELECT SchoolID, SchoolName FROM SchoolInfo ORDER BY SchoolName">
                                </asp:SqlDataSource>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label>&nbsp;</label>
                                <asp:Button ID="SearchFailedButton" runat="server" 
                                    CssClass="btn btn-search btn-block" Text="Search Failed SMS" 
                                    OnClick="SearchFailedButton_Click" />
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <asp:Button ID="ClearFailedFilterButton" runat="server" 
                                CssClass="btn btn-secondary btn-sm" Text="Clear Filter" 
                                OnClick="ClearFailedFilterButton_Click" />
                        </div>
                    </div>
                </div>

                <div class="stats-card">
                    <asp:FormView ID="FailedStatsFormView" runat="server" DataSourceID="FailedStatsSQL" RenderOuterTable="false">
                        <ItemTemplate>
                            <div class="row">
                                <div class="col-md-4 text-center">
                                    <h3><%# Eval("TotalFailed") %></h3>
                                    <p>Total Failed SMS</p>
                                </div>
                                <div class="col-md-4 text-center">
                                    <h3 class="red-text"><%# Eval("TodayFailed") %></h3>
                                    <p>Today's Failed SMS</p>
                                </div>
                                <div class="col-md-4 text-center">
                                    <h3 class="text-warning"><%# Eval("ThisWeekFailed") %></h3>
                                    <p>This Week's Failed SMS</p>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:FormView>
                    <asp:SqlDataSource ID="FailedStatsSQL" runat="server" 
                        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                        SelectCommand="SELECT COUNT(*) AS TotalFailed, SUM(CASE WHEN CAST(InsertDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS TodayFailed, SUM(CASE WHEN InsertDate >= DATEADD(DAY, -7, GETDATE()) THEN 1 ELSE 0 END) AS ThisWeekFailed FROM Attendance_SMS_Failed INNER JOIN SchoolInfo ON Attendance_SMS_Failed.SchoolID = SchoolInfo.SchoolID WHERE (CAST(AttendanceDate AS DATE) >= CAST(@StartDate AS DATE) OR @StartDate IS NULL OR @StartDate = '') AND (CAST(AttendanceDate AS DATE) <= CAST(@EndDate AS DATE) OR @EndDate IS NULL OR @EndDate = '') AND (FailedReson = @FailedReason OR @FailedReason IS NULL OR @FailedReason = '') AND (Attendance_SMS_Failed.SchoolID = @SchoolID OR @SchoolID IS NULL OR @SchoolID = '')">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="FailedStartDateTextBox" Name="StartDate" PropertyName="Text" Type="String" ConvertEmptyStringToNull="true" />
                            <asp:ControlParameter ControlID="FailedEndDateTextBox" Name="EndDate" PropertyName="Text" Type="String" ConvertEmptyStringToNull="true" />
                            <asp:ControlParameter ControlID="FailedReasonDropDown" Name="FailedReason" PropertyName="SelectedValue" Type="String" ConvertEmptyStringToNull="true" />
                            <asp:ControlParameter ControlID="InstitutionDropDown" Name="SchoolID" PropertyName="SelectedValue" Type="String" ConvertEmptyStringToNull="true" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>

                <div class="table-responsive">
                    <asp:GridView ID="SmsFailGridView" CssClass="mGrid table table-striped table-hover" 
                        runat="server" AutoGenerateColumns="False" 
                        DataKeyNames="AttendanceSmsFailedId" DataSourceID="SmsFailSQL" 
                        AllowPaging="True" AllowSorting="True" PageSize="30">
                        <Columns>
                            <asp:BoundField DataField="AttendanceSmsFailedId" HeaderText="ID" SortExpression="AttendanceSmsFailedId" />
                            <asp:BoundField DataField="SchoolName" HeaderText="Institution" SortExpression="SchoolName" />
                            <asp:TemplateField HeaderText="Failed Reason" SortExpression="FailedReson">
                                <ItemTemplate>
                                    <span class="badge badge-danger"><%# Eval("FailedReson") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="AttendanceDate" HeaderText="Date" 
                                SortExpression="AttendanceDate" DataFormatString="{0:d MMM, yyyy}" />
                            <asp:TemplateField HeaderText="Create" SortExpression="CreateTime">
                                <ItemTemplate>
                                    <small><%# Eval("CreateTime") %></small>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Schedule" SortExpression="ScheduleTime">
                                <ItemTemplate>
                                    <small><%# Eval("ScheduleTime") %></small>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="SMS_Text" HeaderText="Text" SortExpression="SMS_Text" />
                            <asp:BoundField DataField="MobileNo" HeaderText="Number" SortExpression="MobileNo" />
                            <asp:BoundField DataField="AttendanceStatus" HeaderText="Attendance" SortExpression="AttendanceStatus" />
                            <asp:BoundField DataField="SMS_TimeOut" HeaderText="TimeOut" SortExpression="SMS_TimeOut" />
                            <asp:BoundField DataField="InsertDate" HeaderText="Fail Date" 
                                SortExpression="InsertDate" DataFormatString="{0:d MMM, yyyy (hh:mm tt)}" />
                        </Columns>
                        <PagerStyle CssClass="pgr" />
                    </asp:GridView>
                    <asp:SqlDataSource ID="SmsFailSQL" runat="server" 
                        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
                        SelectCommand="SELECT Attendance_SMS_Failed.AttendanceSmsFailedId, Attendance_SMS_Failed.SchoolID, ISNULL(CONVERT(varchar(15),Attendance_SMS_Failed.ScheduleTime,100),'') AS ScheduleTime, ISNULL(CONVERT(varchar(15),Attendance_SMS_Failed.CreateTime,100),'') AS CreateTime, ISNULL(CONVERT(varchar(15),Attendance_SMS_Failed.SentTime,100),'') AS SentTime, Attendance_SMS_Failed.AttendanceDate, Attendance_SMS_Failed.SMS_Text, Attendance_SMS_Failed.MobileNo, Attendance_SMS_Failed.AttendanceStatus, Attendance_SMS_Failed.SMS_TimeOut, Attendance_SMS_Failed.FailedReson, Attendance_SMS_Failed.InsertDate, SchoolInfo.SchoolName FROM Attendance_SMS_Failed INNER JOIN SchoolInfo ON Attendance_SMS_Failed.SchoolID = SchoolInfo.SchoolID WHERE (CAST(AttendanceDate AS DATE) >= CAST(@StartDate AS DATE) OR @StartDate IS NULL OR @StartDate = '') AND (CAST(AttendanceDate AS DATE) <= CAST(@EndDate AS DATE) OR @EndDate IS NULL OR @EndDate = '') AND (FailedReson = @FailedReason OR @FailedReason IS NULL OR @FailedReason = '') AND (Attendance_SMS_Failed.SchoolID = @SchoolID OR @SchoolID IS NULL OR @SchoolID = '') ORDER BY AttendanceSmsFailedId DESC">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="FailedStartDateTextBox" Name="StartDate" PropertyName="Text" Type="String" ConvertEmptyStringToNull="true" />
                            <asp:ControlParameter ControlID="FailedEndDateTextBox" Name="EndDate" PropertyName="Text" Type="String" ConvertEmptyStringToNull="true" />
                            <asp:ControlParameter ControlID="FailedReasonDropDown" Name="FailedReason" PropertyName="SelectedValue" Type="String" ConvertEmptyStringToNull="true" />
                            <asp:ControlParameter ControlID="InstitutionDropDown" Name="SchoolID" PropertyName="SelectedValue" Type="String" ConvertEmptyStringToNull="true" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </div>
            </div>

        </div>
    </div>

    <script>
        // Activate Bootstrap tabs
        $(document).ready(function () {
            $('#smsTab button').on('click', function (e) {
                e.preventDefault();
                $(this).tab('show');
            });
        });
    </script>
</asp:Content>
