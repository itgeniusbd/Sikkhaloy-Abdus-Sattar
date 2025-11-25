<%@ Page Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="CharecterCertificate_Bangla.aspx.cs" Inherits="EDUCATION.COM.Administration_Basic_Settings.AllCertificate.CharecterCertificate_Bangla" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">

    <style>

        .C-title {
            font-size: 2rem;
            font-weight: 700;
            width: 290px;
            margin: auto;
            margin-top: 3rem;
            border-bottom: 2px solid #000;
            font-family: 'Kalpurush', Arial, sans-serif !important;
            color: #333;
        }

        .C-title2 {
            font-size: 2rem;
            color: #000;
            text-align: center;
            margin-bottom: 2rem;
            margin-top: 0.5rem;
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }

        .c-body {
            padding-top: 80px;
            padding-left:20px;
            padding-right:20px;
            color: #000;
            font-size: 18px;
            line-height: 40px;
            text-align: justify;
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }

        .c-footer {
padding-left: 20px;
  font-size: 18px;
  color: #000;
  margin-top:40px;
  font-family: 'Kalpurush', Arial, sans-serif !important;
        }

        .c-sign {
            position: absolute;
            padding: 0 4rem;
            bottom: 4rem;
            font-size: 18px;
            color: #000;
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }

        .c-sign2 {
            position: absolute;
            padding: 0 4rem;
            right: 0;
            bottom: 4rem;
            font-size: 18px;
            color: #000;
            font-family: 'Kalpurush', Arial, sans-serif !important;
        }
        .C-title {
font-size: 2rem;
  font-weight: 700;
  width: 260px;
  margin: auto;
    margin-top: auto;
  margin-top: auto;
  margin-top: 3rem;
  border: 2px solid #ff0000;
  color: #333;
  text-align: center;
  padding: 10px;
  border-radius: 10px;
  font-family: 'Kalpurush', Arial, sans-serif !important;
}

       .c-body strong {
            font-weight:bold;

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


            <div class="C-title">চারিত্রিক সনদ</div>
    <label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:30px">তারিখ: ......../........../.........</label>
            <div class="c-body">
                এই মর্মে প্রত্যয়ন করা হইতেছে যে,  শিক্ষার্থী: <strong><%# Eval("StudentsName") %>, </strong> আইডি নম্বর: <strong><%# Eval("ID") %></strong>, শ্রেণি: <b><%# Eval("Class") %></b>, রোল নং: <strong><%# Eval("RollNo") %></strong>,
          পিতা: <strong><%# Eval("FathersName") %></strong>, মাতা:  <strong><%# Eval("MothersName") %></strong>, 
     জন্ম তারিখ: <strong><%# Eval("DateofBirth","{0:d MMM, yyyy}") %></strong>,
           ঠিকানা: <strong><%# Eval("StudentPermanentAddress")%></strong>। আমার নিকট সে ব্যাক্তিগত रुपमा পরিচিত। আমার জানামতে সে রাষ্ট্র বিরোধী কোন কার্যক্রমে জড়িত নয়। 
       তাহার স্বভাব চরিত্র ভালো。
      </div>

            <div class="c-footer">
      <strong>আমি তাহার সর্বাঙ্গীন উন্নতি ও মঙ্গল কামনা করছি।</strong> 
   </div>
 
  <label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:300px; font-weight:bold;border-top:solid 1px #000">কর্তৃপক্ষের স্বাক্ষর</label> 

        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="Reject_StudentInfoSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class, Student.ID, Student.StudentsName, Student.Gender, Student.FathersName,Student.MothersName, Student.StudentPermanentAddress, Education_Year.EducationYear, SchoolInfo.SchoolName, SchoolInfo.Address, Student.DateofBirth, StudentsClass.StudentClassID, Student.StudentID, StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID INNER JOIN SchoolInfo ON Student.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.ID = @ID) AND (Student.SchoolID = @SchoolID)">
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


      
  

      <asp:Panel ID="Data_Panel" runat="server" CssClass="word-style">
 <label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:30px">তারিখ: ......../........../.........</label>
        <div class="C-title">চারিত্রিক সনদ</div>
       <div class="Head" style="margin-top: 100px">
     এই মর্মে প্রত্যয়ন করা হইতেছে যে,  শিক্ষার্থী: <strong><%# Eval("StudentsName") %></strong>, আইডি নম্বর: <strong><%# Eval("ID") %></strong>, শ্রেণি: <b><%# Eval("Class") %></b>, রোল নং: <strong><%# Eval("RollNo") %></strong>,
        পিতা:  <strong><%# Eval("FathersName") %>, </strong>মাতা: <strong><%# Eval("MothersName") %></strong>, 
    জন্ম তারিখ: <strong><%# Eval("DateofBirth","{0:d MMM, yyyy}") %></strong>,
            ঠিকানা: <strong><%# Eval("StudentPermanentAddress")%></strong>, আমার নিকট সে ব্যাক্তিগত ভাবে পরিচিত। আমার জানামতে সে রাষ্ট্র বিরোধী কোন কার্যক্রমে জড়িত নয়।
  <p>তাহার স্বভাব চরিত্র ভালো।</p>
                </div>

                <div class="c-footer">
       <strong>আমি তাহার সর্বাঙ্গীন উন্নতি ও মঙ্গল কামনা করছি।</strong> 
       </div>


     
       <label class="date-position" style="float: right;font-size: 18px;margin-right:20px;margin-top:30px">স্বাক্ষর: ............................</label> 
         
       
      

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

        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function (a, b) {
            if ($(".isStatus").text() === "Active") {
                $(".isStatus").css("color", "green");
            }
            else {
                $(".isStatus").css("color", "red");
            }

            $('.Sid').typeahead({
                minLength: 1,
                source: function (request, result) {
                    $.ajax({
                        url: "Reject_Student_from_school.aspx/GetAllID",
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
