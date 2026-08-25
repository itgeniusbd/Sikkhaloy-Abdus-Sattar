window.sikkhaloyCompressImage = function (dataUrl, maxWidth, quality, maxBytes) {
    return new Promise(function (resolve, reject) {
        var img = new Image();
        img.onload = function () {
            var w = img.width;
            var h = img.height;
            if (w > maxWidth) {
                h = Math.max(1, Math.round(h * maxWidth / w));
                w = maxWidth;
            }
            var canvas = document.createElement("canvas");
            var q = quality;
            var result = dataUrl;
            for (var i = 0; i < 8; i++) {
                canvas.width = w;
                canvas.height = h;
                var ctx = canvas.getContext("2d");
                ctx.fillStyle = "#fff";
                ctx.fillRect(0, 0, w, h);
                ctx.drawImage(img, 0, 0, w, h);
                result = canvas.toDataURL("image/jpeg", q);
                var bytes = Math.ceil((result.length - 23) * 0.75);
                if (bytes <= maxBytes || (w <= 140 && q <= 0.42))
                    break;
                if (bytes > maxBytes * 1.5) {
                    w = Math.max(140, Math.round(w * 0.82));
                    h = Math.max(140, Math.round(h * 0.82));
                } else {
                    q = Math.max(0.42, q - 0.08);
                }
            }
            resolve(result);
        };
        img.onerror = function () { reject(new Error("image")); };
        img.src = dataUrl;
    });
};

window.sikkhaloyReceiptPrintSave = function (opts) {
    try { localStorage.setItem("sikkhaloy-receipt-print", JSON.stringify(opts || {})); } catch (e) { }
};

window.sikkhaloyReceiptPrintLoad = function () {
    try {
        return JSON.parse(localStorage.getItem("sikkhaloy-receipt-print") || "{}");
    } catch (e) {
        return {};
    }
};

