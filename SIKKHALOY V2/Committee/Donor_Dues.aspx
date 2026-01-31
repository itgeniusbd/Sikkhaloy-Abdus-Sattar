<%@ Page Title="My Donations Due" Language="C#" MasterPageFile="~/Basic_Donor.Master" AutoEventWireup="true" CodeBehind="Donor_Dues.aspx.cs" Inherits="EDUCATION.COM.Committee.Donor_Dues" Culture="auto" UICulture="auto" %>

<asp:Content ID="Content1" ContentPlaceHolderID="headContent" runat="server">
    <style>
        .stat-card {
            background: #fff;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            text-align: center;
        }
        .stat-card i {font-size: 2.5rem; margin-bottom: 15px;}
        .stat-card .amount {font-size: 1.8rem; font-weight: bold; display: block;}
        .stat-card .label {color: #777; text-transform: uppercase; font-size: 0.9rem;}

        .payment-button-section {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            border-radius: 15px;
            padding: 25px;
            margin-bottom: 20px;
            box-shadow: 0 8px 20px rgba(102, 126, 234, 0.3);
            display: none;
        }
        .payment-button-section h5 {color: white; font-weight: bold; margin-bottom: 15px;}

        .btn-pay-now {
            background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
            border: none;
            padding: 15px 50px;
            font-size: 1.2rem;
            font-weight: bold;
            border-radius: 50px;
            box-shadow: 0 5px 15px rgba(56, 239, 125, 0.4);
            transition: all 0.3s ease;
        }
        .btn-pay-now:hover {
            transform: translateY(-3px);
            box-shadow: 0 8px 25px rgba(56, 239, 125, 0.6);
        }
        .btn-pay-now:disabled {
            opacity: 0.7;
            cursor: not-allowed;
        }

        .row-selected {background-color: #d4edda !important;}
        
        table tbody tr {cursor: pointer; transition: background-color 0.2s;}
        table tbody tr:hover {background-color: #f8f9fa;}
        
        input[type="checkbox"] {
            width: 20px !important;
            height: 20px !important;
            cursor: pointer !important;
            display: inline-block !important;
            opacity: 1 !important;
            visibility: visible !important;
            position: relative !important;
            margin: 0 auto !important;
            -webkit-appearance: checkbox !important;
            -moz-appearance: checkbox !important;
            appearance: checkbox !important;
        }
        
        .donation-checkbox {
            width: 20px !important;
            height: 20px !important;
            min-width: 20px !important;
            min-height: 20px !important;
        }
        
        table th:first-child,
        table td:first-child {
            width: 80px !important;
            min-width: 80px !important;
            text-align: center !important;
            vertical-align: middle !important;
            padding: 15px !important;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3 class="mb-4">
        <i class="fa fa-money mr-2"></i>My Donation Dues
    </h3>

    <!-- Summary Cards -->
    <div class="row mb-4">
        <!-- 1️⃣ Due Amount Card - FIRST -->
        <div class="col-md-4 mb-3">
            <div class="stat-card">
                <i class="fa fa-exclamation-triangle text-danger"></i>
                <asp:FormView ID="DueDonationFV" runat="server" DataSourceID="SummarySQL">
                    <ItemTemplate>
                        <span class="amount">&#2547;<%# Eval("DueDonation") %></span>
                    </ItemTemplate>
                </asp:FormView>
                <span class="label">Due Amount</span>
            </div>
        </div>
        
        <!-- 2️⃣ Paid Amount Card - SECOND -->
        <div class="col-md-4 mb-3">
            <div class="stat-card">
                <i class="fa fa-check-circle text-success"></i>
                <asp:FormView ID="PaidDonationFV" runat="server" DataSourceID="SummarySQL">
                    <ItemTemplate>
                        <span class="amount">&#2547;<%# Eval("PaidDonation") %></span>
                    </ItemTemplate>
                </asp:FormView>
                <span class="label">Paid Amount</span>
            </div>
        </div>
        
        <!-- 3️⃣ Total Donation Card - THIRD -->
        <div class="col-md-4 mb-3">
            <div class="stat-card">
                <i class="fa fa-money text-primary"></i>
                <asp:FormView ID="TotalDonationFV" runat="server" DataSourceID="SummarySQL">
                    <ItemTemplate>
                        <span class="amount">&#2547;<%# Eval("TotalDonation") %></span>
                    </ItemTemplate>
                </asp:FormView>
                <span class="label">Total Donation</span>
            </div>
        </div>
    </div>

    <asp:SqlDataSource ID="SummarySQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
        SelectCommand="SELECT CM.TotalDonation, CM.PaidDonation, CM.DueDonation 
                       FROM CommitteeMember CM 
                       INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.CommitteeMemberId = CM.CommitteeMemberId
                       WHERE R.RegistrationID = @RegistrationID AND R.SchoolID = @SchoolID">
        <SelectParameters>
            <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
            <asp:SessionParameter Name="SchoolID" SessionField="SchoolID" />
        </SelectParameters>
    </asp:SqlDataSource>

    <!-- Payment Button Section -->
    <div id="payment-section" class="payment-button-section">
        <div class="text-center">
            <h5 id="total-display"></h5>
            <button type="button" id="PayDonationButton" class="btn btn-success btn-pay-now">
                PAY NOW
            </button>
            <input type="hidden" id="selectedDonationIds" runat="server" />
        </div>
    </div>

    <!-- Due List -->
    <div class="card">
        <div class="card-header bg-white">
            <h5 class="mb-0 text-danger">
                <i class="fa fa-hourglass-half"></i> Your Due Donations
            </h5>
        </div>
        <div class="card-body p-0">
            <table class="table table-hover table-bordered mb-0">
                <thead style="background-color: #f8f9fa;">
                    <tr>
                        <th class="text-center" style="background-color: #e9ecef;">
                            <label style="display: flex; align-items: center; justify-content: center; margin: 0;">
                                <input type="checkbox" id="selectAll" style="width: 20px; height: 20px; margin-right: 5px;" />
                                <span>Select All</span>
                            </label>
                        </th>
                        <th>Category</th>
                        <th>Amount</th>
                        <th>Paid</th>
                        <th>Due</th>
                        <th>Date</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="DueDonationsRepeater" runat="server" DataSourceID="DueDonationsSQL">
                        <ItemTemplate>
                            <tr class="donation-row">
                                <td class="text-center" style="background-color: #f8f9fa;">
                                    <label style="display: inline-block; margin: 0; cursor: pointer;">
                                        <input type="checkbox" class="donation-checkbox" 
                                               data-id='<%# Eval("CommitteeDonationId") %>'
                                               data-due='<%# Eval("Due") %>' />
                                    </label>
                                </td>
                                <td><%# Eval("DonationCategory") %></td>
                                <td>&#2547;<%# Eval("Amount") %></td>
                                <td>&#2547;<%# Eval("PaidAmount") %></td>
                                <td class="text-danger font-weight-bold">&#2547;<%# Eval("Due", "{0:N2}") %></td>
                                <td><%# Eval("PromiseDate", "{0:d MMM yyyy}") %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
            
            <asp:SqlDataSource ID="DueDonationsSQL" runat="server" ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"
                SelectCommand="SELECT CD.CommitteeDonationId, CD.Amount, CD.PaidAmount, CD.Due, CD.PromiseDate, CDC.DonationCategory 
                               FROM CommitteeDonation CD
                               INNER JOIN CommitteeDonationCategory CDC ON CD.CommitteeDonationCategoryId = CDC.CommitteeDonationCategoryId
                               INNER JOIN CommitteeMember CM ON CD.CommitteeMemberId = CM.CommitteeMemberId
                               INNER JOIN Registration R ON R.SchoolID = CM.SchoolID AND R.CommitteeMemberId = CM.CommitteeMemberId
                               WHERE R.RegistrationID = @RegistrationID AND CD.Due > 0
                               ORDER BY CD.PromiseDate ASC">
                <SelectParameters>
                    <asp:SessionParameter Name="RegistrationID" SessionField="RegistrationID" />
                </SelectParameters>
            </asp:SqlDataSource>
        </div>
    </div>

    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script>
        $(document).ready(function() {
            console.log('Page loaded, initializing...');
            
            // Select All
            $('#selectAll').on('change', function() {
                console.log('Select All clicked:', this.checked);
                $('.donation-checkbox').prop('checked', this.checked).each(function() {
                    $(this).closest('tr').toggleClass('row-selected', this.checked);
                });
                updateTotal();
            });

            // Individual checkbox
            $('.donation-checkbox').on('change', function() {
                console.log('Checkbox changed:', this.checked);
                $(this).closest('tr').toggleClass('row-selected', this.checked);
                $('#selectAll').prop('checked', $('.donation-checkbox').length === $('.donation-checkbox:checked').length);
                updateTotal();
            });

            // Row click
            $('.donation-row').on('click', function(e) {
                if ($(e.target).is('input[type="checkbox"]') || $(e.target).is('label')) return;
                const cb = $(this).find('.donation-checkbox');
                cb.prop('checked', !cb.prop('checked')).trigger('change');
            });

            function updateTotal() {
                let total = 0;
                let ids = [];
                
                $('.donation-checkbox:checked').each(function() {
                    total += parseFloat($(this).data('due'));
                    ids.push($(this).data('id'));
                });

                console.log('Total:', total, 'IDs:', ids);

                $('#<%=selectedDonationIds.ClientID %>').val(ids.join(','));
                $('#total-display').html('Total: <span style="font-size: 1.8rem;">&#2547;' + total.toFixed(2) + '</span>');

                if (total > 0) {
                    $('#payment-section').slideDown();
                    $('#PayDonationButton').prop('disabled', false);
                } else {
                    $('#payment-section').slideUp();
                    $('#PayDonationButton').prop('disabled', true);
                }
            }

            // Payment button click handler - Use AJAX
            $('#PayDonationButton').on('click', function(e) {
                e.preventDefault();
                
                const selectedValue = $('#<%=selectedDonationIds.ClientID %>').val();
                
                if (!selectedValue || selectedValue.trim() === '') {
                    alert('Please select at least one donation to pay.');
                    return false;
                }
                
                console.log('Payment button clicked with IDs:', selectedValue);
                
                // Show loading state
                $(this).prop('disabled', true).text('PROCESSING...');
                
                // Trigger server-side code via __doPostBack
                __doPostBack('PayDonationButton', '');
            });

            console.log('Checkboxes found:', $('.donation-checkbox').length);
        });
    </script>
</asp:Content>
