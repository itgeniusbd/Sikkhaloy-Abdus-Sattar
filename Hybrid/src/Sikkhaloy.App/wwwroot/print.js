window.sikkhaloyParseHexColor = function (hex) {
    hex = String(hex || "#ffffff").trim().replace("#", "");
    if (hex.length === 3)
        hex = hex.charAt(0) + hex.charAt(0) + hex.charAt(1) + hex.charAt(1) + hex.charAt(2) + hex.charAt(2);
    if (hex.length !== 6)
        return [255, 255, 255];
    return [parseInt(hex.slice(0, 2), 16) || 0, parseInt(hex.slice(2, 4), 16) || 0, parseInt(hex.slice(4, 6), 16) || 0];
};

window.sikkhaloyColorDist = function (r, g, b, br, bg, bb) {
    var dr = r - br, dg = g - bg, db = b - bb;
    return Math.sqrt(dr * dr + dg * dg + db * db);
};

window.sikkhaloyLuma = function (r, g, b) {
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
};

window.sikkhaloyReplaceEdgeBackground = function (ctx, w, h, fillHex) {
    var fill = window.sikkhaloyParseHexColor(fillHex);
    var img = ctx.getImageData(0, 0, w, h);
    var d = img.data;
    var nPix = w * h;
    var patch = Math.max(2, Math.min(6, (Math.min(w, h) / 40) | 0));
    function samplePatch(x0, y0) {
        var r = 0, g = 0, b = 0, n = 0;
        for (var y = y0; y < y0 + patch && y < h; y++) {
            for (var x = x0; x < x0 + patch && x < w; x++) {
                var i = (y * w + x) * 4;
                r += d[i];
                g += d[i + 1];
                b += d[i + 2];
                n++;
            }
        }
        return n ? [r / n, g / n, b / n] : [255, 255, 255];
    }
    var corners = [
        samplePatch(0, 0),
        samplePatch(w - patch, 0),
        samplePatch(0, h - patch),
        samplePatch(w - patch, h - patch)
    ];
    var best = 0, bestCount = 0;
    for (var i = 0; i < 4; i++) {
        var votes = 0;
        for (var j = 0; j < 4; j++) {
            if (window.sikkhaloyColorDist(corners[i][0], corners[i][1], corners[i][2],
                corners[j][0], corners[j][1], corners[j][2]) <= 36)
                votes++;
        }
        if (votes > bestCount) {
            bestCount = votes;
            best = i;
        }
    }
    if (bestCount < 3)
        return;
    var br = 0, bgc = 0, bb = 0, cn = 0;
    for (var j = 0; j < 4; j++) {
        var dist = window.sikkhaloyColorDist(corners[best][0], corners[best][1], corners[best][2],
            corners[j][0], corners[j][1], corners[j][2]);
        if (dist > 36) continue;
        br += corners[j][0];
        bgc += corners[j][1];
        bb += corners[j][2];
        cn++;
    }
    if (cn < 3) return;
    br /= cn;
    bgc /= cn;
    bb /= cn;
    var tol = 28;
    var lumaBg = window.sikkhaloyLuma(br, bgc, bb);
    var cx = (w - 1) / 2, cy = (h - 1) / 2;
    var rx = w * 0.38, ry = h * 0.44;
    function inSubject(x, y) {
        var nx = (x - cx) / rx, ny = (y - cy) / ry;
        return nx * nx + ny * ny <= 1;
    }
    function isBgPixel(idx, x, y) {
        if (x != null && inSubject(x, y)) return false;
        if (d[idx + 3] < 40) return true;
        var r = d[idx], g = d[idx + 1], b = d[idx + 2];
        var L = window.sikkhaloyLuma(r, g, b);
        if (Math.abs(L - lumaBg) > 38) return false;
        return window.sikkhaloyColorDist(r, g, b, br, bgc, bb) <= tol;
    }

    var seen = new Uint8Array(nPix);
    var q = [];
    function seed(x, y) {
        if (x < 0 || y < 0 || x >= w || y >= h) return;
        var p = y * w + x;
        if (seen[p] || inSubject(x, y) || !isBgPixel(p * 4, x, y)) return;
        seen[p] = 1;
        q.push(p);
    }
    for (var x = 0; x < w; x++) {
        seed(x, 0);
        seed(x, h - 1);
    }
    for (var y = 0; y < h; y++) {
        seed(0, y);
        seed(w - 1, y);
    }
    if (q.length < 8) return;
    var qs = 0;
    while (qs < q.length) {
        var p = q[qs++];
        var px = p % w;
        var py = (p / w) | 0;
        for (var dy = -1; dy <= 1; dy++) {
            for (var dx = -1; dx <= 1; dx++) {
                if (!dx && !dy) continue;
                var nx = px + dx, ny = py + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                var np = ny * w + nx;
                if (seen[np] || inSubject(nx, ny) || !isBgPixel(np * 4, nx, ny)) continue;
                seen[np] = 1;
                q.push(np);
            }
        }
    }
    var marked = 0;
    for (var n = 0; n < seen.length; n++)
        if (seen[n]) marked++;
    if (marked < nPix * 0.02 || marked > nPix * 0.62)
        return;

    var fringe = new Uint8Array(nPix);
    for (var y = 0; y < h; y++) {
        for (var x = 0; x < w; x++) {
            var p = y * w + x;
            if (seen[p] || inSubject(x, y)) continue;
            var i = p * 4;
            if (window.sikkhaloyColorDist(d[i], d[i + 1], d[i + 2], br, bgc, bb) > tol + 10)
                continue;
            var touch = false;
            for (var dy = -1; dy <= 1 && !touch; dy++) {
                for (var dx = -1; dx <= 1; dx++) {
                    var nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    if (seen[ny * w + nx]) { touch = true; break; }
                }
            }
            if (touch) fringe[p] = 1;
        }
    }

    for (var n = 0; n < nPix; n++) {
        var i = n * 4;
        if (seen[n]) {
            d[i] = fill[0];
            d[i + 1] = fill[1];
            d[i + 2] = fill[2];
            d[i + 3] = 255;
            continue;
        }
        if (!fringe[n]) {
            d[i + 3] = 255;
            continue;
        }
        var a = 0.45;
        d[i] = Math.round(d[i] * a + fill[0] * (1 - a));
        d[i + 1] = Math.round(d[i + 1] * a + fill[1] * (1 - a));
        d[i + 2] = Math.round(d[i + 2] * a + fill[2] * (1 - a));
        d[i + 3] = 255;
    }
    ctx.putImageData(img, 0, 0);
};

