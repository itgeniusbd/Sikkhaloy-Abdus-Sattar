<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Exam_Routine.aspx.cs" Inherits="EDUCATION.COM.Routines.Exam_Routine_Bangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
 <link href="CSS/Exam_Routine.css" rel="stylesheet" />
 <link href="/JS/TimePicker/mdtimepicker.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
 <asp:UpdatePanel ID="MainUpdatePanel" runat="server" UpdateMode="Conditional">
  <ContentTemplate>
<div class="exam-routine-container">
  
  <!-- Routine Name Display ONLY -->
  <div class="routine-name-display">
      <asp:Label ID="RoutineNameLabel" runat="server" Text="পরীক্ষার রুটিন"></asp:Label>
  </div>
        
<!-- Control Panel for Dynamic Columns and Rows -->
      <div class="control-panel print-hide">
<div style="display: flex; justify-content: space-between; flex-wrap: wrap; gap: 10px;">
     <div>
   <span class="control-label">শ্রেণী কলাম:</span>
  <asp:Button ID="AddClassColumnButton" runat="server" Text="➕ যোগ" 
  CssClass="btn-add-column" OnClick="AddClassColumnButton_Click" />
        <asp:Button ID="RemoveClassColumnButton" runat="server" Text="➖ মুছে ফেলুন" 
  CssClass="btn-remove-column" OnClick="RemoveClassColumnButton_Click" />
   <asp:Label ID="ClassColumnCountLabel" runat="server" Text="(1)" 
 style="margin-left: 5px;"></asp:Label>
     </div>
  
    <div>
    <span class="control-label">সারি:</span>
       <asp:Button ID="AddRowButton" runat="server" Text="➕ যোগ" 
 CssClass="btn-add-row" OnClick="AddRowButton_Click" />
     <asp:Button ID="RemoveRowButton" runat="server" Text="➖ মুছে ফেলুন" 
    CssClass="btn-remove-row" OnClick="RemoveRowButton_Click" />
  <asp:Label ID="RowCountLabel" runat="server" Text="(1)" 
   style="margin-left: 5px;"></asp:Label>
</div>
  
   <div>
 <asp:Button ID="RefreshSubjectsButton" runat="server" Text="🔄 রিফ্রেশ" 
    CssClass="btn-reload" OnClick="RefreshSubjectsButton_Click" />
 </div>
        </div>
 </div>
  
 <!-- Buttons -->
 <div class="form-inline print-hide" style="gap:10px; align-items:center; flex-wrap:wrap;">
 <asp:Button ID="LoadRoutineButton" runat="server" Text="রুটিন লোড করুন" 
 CssClass="btn btn-info" OnClick="LoadRoutineButton_Click" />
 <asp:Button ID="DeleteRoutineButton" runat="server" Text="ডিলেট করুন" 
 CssClass="btn btn-danger" OnClick="DeleteRoutineButton_Click" 
 OnClientClick="return confirm('আপনি কি নিশ্চিত যে এই রুটিনটি মুছে ফেলতে চান?');" />
 <asp:Button ID="SaveButton" runat="server" Text="সংরক্ষণ করুন" 
 CssClass="btn btn-success" OnClick="SaveRoutineButton_Click" />
 <asp:Button ID="PrintButton" runat="server" Text="প্রিন্ট করুন" 
 CssClass="btn btn-primary" OnClientClick="printRoutine(); return false;" />

 <div style="display:flex; gap:8px; align-items:center;">
 <span class="control-label">রুটিন নাম:</span>
 <asp:TextBox ID="RoutineNameTextBox" runat="server" CssClass="form-control" Width="220" placeholder="রুটিনের নাম লিখুন"></asp:TextBox>
 </div>

 <div style="display:flex; gap:8px; align-items:center;">
 <span class="control-label">রুটিন লিস্ট:</span>
 <asp:DropDownList ID="RoutineListDropDown" runat="server" CssClass="form-control" 
     DataTextField="DisplayText" DataValueField="RoutineID" 
     AppendDataBoundItems="true">
     <asp:ListItem Value="0">[ নির্বাচন করুন ]</asp:ListItem>
 </asp:DropDownList>
 <!-- Debug label for testing -->
 <asp:Label ID="DebugLabel" runat="server" CssClass="text-muted ml-2"></asp:Label>
 </div>
 </div>
    
     <!-- Hidden fields to track counts -->
   <asp:HiddenField ID="ClassColumnCountHF" runat="server" Value="1" />
   <asp:HiddenField ID="RowCountHF" runat="server" Value="1" />
     <asp:HiddenField ID="CellDataJsonHF" runat="server" />
     <asp:HiddenField ID="LoadedRoutineIdHF" runat="server" Value="0" />
  
      <!-- Exam Routine Table -->
         <table class="routine-table" id="routineTable">
   <thead>
   <tr>
    <th>তারিখ</th>
    <th>বার</th>
    <th>সময়</th>
     <asp:Literal ID="ClassHeaderLiteral" runat="server"></asp:Literal>
    </tr>
 </thead>
   <tbody>
   <asp:Repeater ID="RoutineRepeater" runat="server" OnItemDataBound="RoutineRepeater_ItemDataBound">
    <ItemTemplate>
        <tr>
 <td class="date-cell">
      <asp:TextBox ID="ExamDateTextBox" runat="server" CssClass="form-control-routine datepicker-input" name='<%# "ExamDate_" + Container.ItemIndex %>' Text='<%# Eval("ExamDate") %>' placeholder="dd/mm/yyyy"></asp:TextBox>
