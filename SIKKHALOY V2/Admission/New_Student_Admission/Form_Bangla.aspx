<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/BASIC.Master" CodeBehind="Form_Bangla.aspx.cs" Inherits="EDUCATION.COM.Admission.New_Student_Admission.Form_Bangla" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="https://fonts.maateen.me/kalpurush/font.css" rel="stylesheet">
    <link rel="stylesheet" href="https://www.w3schools.com/w3css/4/w3.css">
    <style>
        body { line-height: initial; color: #333;font-family: 'Kalpurush', Arial, sans-serif !important; }

        .img-thumbnail {
  padding: 0.25rem;
  background-color: #fff;
  border: 1px solid #dee2e6;
  border-radius:5%;
  width: 80px;
  height: 100px;
}

table {
  border-collapse: collapse;
  width: 100%;
}

th, td {
  text-align: left;
  padding: 8px;
}


  


/*
     -------------------------*/

  .online-form { color: #000;font-family: 'Kalpurush', Arial, sans-serif !important;}
  .online-form h3{border:none; font-family: 'Kalpurush', Arial, sans-serif !important;}
  .online-form .border-bottom { margin-bottom:20px; border-bottom: 1px solid #777 !important; }
  .online-form .cb label { margin-bottom: 0; font-family: 'Kalpurush', Arial, sans-serif !important;}
    .C-title {
    font-size: 2rem;
    font-weight: 800;
    width: 290px;
    margin: auto;
    border-radius:15px;
    font-family: 'Kalpurush', Arial, sans-serif !important;
    color: #333;
    text-align:center;
}
    .c-body {
       color: #000;
      font-size: 20px;
      text-align: justify;
      font-family: 'Kalpurush', Arial, sans-serif !important;
  }
 .online-form fieldset { border-radius:3px;font-family: 'Kalpurush', Arial, sans-serif !important;}
   .online-form legend { padding: 0 7px; font-size: 1rem; font-weight:bold;font-family: 'Kalpurush', Arial, sans-serif !important; }
  .online-form .border{border: 0.5px solid #333 !important;font-family: 'Kalpurush', Arial, sans-serif !important;}

    </style>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    
     
   <asp:FormView ID="FormView1" runat="server" DataSourceID="FormSQL" Width="100%">
       <ItemTemplate>
           <div> <p style="float:left;margin-top:30px;margin-left:10px;font-size:14px; font-weight:bold;font-family: 'Kalpurush', Arial, sans-serif !important;">ভর্তির তারিখ: <%# Eval("AdmissionDate","{0:d MMM yyyy}") %></p></div>
<div class="C-title"><p style="float:left;margin-left:80px;margin-top:20px; border:1px solid #777;padding-left:20px;padding-right:20px;border-radius:10px;">ভর্তি ফরম</p></div>
<div><p style="float:right;margin-top:0px;margin-right:10px;"><img src="/Handeler/Student_Photo.ashx?SID=<%#Eval("StudentImageID") %>" class="img-thumbnail" /></p></div>

           </ItemTemplate>
       </asp:FormView>
    <asp:FormView ID="FormFormView" runat="server" DataSourceID="FormSQL" Width="100%">
        <ItemTemplate>

            <a class="btn btn-blue btn-sm d-print-none" href="../Edit_Student_Info/Edit_Student_information.aspx?Student=<%# Eval("StudentID") %>&Student_Class=<%# Eval("StudentClassID") %>"><i class="fa fa-pencil-square mr-1" aria-hidden="true"></i>Update Info</a>
            <input class="btn btn-blue btn-sm d-print-none" onclick="window.print();" type="button" value="Print" />
            <a class="btn btn-blue btn-sm d-print-none" href="Admission_New_Student.aspx">Admission New Student</a>



            <fieldset class="border p-2 mb-3 c-body">
                <legend class="w-auto">শিক্ষার্থীর তথ্য</legend>

                <table>

                    <tr>
                        <td> ID: <%# Eval("ID") %></td>
                        <td colspan="2"> শিক্ষার্থীর নাম: <%# Eval("StudentsName") %></td>
                        <td> SMS মোবাইল নম্বর: <%# Eval("SMSPhoneNo") %></td>
                        

                    </tr>

                    <tr>
                        <td>*লিঙ্গ: <%# Eval("Gender") %></td>
                        <td>জন্ম তারিখ: <%# Eval("DateofBirth","{0:d MMM yyyy}") %></td>
                        <td>রক্তের গ্রুপ: <%# Eval("BloodGroup") %></td>
                        <td>ধর্ম: <%# Eval("Religion") %></td>


                    </tr>
                    <tr>

                        <td style="text-align: left" colspan="4">শিক্ষার্থীর স্থায়ী ঠিকানা: <%# Eval("StudentPermanentAddress") %></td>


                    </tr>
                    <tr>


                        <td style="text-align: left" colspan="4">শিক্ষার্থীর অস্থায়ী ঠিকানা: <%# Eval("StudentsLocalAddress") %></td>
                    </tr>
                </table>

            </fieldset>
            <fieldset class="border p-2 mb-3 c-body">
                <legend class="w-auto">পিতা-মাতার তথ্য</legend>
    
     <table>
    <tr>

        <td>পিতার নাম: <%# Eval("FathersName") %></td>
        <td>মোবাইল : <%# Eval("FatherPhoneNumber") %></td>
        <td>পিতার পেশা : <%# Eval("FatherOccupation") %></td>
    </tr>


    <tr>

        <td>মাতার নাম : <%# Eval("MothersName") %></td>
        <td>মোবাইল : <%# Eval("MotherPhoneNumber") %></td>
        <td>মাতার পেশা : <%# Eval("MotherOccupation") %></td>
    </tr>
   
</table>
</fieldset>


<fieldset class="border p-2 mb-3 c-body">
<legend class="w-auto">অভিভাবকের তথ্য</legend>  

            <table >
                <tr>
                    <td>অভিভাবকের নাম : <%# Eval("GuardianName") %></td>
 
                    <td>সম্পর্ক : <%# Eval("GuardianRelationshipwithStudent") %></td>
                    
                    <td>মোবাইল: <%# Eval("GuardianPhoneNumber") %></td>
                    
                </tr>
            </table>
    </fieldset>
           
<fieldset class="border p-2 mb-3 c-body">
<legend class="w-auto">পূর্বে যে প্রতিষ্ঠানে  পড়েছে তার তথ্য</legend>  

            <table>
                <tr>
                    <td>পূর্বের প্রতিষ্ঠান : <%# Eval("PrevSchoolName") %> </td>
                    <td>যে শ্রেণিতে পড়েছে : <%# Eval("PrevClass") %></td>
                    <td>পরীক্ষার বছর : <%# Eval("PrevExamYear") %></td>


                    <td>পরীক্ষায় যে গ্রেড পেয়েছে : <%# Eval("PrevExamGrade") %></td>

                </tr>
            </table>
</fieldset>

<fieldset class="border p-2 mb-3 c-body">
<legend class="w-auto">প্রাতিষ্ঠানিক তথ্য</legend>  

            <table>
                <tr>
                    <td>যে শ্রেণিতে পড়বে : <%# Eval("Class") %></td>
                   
                    <td>রোল নং : <%# Eval("RollNo") %></td>
                    
                    <td>শাখা : <%# Eval("Section") %></td>
                   
                    <td>শিফট : <%# Eval("Shift") %></td>
                   
                    <td>গ্রুপ : <%# Eval("SubjectGroup") %></td>
                    
                </tr>
            </table>
    </fieldset>
        </ItemTemplate>
    </asp:FormView>
    <asp:SqlDataSource ID="FormSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT Student.ID, Student.StudentsName, Student.FathersName, CreateClass.Class, StudentsClass.RollNo, CreateSection.Section, CreateSubjectGroup.SubjectGroup, CreateShift.Shift, Student.SMSPhoneNo, Student.StudentImageID, Student.StudentID, Student.SchoolID, Student.StudentEmailAddress, Student.DateofBirth, Student.BloodGroup, Student.Religion, Student.Gender, Student.StudentPermanentAddress, Student.StudentsLocalAddress, Student.PrevSchoolName, Student.PrevClass, Student.PrevExamYear, Student.PrevExamGrade, Student.MothersName, Student.MotherOccupation, Student.MotherPhoneNumber, Student.FatherOccupation, Student.FatherPhoneNumber, Student.GuardianName, Student.GuardianRelationshipwithStudent, Student.GuardianPhoneNumber, Student.OtherDetails, Student.AdmissionDate, StudentsClass.ClassID, StudentsClass.StudentClassID FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID LEFT OUTER JOIN CreateShift ON StudentsClass.ShiftID = CreateShift.ShiftID LEFT OUTER JOIN CreateSubjectGroup ON StudentsClass.SubjectGroupID = CreateSubjectGroup.SubjectGroupID LEFT OUTER JOIN CreateSection ON StudentsClass.SectionID = CreateSection.SectionID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.SchoolID = @SchoolID) AND (Student.StudentID = @StudentID) AND (StudentsClass.StudentClassID = @StudentClassID)">
        <SelectParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:QueryStringParameter Name="StudentClassID" QueryStringField="StudentClass" />
            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
        </SelectParameters>
    </asp:SqlDataSource>


    <div>
        <p style="float:left;border-top : solid 1px #808080;margin-top:100px">শিক্ষার্থীর স্বাক্ষর</p>
        <p style="float:right;border-top : solid 1px #808080;margin-top:100px">মুহতামীমের স্বাক্ষর</p>
    </div>


</asp:Content>
