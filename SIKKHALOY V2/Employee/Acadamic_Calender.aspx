<%@ Page Title="Academic Calendar" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Acadamic_Calender.aspx.cs" Inherits="EDUCATION.COM.Employee.Acadamic_Calender" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Acadamic_Calender.css?v=4" rel="stylesheet" />
    <style>
  .page-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
            border-radius: 8px;
            margin-bottom: 20px;
    box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
   }
   
        .page-header h3 {
         color: white;
            margin: 0;
   font-weight: 600;
          text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
    }
        
        .page-header .header-subtitle {
        color: rgba(255, 255, 255, 0.9);
          font-size: 14px;
        margin-top: 5px;
        }
     
        .action-buttons {
            margin-bottom: 20px;
        }
    
    .btn-add-calendar {
     background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
         padding: 10px 20px;
  border-radius: 6px;
   text-decoration: none;
            font-weight: 600;
        display: inline-block;
        margin-right: 10px;
         transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(245, 87, 108, 0.3);
        }
        
 .btn-add-calendar:hover {
   transform: translateY(-2px);
 box-shadow: 0 4px 12px rgba(245, 87, 108, 0.4);
    color: white;
   text-decoration: none;
        }
    
        .btn-print {
            background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
     color: white;
    padding: 10px 20px;
         border-radius: 6px;
            border: none;
      font-weight: 600;
        cursor: pointer;
         transition: all 0.3s ease;
            box-shadow: 0 2px 8px rgba(79, 172, 254, 0.3);
        }
        
        .btn-print:hover {
         transform: translateY(-2px);
 box-shadow: 0 4px 12px rgba(79, 172, 254, 0.4);
        }
        
    .calendar-card {
 border-radius: 8px;
            overflow: hidden;
          box-shadow: 0 2px 15px rgba(0, 0, 0, 0.1);
    margin-top: 20px;
   }
        
        .calendar-card-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
          padding: 15px 20px;
       border: none;
            text-align: center;
        }
        
        .calendar-card-header strong {
            font-size: 18px;
    font-weight: 600;
display: block;
        margin-bottom: 5px;
        }
    
      .calendar-card-header small {
            opacity: 0.9;
        font-size: 13px;
   }
    
        @media print {
         .NoPrint, .action-buttons, .page-header {
display: none !important;
            }
    
     .calendar-card {
            box-shadow: none;
}
}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">


    <!-- Action Buttons -->
    <div class="action-buttons NoPrint wow fadeIn">
        <a href="Add_Holidays.aspx" class="btn-add-calendar">
     <i class="fa fa-plus-circle"></i> Add New/Modify Academic Calendar
   </a>
        <button class="btn-print" onclick="window.print();">
            <i class="fa fa-print"></i> Print Calendar
   </button>
    </div>

    <!-- Calendar Card -->
  <div class="calendar-card wow fadeIn">
        <div class="calendar-card-header">
     <strong>
       <i class="fa fa-calendar-alt"></i> 
   একাডেমিক ক্যালেন্ডার | Academic Calendar | التقويم الأكاديمي
       </strong>
       <small>
      <i class="fa fa-globe"></i> Multi-Language Support (English, বাংলা, العربية)
         </small>
        </div>
        <div class="card-body" style="padding: 0;">
 <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
       <ContentTemplate>
    <div class="table-responsive" style="overflow-y: hidden !important">
  <asp:Calendar ID="HolidayCalendar" OnDayRender="HolidayCalendar_DayRender" runat="server" 
  NextMonthText="." PrevMonthText="." SelectMonthText="»" SelectWeekText="›" 
 CellPadding="0" CssClass="myCalendar" Width="100%" FirstDayOfWeek="Saturday" SelectionMode="None">
          <DayStyle CssClass="myCalendarDay"/>
         <DayHeaderStyle CssClass="myCalendarDayHeader"/>
     <SelectedDayStyle CssClass="myCalendarSelector"/>
  <TodayDayStyle CssClass="myCalendarToday" />
               <SelectorStyle CssClass="myCalendarSelector" />
               <NextPrevStyle CssClass="myCalendarNextPrev" />
             <TitleStyle CssClass="myCalendarTitle" />
  </asp:Calendar>
       </div>
        </ContentTemplate>
     </asp:UpdatePanel>
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