</td>
 <td class="day-cell">
     <asp:TextBox ID="DayNameTextBox" runat="server" CssClass="form-control-routine day-input" name='<%# "DayName_" + Container.ItemIndex %>' Text='<%# Eval("DayName") %>' placeholder="বার" ReadOnly="true"></asp:TextBox>
  </td>
   <td class="time-cell">
      <div style="display: flex; align-items: center; gap: 5px; margin-bottom: 3px;">
<asp:TextBox ID="StartTimeTextBox" runat="server" 
     CssClass="form-control-routine time-input start-time-input" 
Text="10:00 AM"
   placeholder="শুরু"
    style="width: 70px; text-align: center;" />
 <span style="font-weight: bold;">-</span>
   <asp:TextBox ID="EndTimeTextBox" runat="server" 
   CssClass="form-control-routine time-input end-time-input"
       Text="01:00 PM"
 placeholder="শেষ"
   style="width: 70px; text-align: center;" />
    </div>
        <!-- Duration Display (Auto-calculated) -->
     <div class="duration-display" style="color: #d32f2f; font-weight: bold; font-size: 11px; text-align: center;">
    <span id='<%# "DurationLabel_" + Container.ItemIndex %>' data-row='<%# Container.ItemIndex %>'>৩ ঘন্টা</span>
   </div>
 </td>
    <asp:Literal ID="ClassColumnsLiteral" runat="server"></asp:Literal>
    </tr>
    </ItemTemplate>
 </asp:Repeater>
 </tbody>
