<%@ Page Title="SMS Template Management" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="SMS_Template.aspx.cs" Inherits="EDUCATION.COM.SMS.SMS_Template" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .placeholder-tag {
 display: inline-block;
        background: #e3f2fd;
            color: #1976d2;
            padding: 4px 10px;
     margin: 3px;
     border-radius: 12px;
  font-size: 12px;
        font-weight: 500;
 border: 1px solid #90caf9;
   }

        .category-tabs {
            border-bottom: 2px solid #dee2e6;
            margin-bottom: 20px;
        }

  .category-tabs .nav-link {
            border: none;
 border-bottom: 3px solid transparent;
            color: #6c757d;
      font-weight: 500;
            padding: 12px 20px;
       font-size: 16px;
        }

        .category-tabs .nav-link:hover {
   color: #495057;
            border-bottom-color: #dee2e6;
        }

        .category-tabs .nav-link.active {
        color: #007bff;
     border-bottom-color: #007bff;
      background: transparent;
        }

      .tab-icon {
         font-size: 20px;
  margin-right: 8px;
        }

        /* Fix modal text visibility */
        .modal-body label {
            color: #333 !important;
  font-weight: 500;
        }

  .modal-body .form-text {
            color: #6c757d !important;
 }

    /* Ensure proper container width */
        .container-fluid {
  padding-left: 15px;
    padding-right: 15px;
            padding-bottom: 50px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container-fluid">
    <h2 class="mb-4">
      <i class="fa fa-comments"></i> SMS TEMPLATE MANAGEMENT (সকল ধরনের SMS টেমপ্লেট ম্যানেজমেন্ট)
   </h2>

   <asp:Label ID="MessageLabel" runat="server" Visible="false" CssClass="alert"></asp:Label>

        <!-- Category Tabs -->
     <ul class="nav nav-tabs category-tabs" id="categoryTabs" role="tablist">
 <li class="nav-item">
                <a class="nav-link active" id="exam-tab" data-toggle="tab" href="#exam-panel" role="tab">
          <span class="tab-icon">📝</span> Exam Result
              </a>
            </li>
<li class="nav-item">
      <a class="nav-link" id="payment-tab" data-toggle="tab" href="#payment-panel" role="tab">
                <span class="tab-icon">💰</span> Payment
     </a>
       </li>
         <li class="nav-item">
     <a class="nav-link" id="due-tab" data-toggle="tab" href="#due-panel" role="tab">
<span class="tab-icon">💸</span> Due SMS
       </a>
   </li>

  <li class="nav-item">
        <a class="nav-link" id="attendance-tab" data-toggle="tab" href="#attendance-panel" role="tab">
         <span class="tab-icon">📅</span> Attendance
                </a>
   </li>

            <li class="nav-item">
            <a class="nav-link" id="admission-tab" data-toggle="tab" href="#admission-panel" role="tab">
          <span class="tab-icon">🎓</span> Admission
     </a>
            </li>
        </ul>

        <!-- Tab Content -->
        <div class="tab-content" id="categoryTabContent">
            <!-- Exam Result Tab -->
        <div class="tab-pane fade show active" id="exam-panel" role="tabpanel">
    <div class="row">
         <div class="col-md-12">
     <div class="alert alert-info">
    <strong>📝 Exam Result Templates:</strong><br />
   <span class="placeholder-tag">{StudentName}</span>
  <span class="placeholder-tag">{ID}</span>
  <span class="placeholder-tag">{ExamName}</span>
  <span class="placeholder-tag">{TotalMarks}</span>
    <span class="placeholder-tag">{Grade}</span>
      <span class="placeholder-tag">{Point}</span>
        <span class="placeholder-tag">{ClassPosition}</span>
     <span class="placeholder-tag">{SectionPosition}</span>
       <span class="placeholder-tag">{SchoolName}</span>
       </div>
         </div>
  </div>

         <asp:Button ID="AddExamTemplateButton" runat="server" Text="+ Add New Exam Template" 
   CssClass="btn btn-primary mb-3" OnClick="AddNewTemplate_Click" CommandArgument="ExamResult" />

          <asp:GridView ID="ExamTemplatesGridView" runat="server" AutoGenerateColumns="False" 
  CssClass="table table-hover" DataSourceID="ExamTemplatesSQL" DataKeyNames="TemplateID"
          OnRowCommand="TemplatesGridView_RowCommand">
  <Columns>
    <asp:BoundField DataField="TemplateName" HeaderText="Template Name" />
   <asp:BoundField DataField="TemplateType" HeaderText="Type" />
               <asp:TemplateField HeaderText="Message Preview">
           <ItemTemplate>
        <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
    <%# Eval("MessageTemplate") %>
          </div>
          </ItemTemplate>
    </asp:TemplateField>
              <asp:TemplateField HeaderText="Status">
    <ItemTemplate>
 <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
         <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
         </span>
       </ItemTemplate>
         </asp:TemplateField>
            <asp:TemplateField HeaderText="Actions">
         <ItemTemplate>
 <asp:LinkButton ID="EditButton" runat="server" CssClass="btn btn-sm btn-info" 
           CommandName="EditTemplate" CommandArgument='<%# Eval("TemplateID") %>'>
  <i class="fa fa-edit"></i> Edit
        </asp:LinkButton>
            <asp:LinkButton ID="DeleteButton" runat="server" CssClass="btn btn-sm btn-danger ml-1" 
      CommandName="DeleteTemplate" CommandArgument='<%# Eval("TemplateID") %>'
         OnClientClick="return confirm('Are you sure you want to delete this template?');">
           <i class="fa fa-trash"></i> Delete
     </asp:LinkButton>
 </ItemTemplate>
 </asp:TemplateField>
  </Columns>
        </asp:GridView>
<asp:SqlDataSource ID="ExamTemplatesSQL" runat="server" 
        ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT TemplateID, TemplateName, TemplateType, MessageTemplate, IsActive, CreatedDate 
      FROM SMS_Template 
            WHERE SchoolID = @SchoolID AND TemplateCategory = 'ExamResult'
        ORDER BY CreatedDate DESC">
   <SelectParameters>
     <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
   </asp:SqlDataSource>
       </div>

  <!-- Payment Tab -->
        <div class="tab-pane fade" id="payment-panel" role="tabpanel">
           <div class="row">
 <div class="col-md-12">
   <div class="alert alert-success">
           <strong>💰 Payment Templates:</strong><br />
    <span class="placeholder-tag">{StudentName}</span>
           <span class="placeholder-tag">{ID}</span>
                 <span class="placeholder-tag">{Amount}</span>
        <span class="placeholder-tag">{ReceiptNo}</span>
      <span class="placeholder-tag">{PaymentDetails}</span>
                <span class="placeholder-tag">{CurrentDue}</span>
           <span class="placeholder-tag">{SchoolName}</span>
       </div>
              </div>
     </div>

        <asp:Button ID="AddPaymentTemplateButton" runat="server" Text="+ Add New Payment Template" 
            CssClass="btn btn-success mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Payment" />

     <asp:GridView ID="PaymentTemplatesGridView" runat="server" AutoGenerateColumns="False" 
    CssClass="table table-hover" DataSourceID="PaymentTemplatesSQL" DataKeyNames="TemplateID"
            OnRowCommand="TemplatesGridView_RowCommand">
  <Columns>
           <asp:BoundField DataField="TemplateName" HeaderText="Template Name" />
 <asp:BoundField DataField="TemplateType" HeaderText="Type" />
            <asp:TemplateField HeaderText="Message Preview">
      <ItemTemplate>
           <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
 <%# Eval("MessageTemplate") %>
    </div>
              </ItemTemplate>
        </asp:TemplateField>
   <asp:TemplateField HeaderText="Status">
       <ItemTemplate>
              <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
      </span>
        </ItemTemplate>
     </asp:TemplateField>
    <asp:TemplateField HeaderText="Actions">
  <ItemTemplate>
       <asp:LinkButton ID="EditButton" runat="server" CssClass="btn btn-sm btn-info" 
           CommandName="EditTemplate" CommandArgument='<%# Eval("TemplateID") %>'>
      <i class="fa fa-edit"></i> Edit
            </asp:LinkButton>
           <asp:LinkButton ID="DeleteButton" runat="server" CssClass="btn btn-sm btn-danger ml-1" 
        CommandName="DeleteTemplate" CommandArgument='<%# Eval("TemplateID") %>'
       OnClientClick="return confirm('Are you sure you want to delete this template?');">
            <i class="fa fa-trash"></i> Delete
   </asp:LinkButton>
       </ItemTemplate>
          </asp:TemplateField>
   </Columns>
      </asp:GridView>
    <asp:SqlDataSource ID="PaymentTemplatesSQL" runat="server" 
  ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT TemplateID, TemplateName, TemplateType, MessageTemplate, IsActive, CreatedDate 
       FROM SMS_Template 
        WHERE SchoolID = @SchoolID AND TemplateCategory = 'Payment'
  ORDER BY CreatedDate DESC">
    <SelectParameters>
          <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            </SelectParameters>
             </asp:SqlDataSource>
            </div>

          <!-- Attendance Tab -->
   <div class="tab-pane fade" id="attendance-panel" role="tabpanel">
     <div class="row">
      <div class="col-md-12">
  <div class="alert alert-warning">
 <strong>📅 Attendance SMS Templates (হাজিরা এসএমএস টেমপ্লেট):</strong><br />
        <div style="margin-top: 10px;">
             <strong>Available Placeholders (ব্যবহারযোগ্য প্লেসহোল্ডার):</strong><br />
     <span class="placeholder-tag">{StudentName}</span>
      <span class="placeholder-tag">{ID}</span>
  <span class="placeholder-tag">{Date}</span>
        <span class="placeholder-tag">{EntryTime}</span>
       <span class="placeholder-tag">{ExitTime}</span>
         <span class="placeholder-tag">{LateMinutes}</span>
      <span class="placeholder-tag">{SchoolName}</span>
   <span class="placeholder-tag">{Class}</span>
      <span class="placeholder-tag">{Roll}</span>
     <br /><br />
   <strong>Template Types (টেমপ্লেট টাইপ):</strong><br />
                  • ✅ <strong>Entry</strong> - স্কুলে প্রবেশের SMS<br />
             • 🚪 <strong>Exit</strong> - স্কুল ত্যাগের SMS<br />
    • ⏰ <strong>Late</strong> - দেরিতে আসার SMS<br />
      • ❌ <strong>Absent</strong> - অনুপস্থিতির SMS<br />
    • ⚠️ <strong>Late Abs</strong> - দেরিতে + অনুপস্থিত গণনা<br />
             • ✔️ <strong>Present</strong> - সাধারণ উপস্থিতি নিশ্চিতকরণ
          </div>
 </div>
 </div>
      </div>

 <asp:Button ID="AddAttendanceTemplateButton" runat="server" Text="+ Add New Attendance Template" 
           CssClass="btn btn-warning mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Attendance" />

       <asp:GridView ID="AttendanceTemplatesGridView" runat="server" AutoGenerateColumns="False" 
   CssClass="table table-hover" DataSourceID="AttendanceTemplatesSQL" DataKeyNames="TemplateID"
  OnRowCommand="TemplatesGridView_RowCommand">
    <Columns>
   <asp:BoundField DataField="TemplateName" HeaderText="Template Name" />
    <asp:BoundField DataField="TemplateType" HeaderText="Type" />
   <asp:TemplateField HeaderText="Message Preview">
<ItemTemplate>
      <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
     <%# Eval("MessageTemplate") %>
     </div>
      </ItemTemplate>
         </asp:TemplateField>
          <asp:TemplateField HeaderText="Status">
         <ItemTemplate>
        <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
   <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
    </span>
      </ItemTemplate>
</asp:TemplateField>
      <asp:TemplateField HeaderText="Actions">
     <ItemTemplate>
   <asp:LinkButton ID="EditButton" runat="server" CssClass="btn btn-sm btn-info" 
   CommandName="EditTemplate" CommandArgument='<%# Eval("TemplateID") %>'>
<i class="fa fa-edit"></i> Edit
      </asp:LinkButton>
          <asp:LinkButton ID="DeleteButton" runat="server" CssClass="btn btn-sm btn-danger ml-1" 
       CommandName="DeleteTemplate" CommandArgument='<%# Eval("TemplateID") %>'
       OnClientClick="return confirm('Are you sure you want to delete this template?');">
           <i class="fa fa-trash"></i> Delete
        </asp:LinkButton>
        </ItemTemplate>
    </asp:TemplateField>
         </Columns>
           </asp:GridView>
   <asp:SqlDataSource ID="AttendanceTemplatesSQL" runat="server" 
     ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
       SelectCommand="SELECT TemplateID, TemplateName, TemplateType, MessageTemplate, IsActive, CreatedDate 
  FROM SMS_Template 
     WHERE SchoolID = @SchoolID AND TemplateCategory = 'Attendance'
        ORDER BY CreatedDate DESC">
       <SelectParameters>
          <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
     </SelectParameters>
     </asp:SqlDataSource>
         </div>

    <!-- Due SMS Tab -->
            <div class="tab-pane fade" id="due-panel" role="tabpanel">
           <div class="row">
          <div class="col-md-12">
         <div class="alert alert-danger">
         <strong>💸 Due SMS Templates:</strong><br />
 <span class="placeholder-tag">{StudentName}</span>
        <span class="placeholder-tag">{ID}</span>
  <span class="placeholder-tag">{TotalDue}</span>
   <span class="placeholder-tag">{DueDetails}</span>
   <span class="placeholder-tag">{SchoolName}</span>
         </div>
          </div>
       </div>

     <asp:Button ID="AddDueTemplateButton" runat="server" Text="+ Add New Due Template" 
    CssClass="btn btn-danger mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Due" />

           <asp:GridView ID="DueTemplatesGridView" runat="server" AutoGenerateColumns="False" 
           CssClass="table table-hover" DataSourceID="DueTemplatesSQL" DataKeyNames="TemplateID"
       OnRowCommand="TemplatesGridView_RowCommand">
  <Columns>
          <asp:BoundField DataField="TemplateName" HeaderText="Template Name" />
      <asp:BoundField DataField="TemplateType" HeaderText="Type" />
          <asp:TemplateField HeaderText="Message Preview">
                <ItemTemplate>
                <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
         <%# Eval("MessageTemplate") %>
       </div>
       </ItemTemplate>
         </asp:TemplateField>
     <asp:TemplateField HeaderText="Status">
        <ItemTemplate>
    <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
        <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
     </span>
         </ItemTemplate>
        </asp:TemplateField>
            <asp:TemplateField HeaderText="Actions">
      <ItemTemplate>
        <asp:LinkButton ID="EditButton" runat="server" CssClass="btn btn-sm btn-info" 
       CommandName="EditTemplate" CommandArgument='<%# Eval("TemplateID") %>'>
   <i class="fa fa-edit"></i> Edit
        </asp:LinkButton>
     <asp:LinkButton ID="DeleteButton" runat="server" CssClass="btn btn-sm btn-danger ml-1" 
 CommandName="DeleteTemplate" CommandArgument='<%# Eval("TemplateID") %>'
     OnClientClick="return confirm('Are you sure you want to delete this template?');">
          <i class="fa fa-trash"></i> Delete
           </asp:LinkButton>
      </ItemTemplate>
    </asp:TemplateField>
     </Columns>
     </asp:GridView>
    <asp:SqlDataSource ID="DueTemplatesSQL" runat="server" 
            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
     SelectCommand="SELECT TemplateID, TemplateName, TemplateType, MessageTemplate, IsActive, CreatedDate 
         FROM SMS_Template 
   WHERE SchoolID = @SchoolID AND TemplateCategory = 'Due'
               ORDER BY CreatedDate DESC">
 <SelectParameters>
  <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
    </SelectParameters>
                </asp:SqlDataSource>
            </div>

            <!-- Admission Tab -->
            <div class="tab-pane fade" id="admission-panel" role="tabpanel">
       <div class="row">
   <div class="col-md-12">
           <div class="alert alert-primary">
    <strong>🎓 Admission Templates:</strong><br />
     <span class="placeholder-tag">{StudentName}</span>
   <span class="placeholder-tag">{ID}</span>
     <span class="placeholder-tag">{Class}</span>
             <span class="placeholder-tag">{RollNo}</span>
      <span class="placeholder-tag">{AdmissionDate}</span>
     <span class="placeholder-tag">{SchoolName}</span>
         </div>
               </div>
         </div>

       <asp:Button ID="AddAdmissionTemplateButton" runat="server" Text="+ Add New Admission Template" 
     CssClass="btn btn-primary mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Admission" />

     <asp:GridView ID="AdmissionTemplatesGridView" runat="server" AutoGenerateColumns="False" 
            CssClass="table table-hover" DataSourceID="AdmissionTemplatesSQL" DataKeyNames="TemplateID"
          OnRowCommand="TemplatesGridView_RowCommand">
            <Columns>
<asp:BoundField DataField="TemplateName" HeaderText="Template Name" />
        <asp:BoundField DataField="TemplateType" HeaderText="Type" />
          <asp:TemplateField HeaderText="Message Preview">
          <ItemTemplate>
                  <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
              <%# Eval("MessageTemplate") %>
   </div>
     </ItemTemplate>
  </asp:TemplateField>
  <asp:TemplateField HeaderText="Status">
        <ItemTemplate>
     <span class='<%# Convert.ToBoolean(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
    <%# Convert.ToBoolean(Eval("IsActive")) ? "Active" : "Inactive" %>
      </span>
</ItemTemplate>
           </asp:TemplateField>
     <asp:TemplateField HeaderText="Actions">
  <ItemTemplate>
   <asp:LinkButton ID="EditButton" runat="server" CssClass="btn btn-sm btn-info" 
              CommandName="EditTemplate" CommandArgument='<%# Eval("TemplateID") %>'>
         <i class="fa fa-edit"></i> Edit
   </asp:LinkButton>
                   <asp:LinkButton ID="DeleteButton" runat="server" CssClass="btn btn-sm btn-danger ml-1" 
   CommandName="DeleteTemplate" CommandArgument='<%# Eval("TemplateID") %>'
                 OnClientClick="return confirm('Are you sure you want to delete this template?');">
                  <i class="fa fa-trash"></i> Delete
     </asp:LinkButton>
    </ItemTemplate>
          </asp:TemplateField>
   </Columns>
        </asp:GridView>
   <asp:SqlDataSource ID="AdmissionTemplatesSQL" runat="server" 
           ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
   SelectCommand="SELECT TemplateID, TemplateName, TemplateType, MessageTemplate, IsActive, CreatedDate 
         FROM SMS_Template 
                WHERE SchoolID = @SchoolID AND TemplateCategory = 'Admission'
     ORDER BY CreatedDate DESC">
          <SelectParameters>
        <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
              </SelectParameters>
     </asp:SqlDataSource>
            </div>
    </div>
    </div>

    <!-- Edit/Create Modal -->
    <div class="modal fade" id="templateModal" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white">
       <h5 class="modal-title">
 <asp:Label ID="FormTitleLabel" runat="server" Text="Create New Template"></asp:Label>
                    </h5>
         <button type="button" class="close text-white" data-dismiss="modal">
             <span>&times;</span>
          </button>
    </div>
    <div class="modal-body">
     <asp:HiddenField ID="TemplateIDHiddenField" runat="server" Value="0" />
      <asp:HiddenField ID="CurrentCategoryHiddenField" runat="server" />

          <div class="form-group">
       <label>Template Name (টেমপ্লেটের নাম) <span class="text-danger">*</span></label>
        <asp:TextBox ID="TemplateNameTextBox" runat="server" CssClass="form-control" 
        placeholder="e.g., Monthly Exam Result"></asp:TextBox>
          <asp:RequiredFieldValidator ID="TemplateNameRequired" runat="server" 
     ControlToValidate="TemplateNameTextBox" ErrorMessage="Template name is required" 
       CssClass="text-danger" Display="Dynamic" ValidationGroup="TemplateSave"></asp:RequiredFieldValidator>
        </div>

        <div class="form-group">
         <label>Template Category (ক্যাটাগরি) <span class="text-danger">*</span></label>
  <asp:DropDownList ID="TemplateCategoryDropDownList" runat="server" CssClass="form-control" 
          AutoPostBack="True" OnSelectedIndexChanged="TemplateCategoryDropDownList_SelectedIndexChanged">
      <asp:ListItem Value="ExamResult">📝 Exam Result (পরীক্ষার ফলাফল)</asp:ListItem>
       <asp:ListItem Value="Payment">💰 Payment (পেমেন্ট)</asp:ListItem>
        <asp:ListItem Value="Attendance">📅 Attendance (উপস্থিতি)</asp:ListItem>
             <asp:ListItem Value="Due">💸 Due SMS (বকেয়া এসএমএস)</asp:ListItem>
         <asp:ListItem Value="Admission">🎓 Admission (ভর্তি)</asp:ListItem>
  </asp:DropDownList>
            </div>

      <div class="form-group">
         <label>Template Type (টেমপ্লেট টাইপ) <span class="text-danger">*</span></label>
 <asp:DropDownList ID="TemplateTypeDropDownList" runat="server" CssClass="form-control">
   </asp:DropDownList>
        </div>

        <div class="form-group">
     <label>Message Template (মেসেজ টেমপ্লেট) <span class="text-danger">*</span></label>
               <asp:TextBox ID="MessageTemplateTextBox" runat="server" CssClass="form-control" 
 TextMode="MultiLine" Rows="6" onkeyup="updatePreview()"></asp:TextBox>
                <asp:RequiredFieldValidator ID="MessageTemplateRequired" runat="server" 
           ControlToValidate="MessageTemplateTextBox" ErrorMessage="Message template is required" 
      CssClass="text-danger" Display="Dynamic" ValidationGroup="TemplateSave"></asp:RequiredFieldValidator>
       <small class="form-text text-muted">
   Character Count: <span id="charCount">0</span> / 450
                </small>
         </div>

 <div class="form-group">
                  <div class="custom-control custom-checkbox">
          <asp:CheckBox ID="IsActiveCheckBox" runat="server" Checked="true" 
  CssClass="custom-control-input" />
        <label class="custom-control-label">Active Template</label>
         </div>
            </div>

        <div class="card">
         <div class="card-header bg-light">
        <strong>Preview (পুব্বরূপ)</strong>
    </div>
     <div class="card-body">
 <asp:Label ID="PreviewLabel" runat="server" CssClass="text-muted" 
   Text="Template preview will appear here..."></asp:Label>
</div>
     </div>
       </div>
      <div class="modal-footer">
     <button type="button" class="btn btn-secondary" data-dismiss="modal">CANCEL</button>
 <asp:Button ID="SaveButton" runat="server" Text="SAVE TEMPLATE" 
      CssClass="btn btn-primary" OnClick="SaveButton_Click" 
 ValidationGroup="TemplateSave" CausesValidation="true" />
                </div>
      </div>
        </div>
    </div>

    <script type="text/javascript">
        // Character counter
        $(document).ready(function () {
        $('#<%= MessageTemplateTextBox.ClientID %>').on('input', function () {
            var length = $(this).val().length;
  $('#charCount').text(length);
        updatePreview();
        });
        });

        // Auto-update preview
        function updatePreview() {
     var template = $('#<%= MessageTemplateTextBox.ClientID %>').val();
var category = $('#<%= TemplateCategoryDropDownList.ClientID %>').val();
            
  var preview = template;

     // Replace placeholders based on category
  if (category === 'ExamResult') {
  preview = preview
      .replace(/{StudentName}/g, 'আব্দুস সাত্তার')
 .replace(/{ID}/g, '12345')
      .replace(/{ExamName}/g, 'Half Yearly Exam')
   .replace(/{TotalMarks}/g, '850.00')
      .replace(/{Grade}/g, 'A+')
      .replace(/{Point}/g, '5.00')
     .replace(/{ClassPosition}/g, '1st')
       .replace(/{SectionPosition}/g, '2nd')
  .replace(/{SchoolName}/g, 'Your School Name');
 } else if (category === 'Payment') {
   preview = preview
  .replace(/{StudentName}/g, 'আব্দুস সাত্তার')
    .replace(/{ID}/g, '12345')
   .replace(/{Amount}/g, '5000')
      .replace(/{ReceiptNo}/g, 'MR-2024-001')
   .replace(/{PaymentDetails}/g, 'January Tuition Fee, Exam Fee')
   .replace(/{CurrentDue}/g, '15000')
 .replace(/{SchoolName}/g, 'Your School Name');
 } else if (category === 'Attendance') {
  preview = preview
     .replace(/{StudentName}/g, 'আব্দুস সাত্তার')
 .replace(/{ID}/g, '12345')
  .replace(/{Date}/g, '২০২৪-০১-১৫')
         .replace(/{Status}/g, 'Present')
     .replace(/{SchoolName}/g, 'Your School Name');
            } else if (category === 'Due') {
       preview = preview
     .replace(/{StudentName}/g, 'আব্দুস সাত্তার')
       .replace(/{ID}/g, '12345')
     .replace(/{TotalDue}/g, '25000.00')
    .replace(/{DueDetails}/g, 'Tuition Fee: 15000 Tk, Exam Fee: 5000 Tk, Transport: 5000 Tk')
   .replace(/{SchoolName}/g, 'Your School Name');
      } else if (category === 'Admission') {
       preview = preview
  .replace(/{StudentName}/g, 'আব্দুস সাত্তার')
.replace(/{ID}/g, '12345')
           .replace(/{Class}/g, 'Class 10')
   .replace(/{RollNo}/g, '05')
          .replace(/{AdmissionDate}/g, '15 Jan 2025')
     .replace(/{SchoolName}/g, 'Your School Name');
    }
  
         $('#<%= PreviewLabel.ClientID %>').text(preview || 'Template preview will appear here...');
        }

        // Show modal function
        function showTemplateModal() {
     $('#templateModal').modal('show');
            setTimeout(function() {
   updatePreview();
     }, 300);
        }

        // Close modal function - NO PAGE RELOAD
        function closeTemplateModal() {
    $('#templateModal').modal('hide');
      // Clear form after closing
         setTimeout(function() {
          $('#<%= TemplateNameTextBox.ClientID %>').val('');
   $('#<%= MessageTemplateTextBox.ClientID %>').val('');
        $('#charCount').text('0');
}, 500);
 }

      // Prevent form resubmission on page refresh
        if (window.history.replaceState) {
     window.history.replaceState(null, null, window.location.href);
        }
    </script>
</asp:Content>