window.sikkhaloyPrintReceipt = function (opts) {
    opts = opts || {};
    try { localStorage.setItem("sikkhaloy-receipt-print", JSON.stringify(opts)); } catch (e) { }

    var inches = Number(opts.size);
    if (opts.size === "half") inches = 4;
    else if (opts.size === "quarter") inches = 3;
    else if (opts.size === "full") inches = 6;
    if (!isFinite(inches)) inches = 4;
    if (inches < 3) inches = 3;
    if (inches > 6) inches = 6;

    var topPx = Math.max(0, Number(opts.topSpace) || 0);
    var fontPx = Math.min(20, Math.max(10, Number(opts.fontSize) || 11));
    var narrow = inches <= 3.5;
    var infoPx = narrow ? (inches <= 3 ? 8 : 9) : (fontPx + 1);
    var tablePx = narrow ? (inches <= 3 ? 8 : 9) : fontPx;
    var titlePx = narrow ? (inches <= 3 ? 11 : 12) : 15;
    var schoolPx = narrow ? (inches <= 3 ? 11 : 12) : (fontPx + 4);
    var sideMm = Math.max(4, (210 - inches * 25.4) / 2);
    var width = inches + "in";

    var id = "sikkhaloy-print-page";
    var el = document.getElementById(id);
    if (!el) {
        el = document.createElement("style");
        el.id = id;
        document.head.appendChild(el);
    }

    document.body.classList.remove("print-landscape", "print-compact", "print-leave");
    document.body.classList.add("print-portrait", "print-receipt");

    var finish = function () {
        document.body.classList.remove("print-receipt");
        window.removeEventListener("afterprint", finish);
    };
    window.addEventListener("afterprint", finish);

    el.textContent =
        "@page { size: A4 portrait; margin: " + topPx + "px " + sideMm + "mm 8mm " + sideMm + "mm !important; }" +
        "@media print {" +
        "html, body, #app, .app-shell, .app-main, .app-content, .card, .mr-page {" +
        " height: auto !important; min-height: 0 !important; width: 100% !important; max-width: none !important;" +
        " display: block !important; padding: 0 !important; margin: 0 !important;" +
        " overflow: visible !important; box-shadow: none !important; border: 0 !important;" +
        " transform: none !important; zoom: 1 !important; }" +
        ".mr-sheet { width: " + width + " !important; max-width: " + width + " !important;" +
        " margin: 0 auto !important; padding: 0 !important; border: 0 !important; height: auto !important; }" +
        ".print-receipt .print-header, .print-receipt .print-date, .print-receipt .print-only," +
        ".print-receipt .app-header, .print-receipt .app-sidebar { display: none !important; }" +
        ".mr-name-logo { max-width: 100% !important; max-height: " + (narrow ? "42px" : "72px") + " !important; display: block !important; margin: 0 auto 4px !important; }" +
        ".mr-school { font-size: " + schoolPx + "px !important; font-weight: 700 !important; color: #000 !important; }" +
        ".mr-addr, .mr-addr span { font-size: " + infoPx + "px !important; color: #000 !important; }" +
        ".mr-head { font-size: " + titlePx + "px !important; letter-spacing: " + (narrow ? "0" : "1px") + " !important; }" +
        ".mr-info { margin: " + (narrow ? "6px 0" : "15px 0") + " !important; overflow: visible !important; }" +
        ".mr-info > div { padding: " + (narrow ? "1px 3px" : "2px 6px") + " !important; overflow: visible !important; }" +
        ".mr-info p { font-size: " + infoPx + "px !important; white-space: nowrap !important;" +
        " overflow: visible !important; margin: " + (narrow ? "2px 0" : "5px 0") + " !important; }" +
        ".mr-page .mgrid th, .mr-page .mgrid td { font-size: " + tablePx + "px !important; padding: " + (narrow ? "2px 1px" : "3px") + " !important; white-space: nowrap !important; }" +
        ".mr-words, .mr-recv, .mr-due-title, .oi-total, .mr-blessing { font-size: " + infoPx + "px !important; white-space: nowrap !important; }" +
        "}";
    window.print();
};

window.sikkhaloyPrint = function (orientation, compact, marginOverride) {
    var id = "sikkhaloy-print-page";
    var el = document.getElementById(id);
    if (!el) {
        el = document.createElement("style");
        el.id = id;
        document.head.appendChild(el);
    }

    var landscape = orientation === "landscape";
    var margin = marginOverride || (compact ? "6mm 8mm" : (landscape ? "8mm" : "10mm"));
    el.textContent =
        "@page { size: A4 " + (landscape ? "landscape" : "portrait") + "; margin: " + margin + "; }" +
        "@media print {" +
        "html, body, #app, .ad-shell, .ad-main, .ad-content, .app-shell, .app-main, .app-content, .card, .ai-print {" +
        " background: #fff !important; height: auto !important; overflow: visible !important;" +
        " box-shadow: none !important; padding: 0 !important; margin: 0 !important; border: 0 !important; }" +
        ".ad-sidebar, .ad-header, .app-sidebar, .app-header, .go-top, .nav-back, .no-print { display: none !important; }" +
        ".ad-shell, .app-shell { display: block !important; grid-template-columns: none !important; }" +
        "}";
    document.body.classList.remove("print-portrait", "print-landscape", "print-compact");
    document.body.classList.add(landscape ? "print-landscape" : "print-portrait");
    if (compact)
        document.body.classList.add("print-compact");

    var restore = function () {
        document.body.classList.remove("print-portrait", "print-landscape", "print-compact");
        window.removeEventListener("afterprint", restore);
    };
    window.addEventListener("afterprint", restore);
    window.print();
};

window.sikkhaloyPrintLeave = function (pageSize, topSpace) {
    window.sikkhaloyApplyLeavePrint(pageSize, topSpace);
    window.print();
};

