<%@ Page Title="Balance Submission to Authority" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="SubmitBalance.aspx.cs" Inherits="EDUCATION.COM.Accounts.Reports.SubmitBalance0" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8" />
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link href="CSS/SubmitBalance.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="Contain">
  <h3>Submit Balance to Authority</h3>

   <asp:UpdatePanel ID="UpdatePanel1" runat="server">
     <ContentTemplate>
 <div class="form-inline NoPrint">
         <div class="form-group">
     <asp:DropDownList ID="UserDropDown" runat="server" CssClass="form-control" 
       DataSourceID="UsersSQL" DataTextField="UserName" DataValueField="RegistrationID"
       AppendDataBoundItems="true" AutoPostBack="True" OnSelectedIndexChanged="UserDropDown_SelectedIndexChanged">
     <asp:ListItem Text="[ All Users ]" Value="0" Selected="True"></asp:ListItem>
     </asp:DropDownList>
      <asp:SqlDataSource ID="UsersSQL" runat="server" 
    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
  SelectCommand="SELECT Registration.RegistrationID, 
      ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + ' (' + Registration.UserName + ')' AS UserName,
 Registration.Category
     FROM Registration 
  LEFT OUTER JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
  WHERE Registration.SchoolID = @SchoolID
     AND Registration.Category IN ('Admin', 'Sub-Admin')
  ORDER BY Registration.Category, Admin.FirstName">
        <SelectParameters>
<asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
  </SelectParameters>
      </asp:SqlDataSource>
   </div>

  <div class="form-group">
   <asp:TextBox ID="FromDateTextBox" placeholder="From Date" runat="server" 
     autocomplete="off" CssClass="form-control Datetime"></asp:TextBox>
    </div>

     <div class="form-group">
 <asp:TextBox ID="ToDateTextBox" placeholder="To Date" runat="server" 
     autocomplete="off" CssClass="form-control Datetime"></asp:TextBox>
   </div>

   <div class="form-group">
   <asp:TextBox ID="ReceiverNameTextBox" placeholder="Receiver Name" runat="server" 
     autocomplete="off" CssClass="form-control"></asp:TextBox>
    </div>

<div class="form-group">
       <asp:TextBox ID="ReceiverMobileTextBox" placeholder="Receiver Mobile" runat="server" 
       autocomplete="off" CssClass="form-control" onkeypress="return isNumberKey(event)" MaxLength="11"></asp:TextBox>
    </div>

   <div class="form-group">
       <asp:Button ID="FindButton" runat="server" CssClass="btn btn-blue-grey" Text="Search" OnClick="FindButton_Click" />
    </div>

  <div class="form-group pull-right">
  <button type="button" class="btn btn-deep-orange" data-toggle="modal" data-target="#SubmissionModal">
 <i class="fa fa-paper-plane"></i> Submit Balance
  </button>
  </div>
 <div class="clearfix"></div>
     </div>

  <!-- Summary Section -->
       <div class="row summary-boxes">
 <div class="col-md-4">
            <div class="summary-box">
          <h5>Current Balance</h5>
    <div class="amount" style="color: #FF9800;">
   <asp:Label ID="CurrentBalanceLabel" runat="server" Text="0"></asp:Label> TK
        </div>
    </div>
    </div>
   <div class="col-md-4">
    <div class="summary-box">
  <h5>Total Submitted</h5>
     <div class="amount" style="color: #9C27B0;">
   <asp:Label ID="TotalSubmissionLabel" runat="server" Text="0"></asp:Label> TK
     </div>
     </div>
     </div>
  <div class="col-md-4">
  <div class="summary-box">
   <h5>Total Transactions</h5>
  <div class="amount" style="color: #4CAF50;">
          <asp:Label ID="TotalTransactionsLabel" runat="server" Text="0"></asp:Label>
     </div>
          </div>
     </div>
    </div>

 <div class="table-responsive">
  <asp:GridView ID="SubmissionGridView" runat="server" 
      AutoGenerateColumns="False" DataSourceID="SubmissionSQL"
       AlternatingRowStyle-CssClass="alt" PagerStyle-CssClass="pgr" 
    DataKeyNames="SubmissionID" CssClass="mGrid" 
      AllowPaging="True" PageSize="50" AllowSorting="True">
     <AlternatingRowStyle CssClass="alt" />
    <RowStyle CssClass="RowStyle" />
    <PagerStyle CssClass="pgr" />
 <Columns>
    <asp:TemplateField HeaderText="SN">
     <ItemTemplate>
    <%# Container.DataItemIndex + 1 %>
  </ItemTemplate>
    </asp:TemplateField>
     <asp:BoundField DataField="SubmissionDate" HeaderText="Date" 
    DataFormatString="{0:dd MMM yyyy}" SortExpression="SubmissionDate" 
 HtmlEncode="false" />
    <asp:BoundField DataField="UserName" HeaderText="User" 
     SortExpression="UserName" />
  <asp:BoundField DataField="SubmissionAmount" HeaderText="Submission Amount (TK)" 
      DataFormatString="{0:N0}" SortExpression="SubmissionAmount" 
     ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
    <asp:BoundField DataField="ReceivedBy" HeaderText="Received By" 
     SortExpression="ReceivedBy" />
  <asp:BoundField DataField="ReceiverPhone" HeaderText="Receiver Phone" 
     SortExpression="ReceiverPhone" />
  <asp:BoundField DataField="PaymentMethod" HeaderText="Payment Method" 
 SortExpression="PaymentMethod" />
   <asp:BoundField DataField="Remarks" HeaderText="Remarks" 
       SortExpression="Remarks" />
 <asp:BoundField DataField="SubmissionDateTime" HeaderText="Submission Date Time" 
  DataFormatString="{0:dd MMM yyyy hh:mm tt}" SortExpression="SubmissionDateTime" 
      ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" />
  </Columns>
  <EmptyDataTemplate>
 <div class="alert alert-info">
    No submissions found for the selected period.
   </div>
  </EmptyDataTemplate>
    </asp:GridView>

    <asp:SqlDataSource ID="SubmissionSQL" runat="server" 
      ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
    SelectCommand="SELECT 
 UBS.SubmissionID,
    UBS.SubmissionDate AS SubmissionDate,
  ISNULL(Admin.FirstName, '') + ' ' + ISNULL(Admin.LastName, '') + ' (' + Registration.UserName + ')' AS UserName,
 UBS.SubmissionAmount, 
    UBS.ReceivedBy, 
        UBS.ReceiverPhone,
   UBS.PaymentMethod, 