</table>
     
   <!-- SqlDataSource for Classes -->
    <asp:SqlDataSource ID="ClassSQL" runat="server" 
       ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
    SelectCommand="SELECT ClassID, Class FROM CreateClass WHERE SchoolID = @SchoolID ORDER BY SN">
 <SelectParameters>
   <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
     </SelectParameters>
     </asp:SqlDataSource>
   
   <!-- SqlDataSource for Routine List -->
        <asp:SqlDataSource ID="RoutineListSQL" runat="server" 
    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" 
     SelectCommand="SELECT RoutineID, RoutineName + ' (' + CONVERT(VARCHAR, CreatedDate, 106) + ')' AS DisplayText, CreatedDate FROM Exam_Routine_SavedData WHERE SchoolID = @SchoolID AND IsActive = 1 ORDER BY CreatedDate DESC">
 <SelectParameters>
        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
      </SelectParameters>
    </asp:SqlDataSource>
 </div>
 </ContentTemplate>
 </asp:UpdatePanel>
    
    <!-- Load jQuery UI after jQuery from master page -->
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" />
    <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.min.js"></script>
    <script src="/JS/TimePicker/mdtimepicker.js?v=5"></script>
    
    <script type="text/javascript">
     // Ensure jQuery and jQuery UI are loaded
    if (typeof jQuery === 'undefined') {
   document.write('<script src="https://code.jquery.com/jquery-3.3.1.min.js"><\/script>');
   }
 
    // Wait for jQuery UI to load
  (function checkjQueryUI() {
 if (typeof jQuery === 'undefined' || typeof jQuery.ui === 'undefined') {
    setTimeout(checkjQueryUI, 100);
  return;
   }

   // Initialize everything when page is ready
$(document).ready(function() {
  
  // **NEW: Fix date format on page load**
        FixAllDateFormats();

        // Initialize date pickers first
      InitializeDatepickers();
   
     // Initialize time inputs
  InitializeTimeInputs();
 
   // Force refresh on UpdatePanel postback
if (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined') {
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function() {
   setTimeout(function() {
       // **CRITICAL: Fix date formats after postback**
     FixAllDateFormats();
   InitializeDatepickers();
   InitializeTimeInputs();
     UpdatePrintDataAttributes();
     }, 300);
       });
  }

    // **NEW: Update print data attributes on page load**
    UpdatePrintDataAttributes();

    // **NEW: Update data attributes when any input changes**
$(document).on('change', 'input, select', function() {
        UpdatePrintDataAttributes();
    });
});
      })();

        // জন্ম তারিখ ইনপুট Inicialization
  var bengaliDays = ["রবিবার", "সোমবার", "মঙ্গলবার", "বুধবার", "বৃহস্পতিবার", "শুক্রবার", "শনিবার"];

  // **NEW: Function to fix all date formats to dd/MM/yyyy**
  function FixAllDateFormats() {
    
    $("input.datepicker-input, input[id*='ExamDateTextBox']").each(function() {
var $input = $(this);
 var currentValue = $input.val().trim();
     
     if (!currentValue) {
 return;
      }
  
  // **CRITICAL: If already in dd/MM/yyyy format (2 digits/2 digits/4 digits), skip**
var ddMMyyyyPattern = /^(\d{2})\/(\d{2})\/(\d{4})$/;
        if (ddMMyyyyPattern.test(currentValue)) {
     var parts = currentValue.split('/');
    var day = parseInt(parts[0], 10);
    var month = parseInt(parts[1], 10);
       
    // Validate it's a valid date in dd/MM/yyyy format
 if (day >= 1 && day <= 31 && month >= 1 && month <= 12) {
       return;
     }
  }
  
   // Try to parse the date from various formats
   var dateObj = null;
    
     // Pattern 1: mm/dd/yyyy or dd/mm/yyyy (ambiguous)
      var slashPattern = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/;
  var match = currentValue.match(slashPattern);
            
    if (match) {
 var part1 = parseInt(match[1], 10);
var part2 = parseInt(match[2], 10);
 var year = parseInt(match[3], 10);
   
   if (part1 > 12) {
dateObj = new Date(year, part2 - 1, part1);
   } else if (part2 > 12) {
       dateObj = new Date(year, part1 - 1, part2);
   } else {
  dateObj = new Date(year, part1 - 1, part2);
     }
        }
       
   // Pattern 2: yyyy-mm-dd (ISO format)
    var isoPattern = /^(\d{4})-(\d{1,2})-(\d{1,2})$/;
   match = currentValue.match(isoPattern);
   if (match) {
   var year = parseInt(match[1], 10);
 var month = parseInt(match[2], 10);
  var day = parseInt(match[3], 10);
   dateObj = new Date(year, month - 1, day);
      }
 
       // If we successfully parsed a date, format it as dd/MM/yyyy
  if (dateObj && !isNaN(dateObj.getTime())) {
     var day = dateObj.getDate();
  var month = dateObj.getMonth() + 1;
     var year = dateObj.getFullYear();
     
      var fixedDate = padZero(day) + '/' + padZero(month) + '/' + year;
  $input.val(fixedDate);
 
  // Update the day name
   var $row = $input.closest('tr');
   var $dayTextbox = $row.find("input[id*='DayNameTextBox'], input.day-input");
 if ($dayTextbox.length > 0) {
       var dayOfWeek = dateObj.getDay();
     var bengaliDay = bengaliDays[dayOfWeek];
   $dayTextbox.val(bengaliDay);
 }
   }
    });
    }


