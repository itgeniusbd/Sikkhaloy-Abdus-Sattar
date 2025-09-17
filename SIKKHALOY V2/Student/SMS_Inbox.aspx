<%@ Page Title="SMS Inbox" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="SMS_Inbox.aspx.cs" Inherits="EDUCATION.COM.Student.SMS_Inbox" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>SMS Inbox</h3>
    <div class="table-responsive">
        <asp:GridView ID="SMSGridView" AllowPaging="true" PageSize="30" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataSourceID="SMSRecordSQL">
            <Columns>
                <asp:BoundField DataField="PhoneNumber" HeaderText="Phone" SortExpression="PhoneNumber" />
                <asp:BoundField DataField="TextSMS" HeaderText="Text " SortExpression="TextSMS" />
                <asp:BoundField DataField="PurposeOfSMS" HeaderText="Purpose " SortExpression="PurposeOfSMS" />
                <asp:BoundField DataField="Date" HeaderText="Date" SortExpression="Date" />
            </Columns>
            <EmptyDataTemplate>
                No records
            </EmptyDataTemplate>
            <PagerStyle CssClass="pgr" />
        </asp:GridView>
        <asp:SqlDataSource ID="SMSRecordSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT SMS_Send_Record.PhoneNumber, SMS_Send_Record.TextSMS, SMS_Send_Record.Date, SMS_Send_Record.TextCount, SMS_Send_Record.SMSCount, SMS_Send_Record.PurposeOfSMS FROM SMS_Send_Record INNER JOIN SMS_OtherInfo ON SMS_Send_Record.SMS_Send_ID = SMS_OtherInfo.SMS_Send_ID WHERE (SMS_OtherInfo.SchoolID = @SchoolID) AND (SMS_OtherInfo.StudentID = @StudentID) AND (SMS_OtherInfo.EducationYearID = @EducationYearID)">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                <asp:SessionParameter Name="StudentID" SessionField="StudentID" />
                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>
    <script>
        $(function () {
            $("#_7").addClass("active");
        });
    </script>
</asp:Content>
