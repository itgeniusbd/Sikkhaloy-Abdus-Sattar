<%@ Page Title="Add Sub-Exam" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Edit_Delete_Exam_Role.aspx.cs" Inherits="EDUCATION.COM.EXAM.ExamSetting.Create_Edit_Delete_Exam_Role" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="CSS/Create_Edit_Delete_Exam.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <div id="ExamRole">
      <h3>Add Sub-Exam</h3>
      <asp:UpdatePanel ID="UpdatePanel1" runat="server">
         <ContentTemplate>
             <a href="Marks_Distribution.aspx">Click here To Marks Distribution For Exam</a>
            <asp:ListView ID="ExamRoleListView" runat="server" DataKeyNames="SubExamID" DataSourceID="ExmRoleSQL"
               InsertItemPosition="FirstItem" OnItemInserting="ExamRoleListView_ItemInserting" OnItemDeleted="ExamRoleListView_ItemDeleted">
               <ItemTemplate>
                  <tr class="ExamListview_Row">
                     <td>
                        <asp:Button ID="DeleteButton" CssClass="btn btn-default btn-sm" runat="server" CommandName="Delete" Text="Delete" OnClientClick="return confirm('Are you sure want to delete?')" />
                        <asp:Button ID="EditButton" CssClass="btn btn-default btn-sm" runat="server" CommandName="Edit" Text="Edit" />
                     </td>
                     <td>
                        <asp:Label ID="ExamNameLabel" runat="server" Text='<%# Eval("SubExamName") %>' />
                     </td>
                       <td>
                        <asp:Label ID="Label1" runat="server" Text='<%# Eval("Sub_ExamSN") %>' />
                     </td>
                  </tr>
               </ItemTemplate>

               <EditItemTemplate>
                  <tr class="ExamListview_Edit">
                     <td>
                        <asp:Button ID="UpdateButton" CssClass="btn btn-default btn-sm" runat="server" CommandName="Update" Text="Update" />
                        <asp:Button ID="CancelButton" CssClass="btn btn-default btn-sm" runat="server" CommandName="Cancel" Text="Cancel" />
                     </td>

                     <td>
                        <asp:TextBox ID="ExamNameTextBox" CssClass="form-control" runat="server" Text='<%# Bind("SubExamName") %>' />
                     </td>
                       <td>
                        <asp:TextBox ID="TextBox1" CssClass="form-control" runat="server" Text='<%# Bind("Sub_ExamSN") %>' />
                     </td>
                  </tr>
               </EditItemTemplate>

               <InsertItemTemplate>
                  <tr class="ExamListview_Insert">
                     <td>
                        <asp:Button ID="InsertButton"  CssClass="btn btn-primary btn-sm" runat="server" CommandName="Insert" Text="Insert" />
                        <asp:Button ID="CancelButton" CssClass="btn btn-primary btn-sm" runat="server" CommandName="Cancel" Text="Clear"/>
                     </td>
                     <td>
                        <asp:TextBox ID="ExamNameTextBox" placeholder="Sub-Exam" CssClass="form-control" runat="server" Text='<%# Bind("SubExamName") %>' />
                     </td>
                        <td>
                        <asp:TextBox ID="TextBox3" CssClass="form-control" placeholder="Serial No" runat="server" Text='<%# Bind("Sub_ExamSN") %>' />
                     </td>
                  </tr>
               </InsertItemTemplate>

               <LayoutTemplate>
                  <table id="itemPlaceholderContainer">
                     <tr id="Tr1" runat="server" class="ExamListview_Head">
                        <th id="Th1" runat="server">Modify</th>
                        <th id="Th2" runat="server">Sub-Exam Name</th>
                         <th id="Th3" runat="server">Serial No</th>
                     </tr>
                     <tr id="itemPlaceholder" runat="server"></tr>
                  </table>
               </LayoutTemplate>
            </asp:ListView>

            <asp:SqlDataSource ID="ExmRoleSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
               InsertCommand="INSERT INTO Exam_SubExam_Name(SchoolID, RegistrationID, EducationYearID, SubExamName, Sub_ExamSN) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @SubExamName, @Sub_ExamSN)"
               SelectCommand="SELECT SubExamID, SchoolID, RegistrationID, EducationYearID, SubExamName, Sub_ExamSN FROM Exam_SubExam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID) ORDER BY Sub_ExamSN"
               DeleteCommand="DELETE FROM [Exam_SubExam_Name] WHERE [SubExamID] = @SubExamID"
               UpdateCommand="UPDATE Exam_SubExam_Name SET SubExamName = @SubExamName, Sub_ExamSN = @Sub_ExamSN WHERE (SubExamID = @SubExamID)">
               <DeleteParameters>
                  <asp:Parameter Name="SubExamID" Type="Int32" />
               </DeleteParameters>
               <InsertParameters>
                  <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                  <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                  <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
                  <asp:Parameter Name="SubExamName" Type="String" />
                   <asp:Parameter Name="Sub_ExamSN" />
               </InsertParameters>
               <SelectParameters>
                  <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                  <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
               </SelectParameters>
               <UpdateParameters>
                  <asp:Parameter Name="SubExamName" Type="String" />
                   <asp:Parameter Name="Sub_ExamSN" />
                  <asp:Parameter Name="SubExamID" Type="Int32" />
               </UpdateParameters>
            </asp:SqlDataSource>
         </ContentTemplate>
      </asp:UpdatePanel>

      <asp:UpdateProgress ID="UpdateProgress" runat="server">
         <ProgressTemplate>
            <div id="progress_BG"></div>
            <div id="progress">
               <img src="../../CSS/loading.gif" alt="Loading..." />
               <br />
               <b>Loading...</b></div>
         </ProgressTemplate>
      </asp:UpdateProgress>
   </div>
</asp:Content>