// Helper function to pad single digits with zero
    function padZero(num) {
        return num < 10 ? '0' + num : num.toString();
    }

  function InitializeDatepickers() {
  
 // Check if jQuery UI datepicker is available
    if (typeof $.fn.datepicker === 'undefined') {
 return;
   }
  
  // Find all date inputs
    $("input.datepicker-input, input[id*='ExamDateTextBox']").each(function() {
 var $datepicker = $(this);
  
    // Skip if not in table row
if (!$datepicker.closest('tr').length) {
        return;
      }
 
 var $row = $datepicker.closest('tr');
     var $dayTextbox = $row.find("input[id*='DayNameTextBox'], input.day-input");
 
   // Destroy existing datepicker
  if ($datepicker.hasClass('hasDatepicker')) {
 $datepicker.datepicker('destroy');
  $datepicker.removeClass('hasDatepicker');
    }
        
   // Initialize jQuery UI datepicker
  $datepicker.datepicker({
   dateFormat: 'dd/mm/yy',
  changeMonth: true,
changeYear: true,
  yearRange: '2020:2030',
   showButtonPanel: false,
 closeText: 'বন্ধ করুন',
currentText: 'আজ',
  onSelect: function(dateText, inst) {
       // Update the textbox value
  $(this).val(dateText);
     
// Parse date from dd/mm/yyyy format
  var parts = dateText.split('/');
  var day = parseInt(parts[0], 10);
     var month = parseInt(parts[1], 10) - 1;
 var year = parseInt(parts[2], 10);
    
 var selectedDate = new Date(year, month, day);
     var dayOfWeek = selectedDate.getDay();
 var bengaliDay = bengaliDays[dayOfWeek];
   
    // Set day in Bangla
 if ($dayTextbox.length > 0) {
        $dayTextbox.val(bengaliDay);
 }
    },
      onClose: function(dateText) {
  if (dateText) {
    $(this).val(dateText);
      }
        }
 });
   
// Add placeholder
        $datepicker.attr('placeholder', 'dd/mm/yyyy');
      // Make it clickable
$datepicker.css('cursor', 'pointer');
  });
  }
        
        function InitializeTimeInputs() {
    
      // Initialize mdtimepicker ONLY for main time column inputs (start/end time)
      // NOT for subject cell time inputs
 $("input.start-time-input, input.end-time-input, input[id*='StartTimeTextBox'], input[id*='EndTimeTextBox']").each(function() {
    var $input = $(this);
     
  // Skip if it's a manual time input in subject cell
    if ($input.hasClass('time-manual-input')) {
     return;
  }

       // Initialize mdtimepicker
  $input.mdtimepicker({
    theme: 'green',
    timeFormat: 'hh:mm:ss.000'
        }).on('timechanged', function(e) {
   // Calculate duration when time changes
   calculateDuration($(this));
    });
 });
     
 // Initial duration calculation for all rows
     InitializeDurationCalculation();
        }
      
  // Initialize duration calculation for all time input pairs
        function InitializeDurationCalculation() {
     
// Find all rows with start/end time inputs
 $("input.start-time-input, input[id*='StartTimeTextBox']").each(function() {
 var $startInput = $(this);
       var inputId = $startInput.attr('id');
 
if (!inputId) {
 return;
 }

  // Extract row index - try multiple patterns
   var rowIndex = null;
   
        // Pattern 1: StartTimeTextBox_0
    var match1 = inputId.match(/_(\d+)$/);
   if (match1) {
     rowIndex = match1[1];
    }
        
        // Pattern 2: ContentPlaceHolderID_StartTimeTextBox_0
 if (!rowIndex) {
    var match2 = inputId.match(/StartTimeTextBox_(\d+)$/);
    if (match2) {
  rowIndex = match2[1];
  }
  }

    if (rowIndex !== null) {
    // Try to find end input with same row index
       var $endInput = $("input[id$='EndTimeTextBox_" + rowIndex + "']");
   
    if ($endInput.length > 0) {
       // Calculate initial duration if both times exist
   if ($startInput.val() && $endInput.val()) {
     calculateDurationForRow(rowIndex);
}
   }
   }
  });
   }
  
  // Calculate duration when a time input changes
   function calculateDuration($input) {
    // Find the row index from the input ID
    var inputId = $input.attr('id');
   if (!inputId) return;
   
  // Try multiple patterns
  var rowIndex = null;
 
 var match1 = inputId.match(/_(\d+)$/);
 if (match1) {
        rowIndex = match1[1];
 }
  
    if (!rowIndex) {
  var match2 = inputId.match(/TimeTextBox_(\d+)$/);
      if (match2) {
   rowIndex = match2[1];
   }
 }
    
 if (!rowIndex) return;
      
  calculateDurationForRow(rowIndex);
   }
  
  // Calculate duration for a specific row
