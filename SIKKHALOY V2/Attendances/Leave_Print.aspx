<%@ Page Title="গেইট পাস" Language="C#" AutoEventWireup="true" CodeBehind="Leave_Print.aspx.cs" Inherits="EDUCATION.COM.ATTENDANCES.Leave_Print" ResponseEncoding="UTF-8" ContentType="text/html" %>

<!DOCTYPE html>
<html lang="bn">
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>গেইট পাস / ছুটির অনুমোদন পত্র</title>
    <style>
        /* ── Print options UI ── */
        .top-actions {
            width: 190mm; margin: 8px auto 0; display: flex; justify-content: center; gap: 8px;
        }
        .btn-top {
            display: inline-flex; align-items: center; gap: 5px;
            padding: 7px 20px; border-radius: 7px; font-size: 13px; font-weight: 600;
            border: none; cursor: pointer; transition: opacity .2s;
        }
        .btn-top-primary { background: linear-gradient(135deg,#1a6fc4,#0e4f96); color: #fff; }
        .btn-top-secondary { background: #6c757d; color: #fff; }
        .btn-top:hover { opacity: .88; }

        .print-panel {
            width: 190mm; margin: 6px auto 8px;
            background: #fff; border: 1px solid #c5d8f0; border-radius: 10px;
            box-shadow: 0 2px 8px rgba(26,111,196,.1); overflow: hidden;
        }
        .print-panel-header {
            background: #f0f6ff; border-bottom: 1px solid #c5d8f0;
            padding: 8px 14px; font-size: 13px; font-weight: 700; color: #1a6fc4;
            display: flex; align-items: center; gap: 6px;
        }
        .print-panel-body {
            padding: 10px 14px; display: flex; align-items: flex-end; flex-wrap: wrap; gap: 14px;
        }
        .pp-check { display: flex; align-items: center; gap: 6px; font-size: 12.5px; color: #444; margin-bottom: 2px; }
        .pp-check input[type=checkbox] { width: 15px; height: 15px; cursor: pointer; accent-color: #1a6fc4; }
        .pp-group { display: flex; flex-direction: column; gap: 3px; }
        .pp-group label { font-size: 11.5px; font-weight: 600; color: #1a6fc4; }
        .pp-group input, .pp-group select {
            border: 1px solid #c5d8f0; border-radius: 6px; padding: 5px 9px;
            font-size: 12.5px; color: #222; outline: none; background: #fff;
        }
        .pp-group input:focus, .pp-group select:focus { border-color: #1a6fc4; box-shadow: 0 0 0 2px rgba(26,111,196,.12); }
        .print-panel-footer {
            background: #f7faff; border-top: 1px solid #e0eaf6; padding: 7px 14px;
        }
        .btn-print-sm {
            background: linear-gradient(135deg,#1a6fc4,#0e4f96); color: #fff;
            border: none; border-radius: 6px; padding: 6px 18px;
            font-size: 12.5px; font-weight: 700; cursor: pointer; transition: opacity .2s;
        }
        .btn-print-sm:hover { opacity: .88; }
    </style>

    <!--dynamic css for printing-->
    <style type="text/css" media="print" id="print-content"></style>

    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'SolaimanLipi', 'Kalpurush', 'Vrinda', Arial, sans-serif;
            font-size: 12px;
            background: #e8ecf0;
            color: #222;
        }

        /* ── Page wrapper ── */
        .page-wrapper {
            width: 190mm;
            margin: 8mm auto;
            background: #fff;
            border-radius: 6px;
            overflow: hidden;
        }

        /* ── Student copy card ── */
        .gp-card {
            border: 1.5px solid #1a6fc4;
            border-radius: 5px;
            overflow: hidden;
            margin-bottom: 0;
        }

        /* School header */
        .gp-header {
            background: linear-gradient(135deg, #1a6fc4 0%, #0e4f96 100%);
            color: #fff;
            padding: 8px 12px 6px;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .gp-header .logo {
            width: 48px; height: 48px;
            object-fit: contain;
            border-radius: 50%;
            background: rgba(255,255,255,0.15);
            padding: 2px;
            flex-shrink: 0;
        }
        .gp-header .school-info { flex: 1; text-align: center; }
        .gp-header .school-name  { font-size: 15px; font-weight: 700; letter-spacing: .3px; }
        .gp-header .school-addr  { font-size: 10px; opacity: .92; margin-top: 1px; }
        .gp-header .school-phone { font-size: 10px; opacity: .85; }

        /* Title band */
        .gp-title-band {
            display: flex;
            align-items: stretch;
            border-bottom: 1.5px solid #1a6fc4;
        }
        .gp-title-band .band-left,
        .gp-title-band .band-right {
            background: #f0f6ff;
            color: #1a6fc4;
            font-size: 11px;
            font-weight: 700;
            padding: 4px 10px;
            display: flex;
            align-items: center;
            min-width: 90px;
        }
        .gp-title-band .band-right { justify-content: flex-end; }
        .gp-title-band .band-title {
            flex: 1;
            text-align: center;
            font-size: 15px;
            font-weight: 700;
            color: #0e4f96;
            padding: 5px 0;
            border-left: 1.5px solid #1a6fc4;
            border-right: 1.5px solid #1a6fc4;
            letter-spacing: .5px;
        }

        /* Info grid */
        .gp-info {
            display: flex;
            gap: 0;
            padding: 6px 10px 4px;
        }
        .gp-info-col { flex: 1; }
        .gp-info-col + .gp-info-col { border-left: 1px dashed #bcd; padding-left: 10px; margin-left: 8px; }
        .info-row {
            display: flex;
            align-items: baseline;
            margin-bottom: 3px;
            font-size: 11.5px;
        }
        .info-label {
            width: 52px;
            font-weight: 600;
            color: #444;
            flex-shrink: 0;
            font-size: 11px;
        }
        .info-sep  { margin: 0 3px; color: #888; flex-shrink: 0; }
        .info-value {
            flex: 1;
            border-bottom: 1px dotted #aac;
            padding-bottom: 1px;
            color: #111;
            word-break: break-word;
        }

        /* Date/time table */
        .gp-table {
            width: calc(100% - 20px);
            margin: 2px 10px 6px;
            border-collapse: collapse;
            font-size: 11px;
        }
        .gp-table th {
            background: #1a6fc4;
            color: #fff;
            padding: 4px 6px;
            font-weight: 600;
            text-align: center;
            border: 1px solid #1557a0;
            print-color-adjust: exact;
            -webkit-print-color-adjust: exact;
        }
        .gp-table td {
            border: 1px solid #c5d8f0;
            padding: 4px 6px;
            text-align: center;
            background: #f7faff;
        }
        .gp-table .row-lbl {
            text-align: left;
            font-weight: 600;
            background: #e8f0fb;
            color: #1a4f8a;
        }

        /* Remarks */
        .gp-remarks {
            margin: 0 10px 6px;
            border: 1px solid #c5d8f0;
            border-radius: 3px;
            padding: 4px 8px;
            min-height: 28px;
            font-size: 11.5px;
            background: #fafcff;
        }
        .gp-remarks .lbl { font-weight: 700; color: #1a6fc4; margin-right: 6px; }

        /* Footer */
        .gp-footer {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 5px 10px 6px;
            background: #f0f6ff;
            border-top: 1px solid #c5d8f0;
        }
        .gp-footer .guardian { font-size: 11px; color: #444; }
        .gp-footer .approval {
            font-size: 14px;
            font-weight: 700;
            color: #0e6640;
            background: #e6f5ee;
            border: 1px solid #a3d9bc;
            border-radius: 20px;
            padding: 3px 14px;
            letter-spacing: .3px;
        }
        .gp-footer .issue-date { font-size: 10px; color: #666; text-align: right; }

        /* ── Scissor divider ── */
        .scissor-divider {
            text-align: center;
            color: #999;
            font-size: 12px;
            letter-spacing: 4px;
            margin: 4px 0;
            user-select: none;
        }

        /* ── Office copy card ── */
        .gp-office {
            border: 1.5px solid #888;
            border-radius: 5px;
            overflow: hidden;
        }
        .gp-office-header {
            background: #555;
            color: #fff;
            text-align: center;
            font-size: 12px;
            font-weight: 700;
            padding: 4px;
            letter-spacing: .5px;
            print-color-adjust: exact;
            -webkit-print-color-adjust: exact;
        }
        .gp-office-body { padding: 5px 10px 6px; }
        .gp-office-table {
            width: 100%;
            border-collapse: collapse;
            font-size: 11px;
            margin-top: 4px;
        }
        .gp-office-table td {
            border: 1px solid #bbb;
            padding: 4px 8px;
            text-align: center;
        }
        .gp-sign-row {
            margin-top: 4px;
            border: 1px dashed #aaa;
            border-radius: 3px;
            padding: 4px 8px;
            font-size: 11px;
            font-weight: 600;
            color: #555;
            min-height: 28px;
        }

        @media print {
            body { background: #fff; }
            .no-print { display: none !important; }
            .top-actions, .print-panel { display: none !important; }
            .page-wrapper { margin: 0; border-radius: 0; }
            @page { margin: 8mm; size: A4; }

            /* Header: force dark text when background graphics are off */
            .gp-header { border-bottom: 1.5px solid #1a6fc4; }
            .gp-header .school-name  { color: #000 !important; opacity: 1 !important; }
            .gp-header .school-addr  { color: #333 !important; opacity: 1 !important; }
            .gp-header .school-phone { color: #333 !important; opacity: 1 !important; }

            /* Title band: explicit border around "গেইট পাস" */
            .gp-title-band { border: 1.5px solid #1a6fc4; border-top: none; }
            .gp-title-band .band-title {
                border-left: 1.5px solid #1a6fc4 !important;
                border-right: 1.5px solid #1a6fc4 !important;
                color: #000 !important;
            }
            .gp-title-band .band-left,
            .gp-title-band .band-right { color: #000 !important; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">

        <asp:HiddenField ID="_regIdHidden" runat="server" />

        <div class="no-print top-actions">
            <button type="button" class="btn-top btn-top-primary" onclick="window.print()">&#128438; প্রিন্ট করুন</button>
            <button type="button" class="btn-top btn-top-secondary" onclick="history.back()">&#8592; ফিরে যান</button>
        </div>

        <!-- Print Options Panel -->
        <div class="print-panel no-print d-print-none">
            <div class="print-panel-header">
                <span>&#9881;&#65039;</span> প্রিন্ট সেটিং
            </div>
            <div class="print-panel-body">
                <div class="pp-check">
                    <input type="checkbox" id="checkboxInstitution" />
                    <label for="checkboxInstitution">প্রতিষ্ঠানের নাম লুকান</label>
                </div>
                <div class="pp-group">
                    <label for="inputTopSpace">উপর থেকে স্পেস (px)</label>
                    <input id="inputTopSpace" min="0" type="number" style="width:130px;" />
                </div>
                <div class="pp-group">
                    <label for="inputFontSize">ফন্ট সাইজ (px)</label>
                    <input id="inputFontSize" min="8" max="20" type="number" style="width:130px;" />
                </div>
                <div class="pp-group">
                    <label for="inputPageSize">পেইজ সাইজ</label>
                    <select id="inputPageSize" style="width:180px;">
                        <option value="A4">A4</option>
                        <option value="A5">A5</option>
                        <option value="A6">A6 (A4 এর চার ভাগের এক)</option>
                        <option value="letter">Letter</option>
                    </select>
                </div>
            </div>
            <div class="print-panel-footer">
                <button type="button" class="btn-print-sm" onclick="window.print()">&#128438; প্রিন্ট করুন</button>
            </div>
        </div>

        <asp:Literal ID="PrintLiteral" runat="server"></asp:Literal>

    </form>

    <script>
        var printingOptions = {
            isInstitutionHidden: false,
            topSpace: 0,
            fontSize: 12,
            pageSize: 'A4'
        };

        var stores = {
            set: function () { localStorage.setItem('leave-print-options', JSON.stringify(printingOptions)); },
            get: function () {
                var d = localStorage.getItem('leave-print-options');
                if (d) printingOptions = JSON.parse(d);
            }
        };

        var printContent   = document.getElementById('print-content');
        var chkInstitution = document.getElementById('checkboxInstitution');
        var inputTopSpace  = document.getElementById('inputTopSpace');
        var inputFontSize  = document.getElementById('inputFontSize');
        var inputPageSize  = document.getElementById('inputPageSize');

        chkInstitution.addEventListener('change', function () {
            printingOptions.isInstitutionHidden = this.checked;
            stores.set(); applyOptions();
        });
        inputTopSpace.addEventListener('input', function () {
            printingOptions.topSpace = +this.value;
            stores.set(); applyOptions();
        });
        inputFontSize.addEventListener('input', function () {
            var s = +this.value;
            if (s >= 8 && s <= 20) { printingOptions.fontSize = s; stores.set(); applyOptions(); }
        });
        inputPageSize.addEventListener('change', function () {
            printingOptions.pageSize = this.value;
            stores.set(); applyOptions();
        });

        function applyOptions() {
            stores.get();
            chkInstitution.checked  = printingOptions.isInstitutionHidden;
            inputTopSpace.value     = printingOptions.topSpace;
            inputFontSize.value     = printingOptions.fontSize;
            inputPageSize.value     = printingOptions.pageSize || 'A4';

            var headers = document.querySelectorAll('.gp-header');
            headers.forEach(function (el) {
                el.style.display = printingOptions.isInstitutionHidden ? 'none' : '';
            });

            var fs   = printingOptions.fontSize;
            var pg   = printingOptions.pageSize || 'A4';
            var top  = printingOptions.topSpace;
            var hide = printingOptions.isInstitutionHidden;

            // Determine page width for centering
            var pgWidths = { A4: '210mm', A5: '148mm', A6: '105mm', letter: '216mm' };
            var pgW = pgWidths[pg] || '210mm';

            printContent.textContent =
                '.gp-header { display: ' + (hide ? 'none' : 'flex') + ' !important; }' +
                '.page-wrapper { padding-top: ' + top + 'px; width: ' + pgW + ' !important; margin-left: auto !important; margin-right: auto !important; font-size: ' + fs + 'px !important; }' +
                '.page-wrapper *, .page-wrapper p, .page-wrapper span, .page-wrapper div, .page-wrapper td, .page-wrapper th, .page-wrapper label, .page-wrapper a, .page-wrapper li, .page-wrapper strong, .page-wrapper b, .page-wrapper em, .page-wrapper small { font-size: ' + fs + 'px !important; }' +
                '.gp-header .school-name { font-size: ' + (fs + 3) + 'px !important; }' +
                '.gp-header .school-addr, .gp-header .school-phone { font-size: ' + (fs - 1) + 'px !important; }' +
                '.gp-title-band .band-title { font-size: ' + (fs + 3) + 'px !important; }' +
                '.gp-title-band .band-left, .gp-title-band .band-right { font-size: ' + fs + 'px !important; }' +
                '.gp-table th, .gp-table td { font-size: ' + fs + 'px !important; }' +
                '.gp-office-table td { font-size: ' + fs + 'px !important; }' +
                '.info-label, .info-value, .info-sep { font-size: ' + fs + 'px !important; }' +
                '.gp-remarks, .gp-remarks .lbl { font-size: ' + fs + 'px !important; }' +
                '.gp-footer .guardian, .gp-footer .issue-date { font-size: ' + fs + 'px !important; }' +
                '.gp-footer .approval { font-size: ' + (fs + 2) + 'px !important; }' +
                '.gp-office-header { font-size: ' + (fs + 1) + 'px !important; }' +
                '.gp-sign-row { font-size: ' + fs + 'px !important; }' +
                '@page { size: ' + pg + ' portrait; margin: 6mm 10mm; }';
        }

        applyOptions();

        // Apply saved header color from BASIC.Master
        (function () {
            var regId = document.getElementById('<%= _regIdHidden.ClientID %>').value;
            var key = 'headerColor_' + (regId || 'default');
            var savedColor = localStorage.getItem(key);
            if (savedColor) {
                var headers = document.querySelectorAll('.gp-header');
                headers.forEach(function (el) {
                    el.style.background = savedColor;
                });
            }
        })();
    </script>
</body>
</html>
