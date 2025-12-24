<%@ Page Title="Cumulative Result" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Cumulative_Result_old.aspx.cs" Inherits="EDUCATION.COM.Exam.CumulativeResult.Cumulative_Result" Async="true" AsyncTimeout="900" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=15.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <link href="CSS/Result_Print.css" rel="stylesheet" />
   <style>
       .loading-overlay {
           position: fixed;
           top: 0;
           left: 0;
           width: 100%;
           height: 100%;
           background: rgba(255,255,255,0.9);
           z-index: 9999;
           display: none;
           justify-content: center;
           align-items: center;
           flex-direction: column;
       }
       .loading-overlay.show {
           display: flex;
       }
       .alert-warning-custom {
           background-color: #fff3cd;
           border: 1px solid #ffc107;
           color: #856404;
           padding: 10px 15px;
           border-radius: 4px;
           margin-bottom: 15px;
       }
   </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
   
   <h3>Cumulative Result

       <a href="Cumulative_Result.aspx"><span class="btn-text-full"> ----> New</span></a>
   </h3>

   <asp:UpdatePanel ID="UpdatePanel1" runat="server">
      <ContentTemplate></ContentTemplate>
   </asp:UpdatePanel>
   
   <!-- Warning Message for Large Data -->
   <div class="alert-warning-custom NoPrint">
       <strong>⚠️ নোটিশ:</strong> বেশি স্টুডেন্ট থাকলে অবশ্যই <strong>Section</strong> সিলেক্ট করুন। একসাথে সব স্টুডেন্টের রিপোর্ট লোড করলে সময় বেশি লাগতে পারে।
   </div>
   
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
   
   <!-- Loading Overlay -->
   <div id="loadingOverlay" class="loading-overlay">
       <img src="../../CSS/loading.gif" alt="Loading..." />
       <br />
       <b>Loading Report... Please wait...</b>
       <p style="color:#666; font-size:12px;">বেশি স্টুডেন্ট থাকলে কিছু সময় লাগতে পারে (৫-১০ মিনিট)</p>
       <p style="color:#ff6600; font-size:11px;">দয়া করে পেইজ রিফ্রেশ করবেন না</p>
   </div>
  
    <%if (ExamDropDownList.SelectedIndex != 0)
     {%>
   <rsweb:ReportViewer ID="ResultReportViewer" runat="server" Font-Names="Verdana" Font-Size="8pt" 
      WaitMessageFont-Names="Verdana" ShowRefreshButton="False" WaitMessageFont-Size="14pt" 
      AsyncRendering="True" 
      ProcessingMode="Local"
      SizeToReportContent="True" SplitterBackColor="White" DocumentMapWidth="" ZoomMode="PageWidth" 
      Width="100%" Height="100%"
      KeepSessionAlive="True"
      ShowWaitControlCancelLink="True">
      <LocalReport ReportEmbeddedResource="EDUCATION.COM.Report_Individual_Result.rdlc" ReportPath="Exam\CumulativeResult\Cumulative_Sub_Exam.rdlc" EnableExternalImages="True" EnableHyperlinks="True">
         <DataSources>
            <rsweb:ReportDataSource DataSourceId="ExamResultODS" Name="DataSet1" />
         </DataSources>
      </LocalReport>
   </rsweb:ReportViewer>
   <asp:ObjectDataSource ID="ExamResultODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam.CumulativeResult.CumulativeResultDataAccess" EnableCaching="True" CacheDuration="600" CacheExpirationPolicy="Sliding">
      <SelectParameters>
         <asp:ControlParameter ControlID="ExamDropDownList" Name="CumulativeNameID" PropertyName="SelectedValue" Type="Int32" />
         <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
         <asp:SessionParameter Name="EducationYearID" SessionField="Edu_Year" Type="Int32" />
         <asp:ControlParameter ControlID="ClassDropDownList" Name="ClassID" PropertyName="SelectedValue" Type="Int32" />
         <asp:ControlParameter ControlID="SectionDropDownList" DefaultValue="%" Name="SectionID" PropertyName="SelectedValue" Type="String" />
         <asp:ControlParameter ControlID="GroupDropDownList" DefaultValue="%" Name="SubjectGroupID" PropertyName="SelectedValue" Type="String" />
         <asp:ControlParameter ControlID="ShiftDropDownList" DefaultValue="%" Name="ShiftID" PropertyName="SelectedValue" Type="String" />
      </SelectParameters>
   </asp:ObjectDataSource>
   <asp:ObjectDataSource ID="GradingSystemODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="EDUCATION.COM.Exam_ResultTableAdapters.Cumuletive_Grading_SystemTableAdapter" EnableCaching="True" CacheDuration="600">
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
   
   <script type="text/javascript">
       // Show loading overlay when report is loading
       var prm = Sys.WebForms.PageRequestManager.getInstance();
       
       // Set page request timeout to 15 minutes
       prm._request = null;
       
       prm.add_beginRequest(function(sender, args) {
           document.getElementById('loadingOverlay').classList.add('show');
       });
       
       prm.add_endRequest(function(sender, args) {
           document.getElementById('loadingOverlay').classList.remove('show');
           
           if (args.get_error()) {
               var errorMsg = args.get_error().message;
               
               // Check for timeout error
               if (errorMsg.indexOf('timeout') > -1 || errorMsg.indexOf('Timeout') > -1 || errorMsg.indexOf('Sys.WebForms.PageRequestManagerTimeoutException') > -1) {
                   alert('রিপোর্ট লোড করতে সময় শেষ হয়ে গেছে।\n\nসমাধান:\n১. অবশ্যই Section সিলেক্ট করুন\n২. Group/Shift সিলেক্ট করে কম স্টুডেন্ট নিয়ে চেষ্টা করুন\n৩. কিছুক্ষণ পরে আবার চেষ্টা করুন');
               } 
               // Check for server error
               else if (errorMsg.indexOf('500') > -1 || errorMsg.indexOf('server') > -1) {
                   alert('সার্ভার এরর হয়েছে।\n\nসমাধান:\n১. Section সিলেক্ট করে আবার চেষ্টা করুন\n২. পেইজ রিফ্রেশ করে আবার চেষ্টা করুন');
               }
               else {
                   alert('রিপোর্ট লোড করতে সমস্যা হয়েছে।\n\nঅনুগ্রহ করে Section/Group সিলেক্ট করে আবার চেষ্টা করুন।\n\nError: ' + errorMsg);
               }
               
               args.set_errorHandled(true);
           }
       });
       
       // Handle page unload to prevent incomplete requests
       window.onbeforeunload = function() {
           if (document.getElementById('loadingOverlay').classList.contains('show')) {
               return 'রিপোর্ট এখনও লোড হচ্ছে। আপনি কি নিশ্চিত যে পেইজ ছেড়ে যেতে চান?';
           }
       };
   </script>
</asp:Content>
