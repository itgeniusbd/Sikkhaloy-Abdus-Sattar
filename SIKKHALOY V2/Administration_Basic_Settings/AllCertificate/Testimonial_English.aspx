<%@ Page Language="C#"MasterPageFile="~/BASIC.Master"AutoEventWireup="true" CodeBehind="Testimonial_English.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.AllCertificate.Testimonial_English" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
   <style>
     /* Import Google Fonts for elegant certificate look */
  @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400;0,700;1,400;1,700&family=Crimson+Text:ital,wght@0,400;0,600;1,400;1,600&family=Libre+Baskerville:ital,wght@0,400;0,700;1,400&display=swap');

        /* Certificate Border Design */
  body {
         background-color: #f5f5f5;
 }

   .certificate-wrapper {
  background: linear-gradient(to right, #d4af37 0%, #d4af37 100%);
  padding: 3px;
   margin: 20px auto;
     max-width: 210mm;
}

  .certificate-inner {
      background: white;
   border: 2px solid #d4af37;
box-shadow: 
    inset 0 0 0 10px white,
   inset 0 0 0 12px #d4af37,
   inset 0 0 0 22px white,
  inset 0 0 0 24px #d4af37;
    padding: 60px 50px;
      position: relative;
     min-height: 240mm;
   }

 /* Corner decorations */
  .certificate-inner::before,
  .certificate-inner::after {
   content: '';
    position: absolute;
       width: 50px;
   height: 50px;
  border: 3px solid #d4af37;
 }

     .certificate-inner::before {
   top: 25px;
left: 25px;
      border-right: none;
     border-bottom: none;
 }

  .certificate-inner::after {
   bottom: 25px;
    right: 25px;
 border-left: none;
border-top: none;
    }

  .corner-decoration {
     position: absolute;
      width: 50px;
  height: 50px;
       border: 3px solid #d4af37;
 }

   .corner-top-right {
   top: 25px;
 right: 25px;
    border-left: none;
      border-bottom: none;
  }

  .corner-bottom-left {
 bottom: 25px;
  left: 25px;
  border-right: none;
       border-top: none;
        }

        .C-title {
font-size: 2rem;
  font-weight: 700;
  width: 260px;
  margin: auto;
    margin-top: 2rem;
  border: 2px solid #d4af37;
  color: #333;
  text-align: center;
  padding: 10px;
  border-radius: 10px;
  font-family: 'Arial Rounded MT', serif;
    letter-spacing: 1px;
}

   .C-title2 {
  font-size: 2rem;
   color: #000;
     text-align: center;
  margin-bottom: 2rem;
       margin-top: 0.5rem;
 font-family: 'Playfair Display', serif;
        }

        .c-body {
    padding-top: 60px;
    padding-left:20px;
   padding-right:20px;
  color: #2c2c2c;
 font-size: 18px;
   line-height: 40px;
    text-align: justify;
  font-family: 'Libre Baskerville', serif;
    }

        .c-body strong {
            font-weight: 700;
     font-family: 'Crimson Text', serif;
          font-style: italic;
     color: #000;
    }

  .c-footer {
padding-left: 20px;
  font-size: 18px;
  color: #2c2c2c;
  margin-top:40px;
  font-family: 'Crimson Text', serif;
font-style: italic;
  }

     .c-sign {
     position: absolute;
padding: 0 4rem;
       bottom: 4rem;
   font-size: 18px;
  color: #000;
     font-family: 'Crimson Text', serif;
 }

    .c-sign2 {
      position: absolute;
     padding: 0 4rem;
   right: 0;
       bottom: 4rem;
    font-size: 18px;
  color: #000;
    font-family: 'Crimson Text', serif;
      }

     @media print {
@page {
    size: A4 portrait;
 margin: 0;
 }

body {
     background: white !important;
margin: 0 !important;
     padding: 0 !important;
       -webkit-print-color-adjust: exact;
      print-color-adjust: exact;
 }

   .certificate-wrapper {
    margin: 0 !important;
 padding: 4px !important;
      max-width: 100% !important;
background: #d4af37 !important;
   page-break-inside: avoid;
      }
    
.certificate-inner {
  box-shadow: 
    inset 0 0 0 10px white,
  inset 0 0 0 12px #d4af37,
    inset 0 0 0 22px white,
 inset 0 0 0 20px #d4af37 !important;
 padding: 80px 60px !important;
    min-height: 1000px !important;
     page-break-inside: avoid;
}

.certificate-inner::before,
 .certificate-inner::after,
    .corner-decoration {
border-width: 3px !important;
 width: 60px !important;
 height: 40px !important;
     }

    .certificate-inner::before {
   top: 30px !important;
left: 30px !important;
}

.certificate-inner::after {
     bottom: 30px !important;
  right: 30px !important;
       }

 .corner-top-right {
  top: 30px !important;
          right: 30px !important;
    }

  .corner-bottom-left {
bottom: 30px !important;
    left: 30px !important;
}

   .C-title {
       font-size: 2rem !important;
    margin-top: 3rem !important;
    }

        .C-title2 {
        font-size: 2rem !important;
   margin-bottom: 3rem !important;
      }

   .c-body {
    padding-top: 80px !important;
    padding-left: 60px !important;
       padding-right: 60px !important;
      font-size: 18px !important;
   line-height: 40px !important;
}

  .c-footer {
padding-left: 60px !important;
        font-size: 18px !important;
    margin-top: 40px !important;
}

.d-print-none, .NoPrint {
    display: none !important;
}
   }
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <asp:Label ID="CGSSLabel" runat="server"></asp:Label>
    <div class="form-inline NoPrint">
        <div class="form-group">
            <asp:TextBox ID="IDTextBox" autocomplete="off" placeholder="Enter Student ID" runat="server" CssClass="form-control"></asp:TextBox>
        </div>

        <div class="form-group">
            <asp:Button ID="FindButton" runat="server" CssClass="btn btn-primary" Text="Find" ValidationGroup="1" />
        </div>
    </div>
    <div id="ExportPanel" runat="server" class="Exam_Position">
        <asp:Label ID="Export_ClassLabel" runat="server" Font-Bold="True" Font-Names="Tahoma" Font-Size="20px"></asp:Label>

       

    </div>
        <asp:FormView ID="StudentInfoFormView" runat="server" DataSourceID="Reject_StudentInfoSQL" Width="100%">
        <ItemTemplate>
     <a class="btn btn-dark-green d-print-none" href="/Admission/Edit_Student_Info/Edit_Student_information.aspx?Student=<%#Eval("StudentID") %>&Student_Class=<%#Eval("StudentClassID") %>"><i class="fa fa-pencil-square-o" aria-hidden="true"></i>Update Information</a>
 <button type="button" onclick="window.print();" class="d-print-none btn btn-amber pull-right">Print</button>
            <asp:Button ID="ExportWordButton" runat="server" CssClass="btn btn-primary d-print-none" OnClick="ExportWordButton_Click" Text="Export To Word" />

<div class="certificate-wrapper">
   <div class="certificate-inner">
     <div class="corner-decoration corner-top-right"></div>
        <div class="corner-decoration corner-bottom-left"></div>

        <div class="C-title">TESTIMONIAL</div>
          <label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:30px">Date: ......../........../.........</label>
        <div class="c-body">
      It is hereby certified that the student: <strong><%# Eval("StudentsName") %>, </strong> ID: <strong><%# Eval("ID") %></strong>, Class: <strong><%# Eval("Class") %></strong>, Roll No: <strong><%# Eval("RollNo") %></strong>,
   Father: <strong><%# Eval("FathersName") %></strong>, Mother:  <strong><%# Eval("MothersName") %></strong>, 
    Date Of Birth: <strong><%# Eval("DateofBirth","{0:d MMM, yyyy}") %></strong>,
        Address: <strong><%# Eval("StudentPermanentAddress")%></strong>. To my knowledge <%#(string)Eval("Gender") == "Male" ? "he" : "she" %> is of good character and is not involved in any anti-state activities. 
      </div>

 <div class="c-footer">
  I wish all success and prosperity in <%#(string)Eval("Gender") == "Male" ? "his" : "her" %> life.
      </div>
    
       <label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:300px; font-weight:bold;border-top:solid 1px #000">Authority Signature</label> 
   </div>
</div>
   </ItemTemplate>
    </asp:FormView>

    <asp:FormView ID="FormView1" runat="server" DataSourceID="Reject_StudentInfoSQL" Width="100%" Visible="false">
        <ItemTemplate>
          <a class="btn btn-dark-green d-print-none" href="/Admission/Edit_Student_Info/Edit_Student_information.aspx?Student=<%#Eval("StudentID") %>&Student_Class=<%#Eval("StudentClassID") %>"><i class="fa fa-pencil-square-o" aria-hidden="true"></i>Update Information</a>
  <button type="button" onclick="window.print();" class="d-print-none btn btn-amber pull-right">Print</button>
            <asp:Button ID="ExportWordButton" runat="server" CssClass="btn btn-primary d-print-none" OnClick="ExportWordButton_Click" Text="Export To Word" />


   <div class="C-title">TESTIMONIAL</div>
            
   <label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:30px">Date: ......../........../.........</label>
      <asp:Panel ID="Data_Panel" runat="server" CssClass="word-style">
       <div class="Head" style="margin-top: 100px">
        It is hereby certified that the student: <strong><%# Eval("StudentsName") %></strong>, ID: <strong><%# Eval("ID") %></strong>, Class: <strong><%# Eval("Class") %></strong>, Roll No: <strong><%# Eval("RollNo") %></strong>,
  Father:  <strong><%# Eval("FathersName") %>, </strong> Mother: <strong><%# Eval("MothersName") %></strong>, 
           Date Of Birth: <strong><%# Eval("DateofBirth","{0:d MMM, yyyy}") %></strong>,
            Address: <strong><%# Eval("StudentPermanentAddress")%></strong>. To my knowledge <%#(string)Eval("Gender") == "Male" ? "he" : "she" %> is of good character and is not involved in any anti-state activities.
       </div>

  <div class="c-footer">
             I wish all success and prosperity in <%#(string)Eval("Gender") == "Male" ? "his" : "her" %> life.
      </div>

<label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:300px; font-weight:bold;border-top:solid 1px #000">Authority Signature</label> 
    </asp:Panel>
     </ItemTemplate>
    </asp:FormView>
   <asp:SqlDataSource ID="Reject_StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, Student.ID, Student.StudentsName, Student.Gender, Student.FathersName, Student.MothersName, Student.StudentPermanentAddress, StudentsClass.StudentClassID, Student.StudentID, Student.DateofBirth, StudentsClass.RollNo FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (Student.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.Class_Status IS NULL)">
       <SelectParameters>
      <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
 <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
  </SelectParameters>
   </asp:SqlDataSource>
    <script>
        $(function () {
            $('[id*=IDTextBox]').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "Testimonial_English.aspx/GetAllID",
                        data: JSON.stringify({ 'ids': request }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            result($.map(JSON.parse(response.d), function (item) {
                                return item;
                            }));
                        }
                    });
                }
            });
        });
    </script>
</asp:Content>