window.sikkhaloyApplyLeavePrint = function (pageSize, topSpace) {
    var id = "sikkhaloy-print-page";
    var el = document.getElementById(id);
    if (!el) {
        el = document.createElement("style");
        el.id = id;
        document.head.appendChild(el);
    }
    var sizes = {
        A4: "210mm 297mm",
        A5: "148mm 210mm",
        A6: "105mm 148mm",
        letter: "8.5in 11in"
    };
    var paper = sizes[pageSize] || sizes.A4;
    var top = Math.max(0, Number(topSpace) || 0);
    el.textContent =
        "@page { size: " + paper + "; margin: " + top + "px 6mm 6mm 6mm; }" +
        "@media print {" +
        "html, body, #app, .app-shell, .app-main, .app-content {" +
        " height: auto !important; min-height: 0 !important; display: block !important;" +
        " padding: 0 !important; margin: 0 !important; overflow: visible !important; }" +
        ".leave-print-page, .gp-page-wrap {" +
        " margin: 0 !important; padding-top: 0 !important; max-width: none !important; width: 100% !important; }" +
        "}";
    document.body.classList.remove("print-landscape", "print-compact");
    document.body.classList.add("print-portrait", "print-leave");
};

window.sikkhaloyGoTop = (function () {
    var el = null;
    var handler = null;
    function target() {
        return document.querySelector(".app-content") || document.querySelector("#app");
    }
    return {
        bind: function (dotNet) {
            this.unbind();
            el = target();
            if (!el || !dotNet) return;
            handler = function () {
                dotNet.invokeMethodAsync("OnContentScroll", el.scrollTop || 0);
            };
            el.addEventListener("scroll", handler, { passive: true });
            handler();
        },
        unbind: function () {
            if (el && handler) el.removeEventListener("scroll", handler);
            el = null;
            handler = null;
        },
        scroll: function () {
            var box = target();
            if (box) box.scrollTo({ top: 0, behavior: "smooth" });
        }
    };
})();

