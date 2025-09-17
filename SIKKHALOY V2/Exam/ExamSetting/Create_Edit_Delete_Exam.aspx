<%@ Page Title="Add Exam" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Create_Edit_Delete_Exam.aspx.cs" Inherits="EDUCATION.COM.EXAM.ExamSetting.Create_Edit_Delete_Exam" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="CSS/Create_Edit_Delete_Exam.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <h3>Add Exam</h3>

    <div class="form-inline">
        <div class="form-group">
            <asp:TextBox ID="ExamNameTextBox" placeholder="Exam Name" runat="server" CssClass="form-control"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ControlToValidate="ExamNameTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AE"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="Period_StartDateTextBox" autocomplete="off" placeholder="Period Start Date" runat="server" CssClass="form-control Datetime"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="Period_StartDateTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AE"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:TextBox ID="Period_EndDateTextBox" autocomplete="off" placeholder="Period End Date" runat="server" CssClass="form-control Datetime"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ControlToValidate="Period_EndDateTextBox" CssClass="EroorSummer" ErrorMessage="*" ValidationGroup="AE"></asp:RequiredFieldValidator>
        </div>
        <div class="form-group">
            <asp:Button ID="ExamButton" runat="server" CssClass="btn btn-primary" Text="Add Exam" OnClick="ExamButton_Click" ValidationGroup="AE" />
        </div>
    </div>

   <a href="Create_Edit_Delete_Exam_Role.aspx">Click here To Add Sub-Exam</a>
   <asp:GridView ID="ExamGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" DataKeyNames="ExamID" DataSourceID="ExamSQL" OnRowDeleted="ExamGridView_RowDeleted">
      <Columns>
         <asp:CommandField ShowEditButton="True" />
         <asp:TemplateField HeaderText="Exam Name" SortExpression="ExamName">
            <EditItemTemplate>
               <asp:TextBox ID="TextBox1" CssClass="textbox" runat="server" Text='<%# Bind("ExamName") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
               <asp:Label ID="Label1" runat="server" Text='<%# Bind("ExamName") %>'></asp:Label>
            </ItemTemplate>
         </asp:TemplateField>
         <asp:TemplateField HeaderText="Period Start Date" SortExpression="Period_StartDate">
            <EditItemTemplate>
               <asp:TextBox ID="TextBox2" CssClass="Datetime" runat="server" Text='<%# Bind("Period_StartDate","{0:d MMM yyyy}") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
               <asp:Label ID="Label2" runat="server" Text='<%# Bind("Period_StartDate","{0:d MMM yyyy}") %>'></asp:Label>
            </ItemTemplate>
         </asp:TemplateField>
         <asp:TemplateField HeaderText="Period End Date" SortExpression="Period_EndDate">
            <EditItemTemplate>
               <asp:TextBox ID="TextBox3" CssClass="Datetime" runat="server" Text='<%# Bind("Period_EndDate","{0:d MMM yyyy}") %>'></asp:TextBox>
            </EditItemTemplate>
            <ItemTemplate>
               <asp:Label ID="Label3" runat="server" Text='<%# Bind("Period_EndDate","{0:d MMM yyyy}") %>'></asp:Label>
            </ItemTemplate>
         </asp:TemplateField>
         <asp:CommandField ShowDeleteButton="True" />
      </Columns>
   </asp:GridView>
   <asp:SqlDataSource ID="ExamSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
      InsertCommand="IF NOT EXISTS(SELECT * FROM Exam_Name WHERE SchoolID = @SchoolID AND ExamName = @ExamName AND EducationYearID = @EducationYearID)
INSERT INTO Exam_Name(SchoolID, RegistrationID, EducationYearID, ExamName, Period_StartDate, Period_EndDate, Date) VALUES (@SchoolID, @RegistrationID, @EducationYearID, @ExamName, @Period_StartDate, @Period_EndDate, GETDATE())"
      SelectCommand="SELECT * FROM Exam_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)"
      DeleteCommand="DELETE FROM Exam_Name WHERE [ExamID] = @ExamID"
      UpdateCommand="IF NOT EXISTS(SELECT * FROM Exam_Name WHERE SchoolID = @SchoolID AND ExamName = @ExamName AND Period_StartDate = @Period_StartDate AND Period_EndDate = @Period_EndDate  AND EducationYearID = @EducationYearID)
UPDATE Exam_Name SET ExamName = @ExamName, Period_StartDate = @Period_StartDate, Period_EndDate = @Period_EndDate WHERE (ExamID = @ExamID)">
      <DeleteParameters>
         <asp:Parameter Name="ExamID" Type="Int32" />
      </DeleteParameters>
      <InsertParameters>
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         <asp:ControlParameter ControlID="ExamNameTextBox" Name="ExamName" PropertyName="Text" Type="String" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
         <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
         <asp:ControlParameter ControlID="Period_StartDateTextBox" Name="Period_StartDate" PropertyName="Text" />
         <asp:ControlParameter ControlID="Period_EndDateTextBox" Name="Period_EndDate" PropertyName="Text" />
      </InsertParameters>
      <SelectParameters>
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
      </SelectParameters>
      <UpdateParameters>
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
         <asp:Parameter Name="ExamName" Type="String" />
         <asp:Parameter Name="Period_StartDate" />
         <asp:Parameter Name="Period_EndDate" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
         <asp:Parameter Name="ExamID" Type="Int32" />
      </UpdateParameters>
   </asp:SqlDataSource>


 
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


    <script>
        $(function () {
            $(".Datetime").datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });
        });
   </script>
</asp:Content>
