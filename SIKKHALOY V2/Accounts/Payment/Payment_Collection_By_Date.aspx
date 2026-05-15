<%@ Page Title="Collect Payment By Date" Language="C#" MasterPageFile="~/BASIC.Master" AutoEventWireup="true" CodeBehind="Payment_Collection_By_Date.aspx.cs" Inherits="EDUCATION.COM.ACCOUNTS.Payment.Payment_Collection_By_Date" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="CSS/Payment_Collection.css?v=1" rel="stylesheet" />
    <style>
        .pc-table { width:100%; border-collapse:collapse; font-size:14px; border:2px solid #bbb; }
        .pc-table th { background:#222; color:#fff; padding:10px 8px; white-space:nowrap; text-align:center; vertical-align:middle; border:1px solid #444; }
        .pc-table td { padding:7px 8px; border:1px solid #ddd; vertical-align:middle; text-align:center; }
        .pc-table td:nth-child(3), .pc-table td:nth-child(4), .pc-table td:nth-child(5) { text-align:left; }
        .due-checkbox { width:16px; height:16px; cursor:pointer; display:inline-block !important; visibility:visible !important; }
        input[name="smsStatus"] { opacity:1 !important; position:relative !important; left:auto !important; width:16px; height:16px; cursor:pointer; -webkit-appearance:radio !important; appearance:radio !important; display:inline-block !important; }
        .pc-table tr:nth-child(even) td { background:#f4f8ff; }
        .pc-table tr:nth-child(odd) td { background:#fff; }
        .pc-table tr.overdue td { color:#dc0000; }
        .pc-table tr.row-selected td { background:#1CAA56 !important; color:#fff !important; font-weight:bold; }
        .pc-table tr.others-payment td { background:#5fc42a !important; color:#fff !important; }
        .pc-input { width:100px; padding:5px 6px; border:1px solid #bbb; border-radius:4px; font-size:13px; text-align:center; display:block; margin:0 auto; }
        .pc-section-title { font-weight:bold; margin:18px 0 8px; font-size:1rem; border-left:4px solid #2196f3; padding-left:8px; }
        .pc-receipt-table { width:100%; font-size:13px; }
        .pc-receipt-table td { padding:4px 6px; border-bottom:1px solid #eee; }
        #payment-submit { display:none; }
        #total-pay-amount { font-weight:bold; font-size:1rem; color:#1a237e; }
        #grand-total-fixed { display:none; opacity:.95; position:fixed; width:87%; background:#fff; bottom:0; box-shadow:0 0 16px -1px rgba(40,40,40,.75); margin-left:-15px; text-align:center; font-size:1.5rem; font-weight:bold; padding:1.5rem 0; z-index:999; }
        @media(max-width:767px){ #grand-total-fixed { width:100%; } }
        .spinner-wrap { text-align:center; padding:30px; }
        .spin-icon { width:2rem; height:2rem; border:.25em solid #ccc; border-top-color:#2196f3; border-radius:50%; animation:spin .75s linear infinite; display:inline-block; }
        @keyframes spin { to { transform:rotate(360deg); } }
        /* Full-page blur overlay */
        #pc-overlay { display:none; position:fixed; inset:0; z-index:99999; background:rgba(255,255,255,0.55); backdrop-filter:blur(4px); -webkit-backdrop-filter:blur(4px); }
        #pc-overlay .pc-loader-box { position:absolute; top:50%; left:50%; transform:translate(-50%,-50%); background:#fff; border-radius:14px; box-shadow:0 8px 40px rgba(0,0,0,0.18); padding:36px 48px; text-align:center; min-width:220px; }
        #pc-overlay .pc-loader-spinner { width:52px; height:52px; border:5px solid #e3f0ff; border-top-color:#2196f3; border-radius:50%; animation:spin .8s linear infinite; margin:0 auto 16px; }
        #pc-overlay .pc-loader-bar-wrap { width:180px; height:6px; background:#e3f0ff; border-radius:99px; overflow:hidden; margin:12px auto 0; }
        #pc-overlay .pc-loader-bar { height:100%; width:0; background:linear-gradient(90deg,#4cd964,#2196f3,#9c27b0); border-radius:99px; animation:pc-bar-move 1.4s ease-in-out infinite; }
        @keyframes pc-bar-move { 0%{width:0;margin-left:0} 50%{width:70%;margin-left:15%} 100%{width:0;margin-left:100%} }
        #pc-overlay .pc-loader-text { font-size:14px; color:#555; margin-top:8px; font-weight:500; }
        .paid-date-wrap { display:inline-flex; align-items:center; gap:6px; background:#e8f4fd; border:1px solid #2196f3; border-radius:6px; padding:6px 12px; }
        .paid-date-wrap label { margin:0; font-weight:600; color:#1565c0; white-space:nowrap; }
        #txtPaidDate { border:1px solid #90caf9; border-radius:4px; padding:4px 8px; font-size:14px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
    <h3>Collect Payment <small class="text-muted" style="font-size:14px;">(By Date)</small></h3>

    <!-- Full-page blur overlay -->
    <div id="pc-overlay">
        <div class="pc-loader-box">
            <div class="pc-loader-spinner"></div>
            <div class="pc-loader-text">লোড হচ্ছে...</div>
            <div class="pc-loader-bar-wrap"><div class="pc-loader-bar"></div></div>
        </div>
    </div>

    <!-- Search -->
    <div class="form-inline mb-3" style="gap:8px;">    
        <input type="text" id="txtStudentID" class="form-control" placeholder="Enter Student ID" autocomplete="off" style="width:180px;" />
        <button id="btnFind" type="button" class="btn btn-primary"><i class="fa fa-search"></i> Find</button>
        <span id="searchError" class="text-danger" style="display:none;">Student ID প্রয়োজন</span>
    </div>

    <!-- Main Area -->
    <div id="studentInfoArea" style="display:none;">

        <!-- Student Card + Recent Payments -->
        <div class="row">
            <div class="col-lg-8">
                <div class="z-depth-1 p-3 mb-3" id="studentCard"></div>
            </div>
            <div class="col-lg-4">
                <div id="recentPaymentsArea"></div>
            </div>
        </div>

        <!-- Current Due Banner -->
        <div id="currentDueBanner" class="current-due-total"></div>

        <!-- Current Session Due Table -->
        <div class="table-responsive" id="payment-container">
            <table class="pc-table">
                <thead>
                    <tr>
                        <th></th><th>Session</th><th>Class</th><th>Role</th><th>Pay For</th>
                        <th>End Date</th><th>Fee</th><th>Concession</th><th>Late Fee</th><th>LF Discount</th>
                        <th>Paid</th><th>Due</th><th>Pay</th>
                    </tr>
                </thead>
                <tbody id="dueTableBody"></tbody>
            </table>
        </div>

        <!-- Other Session Due -->
        <div id="otherSessionArea" style="display:none;">
            <div class="pc-section-title">OTHERS SESSION DUE</div>
            <div class="table-responsive">
                <table class="pc-table">
                    <thead>
                        <tr>
                            <th></th><th>Session</th><th>Class</th><th>Role</th><th>Pay For</th>
                            <th>End Date</th><th>Fee</th><th>Concession</th><th>Late Fee</th><th>LF Discount</th>
                            <th>Paid</th><th>Due</th><th>Pay</th>
                        </tr>
                    </thead>
                    <tbody id="otherDueTableBody"></tbody>
                </table>
            </div>
        </div>

        <!-- Submit Area -->
        <div id="payment-submit" class="mt-4">
            <!-- Total Amount -->
            <div id="total-pay-amount" class="mb-3" style="font-size:1.4rem; font-weight:700; color:#1a237e; background:#e8f4fd; border-left:5px solid #2196f3; padding:10px 18px; border-radius:6px;"></div>

            <!-- Main Action Bar -->
            <div style="background:#f8f9fa; border:1px solid #dee2e6; border-radius:10px; padding:18px 20px;">
                <div class="d-flex flex-wrap align-items-center" style="gap:12px;">

                    <!-- Account Dropdown -->
                    <div style="display:flex; flex-direction:column; gap:3px;">
                        <label style="font-size:12px; font-weight:600; color:#555; margin:0;">একাউন্ট</label>
                        <select id="ddlAccount" class="form-control" style="width:150px; height:42px; font-size:14px;"></select>
                    </div>

                    <!-- Add More Button -->
                    <div style="display:flex; flex-direction:column; gap:3px;">
                        <label style="font-size:12px; color:transparent; margin:0;">-</label>
                        <button type="button" class="btn btn-outline-success" style="height:42px; font-size:14px; padding:0 18px; font-weight:600;" data-toggle="modal" data-target="#addMoreModal">
                            <i class="fa fa-plus"></i> Add More
                        </button>
                    </div>

                    <!-- Date Picker -->
                    <div style="display:flex; flex-direction:column; gap:3px;">
                        <label for="txtPaidDate" style="font-size:12px; font-weight:600; color:#1565c0; margin:0;"><i class="fa fa-calendar"></i> পেমেন্টের তারিখ</label>
                        <input type="date" id="txtPaidDate" class="form-control" style="height:42px; font-size:14px; border:2px solid #2196f3; border-radius:6px; padding:4px 10px; min-width:170px;" />
                    </div>

                    <!-- SMS Status -->
                    <div style="display:flex; flex-direction:column; gap:6px;">
                        <label style="font-size:12px; font-weight:600; color:#555; margin:0;">SMS সেটিং</label>
                        <div style="display:flex; gap:14px; align-items:center; background:#fff; border:1px solid #ccc; border-radius:6px; padding:6px 14px; height:42px;">
                            <label style="margin:0; font-size:14px; font-weight:500; cursor:pointer;">
                                <input type="radio" name="smsStatus" id="rbActive" value="1" style="opacity:1 !important; position:relative !important; left:auto !important; width:16px; height:16px; margin-right:5px; cursor:pointer; -webkit-appearance:radio !important; appearance:radio !important; display:inline-block !important;" />
                                SMS Active
                            </label>
                            <label style="margin:0; font-size:14px; font-weight:500; cursor:pointer;">
                                <input type="radio" name="smsStatus" id="rbInactive" value="0" style="opacity:1 !important; position:relative !important; left:auto !important; width:16px; height:16px; margin-right:5px; cursor:pointer; -webkit-appearance:radio !important; appearance:radio !important; display:inline-block !important;" />
                                SMS Inactive
                            </label>
                        </div>
                    </div>

                    <!-- PAY Button -->
                    <div style="display:flex; flex-direction:column; gap:3px;">
                        <label style="font-size:12px; color:transparent; margin:0;">-</label>
                        <button id="btnPay" type="button" class="btn btn-primary" style="height:42px; font-size:16px; font-weight:700; padding:0 32px; border-radius:8px; letter-spacing:.5px;">
                            <i class="fa fa-money"></i> PAY
                        </button>
                    </div>

                    <!-- Concession Update Button -->
                    <div style="display:flex; flex-direction:column; gap:3px;">
                        <label style="font-size:12px; color:transparent; margin:0;">-</label>
                        <button id="btnUpdateConcession" type="button" class="btn btn-warning" style="height:42px; font-size:14px; font-weight:600; padding:0 18px; border-radius:8px; display:none;">
                            <i class="fa fa-refresh"></i> Concession / Late Fee Update
                        </button>
                    </div>

                    <!-- Error Message -->
                    <span id="payError" class="text-danger" style="font-size:14px; font-weight:600;"></span>
                </div>

                <!-- SMS Template Row -->
                <div class="mt-3" style="border-top:1px solid #dee2e6; padding-top:12px;">
                    <span style="background:#fff3cd; border:1px solid #ffc107; border-radius:6px; padding:8px 16px; display:inline-flex; align-items:center; gap:10px; font-size:13px;">
                        <i class="fa fa-info-circle" style="color:#ff9800; font-size:16px;"></i>
                        <span style="color:#856404; font-weight:600;">SMS Template Active</span>
                        <a href="/SMS/SMS_Template.aspx" class="btn btn-warning btn-sm" style="font-size:13px; padding:4px 12px; font-weight:600;">
                            <i class="fa fa-edit"></i> Edit Template
                        </a>
                    </span>
                </div>
            </div>
        </div>
    </div>

    <!-- Grand Total Fixed Bottom -->
    <div id="grand-total-fixed"></div>

    <!-- Add More Payment Modal -->
    <div class="modal fade" id="addMoreModal" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <div class="title">Add More Payment</div>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="form-group">
                        <label>Role <a href="Create_Payment_Roles.aspx">Add New Role</a></label>
                        <select id="ddlRole" class="form-control"></select>
                    </div>
                    <div class="form-group">
                        <label>Pay For <span class="text-danger">*</span></label>
                        <input type="text" id="txtPayFor" class="form-control" placeholder="Input Pay For" />
                    </div>
                    <div class="form-group">
                        <label>Amount <span class="text-danger">*</span></label>
                        <input type="text" id="txtAmount" class="form-control" placeholder="Input amount" />
                    </div>
                    <div class="form-group">
                        <label>Concession</label>
                        <input type="text" id="txtConcession" class="form-control" placeholder="Input Concession" autocomplete="off" />
                    </div>
                    <div class="form-group">
                        <button id="btnAddMoreSave" type="button" class="btn btn-primary">Add Payment</button>
                        <span id="addMoreError" class="text-danger ml-2"></span>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- All Paid Records Modal -->
    <div class="modal fade" id="allPaidModal" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Student Paid Records (Current Session)</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body" id="allPaidModalBody">
                    <div class="text-center py-3"><div class="spin-icon"></div></div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-danger" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Previous Year Paid Records Modal -->
    <div class="modal fade" id="prevYearModal" tabindex="-1" role="dialog">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header" style="background:#6f42c1; color:#fff;">
                    <h4 class="modal-title"><i class="fa fa-history"></i> Previous Year Paid Records</h4>
                    <button type="button" class="close" style="color:#fff;" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body" id="prevYearModalBody">
                    <div class="text-center py-3"><div class="spin-icon"></div></div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-danger" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>

    <!-- Receipt Detail Modal -->
    <div class="modal fade" id="receiptDetailModal" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Paid Record Details</h5>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body" id="receiptDetailBody">
                    <div class="text-center py-3"><div class="spin-icon"></div></div>
                </div>
            </div>
        </div>
    </div>

<script>
(function () {
    var state = {
        studentID: '', studentDbID: 0, studentClassID: 0, classID: 0,
        educationYearID: 0, registrationID: 0, smsPhoneNo: '', studentName: ''
    };

    // Set today's date as default in paid date picker
    (function setDefaultDate() {
        var today = new Date();
        var yyyy = today.getFullYear();
        var mm = String(today.getMonth() + 1).padStart(2, '0');
        var dd = String(today.getDate()).padStart(2, '0');
        document.getElementById('txtPaidDate').value = yyyy + '-' + mm + '-' + dd;
    })();

    // Typeahead
    $('#txtStudentID').typeahead({
        source: function (q, res) {
            $.ajax({ url: '/Handeler/Student_IDs.asmx/GetStudentID', type: 'POST',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify({ ids: q }), dataType: 'json',
                success: function (r) { res(JSON.parse(r.d)); }
            });
        }
    });

    document.getElementById('txtStudentID').addEventListener('keyup', function (e) {
        if (e.keyCode === 13) document.getElementById('btnFind').click();
    });

    document.getElementById('btnFind').addEventListener('click', function () {
        var id = document.getElementById('txtStudentID').value.trim();
        if (!id) { document.getElementById('searchError').style.display = ''; return; }
        document.getElementById('searchError').style.display = 'none';
        sessionStorage.setItem('pcbd_lastStudentID', id);
        loadStudent(id);
    });

    // ── Restore last searched student on back navigation ──────────────────────
    (function restoreLastStudent() {
        var saved = sessionStorage.getItem('pcbd_lastStudentID');
        if (saved && document.referrer.indexOf('Money_Receipt.aspx') !== -1) {
            document.getElementById('txtStudentID').value = saved;
            loadStudent(saved);
        } else {
            sessionStorage.removeItem('pcbd_lastStudentID');
        }
    })();

    // ── Load Student ──────────────────────────────────────────────────────────
    function loadStudent(id) {
        showLoading(true);
        document.getElementById('studentInfoArea').style.display = 'none';
        ajax('GetStudentData', { studentID: id }, function (d) {
            if (!d || !d.StudentID) { showLoading(false); alert('Student not found!'); return; }
            state.studentID = id; state.studentDbID = d.StudentID;
            state.studentClassID = d.StudentClassID; state.classID = d.ClassID;
            state.educationYearID = d.EducationYearID; state.registrationID = d.RegistrationID;
            state.smsPhoneNo = d.SMSPhoneNo; state.studentName = d.StudentsName;
            renderCard(d);
            loadAccounts(); loadRoles(); loadSMSSetting(); loadConcessionPerm();
            loadDues(function () { showLoading(false); loadRecentPayments(); });
        });
    }

    function renderCard(d) {
        var sc = d.Status === 'Active' ? 'active-status' : 'in-active-status';
        document.getElementById('studentCard').innerHTML =
            '<div class="d-flex flex-sm-row flex-column text-center text-sm-left">' +
            '<div class="student-photo"><img src="/Handeler/Student_Photo.ashx?SID=' + d.StudentImageID +
            '" class="img-thumbnail rounded-circle img-fluid z-depth-1" style="height:160px;width:160px;" />' +
            '<div class="student-activation ' + sc + '">' + d.Status + '</div></div>' +
            '<div class="info"><ul>' +
            '<li><strong>(' + d.ID + ') ' + d.StudentsName + '</strong></li>' +
            '<li><b>Fathers Name: </b>' + (d.FathersName || '') + '</li>' +
            '<li><b>Class:</b> ' + (d.Class || '') + '</li>' +
            '<li>Roll No:' + (d.RollNo || '') + (d.Section ? ', Section: ' + d.Section : '') + (d.Shift ? ', Shift: ' + d.Shift : '') + '</li>' +
            '<li><b>Phone: </b>' + (d.SMSPhoneNo || '') + '</li>' +
            '<li><b>Session: </b>' + (d.EducationYear || '') +
            ' <a target="_blank" href="/Admission/Student_Report/Report.aspx?Student=' + d.StudentID + '&Student_Class=' + d.StudentClassID + '">Full Details</a>' +
            ' &mdash; <a target="_blank" href="../../Admission/New_Student_Admission/Admission_Form.aspx?Student=' + d.StudentID + '&StudentClass=' + d.StudentClassID + '">Print Admission Form</a></li>' +
            '</ul><button type="button" data-toggle="modal" data-target="#addMoreModal" class="btn btn-outline-success btn-md m-0">Add More Payment</button></div></div>';
        document.getElementById('studentInfoArea').style.display = '';
    }

    // ── Load Due Tables ───────────────────────────────────────────────────────
    function loadDues(onComplete) {
        ajax('GetCurrentDue', { studentID: state.studentID }, function (due) {
            document.getElementById('currentDueBanner').textContent = 'CURRENT DUE: ' + fmtAmt(due) + ' TK';
        });
        ajax('GetDues', { studentID: state.studentID }, function (d) {
            var cur = d.CurrentDues || [], oth = d.OtherDues || [];
            renderTable('dueTableBody', cur, 'DueCB');
            if (oth.length > 0) {
                document.getElementById('otherSessionArea').style.display = '';
                renderTable('otherDueTableBody', oth, 'OtherCB');
            } else {
                document.getElementById('otherSessionArea').style.display = 'none';
            }
            document.getElementById('payment-submit').style.display = (cur.length || oth.length) ? 'block' : 'none';
            document.querySelectorAll('.due-checkbox').forEach(function (c) { c.checked = false; });
            if (typeof onComplete === 'function') onComplete();
        });
    }

    function renderTable(tbId, rows, cbName) {
        var tb = document.getElementById(tbId);
        if (!rows || !rows.length) {
            tb.innerHTML = '<tr><td colspan="13" class="text-center text-muted py-3">No due found</td></tr>';
            return;
        }
        var html = '';
        rows.forEach(function (r) {
            var today = new Date(); today.setHours(0, 0, 0, 0);
            var end = parseDate(r.EndDate), start = parseDate(r.StartDate);
            var rc = '';
            if (end < today) rc = 'overdue';
            if (+start === +end && +start === +today) rc = 'others-payment';
            html += '<tr class="' + rc + '" data-payorderid="' + r.PayOrderID +
                '" data-due="' + r.Due + '" data-fee="' + r.Amount +
                '" data-latefee="' + (r.LateFee || 0) + '" data-paid="' + (r.PaidAmount || 0) + '">' +
                '<td><input type="checkbox" class="due-checkbox" name="' + cbName + '" /></td>' +
                '<td>' + (r.EducationYear || '') + '</td>' +
                '<td>' + (r.Class || '') + '</td>' +
                '<td>' + (r.Role || '') + '</td>' +
                '<td>' + (r.PayFor || '') + '</td>' +
                '<td>' + fmtDate(r.EndDate) + '</td>' +
                '<td>' + (r.Amount || 0) + '</td>' +
                '<td><input type="text" class="pc-input concession-input" value="' + (r.Discount || 0) + '" disabled autocomplete="off" /></td>' +
                '<td><input type="text" class="pc-input latefee-input" value="' + (r.LateFee || 0) + '" disabled autocomplete="off" />' +
                '<input type="hidden" class="prev-latefee" value="' + (r.LateFee || 0) + '" /></td>' +
                '<td>' + (r.LateFeeDiscount || 0) + '</td>' +
                '<td>' + (r.PaidAmount || 0) + '</td>' +
                '<td class="due-cell">' + fmtAmt(r.Due) + '</td>' +
                '<td><input type="text" class="pc-input due-input" value="' + fmtAmt(r.Due) + '" disabled autocomplete="off" /></td>'
                '</tr>';
        });
        tb.innerHTML = html;
    }

    // ── Recent Payments ───────────────────────────────────────────────────────
    function loadRecentPayments() {
        ajax('GetRecentPayments', { studentID: state.studentID }, function (d) {
            var html = '<div class="mb-2"><button type="button" class="btn btn-outline-purple btn-sm w-100" id="btnPrevYear" style="border-color:#6f42c1; color:#6f42c1;"><i class="fa fa-history"></i> Previous Year Receipts</button></div>';
            if (d && d.length) {
                html += '<table class="pc-receipt-table"><thead><tr><th>Receipt</th><th style="text-align:right">Paid</th></tr></thead><tbody>';
                d.slice(0, 5).forEach(function (r) {
                    html += '<tr><td><a href="#" class="rcpt-link" data-id="' + r.MoneyReceiptID + '">' + r.MoneyReceipt_SN +
                        '</a><small class="d-block text-muted">' + r.PaidDate + '</small></td>' +
                        '<td style="text-align:right">' + r.TotalAmount + ' Tk<br>' +
                        '<a href="#" class="print-link" data-id="' + r.MoneyReceiptID + '"><i class="fa fa-print"></i> Print</a></td></tr>';
                });
                html += '</tbody></table>';
                if (d.length > 5) html += '<div class="mt-2"><button type="button" class="btn btn-outline-success btn-sm" id="btnViewAll">View All</button></div>';
            } else {
                html += '<div class="text-muted">No payment records</div>';
            }
            document.getElementById('recentPaymentsArea').innerHTML = html;
            bindRecentEvents();
        });
    }

    function bindRecentEvents() {
        document.querySelectorAll('.rcpt-link').forEach(function (a) {
            a.addEventListener('click', function (e) { e.preventDefault(); openReceiptDetail(this.dataset.id); });
        });
        document.querySelectorAll('.print-link').forEach(function (a) {
            a.addEventListener('click', function (e) { e.preventDefault(); doPrint(this.dataset.id); });
        });
        var bva = document.getElementById('btnViewAll');
        if (bva) bva.addEventListener('click', function () { $('#allPaidModal').modal('show'); loadAllPaid(); });
        var bpy = document.getElementById('btnPrevYear');
        if (bpy) bpy.addEventListener('click', function () { $('#prevYearModal').modal('show'); loadPrevYearPaid(); });
    }

    function openReceiptDetail(id) {
        document.getElementById('receiptDetailBody').innerHTML = '<div class="text-center py-3"><div class="spin-icon"></div></div>';
        $('#receiptDetailModal').modal('show');
        ajax('GetReceiptDetail', { moneyReceiptID: parseInt(id) }, function (d) {
            if (!d || !d.length) { document.getElementById('receiptDetailBody').innerHTML = 'No records.'; return; }
            var tot = 0;
            var html = '<table class="table table-sm"><thead><tr><th>Pay For</th><th>Role</th><th>Paid</th></tr></thead><tbody>';
            d.forEach(function (r) { tot += r.PaidAmount; html += '<tr><td>' + r.PayFor + '</td><td>' + r.Role + '</td><td>' + r.PaidAmount + '</td></tr>'; });
            html += '</tbody><tfoot><tr><th colspan="2">Total</th><th>' + fmtAmt(tot) + ' Tk</th></tr></tfoot></table>';
            document.getElementById('receiptDetailBody').innerHTML = html;
        });
    }

    function loadAllPaid() {
        document.getElementById('allPaidModalBody').innerHTML = '<div class="text-center py-3"><div class="spin-icon"></div></div>';
        ajax('GetAllPaidRecords', { studentID: state.studentID }, function (d) {
            if (!d || !d.length) { document.getElementById('allPaidModalBody').innerHTML = '<p class="text-muted">No records.</p>'; return; }
            var html = '<table class="table table-sm table-bordered"><thead><tr><th>Receipt No</th><th>Printed Receipt</th><th>Paid Date</th><th>Amount</th><th>Re-Print</th><th>Received By</th></tr></thead><tbody>';
            d.forEach(function (r) {
                html += '<tr><td><a href="#" class="rcpt-link2" data-id="' + r.MoneyReceiptID + '">' + r.MoneyReceipt_SN + '</a></td>' +
                    '<td>' + (r.PrintedReceiptNo || '-') + '</td><td>' + r.PaidDate + '</td><td>' + r.TotalAmount + ' Tk</td>' +
                    '<td><a href="#" class="print-link2" data-id="' + r.MoneyReceiptID + '"><i class="fa fa-print"></i> Print</a></td>' +
                    '<td>' + (r.ReceivedBy || '') + '</td></tr>';
            });
            html += '</tbody></table>';
            document.getElementById('allPaidModalBody').innerHTML = html;
            document.querySelectorAll('.rcpt-link2').forEach(function (a) {
                a.addEventListener('click', function (e) { e.preventDefault(); $('#allPaidModal').modal('hide'); openReceiptDetail(this.dataset.id); });
            });
            document.querySelectorAll('.print-link2').forEach(function (a) {
                a.addEventListener('click', function (e) { e.preventDefault(); doPrint(this.dataset.id); });
            });
        });
    }

    function doPrint(id) {
        ajax('EncryptReceiptID', { moneyReceiptID: parseInt(id), studentID: state.studentID }, function (d) {
            if (d && d.MRid) window.location.href = 'Money_Receipt_By_Date.aspx?mN_R=' + d.MRid + '&s_icD=' + d.Sid;
        });
    }

    function loadPrevYearPaid() {
        document.getElementById('prevYearModalBody').innerHTML = '<div class="text-center py-3"><div class="spin-icon"></div></div>';
        ajax('GetPreviousYearPaidRecords', { studentID: state.studentID }, function (d) {
            if (!d || !d.length) { document.getElementById('prevYearModalBody').innerHTML = '<p class="text-muted text-center">No previous year records found.</p>'; return; }
            var html = '<table class="table table-sm table-bordered"><thead><tr><th>Receipt No</th><th>Session</th><th>Paid Date</th><th>Amount</th><th>Re-Print</th></tr></thead><tbody>';
            d.forEach(function (r) {
                html += '<tr><td><a href="#" class="rcpt-link3" data-id="' + r.MoneyReceiptID + '">' + r.MoneyReceipt_SN + '</a></td>' +
                    '<td>' + (r.EducationYear || '') + '</td><td>' + r.PaidDate + '</td><td>' + r.TotalAmount + ' Tk</td>' +
                    '<td><a href="#" class="print-link3" data-id="' + r.MoneyReceiptID + '"><i class="fa fa-print"></i> Print</a></td></tr>';
            });
            html += '</tbody></table>';
            document.getElementById('prevYearModalBody').innerHTML = html;
            document.querySelectorAll('.rcpt-link3').forEach(function (a) {
                a.addEventListener('click', function (e) { e.preventDefault(); $('#prevYearModal').modal('hide'); openReceiptDetail(this.dataset.id); });
            });
            document.querySelectorAll('.print-link3').forEach(function (a) {
                a.addEventListener('click', function (e) { e.preventDefault(); doPrint(this.dataset.id); });
            });
        });
    }

    // ── Dropdowns ─────────────────────────────────────────────────────────────
    function loadAccounts() {
        ajax('GetAccounts', {}, function (d) {
            var ddl = document.getElementById('ddlAccount'); ddl.innerHTML = '';
            var defaultID = null;
            (d || []).forEach(function (a) {
                var o = document.createElement('option');
                o.value = a.AccountID; o.text = a.AccountName;
                if (a.IsDefault) defaultID = a.AccountID;
                ddl.appendChild(o);
            });
            if (defaultID) ddl.value = defaultID;
        });
    }

    function loadRoles() {
        ajax('GetRoles', {}, function (d) {
            var ddl = document.getElementById('ddlRole'); ddl.innerHTML = '<option value="0">[ SELECT ]</option>';
            (d || []).forEach(function (r) { var o = document.createElement('option'); o.value = r.RoleID; o.text = r.Role; ddl.appendChild(o); });
        });
    }

    function loadSMSSetting() {
        ajax('GetSMSSetting', {}, function (v) { document.getElementById(v == 1 ? 'rbActive' : 'rbInactive').checked = true; });
    }

    document.querySelectorAll('input[name="smsStatus"]').forEach(function (r) {
        r.addEventListener('change', function () { ajax('SaveSMSSetting', { value: parseInt(this.value) }, function () {}); });
    });

    function loadConcessionPerm() {
        ajax('GetConcessionPermission', {}, function (ok) { document.getElementById('btnUpdateConcession').style.display = ok ? '' : 'none'; });
    }

    // ── Table Interactions ────────────────────────────────────────────────────
    document.addEventListener('input', function (evt) {
        var el = evt.target;
        if (!el.classList) return;
        if (el.type === 'checkbox' && el.classList.contains('due-checkbox')) {
            var row = el.closest('tr');
            row.classList.toggle('row-selected', el.checked);
            row.querySelector('.due-input').disabled = !el.checked;
            row.querySelector('.concession-input').disabled = !el.checked;
            var lf = row.querySelector('.latefee-input'); if (lf) lf.disabled = !el.checked;
        }
        if (el.classList.contains('concession-input')) {
            var row2 = el.closest('tr');
            var dv = parseFloat(row2.querySelector('.due-cell').textContent) || 0;
            var cv = parseFloat(el.value) || 0;
            if (cv > dv) { el.style.borderColor = 'red'; el.value = dv; } else el.style.borderColor = '';
        }
        recalc();
    });

    function recalc() {
        var t = 0;
        document.querySelectorAll('.due-input:not([disabled])').forEach(function (i) { t += parseFloat(i.value) || 0; });
        var txt = 'Total Amount: <span id="total-amount-pay">' + fmtAmt(t) + '</span> Tk';
        document.getElementById('total-pay-amount').innerHTML = txt;
        document.getElementById('grand-total-fixed').innerHTML = txt;
    }

    // ── PAY Button ────────────────────────────────────────────────────────────
    document.getElementById('btnPay').addEventListener('click', function () {
        var checked = document.querySelectorAll('.due-checkbox:checked');
        if (!checked.length) { document.getElementById('payError').textContent = 'Select payment to pay!'; return; }

        var paidDateVal = document.getElementById('txtPaidDate').value;
        if (!paidDateVal) { document.getElementById('payError').textContent = 'Paid Date প্রয়োজন!'; return; }

        document.getElementById('payError').textContent = '';
        var items = [];
        document.querySelectorAll('#dueTableBody tr').forEach(function (row) {
            var cb = row.querySelector('.due-checkbox'); if (!cb || !cb.checked) return;
            items.push({ PayOrderID: parseInt(row.dataset.payorderid), PaidAmount: parseFloat(row.querySelector('.due-input').value) || 0, IsOtherSession: false });
        });
        document.querySelectorAll('#otherDueTableBody tr').forEach(function (row) {
            var cb = row.querySelector('.due-checkbox'); if (!cb || !cb.checked) return;
            items.push({ PayOrderID: parseInt(row.dataset.payorderid), PaidAmount: parseFloat(row.querySelector('.due-input').value) || 0, IsOtherSession: true });
        });
        var btn = document.getElementById('btnPay'); btn.disabled = true; btn.textContent = 'Processing...';
        ajax('ProcessPayment', {
            studentDbID: state.studentDbID, studentClassID: state.studentClassID,
            educationYearID: state.educationYearID,
            smsPhoneNo: state.smsPhoneNo, studentID: state.studentID, studentName: state.studentName,
            accountID: parseInt(document.getElementById('ddlAccount').value),
            smsActive: document.getElementById('rbActive').checked,
            paidDate: paidDateVal,
            items: items
        }, function (r) {
            if (r && r.Success) { window.location.href = 'Money_Receipt_By_Date.aspx?mN_R=' + r.MRid + '&s_icD=' + r.Sid; }
            else { alert('Payment Error: ' + (r && r.Message ? r.Message : 'Unknown error')); btn.disabled = false; btn.textContent = 'PAY'; }
        }, function(errMsg) {
            alert('Server Error:\n' + errMsg);
            btn.disabled = false; btn.textContent = 'PAY';
        });
    });

    // ── Update Concession Button ──────────────────────────────────────────────
    document.getElementById('btnUpdateConcession').addEventListener('click', function () {
        var items = [];
        var allRows = Array.from(document.querySelectorAll('#dueTableBody tr, #otherDueTableBody tr'));
        for (var i = 0; i < allRows.length; i++) {
            var row = allRows[i]; var cb = row.querySelector('.due-checkbox');
            if (!cb || !cb.checked) continue;
            var fee = parseFloat(row.dataset.fee) || 0, paid = parseFloat(row.dataset.paid) || 0;
            var lfInp = row.querySelector('.latefee-input');
            var prevLf = parseFloat(row.querySelector('.prev-latefee').value) || 0;
            var newLf = lfInp ? (parseFloat(lfInp.value) || 0) : prevLf;
            var effLf = Math.max(newLf, prevLf);
            var conInp = row.querySelector('.concession-input');
            var conVal = conInp ? (parseFloat(conInp.value) || 0) : 0;
            var maxA = fee + effLf - paid;
            if (conVal > maxA) {
                alert('কনসেশন এমাউন্ট অবশিষ্ট এমাউন্টের চেয়ে বেশি হতে পারবে না!\nMax: ' + maxA + ' TK  You entered: ' + conVal + ' TK');
                if (conInp) conInp.focus(); return;
            }
            items.push({ PayOrderID: parseInt(row.dataset.payorderid), Discount: conVal, LateFee: newLf, PrevLateFee: prevLf });
        }
        if (!items.length) { alert('Row চেক করুন।'); return; }
        ajax('UpdateConcession', { items: items }, function (r) {
            if (r && r.Success) { alert('Update Successfully!!'); loadDues(); }
            else alert(r && r.Message ? r.Message : 'Update failed!');
        });
    });

    // ── Add More Payment ──────────────────────────────────────────────────────
    document.getElementById('btnAddMoreSave').addEventListener('click', function () {
        var roleID = parseInt(document.getElementById('ddlRole').value);
        var payFor = document.getElementById('txtPayFor').value.trim();
        var amount = document.getElementById('txtAmount').value.trim();
        var concession = document.getElementById('txtConcession').value.trim();
        var errSpan = document.getElementById('addMoreError');
        if (roleID === 0 || !payFor || !amount) { errSpan.textContent = 'Role, Pay For ও Amount প্রয়োজন'; return; }
        errSpan.textContent = '';
        ajax('AddMorePayment', {
            studentDbID: state.studentDbID, studentClassID: state.studentClassID, classID: state.classID,
            educationYearID: state.educationYearID,
            roleID: roleID, payFor: payFor, amount: parseFloat(amount) || 0, discount: parseFloat(concession) || 0
        }, function (r) {
            if (r && r.Success) {
                $('#addMoreModal').modal('hide');
                document.getElementById('txtPayFor').value = ''; document.getElementById('txtAmount').value = '';
                document.getElementById('txtConcession').value = ''; document.getElementById('ddlRole').selectedIndex = 0;
                loadDues();
            } else errSpan.textContent = r && r.Message ? r.Message : 'Failed!';
        });
    });

    // ── Scroll sticky total ───────────────────────────────────────────────────
    $(window).on('scroll', function () {
        var el = document.getElementById('total-amount-pay'); if (!el) return;
        var t = parseFloat(el.textContent) || 0;
        if (t === 0) { $('#grand-total-fixed').fadeOut(); return; }
        if ($(window).scrollTop() + $(window).height() > $(document).height() - 300) $('#grand-total-fixed').fadeOut();
        else $('#grand-total-fixed').fadeIn();
    });

    // ── Helpers ───────────────────────────────────────────────────────────────
    function ajax(method, data, cb, errCb) {
        $.ajax({
            url: 'Payment_Collection_By_Date.aspx/' + method, type: 'POST',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data), dataType: 'json',
            success: function (r) { if (cb) cb(r.d !== undefined ? r.d : r); },
            error: function (e) {
                showLoading(false);
                var msg = '';
                try { msg = JSON.parse(e.responseText).Message || e.responseText; } catch(x) { msg = e.responseText || e.statusText; }
                console.error(method, msg);
                if (errCb) errCb(msg);
                else if (method === 'GetStudentData') alert('Error: ' + msg);
            }
        });
    }

    function showLoading(v) {
        document.getElementById('pc-overlay').style.display = v ? 'block' : 'none';
    }

    function fmtAmt(n) { var v = parseFloat(parseFloat(n).toFixed(2)); return (v % 1 === 0) ? v.toFixed(0) : v.toFixed(2); }

    function parseDate(val) {
        if (!val) return new Date(0);
        var m = val.match(/\d+/); return m ? new Date(parseInt(m[0])) : new Date(val);
    }

    function fmtDate(val) {
        var d = parseDate(val); if (isNaN(d)) return val || '';
        return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
    }
})();
</script>
</asp:Content>