window.sikkhaloyPreparePhoto = function (dataUrl, opts) {
    opts = opts || {};
    var maxWidth = Number(opts.maxWidth) || 250;
    var quality = Number(opts.quality) || 0.7;
    var maxBytes = Number(opts.maxBytes) || 40960;
    var bgColor = opts.bgColor || "#ffffff";
    var replaceBg = opts.replaceBg !== false;
    return new Promise(function (resolve, reject) {
        var img = new Image();
        img.onload = function () {
            var workMax = replaceBg ? Math.max(maxWidth, 560) : maxWidth;
            var w = img.width;
            var h = img.height;
            if (w > workMax) {
                h = Math.max(1, Math.round(h * workMax / w));
                w = workMax;
            }
            var source = document.createElement("canvas");
            source.width = w;
            source.height = h;
            var sctx = source.getContext("2d");
            sctx.imageSmoothingEnabled = true;
            sctx.imageSmoothingQuality = "high";
            sctx.fillStyle = bgColor;
            sctx.fillRect(0, 0, w, h);
            sctx.drawImage(img, 0, 0, w, h);
            if (replaceBg) {
                try {
                    window.sikkhaloyReplaceEdgeBackground(sctx, w, h, bgColor);
                } catch (err) { }
            }
            if (w > maxWidth) {
                h = Math.max(1, Math.round(h * maxWidth / w));
                w = maxWidth;
                var scaled = document.createElement("canvas");
                scaled.width = w;
                scaled.height = h;
                var xctx = scaled.getContext("2d");
                xctx.imageSmoothingEnabled = true;
                xctx.imageSmoothingQuality = "high";
                xctx.fillStyle = bgColor;
                xctx.fillRect(0, 0, w, h);
                xctx.drawImage(source, 0, 0, w, h);
                source = scaled;
            }
            var q = quality;
            var result = "";
            var canvas = document.createElement("canvas");
            for (var i = 0; i < 8; i++) {
                canvas.width = w;
                canvas.height = h;
                var ctx = canvas.getContext("2d");
                ctx.imageSmoothingEnabled = true;
                ctx.imageSmoothingQuality = "high";
                ctx.fillStyle = bgColor;
                ctx.fillRect(0, 0, w, h);
                ctx.drawImage(source, 0, 0, w, h);
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
                source = canvas;
                canvas = document.createElement("canvas");
            }
            resolve(result);
        };
        img.onerror = function () { reject(new Error("image")); };
        img.src = dataUrl;
    });
};

