<%@ Page Title="Reset Institution Data" Language="C#" MasterPageFile="~/Basic_Authority.Master" AutoEventWireup="true" CodeBehind="Reset_Institution_Data.aspx.cs" Inherits="EDUCATION.COM.Authority.Institutions.Reset_Institution_Data" %>



<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <style>

        .reset-card {

            background: #fff;

            border-radius: 8px;

            box-shadow: 0 1px 4px rgba(0,0,0,.1);

            padding: 20px 22px;

            margin-bottom: 20px;

        }

        .reset-card h5 {

            font-weight: 600;

            border-bottom: 2px solid #eee;

            padding-bottom: 8px;

            margin-bottom: 14px;

        }

        .warn-box {

            background: #fff3cd;

            border: 1px solid #ffc107;

            border-radius: 6px;

            padding: 12px 14px;

            margin-bottom: 14px;

        }

        .danger-box {

            background: #f8d7da;

            border: 1px solid #f5c6cb;

            border-radius: 6px;

            padding: 12px 14px;

            margin-bottom: 14px;

        }

        .purge-box {

            background: #2b0000;

            color: #ffcccc;

            border: 1px solid #8b0000;

            border-radius: 6px;

            padding: 12px 14px;

            margin-bottom: 14px;

        }

        .keep-list { margin: 0; padding-left: 18px; }

        .keep-list li { margin-bottom: 3px; }

        #resetProgressModal .modal-body { max-height: 70vh; overflow-y: auto; }

        #resetTableWrap { max-height: 260px; overflow-y: auto; border: 1px solid #eee; }

        #resetTableWrap table { margin-bottom: 0; font-size: 13px; }

        .reset-stat {

            display: inline-block;

            min-width: 110px;

            background: #f8f9fa;

            border: 1px solid #e9ecef;

            border-radius: 6px;

            padding: 8px 12px;

            margin: 0 8px 8px 0;

        }

        .reset-stat .lbl { display: block; font-size: 11px; color: #666; text-transform: uppercase; }

        .reset-stat .val { font-size: 18px; font-weight: 700; }

        .reset-timer {

            font-size: 28px;

            font-weight: 700;

            font-variant-numeric: tabular-nums;

            letter-spacing: .5px;

        }

        .progress-indeterminate {

            height: 10px;

            border-radius: 6px;

            overflow: hidden;

            background: #e9ecef;

        }

        .progress-indeterminate > div {

            height: 100%;

            width: 40%;

            background: linear-gradient(90deg, #007bff, #17a2b8);

            animation: resetSlide 1.2s ease-in-out infinite;

        }

        .progress-determinate {

            height: 12px;

            border-radius: 6px;

            overflow: hidden;

            background: #e9ecef;

        }

        .progress-determinate > div {

            height: 100%;

            width: 0%;

            background: linear-gradient(90deg, #007bff, #17a2b8);

            transition: width .35s ease;

        }

        .reset-row-count {

            font-size: 22px;

            font-weight: 700;

            font-variant-numeric: tabular-nums;

            margin-top: 6px;

        }

        @keyframes resetSlide {

            0% { margin-left: -40%; }

            100% { margin-left: 100%; }

        }

    </style>

</asp:Content>



<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">

    <h3><i class="fa fa-trash"></i> Reset Institution Data</h3>



    <div class="reset-card">

        <h5><i class="fa fa-university"></i> Select Institution</h5>

        <div class="form-inline">

            <div class="form-group mr-2">

                <asp:DropDownList ID="SchoolDropDown" runat="server" CssClass="form-control" Width="420px"

                    AutoPostBack="True" OnSelectedIndexChanged="SchoolDropDown_SelectedIndexChanged"

                    AppendDataBoundItems="True">

                    <asp:ListItem Value="0">[ SELECT INSTITUTION ]</asp:ListItem>

                </asp:DropDownList>

            </div>

            <asp:Label ID="SchoolInfoLabel" runat="server" CssClass="text-muted ml-2"></asp:Label>

        </div>

    </div>



    <asp:Panel ID="ActionPanel" runat="server" Visible="false">

        <div class="row">

            <div class="col-md-6">

                <div class="reset-card">

                    <h5 class="text-danger"><i class="fa fa-refresh"></i> Full Reset (New Signup State)</h5>

                    <div class="danger-box">

                        <strong>Warning:</strong> All operational data of this institution will be permanently deleted

                        (students, teachers, exams, fees, attendance, classes, SMS history, etc.).

                        <br /><strong>Tip:</strong> Uses short batched deletes (does not lock whole server).

                    </div>

                    <p><strong>Will KEEP:</strong></p>

                    <ul class="keep-list">

                        <li>School profile (<code>SchoolInfo</code>)</li>

                        <li>Admin login (Membership / Registration / AST)</li>

                        <li>One Education Year + Admin year link</li>

                        <li>SMS wallet (balance reset to 0)</li>

                        <li>Platform invoices &amp; referrer assignment</li>

                    </ul>

                    <div class="form-group mt-3">

                        <label>Type School ID to confirm</label>

                        <asp:TextBox ID="FullConfirmTextBox" runat="server" CssClass="form-control" placeholder="Enter School ID"></asp:TextBox>

                    </div>

                    <button type="button" class="btn btn-danger" onclick="startResetFlow('FULL');">

                        Reset Institution to New Signup State

                    </button>

                </div>

            </div>



            <div class="col-md-6">

                <div class="reset-card">

                    <h5 class="text-warning"><i class="fa fa-calendar"></i> Delete Specific Session Data</h5>

                    <div class="warn-box">

                        Deletes only selected session data (students-class links, exams, fees, attendance for that year, etc.).

                        School profile, admin login, classes/subjects master and other sessions stay.

                        <br /><strong>Tip:</strong> Prefer off-peak time. Uses short batched deletes (does not lock whole server).

                    </div>

                    <div class="form-group">

                        <label>Session / Education Year</label>

                        <asp:DropDownList ID="SessionDropDown" runat="server" CssClass="form-control"

                            AppendDataBoundItems="True">

                            <asp:ListItem Value="0">[ SELECT SESSION ]</asp:ListItem>

                        </asp:DropDownList>

                    </div>

                    <div class="form-group">

                        <label>Type School ID to confirm</label>

                        <asp:TextBox ID="SessionConfirmTextBox" runat="server" CssClass="form-control" placeholder="Enter School ID"></asp:TextBox>

                    </div>

                    <button type="button" class="btn btn-warning" onclick="startResetFlow('SESSION');">

                        Delete Selected Session Data

                    </button>

                </div>

            </div>

        </div>



        <div class="reset-card">

            <h5 style="color:#8b0000;"><i class="fa fa-bomb"></i> Permanently Delete Institution</h5>

            <div class="purge-box">

                <strong>Danger — irreversible:</strong>

                Completely removes this institution from the database:

                all data + admin user id/password (Membership) + Registration/AST + Education Year + SMS + invoices + <code>SchoolInfo</code> row.

                After this, the institution will not exist at all.

                <br /><strong>Tip:</strong> Uses short batched deletes (does not lock whole server).

            </div>

            <div class="row">

                <div class="col-md-4">

                    <div class="form-group">

                        <label>Type School ID to confirm</label>

                        <asp:TextBox ID="PurgeConfirmIdTextBox" runat="server" CssClass="form-control" placeholder="Enter School ID"></asp:TextBox>

                    </div>

                </div>

                <div class="col-md-4">

                    <div class="form-group">

                        <label>Type <strong>DELETE</strong> to confirm</label>

                        <asp:TextBox ID="PurgeConfirmWordTextBox" runat="server" CssClass="form-control" placeholder="DELETE"></asp:TextBox>

                    </div>

                </div>

                <div class="col-md-4">

                    <div class="form-group">

                        <label>&nbsp;</label><br />

                        <button type="button" class="btn btn-dark btn-block" onclick="startResetFlow('PURGE');">

                            Permanently Delete Institution

                        </button>

                    </div>

                </div>

            </div>

        </div>

    </asp:Panel>



    <asp:Label ID="MsgLabel" runat="server" CssClass="font-weight-bold d-block mt-2"></asp:Label>



    <div class="modal fade" id="resetProgressModal" tabindex="-1" role="dialog" aria-hidden="true" data-backdrop="static" data-keyboard="false">

        <div class="modal-dialog modal-lg" role="document">

            <div class="modal-content">

                <div class="modal-header">

                    <h5 class="modal-title" id="resetModalTitle">Data Preview</h5>

                    <button type="button" class="close" id="resetModalCloseX" data-dismiss="modal" aria-label="Close">

                        <span aria-hidden="true">&times;</span>

                    </button>

                </div>

                <div class="modal-body">

                    <div id="resetPreviewPhase">

                        <p id="resetPreviewSummary" class="mb-2"></p>

                        <div id="resetPreviewStats"></div>

                        <div id="resetActiveWarn" class="alert alert-warning d-none py-2"></div>

                        <div id="resetTableWrap" class="mb-2">

                            <table class="table table-sm table-striped">

                                <thead class="thead-light">

                                    <tr><th>Table</th><th class="text-right">Rows</th></tr>

                                </thead>

                                <tbody id="resetTableBody"></tbody>

                            </table>

                        </div>

                        <p class="text-muted small mb-0">Review the data volume above, then confirm to start delete.</p>

                    </div>



                    <div id="resetRunningPhase" class="d-none text-center py-3">

                        <p class="mb-1">Deleting data… please wait. Do not close this window.</p>

                        <div class="reset-timer mb-1" id="resetLiveTimer">0.0s</div>

                        <div class="reset-row-count mb-2" id="resetLiveRows">0 / 0 rows</div>

                        <div class="progress-determinate mb-2" id="resetProgressBarWrap"><div id="resetProgressBar"></div></div>

                        <p class="text-muted small mb-0" id="resetRunningHint">Working on selected school…</p>

                    </div>



                    <div id="resetResultPhase" class="d-none">

                        <div id="resetResultAlert" class="alert"></div>

                        <div id="resetResultStats"></div>

                    </div>

                </div>

                <div class="modal-footer">

                    <button type="button" class="btn btn-secondary" id="resetCancelBtn" data-dismiss="modal">Cancel</button>

                    <button type="button" class="btn btn-danger" id="resetConfirmBtn" onclick="confirmAndExecute();">Confirm Delete</button>

                    <button type="button" class="btn btn-primary d-none" id="resetDoneBtn" data-dismiss="modal">Done</button>

                </div>

            </div>

        </div>

    </div>



    <script type="text/javascript">

        var resetApiUrl = '<%= ResolveUrl("~/Authority/Institutions/Reset_Institution_Data_API.ashx") %>';

        var resetSchoolClientId = '<%= SchoolDropDown.ClientID %>';

        var resetSessionClientId = '<%= SessionDropDown.ClientID %>';

        var resetFullConfirmId = '<%= FullConfirmTextBox.ClientID %>';

        var resetSessionConfirmId = '<%= SessionConfirmTextBox.ClientID %>';

        var resetPurgeConfirmId = '<%= PurgeConfirmIdTextBox.ClientID %>';

        var resetPurgeWordId = '<%= PurgeConfirmWordTextBox.ClientID %>';

        var resetMsgClientId = '<%= MsgLabel.ClientID %>';



        var pendingReset = null;

        var timerHandle = null;

        var timerStarted = 0;

        var progressHandle = null;



        function schoolIdSelected() {

            return parseInt(document.getElementById(resetSchoolClientId).value, 10) || 0;

        }



        function showPageMsg(text, ok) {

            var el = document.getElementById(resetMsgClientId);

            if (!el) return;

            el.className = ok

                ? 'font-weight-bold d-block mt-2 text-success'

                : 'font-weight-bold d-block mt-2 text-danger';

            el.innerText = text || '';

        }



        function formatNumber(n) {

            try { return Number(n).toLocaleString(); } catch (e) { return String(n); }

        }



        function formatElapsedMs(ms) {

            var s = Math.floor(ms / 1000);

            var m = Math.floor(s / 60);

            var h = Math.floor(m / 60);

            if (h > 0) return h + 'h ' + (m % 60) + 'm ' + (s % 60) + 's';

            if (m > 0) return m + 'm ' + (s % 60) + 's';

            var frac = Math.floor((ms % 1000) / 100);

            return s + '.' + frac + 's';

        }



        function startTimer() {

            stopTimer();

            timerStarted = Date.now();

            var el = document.getElementById('resetLiveTimer');

            el.innerText = '0.0s';

            timerHandle = setInterval(function () {

                el.innerText = formatElapsedMs(Date.now() - timerStarted);

            }, 100);

        }



        function stopTimer() {

            if (timerHandle) {

                clearInterval(timerHandle);

                timerHandle = null;

            }

        }



        function updateLiveRows(deleted, total) {

            var el = document.getElementById('resetLiveRows');

            var bar = document.getElementById('resetProgressBar');

            if (!el) return;

            var d = Number(deleted) || 0;

            var t = Number(total) || 0;

            if (t > 0) {

                el.innerText = formatNumber(d) + ' / ' + formatNumber(t) + ' rows';

                var pct = Math.min(100, Math.round(100 * d / t));

                if (pct >= 100 && d < t) pct = 99;

                if (bar) bar.style.width = pct + '%';

            } else {

                el.innerText = formatNumber(d) + ' rows deleted';

                if (bar) bar.style.width = Math.min(90, Math.round(d > 0 ? 15 + Math.log10(d + 1) * 20 : 5)) + '%';

            }

        }



        function startProgressPoll() {

            stopProgressPoll();

            if (!pendingReset) return;

            updateLiveRows(0, pendingReset.totalRows || 0);

            var tick = function () {

                if (!pendingReset) return;

                var url = resetApiUrl + '?action=progress&schoolId=' + pendingReset.schoolId;

                fetch(url, { credentials: 'same-origin', cache: 'no-store' })

                    .then(function (r) { return r.json(); })

                    .then(function (data) {

                        if (!data || !data.ok) return;

                        var total = data.totalRows || pendingReset.totalRows || 0;

                        updateLiveRows(data.deletedRows || 0, total);

                        if (data.status) {

                            document.getElementById('resetRunningHint').innerText =

                                'SchoolID ' + pendingReset.schoolId + ' · Mode ' + pendingReset.mode

                                + ' · Status: ' + data.status;

                        }

                    })

                    .catch(function () { /* ignore poll errors while delete runs */ });

            };

            tick();

            progressHandle = setInterval(tick, 1500);

        }



        function stopProgressPoll() {

            if (progressHandle) {

                clearInterval(progressHandle);

                progressHandle = null;

            }

        }



        function setModalPhase(phase) {

            document.getElementById('resetPreviewPhase').classList.toggle('d-none', phase !== 'preview');

            document.getElementById('resetRunningPhase').classList.toggle('d-none', phase !== 'running');

            document.getElementById('resetResultPhase').classList.toggle('d-none', phase !== 'result');



            document.getElementById('resetCancelBtn').classList.toggle('d-none', phase !== 'preview');

            document.getElementById('resetConfirmBtn').classList.toggle('d-none', phase !== 'preview');

            document.getElementById('resetDoneBtn').classList.toggle('d-none', phase !== 'result');

            document.getElementById('resetModalCloseX').classList.toggle('d-none', phase === 'running');

        }



        function startResetFlow(mode) {

            var schoolId = schoolIdSelected();

            if (!schoolId) {

                alert('Please select an institution.');

                return;

            }



            var confirmText = '';

            var confirmWord = '';

            var educationYearId = 0;

            var title = '';



            if (mode === 'FULL') {

                confirmText = (document.getElementById(resetFullConfirmId).value || '').trim();

                title = 'Full Reset — Preview';

            } else if (mode === 'SESSION') {

                educationYearId = parseInt(document.getElementById(resetSessionClientId).value, 10) || 0;

                if (!educationYearId) {

                    alert('Please select a session.');

                    return;

                }

                confirmText = (document.getElementById(resetSessionConfirmId).value || '').trim();

                title = 'Session Delete — Preview';

            } else if (mode === 'PURGE') {

                confirmText = (document.getElementById(resetPurgeConfirmId).value || '').trim();

                confirmWord = (document.getElementById(resetPurgeWordId).value || '').trim();

                if (confirmWord !== 'DELETE') {

                    alert('Type DELETE (capital letters) to confirm permanent delete.');

                    return;

                }

                title = 'Permanent Delete — Preview';

            }



            if (parseInt(confirmText, 10) !== schoolId) {

                alert('Type the exact School ID (' + schoolId + ') to proceed.');

                return;

            }



            pendingReset = {

                mode: mode,

                schoolId: schoolId,

                confirmSchoolId: schoolId,

                educationYearId: educationYearId,

                confirmWord: confirmWord,

                totalRows: 0

            };



            document.getElementById('resetModalTitle').innerText = title;

            document.getElementById('resetPreviewSummary').innerText = 'Loading data counts…';

            document.getElementById('resetPreviewStats').innerHTML = '';

            document.getElementById('resetTableBody').innerHTML = '';

            document.getElementById('resetActiveWarn').className = 'alert alert-warning d-none py-2';

            setModalPhase('preview');

            document.getElementById('resetConfirmBtn').disabled = true;

            $('#resetProgressModal').modal('show');



            var qs = 'action=preview&mode=' + encodeURIComponent(mode)

                + '&schoolId=' + schoolId

                + '&educationYearId=' + educationYearId;



            fetch(resetApiUrl + '?' + qs, { credentials: 'same-origin' })

                .then(function (r) { return r.json(); })

                .then(function (data) {

                    if (!data || !data.ok) {

                        document.getElementById('resetPreviewSummary').innerText = (data && data.message) || 'Preview failed.';

                        return;

                    }

                    var s = data.summary || {};

                    pendingReset.totalRows = Number(s.totalRows) || 0;

                    document.getElementById('resetPreviewSummary').innerHTML =

                        '<strong>' + (s.schoolName || '') + '</strong> (SchoolID: ' + s.schoolId + ')'

                        + ' — Mode: <strong>' + s.mode + '</strong>'

                        + (s.educationYearId ? ' — Session: <strong>' + s.educationYearId + '</strong>' : '');



                    document.getElementById('resetPreviewStats').innerHTML =

                        '<span class="reset-stat"><span class="lbl">Total rows</span><span class="val text-danger">' + formatNumber(s.totalRows || 0) + '</span></span>'

                        + '<span class="reset-stat"><span class="lbl">Tables with data</span><span class="val">' + formatNumber((data.tables || []).length) + '</span></span>'

                        + '<span class="reset-stat"><span class="lbl">Active users (30m)</span><span class="val">' + formatNumber(s.activeUsers || 0) + '</span></span>';



                    var warn = document.getElementById('resetActiveWarn');

                    if ((s.activeUsers || 0) > 0) {

                        warn.className = 'alert alert-warning py-2';

                        warn.innerText = s.activeUsers + ' user(s) were active in the last 30 minutes. Prefer off-peak time.';

                    }



                    var body = document.getElementById('resetTableBody');

                    body.innerHTML = '';

                    (data.tables || []).forEach(function (t) {

                        var tr = document.createElement('tr');

                        tr.innerHTML = '<td>' + t.tableName + '</td><td class="text-right">' + formatNumber(t.rowCnt) + '</td>';

                        body.appendChild(tr);

                    });

                    if (!(data.tables || []).length) {

                        body.innerHTML = '<tr><td colspan="2" class="text-muted">No counted rows found for key tables.</td></tr>';

                    }



                    document.getElementById('resetConfirmBtn').disabled = false;

                })

                .catch(function (err) {

                    document.getElementById('resetPreviewSummary').innerText = 'Preview error: ' + err;

                });

        }



        function confirmAndExecute() {

            if (!pendingReset) return;



            var modeLabel = pendingReset.mode === 'FULL' ? 'FULL RESET'

                : (pendingReset.mode === 'SESSION' ? 'SESSION DELETE' : 'PERMANENT DELETE');

            if (!confirm(modeLabel + ': This cannot be undone. Continue?')) return;



            setModalPhase('running');

            document.getElementById('resetModalTitle').innerText = modeLabel + ' — In Progress';

            document.getElementById('resetRunningHint').innerText =

                'SchoolID ' + pendingReset.schoolId + ' · Mode ' + pendingReset.mode;

            startTimer();

            startProgressPoll();



            var body =

                'action=execute'

                + '&mode=' + encodeURIComponent(pendingReset.mode)

                + '&schoolId=' + pendingReset.schoolId

                + '&confirmSchoolId=' + pendingReset.confirmSchoolId

                + '&educationYearId=' + (pendingReset.educationYearId || 0)

                + '&totalRows=' + (pendingReset.totalRows || 0)

                + '&confirmWord=' + encodeURIComponent(pendingReset.confirmWord || '');



            fetch(resetApiUrl, {

                method: 'POST',

                credentials: 'same-origin',

                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },

                body: body

            })

                .then(function (r) { return r.json(); })

                .then(function (data) {

                    stopTimer();

                    stopProgressPoll();

                    setModalPhase('result');

                    var alertEl = document.getElementById('resetResultAlert');

                    var statsEl = document.getElementById('resetResultStats');

                    if (data && data.ok) {

                        document.getElementById('resetModalTitle').innerText = 'Completed';

                        alertEl.className = 'alert alert-success';

                        alertEl.innerText = data.message || 'Completed successfully.';

                        var deleted = data.deletedRows || 0;

                        var target = (pendingReset && pendingReset.totalRows) ? pendingReset.totalRows : 0;

                        updateLiveRows(deleted, target || deleted);

                        statsEl.innerHTML =

                            '<span class="reset-stat"><span class="lbl">Deleted rows</span><span class="val text-success">' + formatNumber(deleted) + (target ? ' / ' + formatNumber(target) : '') + '</span></span>'

                            + '<span class="reset-stat"><span class="lbl">Elapsed time</span><span class="val">' + (data.elapsedText || formatElapsedMs(data.elapsedMs || 0)) + '</span></span>';

                        showPageMsg((data.message || 'Done.') + ' Deleted rows: ' + formatNumber(data.deletedRows || 0) + '. Time: ' + (data.elapsedText || '') + '.', true);

                        if (pendingReset.mode === 'PURGE') {

                            setTimeout(function () { window.location.reload(); }, 1200);

                        } else {

                            document.getElementById(resetFullConfirmId).value = '';

                            document.getElementById(resetSessionConfirmId).value = '';

                            document.getElementById(resetPurgeConfirmId).value = '';

                            document.getElementById(resetPurgeWordId).value = '';

                        }

                    } else {

                        document.getElementById('resetModalTitle').innerText = 'Failed';

                        alertEl.className = 'alert alert-danger';

                        alertEl.innerText = (data && data.message) || 'Delete failed.';

                        statsEl.innerHTML =

                            '<span class="reset-stat"><span class="lbl">Deleted so far</span><span class="val">' + formatNumber((data && data.deletedRows) || 0) + '</span></span>'

                            + '<span class="reset-stat"><span class="lbl">Elapsed time</span><span class="val">' + ((data && data.elapsedText) || formatElapsedMs(Date.now() - timerStarted)) + '</span></span>';

                        showPageMsg('Failed: ' + ((data && data.message) || 'Unknown error'), false);

                    }

                    pendingReset = null;

                })

                .catch(function (err) {

                    stopTimer();

                    stopProgressPoll();

                    setModalPhase('result');

                    document.getElementById('resetModalTitle').innerText = 'Failed';

                    document.getElementById('resetResultAlert').className = 'alert alert-danger';

                    document.getElementById('resetResultAlert').innerText = 'Error: ' + err;

                    document.getElementById('resetResultStats').innerHTML = '';

                    showPageMsg('Error: ' + err, false);

                    pendingReset = null;

                });

        }

    </script>

</asp:Content>

