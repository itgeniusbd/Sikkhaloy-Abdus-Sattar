<%@ Page Title="Cumulative Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Cumulative_Result.aspx.cs" Inherits="EDUCATION.COM.Exam.CumulativeResult.Cumulative_Result" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="CSS/Result_Print.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   <h3>Cumulative Result</h3>

   <asp:UpdatePanel ID="UpdatePanel1" runat="server">
      <ContentTemplate></ContentTemplate>
   </asp:UpdatePanel>
   <div class="row NoPrint">
      <div class="col-md-2">
         <div class="form-group">
            <label>Class</label>
            <asp:DropDownList ID="ClassDropDownList" runat="server" AppendDataBoundItems="True" CssClass="form-control" DataSourceID="ClassSQL" DataTextField="Class" DataValueField="ClassID" AutoPostBack="True" OnSelectedIndexChanged="ClassDropDownList_SelectedIndexChanged">
               <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ClassSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
               SelectCommand="SELECT DISTINCT CreateClass.Class, CreateClass.ClassID FROM Exam_Result_of_Student INNER JOIN CreateClass ON Exam_Result_of_Student.ClassID = CreateClass.ClassID WHERE (Exam_Result_of_Student.SchoolID = @SchoolID) AND (Exam_Result_of_Student.EducationYearID = @EducationYearID) ORDER BY CreateClass.ClassID">
               <SelectParameters>
                  <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                  <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
               </SelectParameters>
            </asp:SqlDataSource>
         </div>
      </div>

      <%if (GroupDropDownList.Items.Count > 1)
        {%>
      <div class="col-md-2">
         <div class="form-group">
            <label>Group</label>
            <asp:DropDownList ID="GroupDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="GroupSQL" DataTextField="SubjectGroup"
               DataValueField="SubjectGroupID" OnDataBound="GroupDropDownList_DataBound" OnSelectedIndexChanged="GroupDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="GroupSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
               SelectCommand="SELECT DISTINCT [Join].SubjectGroupID, CreateSubjectGroup.SubjectGroup FROM [Join] INNER JOIN CreateSubjectGroup ON [Join].SubjectGroupID = CreateSubjectGroup.SubjectGroupID WHERE ([Join].ClassID = @ClassID) AND ([Join].SectionID LIKE @SectionID) AND ([Join].ShiftID LIKE  @ShiftID) ">
               <SelectParameters>
                  <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
               </SelectParameters>
            </asp:SqlDataSource>
         </div>
      </div>
      <%}%>

      <%if (SectionDropDownList.Items.Count > 1)
        {%>
      <div class="col-md-2">
         <div class="form-group">
            <label>Section</label>
            <asp:DropDownList ID="SectionDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="SectionSQL" DataTextField="Section" DataValueField="SectionID" OnDataBound="SectionDropDownList_DataBound" OnSelectedIndexChanged="SectionDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="SectionSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
               SelectCommand="SELECT DISTINCT [Join].SectionID, CreateSection.Section FROM [Join] INNER JOIN CreateSection ON [Join].SectionID = CreateSection.SectionID WHERE ([Join].ClassID = @ClassID) AND ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].ShiftID LIKE @ShiftID) ">
               <SelectParameters>
                  <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="ShiftDropDownList" Name="ShiftID" PropertyName="SelectedValue" />
               </SelectParameters>
            </asp:SqlDataSource>
         </div>
      </div>
      <%}%>

      <%if (ShiftDropDownList.Items.Count > 1)
        {%>
      <div class="col-md-2">
         <div class="form-group">
            <label>Shift</label>
            <asp:DropDownList ID="ShiftDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ShiftSQL" DataTextField="Shift" DataValueField="ShiftID" OnDataBound="ShiftDropDownList_DataBound" OnSelectedIndexChanged="ShiftDropDownList_SelectedIndexChanged">
            </asp:DropDownList>
            <asp:SqlDataSource ID="ShiftSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
               SelectCommand="SELECT DISTINCT [Join].ShiftID, CreateShift.Shift FROM [Join] INNER JOIN CreateShift ON [Join].ShiftID = CreateShift.ShiftID WHERE ([Join].SubjectGroupID LIKE @SubjectGroupID) AND ([Join].SectionID LIKE  @SectionID) AND ([Join].ClassID = @ClassID)">
               <SelectParameters>
                  <asp:ControlParameter ControlID="GroupDropDownList" Name="SubjectGroupID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="SectionDropDownList" Name="SectionID" PropertyName="SelectedValue" />
                  <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" />
               </SelectParameters>
            </asp:SqlDataSource>
         </div>
      </div>
      <%}%>


      <%if (ExamDropDownList.Items.Count > 1)
        {%>
      <div class="col-md-2">
         <div class="form-group">
            <label>Exam</label>
            <asp:DropDownList ID="ExamDropDownList" runat="server" AutoPostBack="True" CssClass="form-control" DataSourceID="ExamNameSQl" DataTextField="CumulativeResultName" DataValueField="CumulativeNameID" AppendDataBoundItems="True" OnSelectedIndexChanged="ExamDropDownList_SelectedIndexChanged">
               <asp:ListItem Value="0">[ SELECT ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="ExamNameSQl" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
               SelectCommand="SELECT CumulativeNameID, SchoolID, RegistrationID, EducationYearID, CumulativeResultName, Date FROM Exam_Cumulative_Name WHERE (SchoolID = @SchoolID) AND (EducationYearID = @EducationYearID)">
               <SelectParameters>
                  <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                  <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" />
               </SelectParameters>
            </asp:SqlDataSource>
         </div>
      </div>
      <%}%>
   </div>


   <div class="alert alert-success NoPrint">Page Setup Must Be (Page Size: A4. Page Margin: Narrow) In Word File</div>
  
    <%if (ExamDropDownList.SelectedIndex != 0)
     {%>
   <rsweb:ReportViewer ID="ResultReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" ShowRefreshButton="False" WaitMessageFont-Size="14pt" AsyncRendering="False" SizeToReportContent="True" SplitterBackColor="White" DocumentMapWidth="" ZoomMode="PageWidth" Width="100%" Height="100%">
      <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Individual_Result.rdlc" ReportPath="Exam\CumulativeResult\Cumulative_Sub_Exam.rdlc">
         <DataSources>
            <rsweb:ReportDataSource DataSourceId="ExamResultODS" Name="DataSet1" />
         </DataSources>
      </LocalReport>
   </rsweb:ReportViewer>
   <asp:ObjectDataSource ID="ExamResultODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam.CumulativeResult.Cu_ExamTableAdapters.Sub_ResultTableAdapter">
      <SelectParameters>
         <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" Type="Int32" />
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
         <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
         <asp:ControlParameter ControlID="SectionDropDownList" DefaultValue="" Name="SectionID" PropertyName="SelectedValue" Type="String" />
         <asp:ControlParameter ControlID="GroupDropDownList" DefaultValue="" Name="SubjectGroupID" PropertyName="SelectedValue" Type="String" />
         <asp:ControlParameter ControlID="ShiftDropDownList" DefaultValue="" Name="ShiftID" PropertyName="SelectedValue" Type="String" />
      </SelectParameters>
   </asp:ObjectDataSource>
   <asp:ObjectDataSource ID="GradingSystemODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Cumuletive_Grading_SystemTableAdapter">
      <SelectParameters>
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
          <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
          <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" Type="Int32" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
      </SelectParameters>
   </asp:ObjectDataSource>
   <%}%>

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