window.sikkhaloyCompressImage = function (dataUrl, maxWidth, quality, maxBytes, bgColor) {
    return window.sikkhaloyPreparePhoto(dataUrl, {
        maxWidth: maxWidth,
        quality: quality,
        maxBytes: maxBytes,
        bgColor: bgColor || "#ffffff",
        replaceBg: false
    });
};

window.sikkhaloyCrop = (function () {
    var st = null;

    function clamp(v, a, b) { return Math.max(a, Math.min(b, v)); }

    function boxRect() {
        var v = st.view.getBoundingClientRect();
        var side = Math.min(v.width, v.height) * 0.78;
        return { left: (v.width - side) / 2, top: (v.height - side) / 2, side: side };
    }

    function applyTransform() {
        st.img.style.transform = "translate(" + st.x + "px," + st.y + "px) scale(" + st.scale + ")";
    }

    function minScale() {
        var b = boxRect();
        return Math.max(b.side / st.natW, b.side / st.natH);
    }

    function constrain() {
        var b = boxRect();
        var w = st.natW * st.scale;
        var h = st.natH * st.scale;
        st.x = clamp(st.x, b.left + b.side - w, b.left);
        st.y = clamp(st.y, b.top + b.side - h, b.top);
    }

    function layoutBox() {
        var b = boxRect();
        st.hole.style.width = b.side + "px";
        st.hole.style.height = b.side + "px";
        st.hole.style.left = b.left + "px";
        st.hole.style.top = b.top + "px";
    }

    function fit() {
        var v = st.view.getBoundingClientRect();
        st.scale = Math.max(minScale(), Math.max(v.width / st.natW, v.height / st.natH));
        var w = st.natW * st.scale;
        var h = st.natH * st.scale;
        st.x = (v.width - w) / 2;
        st.y = (v.height - h) / 2;
        constrain();
        applyTransform();
        layoutBox();
    }

    function zoomAt(factor, clientX, clientY) {
        if (!st) return;
        var r = st.view.getBoundingClientRect();
        var px = clientX - r.left;
        var py = clientY - r.top;
        var next = clamp(st.scale * factor, minScale(), minScale() * 8);
        if (Math.abs(next - st.scale) < 0.0001) return;
        st.x = px - (px - st.x) * (next / st.scale);
        st.y = py - (py - st.y) * (next / st.scale);
        st.scale = next;
        constrain();
        applyTransform();
    }

    function onResize() {
        if (!st) return;
        layoutBox();
        st.scale = Math.max(st.scale, minScale());
        constrain();
        applyTransform();
    }

    function onDown(e) {
        if (!st) return;
        st.view.setPointerCapture(e.pointerId);
        st.pointers.set(e.pointerId, { x: e.clientX, y: e.clientY });
        if (st.pointers.size === 2) {
            var pts = Array.from(st.pointers.values());
            st.lastPinch = Math.hypot(pts[0].x - pts[1].x, pts[0].y - pts[1].y);
        }
        e.preventDefault();
    }

    function onMove(e) {
        if (!st || !st.pointers.has(e.pointerId)) return;
        var prev = st.pointers.get(e.pointerId);
        st.pointers.set(e.pointerId, { x: e.clientX, y: e.clientY });
        if (st.pointers.size === 2) {
            var pts = Array.from(st.pointers.values());
            var dist = Math.hypot(pts[0].x - pts[1].x, pts[0].y - pts[1].y);
            if (st.lastPinch > 0)
                zoomAt(dist / st.lastPinch, (pts[0].x + pts[1].x) / 2, (pts[0].y + pts[1].y) / 2);
            st.lastPinch = dist;
        } else if (st.pointers.size === 1) {
            st.x += e.clientX - prev.x;
            st.y += e.clientY - prev.y;
            constrain();
            applyTransform();
        }
        e.preventDefault();
    }

    function onUp(e) {
        if (!st) return;
        st.pointers.delete(e.pointerId);
        if (st.pointers.size < 2) st.lastPinch = 0;
        try { st.view.releasePointerCapture(e.pointerId); } catch (err) { }
    }

    function onWheel(e) {
        if (!st) return;
        e.preventDefault();
        zoomAt(e.deltaY < 0 ? 1.08 : 0.92, e.clientX, e.clientY);
    }

    function start(host, dataUrl) {
        destroy();
        if (!host) return;
        host.innerHTML = "";
        var view = document.createElement("div");
        view.className = "sm-crop-view";
        var img = document.createElement("img");
        img.alt = "";
        img.draggable = false;
        var hole = document.createElement("div");
        hole.className = "sm-crop-hole";
        view.appendChild(img);
        view.appendChild(hole);
        host.appendChild(view);
        st = {
            host: host, view: view, img: img, hole: hole,
            x: 0, y: 0, scale: 1, natW: 1, natH: 1,
            pointers: new Map(), lastPinch: 0
        };
        img.onload = function () {
            st.natW = img.naturalWidth || 1;
            st.natH = img.naturalHeight || 1;
            img.style.width = st.natW + "px";
            img.style.height = st.natH + "px";
            fit();
        };
        img.src = dataUrl;
        view.addEventListener("pointerdown", onDown);
        view.addEventListener("pointermove", onMove);
        view.addEventListener("pointerup", onUp);
        view.addEventListener("pointercancel", onUp);
        window.addEventListener("pointerup", onUp);
        window.addEventListener("pointercancel", onUp);
        view.addEventListener("wheel", onWheel, { passive: false });
        window.addEventListener("resize", onResize);
    }

    function zoom(delta) {
        if (!st) return;
        var r = st.view.getBoundingClientRect();
        zoomAt(delta > 0 ? 1.12 : 0.88, r.left + r.width / 2, r.top + r.height / 2);
    }

    function apply() {
        if (!st || !st.img) return "";
        var nw = st.img.naturalWidth || st.natW || 1;
        var nh = st.img.naturalHeight || st.natH || 1;
        var b = boxRect();
        var scale = st.scale || 1;
        if (scale <= 0 || !b.side) return "";
        var sx = (b.left - st.x) / scale;
        var sy = (b.top - st.y) / scale;
        var sw = b.side / scale;
        var sh = b.side / scale;
        if (!isFinite(sx) || !isFinite(sy) || !isFinite(sw) || !isFinite(sh))
            return "";
        sx = clamp(sx, 0, Math.max(0, nw - 1));
        sy = clamp(sy, 0, Math.max(0, nh - 1));
        sw = Math.max(1, Math.min(sw, nw - sx));
        sh = Math.max(1, Math.min(sh, nh - sy));
        var out = 640;
        var canvas = document.createElement("canvas");
        canvas.width = out;
        canvas.height = out;
        var ctx = canvas.getContext("2d");
        ctx.fillStyle = "#ffffff";
        ctx.fillRect(0, 0, out, out);
        ctx.drawImage(st.img, sx, sy, sw, sh, 0, 0, out, out);
        return canvas.toDataURL("image/jpeg", 0.92);
    }

    function destroy() {
        if (!st) return;
        window.removeEventListener("resize", onResize);
        window.removeEventListener("pointerup", onUp);
        window.removeEventListener("pointercancel", onUp);
        st.host.innerHTML = "";
        st = null;
    }

    return { start: start, zoom: zoom, apply: apply, destroy: destroy };
})();