function calculateDurationForRow(rowIndex) {
    // Find inputs using ID ends with pattern (works with master page prefixes)
 var $startInput = $("input[id$='StartTimeTextBox_" + rowIndex + "']");
var $endInput = $("input[id$='EndTimeTextBox_" + rowIndex + "']");

  var $durationLabel = $("#DurationLabel_" + rowIndex);
     
  if ($startInput.length === 0 || $endInput.length === 0) {
 return;
  }
   
var startTime = $startInput.val().trim().toUpperCase();
   var endTime = $endInput.val().trim().toUpperCase();
   
  if (!startTime || !endTime) {
      // If either time is empty, clear duration
   if ($durationLabel.length > 0) {
    $durationLabel.text('');
    }
   return;
       }
 
     // Parse times
var startMoment = parseTime(startTime);
   var endMoment = parseTime(endTime);
      
 if (!startMoment || !endMoment) {
     return;
     }

     // Calculate duration in hours
   var durationMs = endMoment - startMoment;
     
     // Handle case where end time is before start time (assume next day)
   if (durationMs < 0) {
 durationMs += 24 * 60 * 60 * 1000; // Add 24 hours
   }
   
   var durationHours = durationMs / (1000 * 60 * 60);
   
    // Format duration in Bangla
      var durationText = formatDurationBangla(durationHours);
     
    // Update duration display
  if ($durationLabel.length > 0) {
   $durationLabel.text(durationText);
   }
   }



    // **NEW: Update data attributes for printing**
    function UpdatePrintDataAttributes() {
   console.log('Updating print data attributes');
   
        // Update each row
        $('.routine-table > tbody > tr').each(function() {
            var $row = $(this);

      // Date cell
       var $dateCell = $row.find('.date-cell, td:nth-child(1)');
       var dateValue = $dateCell.find('input').val() || '';
      $dateCell.attr('data-date', dateValue);

            // Day cell
            var $dayCell = $row.find('.day-cell, td:nth-child(2)');
            var dayValue = $dayCell.find('input').val() || '';
     $dayCell.attr('data-day', dayValue);

   // Time cell - **FIXED: Proper formatting without extra hyphen**
       var $timeCell = $row.find('.time-cell, td:nth-child(3)');
      var startTime = $timeCell.find('input[id*="StartTimeTextBox"]').val() || '';
       var endTime = $timeCell.find('input[id*="EndTimeTextBox"]').val() || '';
   var duration = $timeCell.find('span[id*="DurationLabel"]').text() || '';
     var timeDisplay = '';
            
            if (startTime && endTime) {
         // **Format: "10:00 AM - 1:00 PM" on first line**
      timeDisplay = startTime + ' - ' + endTime;
      
     // **Add duration on second line without hyphen**
    if (duration) {
          timeDisplay += '\n(' + duration + ')';
          }
          }
   
       $timeCell.attr('data-time', timeDisplay);

         // Subject cells
            $row.find('.editable-cell').each(function() {
          var $cell = $(this);
      var cellContent = '';

     // Get selected subject from dropdown
       var $dropdown = $cell.find('select');
 if ($dropdown.length > 0 && $dropdown.val() && $dropdown.val() !== '0') {
    var selectedText = $dropdown.find('option:selected').text();
            if (selectedText && selectedText !== 'বিষয় নির্বাচন করুন') {
      cellContent = selectedText;
             }
   }

 // Get subject textbox value
                var $textbox = $cell.find('input.subject-textbox');
    if ($textbox.length > 0 && $textbox.val()) {
      if (cellContent) cellContent += '\n';
     cellContent += $textbox.val();
 }

    // Get time manual input
         var $timeInput = $cell.find('input.time-manual-input');
     if ($timeInput.length > 0 && $timeInput.val()) {
          if (cellContent) cellContent += '\n';
         cellContent += '(' + $timeInput.val() + ')';
            }

  $cell.attr('data-print-value', cellContent);
      });
        });

      console.log('Print data attributes updated');
    }

    // Print routine function - **SIMPLIFIED: Direct window.print()**
    function printRoutine() {
      console.log('Print function called - using direct window.print()');
 
     // **Update data attributes before printing**
        UpdatePrintDataAttributes();
        
 // **Simply call window.print() - CSS will handle everything**
      setTimeout(function() {
    window.print();
        }, 100);
 }

    // Parse time string to Date object - GLOBAL SCOPE
    function parseTime(timeStr) {
    if (!timeStr) return null;
 
      // Remove extra spaces and convert to uppercase
     timeStr = timeStr.trim().toUpperCase();
     
        // Extract hour, minute, and am/pm
        // Patterns: "10:00 AM", "10:00AM", "10 AM", "10AM", "1000AM"
        var match = timeStr.match(/(\d{1,2}):?(\d{2})?\s*(AM|PM)?/i);
        if (!match) return null;
        
 var hour = parseInt(match[1]);
     var minute = match[2] ? parseInt(match[2]) : 0;
    var period = match[3] ? match[3].toUpperCase() : '';
    
        // Convert to 24-hour format
     if (period === 'PM' && hour !== 12) {
     hour += 12;
        } else if (period === 'AM' && hour === 12) {
          hour = 0;
        }
        
   // Create a date object (date doesn't matter, only time)
        var date = new Date();
        date.setHours(hour);
   date.setMinutes(minute);
    date.setSeconds(0);
        date.setMilliseconds(0);

        return date;
    }
    
    // Format duration in Bangla - GLOBAL SCOPE
    function formatDurationBangla(hours) {
    var bengaliDigits = ['০', '১', '২', '৩', '৪', '৫', '৬', '৭', '৮', '৯'];
    
        // Round to 1 decimal place
        hours = Math.round(hours * 10) / 10;
    
   if (hours === 0) {
   return '০ ঘন্টা';
        }
      
        var wholeHours = Math.floor(hours);
        var minutes = Math.round((hours - wholeHours) * 60);
        
        var hourStr = wholeHours.toString().split('').map(function(d) {
    return bengaliDigits[parseInt(d)];
  }).join('');
        
  var minuteStr = '';
      if (minutes > 0) {
    minuteStr = minutes.toString().split('').map(function(d) {
      return bengaliDigits[parseInt(d)];
       }).join('');
        }
        
        if (wholeHours > 0 && minutes > 0) {
   return hourStr + ' ঘন্টা ' + minuteStr + ' মিনিট';
    } else if (wholeHours > 0) {
   return hourStr + ' ঘন্টা';
        } else {
        return minuteStr + ' মিনিট';
}
    }
    </script>

 <!-- Printable Area (OUTSIDE script tag) -->
    <div id="printableArea" style="display:none;"></div>

    </asp:Content>    