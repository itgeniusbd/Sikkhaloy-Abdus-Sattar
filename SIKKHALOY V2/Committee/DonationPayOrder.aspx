<%@ Page Title="Donation Pay Order" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="DonationPayOrder.aspx.cs" Inherits="EDUCATION.COM.Committee.DonationPayOrder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        .donation-info { margin: 15px 0; padding: 10px; background-color: #f8f9fa; border-radius: 5px; }
        .MultiRoleGV tr td { border: 1px solid #ddd; padding: 6px 0; }
        .PayFor { width: 96%; }
        .form-control { display: inline; }
        .member-selected td { background-color: #d4edda; }
        .auto-fill-months { margin-bottom: 10px; }
        .auto-fill-months:hover { box-shadow: 0 2px 5px rgba(0,0,0,0.2); }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Donation Pay Order (Multiple Months)</h3>
    <asp:Label ID="PayorderMsgLabel" runat="server" CssClass="alert-success"></asp:Label>

    <div class="form-inline mb-3">
        <div class="form-group">
            <label class="mr-2">Member Type</label>
            <asp:DropDownList ID="MemberTypeDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="MemberTypeSQL" DataTextField="CommitteeMemberType" DataValueField="CommitteeMemberTypeId" OnSelectedIndexChanged="MemberTypeDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="">[ Select Member Type ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="MemberTypeSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeMemberTypeId, CommitteeMemberType FROM CommitteeMemberType WHERE (SchoolID = @SchoolID) ORDER BY CommitteeMemberType">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group ml-3">
            <label class="mr-2">Donation Category</label>
            <asp:DropDownList ID="DonationCategoryDropDownList" runat="server" CssClass="form-control" AppendDataBoundItems="True" AutoPostBack="True" DataSourceID="CategorySQL" DataTextField="DonationCategory" DataValueField="CommitteeDonationCategoryId" OnSelectedIndexChanged="DonationCategoryDropDownList_SelectedIndexChanged">
                <asp:ListItem Value="">[ Select Category ]</asp:ListItem>
            </asp:DropDownList>
            <asp:SqlDataSource ID="CategorySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>" SelectCommand="SELECT CommitteeDonationCategoryId, DonationCategory FROM CommitteeDonationCategory WHERE (SchoolID = @SchoolID)">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>

        <div class="form-group ml-3">
            <asp:Label ID="TemplateAmountLabel" runat="server" CssClass="alert alert-info py-1 px-2 mb-0" Visible="false">
                Template Amount: <strong id="templateAmountValue">0</strong> Tk
            </asp:Label>
        </div>

        <div class="form-group ml-3">
            <a class="btn btn-outline-secondary btn-sm" href="DonationTemplates.aspx" target="_blank">
                <i class="fa fa-cog"></i> Manage Templates
            </a>
        </div>
    </div>

    <div class="table-responsive mb-4 Hide_Members" style="display: none">
        <div class="alert alert-info">
            Select Members
            (<asp:Label ID="TotalMembersLabel" runat="server"></asp:Label>)
        </div>
        <div style="max-height: 500px; overflow: auto;">
            <asp:GridView ID="MembersGridView" runat="server" AutoGenerateColumns="False" DataSourceID="MembersSQL" DataKeyNames="CommitteeMemberId" CssClass="mGrid">
                <Columns>
                    <asp:TemplateField>
                        <HeaderTemplate>
                            <asp:CheckBox ID="AllMembersCheckBox" runat="server" Text="All" />
                        </HeaderTemplate>
                        <ItemTemplate>
                            <asp:CheckBox ID="SingleMemberCheckBox" runat="server" Text=" " />
                        </ItemTemplate>
                        <ItemStyle Width="50px" />
                    </asp:TemplateField>
                    <asp:BoundField DataField="MemberName" HeaderText="Name" SortExpression="MemberName" />
                    <asp:BoundField DataField="SmsNumber" HeaderText="Phone" SortExpression="SmsNumber" />
                    <asp:BoundField DataField="Address" HeaderText="Address" SortExpression="Address" />
                    <asp:BoundField DataField="CommitteeMemberType" HeaderText="Type" SortExpression="CommitteeMemberType" />
                </Columns>
            </asp:GridView>
            <asp:SqlDataSource ID="MembersSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CommitteeMember.CommitteeMemberId, CommitteeMember.MemberName, CommitteeMember.SmsNumber, CommitteeMember.Address, CommitteeMemberType.CommitteeMemberType 
                FROM CommitteeMember 
                INNER JOIN CommitteeMemberType ON CommitteeMember.CommitteeMemberTypeId = CommitteeMemberType.CommitteeMemberTypeId 
                WHERE (CommitteeMember.SchoolID = @SchoolID) AND (CommitteeMember.CommitteeMemberTypeId = @CommitteeMemberTypeId) 
                ORDER BY CommitteeMember.MemberName">
                <SelectParameters>
                    <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
                    <asp:ControlParameter ControlID="MemberTypeDropDownList" Name="CommitteeMemberTypeId" PropertyName="SelectedValue" />
                </SelectParameters>
            </asp:SqlDataSource>
            <asp:CustomValidator ID="CV" runat="server" ClientValidationFunction="ValidateMembers" ErrorMessage="You do not select any member from member list." ForeColor="Red" ValidationGroup="A"></asp:CustomValidator>
        </div>
    </div>

    <div class="table-responsive mb-4 Monthly_Donations" style="display: none;">
        <div class="alert alert-success">Monthly Donation Details</div>
        <asp:GridView ID="MonthlyDonationGridView" runat="server" AutoGenerateColumns="False" CssClass="MultiRoleGV" OnRowDataBound="MonthlyDonationGridView_RowDataBound" ShowHeader="False" Width="100%">
            <Columns>
                <asp:TemplateField>
                    <ItemTemplate>
                        <div class="donation-info">
                            <asp:CheckBox ID="AddMonthlyDonationCheckBox" runat="server" Text=" " />
                            <strong>Add - Monthly Donations (
                            <asp:Label ID="MonthCountLabel" runat="server" Text='<%# Eval("MonthCount") %>'></asp:Label>
                            months)</strong>
                        </div>
                        <div class="criteriaData" style="display: none">
                            <div class="mb-2">
                                <button type="button" class="btn btn-sm btn-success auto-fill-months" title="Auto-fill all months from education year">
                                    <i class="fa fa-magic"></i> Auto Fill Pay For & Promise Date
                                </button>
                            </div>
                            <asp:GridView ID="MonthDetailsGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid" Width="100%">
                                <Columns>
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <asp:CheckBox ID="MonthCheckBox" runat="server" Text=" " />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Pay For">
                                        <ItemTemplate>
                                            <asp:TextBox ID="PayForTextBox" autocomplete="off" placeholder="Pay For (e.g., January 2024)" runat="server" CssClass="PayFor form-control"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="payForRF" Enabled="false" runat="server" ControlToValidate="PayForTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField>
                                        <HeaderTemplate>
                                            <small>Amount Same For All</small><br />
                                            <asp:TextBox ID="AssignAmountTextBox" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false" CssClass="form-control" placeholder="Amount Same For All" runat="server"></asp:TextBox>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <asp:TextBox ID="AmountTextBox" placeholder="Amount" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="AmountRF" Enabled="false" runat="server" ControlToValidate="AmountTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Promise Date">
                                        <ItemTemplate>
                                            <asp:TextBox ID="PromiseDateTextBox" placeholder="Promise Date" runat="server" CssClass="form-control Datetime" onkeypress="return isNumberKey(event)" autocomplete="off" onDrop="blur();return false;" onpaste="return false"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="PromiseDateRF" Enabled="false" runat="server" ControlToValidate="PromiseDateTextBox" CssClass="EroorStar" Display="Dynamic" ErrorMessage="!" SetFocusOnError="True" ValidationGroup="A"></asp:RequiredFieldValidator>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Description">
                                        <ItemTemplate>
                                            <asp:TextBox ID="DescriptionTextBox" placeholder="Description" runat="server" CssClass="form-control"></asp:TextBox>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>

    <div class="mb-3">
        <asp:Button ID="PayOrderButton" runat="server" Text="Create Pay Orders" CssClass="btn btn-primary" OnClick="PayOrderButton_Click" ValidationGroup="A" CausesValidation="true" />
        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="EroorSummer" DisplayMode="List" ShowMessageBox="True" ValidationGroup="A" />
    </div>

    <asp:SqlDataSource ID="PayOrderSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        InsertCommand="INSERT INTO CommitteeDonation(SchoolID, RegistrationID, CommitteeMemberId, CommitteeDonationCategoryId, Amount, Description, PromiseDate) VALUES(@SchoolID, @RegistrationID, @CommitteeMemberId, @CommitteeDonationCategoryId, @Amount, @Description, @PromiseDate)"
        SelectCommand="SELECT * FROM CommitteeDonation">
        <InsertParameters>
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:Parameter Name="CommitteeMemberId" Type="Int32" />
            <asp:ControlParameter ControlID="DonationCategoryDropDownList" Name="CommitteeDonationCategoryId" PropertyName="SelectedValue" />
            <asp:Parameter Name="Amount" Type="Double" />
            <asp:Parameter Name="Description" Type="String" />
            <asp:Parameter Name="PromiseDate" DbType="Date" />
        </InsertParameters>
    </asp:SqlDataSource>

    <script type="text/javascript">
        $(function () {
            // Date picker
            $('.Datetime').datepicker({
                format: 'dd M yyyy',
                todayBtn: "linked",
                todayHighlight: true,
                autoclose: true
            });

            // Handle "All" checkbox in Members GridView
            $("[id*=AllMembersCheckBox]").on("click", function () {
                var isChecked = $(this).is(':checked');
                var grid = $(this).closest('table');

                $("input[id*=SingleMemberCheckBox]", grid).each(function () {
                    $(this).prop('checked', isChecked);
                    if (isChecked) {
                        $(this).closest('tr').addClass('member-selected');
                    } else {
                        $(this).closest('tr').removeClass('member-selected');
                    }
                });
            });

            // Handle individual member checkbox clicks
            $("[id*=SingleMemberCheckBox]").on("click", function () {
                if ($(this).is(':checked')) {
                    $(this).closest('tr').addClass('member-selected');
                } else {
                    $(this).closest('tr').removeClass('member-selected');
                    $("[id*=AllMembersCheckBox]").prop('checked', false);
                }
            });

            // Show Members GridView if it has data
            if ($('[id*=MembersGridView] tr').length > 1) {
                $(".Hide_Members").show();
            }

            // Show Monthly Donations section
            if ($('[id*=MonthlyDonationGridView] tr').length > 0) {
                $(".Monthly_Donations").show();
            }

            // PayFor autocomplete for months
            $('.PayFor').on("keypress", function () {
                var tr = $(this).closest("tr");
                $(this).typeahead({
                    minLength: 1,
                    source: function (request, result) {
                        $.ajax({
                            url: "/Committee/DonationPayOrder.aspx/GetMonth",
                            data: JSON.stringify({ 'prefix': request }),
                            dataType: "json",
                            type: "POST",
                            contentType: "application/json; charset=utf-8",
                            success: function (response) {
                                label = [];
                                map = {};
                                $.map(JSON.parse(response.d), function (item) {
                                    label.push(item.Month);
                                    map[item.Month] = item;
                                });
                                result(label);
                            }
                        });
                    },
                    updater: function (item) {
                        $(".Datetime:eq(0)", tr).val("10 " + map[item].MonthYearValue);
                        return item;
                    }
                });
            });

            // Monthly Donation CheckBox - Show/Hide detail section
            $("[id*=AddMonthlyDonationCheckBox]").on("click", function () {
                if ($(this).is(':checked')) {
                    $(this).closest("tr").find("div.criteriaData").show("slow");
                }
                else {
                    $(this).closest("tr").find("div.criteriaData").hide("slow");
                }
            });

            // Monthly Donation CheckBox - Check/Uncheck all child checkboxes
            $("[id*=AddMonthlyDonationCheckBox]").on("click", function () {
                var addCheckBox = $(this);
                var grid = $(this).closest("tr").find("div.criteriaData");

                $("input[type=checkbox]", grid).each(function () {
                    var row = $(this).closest("tr");
                    var payForRF = row.find("[id*=payForRF]")[0];
                    var amountRF = row.find("[id*=AmountRF]")[0];
                    var promiseDateRF = row.find("[id*=PromiseDateRF]")[0];
                    
                    if (addCheckBox.is(":checked")) {
                        $(this).prop("checked", true);
                        if (typeof ValidatorEnable === 'function') {
                            if (payForRF) ValidatorEnable(payForRF, true);
                            if (amountRF) ValidatorEnable(amountRF, true);
                            if (promiseDateRF) ValidatorEnable(promiseDateRF, true);
                        }
                    }
                    else {
                        $(this).prop("checked", false);
                        if (typeof ValidatorEnable === 'function') {
                            if (payForRF) ValidatorEnable(payForRF, false);
                            if (amountRF) ValidatorEnable(amountRF, false);
                            if (promiseDateRF) ValidatorEnable(promiseDateRF, false);
                        }
                    }
                });
            });

            $("[id*=MonthCheckBox]").on("click", function () {
                var row = $(this).closest("tr");
                var isChecked = $(this).is(":checked");
                var payForRF = row.find("[id*=payForRF]")[0];
                var amountRF = row.find("[id*=AmountRF]")[0];
                var promiseDateRF = row.find("[id*=PromiseDateRF]")[0];
                
                if (typeof ValidatorEnable === 'function') {
                    if (payForRF) ValidatorEnable(payForRF, isChecked);
                    if (amountRF) ValidatorEnable(amountRF, isChecked);
                    if (promiseDateRF) ValidatorEnable(promiseDateRF, isChecked);
                }
            });

            // Assign Amount to All
            $("[id*=AssignAmountTextBox]").on("keyup", function () {
                $("[id*=AmountTextBox]", $(this).closest("tr td")).val($.trim($(this).val()));
            });

            // Auto Fill Pay For and Promise Date button
            $(document).on("click", ".auto-fill-months", function (e) {
                e.preventDefault();
                e.stopPropagation();
                
                var btn = $(this);
                var criteriaDiv = btn.closest(".criteriaData");
                var monthsTable = criteriaDiv.find("table");
                
                // Get amount same for all value if exists
                var amountSameForAll = criteriaDiv.find("[id*=AssignAmountTextBox]").val();
                
                // If no amount same for all, try to get from template
                if (!amountSameForAll || amountSameForAll.trim() === '') {
                    var templateAmount = document.getElementById('templateAmountValue');
                    if (templateAmount && templateAmount.innerText !== '0') {
                        amountSameForAll = templateAmount.innerText;
                    }
                }

                // Generate months in JavaScript (January to December)
                var currentYear = new Date().getFullYear();
                var monthNames = ["January", "February", "March", "April", "May", "June", 
                                  "July", "August", "September", "October", "November", "December"];
                var monthShortNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", 
                                       "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                
                var months = [];
                for (var i = 0; i < 12; i++) {
                    months.push({
                        MonthName: monthNames[i] + " " + currentYear,
                        MonthYearValue: monthShortNames[i] + " " + currentYear
                    });
                }

                // Get only data rows (skip header row) - select rows that have MonthCheckBox
                var dataRows = monthsTable.find("tr").filter(function() {
                    return $(this).find("[id*=MonthCheckBox]").length > 0;
                });

                var monthIndex = 0;
                dataRows.each(function () {
                    if (monthIndex < months.length) {
                        var row = $(this);
                        var payForTextBox = row.find("[id*=PayForTextBox]");
                        var promiseDateTextBox = row.find("[id*=PromiseDateTextBox]");
                        var amountTextBox = row.find("[id*=AmountTextBox]");
                        var checkBox = row.find("[id*=MonthCheckBox]");

                        // Auto check the checkbox
                        if (!checkBox.is(':checked')) {
                            checkBox.prop('checked', true);
                            // Trigger validation enable logic
                            if (typeof ValidatorEnable === 'function') {
                                var payForRF = row.find("[id*=payForRF]")[0];
                                var amountRF = row.find("[id*=AmountRF]")[0];
                                var promiseDateRF = row.find("[id*=PromiseDateRF]")[0];
                                
                                if (payForRF) ValidatorEnable(payForRF, true);
                                if (amountRF) ValidatorEnable(amountRF, true);
                                if (promiseDateRF) ValidatorEnable(promiseDateRF, true);
                            }
                        }

                        // Fill Pay For with month name (e.g., "January 2026")
                        payForTextBox.val(months[monthIndex].MonthName);

                        // Fill Promise Date with 10th of that month (e.g., "10 Jan 2026")
                        promiseDateTextBox.val("10 " + months[monthIndex].MonthYearValue);
                        
                        // Fill Amount if available
                        if (amountSameForAll && amountSameForAll.trim() !== '') {
                            amountTextBox.val(amountSameForAll);
                        }
                        
                        monthIndex++;
                    }
                });

                if (typeof $.notify === 'function') {
                    $.notify("All months filled successfully!", "success");
                } else {
                    alert("All months filled successfully!");
                }
                
                return false;
            });
        });

        // Validation function
        function ValidateMembers(source, args) {
            var grid = document.getElementById("<%=MembersGridView.ClientID%>");
            var checkboxes = grid.getElementsByTagName("input");
            for (var i = 0; i < checkboxes.length; i++) {
                if (checkboxes[i].type == "checkbox" && checkboxes[i].checked) {
                    args.IsValid = true;
                    return;
                }
            }
            args.IsValid = false;
        }

        function isNumberKey(a) {
            a = a.which ? a.which : event.keyCode;
            return 46 != a && 31 < a && (48 > a || 57 < a) ? !1 : !0
        };
    </script>
</asp:Content>