window.sikkhaloyCropStart = function (host, dataUrl) { window.sikkhaloyCrop.start(host, dataUrl); };
window.sikkhaloyCropZoom = function (delta) { window.sikkhaloyCrop.zoom(delta); };
window.sikkhaloyCropApply = function () {
    try {
        window.__sikkhaloyCropData = window.sikkhaloyCrop.apply() || "";
        return window.__sikkhaloyCropData ? "ok" : "";
    } catch (e) {
        window.__sikkhaloyCropData = "";
        return "";
    }
};
window.sikkhaloyCropLength = function () {
    return (window.__sikkhaloyCropData || "").length;
};
window.sikkhaloyCropRead = function (start, len) {
    var d = window.__sikkhaloyCropData || "";
    start = Math.max(0, Number(start) || 0);
    len = Math.max(0, Number(len) || 0);
    return d.substring(start, start + len);
};
window.sikkhaloyCropClear = function () {
    window.__sikkhaloyCropData = "";
};
window.sikkhaloyCropDestroy = function () { window.sikkhaloyCrop.destroy(); };

window.sikkhaloyPhotoSwatchesLoad = function (userKey) {
    try {
        var raw = localStorage.getItem("sikkhaloy-photo-swatches-" + (userKey || "0"));
        var arr = raw ? JSON.parse(raw) : null;
        return Array.isArray(arr) ? arr : null;
    } catch (e) {
        return null;
    }
};

