<%@ Page Title="SMS Setting" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="SmsSetting.aspx.cs" Inherits="EDUCATION.COM.Authority.SmsSetting" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="row">
        <div class="col">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">Single SMS Provider</h5>
                    <div class="form-group">
                        <asp:RadioButtonList ID="SmsProviderRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="SmsProviderRadioButtonList_SelectedIndexChanged">
                        </asp:RadioButtonList>
                    </div>
                </div>
            </div>
        </div>

        <div class="col">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">Multiple SMS Provider</h5>

                    <div class="form-group">
                        <asp:RadioButtonList ID="SmsProviderMultipleRadioButtonList" runat="server" RepeatDirection="Horizontal" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="SmsProviderRadioButtonList_SelectedIndexChanged">
                        </asp:RadioButtonList>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="card my-4">
        <div class="card-body">
            <div class="d-flex justify-content-between">
                <h5 class="card-title">SMS Sender Setting For Server App</h5>
                <asp:FormView ID="SMSPendingFormView" runat="server" DataSourceID="SMSPendingSQL" RenderOuterTable="false">
                    <ItemTemplate>
                        <h5 class="red-text">Pending SMS: <%# Eval("PendingSMS") %></h5>
                    </ItemTemplate>
                </asp:FormView>
                <asp:SqlDataSource ID="SMSPendingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT COUNT(Attendance_SMSID) AS PendingSMS FROM Attendance_SMS"></asp:SqlDataSource>
            </div>

            <div class="form-group">
                <label>SMS Sending Interval Munite (কত মিনিট পর পর মেসেজ পাঠাবে)</label>
                <asp:TextBox ID="SMSSendingIntervalTextBox" runat="server" CssClass="form-control" TextMode="Number"></asp:TextBox>
            </div>

            <div class="form-group">
                <label>SMS Processing Unit (মিনিটে কত মেসেজ পাঠাবে)</label>
                <asp:TextBox ID="SMSProcessingUnitTextBox" runat="server" CssClass="form-control" TextMode="Number" max="1000"></asp:TextBox>
            </div>

            <div class="form-group">
                <asp:Button ID="SMSSettingUpdateButton" runat="server" CssClass="btn btn-primary" Text="Update SMS Setting" OnClick="SMSSettingUpdateButton_Click" />
            </div>

            <asp:SqlDataSource ID="SmsSettingSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EduConnectionString %>" SelectCommand="SELECT SmsProvider, SmsProviderMultiple, SmsSendInterval, SmsProcessingUnit FROM SikkhaloySetting" UpdateCommand="UPDATE SikkhaloySetting SET SmsProvider = @SmsProvider, SmsProviderMultiple = @SmsProviderMultiple" InsertCommand="UPDATE SikkhaloySetting SET SmsSendInterval = @SmsSendInterval, SmsProcessingUnit = @SmsProcessingUnit">
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

    <asp:GridView ID="SmsSenderGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="AttendanceSmsSenderId" DataSourceID="SmsSenderSQL" AllowPaging="True" AllowSorting="True" PageSize="15">
        <Columns>
            <asp:BoundField DataField="AppStartTime" HeaderText="App Start Time" SortExpression="AppStartTime" DataFormatString="{0:d MMM, yyyy (hh:mm tt)}" />
            <asp:BoundField DataField="AppCloseTime" HeaderText="App Close Time" SortExpression="AppCloseTime" DataFormatString="{0:d MMM, yyyy (hh:mm tt)}" />
            <asp:BoundField DataField="TotalEventCall" HeaderText="Event Call" SortExpression="TotalEventCall" />
            <asp:BoundField DataField="TotalSmsSend" HeaderText="SMS Send" SortExpression="TotalSmsSend" />
            <asp:BoundField DataField="TotalSmsFailed" HeaderText="SMS Failed" SortExpression="TotalSmsFailed" />
        </Columns>
         <PagerStyle CssClass="pgr" />
    </asp:GridView>
    <asp:SqlDataSource ID="SmsSenderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT AttendanceSmsSenderId, AppStartTime, AppCloseTime, TotalEventCall, TotalSmsSend, TotalSmsFailed FROM Attendance_SMS_Sender ORDER BY AttendanceSmsSenderId DESC"></asp:SqlDataSource>

    <h4 class="mt-4">Failed SMS Records</h4>
    <asp:GridView ID="SmsFailGridView" CssClass="mGrid" runat="server" AutoGenerateColumns="False" DataKeyNames="AttendanceSmsFailedId" DataSourceID="SmsFailSQL" AllowPaging="True" AllowSorting="True" PageSize="30">
        <Columns>
            <asp:BoundField DataField="SchoolName" HeaderText="Institution" SortExpression="SchoolName" />
            <asp:BoundField DataField="FailedReson" HeaderText="Failed Reson" SortExpression="FailedReson" />
            <asp:BoundField DataField="AttendanceDate" HeaderText="Date" SortExpression="AttendanceDate" DataFormatString="{0:d MMM, yyyy}" />
            <asp:TemplateField HeaderText="Create" SortExpression="CreateTime">
                <ItemTemplate>
                    <%# Eval("CreateTime") %>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Schedule" SortExpression="ScheduleTime">
                <ItemTemplate>
                    <%# Eval("ScheduleTime") %>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="SMS_Text" HeaderText="Text" SortExpression="SMS_Text"/>
            <asp:BoundField DataField="MobileNo" HeaderText="Number" SortExpression="MobileNo" />
            <asp:BoundField DataField="AttendanceStatus" HeaderText="Attendance" SortExpression="AttendanceStatus" />
            <asp:BoundField DataField="SMS_TimeOut" HeaderText="TimeOut" SortExpression="SMS_TimeOut" />
            <asp:BoundField DataField="InsertDate" HeaderText="Fail Date" SortExpression="InsertDate" DataFormatString="{0:d MMM, yyyy (hh:mm tt)}" />
        </Columns>
        <PagerStyle CssClass="pgr" />
    </asp:GridView>
    <asp:SqlDataSource ID="SmsFailSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Attendance_SMS_Failed.AttendanceSmsFailedId, Attendance_SMS_Failed.SchoolID, ISNULL(CONVERT(varchar(15),Attendance_SMS_Failed.ScheduleTime,100),'') AS ScheduleTime,  ISNULL(CONVERT(varchar(15),Attendance_SMS_Failed.CreateTime,100),'') AS CreateTime,  ISNULL(CONVERT(varchar(15),Attendance_SMS_Failed.SentTime,100),'') AS SentTime, Attendance_SMS_Failed.CreateTime, Attendance_SMS_Failed.AttendanceDate, Attendance_SMS_Failed.SMS_Text, Attendance_SMS_Failed.MobileNo, Attendance_SMS_Failed.AttendanceStatus, Attendance_SMS_Failed.SMS_TimeOut, Attendance_SMS_Failed.FailedReson, Attendance_SMS_Failed.InsertDate, SchoolInfo.SchoolName FROM Attendance_SMS_Failed INNER JOIN SchoolInfo ON Attendance_SMS_Failed.SchoolID = SchoolInfo.SchoolID"></asp:SqlDataSource>
</asp:Content>
