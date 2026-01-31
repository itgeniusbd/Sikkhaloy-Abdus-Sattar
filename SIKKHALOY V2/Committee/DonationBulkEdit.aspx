<%@ Page Title="Bulk Edit Donations" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationBulkEdit.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationBulkEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .selected-row { background-color: #fff3cd !important; }
        .selected-row td { background-color: #fff3cd !important; }
        
        /* Force checkbox column to show */
        .checkbox-col {
            width: 60px !important;
            min-width: 60px !important;
            max-width: 60px !important;
            text-align: center !important;
            padding: 8px !important;
        }
        
        /* Make sure checkboxes are visible */
        input[type="checkbox"] {
            width: 18px !important;
            height: 18px !important;
            cursor: pointer !important;
            display: inline-block !important;
            visibility: visible !important;
            opacity: 1 !important;
        }
        
        .amount-edit, .date-edit {
            width: 100% !important;
            font-size: 12px !important;
            padding: 4px !important;
            display: none;
        }
        
        .table > thead > tr > th {
            background-color: #343a40 !important;
            color: white !important;
            border: 1px solid #dee2e6 !important;
            padding: 10px !important;
        }
        
        .table > tbody > tr > td {
            border: 1px solid #dee2e6 !important;
            padding: 8px !important;
            vertical-align: middle !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Bulk Edit Donations</h3>

    <div class="row mb-3">
        <div class="col-md-12">
            <div class="card card-body">
                <h5 class="mb-3">Search & Filter</h5>
                <div class="row">
                    <div class="col-md-2">
                        <div class="form-group">
                            <label>Donor Type</label>
                            <asp:DropDownList ID="MemberTypeDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId">
                                <asp:ListItem Value="">[ All Types ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID) ORDER BY CommitteeMemberType">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

                    <div class="col-md-3">
                        <div class="form-group">
                            <label>Search by Name</label>
                            <asp:TextBox ID="SearchNameTextBox" runat="server" CssClass="form-control donor-name-autocomplete" placeholder="Enter donor name"></asp:TextBox>
                            <asp:HiddenField ID="SelectedDonorIdHiddenField" runat="server" />
                        </div>
                    </div>

                    <div class="col-md-2">
                        <div class="form-group">
                            <label>Search by Phone</label>
                            <asp:TextBox ID="SearchPhoneTextBox" runat="server" CssClass="form-control donor-phone-autocomplete" placeholder="Enter phone"></asp:TextBox>
                        </div>
                    </div>

                    <div class="col-md-3">
                        <div class="form-group">
                            <label>Donation Category</label>
                            <asp:DropDownList ID="CategoryDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId">
                                <asp:ListItem Value="">[ All Categories ]</asp:ListItem>
                            </asp:DropDownList>
                            <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID) ORDER BY DonationCategory">
                                <SelectParameters>
                                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                                </SelectParameters>
                            </asp:SqlDataSource>
                        </div>
                    </div>

                    <div class="col-md-2">
                        <div class="form-group">
                            <label>Status</label>
                            <asp:DropDownList ID="StatusDropDownList" runat="server" CssClass="form-control">
                                <asp:ListItem Value="">All</asp:ListItem>
                                <asp:ListItem Value="0" Selected="True">Due Only</asp:ListItem>
                                <asp:ListItem Value="1">Paid Only</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-12">
                        <asp:Button ID="SearchButton" runat="server" Text="Search" CssClass="btn btn-primary btn-sm" OnClick="SearchButton_Click" />
                        <asp:Button ID="ClearFiltersButton" runat="server" Text="Clear Filters" CssClass="btn btn-secondary btn-sm" OnClick="ClearFiltersButton_Click" />
                        <asp:Label ID="ResultCountLabel" runat="server" CssClass="ml-3 text-muted"></asp:Label>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="card mb-3">
        <div class="card-body py-2">
            <div class="row align-items-center">
                <div class="col-md-8">
                    <div class="btn-group">
                        <asp:Button ID="BulkUpdateButton" runat="server" Text="Update Selected" CssClass="btn btn-success" OnClick="BulkUpdateButton_Click" />
                        <asp:Button ID="BulkDeleteButton" runat="server" Text="Delete Selected" CssClass="btn btn-danger" OnClick="BulkDeleteButton_Click" OnClientClick="return confirmBulkDelete();" />
                    </div>
                    <small class="text-muted ml-3">Select checkboxes to enable editing. Click buttons to save or delete multiple items.</small>
                </div>
            </div>
        </div>
    </div>

    <div class="table-responsive">
        <asp:GridView ID="DonationsGridView" runat="server" CssClass="table table-bordered table-hover" 
                      AutoGenerateColumns="False" DataKeyNames="CommitteeDonationId" 
                      DataSourceID="DonationsSQL" AllowPaging="True" PageSize="50" 
                      OnRowDataBound="DonationsGridView_RowDataBound">
            <Columns>
                <asp:TemplateField HeaderText="Select">
                    <HeaderTemplate>
                        <input type="checkbox" id="SelectAllCheckBox" /> All
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:CheckBox ID="SelectCheckBox" runat="server" CssClass="select-checkbox" />
                    </ItemTemplate>
                    <ItemStyle CssClass="checkbox-col" />
                    <HeaderStyle CssClass="checkbox-col" />
                </asp:TemplateField>
                
                <asp:TemplateField HeaderText="Donor Name">
                    <ItemTemplate>
                        <strong><%# Eval("MemberName") %></strong><br />
                        <small class="text-muted"><%# Eval("SmsNumber") %></small>
                    </ItemTemplate>
                </asp:TemplateField>
                
                <asp:BoundField DataField="DonationCategory" HeaderText="Category" ReadOnly="True" />
                <asp:BoundField DataField="Description" HeaderText="Description" ReadOnly="True" />
                
                <asp:TemplateField HeaderText="Amount">
                    <ItemTemplate>
                        <div class="amount-display"><%# Eval("Amount", "{0:N2}") %></div>
                        <asp:TextBox ID="AmountTextBox" runat="server" CssClass="form-control form-control-sm amount-edit" 
                                     Text='<%# Eval("Amount") %>' 
                                     type="number" step="0.01" />
                    </ItemTemplate>
                    <ItemStyle CssClass="text-right" />
                </asp:TemplateField>
                
                <asp:BoundField DataField="PaidAmount" HeaderText="Paid" DataFormatString="{0:N2}" ReadOnly="True">
                    <ItemStyle CssClass="text-right" />
                </asp:BoundField>
                
                <asp:BoundField DataField="Due" HeaderText="Due" DataFormatString="{0:N2}" ReadOnly="True">
                    <ItemStyle CssClass="text-right" />
                </asp:BoundField>
                
                <asp:TemplateField HeaderText="Promise Date">
                    <ItemTemplate>
                        <div class="date-display"><%# Eval("PromiseDate", "{0:d MMM yyyy}") %></div>
                        <asp:TextBox ID="PromiseDateTextBox" runat="server" CssClass="form-control form-control-sm date-edit" 
                                     Text='<%# Eval("PromiseDate", "{0:dd MMM yyyy}") %>' 
                                     autocomplete="off" />
                    </ItemTemplate>
                </asp:TemplateField>
                
                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <span class='<%# Convert.ToInt32(Eval("IsPaid")) == 1 ? "badge badge-success" : "badge badge-warning" %>'>
                            <%# Convert.ToInt32(Eval("IsPaid")) == 1 ? "Paid" : "Due" %>
                        </span>
                    </ItemTemplate>
                    <ItemStyle CssClass="text-center" />
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="pagination-ys" />
            <HeaderStyle CssClass="bg-dark text-white" />
        </asp:GridView>
        
        <asp:SqlDataSource ID="DonationsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
            SelectCommand="SELECT cd.CommitteeDonationId, cd.CommitteeMemberId, cd.Amount, cd.PaidAmount, cd.Due, cd.PromiseDate, cd.Description, cd.IsPaid,
                           cm.MemberName, cm.SmsNumber, cmt.CommitteeMemberType, cdc.DonationCategory
                           FROM CommitteeDonation cd
                           INNER JOIN CommitteeMember cm ON cd.CommitteeMemberId = cm.CommitteeMemberId
                           INNER JOIN CommitteeMemberType cmt ON cm.CommitteeMemberTypeId = cmt.CommitteeMemberTypeId
                           INNER JOIN CommitteeDonationCategory cdc ON cd.CommitteeDonationCategoryId = cdc.CommitteeDonationCategoryId
                           WHERE cd.SchoolID = @SchoolID 
                           AND (NULLIF(@MemberTypeId, '') IS NULL OR cmt.CommitteeMemberTypeId = CONVERT(int, @MemberTypeId))
                           AND (NULLIF(@SelectedDonorId, '') IS NULL OR cd.CommitteeMemberId = CONVERT(int, @SelectedDonorId))
                           AND (NULLIF(@SearchName, '') IS NULL OR cm.MemberName LIKE '%' + @SearchName + '%')
                           AND (NULLIF(@SearchPhone, '') IS NULL OR cm.SmsNumber LIKE '%' + @SearchPhone + '%')
                           AND (NULLIF(@CategoryId, '') IS NULL OR cd.CommitteeDonationCategoryId = CONVERT(int, @CategoryId))
                           AND (NULLIF(@Status, '') IS NULL OR cd.IsPaid = CONVERT(int, @Status))
                           ORDER BY cd.InsertDate DESC"
            CancelSelectOnNullParameter="False">
            <SelectParameters>
                <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" Type="Int32" />
                <asp:ControlParameter ControlID="MemberTypeDropDownList" Name="MemberTypeId" PropertyName="SelectedValue" Type="String" ConvertEmptyStringToNull="False" />
                <asp:ControlParameter ControlID="SelectedDonorIdHiddenField" Name="SelectedDonorId" PropertyName="Value" Type="String" ConvertEmptyStringToNull="False" />
                <asp:ControlParameter ControlID="SearchNameTextBox" Name="SearchName" PropertyName="Text" Type="String" ConvertEmptyStringToNull="False" />
                <asp:ControlParameter ControlID="SearchPhoneTextBox" Name="SearchPhone" PropertyName="Text" Type="String" ConvertEmptyStringToNull="False" />
                <asp:ControlParameter ControlID="CategoryDropDownList" Name="CategoryId" PropertyName="SelectedValue" Type="String" ConvertEmptyStringToNull="False" />
                <asp:ControlParameter ControlID="StatusDropDownList" Name="Status" PropertyName="SelectedValue" Type="String" ConvertEmptyStringToNull="False" />
            </SelectParameters>
        </asp:SqlDataSource>
    </div>

    <script>
        $(function () {
            console.log("Page loaded. Initializing...");
            
            // Date picker for inline editing
            initializeDatePickers();

            // Check if checkboxes exist
            console.log("Checkboxes found:", $(".select-checkbox input").length);
            console.log("Select all checkbox:", $("#SelectAllCheckBox").length);

            // Global map variable for autocomplete
            var donorMap = {};

            // Donor Name Autocomplete
            $(".donor-name-autocomplete").typeahead({
                minLength: 2,
                source: function (query, result) {
                    $.ajax({
                        url: "DonationBulkEdit.aspx/SearchDonorsByName",
                        data: JSON.stringify({ 'searchText': query }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            var donors = JSON.parse(response.d);
                            var labels = [];
                            donorMap = {};
                            $.each(donors, function (i, donor) {
                                var label = donor.MemberName + " (" + donor.SmsNumber + ")";
                                labels.push(label);
                                donorMap[label] = donor;
                            });
                            result(labels);
                        },
                        error: function (err) {
                            console.log(err);
                        }
                    });
                },
                updater: function (item) {
                    var donor = donorMap[item];
                    if (donor) {
                        $("#<%= SelectedDonorIdHiddenField.ClientID %>").val(donor.CommitteeMemberId);
                        $("#<%= SearchPhoneTextBox.ClientID %>").val(donor.SmsNumber);
                        return donor.MemberName;
                    }
                    return item;
                }
            });

            // Donor Phone Autocomplete
            $(".donor-phone-autocomplete").typeahead({
                minLength: 3,
                source: function (query, result) {
                    $.ajax({
                        url: "DonationBulkEdit.aspx/SearchDonorsByPhone",
                        data: JSON.stringify({ 'searchText': query }),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (response) {
                            var donors = JSON.parse(response.d);
                            var labels = [];
                            donorMap = {};
                            $.each(donors, function (i, donor) {
                                var label = donor.SmsNumber + " - " + donor.MemberName;
                                labels.push(label);
                                donorMap[label] = donor;
                            });
                            result(labels);
                        },
                        error: function (err) {
                            console.log(err);
                        }
                    });
                },
                updater: function (item) {
                    var donor = donorMap[item];
                    if (donor) {
                        $("#<%= SelectedDonorIdHiddenField.ClientID %>").val(donor.CommitteeMemberId);
                        $("#<%= SearchNameTextBox.ClientID %>").val(donor.MemberName);
                        return donor.SmsNumber;
                    }
                    return item;
                }
            });

            // Auto-submit on dropdown change
            $("#<%= MemberTypeDropDownList.ClientID %>").on("change", function () {
                setTimeout(function () {
                    $("#<%= SearchButton.ClientID %>").click();
                }, 100);
            });

            $("#<%= CategoryDropDownList.ClientID %>").on("change", function () {
                setTimeout(function () {
                    $("#<%= SearchButton.ClientID %>").click();
                }, 100);
            });

            $("#<%= StatusDropDownList.ClientID %>").on("change", function () {
                setTimeout(function () {
                    $("#<%= SearchButton.ClientID %>").click();
                }, 100);
            });

            // Select All checkbox
            $(document).on("change", "#SelectAllCheckBox", function () {
                var isChecked = $(this).is(':checked');
                $(".select-checkbox input").each(function () {
                    $(this).prop('checked', isChecked);
                    toggleRowEdit($(this));
                });
            });

            // Individual checkbox - Enable/Disable inline editing
            $(document).on("change", ".select-checkbox input", function () {
                toggleRowEdit($(this));
                
                // Uncheck "Select All" if any individual is unchecked
                if (!$(this).is(':checked')) {
                    $("#SelectAllCheckBox").prop('checked', false);
                }
            });

            // Function to toggle row editing
            function toggleRowEdit(checkbox) {
                var row = checkbox.closest('tr');
                var isChecked = checkbox.is(':checked');
                
                console.log("Toggle row edit. Checked:", isChecked);
                
                if (isChecked) {
                    // Show editable fields
                    row.addClass('selected-row');
                    row.find('.amount-display').hide();
                    row.find('.amount-edit').show().css('display', 'block');
                    row.find('.date-display').hide();
                    row.find('.date-edit').show().css('display', 'block').datepicker({
                        format: 'dd M yyyy',
                        todayBtn: "linked",
                        todayHighlight: true,
                        autoclose: true
                    });
                } else {
                    // Hide editable fields
                    row.removeClass('selected-row');
                    row.find('.amount-display').show().css('display', 'block');
                    row.find('.amount-edit').hide();
                    row.find('.date-display').show().css('display', 'block');
                    row.find('.date-edit').hide();
                }
            }

            // Initialize datepickers for already checked rows (e.g., after postback)
            function initializeDatePickers() {
                $('.select-checkbox input:checked').each(function () {
                    var row = $(this).closest('tr');
                    row.find('.date-edit').datepicker({
                        format: 'dd M yyyy',
                        todayBtn: "linked",
                        todayHighlight: true,
                        autoclose: true
                    });
                });
            }
        });

        function confirmBulkDelete() {
            var count = $(".select-checkbox input:checked").length;
            if (count === 0) {
                alert("Please select at least one donation to delete.");
                return false;
            }
            return confirm("Are you sure you want to DELETE " + count + " donation(s)? This action cannot be undone!");
        }
    </script>
</asp:Content>