window.sikkhaloyPhotoSwatchesSave = function (userKey, colors) {
    try {
        localStorage.setItem("sikkhaloy-photo-swatches-" + (userKey || "0"), JSON.stringify(colors || []));
    } catch (e) { }
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
    var schoolPx = narrow ? (inches <= 3 ? 16 : 18) : Math.max(24, fontPx + 13);
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
        "html, body, #app, .app-shell, .app-main, .app-content, .card, .mr-page, .mr-preview, .mr-layout {" +
        " height: auto !important; min-height: 0 !important; width: 100% !important; max-width: none !important;" +
        " display: block !important; padding: 0 !important; margin: 0 !important;" +
        " overflow: visible !important; box-shadow: none !important; border: 0 !important;" +
        " background: #fff !important; -webkit-print-color-adjust: economy; print-color-adjust: economy;" +
        " transform: none !important; zoom: 1 !important; }" +
        ".mr-preview .mr-sheet { box-shadow: none !important; background: #fff !important; }" +
        ".mr-sheet { width: " + width + " !important; max-width: " + width + " !important;" +
        " margin: 0 auto !important; padding: 0 !important; border: 0 !important; height: auto !important;" +
        " background: #fff !important; box-shadow: none !important; }" +
        ".mr-page .mgrid th { background: #000 !important; color: #fff !important; font-weight: 700 !important;" +
        " border-color: #000 !important; -webkit-print-color-adjust: exact !important; print-color-adjust: exact !important; }" +
        ".no-print, .app-sidebar, .app-header { display: none !important; }" +
        ".print-receipt .print-header, .print-receipt .print-date, .print-receipt .print-only," +
        ".print-receipt .app-header, .print-receipt .app-sidebar { display: none !important; }" +
        ".mr-sheet p, .mr-sheet strong { font-weight: 700 !important; }" +
        ".mr-name-logo { max-width: 100% !important; max-height: " + (narrow ? "42px" : "72px") + " !important; display: block !important; margin: 0 auto 4px !important; }" +
        ".mr-school { font-size: " + schoolPx + "px !important; font-weight: 800 !important; color: #000 !important; text-align: center !important; line-height: 1.2 !important; }" +
        ".mr-addr, .mr-addr span { font-size: " + infoPx + "px !important; font-weight: 500 !important; color: #000 !important; text-align: center !important; }" +
        ".mr-head { font-size: " + titlePx + "px !important; letter-spacing: " + (narrow ? "0" : "1px") + " !important; font-weight: 700 !important; text-align: center !important; }" +
        ".mr-info { margin: " + (narrow ? "5px 0 8px" : "8px 0 10px") + " !important; overflow: visible !important; text-align: left !important; align-items: start !important; }" +
        ".mr-info > div { padding: " + (narrow ? "3px 5px" : "4px 8px") + " !important; overflow: visible !important; align-self: start !important; }" +
        ".mr-info p { font-size: " + infoPx + "px !important; white-space: normal !important; overflow-wrap: anywhere !important;" +
        " text-align: left !important; font-weight: 600 !important; line-height: 1.35 !important;" +
        " overflow: visible !important; margin: 0 !important; padding: 3px 0 !important; }" +
        ".mr-info .mr-dates { font-size: " + (narrow ? "7.5px" : (infoPx - 1) + "px") + " !important; line-height: 1.35 !important; letter-spacing: -0.02em !important; }" +
        ".mr-info strong { font-weight: 800 !important; }" +
        ".mr-money { white-space: nowrap !important; font-weight: 800 !important; }" +
        ".mr-page .mr-grid th, .mr-page .mgrid th { font-size: " + tablePx + "px !important; padding: " + (narrow ? "3px 1px" : "5px 3px") + " !important;" +
        " text-align: center !important; font-weight: 700 !important; }" +
        ".mr-page .mr-grid td, .mr-page .mgrid td { font-size: " + tablePx + "px !important; padding: " + (narrow ? "3px 1px" : "4px 3px") + " !important;" +
        " white-space: normal !important; vertical-align: middle !important; }" +
        ".mr-page .mr-grid td.mr-payfor, .mr-page .mgrid td.mr-payfor { text-align: center !important; font-weight: 600 !important; }" +
        ".mr-page .mr-grid tfoot tr.mr-total-row td, .mr-page .mgrid tfoot tr.mr-total-row td { font-size: " + (tablePx + 1) + "px !important; font-weight: 800 !important; }" +
        ".mr-page .mr-grid tfoot td.mr-total-label, .mr-page .mgrid tfoot td.mr-total-label { text-align: right !important; padding-right: 6px !important; }" +
        ".mr-page .mr-grid td.mr-fee, .mr-page .mr-grid td.mr-amt, .mr-page .mgrid td.mr-fee, .mr-page .mgrid td.mr-amt {" +
        " text-align: center !important; font-weight: 800 !important; }" +
        ".mr-page .mr-grid tfoot td, .mr-page .mgrid tfoot td { text-align: center !important; font-weight: 800 !important; }" +
        ".mr-words, .mr-recv, .mr-due-title, .oi-total, .mr-blessing { font-size: " + infoPx + "px !important;" +
        " white-space: normal !important; text-align: center !important; font-weight: 700 !important; }" +
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

    var landscapeHint = document.getElementById("sikkhaloy-print-landscape");
    if (landscapeHint)
        landscapeHint.textContent = "";

    var isLandscape = String(orientation || "").toLowerCase() === "landscape";
    var size = isLandscape ? "A4 landscape" : "A4 portrait";
    var margin = marginOverride || (compact ? "6mm 8mm" : "10mm");
    el.textContent =
        "@page { size: " + size + "; margin: " + margin + "; }" +
        "@media print {" +
        "html, body, #app, .ad-shell, .ad-main, .ad-content, .app-shell, .app-main, .app-content, .card, .ai-print {" +
        " background: #fff !important; height: auto !important; overflow: visible !important;" +
        " box-shadow: none !important; padding: 0 !important; margin: 0 !important; border: 0 !important; }" +
        ".ad-sidebar, .ad-header, .app-sidebar, .app-header, .go-top, .nav-back, .no-print { display: none !important; }" +
        ".ad-shell, .app-shell { display: block !important; grid-template-columns: none !important; }" +
        "}";
    document.body.classList.remove("print-portrait", "print-landscape", "print-compact");
    document.body.classList.add(isLandscape ? "print-landscape" : "print-portrait");
    if (compact)
        document.body.classList.add("print-compact");

    var restore = function () {
        document.body.classList.remove("print-portrait", "print-landscape", "print-compact");
        window.removeEventListener("afterprint", restore);
    };
    window.addEventListener("afterprint", restore);
    window.print();
};

