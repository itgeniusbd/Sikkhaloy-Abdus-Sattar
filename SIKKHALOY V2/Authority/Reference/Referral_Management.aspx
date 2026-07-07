<%@ Page Title="Referral Management" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Referral_Management.aspx.cs" Inherits="EDUCATION.COM.Authority.Reference.Referral_Management" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .section-card {
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 1px 4px rgba(0,0,0,0.10);
            padding: 22px 24px 16px 24px;
            margin-bottom: 22px;
        }
        .section-title {
            font-size: 1.1rem;
            font-weight: 600;
            color: #2c3e50;
            margin-bottom: 14px;
            border-bottom: 2px solid #e9ecef;
            padding-bottom: 7px;
        }
        .badge-commission { background: #17a2b8; color: #fff; padding: 3px 10px; border-radius: 12px; font-size: 0.85rem; }
        .badge-active { background: #28a745; color: #fff; padding: 3px 10px; border-radius: 12px; font-size: 0.85rem; }
        .badge-expired { background: #dc3545; color: #fff; padding: 3px 10px; border-radius: 12px; font-size: 0.85rem; }
        .search-result-box { border: 1px solid #dee2e6; border-radius: 6px; max-height: 220px; overflow-y: auto; background: #fff; position: absolute; z-index: 1000; width: 100%; }
        .search-result-item { padding: 8px 14px; cursor: pointer; font-size: 0.95rem; }
        .search-result-item:hover { background: #e9ecef; }
        .ins-search-wrap { position: relative; }
        .selected-ins-badge { background: #e3f2fd; border: 1px solid #90caf9; border-radius: 6px; padding: 6px 12px; display: inline-block; margin: 4px 0; font-size: 0.95rem; }
        .datepicker { z-index: 2000 !important; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3><i class="fa fa-handshake-o"></i> Referral Management</h3>

    <asp:UpdatePanel ID="MainUpdatePanel" runat="server">
        <ContentTemplate>

            <%-- ===== SECTION 1: Add / Update Referrer ===== --%>
            <div class="section-card">
                <div class="section-title"><i class="fa fa-user-plus"></i> Add / Update Referrer</div>
                <div class="row">
                    <div class="col-md-3">
                        <div class="form-group">
                            <label>Name <span class="text-danger">*</span></label>
                            <asp:TextBox ID="RefNameTextBox" runat="server" CssClass="form-control" placeholder="Enter referrer name"></asp:TextBox>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="form-group">
                            <label>Phone Number</label>
                            <asp:TextBox ID="RefPhoneTextBox" runat="server" CssClass="form-control" placeholder="01XXXXXXXXX"></asp:TextBox>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="form-group">
                            <label>Address</label>
                            <asp:TextBox ID="RefAddressTextBox" runat="server" CssClass="form-control" placeholder="Address"></asp:TextBox>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="form-group">
                            <label>Start Date</label>
                            <div class="input-group date ref-datepicker">
                                <asp:TextBox ID="RefStartDateTextBox" runat="server" CssClass="form-control" placeholder="dd MMM yyyy" autocomplete="off"></asp:TextBox>
                                <div class="input-group-append">
                                    <span class="input-group-text"><i class="fa fa-calendar"></i></span>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-2">
                        <div class="form-group">
                            <label>&nbsp;</label><br />
                            <asp:Button ID="SaveRefButton" runat="server" CssClass="btn btn-primary btn-block" Text="Save Referrer" OnClick="SaveRefButton_Click" />
                            <asp:HiddenField ID="EditReferenceIDHidden" runat="server" Value="0" />
                        </div>
                    </div>
                </div>
                <asp:Label ID="RefMsgLabel" runat="server" CssClass="text-success font-weight-bold"></asp:Label>
            </div>

            <%-- ===== SECTION 2: Referrer List ===== --%>
            <div class="section-card">
                <div class="section-title"><i class="fa fa-list"></i> Referrer List</div>
                <asp:GridView ID="ReferrerGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid"
                    DataKeyNames="ReferenceID" OnSelectedIndexChanged="ReferrerGridView_SelectedIndexChanged"
                    OnRowCommand="ReferrerGridView_RowCommand">
                    <Columns>
                        <asp:CommandField HeaderText="Action" ShowSelectButton="True" SelectText="<i class='fa fa-eye'></i> View" />
                        <asp:BoundField DataField="Reference_SN" HeaderText="SL" />
                        <asp:BoundField DataField="Reference_Name" HeaderText="Name" />
                        <asp:BoundField DataField="Reference_Phone" HeaderText="Phone" />
                        <asp:BoundField DataField="Address" HeaderText="Address" />
                        <asp:BoundField DataField="Marketing_StartDate" HeaderText="Start Date" DataFormatString="{0:d MMM yyyy}" />
                        <asp:TemplateField HeaderText="Total Institutions">
                            <ItemTemplate>
                                <span class="badge-commission"><%# Eval("TotalSchools") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Total Commission (৳)">
                            <ItemTemplate>
                                <strong><%# string.Format("{0:N0}", Eval("TotalCommission")) %></strong>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Paid (Tk)">
                            <ItemTemplate>
                                <span class="text-success font-weight-bold"><%# string.Format("{0:N0}", Eval("PaidAmount")) %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Due (Tk)">
                            <ItemTemplate>
                                <span class="text-danger font-weight-bold"><%# string.Format("{0:N0}", Eval("DueAmount")) %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Edit">
                            <ItemTemplate>
                                <asp:LinkButton runat="server" CssClass="btn btn-xs btn-warning" CommandName="EditRef"
                                    CommandArgument='<%# Eval("ReferenceID") %>'>
                                    <i class="fa fa-edit"></i>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <SelectedRowStyle CssClass="Selected" />
                </asp:GridView>
            </div>

            <%-- ===== SECTION 3: Assign Institution ===== --%>
            <asp:Panel ID="AssignPanel" runat="server" Visible="false">
                <div class="section-card">
                    <div class="section-title">
                        <i class="fa fa-university"></i> Assign Institution To —
                        <asp:Label ID="SelectedRefNameLabel" runat="server" CssClass="text-primary"></asp:Label>
                    </div>
                    <div class="row">
                        <div class="col-md-4">
                            <div class="form-group ins-search-wrap">
                                <label>Search Institution</label>
                                <asp:TextBox ID="InsSearchTextBox" runat="server" CssClass="form-control"
                                    placeholder="Type institution name..." AutoPostBack="True"
                                    OnTextChanged="InsSearchTextBox_TextChanged"></asp:TextBox>
                                <asp:HiddenField ID="SelectedSchoolIDHidden" runat="server" Value="0" />
                                <asp:HiddenField ID="SelectedSchoolNameHidden" runat="server" Value="" />
                                <div id="searchResultDiv" runat="server" class="search-result-box" visible="false">
                                    <asp:Repeater ID="SearchResultRepeater" runat="server" OnItemCommand="SearchResultRepeater_ItemCommand">
                                        <ItemTemplate>
                                            <div class="search-result-item">
                                                <asp:LinkButton runat="server" CommandName="SelectSchool"
                                                    CommandArgument='<%# Eval("SchoolID") + "|" + Eval("SchoolName") %>'>
                                                    <%# Eval("SchoolName") %> — <%# Eval("Phone") %>
                                                </asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                            <asp:Panel ID="SelectedInsPanel" runat="server" Visible="false">
                                <div class="selected-ins-badge">
                                    <i class="fa fa-check-circle text-success"></i>
                                    <asp:Label ID="SelectedInsNameLabel" runat="server"></asp:Label>
                                </div>
                            </asp:Panel>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label>Commission % <span class="text-danger">*</span></label>
                                <asp:TextBox ID="CommissionPctTextBox" runat="server" CssClass="form-control" placeholder="Ex: 10"></asp:TextBox>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label>Signup Date</label>
                                <div class="input-group date ref-datepicker">
                                    <asp:TextBox ID="SignupDateTextBox" runat="server" CssClass="form-control" placeholder="dd MMM yyyy" autocomplete="off"></asp:TextBox>
                                    <div class="input-group-append">
                                        <span class="input-group-text"><i class="fa fa-calendar"></i></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label>Expiry Date</label>
                                <div class="input-group date ref-datepicker">
                                    <asp:TextBox ID="CommExpireDateTextBox" runat="server" CssClass="form-control" placeholder="dd MMM yyyy" autocomplete="off"></asp:TextBox>
                                    <div class="input-group-append">
                                        <span class="input-group-text"><i class="fa fa-calendar"></i></span>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-2">
                            <div class="form-group">
                                <label>&nbsp;</label><br />
                                <asp:Button ID="AssignInsButton" runat="server" CssClass="btn btn-success btn-block"
                                    Text="Assign Institution" OnClick="AssignInsButton_Click" />
                            </div>
                        </div>
                    </div>
                    <asp:Label ID="AssignMsgLabel" runat="server" CssClass="text-success font-weight-bold"></asp:Label>

                    <%-- Assigned institutions list --%>
                    <h5 class="mt-3"><i class="fa fa-list-ul"></i> Assigned Institutions List</h5>
                    <asp:GridView ID="AssignedSchoolsGridView" runat="server" AutoGenerateColumns="False"
                        CssClass="mGrid" DataKeyNames="Reference_School_ID"
                        OnRowCommand="AssignedSchoolsGridView_RowCommand"
                        OnRowEditing="AssignedSchoolsGridView_RowEditing"
                        OnRowUpdating="AssignedSchoolsGridView_RowUpdating"
                        OnRowCancelingEdit="AssignedSchoolsGridView_RowCancelingEdit">
                        <Columns>
                            <asp:BoundField DataField="SchoolName" HeaderText="Institution" />
                            <asp:BoundField DataField="Phone" HeaderText="Phone" />
                            <asp:TemplateField HeaderText="Commission %">
                                <ItemTemplate>
                                    <span class="badge-commission"><%# Eval("Percentage") %>%</span>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="EditPctTextBox" runat="server" CssClass="form-control" Text='<%# Eval("Percentage") %>' Style="width:70px"></asp:TextBox>
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Signup Date">
                                <ItemTemplate><%# Eval("School_SignUp_Date", "{0:d MMM yyyy}") %></ItemTemplate>
                                <EditItemTemplate>
                                    <div class="input-group date ref-datepicker" style="width:145px">
                                        <asp:TextBox ID="EditSignupTextBox" runat="server" CssClass="form-control" Text='<%# ((DateTime?)Eval("School_SignUp_Date") != null) ? ((DateTime)Eval("School_SignUp_Date")).ToString("dd MMM yyyy") : "" %>' Style="width:100px" autocomplete="off"></asp:TextBox>
                                        <div class="input-group-append"><span class="input-group-text"><i class="fa fa-calendar"></i></span></div>
                                    </div>
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Expiry Date">
                                <ItemTemplate>
                                    <%# Eval("End_Reference_Date", "{0:d MMM yyyy}") %>
                                    <asp:Label runat="server" Visible='<%# (Eval("End_Reference_Date") != DBNull.Value && (DateTime)Eval("End_Reference_Date") < DateTime.Today) %>'><span class="badge-expired ml-1">Expired</span></asp:Label>
                                    <asp:Label runat="server" Visible='<%# (Eval("End_Reference_Date") != DBNull.Value && (DateTime)Eval("End_Reference_Date") >= DateTime.Today) %>'><span class="badge-active ml-1">Active</span></asp:Label>
                                </ItemTemplate>
                                <EditItemTemplate>
                                    <div class="input-group date ref-datepicker" style="width:145px">
                                        <asp:TextBox ID="EditExpireTextBox" runat="server" CssClass="form-control" Text='<%# ((DateTime?)Eval("End_Reference_Date") != null) ? ((DateTime)Eval("End_Reference_Date")).ToString("dd MMM yyyy") : "" %>' Style="width:100px" autocomplete="off"></asp:TextBox>
                                        <div class="input-group-append"><span class="input-group-text"><i class="fa fa-calendar"></i></span></div>
                                    </div>
                                </EditItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="TotalCommission" HeaderText="Total Commission (৳)" DataFormatString="{0:N0}" />
                            <asp:BoundField DataField="PaidCommission" HeaderText="Paid (৳)" DataFormatString="{0:N0}" />
                            <asp:TemplateField HeaderText="Due (৳)">
                                <ItemTemplate>
                                    <span class="text-danger font-weight-bold"><%# string.Format("{0:N0}", Convert.ToDouble(Eval("TotalCommission") ?? 0) - Convert.ToDouble(Eval("PaidCommission") ?? 0)) %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:CommandField ShowEditButton="True" EditText="<i class='fa fa-edit'></i>" UpdateText="Update" CancelText="Cancel" />
                            <asp:TemplateField HeaderText="Delete">
                                <ItemTemplate>
                                    <asp:LinkButton runat="server" CssClass="btn btn-xs btn-danger"
                                        CommandName="DeleteAssign" CommandArgument='<%# Eval("Reference_School_ID") %>'
                                        OnClientClick="return confirm('Are you sure you want to delete this assignment?')">
                                        <i class="fa fa-trash"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>

        </ContentTemplate>
    </asp:UpdatePanel>

    <script>
        function initDatePickers() {
            $('.ref-datepicker input').each(function () {
                var $input = $(this);
                $input.datepicker('destroy');
                $input.datepicker({
                    format: 'dd MMM yyyy',
                    autoclose: true,
                    todayHighlight: true,
                    orientation: 'bottom auto',
                    clearBtn: false
                });
            });
        }

        $(function () {
            initDatePickers();
        });

        // Re-initialize after UpdatePanel partial postback
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
            initDatePickers();
        });
    </script>

</asp:Content>
