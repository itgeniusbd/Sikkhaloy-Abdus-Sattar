<%@ Page Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="CharecterCertificate_English.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.AllCertificate.CharecterCertificate_English" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <style>
        .C-title {
            font-size: 1.5rem;
            font-weight: 700;
            width: 290px;
            margin: auto;
            margin-top: 3rem;
            border-bottom: 2px solid #000;
            color: #333;
        }

        .C-title2 {
            font-size: 1.1rem;
            color: #000;
            text-align: center;
            margin-bottom: 2rem;
            margin-top: 0.5rem;
        }

        .c-body {
            padding: 4rem;
            color: #000;
            font-size: 1.05rem;
            line-height: 32px;
            text-align: justify;
        }

        .c-footer {
            padding: 0 4rem;
            font-size: 1.05rem;
            color: #000;
        }

        .c-sign {
            position: absolute;
            padding: 0 4rem;
            bottom: 4rem;
            font-size: 1.05rem;
            color: #000;
        }

        .c-sign2 {
            position: absolute;
            padding: 0 4rem;
            right: 0;
            bottom: 4rem;
            font-size: 1.05rem;
            color: #000;
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


            <div class="C-title">CHARACTER CERTIFICATE</div>
      <div class="C-title2">TO WHOM IT MAY CONCERN</div>
<div class="c-body">
       This is to certify that, Student: <strong><%# Eval("StudentsName") %></strong>, ID: <strong><%# Eval("ID") %></strong>, Class: <strong><%# Eval("Class") %></strong>, Roll No: <strong><%# Eval("RollNo") %></strong>,
        <%#(string)Eval("Gender") == "Male" ? "son of" : "daughter of" %> <strong><%# Eval("FathersName") %></strong> & <strong><%# Eval("MothersName") %></strong>, 
             Date Of Birth, <strong><%# Eval("DateofBirth","{0:d MMM, yyyy}") %></strong>,
                residence of <strong><%# Eval("StudentPermanentAddress")%></strong>, is known to me. He is a citizen of Bangladesh by birth. To the best of my knowledge,
        he bears a good moral character and is not involved in such activities which are against the state freedom and peace.
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

        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="Reject_StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, Student.ID, Student.StudentsName, Student.Gender, Student.FathersName, Student.MothersName, Student.StudentPermanentAddress, StudentsClass.StudentClassID, Student.StudentID, Student.DateofBirth, StudentsClass.RollNo FROM Student INNER JOIN StudentsClass ON Student.StudentID = StudentsClass.StudentID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (Student.SchoolID = @SchoolID)">
        <SelectParameters>
            <asp:ControlParameter ControlID="IDTextBox" Name="ID" PropertyName="Text" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
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
       This is to certify that, Student: <strong><%# Eval("StudentsName") %></strong>, ID: <strong><%# Eval("ID") %></strong>, Class: <strong><%# Eval("Class") %></strong>, Roll No: <strong><%# Eval("RollNo") %></strong>,
   <%#(string)Eval("Gender") == "Male" ? "son of" : "daughter of" %> <strong><%# Eval("FathersName") %></strong> & <strong><%# Eval("MothersName") %></strong>, 
            Date Of Birth, <strong><%# Eval("DateofBirth","{0:d MMM, yyyy}") %></strong>,
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
