<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Basic_Student.Master" CodeBehind="Exam_Routine_forStudent.aspx.cs" Inherits="EDUCATION.COM.Student.Exam_Routine" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="../Routines/CSS/Exam_Routine.css" rel="stylesheet" />
    <style>
     .routine-container {
 max-width: 100%;
   margin: 20px auto;
     padding: 20px;
  }

.routine-section {
    margin-bottom: 40px;
padding-bottom: 30px;
  border-bottom: 2px solid #ddd;
}

    .routine-section:last-child {
    border-bottom: none;
}
        
.routine-header {
    text-align: center;
     margin-bottom: 20px;
       padding: 15px;
      background: #f8f9fa;
         border-radius: 5px;
 }
    
  .routine-title {
    font-size: 24px;
      font-weight: bold;
  color: #2c3e50;
    margin-bottom: 10px;
     }
        
      .class-info {
       font-size: 16px;
    color: #7f8c8d;
        }
      
      .no-routine {
    text-align: center;
     padding: 50px;
       font-size: 18px;
 color: #95a5a6;
      }
    
 .print-button {
  margin: 15px 0;
   text-align: center;
    }
  </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
  <div class="routine-container">
        <h3 class="text-center">পরীক্ষার রুটিন / Exam Routine</h3>
    
      <!-- Routine Display Area -->
      <asp:Literal ID="RoutineDisplayLiteral" runat="server"></asp:Literal>
      
        <!-- No Routine Message -->
        <asp:Panel ID="NoRoutinePanel" runat="server" Visible="false" CssClass="no-routine">
   <i class="fa fa-calendar-times-o fa-3x"></i>
 <p>কোনো পরীক্ষার রুটিন পাওয়া যায়নি।</p>
     </asp:Panel>
        
    </div>

    <script>
   $(function () {
 $("#_7").addClass("active"); // Activate Exam Routine menu item
     });
   
        function printRoutine(routineIndex) {
   var printArea = document.getElementById('printableArea' + routineIndex);
 if (!printArea) {
    alert('Routine not found');
      return;
    }
            
      var printContents = printArea.innerHTML;
     var originalContents = document.body.innerHTML;
   
   document.body.innerHTML = printContents;
        window.print();
   document.body.innerHTML = originalContents;
  location.reload();
  }
    </script>
</asp:Content>