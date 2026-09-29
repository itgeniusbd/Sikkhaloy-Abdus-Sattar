(function () {
  "use strict";

  var THRESHOLD = 70;
  var MAX_PULL = 120;
  var SELECTORS = ".app-content, .sp-main";
  var SKIP = ".dc-login, .dc-login-shell, .dc-shell, .year-loading";
  var attached = typeof WeakSet !== "undefined" ? new WeakSet() : null;
  var indicator = null;
  var active = null;

  function ensureIndicator() {
    if (indicator) return indicator;
    indicator = document.createElement("div");
    indicator.className = "ptr-indicator";
    indicator.setAttribute("aria-hidden", "true");
    indicator.innerHTML = '<span class="ptr-spinner"></span><span class="ptr-label">Refresh</span>';
    document.body.appendChild(indicator);
    return indicator;
  }

  function setIndicator(pull, refreshing) {
    var el = ensureIndicator();
    var p = Math.max(0, Math.min(pull, MAX_PULL));
    el.classList.toggle("is-visible", p > 8 || refreshing);
    el.classList.toggle("is-ready", p >= THRESHOLD && !refreshing);
    el.classList.toggle("is-refreshing", !!refreshing);
    el.style.transform = "translate(-50%, " + (refreshing ? 12 : Math.max(-40, p - 48)) + "px)";
    var label = el.querySelector(".ptr-label");
    if (label) {
      label.textContent = refreshing ? "Loading..." : (p >= THRESHOLD ? "Release" : "Refresh");
    }
  }

  function hideIndicator() {
    if (!indicator) return;
    indicator.classList.remove("is-visible", "is-ready", "is-refreshing");
    indicator.style.transform = "translate(-50%, -48px)";
  }

  function atTop(el) {
    if (!el) return true;
    return el.scrollTop <= 2;
  }

  function shouldSkip() {
    return !!document.querySelector(SKIP);
  }

  function doRefresh() {
    setIndicator(THRESHOLD, true);
    try {
      window.location.reload();
    } catch (e) {
      hideIndicator();
    }
  }

  function bind(el) {
    if (!el) return;
    if (attached) {
      if (attached.has(el)) return;
      attached.add(el);
    } else if (el.getAttribute("data-ptr") === "1") {
      return;
    } else {
      el.setAttribute("data-ptr", "1");
    }

    var startY = 0;
    var pulling = false;
    var pull = 0;

    function onStart(e) {
      if (shouldSkip() || active) return;
      if (!e.touches || e.touches.length !== 1) return;
      if (!atTop(el)) return;
      startY = e.touches[0].clientY;
      pulling = false;
      pull = 0;
      active = el;
    }

    function onMove(e) {
      if (active !== el || !e.touches || e.touches.length !== 1) return;
      if (shouldSkip() || !atTop(el)) {
        pulling = false;
        pull = 0;
        hideIndicator();
        active = null;
        return;
      }
      var dy = e.touches[0].clientY - startY;
      if (dy < 10 && !pulling) return;
      if (dy <= 0) {
        pulling = false;
        pull = 0;
        hideIndicator();
        return;
      }
      pulling = true;
      pull = Math.min(MAX_PULL, dy * 0.5);
      setIndicator(pull, false);
      if (e.cancelable) e.preventDefault();
    }

    function onEnd() {
      if (active !== el) return;
      var shouldRefresh = pulling && pull >= THRESHOLD;
      pulling = false;
      active = null;
      if (shouldRefresh) doRefresh();
      else {
        pull = 0;
        hideIndicator();
      }
    }

    el.addEventListener("touchstart", onStart, { passive: true, capture: true });
    el.addEventListener("touchmove", onMove, { passive: false, capture: true });
    el.addEventListener("touchend", onEnd, { passive: true, capture: true });
    el.addEventListener("touchcancel", onEnd, { passive: true, capture: true });
  }

  function scan() {
    if (shouldSkip()) {
      hideIndicator();
      return;
    }
    var nodes = document.querySelectorAll(SELECTORS);
    if (nodes.length) {
      nodes.forEach(bind);
      return;
    }
    // fallback: page body scroll
    bind(document.scrollingElement || document.documentElement);
  }

  function boot() {
    scan();
    if (typeof MutationObserver !== "undefined") {
      var mo = new MutationObserver(function () { scan(); });
      mo.observe(document.documentElement, { childList: true, subtree: true });
    }
    setInterval(scan, 1200);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
  } else {
    boot();
  }
})();
