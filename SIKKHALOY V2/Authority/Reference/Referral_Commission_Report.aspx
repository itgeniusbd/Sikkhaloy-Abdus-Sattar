<%@ Page Title="Referral Commission Report" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Referral_Commission_Report.aspx.cs" Inherits="EDUCATION.COM.Authority.Reference.Referral_Commission_Report" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .summary-box { background:#fff; border-radius:12px; box-shadow:0 4px 16px rgba(0,0,0,.08); padding:22px 24px; margin-bottom:24px; border:1px solid #edeef2; }
        .summary-title { font-size:1.15rem; font-weight:700; color:#2c3e50; margin-bottom:16px; border-bottom:2px solid #eef0f4; padding-bottom:10px; }
        .stat-card { background:#fff; border-radius:12px; padding:18px 20px; margin-bottom:14px; box-shadow:0 4px 16px rgba(0,0,0,.08); border:1px solid #edeef2; position:relative; overflow:hidden; }
        .stat-card::before { content:""; position:absolute; top:0; left:0; width:5px; height:100%; }
        .stat-card.blue::before { background:#0d6efd; }
        .stat-card.green::before { background:#198754; }
        .stat-card.red::before { background:#dc3545; }
        .stat-card.orange::before { background:#fd7e14; }
        .stat-label { font-size:.82rem; color:#212529; text-transform:uppercase; letter-spacing:.5px; margin-bottom:6px; }
        .stat-val { font-size:1.7rem; font-weight:800; color:#212529 !important; }
        .badge-due { background:#dc3545; color:#fff; padding:3px 10px; border-radius:12px; font-size:.82rem; }
        .badge-paid { background:#198754; color:#fff; padding:3px 10px; border-radius:12px; font-size:.82rem; }
        .badge-active { background:#198754; color:#fff; padding:3px 10px; border-radius:12px; font-size:.8rem; }
        .badge-expired { background:#dc3545; color:#fff; padding:3px 10px; border-radius:12px; font-size:.8rem; }
        .payment-box { background:#f8f9fa; border-radius:12px; padding:20px; border-left:5px solid #0d6efd; }
        @media print {
            .no-print { display:none; }
            .section-card { box-shadow:none; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3><i class="fa fa-bar-chart"></i> Referral Commission Report</h3>

    <div class="summary-box no-print">
        <div class="summary-title"><i class="fa fa-filter"></i> Filter</div>
        <div class="row">
            <div class="col-md-3">
                <div class="form-group">
                    <label>Referrer</label>
                    <asp:DropDownList ID="ReferrerDropDown" runat="server" CssClass="form-control"
                        AutoPostBack="True" OnSelectedIndexChanged="ReferrerDropDown_SelectedIndexChanged"
                        AppendDataBoundItems="True">
                        <asp:ListItem Value="0">[ All Referrers ]</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="col-md-2">
                <div class="form-group">
                    <label>From Date</label>
                    <div class="input-group date ref-dp">
                        <asp:TextBox ID="FromDateTextBox" runat="server" CssClass="form-control" placeholder="dd MMM yyyy" autocomplete="off"></asp:TextBox>
                        <div class="input-group-append"><span class="input-group-text"><i class="fa fa-calendar"></i></span></div>
                    </div>
                </div>
            </div>
            <div class="col-md-2">
                <div class="form-group">
                    <label>To Date</label>
                    <div class="input-group date ref-dp">
                        <asp:TextBox ID="ToDateTextBox" runat="server" CssClass="form-control" placeholder="dd MMM yyyy" autocomplete="off"></asp:TextBox>
                        <div class="input-group-append"><span class="input-group-text"><i class="fa fa-calendar"></i></span></div>
                    </div>
                </div>
            </div>
            <div class="col-md-2">
                <div class="form-group">
                    <label>Status</label>
                    <asp:DropDownList ID="StatusDropDown" runat="server" CssClass="form-control">
                        <asp:ListItem Value="">All</asp:ListItem>
                        <asp:ListItem Value="due">Due</asp:ListItem>
                        <asp:ListItem Value="paid">Paid</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="col-md-3">
                <div class="form-group">
                    <label>&nbsp;</label><br />
                    <asp:Button ID="SearchButton" runat="server" CssClass="btn btn-primary" Text="Show Report" OnClick="SearchButton_Click" />
                    <button type="button" class="btn btn-secondary ml-1" onclick="window.print()"><i class="fa fa-print"></i> Print</button>
                </div>
            </div>
        </div>
        <asp:Label ID="ErrorLabel" runat="server" CssClass="text-danger"></asp:Label>
    </div>

    <asp:UpdatePanel ID="ReportUpdatePanel" runat="server">
        <ContentTemplate>

            <%-- Summary Cards --%>
            <asp:Panel ID="SummaryPanel" runat="server" Visible="false">
                <div class="row">
                    <div class="col-md-3">
                        <div class="stat-card blue">
                            <div class="stat-label">Total Commission</div>
                            <div class="stat-val">৳ <asp:Label ID="TotalCommLabel" runat="server" Text="0"></asp:Label></div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="stat-card green">
                            <div class="stat-label">Paid</div>
                            <div class="stat-val">৳ <asp:Label ID="TotalPaidLabel" runat="server" Text="0"></asp:Label></div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="stat-card red">
                            <div class="stat-label">Due</div>
                            <div class="stat-val">৳ <asp:Label ID="TotalDueLabel" runat="server" Text="0"></asp:Label></div>
                        </div>
                    </div>
                    <div class="col-md-3">
                        <div class="stat-card orange">
                            <div class="stat-label">Total Referrers</div>
                            <div class="stat-val"><asp:Label ID="TotalRefLabel" runat="server" Text="0"></asp:Label></div>
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <%-- Referrer wise summary --%>
            <div class="summary-box">
                <div class="summary-title"><i class="fa fa-table"></i> Referrer-wise Commission Summary</div>
                <asp:GridView ID="RefSummaryGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid"
                    DataKeyNames="ReferenceID" OnSelectedIndexChanged="RefSummaryGridView_SelectedIndexChanged"
                    OnRowCommand="GridView_RowCommand">
                    <Columns>
                        <asp:CommandField HeaderText="Details" ShowSelectButton="True" SelectText="<i class='fa fa-list'></i>" />
                        <asp:BoundField DataField="Reference_Name" HeaderText="Referrer Name" />
                        <asp:BoundField DataField="Reference_Phone" HeaderText="Mobile" />
                        <asp:BoundField DataField="TotalSchools" HeaderText="Institutions" />
                        <asp:TemplateField HeaderText="Total Commission (৳)">
                            <ItemTemplate><strong>৳ <%# string.Format("{0:N0}", Eval("TotalCommission")) %></strong></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Paid (৳)">
                            <ItemTemplate><span class="badge-paid">৳ <%# string.Format("{0:N0}", Eval("PaidAmount")) %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Due (৳)">
                            <ItemTemplate><span class="badge-due">৳ <%# string.Format("{0:N0}", Eval("DueAmount")) %></span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Payment">
                            <ItemTemplate>
                                <asp:LinkButton runat="server" CssClass="btn btn-xs btn-success"
                                    CommandName="Pay" CommandArgument='<%# Eval("ReferenceID") %>'>
                                    <i class="fa fa-money"></i> Pay
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <SelectedRowStyle CssClass="Selected" />
                </asp:GridView>
            </div>

            <%-- Institution wise commission details --%>
            <asp:Panel ID="DetailPanel" runat="server" Visible="false">
                <div class="summary-box">
                    <div class="summary-title">
                        <i class="fa fa-university"></i> Institution-wise Commission —
                        <asp:Label ID="DetailRefNameLabel" runat="server" CssClass="text-primary"></asp:Label>
                    </div>
                    <asp:GridView ID="DetailGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid">
                        <Columns>
                            <asp:BoundField DataField="SchoolName" HeaderText="Institution Name" />
                            <asp:TemplateField HeaderText="Commission %">
                                <ItemTemplate><span style="color:#17a2b8;font-weight:600"><%# Eval("Percentage") %>%</span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="School_SignUp_Date" HeaderText="Signup Date" DataFormatString="{0:d MMM yyyy}" />
                            <asp:BoundField DataField="End_Reference_Date" HeaderText="Expiry Date" DataFormatString="{0:d MMM yyyy}" />
                            <asp:BoundField DataField="TotalServiceCharge" HeaderText="Total Service Charge (৳)" DataFormatString="{0:N0}" />
                            <asp:TemplateField HeaderText="Commission (৳)">
                                <ItemTemplate><strong class="text-primary">৳ <%# string.Format("{0:N0}", Eval("CommissionAmount")) %></strong></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Paid (৳)">
                                <ItemTemplate><span class="badge-paid">৳ <%# string.Format("{0:N0}", Eval("PaidAmount")) %></span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Due (৳)">
                                <ItemTemplate><span class="badge-due">৳ <%# string.Format("{0:N0}", Eval("DueAmount")) %></span></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <asp:Label runat="server" Visible='<%# (Eval("End_Reference_Date") != DBNull.Value && (DateTime)Eval("End_Reference_Date") < DateTime.Today) %>'>
                                        <span class="badge-expired">Expired</span>
                                    </asp:Label>
                                    <asp:Label runat="server" Visible='<%# (Eval("End_Reference_Date") == DBNull.Value || (DateTime)Eval("End_Reference_Date") >= DateTime.Today) %>'>
                                        <span class="badge-active">Active</span>
                                    </asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>

            <%-- Payment record modal --%>
            <div class="modal fade" id="PaymentModal" tabindex="-1" role="dialog" aria-hidden="true">
                <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title">
                                <i class="fa fa-money"></i> Commission Payment Record —
                                <asp:Label ID="PayRefNameLabel" runat="server" CssClass="text-white"></asp:Label>
                            </h5>
                            <button type="button" class="close text-white" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            <asp:UpdatePanel ID="PaymentUpdatePanel" runat="server">
                                <ContentTemplate>
                                    <asp:Panel ID="PaymentPanel" runat="server" Visible="false">
                                        <asp:HiddenField ID="PayReferenceIDHidden" runat="server" Value="0" />
                                        <div class="row">
                                            <div class="col-md-3">
                                                <div class="form-group">
                                                    <label>Amount (৳) <span class="text-danger">*</span></label>
                                                    <asp:TextBox ID="PayAmountTextBox" runat="server" CssClass="form-control" placeholder="Enter amount"></asp:TextBox>
                                                </div>
                                            </div>
                                            <div class="col-md-3">
                                                <div class="form-group">
                                                    <label>Date <span class="text-danger">*</span></label>
                                                    <div class="input-group date ref-dp">
                                                        <asp:TextBox ID="PayDateTextBox" runat="server" CssClass="form-control" placeholder="dd MMM yyyy" autocomplete="off"></asp:TextBox>
                                                        <div class="input-group-append"><span class="input-group-text"><i class="fa fa-calendar"></i></span></div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-md-3">
                                                <div class="form-group">
                                                    <label>Paid By</label>
                                                    <asp:TextBox ID="PaidByTextBox" runat="server" CssClass="form-control" placeholder="Name"></asp:TextBox>
                                                </div>
                                            </div>
                                            <div class="col-md-3">
                                                <div class="form-group">
                                                    <label>Payment Method</label>
                                                    <asp:DropDownList ID="PayMethodDropDown" runat="server" CssClass="form-control">
                                                        <asp:ListItem>Cash</asp:ListItem>
                                                        <asp:ListItem>Bank Transfer</asp:ListItem>
                                                        <asp:ListItem>bKash</asp:ListItem>
                                                        <asp:ListItem>Nagad</asp:ListItem>
                                                        <asp:ListItem>Cheque</asp:ListItem>
                                                        <asp:ListItem>Others</asp:ListItem>
                                                    </asp:DropDownList>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-md-5">
                                                <div class="form-group">
                                                    <label>Note</label>
                                                    <asp:TextBox ID="PayNoteTextBox" runat="server" CssClass="form-control" placeholder="Optional note"></asp:TextBox>
                                                </div>
                                            </div>
                                            <div class="col-md-4">
                                                <div class="form-group">
                                                    <label>OTP <span class="text-danger">*</span></label>
                                                    <asp:TextBox ID="PayOTPTextBox" runat="server" CssClass="form-control" placeholder="6 digit OTP" MaxLength="6"></asp:TextBox>
                                                    <small class="text-muted">
                                                        <i class="fa fa-info-circle"></i> OTP sent to referrer's phone. Valid for 5 minutes.
                                                        <asp:Button ID="ResendPayOTPButton" runat="server" CssClass="btn btn-link btn-sm p-0 ml-2" Text="Resend" OnClick="SendPayOTPButton_Click" />
                                                        <span id="payResendTimer" class="ml-1" style="display:none;"></span>
                                                    </small>
                                                </div>
                                            </div>
                                            <div class="col-md-3">
                                                <div class="form-group">
                                                    <label>&nbsp;</label><br />
                                                    <asp:Button ID="SendOTPButton" runat="server" CssClass="btn btn-warning" Text="Send OTP" OnClick="SendPayOTPButton_Click" />
                                                    <asp:Button ID="SavePayButton" runat="server" CssClass="btn btn-success" Text="Save Payment" OnClick="SavePayButton_Click" />
                                                </div>
                                            </div>
                                        </div>
                                        <asp:Label ID="PayMsgLabel" runat="server" CssClass="text-success font-weight-bold"></asp:Label>

                                        <h6 class="mt-3"><i class="fa fa-history"></i> Previous Payment Records</h6>
                                        <asp:GridView ID="PayHistoryGridView" runat="server" AutoGenerateColumns="False" CssClass="mGrid">
                                            <Columns>
                                                <asp:BoundField DataField="PaidDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy hh:mm tt}" />
                                                <asp:TemplateField HeaderText="Amount (৳)">
                                                    <ItemTemplate><strong>৳ <%# string.Format("{0:N0}", Eval("Amount")) %></strong></ItemTemplate>
                                                </asp:TemplateField>
                                                <asp:BoundField DataField="Paid_By" HeaderText="Paid By" />
                                                <asp:BoundField DataField="Payment_Method" HeaderText="Method" />
                                                <asp:BoundField DataField="Reference_Phone" HeaderText="OTP Phone" />
                                                <asp:BoundField DataField="Note" HeaderText="Note" />
                                            </Columns>
                                        </asp:GridView>
                                    </asp:Panel>
                                </ContentTemplate>
                                <Triggers>
                                    <asp:AsyncPostBackTrigger ControlID="SendOTPButton" EventName="Click" />
                                    <asp:AsyncPostBackTrigger ControlID="ResendPayOTPButton" EventName="Click" />
                                    <asp:AsyncPostBackTrigger ControlID="SavePayButton" EventName="Click" />
                                    <asp:AsyncPostBackTrigger ControlID="PayHistoryGridView" EventName="RowCommand" />
                                </Triggers>
                            </asp:UpdatePanel>
                        </div>
                    </div>
                </div>
            </div>

        </ContentTemplate>
    </asp:UpdatePanel>

    <script>
        function initRefDp() {
            $('.ref-dp input').each(function () {
                var $input = $(this);
                $input.datepicker('destroy');
                $input.datepicker({
                    format: 'dd MMM yyyy',
                    autoclose: true,
                    todayHighlight: true,
                    orientation: 'bottom auto'
                });
            });
        }
        $(function () {
            initRefDp();
            $('#PaymentModal').on('hidden.bs.modal', function () {
                $('.modal-backdrop').remove();
                $('body').removeClass('modal-open').css('padding-right', '');
            });
        });
        var prm = Sys.WebForms.PageRequestManager.getInstance();
        prm.add_endRequest(function () {
            initRefDp();
            var $modal = $('#PaymentModal');
            if ($modal.length && $modal.find('[id*="PaymentPanel"]').is(':visible')) {
                $modal.modal('show');
            }
            $modal.off('hidden.bs.modal').on('hidden.bs.modal', function () {
                $('.modal-backdrop').remove();
                $('body').removeClass('modal-open').css('padding-right', '');
            });
        });

        var payResendCountdown = 60;
        var payResendTimerInterval;
        function startPayResendTimer() {
            var resendBtn = $('[id$=ResendPayOTPButton]');
            var timerSpan = $('#payResendTimer');
            resendBtn.prop('disabled', true).addClass('disabled');
            timerSpan.show();
            payResendCountdown = 60;
            payResendTimerInterval = setInterval(function () {
                payResendCountdown--;
                timerSpan.text('(' + payResendCountdown + 's)');
                if (payResendCountdown <= 0) {
                    clearInterval(payResendTimerInterval);
                    resendBtn.prop('disabled', false).removeClass('disabled');
                    timerSpan.hide();
                }
            }, 1000);
        }
        function payOTPSent() { startPayResendTimer(); }
    </script>
</asp:Content>