UBS.Remarks,
    UBS.CreatedDate AS SubmissionDateTime
 FROM User_Balance_Submission UBS
  INNER JOIN Registration ON UBS.RegistrationID = Registration.RegistrationID
    LEFT OUTER JOIN Admin ON Registration.RegistrationID = Admin.RegistrationID
 WHERE UBS.SchoolID = @SchoolID
  AND (@RegistrationID = 0 OR UBS.RegistrationID = @RegistrationID)
AND (CAST(UBS.SubmissionDate AS DATE) >= CASE WHEN NULLIF(@FromDate, '') IS NULL THEN '1-1-1000' ELSE CONVERT(DATE, @FromDate, 103) END)
  AND (CAST(UBS.SubmissionDate AS DATE) <= CASE WHEN NULLIF(@ToDate, '') IS NULL THEN '1-1-3000' ELSE CONVERT(DATE, @ToDate, 103) END)
  AND (NULLIF(@ReceiverName, '') IS NULL OR UBS.ReceivedBy LIKE '%' + @ReceiverName + '%')
  AND (NULLIF(@ReceiverMobile, '') IS NULL OR UBS.ReceiverPhone LIKE '%' + @ReceiverMobile + '%')
       ORDER BY UBS.CreatedDate DESC"
  CancelSelectOnNullParameter="False">
    <SelectParameters>
   <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
     <asp:ControlParameter Name="RegistrationID" ControlID="UserDropDown" 
 PropertyName="SelectedValue" Type="Int32" />
       <asp:ControlParameter ControlID="FromDateTextBox" Name="FromDate" PropertyName="Text" Type="String" />
 <asp:ControlParameter ControlID="ToDateTextBox" Name="ToDate" PropertyName="Text" Type="String" />
   <asp:ControlParameter ControlID="ReceiverNameTextBox" Name="ReceiverName" PropertyName="Text" Type="String" />
   <asp:ControlParameter ControlID="ReceiverMobileTextBox" Name="ReceiverMobile" PropertyName="Text" Type="String" />
   </SelectParameters>
  </asp:SqlDataSource>
       </div>
</ContentTemplate>
    </asp:UpdatePanel>
    </div>

    <!-- Submission Modal -->
    <div class="modal fade" id="SubmissionModal" tabindex="-1" role="dialog" aria-hidden="true">
 <div class="modal-dialog modal-dialog-centered" role="document">
  <div class="modal-content">
      <div class="modal-header">
    <div class="title">
             <i class="fa fa-paper-plane"></i> Submit Balance to Authority
  </div>
     <button type="button" class="close" data-dismiss="modal" aria-label="Close">
 <span aria-hidden="true">&times;</span>
    </button>
  </div>
  <div class="modal-body">
