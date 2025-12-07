<%@ Page Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="CharecterCertificate_English.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.AllCertificate.CharecterCertificate_English" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <style>
        /* Import Google Fonts for elegant certificate look */
   @import url('https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400;0,700;1,400;1,700&family=Crimson+Text:ital,wght@0,400;0,600;1,400;1,600&family=Libre+Baskerville:ital,wght@0,400;0,700;1,400&display=swap');

        /* Certificate Border Design */
        body {
        background-color: #f5f5f5;
     }

  .certificate-container {
         max-width: 1200px;
            margin: 30px auto;
   background: white;
  padding: 0;
position: relative;
 box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }

    .certificate-border {
   border: 40px solid transparent;
      border-image: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="200" height="200" viewBox="0 0 200 200"><rect fill="%23d4af37" x="0" y="0" width="200" height="200"/><path fill="%23ffffff" d="M20,20 L180,20 L180,180 L20,180 Z"/><circle fill="%23d4af37" cx="20" cy="20" r="15"/><circle fill="%23d4af37" cx="180" cy="20" r="15"/><circle fill="%23d4af37" cx="180" cy="180" r="15"/><circle fill="%23d4af37" cx="20" cy="180" r="15"/><circle fill="%23d4af37" cx="100" cy="20" r="10"/><circle fill="%23d4af37" cx="100" cy="180" r="10"/><circle fill="%23d4af37" cx="20" cy="100" r="10"/><circle fill="%23d4af37" cx="180" cy="100" r="10"/></svg>') 40 round;
     padding: 60px;
      background: white;
   min-height: 800px;
    }

/* Alternative decorative border using CSS */
        .certificate-wrapper {
    background: linear-gradient(to right, #d4af37 0%, #d4af37 100%);
   padding: 3px;
    margin: 20px auto;
        max-width: 210mm; /* A4 width */
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
   min-height: 240mm; /* A4 height minus margins */
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

  /* Top right and bottom left corners */
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
    font-size: 1.4rem;
    font-weight: 700;
    width: 340px;
    margin: auto;
    margin-top: 2rem;
    border-bottom: 3px double #ffc107;
    color: #1a1a1a;
    font-family: 'Playfair Display', serif;
    letter-spacing: 1px;
    text-align: center;
}

 .C-title2 {
  font-size: 1.15rem;
  color: #333;
            text-align: center;
      margin-bottom: 2rem;
      margin-top: 0.8rem;
        font-family: 'Crimson Text', serif;
    font-weight: 600;
 letter-spacing: 0.5px;
    }

 .c-body {
      padding: 3rem 2rem;
  color: #2c2c2c;
     font-size: 1.1rem;
   line-height: 2;
   font-style: italic;
        text-align: justify;
     font-family: 'Libre Baskerville', serif;
        }

     .c-body strong {
    font-weight: 700;
    font-family: 'Crimson Text', serif;
    font-style: italic;
    color: #000;
    font-size: 24px;
    padding-left: 10px;
}
      .ptext {
    text-align: justify;
    line-height: 33px;
    padding: 0;
    margin: 0;
}
        .c-footer {
  padding: 0 2rem;
    font-size: 1.4rem;
          color: #2c2c2c;
       font-family: 'Crimson Text', serif;
        font-style: italic;
        }

    .c-sign {
      position: absolute;
        padding: 0 2rem;
   bottom: 3rem;
       font-size: 1rem;
    color: #000;
 font-family: 'Crimson Text', serif;
 }

    .c-sign2 {
     position: absolute;
       padding: 0 2rem;
  right: 55px;
          bottom: 3rem;
   font-size: 1rem;
   color: #000;
    font-family: 'Crimson Text', serif;
  }

      /* PrintStyles */
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
        inset 0 0 0 24px #d4af37 !important;
          padding: 80px 60px !important;
      min-height: 1000px !important;
     page-break-inside: avoid;
   }

       .certificate-inner::before,
   .certificate-inner::after,
   .corner-decoration {
      border-width: 3px !important;
  width: 60px !important;
      height: 60px !important;
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
    font-size: 1.4rem !important;
      margin-top: 3rem !important;
       }

            .C-title2 {
   font-size: 1.2rem !important;
       margin-bottom: 3rem !important;
      }

  .c-body {
padding: 4rem !important;
   font-size: 1.1rem !important;
line-height: 2.2 !important;
     }

    .c-footer {
        padding: 0 4rem !important;
      font-size: 1.4rem !important;
         margin-top: 2rem !important;
     }

            .c-sign {
    padding: 0 4rem !important;
  bottom: 4rem !important;
   font-size: 1.05rem !important;
     }

     .c-sign2 {
      padding: 0 4rem !important;
  bottom: 4rem !important;
         font-size: 1.05rem !important;
        }

          /* Hide print button and other non-printable elements */
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

        <div class="C-title">CHARACTER CERTIFICATE</div>
      <div class="C-title2">TO WHOM IT MAY CONCERN</div>
