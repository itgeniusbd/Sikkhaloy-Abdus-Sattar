<%@ Page Title="Accounts by user" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="UserAccount.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.UserAccount" %>
<%@ OutputCache Duration="300" VaryByParam="RegID;From_Date;To_Date" Location="ServerAndClient" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
 <link href="CSS/Accounts_By_User.css?v=9" rel="stylesheet" />
 
 <style type="text/css">
 @media (min-width: 768px) {
 body #sidedrawer,
 body.hide-sidedrawer #sidedrawer {
 transform: translate(250px) !important;
 left: -250px !important;
 visibility: visible !important;
 display: block !important;
 }
 
 body #header,
 body #content-wrapper,
 body #footer,
 body.hide-sidedrawer #header,
 body.hide-sidedrawer #content-wrapper,
 body.hide-sidedrawer #footer {
 margin-left: 250px !important;
 }
 }
 </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

 <div class="d-flex justify-content-between align-items-center mb-3">
 <h3 class="mb-0">Accounts details by user
 <small class="Date"></small>
 </h3>
 <div class="NoPrint">
 <a href="SubmitBalance.aspx" class="btn btn-success">
 <i class="fa fa-plus"></i> ??? ???
 </a>
 </div>
 </div>
 
 <div class="form-inline NoPrint">
 <div class="form-group">
 <asp:TextBox ID="From_Date_TextBox" CssClass="form-control datepicker" placeholder="From Date" autocomplete="off" runat="server"></asp:TextBox>
 </div>
 <div class="form-group">
 <asp:TextBox ID="To_Date_TextBox" CssClass="form-control datepicker" placeholder="To Date" autocomplete="off" runat="server"></asp:TextBox>
 <i id="PickDate" class="glyphicon glyphicon-calendar fa fa-calendar" style="cursor:pointer;"></i>
 </div>
 <div class="form-group">
 <asp:Button ID="Find_Button" CssClass="btn btn-primary" runat="server" Text="SUBMIT" />
 </div>
 <div class="form-group pull-right Print">
 <a title="Print This Page" onclick="window.print();"><i class="fa fa-print" aria-hidden="true"></i></a>
 </div>
 </div>

 <!-- Cards section - same as before -->
 <asp:FormView ID="IncomeFormView" runat="server" DataSourceID="UserInExSQL" Width="100%">
 <ItemTemplate>
 <div class="row">
 <div class="col-md-2">
 <div class="user-grid user-bg">
 <div class="user-imge">
 <img alt="No Image" src="/Handeler/Admin_Photo.ashx?Img=<%#Eval("AdminID") %>" class="img-circle img-responsive" />
 </div>
 <div class="headline"><%#Eval("Name") %></div>
 <div class="value"><%#Eval("Designation") %></div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid income-bg">
 <i class="fa fa-money"></i>
 <div class="headline">INCOME</div>
 <div class="value"><%#Eval("Income","{0:N0}") %> TK</div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid expense-bg">
 <i class="fa fa-money"></i>
 <div class="headline">EXPENSE</div>
 <div class="value"><%#Eval("Expense","{0:N0}") %> TK</div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid balance-bg">
 <i class="fa fa-calculator"></i>
 <div class="headline">BALANCE</div>
 <div class="value"><%#Eval("Balance","{0:N0}") %> TK</div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid submitted-bg">
 <i class="fa fa-arrow-up"></i>
 <div class="headline">SUBMITTED</div>
 <div class="value"><%#Eval("Submitted","{0:N0}") %> TK</div>
 </div>
 </div>

 <div class="col-md-2">
 <div class="user-grid remaining-bg">
 <i class="fa fa-wallet"></i>
 <div class="headline">REMAINING</div>
 <div class="value"><%#Eval("RemainingBalance","{0:N0}") %> TK</div>
 </div>
 </div>
 </div>
 </ItemTemplate>
 </asp:FormView>

 <!-- SQL DataSource and rest of the content same as original file -->
 <!-- Copy from line 120 to line 430 from original file -->

 <script>