window.sikkhaloyTcPrintSave = function (opts) {
    try { localStorage.setItem("sikkhaloy-tc-print", JSON.stringify(opts || {})); } catch (e) { }
};

window.sikkhaloyTcPrintLoad = function () {
    try {
        return JSON.parse(localStorage.getItem("sikkhaloy-tc-print") || "{}");
    } catch (e) {
        return {};
    }
};

window.sikkhaloyPrintTc = function (opts) {
    opts = opts || {};
    try { localStorage.setItem("sikkhaloy-tc-print", JSON.stringify(opts)); } catch (e) { }

    var id = "sikkhaloy-print-page";
    var el = document.getElementById(id);
    if (!el) {
        el = document.createElement("style");
        el.id = id;
        document.head.appendChild(el);
    }
    var landscapeHint = document.getElementById("sikkhaloy-print-landscape");
    if (landscapeHint)
        landscapeHint.textContent = "";

    var topPx = Math.max(0, Number(opts.topSpace) || 0);
    var hideIns = !!opts.hideIns;
    var margin = topPx > 0 ? (topPx + "px 8mm 8mm 8mm") : "8mm";
    var hideCss = hideIns
        ? ".sm-tc-page.hide-ins .cert-sheet .page-print-header, .sm-tc-page.hide-ins .cert-sheet .print-header, .sm-tc-page.hide-ins .cert-sheet .print-header-banner { display:none !important; }"
        : "";

    el.textContent =
        "@page { margin: " + margin + "; }" +
        "@media print {" +
        "html, body, #app, .app-shell, .app-main, .app-content {" +
        " background:#fff !important; height:100% !important; max-height:100% !important;" +
        " overflow:hidden !important; padding:0 !important; margin:0 !important; }" +
        ".app-sidebar, .app-header, .go-top, .nav-back, .no-print, .pager, .layout-print-header, .admin-notice, #blazor-error-ui, .acc-loading-back { display:none !important; }" +
        ".app-shell { display:block !important; grid-template-columns:none !important; background:#fff !important; }" +
        ".sm-tc-page.card { padding:0 !important; margin:0 !important; border:0 !important; box-shadow:none !important; background:transparent !important; width:100% !important; height:100% !important; max-width:none !important; }" +
        ".sm-tc-page .cert-sheet { width:100% !important; height:100% !important; margin:0 !important; padding:0 !important; border:0 !important; min-height:0 !important; background:transparent !important; }" +
        ".sm-tc-page .cert-frame { position:fixed !important; inset:0 !important; width:auto !important; min-height:0 !important; height:auto !important; padding:3px !important; box-sizing:border-box !important; }" +
        ".sm-tc-page .cert-frame-inner { height:100% !important; min-height:0 !important; overflow:hidden !important; padding:4mm 6mm 5mm !important; box-sizing:border-box !important; }" +
        ".sm-tc-page .print-name-logo { max-height:22mm !important; width:100% !important; object-fit:fill !important; }" +
        ".sm-tc-page .cert-signs { padding-top:8mm !important; margin-top:auto !important; }" +
        ".sm-tc-page .tc-upto { border:none !important; width:auto !important; padding:0 !important; background:transparent !important; box-shadow:none !important; }" +
        hideCss +
        "}";
    document.body.classList.remove("print-portrait", "print-landscape", "print-compact");
    var restore = function () {
        document.body.classList.remove("print-portrait", "print-landscape", "print-compact");
        window.removeEventListener("afterprint", restore);
    };
    window.addEventListener("afterprint", restore);
    window.print();
};