<div class="c-body">
       <p class="ptext">This is to certify that, Student:  <strong> <%# Eval("StudentsName") %></strong> </p>
       <p class="ptext"> ID: <strong><%# Eval("ID") %></strong>, Class: <strong><%# Eval("Class") %></strong>, Roll No: <strong><%# Eval("RollNo") %></strong>,
       Date Of Birth, <strong><%# Eval("DateofBirth","{0:d MMM, yyyy}") %></strong>,</p>

   <p class="ptext"> <%#(string)Eval("Gender") == "Male" ? "son of" : "daughter of" %> <strong><%# Eval("FathersName") %></strong> & <strong><%# Eval("MothersName") %></strong>,</p> 
           
     <p class="ptext"> residence of <strong><%# Eval("StudentPermanentAddress")%></strong> is known to me. He is a citizen of Bangladesh by birth. To the best of my knowledge,
      he bears a good moral character and is not involved in such activities which are against the state freedom and peace.</p>
</div>

   <div class="c-footer">
  I wish all success and prosperity in <%#(string)Eval("Gender") == "Male" ? "his" : "her" %> life.
            </div>

   <div class="c-sign">
      Date: ......../........../.........
          </div>
        <div class="c-sign2 text-right">
     Signature: ............................
 </div>
    </div>
</div>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="Reject_StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, Student.ID, Student.StudentsName, Student.Gender, Student.FathersName, Student.MothersName, Student.StudentPermanentAddress, StudentsClass.StudentClassID, Student.StudentID, Student.DateofBirth, StudentsClass.RollNo FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (Student.SchoolID = @SchoolID) AND (StudentsClass.EducationYearID = @EducationYearID) AND (StudentsClass.Class_Status IS NULL)">
        <SelectParameters>
            <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
        </SelectParameters>
    </asp:SqlDataSource>


    <asp:FormView ID="FormView1" runat="server" DataSourceID="Reject_StudentInfoSQL" Width="100%" Visible="false">
    <ItemTemplate>
            <a class="btn btn-dark-green d-print-none" href="/Admission/Edit_Student_Info/Edit_Student_information.aspx?Student=<%#Eval("StudentID") %>&Student_Class=<%#Eval("StudentClassID") %>"><i class="fa fa-pencil-square-o" aria-hidden="true"></i>Update Information</a>
            <button type="button" onclick="window.print();" class="d-print-none btn btn-amber pull-right">Print</button>
      <asp:Button ID="ExportWordButton" runat="server" CssClass="btn btn-primary d-print-none" OnClick="ExportWordButton_Click" Text="Export To Word" />


            <div class="C-title">CHARACTER CERTIFICATE</div>
   <div class="C-title2">TO WHOM IT MAY CONCERN</div>

     <asp:Panel ID="Data_Panel" runat="server" CssClass="word-style">
     <div class="Head" style="margin-top: 100px">
       This is to certify that, Student: <strong><%# Eval("StudentsName") %></strong>, ID: <strong><%# Eval("ID") %></strong>, Class: <strong><%# Eval("Class") %></strong>, Roll No: <strong><%# Eval("RollNo") %></strong>, Date Of Birth, <strong><%# Eval("DateofBirth","{0:d MMM, yyyy}") %></strong>,
   <%#(string)Eval("Gender") == "Male" ? "son of" : "daughter of" %> <strong><%# Eval("FathersName") %></strong> & <strong><%# Eval("MothersName") %></strong>, 
           
            residence of <strong><%# Eval("StudentPermanentAddress")%></strong>, is known to me. He is a citizen of Bangladesh by birth. To the best of my knowledge,
       he bears a good moral character and is not involved in such activities which are against the state freedom and peace.
  </div>

     <div class="c-footer">
                    I wish all success and prosperity in <%#(string)Eval("Gender") == "Male" ? "his" : "her" %> life.
         </div>


  <div class="form-inline" style="margin-top:300px;position:absolute">
     <label class="date-position" style="font-size:50px">Date: ......../........../.........</label> &nbsp&nbsp&nbsp&nbsp&nbsp&nbsp
  <label>Signature: ............................</label> 

         
   
      </div>

    </asp:Panel>
        </ItemTemplate>
    </asp:FormView>

    <script>
        $(function () {
            $('[id*=IDTextBox]').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "CharecterCertificate_English.aspx/GetAllID",
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