window.sikkhaloyDownloadStream = async function (fileName, contentType, streamRef) {
    var data = await streamRef.arrayBuffer();
    var blob = new Blob([data], { type: contentType || "application/octet-stream" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement("a");
    a.href = url;
    a.download = fileName || "download";
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(function () { URL.revokeObjectURL(url); }, 1500);
};

window.sikkhaloyExportWord = function (fileName, elementId, school, className) {
    var src = document.getElementById(elementId);
    if (!src) return;
    var esc = function (v) {
        return String(v || "").replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    };
    var css = "body{font-family:'Noto Sans Bengali','Nirmala UI','Segoe UI',sans-serif;font-size:11pt;color:#333;}"
        + "h2,h3{text-align:center;margin:0 0 8px;}"
        + "table{border-collapse:collapse;width:100%;margin:6px 0 16px;}"
        + "th,td{border:1px solid #999;padding:4px 6px;text-align:center;}"
        + "th{background:#F4F4F4;}"
        + ".due-head{font-size:14pt;font-weight:bold;color:#282828;margin:12px 0 6px;text-align:left;}"
        + ".due-amt{color:#c00;font-weight:bold;}";
    var heading = (school ? "<h2>" + esc(school) + "</h2>" : "")
        + (className ? "<h3>Current Dues For Class: " + esc(className) + "</h3>" : "");
    var doc = "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word'>"
        + "<head><meta charset='utf-8'><title>Current Due</title><style>" + css + "</style></head><body>"
        + heading
        + src.innerHTML
        + "</body></html>";
    var blob = new Blob(["\ufeff", doc], { type: "application/msword" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement("a");
    a.href = url;
    a.download = fileName || "Current_Due.doc";
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(function () { URL.revokeObjectURL(url); }, 1500);
};

window.sikkhaloyExportCollectPaper = function (fileName) {
    var src = document.getElementById("exam-collect-word");
    if (!src) return;
    var clone = src.cloneNode(true);
    clone.querySelectorAll(".print-only").forEach(function (el) { el.style.display = "block"; });
    clone.querySelectorAll(".print-header").forEach(function (el) {
        el.style.display = "flex";
        el.style.justifyContent = "center";
        el.style.alignItems = "center";
        el.style.textAlign = "center";
    });
    clone.querySelectorAll(".no-print").forEach(function (el) { el.remove(); });
    clone.querySelectorAll(".exam-col-off").forEach(function (el) { el.remove(); });
    var css = "body{font-family:'Noto Sans Bengali','Nirmala UI','Segoe UI',sans-serif;font-size:11pt;color:#000;}"
        + "h1,h2,.exam-print-line,.exam-collect-meta,.exam-class-gss{text-align:center;margin:2px 0;font-weight:bold;}"
        + ".exam-collect-meta{font-size:14pt;border-bottom:1px solid #000;margin-bottom:8px;display:flex;justify-content:center;gap:8px;}"
        + ".exam-collect-meta span + span::before{content:' | ';}"
        + "h1{font-size:22pt;}"
        + ".print-id{display:none;}"
        + "table{border-collapse:collapse;width:100%;margin:8px 0;}"
        + "th,td{border:1px solid #000;padding:6px 8px;text-align:center;}"
        + "th{background:#333;color:#fff;}"
        + "td.is-name{text-align:left;}"
        + "td.blank{height:28px;min-width:80px;}"
        + ".exam-sign{margin-top:24px;font-weight:bold;}";
    var doc = "<html xmlns:o='urn:schemas-microsoft-com:office:office' xmlns:w='urn:schemas-microsoft-com:office:word'>"
        + "<head><meta charset='utf-8'><style>" + css + "</style></head><body>"
        + clone.innerHTML
        + "</body></html>";
    var blob = new Blob(["\ufeff", doc], { type: "application/msword" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement("a");
    a.href = url;
    a.download = fileName || "Marks_Collect_Paper.doc";
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(function () { URL.revokeObjectURL(url); }, 1500);
};

document.addEventListener("keydown", function (e) {
    if (e.key !== "Tab") return;
    var el = e.target;
    if (!el || !el.classList || !el.classList.contains("exam-mark-input")) return;
    var col = parseInt(el.getAttribute("data-mark-col"), 10);
    var row = parseInt(el.getAttribute("data-mark-row"), 10);
    if (isNaN(col) || isNaN(row)) return;
    var nextRow = row + (e.shiftKey ? -1 : 1);
    var next = document.querySelector('.exam-mark-input[data-mark-col="' + col + '"][data-mark-row="' + nextRow + '"]');
    if (!next) {
        var nextCol = col + (e.shiftKey ? -1 : 1);
        var cells = document.querySelectorAll('.exam-mark-input[data-mark-col="' + nextCol + '"]');
        if (cells.length)
            next = e.shiftKey ? cells[cells.length - 1] : cells[0];
    }
    if (!next) return;
    e.preventDefault();
    next.focus();
    if (typeof next.select === "function") next.select();
}, true);

window.sikkhaloyPrintLandscape = function () {
    var id = "sikkhaloy-print-landscape";
    var style = document.getElementById(id);
    if (!style) {
        style = document.createElement("style");
        style.id = id;
        document.head.appendChild(style);
    }
    style.textContent = "@media print { @page { size: A4 landscape; margin: 4mm; } }";
    window.print();
};

window.sikkhaloyPrintAdmit = function () {
    var id = "sikkhaloy-print-landscape";
    var style = document.getElementById(id);
    if (!style) {
        style = document.createElement("style");
        style.id = id;
        document.head.appendChild(style);
    }
    style.textContent = "@media print { @page { size: A4 landscape; margin: 3mm; } }";
    window.print();
};

window.sikkhaloyOpenUrl = function (url) {
    if (!url) return;
    window.open(url, "_blank");
};