window.sikkhaloyPrintCert = function () {
    var id = "sikkhaloy-print-page";
    var el = document.getElementById(id);
    if (!el) {
        el = document.createElement("style");
        el.id = id;
        document.head.appendChild(el);
    }
    var landscapeHint = document.getElementById("sikkhaloy-print-landscape");
    if (landscapeHint)
        landscapeHint.textContent = "";

    el.textContent =
        "@page { margin: 6mm; }" +
        "@media print {" +
        "html, body, #app, .app-shell, .app-main, .app-content {" +
        " background:#fff !important; height:100% !important; max-height:100% !important;" +
        " overflow:hidden !important; padding:0 !important; margin:0 !important; }" +
        ".app-sidebar, .app-header, .go-top, .nav-back, .no-print, .pager, .layout-print-header, .admin-notice, #blazor-error-ui, .acc-loading-back { display:none !important; }" +
        ".app-shell { display:block !important; grid-template-columns:none !important; background:#fff !important; }" +
        ".si-certs-page.card { padding:0 !important; margin:0 !important; border:0 !important; box-shadow:none !important; background:transparent !important; width:100% !important; height:100% !important; max-width:none !important; }" +
        ".si-certs-page .cert-sheet { width:100% !important; height:100% !important; margin:0 !important; padding:0 !important; border:0 !important; min-height:0 !important; background:transparent !important; }" +
        ".si-certs-page .cert-frame { position:fixed !important; inset:0 !important; width:auto !important; min-height:0 !important; height:auto !important; padding:3px !important; box-sizing:border-box !important; }" +
        ".si-certs-page .cert-frame-inner { height:100% !important; min-height:0 !important; overflow:hidden !important; padding:4mm 6mm 5mm !important; box-sizing:border-box !important; }" +
        ".si-certs-page .print-name-logo { max-height:22mm !important; width:100% !important; object-fit:fill !important; }" +
        ".si-certs-page .cert-signs { padding-top:8mm !important; margin-top:auto !important; }" +
        "}";
    document.body.classList.remove("print-portrait", "print-landscape", "print-compact");
    document.body.classList.add("print-portrait");
    var restore = function () {
        document.body.classList.remove("print-portrait", "print-landscape", "print-compact");
        window.removeEventListener("afterprint", restore);
    };
    window.addEventListener("afterprint", restore);
    window.print();
};

window.sikkhaloyPrintLeave = function (opts) {
    window.sikkhaloyApplyLeavePrint(opts);
    window.print();
};