<asp:UpdatePanel ID="UpdatePanel2" runat="server">
      <ContentTemplate>
  <div class="alert alert-warning">
     <strong><i class="fa fa-wallet"></i> Current Balance:</strong> 
   <span>
  <asp:Label ID="ModalBalanceLabel" runat="server" Text="0"></asp:Label> TK
  </span>
      </div>

    <div class="form-group">
   <label><i class="fa fa-mobile"></i> Receiver Phone Number *</label>
         <asp:TextBox ID="ReceiverPhoneTextBox" runat="server" CssClass="form-control" 
    placeholder="01XXXXXXXXX" onkeypress="return isNumberKey(event)" 
 autocomplete="off" MaxLength="11"></asp:TextBox>
       <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" 
   ControlToValidate="ReceiverPhoneTextBox" 
    ErrorMessage="Receiver phone number is required" 
    ForeColor="Red" Display="Dynamic" ValidationGroup="SUB" CssClass="text-danger"></asp:RequiredFieldValidator>
   <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server"
     ControlToValidate="ReceiverPhoneTextBox"
      ErrorMessage="Please enter valid 11 digit phone number"
    ForeColor="Red" Display="Dynamic" ValidationGroup="SUB"
    ValidationExpression="^01[0-9]{9}$" CssClass="text-danger"></asp:RegularExpressionValidator>
     <asp:Button ID="SendOTPButton" runat="server" CssClass="btn btn-info btn-sm mt-2" 
 Text="📱 Send OTP" OnClick="SendOTPButton_Click" ValidationGroup="SENDOTP" />
         </div>

            <div class="form-group" id="otpSection" runat="server" visible="false">
      <label><i class="fa fa-key"></i> Enter OTP *</label>
     <asp:TextBox ID="OTPTextBox" runat="server" CssClass="form-control" 
            placeholder="Enter 6-digit OTP" onkeypress="return isNumberKey(event)" 
   autocomplete="off" MaxLength="6"></asp:TextBox>
  <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" 
 ControlToValidate="OTPTextBox" 
  ErrorMessage="OTP is required" 
       ForeColor="Red" Display="Dynamic" ValidationGroup="SUB" CssClass="text-danger"></asp:RequiredFieldValidator>
 <div class="mt-2">
   <small class="text-muted"><i class="fa fa-info-circle"></i> OTP sent to receiver's phone. Valid for 5 minutes.</small>
    <asp:Button ID="ResendOTPButton" runat="server" CssClass="btn btn-link btn-sm p-0 ml-3" 
          Text="🔄 Resend OTP" OnClick="ResendOTPButton_Click" />
    <span id="resendTimer" class="ml-2" style="display:none;"></span>
        </div>
 </div>

    <div class="form-group">
   <label><i class="fa fa-money"></i> Submission Amount (TK) *</label>
         <asp:TextBox ID="SubmissionAmountTextBox" runat="server" CssClass="form-control" 
    placeholder="Enter amount" onkeypress="return isNumberKey(event)" 
   autocomplete="off"></asp:TextBox>
<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
ControlToValidate="SubmissionAmountTextBox" 
ErrorMessage="Submission amount is required" 
         ForeColor="Red" Display="Dynamic" ValidationGroup="SUB" CssClass="text-danger"></asp:RequiredFieldValidator>
         </div>

     <div class="form-group">
      <label><i class="fa fa-calendar"></i> Submission Date *</label>
       <asp:TextBox ID="SubmissionDateTextBox" runat="server" CssClass="form-control Datetime" 
 placeholder="Select date" autocomplete="off"></asp:TextBox>
      <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" 
   ControlToValidate="SubmissionDateTextBox" 
  ErrorMessage="Date is required" 
     ForeColor="Red" Display="Dynamic" ValidationGroup="SUB" CssClass="text-danger"></asp:RequiredFieldValidator>
 </div>

      <div class="form-group">
       <label><i class="fa fa-user"></i> Received By</label>
  <asp:TextBox ID="ReceivedByTextBox" runat="server" CssClass="form-control" 
       placeholder="Name of receiver"></asp:TextBox>
</div>

<div class="form-group">
    <label><i class="fa fa-credit-card"></i> Payment Method</label>
  <asp:DropDownList ID="PaymentMethodDropDown" runat="server" CssClass="form-control">
 <asp:ListItem Text="💵 Cash" Value="Cash" Selected="True"></asp:ListItem>
         <asp:ListItem Text="🏦 Bank Transfer" Value="Bank Transfer"></asp:ListItem>
  <asp:ListItem Text="📱 Mobile Banking" Value="Mobile Banking"></asp:ListItem>
      <asp:ListItem Text="📝 Cheque" Value="Cheque"></asp:ListItem>
 </asp:DropDownList>
