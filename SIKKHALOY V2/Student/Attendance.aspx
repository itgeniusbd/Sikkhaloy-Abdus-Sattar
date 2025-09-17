<%@ Page Title="Attendance" Language="C#" MasterPageFile="~/Basic_Student.Master" AutoEventWireup="true" CodeBehind="Attendance.aspx.cs" Inherits="EDUCATION.COM.Student.Attendance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Employee/CSS/Acadamic_Calender.css" rel="stylesheet" />
    <style>
        /*Attendance Calender*/
        .Pre { background-color: #8CC63E; color: #fff; border: 1px solid #fff; text-align: center; padding: 5px 0; }
        .Abs { background-color: #C90035; color: #fff; border: 1px solid #fff; text-align: center; padding: 5px 0; }
        .Late { background-color: #FBB917; color: #fff; border: 1px solid #fff; text-align: center; padding: 5px 0; }
        .Late_Abs { background-color: #FF5627; color: #fff; border: 1px solid #fff; text-align: center; padding: 5px 0; }
        .Att_Holidays { background-color: #06d79c; border: 1px solid #fff; color: #fff; text-align: center; padding: 5px 0; }

        #Attendance .Appointment { color: #ffffff; font-family: tahoma; font-size: 15px; }
        .Re_Desin { float: left; font-weight: bold; padding: 5px 10px; }
        #Attendance .myCalendar tr td.myCalendarDay { height: 60px; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Attendance</h3>

    <asp:UpdatePanel ID="UpdatePanel3" runat="server">
        <ContentTemplate>
            <div class="card">
                <div class="card-body">
                    <div class="Pre Re_Desin">Present</div>
                    <div class="Abs Re_Desin">Absent</div>
                    <div class="Late Re_Desin">Late</div>
                    <div class="Late_Abs Re_Desin">Late Abs</div>
                    <div class="Att_Holidays Re_Desin">Holidays</div>
                    <asp:Calendar ID="AttendanceCalendar" SelectionMode="None" OnDayRender="AttendanceCalendar_DayRender" runat="server" Font-Names="Tahoma" Font-Size="20px" NextMonthText="." PrevMonthText="." SelectMonthText="»" SelectWeekText="›" CellPadding="0" CssClass="myCalendar" Width="100%" FirstDayOfWeek="Saturday">
                        <DayStyle CssClass="myCalendarDay" />
                        <DayHeaderStyle CssClass="myCalendarDayHeader" />
                        <SelectedDayStyle CssClass="myCalendarSelector" />
                        <TodayDayStyle CssClass="myCalendarToday" />
                        <SelectorStyle CssClass="myCalendarSelector" />
                        <NextPrevStyle CssClass="myCalendarNextPrev" />
                        <TitleStyle CssClass="myCalendarTitle" />
                    </asp:Calendar>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>

    <script>
        $(function () {
            $("#_5").addClass("active");
        });
    </script>
</asp:Content>