window.sikkhaloyApplyLeavePrint = function (opts) {
    opts = opts || {};
    // Back-compat: old call (pageSize string, topSpace number)
    if (typeof opts === "string") {
        opts = { size: arguments[0], topSpace: arguments[1] };
    }

    var id = "sikkhaloy-print-page";
    var el = document.getElementById(id);
    if (!el) {
        el = document.createElement("style");
        el.id = id;
        document.head.appendChild(el);
    }

    var inches = Number(opts.size);
    if (!isFinite(inches)) inches = 4;
    if (inches < 3) inches = 3;
    if (inches > 6) inches = 6;

    var topPx = Math.max(0, Number(opts.topSpace) || 0);
    var fontPx = Math.min(20, Math.max(8, Number(opts.fontSize) || 11));
    var narrow = inches <= 3.5;
    var sideMm = Math.max(4, (210 - inches * 25.4) / 2);
    var width = inches + "in";
    var logoH = narrow ? "36px" : "56px";

    el.textContent =
        "@page { size: A4 portrait; margin: " + topPx + "px " + sideMm + "mm 8mm " + sideMm + "mm !important; }" +
        "@media print {" +
        "html, body, #app, .app-shell, .app-main, .app-content, .card, .leave-print-page {" +
        " height: auto !important; min-height: 0 !important; width: 100% !important; max-width: none !important;" +
        " display: block !important; padding: 0 !important; margin: 0 !important;" +
        " overflow: visible !important; box-shadow: none !important; border: 0 !important;" +
        " transform: none !important; zoom: 1 !important; }" +
        ".gp-page-wrap {" +
        " width: " + width + " !important; max-width: " + width + " !important;" +
        " margin: 0 auto !important; padding-top: 0 !important;" +
        " font-size: " + fontPx + "px !important; }" +
        ".gp-name-logo { max-height: " + logoH + " !important; width: 100% !important; object-fit: contain !important; display: block !important; margin: 0 auto !important; }" +
        ".print-leave .print-header, .print-leave .print-only," +
        ".print-leave .layout-print-header, .print-leave .app-header, .print-leave .app-sidebar { display: none !important; }" +
        "}";

    document.body.classList.remove("print-landscape", "print-compact", "print-receipt");
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

window.sikkhaloyDownloadText = function (fileName, contentType, text) {
    var blob = new Blob(["\uFEFF" + String(text || "")], { type: contentType || "text/plain;charset=utf-8" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement("a");
    a.href = url;
    a.download = fileName || "download";
    document.body.appendChild(a);
    a.click();
    a.remove();
    setTimeout(function () { URL.revokeObjectURL(url); }, 1500);
};

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

document.addEventListener("wheel", function (e) {
    var t = e.target;
    if (!t || t.tagName !== "INPUT" || t.type !== "number") return;
    if (document.activeElement === t) {
        t.blur();
        e.preventDefault();
    }
}, { passive: false, capture: true });

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
    window.sikkhaloyPrint(null, false, "4mm");
};

window.sikkhaloyPrintAdmit = function () {
    window.sikkhaloyPrint(null, true, "3mm");
};

window.sikkhaloyOpenUrl = function (url) {
    if (!url) return;
    window.open(url, "_blank");
};

window.sikkhaloySessionSave = function (json) {
    try { localStorage.setItem("sikkhaloy.session", json || ""); } catch (e) { }
};

window.sikkhaloySessionLoad = function () {
    try { return localStorage.getItem("sikkhaloy.session") || ""; } catch (e) { return ""; }
};

window.sikkhaloySessionClear = function () {
    try { localStorage.removeItem("sikkhaloy.session"); } catch (e) { }
};

window.sikkhaloyFormValues = function (ids) {
    var o = {};
    ids = ids || [];
    for (var i = 0; i < ids.length; i++) {
        var id = ids[i];
        var el = document.getElementById(id);
        o[id] = el ? String(el.value || "") : "";
    }
    return o;
};

window.sikkhaloyFaultBulkLoad = function (userKey) {
    try {
        var raw = localStorage.getItem("sikkhaloy-fault-bulk-" + (userKey || "0"));
        return raw ? JSON.parse(raw) : null;
    } catch (e) {
        return null;
    }
};

window.sikkhaloyFaultBulkSave = function (userKey, data) {
    try {
        localStorage.setItem("sikkhaloy-fault-bulk-" + (userKey || "0"), JSON.stringify(data || {}));
    } catch (e) { }
};
