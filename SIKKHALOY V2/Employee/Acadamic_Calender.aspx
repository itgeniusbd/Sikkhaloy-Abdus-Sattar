<%@ Page Title="Academic Calendar" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Acadamic_Calender.aspx.cs" Inherits="EDUCATION.COM.Employee.Acadamic_Calender" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Acadamic_Calender.css?v=6" rel="stylesheet" />
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
        .holiday-list-card { border-radius: 10px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.12); margin-top: 24px; }
        .holiday-list-card .hlist-header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 14px 20px; font-weight: 600; font-size: 15px; }
        /* Tab styles */
        .h-nav-tabs { border-bottom: none; margin: 0; padding: 0 16px; background: #f8f9fa; }
        .h-nav-tabs .nav-item .nav-link { border: none; border-radius: 0; padding: 10px 20px; font-weight: 600; color: #555; font-size: 13px; border-bottom: 3px solid transparent; transition: all .2s; }
        .h-nav-tabs .nav-item .nav-link.active { color: #667eea; border-bottom: 3px solid #667eea; background: transparent; }
        .h-nav-tabs .nav-item .nav-link:hover { color: #764ba2; background: transparent; }
        .h-tab-content { padding: 0; }
        /* Action button inline */
        .act-wrap { white-space: nowrap; }
        .btn-hedit { display: inline-flex; align-items: center; gap: 4px; background: linear-gradient(135deg,#4facfe,#00f2fe); color: #fff; border: none; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600; cursor: pointer; transition: all .2s; }
        .btn-hdel  { display: inline-flex; align-items: center; gap: 4px; background: linear-gradient(135deg,#f5576c,#f093fb); color: #fff; border: none; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600; cursor: pointer; transition: all .2s; margin-left: 6px; }
        .btn-hedit:hover { background: linear-gradient(135deg,#2196f3,#00bcd4); color:#fff; text-decoration:none; transform:translateY(-1px); }
        .btn-hdel:hover  { background: linear-gradient(135deg,#c0392b,#e91e8c); color:#fff; text-decoration:none; transform:translateY(-1px); }
        .weekly-badge { display: inline-block; background: #e3f2fd; color: #1565c0; border-radius: 12px; padding: 2px 10px; font-size: 11px; font-weight: 600; }
        .other-badge  { display: inline-block; background: #fce4ec; color: #ad1457; border-radius: 12px; padding: 2px 10px; font-size: 11px; font-weight: 600; }
        .h-tab-content .tab-pane { display: none !important; }
        .h-tab-content .tab-pane.active { display: block !important; }
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
   একাডেমিক ক্যালেন্ডার | Academic Calendar
       </strong>
       <small>
      <i class="fa fa-globe"></i> Multi-Language Support (English, বাংলা)
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

    <!-- Holiday List with Tabs: Weekly / Other -->
    <div class="holiday-list-card NoPrint wow fadeIn mt-4">
        <div class="hlist-header">
            <i class="fa fa-list-ul"></i> ছুটির তালিকা &nbsp;|&nbsp; Holiday List
            <small class="pull-right" style="font-weight:400; font-size:12px;">
                <i class="fa fa-info-circle"></i> Edit / Delete করতে বাটনে ক্লিক করুন
            </small>
        </div>

        <!-- Tabs -->
        <ul class="nav h-nav-tabs" role="tablist">
            <li class="nav-item">
                <a class="nav-link active" data-toggle="tab" href="#tab-weekly" role="tab">
                    <i class="fa fa-refresh"></i> সাপ্তাহিক ছুটি
                    <span class="badge badge-primary ml-1" id="weeklyCount"></span>
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" data-toggle="tab" href="#tab-other" role="tab">
                    <i class="fa fa-calendar-times-o"></i> অন্যান্য ছুটি
                    <span class="badge badge-danger ml-1" id="otherCount"></span>
                </a>
            </li>
        </ul>

        <div class="tab-content h-tab-content">
            <asp:UpdatePanel ID="HolidayListUpdatePanel" runat="server">
                <ContentTemplate>
                    <asp:HiddenField ID="hfEditHolidayID" runat="server" />
                    <asp:HiddenField ID="hfEditHolidayName" runat="server" />
                    <asp:HiddenField ID="hfEditHolidayDate" runat="server" />
                    <asp:Button ID="EditSaveButton" runat="server" Text="Save" Style="display:none;" OnClick="EditSaveButton_Click" />

                    <!-- Tab 1: Weekly Holiday -->
                    <div id="tab-weekly" class="tab-pane active" role="tabpanel">
                        <asp:GridView ID="WeeklyHolidayGridView" runat="server" AutoGenerateColumns="False"
                            CssClass="mGrid" DataKeyNames="HolidayID" DataSourceID="WeeklyHolidaySQL"
                            EmptyDataText="কোনো সাপ্তাহিক ছুটি সেট করা নেই।"
                            AllowPaging="True" PageSize="20"
                            OnRowDeleting="WeeklyHolidayGridView_RowDeleting">
                            <Columns>
                                <asp:TemplateField HeaderText="#" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="ছুটির নাম">
                                    <ItemTemplate>
                                        <span class="weekly-badge"><i class="fa fa-refresh"></i> <%# Eval("HolidayName") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="HolidayDate" HeaderText="তারিখ" DataFormatString="{0:dddd, d MMM yyyy}" />
                                <asp:TemplateField HeaderText="Action" ItemStyle-Width="150px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <div class="act-wrap">
                                            <button type="button" class="btn-hedit"
                                                onclick="openEditModal('<%# Eval("HolidayID") %>','<%# Server.HtmlEncode(Eval("HolidayName").ToString()) %>','<%# ((DateTime)Eval("HolidayDate")).ToString("d MMM yyyy") %>')">
                                                <i class="fa fa-pencil"></i> Edit
                                            </button>
                                            <asp:LinkButton ID="WeeklyDeleteBtn" runat="server" CssClass="btn-hdel"
                                                CommandName="Delete"
                                                OnClientClick="return confirm('এই ছুটিটি মুছে ফেলবেন?');">
                                                <i class="fa fa-trash"></i> Delete
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="WeeklyHolidaySQL" runat="server"
                            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT HolidayID, HolidayName, HolidayDate FROM Employee_Holiday WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND HolidayName = 'Weekly Holiday' ORDER BY HolidayDate"
                            DeleteCommand="DELETE FROM Employee_Holiday WHERE HolidayID = @HolidayID">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            </SelectParameters>
                            <DeleteParameters>
                                <asp:Parameter Name="HolidayID" Type="Int32" />
                            </DeleteParameters>
                        </asp:SqlDataSource>
                    </div>

                    <!-- Tab 2: Other Holidays -->
                    <div id="tab-other" class="tab-pane" role="tabpanel">
                        <asp:GridView ID="OtherHolidayGridView" runat="server" AutoGenerateColumns="False"
                            CssClass="mGrid" DataKeyNames="HolidayID" DataSourceID="OtherHolidaySQL"
                            EmptyDataText="কোনো অন্যান্য ছুটি সেট করা নেই।"
                            AllowPaging="True" PageSize="20"
                            OnRowDeleting="OtherHolidayGridView_RowDeleting">
                            <Columns>
                                <asp:TemplateField HeaderText="#" ItemStyle-Width="40px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="ছুটির নাম">
                                    <ItemTemplate>
                                        <span class="other-badge"><i class="fa fa-star"></i> <%# Eval("HolidayName") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="HolidayDate" HeaderText="তারিখ" DataFormatString="{0:dddd, d MMM yyyy}" />
                                <asp:TemplateField HeaderText="Action" ItemStyle-Width="150px" ItemStyle-HorizontalAlign="Center">
                                    <ItemTemplate>
                                        <div class="act-wrap">
                                            <button type="button" class="btn-hedit"
                                                onclick="openEditModal('<%# Eval("HolidayID") %>','<%# Server.HtmlEncode(Eval("HolidayName").ToString()) %>','<%# ((DateTime)Eval("HolidayDate")).ToString("d MMM yyyy") %>')">
                                                <i class="fa fa-pencil"></i> Edit
                                            </button>
                                            <asp:LinkButton ID="OtherDeleteBtn" runat="server" CssClass="btn-hdel"
                                                CommandName="Delete"
                                                OnClientClick="return confirm('এই ছুটিটি মুছে ফেলবেন?');">
                                                <i class="fa fa-trash"></i> Delete
                                            </asp:LinkButton>
                                        </div>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <PagerStyle CssClass="pgr" />
                        </asp:GridView>
                        <asp:SqlDataSource ID="OtherHolidaySQL" runat="server"
                            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                            SelectCommand="SELECT HolidayID, HolidayName, HolidayDate FROM Employee_Holiday WHERE SchoolID = @SchoolID AND EducationYearID = @EducationYearID AND HolidayName <> 'Weekly Holiday' ORDER BY HolidayDate"
                            DeleteCommand="DELETE FROM Employee_Holiday WHERE HolidayID = @HolidayID">
                            <SelectParameters>
                                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                            </SelectParameters>
                            <DeleteParameters>
                                <asp:Parameter Name="HolidayID" Type="Int32" />
                            </DeleteParameters>
                        </asp:SqlDataSource>
                    </div>

                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <!-- Edit Holiday Modal -->
    <div class="modal fade" id="editHolidayModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content" style="border-radius:10px; overflow:hidden;">
                <div class="modal-header" style="background:linear-gradient(135deg,#667eea,#764ba2);color:white; border:none;">
                    <h5 class="modal-title"><i class="fa fa-pencil-square-o"></i> ছুটি সম্পাদনা করুন</h5>
                    <button type="button" class="close text-white" data-dismiss="modal"><span>&times;</span></button>
                </div>
                <div class="modal-body" style="padding: 24px;">
                    <div class="form-group">
                        <label style="font-weight:600; color:#444;"><i class="fa fa-tag"></i> ছুটির নাম</label>
                        <input type="text" id="editHolidayNameInput" class="form-control" placeholder="ছুটির নাম লিখুন" style="border-radius:6px;" />
                    </div>
                    <div class="form-group">
                        <label style="font-weight:600; color:#444;"><i class="fa fa-calendar"></i> তারিখ</label>
                        <input type="text" id="editHolidayDateInput" class="form-control EditDatetime" placeholder="তারিখ নির্বাচন করুন" autocomplete="off" style="border-radius:6px;" />
                    </div>
                </div>
                <div class="modal-footer" style="border:none; padding: 12px 24px 20px;">
                    <button type="button" class="btn btn-primary px-4" onclick="saveEditHoliday()" style="border-radius:20px;">
                        <i class="fa fa-save"></i> সংরক্ষণ করুন
                    </button>
                    <button type="button" class="btn btn-light px-4" data-dismiss="modal" style="border-radius:20px;">বাতিল</button>
                </div>
            </div>
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

    <script>
        function openEditModal(id, name, date) {
            document.getElementById('<%=hfEditHolidayID.ClientID%>').value = id;
            document.getElementById('editHolidayNameInput').value = name;
            document.getElementById('editHolidayDateInput').value = date;
            $('#editHolidayModal').modal('show');
        }
        function saveEditHoliday() {
            var name = $.trim($('#editHolidayNameInput').val());
            var date = $.trim($('#editHolidayDateInput').val());
            if (!name || !date) { alert('ছুটির নাম ও তারিখ আবশ্যক।'); return; }
            document.getElementById('<%=hfEditHolidayName.ClientID%>').value = name;
            document.getElementById('<%=hfEditHolidayDate.ClientID%>').value = date;
            $('#editHolidayModal').modal('hide');
            __doPostBack('<%=EditSaveButton.UniqueID%>', '');
        }
        $(function () {
            $('.EditDatetime').datepicker({ format: 'dd M yyyy', todayHighlight: true, autoclose: true });
            // Set badge counts
            var wRows = $('[id*=WeeklyHolidayGridView] tbody tr').length;
            var oRows = $('[id*=OtherHolidayGridView] tbody tr').length;
            if (wRows > 0) $('#weeklyCount').text(wRows);
            if (oRows > 0) $('#otherCount').text(oRows);

            // Manual tab switching to avoid Bootstrap version issues
            $('.h-nav-tabs .nav-link').on('click', function (e) {
                e.preventDefault();
                $('.h-nav-tabs .nav-link').removeClass('active');
                $(this).addClass('active');
                var target = $(this).attr('href');
                $('.h-tab-content .tab-pane').removeClass('active');
                $(target).addClass('active');
            });
        });
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            $('.EditDatetime').datepicker({ format: 'dd M yyyy', todayHighlight: true, autoclose: true });
            var wRows = $('[id*=WeeklyHolidayGridView] tbody tr').length;
            var oRows = $('[id*=OtherHolidayGridView] tbody tr').length;
            if (wRows > 0) $('#weeklyCount').text(wRows); else $('#weeklyCount').text('');
            if (oRows > 0) $('#otherCount').text(oRows); else $('#otherCount').text('');
        });
    </script>
</asp:Content>