console.log('? UserAccount.aspx script loaded');

// Sidebar fix
(function() {
 window.preventSidebarHide = true;
 window.forceLockSidebar = true;
 
 function fixSidebar() {
 if (document.body) {
 document.body.classList.remove('hide-sidedrawer');
 document.body.classList.add('lock-sidebar');
 }
 var sidebar = document.getElementById('sidedrawer');
 if (sidebar) {
 sidebar.style.transform = 'translate(250px)';
 sidebar.style.visibility = 'visible';
 sidebar.style.display = 'block';
 }
 }
 
 fixSidebar();
 if (document.readyState === 'loading') {
 document.addEventListener('DOMContentLoaded', fixSidebar);
 } else {
 fixSidebar();
 }
 setInterval(fixSidebar, 200);
 if (typeof Sys !== 'undefined' && Sys.Application) {
 Sys.Application.add_load(fixSidebar);
 }
})();

// Main jQuery code
$(function () {
 console.log('? jQuery ready');
 
 // Individual date picker
 $('.datepicker').datepicker({
 format: 'dd/mm/yyyy',
 autoclose: true,
 todayHighlight: true
 });
 
 console.log('? Datepicker initialized');

 // Update date label
 function updateDateLabel() {
 var from = $("[id*=From_Date_TextBox]").val();
 var to = $("[id*=To_Date_TextBox]").val();
 var label = "";
 
 if (from && to) {
 if (from === to) {
 label = "(" + from + ")";
 } else {
 label = "(" + from + " To " + to + ")";
 }
 } else if (from) {
 label = "(After " + from + ")";
 } else if (to) {
 label = "(Before " + to + ")";
 }
 
 $(".Date").text(label);
 }

 updateDateLabel();

 // Calendar icon click - initialize daterangepicker
 var pickerInit = false;
 
 $('#PickDate').on('click', function(e) {
 e.preventDefault();
 console.log('?? Calendar icon clicked');
 
 if (!pickerInit) {
 console.log('?? Initializing daterangepicker...');
 
 var fromVal = $('[id*=From_Date_TextBox]').val();
 var toVal = $('[id*=To_Date_TextBox]').val();
 
 var startDt = moment();
 var endDt = moment();
 
 if (fromVal && toVal) {
 var parsedStart = moment(fromVal, 'DD/MM/YYYY', true);
 var parsedEnd = moment(toVal, 'DD/MM/YYYY', true);
 
 if (parsedStart.isValid() && parsedEnd.isValid()) {
 startDt = parsedStart;
 endDt = parsedEnd;
 console.log('?? Using existing dates:', fromVal, 'to', toVal);
 }
 }
 
 $(this).daterangepicker({
 startDate: startDt,
 endDate: endDt,
 autoApply: true,
 opens: 'left',
 locale: { format: 'DD/MM/YYYY' },
 ranges: {
 'Today': [moment(), moment()],
 'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
 'Last 7 Days': [moment().subtract(6, 'days'), moment()],
 'Last 30 Days': [moment().subtract(29, 'days'), moment()],
 'This Month': [moment().startOf('month'), moment().endOf('month')],
 'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
 'This Year': [moment().startOf('year'), moment().endOf('year')]
 }
 }, function(start, end) {
 console.log('? Date selected:', start.format('DD/MM/YYYY'), 'to', end.format('DD/MM/YYYY'));
 $('[id*=From_Date_TextBox]').val(start.format('DD/MM/YYYY'));
 $('[id*=To_Date_TextBox]').val(end.format('DD/MM/YYYY'));
 updateDateLabel();
 setTimeout(function() {
 $("[id*=Find_Button]").click();
 }, 100);
 });
 
 pickerInit = true;
 console.log('? Daterangepicker initialized');
 }
 
 $(this).data('daterangepicker').show();
 return false;
 });
 
 console.log('? All scripts loaded successfully');
});
 </script>
</asp:Content>
