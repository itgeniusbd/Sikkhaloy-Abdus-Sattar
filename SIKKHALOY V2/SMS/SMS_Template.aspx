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

        .template-guide-table {
            width: 100%;
            margin-top: 12px;
            background: #fff;
            border-radius: 6px;
            overflow: hidden;
        }

        .template-guide-table th {
            font-size: 13px;
            padding: 8px 12px;
        }

        .template-guide-table td {
            font-size: 13px;
            padding: 8px 12px;
            border: 1px solid #dee2e6;
            vertical-align: top;
        }

        .guide-thead-info th { background: #d1ecf1; color: #0c5460; border-color: #bee5eb !important; }
        .guide-thead-success th { background: #d4edda; color: #155724; border-color: #c3e6cb !important; }
        .guide-thead-danger th { background: #f8d7da; color: #721c24; border-color: #f5c6cb !important; }
        .guide-thead-warning th { background: #fff3cd; color: #856404; border-color: #ffeeba !important; }
        .guide-thead-primary th { background: #cce5ff; color: #004085; border-color: #b8daff !important; }

        .placeholder-panel {
            background: #f0f7ff;
            border: 1px solid #cce0ff;
            border-radius: 6px;
            padding: 8px 10px;
            margin-bottom: 8px;
        }

        .placeholder-panel .placeholder-desc {
            font-size: 12px;
            color: #555;
            margin-bottom: 6px;
            line-height: 1.4;
        }

        .placeholder-panel .placeholder-tag {
            padding: 2px 8px;
            font-size: 11px;
            margin: 2px;
        }

        .modal-compact .modal-body {
            padding: 16px 20px;
        }

        .modal-compact .form-group {
            margin-bottom: 12px;
        }

        .preview-box {
            background: #f8f9fa;
            border: 1px solid #dee2e6;
            border-radius: 6px;
            padding: 10px 12px;
            font-size: 13px;
            min-height: 48px;
            color: #495057;
        }

        .modal-footer-bar {
            display: flex;
            align-items: center;
            justify-content: space-between;
            margin-top: 4px;
        }

        /* Override global checkbox hide (basic-style.css) inside modal */
        #templateModal input[type=checkbox] {
            display: inline-block !important;
            opacity: 1 !important;
            position: relative !important;
            pointer-events: auto !important;
            width: 16px !important;
            height: 16px !important;
            margin: 0 6px 0 0 !important;
            vertical-align: middle;
            -webkit-appearance: checkbox !important;
            appearance: checkbox !important;
        }

        #templateModal input[type=checkbox] + label,
        #templateModal label.active-template-label {
            background-image: none !important;
            padding-left: 0 !important;
            height: auto !important;
            line-height: normal !important;
            cursor: pointer;
        }

        .active-template-label {
            font-size: 13px;
            margin-bottom: 0;
            cursor: pointer;
            user-select: none;
        }

        .placeholder-hint {
            cursor: pointer;
        }

        .placeholder-hint:hover {
            background: #bbdefb;
        }

        .category-locked-note {
            display: none;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <div class="container-fluid">
        <h2 class="mb-4">
            <i class="fa fa-comments"></i> SMS TEMPLATE MANAGEMENT (সকল ধরনের SMS টেমপ্লেট ম্যানেজমেন্ট)
        </h2>

                <asp:HiddenField ID="ActiveTabHiddenField" runat="server" Value="exam-tab" />
                <asp:HiddenField ID="OpenModalAfterPostbackHiddenField" runat="server" Value="0" />

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
                <a class="nav-link" id="donor-tab" data-toggle="tab" href="#donor-panel" role="tab">
                    <span class="tab-icon">🤝</span> Donor
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
                            <strong>📝 পরীক্ষার ফলাফল SMS — কোন SMS কখন যাবে?</strong>
                            <table class="template-guide-table table table-sm table-bordered mb-0 guide-thead-info">
                                <thead>
                                    <tr>
                                        <th style="width:18%">টাইপ</th>
                                        <th style="width:32%">কখন SMS পাঠানো হয়</th>
                                        <th>ব্যবহারযোগ্য প্লেসহোল্ডার</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>✅ <strong>Passed</strong><br /><small class="text-muted">পাস</small></td>
                                        <td>শিক্ষার্থী পরীক্ষায় <strong>পাস</strong> করলে ফলাফল SMS</td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{ExamName}</span><span class="placeholder-tag">{TotalMarks}</span><span class="placeholder-tag">{Grade}</span><span class="placeholder-tag">{Point}</span><span class="placeholder-tag">{ClassPosition}</span><span class="placeholder-tag">{SectionPosition}</span><span class="placeholder-tag">{SchoolName}</span></td>
                                    </tr>
                                    <tr>
                                        <td>❌ <strong>Failed</strong><br /><small class="text-muted">ফেল</small></td>
                                        <td>শিক্ষার্থী পরীক্ষায় <strong>ফেল</strong> করলে ফলাফল SMS</td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{ExamName}</span><span class="placeholder-tag">{TotalMarks}</span><span class="placeholder-tag">{Grade}</span><span class="placeholder-tag">{Point}</span><span class="placeholder-tag">{ClassPosition}</span><span class="placeholder-tag">{SectionPosition}</span><span class="placeholder-tag">{SchoolName}</span></td>
                                    </tr>
                                </tbody>
                            </table>
                            <small class="text-muted d-block mt-2">💡 পাস ও ফেলের জন্য আলাদা টেমপ্লেট রাখতে পারেন।</small>
                        </div>
                    </div>
                </div>

                <asp:Button ID="AddExamTemplateButton" runat="server" Text="+ Add New Exam Template" 
                    CssClass="btn btn-primary mb-3" OnClick="AddNewTemplate_Click" CommandArgument="ExamResult" />

                <asp:GridView ID="ExamTemplatesGridView" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover" DataSourceID="ExamTemplatesSQL" DataKeyNames="TemplateID"
                    OnRowCommand="TemplatesGridView_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="TemplateName" HeaderText="Template Name (নাম)" />
                        <asp:TemplateField HeaderText="Type (টাইপ)">
                            <ItemTemplate>
                                <%# GetTemplateTypeDisplayName("ExamResult", Eval("TemplateType").ToString()) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Message Preview">
                            <ItemTemplate>
                                <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                    <%# Eval("MessageTemplate") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# IsTemplateActive(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
                                    <%# IsTemplateActive(Eval("IsActive")) ? "Active" : "Inactive" %>
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
                            <strong>💰 পেমেন্ট SMS — ফি জমা হলে রিসিট SMS</strong>
                            <table class="template-guide-table table table-sm table-bordered mb-0 guide-thead-success">
                                <thead>
                                    <tr>
                                        <th style="width:18%">টাইপ</th>
                                        <th style="width:32%">কখন SMS পাঠানো হয়</th>
                                        <th>ব্যবহারযোগ্য প্লেসহোল্ডার</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>💰 <strong>Payment</strong><br /><small class="text-muted">পেমেন্ট রিসিট</small></td>
                                        <td>শিক্ষার্থীর <strong>ফি জমা</strong> হলে Money Receipt SMS পাঠানো হয়</td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{Amount}</span><span class="placeholder-tag">{ReceiptNo}</span><span class="placeholder-tag">{PaymentDetails}</span><span class="placeholder-tag">{Session}</span><span class="placeholder-tag">{CurrentDue}</span><span class="placeholder-tag">{SchoolName}</span></td>
                                    </tr>
                                </tbody>
                            </table>
                            <small class="text-muted d-block mt-2">💡 Payment Collection পেজ থেকে ফি জমার পর এই SMS যায়।</small>
                        </div>
                    </div>
                </div>

                <asp:Button ID="AddPaymentTemplateButton" runat="server" Text="+ Add New Payment Template" 
                    CssClass="btn btn-success mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Payment" />

                <asp:GridView ID="PaymentTemplatesGridView" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover" DataSourceID="PaymentTemplatesSQL" DataKeyNames="TemplateID"
                    OnRowCommand="TemplatesGridView_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="TemplateName" HeaderText="Template Name (নাম)" />
                        <asp:TemplateField HeaderText="Type (টাইপ)">
                            <ItemTemplate>
                                <%# GetTemplateTypeDisplayName("Payment", Eval("TemplateType").ToString()) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Message Preview">
                            <ItemTemplate>
                                <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                    <%# Eval("MessageTemplate") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# IsTemplateActive(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
                                    <%# IsTemplateActive(Eval("IsActive")) ? "Active" : "Inactive" %>
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

            <!-- Due SMS Tab -->
            <div class="tab-pane fade" id="due-panel" role="tabpanel">
                <div class="row">
                    <div class="col-md-12">
                        <div class="alert alert-danger">
                            <strong>💸 বকেয়া SMS — কোন SMS কখন যাবে?</strong>
                            <table class="template-guide-table table table-sm table-bordered mb-0 guide-thead-danger">
                                <thead>
                                    <tr>
                                        <th style="width:18%">টাইপ</th>
                                        <th style="width:32%">কখন SMS পাঠানো হয়</th>
                                        <th>ব্যবহারযোগ্য প্লেসহোল্ডার</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>💸 <strong>Due</strong><br /><small class="text-muted">বকেয়া নোটিফিকেশন</small></td>
                                        <td>শিক্ষার্থীর <strong>বকেয়া ফি</strong> থাকলে নোটিফিকেশন SMS</td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{TotalDue}</span><span class="placeholder-tag">{DueDetails}</span><span class="placeholder-tag">{SchoolName}</span></td>
                                    </tr>
                                </tbody>
                            </table>
                            <small class="text-muted d-block mt-2">💡 Present Due পেজ থেকে বকেয়া SMS পাঠানো হয়।</small>
                        </div>
                    </div>
                </div>

                <asp:Button ID="AddDueTemplateButton" runat="server" Text="+ Add New Due Template" 
                    CssClass="btn btn-danger mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Due" />

                <asp:GridView ID="DueTemplatesGridView" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover" DataSourceID="DueTemplatesSQL" DataKeyNames="TemplateID"
                    OnRowCommand="TemplatesGridView_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="TemplateName" HeaderText="Template Name (নাম)" />
                        <asp:TemplateField HeaderText="Type (টাইপ)">
                            <ItemTemplate>
                                <%# GetTemplateTypeDisplayName("Due", Eval("TemplateType").ToString()) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Message Preview">
                            <ItemTemplate>
                                <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                    <%# Eval("MessageTemplate") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# IsTemplateActive(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
                                    <%# IsTemplateActive(Eval("IsActive")) ? "Active" : "Inactive" %>
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

            <!-- Donor Tab -->
            <div class="tab-pane fade" id="donor-panel" role="tabpanel">
                <div class="row">
                    <div class="col-md-12">
                        <div class="alert alert-info">
                            <strong>🤝 ডোনার SMS — কোন SMS কখন যাবে?</strong>
                            <table class="template-guide-table table table-sm table-bordered mb-0 guide-thead-info">
                                <thead>
                                    <tr>
                                        <th style="width:18%">টাইপ</th>
                                        <th style="width:32%">কখন SMS পাঠানো হয়</th>
                                        <th>ব্যবহারযোগ্য প্লেসহোল্ডার</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>💸 <strong>DonorDue</strong><br /><small class="text-muted">বকেয়া</small></td>
                                        <td>ডোনারের <strong>বকেয়া চাঁদা</strong> থাকলে</td>
                                        <td><span class="placeholder-tag">{DonorName}</span><span class="placeholder-tag">{TotalDue}</span><span class="placeholder-tag">{DueDetails}</span><span class="placeholder-tag">{SchoolName}</span></td>
                                    </tr>
                                    <tr>
                                        <td>✅ <strong>DonorPayment</strong><br /><small class="text-muted">পেমেন্ট</small></td>
                                        <td>ডোনার <strong>চাঁদা/দান জমা</strong> দিলে রিসিট SMS</td>
                                        <td><span class="placeholder-tag">{DonorName}</span><span class="placeholder-tag">{Amount}</span><span class="placeholder-tag">{ReceiptNo}</span><span class="placeholder-tag">{PaymentDetails}</span><span class="placeholder-tag">{CurrentDue}</span><span class="placeholder-tag">{SchoolName}</span></td>
                                    </tr>
                                </tbody>
                            </table>
                            <small class="text-muted d-block mt-2">💡 Donor Payment ও Donor Present Due পেজ থেকে SMS পাঠানো হয়।</small>
                        </div>
                    </div>
                </div>

                <asp:Button ID="AddDonorTemplateButton" runat="server" Text="+ Add New Donor Template" 
                    CssClass="btn btn-info mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Donor" />

                <asp:GridView ID="DonorTemplatesGridView" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover" DataSourceID="DonorTemplatesSQL" DataKeyNames="TemplateID"
                    OnRowCommand="TemplatesGridView_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="TemplateName" HeaderText="Template Name (নাম)" />
                        <asp:TemplateField HeaderText="Type (টাইপ)">
                            <ItemTemplate>
                                <%# GetTemplateTypeDisplayName("Donor", Eval("TemplateType").ToString()) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Message Preview">
                            <ItemTemplate>
                                <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                    <%# Eval("MessageTemplate") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# IsTemplateActive(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
                                    <%# IsTemplateActive(Eval("IsActive")) ? "Active" : "Inactive" %>
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
                <asp:SqlDataSource ID="DonorTemplatesSQL" runat="server" 
                    ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                    SelectCommand="SELECT TemplateID, TemplateName, TemplateType, MessageTemplate, IsActive, CreatedDate 
                        FROM SMS_Template 
                        WHERE SchoolID = @SchoolID AND TemplateCategory = 'Donor'
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
                            <strong>📅 হাজিরা SMS টেমপ্লেট — কোন SMS কখন যাবে?</strong>
                            <table class="template-guide-table table table-sm table-bordered mb-0 guide-thead-warning">
                                <thead>
                                    <tr>
                                        <th style="width:18%">টাইপ</th>
                                        <th style="width:32%">কখন SMS পাঠানো হয়</th>
                                        <th>ব্যবহারযোগ্য প্লেসহোল্ডার</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>✅ <strong>Entry</strong><br /><small class="text-muted">প্রবেশ</small></td>
                                        <td>শিক্ষার্থী <strong>সময়মতো</strong> স্কুলে প্রবেশ করলে</td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{EntryTime}</span><span class="placeholder-tag">{Date}</span><span class="placeholder-tag">{ScheduleName}</span><span class="placeholder-tag">{SchoolName}</span><span class="placeholder-tag">{Class}</span><span class="placeholder-tag">{Roll}</span></td>
                                    </tr>
                                    <tr>
                                        <td>🚪 <strong>Exit</strong><br /><small class="text-muted">প্রস্থান</small></td>
                                        <td>শিক্ষার্থী স্কুল <strong>ত্যাগ</strong> করলে</td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{ExitTime}</span><span class="placeholder-tag">{Date}</span><span class="placeholder-tag">{ScheduleName}</span><span class="placeholder-tag">{SchoolName}</span><span class="placeholder-tag">{Class}</span><span class="placeholder-tag">{Roll}</span></td>
                                    </tr>
                                    <tr>
                                        <td>⏰ <strong>Late</strong><br /><small class="text-muted">দেরি</small></td>
                                        <td>শিক্ষার্থী <strong>দেরিতে</strong> এলে (কিন্তু উপস্থিত)</td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{EntryTime}</span><span class="placeholder-tag">{LateMinutes}</span><span class="placeholder-tag">{Date}</span><span class="placeholder-tag">{ScheduleName}</span><span class="placeholder-tag">{SchoolName}</span><span class="placeholder-tag">{Class}</span><span class="placeholder-tag">{Roll}</span></td>
                                    </tr>
                                    <tr>
                                        <td>❌ <strong>Absent</strong><br /><small class="text-muted">অনুপস্থিত</small></td>
                                        <td>শিক্ষার্থী স্কুলে <strong>আসেনি</strong></td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{Date}</span><span class="placeholder-tag">{ScheduleName}</span><span class="placeholder-tag">{SchoolName}</span><span class="placeholder-tag">{Class}</span><span class="placeholder-tag">{Roll}</span></td>
                                    </tr>
                                </tbody>
                            </table>
                            <small class="text-muted d-block mt-2">💡 প্রতিটি টাইপের জন্য একটি করে সক্রিয় টেমপ্লেট রাখুন। নতুন টেমপ্লেট যোগ করতে নিচের বাটনে ক্লিক করুন।</small>
                        </div>
                    </div>
                </div>

                <asp:Button ID="AddAttendanceTemplateButton" runat="server" Text="+ Add New Attendance Template" 
                    CssClass="btn btn-warning mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Attendance" />

                <asp:GridView ID="AttendanceTemplatesGridView" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover" DataSourceID="AttendanceTemplatesSQL" DataKeyNames="TemplateID"
                    OnRowCommand="TemplatesGridView_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="TemplateName" HeaderText="Template Name (নাম)" />
                        <asp:TemplateField HeaderText="Type (টাইপ)">
                            <ItemTemplate>
                                <%# GetTemplateTypeDisplayName("Attendance", Eval("TemplateType").ToString()) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Message Preview">
                            <ItemTemplate>
                                <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                    <%# Eval("MessageTemplate") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# IsTemplateActive(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
                                    <%# IsTemplateActive(Eval("IsActive")) ? "Active" : "Inactive" %>
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

            <!-- Admission Tab -->
            <div class="tab-pane fade" id="admission-panel" role="tabpanel">
                <div class="row">
                    <div class="col-md-12">
                        <div class="alert alert-primary">
                            <strong>🎓 ভর্তি SMS — কোন SMS কখন যাবে?</strong>
                            <table class="template-guide-table table table-sm table-bordered mb-0 guide-thead-primary">
                                <thead>
                                    <tr>
                                        <th style="width:18%">টাইপ</th>
                                        <th style="width:32%">কখন SMS পাঠানো হয়</th>
                                        <th>ব্যবহারযোগ্য প্লেসহোল্ডার</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr>
                                        <td>🎓 <strong>Confirm</strong><br /><small class="text-muted">নিশ্চিতকরণ</small></td>
                                        <td>নতুন শিক্ষার্থীর <strong>ভর্তি সম্পন্ন</strong> হলে</td>
                                        <td><span class="placeholder-tag">{StudentName}</span><span class="placeholder-tag">{ID}</span><span class="placeholder-tag">{Class}</span><span class="placeholder-tag">{RollNo}</span><span class="placeholder-tag">{AdmissionDate}</span><span class="placeholder-tag">{SchoolName}</span></td>
                                    </tr>
                                </tbody>
                            </table>
                            <small class="text-muted d-block mt-2">💡 New Student Admission পেজ থেকে ভর্তির পর SMS পাঠানো হয়।</small>
                        </div>
                    </div>
                </div>

                <asp:Button ID="AddAdmissionTemplateButton" runat="server" Text="+ Add New Admission Template" 
                    CssClass="btn btn-primary mb-3" OnClick="AddNewTemplate_Click" CommandArgument="Admission" />

                <asp:GridView ID="AdmissionTemplatesGridView" runat="server" AutoGenerateColumns="False" 
                    CssClass="table table-hover" DataSourceID="AdmissionTemplatesSQL" DataKeyNames="TemplateID"
                    OnRowCommand="TemplatesGridView_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="TemplateName" HeaderText="Template Name (নাম)" />
                        <asp:TemplateField HeaderText="Type (টাইপ)">
                            <ItemTemplate>
                                <%# GetTemplateTypeDisplayName("Admission", Eval("TemplateType").ToString()) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Message Preview">
                            <ItemTemplate>
                                <div style="max-width: 300px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">
                                    <%# Eval("MessageTemplate") %>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Status">
                            <ItemTemplate>
                                <span class='<%# IsTemplateActive(Eval("IsActive")) ? "badge badge-success" : "badge badge-secondary" %>'>
                                    <%# IsTemplateActive(Eval("IsActive")) ? "Active" : "Inactive" %>
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
    <div class="modal fade modal-compact" id="templateModal" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header bg-primary text-white py-2">
                    <h5 class="modal-title mb-0">
                        <asp:Label ID="FormTitleLabel" runat="server" Text="Create New Template"></asp:Label>
                    </h5>
                    <button type="button" class="close text-white" data-dismiss="modal">
                        <span>&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="TemplateIDHiddenField" runat="server" Value="0" />
                    <asp:HiddenField ID="CurrentCategoryHiddenField" runat="server" />
                    <asp:HiddenField ID="CurrentTemplateTypeHiddenField" runat="server" />
                    <asp:DropDownList ID="TemplateCategoryDropDownList" runat="server" EnableViewState="false" style="display:none;">
                        <asp:ListItem Value="ExamResult">ExamResult</asp:ListItem>
                        <asp:ListItem Value="Payment">Payment</asp:ListItem>
                        <asp:ListItem Value="Attendance">Attendance</asp:ListItem>
                        <asp:ListItem Value="Due">Due</asp:ListItem>
                        <asp:ListItem Value="Donor">Donor</asp:ListItem>
                        <asp:ListItem Value="Admission">Admission</asp:ListItem>
                    </asp:DropDownList>

                    <div class="row">
                        <div class="col-md-6" id="nameCol">
                            <div class="form-group mb-2">
                                <label class="mb-1">Template Name (নাম) <span class="text-danger">*</span></label>
                                <asp:TextBox ID="TemplateNameTextBox" runat="server" CssClass="form-control form-control-sm" 
                                    placeholder="e.g., Monthly Exam Result"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="TemplateNameRequired" runat="server" 
                                    ControlToValidate="TemplateNameTextBox" ErrorMessage="Template name is required" 
                                    CssClass="text-danger" Display="Dynamic" ValidationGroup="TemplateSave"></asp:RequiredFieldValidator>
                            </div>
                        </div>
                        <div class="col-md-6" id="typeGroup">
                            <div class="form-group mb-2">
                                <label class="mb-1" id="typeGroupLabel">Template Type (টাইপ) <span class="text-danger">*</span></label>
                                <asp:DropDownList ID="TemplateTypeDropDownList" runat="server" EnableViewState="false"
                                    CssClass="form-control form-control-sm">
                                </asp:DropDownList>
                                <small class="form-text text-muted" id="typeChangeHint">
                                    ভুল টাইপ দিয়ে থাকলে Edit-এ উপরের Dropdown থেকে সঠিক টাইপ সিলেক্ট করে Update Template চাপুন।
                                </small>
                            </div>
                        </div>
                    </div>

                    <div class="form-group mb-2">
                        <label class="mb-1">Message Template (মেসেজ) <span class="text-danger">*</span></label>
                        <asp:ValidationSummary ID="TemplateSaveValidationSummary" runat="server"
                            ValidationGroup="TemplateSave" CssClass="text-danger small mb-2" DisplayMode="BulletList" />
                        <div id="placeholderPanel" class="placeholder-panel" style="display:none;">
                            <div id="typeHelpDesc" class="placeholder-desc"></div>
                            <div id="clickablePlaceholders"></div>
                        </div>
                        <asp:TextBox ID="MessageTemplateTextBox" runat="server" CssClass="form-control form-control-sm" 
                            TextMode="MultiLine" Rows="4" onkeyup="updatePreview()"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="MessageTemplateRequired" runat="server" 
                            ControlToValidate="MessageTemplateTextBox" ErrorMessage="Message template is required" 
                            CssClass="text-danger" Display="Dynamic" ValidationGroup="TemplateSave"></asp:RequiredFieldValidator>
                        <div class="modal-footer-bar">
                            <small class="form-text text-muted mb-0">
                                <span id="charCount">0</span> / 450 অক্ষর
                            </small>
                            <label class="active-template-label mb-0">
                                <asp:CheckBox ID="IsActiveCheckBox" runat="server" Checked="true" />
                                Active Template — SMS-এ এই টেমপ্লেট ব্যবহার হবে
                            </label>
                        </div>
                    </div>

                    <div>
                        <label class="mb-1" style="font-size:13px; font-weight:500;">Preview (পূর্বরূপ)</label>
                        <div class="preview-box">
                            <asp:Label ID="PreviewLabel" runat="server" 
                                Text="মেসেজ লিখলে এখানে দেখাবে..."></asp:Label>
                        </div>
                    </div>
                </div>
                <div class="modal-footer py-2">
                    <button type="button" class="btn btn-secondary btn-sm" data-dismiss="modal">CANCEL</button>
                    <asp:Button ID="SaveButton" runat="server" Text="SAVE TEMPLATE" 
                        CssClass="btn btn-primary btn-sm" OnClick="SaveButton_Click" 
                        ValidationGroup="TemplateSave" CausesValidation="true"
                        OnClientClick="syncTemplateFieldsBeforeSave();" />
                </div>
            </div>
        </div>
    </div>

    </div>

    <script type="text/javascript">
        var categoryTypeInfo = {
            'ExamResult': {
                'Passed': {
                    title: '✅ Passed — পরীক্ষায় পাস',
                    desc: 'শিক্ষার্থী পরীক্ষায় পাস করলে এই ফলাফল SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{ExamName}', '{TotalMarks}', '{Grade}', '{Point}', '{ClassPosition}', '{SectionPosition}', '{SchoolName}']
                },
                'Failed': {
                    title: '❌ Failed — পরীক্ষায় ফেল',
                    desc: 'শিক্ষার্থী পরীক্ষায় ফেল করলে এই ফলাফল SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{ExamName}', '{TotalMarks}', '{Grade}', '{Point}', '{ClassPosition}', '{SectionPosition}', '{SchoolName}']
                }
            },
            'Payment': {
                'Payment': {
                    title: '💰 Payment — পেমেন্ট রিসিট',
                    desc: 'শিক্ষার্থীর ফি জমা হলে Money Receipt SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{Amount}', '{ReceiptNo}', '{PaymentDetails}', '{Session}', '{CurrentDue}', '{SchoolName}']
                }
            },
            'Attendance': {
                'Entry': {
                    title: '✅ Entry — স্কুলে প্রবেশ',
                    desc: 'শিক্ষার্থী সময়মতো স্কুলে প্রবেশ করলে এই SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{EntryTime}', '{Date}', '{ScheduleName}', '{SchoolName}', '{Class}', '{Roll}']
                },
                'Exit': {
                    title: '🚪 Exit — স্কুল ত্যাগ',
                    desc: 'শিক্ষার্থী স্কুল ত্যাগ করলে এই SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{ExitTime}', '{Date}', '{ScheduleName}', '{SchoolName}', '{Class}', '{Roll}']
                },
                'Late': {
                    title: '⏰ Late — দেরিতে আসা',
                    desc: 'শিক্ষার্থী দেরিতে এলে (কিন্তু উপস্থিত হিসেবে গণনা) এই SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{EntryTime}', '{LateMinutes}', '{Date}', '{ScheduleName}', '{SchoolName}', '{Class}', '{Roll}']
                },
                'Absent': {
                    title: '❌ Absent — অনুপস্থিত',
                    desc: 'শিক্ষার্থী স্কুলে আসেনি — অনুপস্থিত হিসেবে এই SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{Date}', '{ScheduleName}', '{SchoolName}', '{Class}', '{Roll}']
                }
            },
            'Due': {
                'Due': {
                    title: '💸 Due — বকেয়া নোটিফিকেশন',
                    desc: 'শিক্ষার্থীর বকেয়া ফি থাকলে এই SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{TotalDue}', '{DueDetails}', '{SchoolName}']
                }
            },
            'Donor': {
                'DonorDue': {
                    title: '💸 Donor Due — ডোনার বকেয়া',
                    desc: 'ডোনারের বকেয়া চাঁদা থাকলে এই SMS পাঠানো হয়।',
                    placeholders: ['{DonorName}', '{TotalDue}', '{DueDetails}', '{SchoolName}']
                },
                'DonorPayment': {
                    title: '✅ Donor Payment — পেমেন্ট রিসিট',
                    desc: 'ডোনার চাঁদা/দান জমা দিলে রিসিট SMS পাঠানো হয়।',
                    placeholders: ['{DonorName}', '{Amount}', '{ReceiptNo}', '{PaymentDetails}', '{CurrentDue}', '{SchoolName}']
                }
            },
            'Admission': {
                'AdmissionConfirm': {
                    title: '🎓 Confirm — ভর্তি নিশ্চিতকরণ',
                    desc: 'নতুন শিক্ষার্থীর ভর্তি সম্পন্ন হলে এই SMS পাঠানো হয়।',
                    placeholders: ['{StudentName}', '{ID}', '{Class}', '{RollNo}', '{AdmissionDate}', '{SchoolName}']
                }
            }
        };

        function shouldOpenTemplateModal() {
            return $('#<%= OpenModalAfterPostbackHiddenField.ClientID %>').val() === '1';
        }

        function handleTemplateModalAfterPostback() {
            if (shouldOpenTemplateModal()) {
                showTemplateModal();
            } else {
                forceCloseTemplateModal();
            }
        }

        function forceCloseTemplateModal() {
            var $modal = $('#templateModal');
            if ($modal.length) {
                $modal.removeClass('show').css('display', 'none').attr('aria-hidden', 'true');
            }
            $('body').removeClass('modal-open');
            $('body').css('padding-right', '');
            $('.modal-backdrop').remove();
        }

        function restoreActiveTab() {
            var tabId = $('#<%= ActiveTabHiddenField.ClientID %>').val() || 'exam-tab';
            var $tab = $('#' + tabId);
            if ($tab.length) {
                $tab.tab('show');
            }
        }

        function syncTemplateFieldsBeforeSave() {
            var selectedType = $('#<%= TemplateTypeDropDownList.ClientID %>').val();
            if (selectedType) {
                $('#<%= CurrentTemplateTypeHiddenField.ClientID %>').val(selectedType);
            }
            var category = getSelectedCategory();
            if (!category) {
                var activeTabId = $('#<%= ActiveTabHiddenField.ClientID %>').val();
                var tabCategoryMap = {
                    'exam-tab': 'ExamResult', 'payment-tab': 'Payment',
                    'due-tab': 'Due', 'donor-tab': 'Donor',
                    'attendance-tab': 'Attendance', 'admission-tab': 'Admission'
                };
                category = tabCategoryMap[activeTabId] || '';
            }
            if (category) {
                $('#<%= CurrentCategoryHiddenField.ClientID %>').val(category);
                $('#<%= TemplateCategoryDropDownList.ClientID %>').val(category);
            }
            var templateId = $('#<%= TemplateIDHiddenField.ClientID %>').val();
            if (!templateId) {
                $('#<%= TemplateIDHiddenField.ClientID %>').val('0');
            }
        }

        function bindTemplateModalEvents() {
            $('#<%= MessageTemplateTextBox.ClientID %>').off('input.template').on('input.template', function () {
                var length = $(this).val().length;
                $('#charCount').text(length);
                updatePreview();
            });

            $('#<%= TemplateTypeDropDownList.ClientID %>').off('change.template').on('change.template', function () {
                $('#<%= CurrentTemplateTypeHiddenField.ClientID %>').val($(this).val());
                updateTypeHelp();
            });

            $('#categoryTabs a[data-toggle="tab"]').off('shown.bs.tab.template').on('shown.bs.tab.template', function (e) {
                var tabId = $(e.target).attr('id');
                if (tabId) {
                    $('#<%= ActiveTabHiddenField.ClientID %>').val(tabId);
                }
            });
        }

        $(document).ready(function () {
            bindTemplateModalEvents();
            restoreActiveTab();
            handleTemplateModalAfterPostback();
            if (!shouldOpenTemplateModal()) {
                updateTypeHelp();
            }
        });

        function getSelectedCategory() {
            var hidden = $('#<%= CurrentCategoryHiddenField.ClientID %>').val();
            return hidden || $('#<%= TemplateCategoryDropDownList.ClientID %>').val();
        }

        function buildPlaceholderHtml(placeholders) {
            var phHtml = '';
            placeholders.forEach(function (ph) {
                phHtml += '<span class="placeholder-tag placeholder-hint" onclick="insertPlaceholder(\'' + ph + '\')" title="ক্লিক করে মেসেজে যোগ করুন">' + ph + '</span> ';
            });
            return phHtml;
        }

        function updateTypeHelp() {
            var category = getSelectedCategory();
            var typeDdl = $('#<%= TemplateTypeDropDownList.ClientID %>');
            var type = typeDdl.val();
            var typeCount = typeDdl.find('option').length;
            var catInfo = categoryTypeInfo[category];

            if (typeCount <= 1) {
                $('#nameCol').removeClass('col-md-6').addClass('col-md-12');
                $('#typeGroup').hide();
            } else {
                $('#nameCol').removeClass('col-md-12').addClass('col-md-6');
                $('#typeGroup').show();
            }

            var info = catInfo ? (catInfo[type] || catInfo[typeDdl.find('option:first').val()]) : null;

            if (info) {
                var phHtml = buildPlaceholderHtml(info.placeholders);
                $('#typeHelpDesc').html('<strong>' + info.title + '</strong> — ' + info.desc + ' <span class="text-muted">| ক্লিক করে যোগ করুন:</span>');
                $('#clickablePlaceholders').html(phHtml);
                $('#placeholderPanel').show();
            } else {
                $('#placeholderPanel').hide();
            }
            updatePreview();
        }

        function insertPlaceholder(placeholder) {
            var textbox = document.getElementById('<%= MessageTemplateTextBox.ClientID %>');
            var start = textbox.selectionStart;
            var end = textbox.selectionEnd;
            var text = textbox.value;
            textbox.value = text.substring(0, start) + placeholder + text.substring(end);
            textbox.focus();
            textbox.selectionStart = textbox.selectionEnd = start + placeholder.length;
            $('#charCount').text(textbox.value.length);
            updatePreview();
        }

        function updatePreview() {
            var template = $('#<%= MessageTemplateTextBox.ClientID %>').val();
            var category = getSelectedCategory();
            
            var preview = template;

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
                    .replace(/{Session}/g, '2024')
                    .replace(/{CurrentDue}/g, '15000')
                    .replace(/{SchoolName}/g, 'Your School Name');
            } else if (category === 'Attendance') {
                preview = preview
                    .replace(/{StudentName}/g, 'আব্দুস সাত্তার')
                    .replace(/{ID}/g, '12345')
                    .replace(/{Date}/g, '১৫ জানু ২০২৪')
                    .replace(/{EntryTime}/g, '৮:৩০ AM')
                    .replace(/{ExitTime}/g, '২:০০ PM')
                    .replace(/{LateMinutes}/g, '১৫')
                    .replace(/{SchoolName}/g, 'Your School Name')
                    .replace(/{Class}/g, 'দশম')
                    .replace(/{Roll}/g, '০৫');
            } else if (category === 'Due') {
                preview = preview
                    .replace(/{StudentName}/g, 'আব্দুস সাত্তার')
                    .replace(/{ID}/g, '12345')
                    .replace(/{TotalDue}/g, '25000.00')
                    .replace(/{DueDetails}/g, 'Tuition Fee: 15000 Tk, Exam Fee: 5000 Tk, Transport: 5000 Tk')
                    .replace(/{SchoolName}/g, 'Your School Name');
            } else if (category === 'Donor') {
                preview = preview
                    .replace(/{DonorName}/g, 'আব্দুস সাত্তার')
                    .replace(/{Amount}/g, '5000.00')
                    .replace(/{ReceiptNo}/g, 'DR-2024-001')
                    .replace(/{PaymentDetails}/g, 'মাসিক চাঁদা: January-2024, নির্মাণ ফান্ড')
                    .replace(/{TotalDue}/g, '13500.00')
                    .replace(/{CurrentDue}/g, '8500.00')
                    .replace(/{DueDetails}/g, 'মাসিক চাঁদা: June-26 - 2500 Tk, মাসিক চাঁদা: Dec-25 - 1000 Tk')
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
            
            $('#<%= PreviewLabel.ClientID %>').text(preview || 'মেসেজ লিখলে এখানে দেখাবে...');
        }

        function showTemplateModal() {
            restoreActiveTab();
            var $modal = $('#templateModal');
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('padding-right', '');
            $modal.modal({ backdrop: true, keyboard: true, show: true });
            var msgLen = $('#<%= MessageTemplateTextBox.ClientID %>').val().length;
            $('#charCount').text(msgLen);
            setTimeout(function () {
                updateTypeHelp();
            }, 200);
        }

        function closeTemplateModal() {
            forceCloseTemplateModal();
            setTimeout(function() {
                $('#<%= TemplateNameTextBox.ClientID %>').val('');
                $('#<%= MessageTemplateTextBox.ClientID %>').val('');
                $('#charCount').text('0');
                $('#nameCol').removeClass('col-md-12').addClass('col-md-6');
                $('#typeGroup').show();
                $('#placeholderPanel').hide();
            }, 500);
        }

        if (window.history.replaceState) {
            window.history.replaceState(null, null, window.location.href);
        }
    </script>
</asp:Content>
