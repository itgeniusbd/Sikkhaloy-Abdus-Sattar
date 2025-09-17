<%@ Page Title="Delete Exam & Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Delete_Exam_And_Result.aspx.cs" Inherits="EDUCATION.COM.Exam.Result.Delete_Exam_And_Result" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <style>
      .Contain p { font-size: 14px; color: red; }
   </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <asp:UpdatePanel ID="ContainUpdatePanel" runat="server">
      <ContentTemplate>

         <div class="Contain">

            <h3>Delete Exam & Result</h3>
            <p>Warning!! If You Delete Any Exam or sub-exam result, Its Will be Deleted Permanently.</p>

            <table>
               <tr>
                  <td>Select Exam 
                     <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="ExamDropDownList" CssClass="EroorSummer" ErrorMessage="Select Exam" InitialValue="0" ValidationGroup="A" SetFocusOnError="True"></asp:RequiredFieldValidator>
                  </td>
                  <td>
                     <asp:Label ID="ClassLabel" runat="server" Text="Class"></asp:Label>
                  </td>
                  <td>
                     <asp:Label ID="SubjectLabel" runat="server" Text="Subject"></asp:Label>
                  </td>
                  <td>
                     <asp:Label ID="SubExamLabel" runat="server" Visible="False">Sub Exam</asp:Label>
                  </td>
               </tr>
               <tr>
                  <td>
                     <asp:DropDownList ID="ExamDropDownList" runat="server" AppendDataBoundItems="True" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamSQL" DataTextField="ExamName" DataValueField="ExamID" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged" Width="200px">
                        <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
                     </asp:DropDownList>
                     <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Exam_Name.ExamID, Exam_Name.ExamName FROM Exam_Name INNER JOIN Exam_Result_of_Student ON Exam_Name.ExamID = Exam_Result_of_Student.ExamID WHERE (Exam_Name.SchoolID = @SchoolID) AND (Exam_Name.EducationYearID = @EducationYearID)">
                        <SelectParameters>
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                  </td>
                  <td>
                     <asp:DropDownList ID="ClassDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ClassNameSQL" DataTextField="Class" DataValueField="ClassID" OnDataBound="ClassDropDownList_DataBound" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
                     </asp:DropDownList>
                     <asp:SqlDataSource ID="ClassNameSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM CreateClass INNER JOIN Exam_Result_of_Student ON CreateClass.ClassID = Exam_Result_of_Student.ClassID WHERE (CreateClass.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) AND (Exam_Result_of_Student.ExamID = @ExamID) ORDER BY CreateClass.ClassID">
                        <SelectParameters>
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                           <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                  </td>
                  <td>
                     <asp:DropDownList ID="SubjectDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubjectSQL" DataTextField="SubjectName" DataValueField="SubjectID" OnDataBound="SubjectDropDownList_DataBound" OnSelectedIndexChanged="SubjectDropDownList_SelectedIndexChanged">
                     </asp:DropDownList>
                     <asp:SqlDataSource ID="SubjectSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT DISTINCT Subject.SubjectID, Subject.SubjectName FROM Subject INNER JOIN Exam_Result_of_Subject ON Subject.SubjectID = Exam_Result_of_Subject.SubjectID WHERE (Exam_Result_of_Subject.ClassID = @ClassID) AND (Exam_Result_of_Subject.SchoolID = @SchoolID) AND (Exam_Result_of_Subject.EducationYearID = @EducationYearID)">
                        <SelectParameters>
                           <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                  </td>
                  <td>
                     <asp:DropDownList ID="SubExamDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SubExamSQL" DataTextField="SubExamName" DataValueField="SubExamID" OnDataBound="SubExamDownList_DataBound" Visible="False">
                     </asp:DropDownList>
                     <asp:SqlDataSource ID="SubExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT distinct Exam_SubExam_Name.SubExamID, Exam_SubExam_Name.SubExamName FROM Exam_SubExam_Name INNER JOIN Exam_Obtain_Marks ON Exam_SubExam_Name.SubExamID = Exam_Obtain_Marks.SubExamID WHERE (Exam_SubExam_Name.SchoolID = @SchoolID) AND (Exam_Obtain_Marks.EducationYearID = @EducationYearID) AND (Exam_Obtain_Marks.ExamID = @ExamID) AND (Exam_Obtain_Marks.ClassID = @ClassID) AND (Exam_Obtain_Marks.SubjectID = @SubjectID)">
                        <SelectParameters>
                           <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                           <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                           <asp:ControlParameter ControlID="ExamDropDownList" Name="ExamID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                           <asp:ControlParameter ControlID="SubjectDropDownList" Name="SubjectID" PropertyName="SelectedValue" />
                        </SelectParameters>
                     </asp:SqlDataSource>
                  </td>
               </tr>
            </table>

            <br />
            <asp:Button ID="DeleteButton" runat="server" CssClass="btn btn-primary" OnClick="DeleteButton_Click" Text="Delete Result" ValidationGroup="A" />
            <p>After Delete Any Kind of Result, You Have to Publish This Exam Result Again<a href="../Publish_Result.aspx"> Click Here For Publish Result >></a></p>

         </div>


      </ContentTemplate>
   </asp:UpdatePanel>
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


</asp:Content>