</div>

   <div class="form-group">
     <label><i class="fa fa-comment"></i> Remarks</label>
       <asp:TextBox ID="RemarksTextBox" runat="server" CssClass="form-control" 
   TextMode="MultiLine" Rows="3" placeholder="Any additional comments"></asp:TextBox>
      </div>

    <div class="text-center mt-4">
      <asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary btn-lg" 
     Text="✓ Submit Balance" OnClick="SubmitButton_Click" ValidationGroup="SUB" />
   </div>
        
            <div class="text-center mt-3">
     <label id="SuccessMsg" style="display:none;"></label>
  <label id="ErrorMsg" runat="server" style="display:none;"></label>
         </div>
  </ContentTemplate>
    </asp:UpdatePanel>
      </div>
</div>
        </div>
        </div>

    <asp:UpdateProgress ID="UpdateProgress" runat="server">
    <ProgressTemplate>
  <div id="progress_BG"></div>
        <div id="progress">
  <img src="../../CSS/loading.gif" alt="Loading..." />
       <br />
     <b>Loading...</b>
   </div>
  </ProgressTemplate>
    </asp:UpdateProgress>

    <script>
    $(function () {
  $('.Datetime').datepicker({
   format: 'dd/mm/yyyy',
    todayBtn: "linked",
        todayHighlight: true,
  autoclose: true
   });
 
   // Get date in label for display
  var from = $("[id*=FromDateTextBox]").val();
   var To = $("[id*=ToDateTextBox]").val();
  
if (from && To) {
       var dateLabel = "(" + from + " To " + To + ")";
    $("h3").append(' <small class="text-muted">' + dateLabel + '</small>');
  }
   });

  Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
$('.Datetime').datepicker({
     format: 'dd/mm/yyyy',
    todayBtn: "linked",
      todayHighlight: true,
        autoclose: true
 });
        });

    function Success() {
   var e = $('#SuccessMsg');
  e.text("Successfully submitted!");
 e.fadeIn();
   e.queue(function () { setTimeout(function () { e.dequeue(); }, 3000); });
    e.fadeOut('slow');
   
 // Close modal and refresh
  setTimeout(function() {
$('#SubmissionModal').modal('hide');
        location.reload();
  }, 2000);
        }

     // Disable submit button after clicking
$("form").submit(function () {
  $("[id$=SubmitButton]").attr("disabled", true);
   setTimeout(function () {
    $("[id$=SubmitButton]").prop('disabled', false);
    }, 3000);
   return true;
        });

  // Validation for Send OTP button
        $("[id$=SendOTPButton]").click(function (e) {
 var phoneNumber = $("[id$=ReceiverPhoneTextBox]").val().trim();
      
      if (phoneNumber === "") {
      alert("Please enter receiver phone number");
   e.preventDefault();
       return false;
            }
        
if (!/^01[0-9]{9}$/.test(phoneNumber)) {
        alert("Please enter valid 11 digit phone number starting with 01");
   e.preventDefault();
    return false;
        }
  });

    // Resend OTP Timer (60 seconds cooldown)
var resendCountdown = 60;
    var resendTimerInterval;

    function startResendTimer() {
        var resendBtn = $("[id$=ResendOTPButton]");
        var timerSpan = $("#resendTimer");
        
        resendBtn.prop("disabled", true);
        resendBtn.addClass("disabled");
        timerSpan.show();
  
        resendCountdown = 60;
        
   resendTimerInterval = setInterval(function() {
 resendCountdown--;
 timerSpan.text("(" + resendCountdown + "s)");
            
            if (resendCountdown <= 0) {
       clearInterval(resendTimerInterval);
    resendBtn.prop("disabled", false);
      resendBtn.removeClass("disabled");
                timerSpan.hide();
      }
        }, 1000);
    }

    // Start timer when OTP is sent (called from code-behind)
    function otpSent() {
        startResendTimer();
    }

    // Handle Resend OTP button click
    $("[id$=ResendOTPButton]").click(function(e) {
        var phoneNumber = $("[id$=ReceiverPhoneTextBox]").val().trim();
        
        if (phoneNumber === "") {
            alert("Please enter receiver phone number");
            e.preventDefault();
       return false;
        }
        
    if (!/^01[0-9]{9}$/.test(phoneNumber)) {
    alert("Please enter valid 11 digit phone number starting with 01");
            e.preventDefault();
         return false;
        }
    });

  function isNumberKey(a) { a = a.which ? a.which : event.keyCode; return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0 };
    </script>
</asp:Content>
