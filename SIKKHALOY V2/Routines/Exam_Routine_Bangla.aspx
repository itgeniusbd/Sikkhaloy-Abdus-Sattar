<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Exam_Routine_Bangla.aspx.cs" Inherits="EDUCATION.COM.Routines.Exam_Routine_Bangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Exam_Routine.css" rel="stylesheet" />
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
 <button type="button" class="btn btn-warning" onclick="debugCheck(); return false;">🐛 Debug Check</button>

 <div style="display:flex; gap:8px; align-items:center;">
 <span class="control-label">রুটিন নাম:</span>
 <asp:TextBox ID="RoutineNameTextBox" runat="server" CssClass="form-control" Width="220" placeholder="রুটিনের নাম লিখুন"></asp:TextBox>
 </div>
 <div style="display:flex; gap:8px; align-items:center;">
 <span class="control-label">রুটিন লিস্ট:</span>
 <asp:DropDownList ID="RoutineListDropDown" runat="server" CssClass="form-control" DataSourceID="RoutineListSQL" DataTextField="RoutineName" DataValueField="RoutineID" AppendDataBoundItems="true">
 <asp:ListItem Value="0">[ নির্বাচন করুন ]</asp:ListItem>
 </asp:DropDownList>
 </div>
 </div>
    
     <!-- Hidden fields to track counts -->
   <asp:HiddenField ID="ClassColumnCountHF" runat="server" Value="1" />
       <asp:HiddenField ID="RowCountHF" runat="server" Value="1" />
       <asp:HiddenField ID="CellDataJsonHF" runat="server" />
       <asp:HiddenField ID="LoadedRoutineIdHF" runat="server" Value="0" />
  
      <!-- Exam Routine Table -->
         <table class="routine-table" id="routineTable">
          <table class="routine-table">
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
                <asp:TextBox ID="ExamDateTextBox" runat="server" TextMode="Date" CssClass="form-control-routine" name='<%# "ExamDate_" + Container.ItemIndex %>' Text='<%# Eval("ExamDate") != DBNull.Value ? Convert.ToDateTime(Eval("ExamDate")).ToString("yyyy-MM-dd") : "" %>'></asp:TextBox>
            </td>
            <td class="day-cell">
                <asp:TextBox ID="DayNameTextBox" runat="server" CssClass="form-control-routine" name='<%# "DayName_" + Container.ItemIndex %>' Text='<%# Eval("DayName") %>'></asp:TextBox>
            </td>
            <td class="time-cell">
                <asp:TextBox ID="ExamTimeTextBox" runat="server" CssClass="form-control-routine" name='<%# "ExamTime_" + Container.ItemIndex %>' Text='<%# Eval("ExamTime") %>'></asp:TextBox>
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
            SelectCommand="SELECT RoutineID, RoutineName, CONVERT(VARCHAR, CreatedDate, 106) AS DisplayDate FROM Exam_Routine_SavedData WHERE SchoolID = @SchoolID AND IsActive = 1 ORDER BY CreatedDate DESC">
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
    
    <script type="text/javascript">
     // Ensure jQuery and jQuery UI are loaded
    if (typeof jQuery === 'undefined') {
     console.error('jQuery not loaded!');
   document.write('<script src="https://code.jquery.com/jquery-3.3.1.min.js"><\/script>');
   } else {
       console.log('jQuery loaded: ' + jQuery.fn.jquery);
        }
 
    // Wait for jQuery UI to load
  (function checkjQueryUI() {
 if (typeof jQuery === 'undefined' || typeof jQuery.ui === 'undefined') {
  console.log('Waiting for jQuery UI...');
    setTimeout(checkjQueryUI, 100);
  return;
   }
        console.log('jQuery UI loaded successfully');

        // বাংলা বার নাম
  var bengaliDays = ["রবিবার", "সোমবার", "মঙ্গলবার", "বুধবার", "বৃহস্পতিবার", "শুক্রবার", "শনিবার"];

  // সময়ের তালিকা
      var timeSuggestions = [
 "6:00 am", "6:30 am", "7:00 am", "7:30 am", "8:00 am", "8:30 am",
     "9:00 am", "9:30 am", "10:00 am", "10:30 am", "11:00 am", "11:30 am",
        "12:00 pm", "12:30 pm", "1:00 pm", "1:30 pm", "2:00 pm", "2:30 pm",
   "3:00 pm", "3:30 pm", "4:00 pm", "4:30 pm", "5:00 pm", "5:30 pm",
      "6:00 pm", "6:30 pm", "7:00 pm", "7:30 pm", "8:00 pm"
  ];

  function InitializeDatepickers() {
console.log('Initializing datepickers...');
      
    // Remove existing time suggestions first
         $('.time-suggestions').remove();
            
 // Check if jQuery UI datepicker is available
      if (typeof $.fn.datepicker === 'undefined') {
    console.error('jQuery UI Datepicker not available!');
    alert('তারিখ সিলেক্টর লোড হয়নি। পেজটি রিফ্রেশ করুন।');
      return;
   }
     
          $("input[id*='DateTextBox']").each(function() {
    var $datepicker = $(this);
  
    // Skip if not in table row
if (!$datepicker.closest('tr').length) {
          return;
        }
   
 var $row = $datepicker.closest('tr');
     var $dayTextbox = $row.find("input[id*='DayTextBox']");
    
       // Destroy existing datepicker
  if ($datepicker.hasClass('hasDatepicker')) {
 $datepicker.datepicker('destroy');
  $datepicker.removeClass('hasDatepicker');
    }
        
     // Initialize datepicker
  $datepicker.datepicker({
   dateFormat: 'dd/mm/yy',  // This will show as dd/MM/yyyy (yy means 4-digit year in jQuery UI)
  changeMonth: true,
    changeYear: true,
  yearRange: '2020:2030',
  onSelect: function(dateText, inst) {
    console.log('Date selected: ' + dateText);
     
   // Parse date
      var parts = dateText.split('/');
  var day = parseInt(parts[0], 10);
     var month = parseInt(parts[1], 10) - 1;
      var year = parseInt(parts[2], 10);
      
 var selectedDate = new Date(year, month, day);
          var dayOfWeek = selectedDate.getDay();
 var bengaliDay = bengaliDays[dayOfWeek];
   
    // Set day in Bangla
    $dayTextbox.val(bengaliDay);
    console.log('Day set to: ' + bengaliDay);
    }
      });
   
console.log('Datepicker initialized for: ' + $datepicker.attr('id'));
    });
    
         console.log('Total datepickers initialized: ' + $("input[id*='DateTextBox']").filter('.hasDatepicker').length);
  }
        
        function InitializeTimeInputs() {
  console.log('Initializing time inputs...');
          
     $("input[id*='TimeTextBox']").each(function() {
      var $input = $(this);
    
           // Skip if already initialized
  if ($input.data('time-initialized')) {
   return;
           }
              
    $input.data('time-initialized', true);
   
    // Remove old event handlers
        $input.off('focus keyup blur');
        
         // Add cursor pointer
     $input.css('cursor', 'pointer');
    
     // Focus event
       $input.on('focus', function() {
       var $this = $(this);
   var $dropdown = $this.siblings('.time-suggestions');
    
     if ($dropdown.length === 0) {
    $dropdown = $('<div class="time-suggestions"></div>');
       $this.after($dropdown);
            
 // Add time options
     $.each(timeSuggestions, function(i, time) {
   var $item = $('<div class="time-suggestion-item">' + time + '</div>');
    $item.on('mousedown', function(e) {
   e.preventDefault();
 $this.val($(this).text());
  $dropdown.hide();
        $this.blur();
       });
     $dropdown.append($item);
  });
      }
            
  // Position dropdown relative to input
   var inputOffset = $this.offset();
    var inputHeight = $this.outerHeight();
         $dropdown.css({
      'position': 'absolute',
    'top': inputOffset.top + inputHeight,
   'left': inputOffset.left
   });
    
     $dropdown.show();
            
    // Scroll to current value if exists
     var currentVal = $this.val().toLowerCase();
     if (currentVal) {
 var $currentItem = $dropdown.find('.time-suggestion-item').filter(function() {
 return $(this).text().toLowerCase() === currentVal;
    }).first();
    
                if ($currentItem.length > 0) {
    $dropdown.scrollTop($currentItem.position().top);
      }
       }
    });
     
       // Keyup event for filtering
     $input.on('keyup', function() {
  var $this = $(this);
    var val = $this.val().toLowerCase();
   var $dropdown = $this.siblings('.time-suggestions');
     
if ($dropdown.length > 0) {
if (val.length > 0) {
     $dropdown.find('.time-suggestion-item').each(function() {
        var text = $(this).text().toLowerCase();
   if (text.indexOf(val) !== -1) {
    $(this).show();
      } else {
  $(this).hide();
       }
    });
  } else {
           $dropdown.find('.time-suggestion-item').show();
      }
        }
       });
 
      // Blur event
  $input.on('blur', function() {
   var $dropdown = $(this).siblings('.time-suggestions');
     setTimeout(function() {
    $dropdown.hide();
         }, 300);
     });
       
    console.log('Time input initialized for: ' + $input.attr('id'));
         });
 }
      
 function InitializeAll() {
 console.log('=== InitializeAll called ===');
     
     // Wait for jQuery UI to be fully loaded
       var attempts = 0;
var maxAttempts = 10;
     
  var checkAndInit = function() {
   attempts++;
     
    if (typeof $.fn.datepicker !== 'undefined') {
   console.log('jQuery UI datepicker found, initializing...');
       InitializeDatepickers();
     InitializeTimeInputs();
} else if (attempts < maxAttempts) {
  console.log('Waiting for jQuery UI... Attempt ' + attempts);
    setTimeout(checkAndInit, 200);
   } else {
console.error('jQuery UI failed to load after ' + maxAttempts + ' attempts');
  alert('দুঃখিত! তারিখ সিলেক্টর লোড হয়নি। পেজটি রিফ্রেশ করুন (F5)।');
   }
  };
     
      // Start checking
      setTimeout(checkAndInit, 100);
        }
     
    // Document ready
        $(document).ready(function() {
      console.log('Document ready - initializing...');
InitializeAll();
        });
      
    // UpdatePanel refresh handling
   var prm = Sys.WebForms.PageRequestManager.getInstance();
    
        if (prm) {
   prm.add_beginRequest(function(sender, args) {
           console.log('UpdatePanel begin request');
      });
   
  prm.add_endRequest(function(sender, args) {
      console.log('UpdatePanel end request - reinitializing...');
 InitializeAll();
   });
  }
    
    // Class dropdown change handler
     $(document).on('change', 'select[id*="ClassDropdown"]', function() {
        var dropdownId = $(this).attr('id');
   var selectedValue = $(this).val();
var selectedText = $(this).find('option:selected').text();
        console.log('Class dropdown changed:');
   console.log('  Dropdown ID: ' + dropdownId);
          console.log('Selected Value: ' + selectedValue);
 console.log('  Selected Text: ' + selectedText);
          
  // Count total selected classes
  var totalSelected = 0;
  $('select[id*="ClassDropdown"]').each(function() {
      var val = $(this).val();
    if (val && val !== '0' && val !== '') {
    totalSelected++;
      }
  });
          console.log('  Total classes selected: ' + totalSelected);
  });
     
        // Debug function
function debugCheck() {
var report = '=== DEBUG REPORT ===\n\n';
    
// Check jQuery
       report += '1. jQuery Version: ' + (typeof jQuery !== 'undefined' ? jQuery.fn.jquery : 'NOT LOADED') + '\n';
        
  // Check jQuery UI
       report += '2. jQuery UI: ' + (typeof $.ui !== 'undefined' ? 'LOADED' : 'NOT LOADED') + '\n';
   
      // Check datepicker inputs
 var dateInputs = $("input[id*='DateTextBox']");
  report += '3. Date Inputs Found: ' + dateInputs.length + '\n';
 report += '   Has Datepicker: ' + dateInputs.filter('.hasDatepicker').length + '\n';
   
   // Check day inputs
   var dayInputs = $("input[id*='DayTextBox']");
    report += '4. Day Inputs Found: ' + dayInputs.length + '\n';
   
  // Check time inputs
 var timeInputs = $("input[id*='TimeTextBox']");
  report += '5. Time Inputs Found: ' + timeInputs.length + '\n';
      report += '   Initialized: ' + timeInputs.filter(function() { return $(this).data('time-initialized'); }).length + '\n';
       
  // Check class dropdowns
   var classDropdowns = $("select[id*='ClassDropdown']");
   report += '6. Class Dropdowns Found: ' + classDropdowns.length + '\n';
 
            // Check selected classes
   var selectedClasses = 0;
   classDropdowns.each(function() {
       if ($(this).val() !== '0' && $(this).val() !== '') {
   selectedClasses++;
       }
   });
       report += '7. Classes Selected: ' + selectedClasses + '\n';
      
    // Check UpdatePanel
  report += '8. UpdatePanel: ' + (typeof Sys !== 'undefined' && typeof Sys.WebForms !== 'undefined' ? 'LOADED' : 'NOT LOADED') + '\n';
     
  console.log(report);
  alert(report);
     }
        
// Helper function to populate loaded routine data
      function populateRoutineData(rowIndex, columnIndex, subjectID, subjectText, timeText) {
     var subjectDropdownId = 'Subject' + columnIndex + 'Dropdown_' + rowIndex;
     var subjectTextboxId = 'Subject' + columnIndex + 'TextBox_' + rowIndex;
  var timeTextboxId = 'Time' + columnIndex + 'TextBox_' + rowIndex;

      var $subjectDropdown = $('[id*="' + subjectDropdownId + '"]');
   var $subjectTextbox = $('[id*="' + subjectTextboxId + '"]');
            var $timeTextbox = $('[id*="' + timeTextboxId + '"]');

    if ($subjectDropdown.length > 0 && subjectID > 0) {
     $subjectDropdown.val(subjectID);
  }
       
            if ($subjectTextbox.length > 0) {
     $subjectTextbox.val(subjectText);
  }
      
    if ($timeTextbox.length > 0) {
     $timeTextbox.val(timeText);
  }
        }

     // Expose function to global scope for code-behind to call
        window.loadRoutineCellData = function(cellDataJson) {
try {
     var cellData = JSON.parse(cellDataJson);
    
     cellData.forEach(function(cell) {
   populateRoutineData(cell.RowIndex, cell.ColumnIndex, cell.SubjectID, cell.SubjectText, cell.TimeText);
          });
              
     console.log('Routine cell data loaded successfully');
     } catch (error) {
    console.error('Error loading routine cell data:', error);
    }
  };
        
    })(); // Execute immediately
    </script>
    
