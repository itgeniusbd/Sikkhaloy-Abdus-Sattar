<%@ Page Title="Academic Calendar" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Acadamic_Calender.aspx.cs" Inherits="EDUCATION.COM.Employee.Acadamic_Calender" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Acadamic_Calender.css?v=2" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Academic Calendar</h3>
    <a href="Add_Holidays.aspx" class="NoPrint">Add New/Modify Academic Calendar</a>

    <div class="card">
        <div class="card-body">
            <div class="table-responsive" style="overflow-y: hidden !important">
                <asp:Calendar ID="HolidayCalendar" OnDayRender="HolidayCalendar_DayRender" runat="server" NextMonthText="." PrevMonthText="." SelectMonthText="»" SelectWeekText="›" CellPadding="0" CssClass="myCalendar" Width="100%" FirstDayOfWeek="Saturday">
                    <DayStyle CssClass="myCalendarDay"/>
                    <DayHeaderStyle CssClass="myCalendarDayHeader"/>
                    <SelectedDayStyle CssClass="myCalendarSelector"/>
                    <TodayDayStyle CssClass="myCalendarToday" />
                    <SelectorStyle CssClass="myCalendarSelector" />
                    <NextPrevStyle CssClass="myCalendarNextPrev" />
                    <TitleStyle CssClass="myCalendarTitle" />
                </asp:Calendar>
            </div>

            <input id="Submit1" class="btn btn-primary" type="button" value="Print" onclick="window.print();" />
        </div>
    </div>
    <asp:UpdateProgress ID="UpdateProgress" runat="server">
        <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
                <img src="../CSS/loading.gif" alt="Loading..." />
                <br />
                <b>Loading...</b>
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>
</asp:Content>
