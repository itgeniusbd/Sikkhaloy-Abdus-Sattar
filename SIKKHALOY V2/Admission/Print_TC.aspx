<%@ Page Title="Print TC" Language="C#" MasterPageFile="~/BASIC.Master" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="Print_TC.aspx.cs" Inherits="EDUCATION.COM.Admission.Print_TC" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/print-tc.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <a href="Reject_Student_from_school.aspx" class="NoPrint">Back</a><br />
    <span class="blink_text">Before Print this page Make sure Page setup > Print Background(colors/Images) Selected</span>
    <div class="TC_Head">TRANSFER CERTIFICATE</div>

    <asp:FormView ID="TCFormView" runat="server" DataSourceID="TC_SQL" RenderOuterTable="false">
        <ItemTemplate>
            <a class="btn btn-dark-green d-print-none" href="/Admission/Edit_Student_Info/Edit_Student_information.aspx?Student=<%#Eval("ID") %>&Student_Class=<%#Eval("StudentClassID") %>">
                <i class="fa fa-pencil-square-o" aria-hidden="true"></i>
                Update Information
            </a>

            <asp:Panel ID="Data_Panel" runat="server" CssClass="word-style">
                <div class="Head">
                    It is hereby gladly declared that <b><%# Eval("StudentsName") %></b> Student ID: <b><%# Eval("ID") %> </b>, 
                 <%#(string)Eval("Gender") == "Male" ? "son of" : "daughter of" %> <b><%# Eval("FathersName") %></b>
                    of residence- <b><%# Eval("StudentsLocalAddress") %></b>
                    had been reading in this institution in class <b><%# Eval("Class") %></b> Roll No. <b><%# Eval("RollNo") %></b> upto <b>
                        <input id="ReadingDate" type="text" value='<%# Eval("RejectedDate", "{0:d MMM yyyy}") %>' /></b>.
                     According to the admission register of the institution, <%#(string)Eval("Gender") == "Male" ? "his" : "her" %> date of birth is: <b><%# Eval("DateofBirth", "{0:d MMM yyyy}") %></b>.
      <%#(string)Eval("Gender") == "Male" ? "He" : "She" %> has been full paid in this institution fee on session: <b><%# Eval("EducationYear") %></b>
                </div>

                <div class="TC_Middle">
                    <b>CAUSES OF TRANSFER OF THE INSTITUTION:</b>
                    <ul>
                        <li>
                            <asp:CheckBox ID="CB1" runat="server" Text="Urgent transfer of resident" /></li>
                        <li>
                            <asp:CheckBox ID="CheckBox1" runat="server" Text="Guardian's wish" /></li>
                        <li>
                            <asp:CheckBox ID="CheckBox2" runat="server" Text="Has been completion of the session" /></li>
                        <li>
                            <asp:CheckBox ID="CheckBox3" runat="server" Text="Reason for leaving institution" /></li>
                        <li>
                            <asp:CheckBox ID="CheckBox4" runat="server" Text="Cause of personal" /></li>

                        <br />
                        <li>Remarks on character and conduct of student : Good</li>
                        <li>Remarks on caliber and metal aptitude of student : Wishing success</li>
                    </ul>
                </div>

                <br />
                <div class="Sign">
                    <div class="Sign-head">Signature of Authority</div>
                </div>
            </asp:Panel>
        </ItemTemplate>
    </asp:FormView>

    <asp:SqlDataSource ID="TC_SQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CreateClass.Class,ID, Student.StudentsName, Student.Gender, Student.RejectedDate, Student.FathersName, Student.StudentsLocalAddress, Education_Year.EducationYear, SchoolInfo.SchoolName, SchoolInfo.Address, Student.DateofBirth, StudentsClass.StudentClassID, Student.ID, StudentsClass.RollNo FROM StudentsClass INNER JOIN Student ON StudentsClass.StudentID = Student.StudentID INNER JOIN Education_Year ON StudentsClass.EducationYearID = Education_Year.EducationYearID INNER JOIN SchoolInfo ON Student.SchoolID = SchoolInfo.SchoolID LEFT OUTER JOIN CreateClass ON StudentsClass.ClassID = CreateClass.ClassID WHERE (Student.Status = @Status) AND (Student.SchoolID = @SchoolID) AND (Student.StudentID = @StudentID) AND (StudentsClass.StudentClassID = @StudentClassID)">
        <SelectParameters>
            <asp:Parameter DefaultValue="Rejected" Name="Status" />
            <asp:SessionParameter DefaultValue="" Name="SchoolID" SessionField="SchoolID" />
            <asp:QueryStringParameter Name="StudentID" QueryStringField="Student" />
            <asp:QueryStringParameter Name="StudentClassID" QueryStringField="S_Class" />
        </SelectParameters>
    </asp:SqlDataSource>

    <input id="Print" type="button" onclick="window.print();" value="Print" class="btn btn-primary  d-print-none" />
    <asp:Button ID="Export_Button" runat="server" Text="Export to word" OnClick="Export_Button_Click" class="btn btn-primary d-print-none" />
</asp:Content>