<div id="printableArea" style="display:none;"></div>


<script type="text/javascript">
    // ... existing script ...

    function printRoutine() {
        // Get the header text
        var routineTitle = $('#<%= RoutineNameLabel.ClientID %>').text().trim();
        if (!routineTitle) {
            routineTitle = " পরীক্ষার রুটিন";
        }

        // Create a new table for printing
        var printTable = $('<table class="routine-table-print"></table>');
        var tableHeader = $('<thead></thead>');
        var tableBody = $('<tbody></tbody>');

        // Build the header for the print table
        var headerRow = $('<tr></tr>');
        $('.routine-table > thead > tr > th').each(function(index) {
            var th = $(this);
            var th_text = "";

            if (index <= 2) { // For Date, Day, Time columns
                th_text = th.text();
            } else { // For class columns
                // Find the span inside the header div
                var classSpan = th.find('span');
                if (classSpan.length > 0) {
                    th_text = classSpan.text();
                } else {
                    th_text = "শ্রেণী";
                }
            }
            headerRow.append('<th>' + th_text + '</th>');
        });
        tableHeader.append(headerRow);


        // Iterate over each row of the original table body
        $('.routine-table > tbody > tr').each(function() {
            var originalRow = $(this);
            var newRow = $('<tr></tr>');

            // 1. Date cell
            var dateVal = originalRow.find('input[id*="ExamDateTextBox"]').val();
            newRow.append('<td>' + (dateVal ? new Date(dateVal).toLocaleDateString('bn-BD', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '') + '</td>');

            // 2. Day cell
            var dayVal = originalRow.find('input[id*="DayNameTextBox"]').val();
            newRow.append('<td>' + dayVal + '</td>');

            // 3. Time cell
            var timeVal = originalRow.find('input[id*="ExamTimeTextBox"]').val();
            newRow.append('<td>' + timeVal + '</td>');

            // 4. Subject cells
            originalRow.find('.editable-cell').each(function() {
                var cell = $(this);
                var cellContent = '';

                // Find all dropdowns and text inputs in the cell, in order
                cell.find('select, input[type="text"]').each(function() {
                    var element = $(this);
                    var value = '';

                    if (element.is('select')) {
                        // It's a dropdown
                        if (element.val() && element.val() !== '0' && element.find('option:selected').text() !== 'বিষয় নির্বাচন করুন') {
                            value = element.find('option:selected').text();
                        }
                    } else {
                        // It's a text input
                        value = element.val();
                    }

                    if (value) {
                        if (cellContent) {
                            // Add a line break if content already exists
                            cellContent += '<br/>';
                        }
                        
                        // Add small tags for time-like inputs
                        if (element.attr('id') && element.attr('id').toLowerCase().includes('time')) {
                            cellContent += '<small>(' + value + ')</small>';
                        } else {
                            cellContent += value;
                        }
                    }
                });

                newRow.append('<td>' + cellContent + '</td>');
            });

            tableBody.append(newRow);
        });

        printTable.append(tableHeader);
        printTable.append(tableBody);

        // Populate the printable area
        var printableArea = $('#printableArea');
        printableArea.empty(); // Clear previous content
        
        var schoolName = "Imperial Ideal School & College"; // You can make this dynamic if needed
        var headerContent = '<div style="text-align:center; margin-bottom: 20px;">' +
                                '<h2>' + schoolName + '</h2>' +
                                '<h4>' + routineTitle + '</h4>' +
                            '</div>';

        printableArea.append(headerContent);
        printableArea.append(printTable);

        // --- New printing logic using an iframe ---
        var printFrame = document.createElement('iframe');
        printFrame.style.position = 'fixed';
        printFrame.style.top = '-1000px';
        document.body.appendChild(printFrame);

        var frameDoc = printFrame.contentWindow.document;
        var frameHead = frameDoc.getElementsByTagName('head')[0];
        var frameBody = frameDoc.getElementsByTagName('body')[0];

        // Copy stylesheets from the main document to the iframe
        $('link[rel="stylesheet"]').each(function () {
            var newLink = frameDoc.createElement('link');
            newLink.rel = 'stylesheet';
            newLink.type = 'text/css';
            newLink.href = this.href;
            frameHead.appendChild(newLink);
        });

        // Add inline styles for printing to ensure content is visible
        var printStyle = frameDoc.createElement('style');
        printStyle.innerHTML = `
            @media print {
                body, html {
                    width: 100%;
                    margin: 0;
                    padding: 0;
                }
                .routine-table-print {
                    width: 100%;
                    border-collapse: collapse;
                }
                .routine-table-print th, .routine-table-print td {
                    border: 1px solid #000;
                    padding: 8px;
                    text-align: left;
                }
            }
        `;
        frameHead.appendChild(printStyle);

        // Set the content of the iframe's body
        frameBody.innerHTML = printableArea.html();

        // Wait for content and styles to load, then print
        setTimeout(function () {
            printFrame.contentWindow.focus();
            printFrame.contentWindow.print();
            // Remove the iframe after a delay
            setTimeout(function () {
                document.body.removeChild(printFrame);
            }, 1000);
        }, 500);
    }
</script>
    </asp:Content>