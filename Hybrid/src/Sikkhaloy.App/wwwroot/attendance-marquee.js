// Attendance display (IN/OUT photo strips) marquee.
// Replaces the fixed-duration CSS animation (38s for the whole strip => speed grew with card count,
// ~800-1800 px/s on a busy day) with a constant px/second scroller, similar to V2 liMarquee
// (Attendance_Slider: IN 50 px/s left, OUT 20 px/s right).
//  - ONE requestAnimationFrame loop per page, no matter how often start() is called or the
//    component re-renders / refreshes every 20s (no stacked loops => no speed-up).
//  - Offset is stored per strip key (data-marquee-key) in pixels, so a data refresh that adds or
//    removes cards does not restart or jump the strip.
//  - Pauses while hovered/touched, while the tab is hidden, and for a moment at each loop point.
const SPEED_PX_PER_SEC = { in: 50, out: 30 };
const LOOP_HOLD_SEC = 2.5;   // pause when the list wraps back to its first card
const START_HOLD_SEC = 1.5;  // pause when a strip first appears
const state = new Map();     // key -> { offset, hold }
let rafId = 0;
let lastTs = 0;
let users = 0;

function trackState(key) {
    let s = state.get(key);
    if (!s) {
        s = { offset: 0, hold: START_HOLD_SEC };
        state.set(key, s);
    }
    return s;
}

function tick(ts) {
    rafId = 0;
    const dt = lastTs ? Math.min((ts - lastTs) / 1000, 0.25) : 0;
    lastTs = ts;
    if (!document.hidden) {
        const tracks = document.querySelectorAll('.att-disp-page .marquee-track[data-marquee-key]');
        for (const track of tracks) {
            if (!track.classList.contains('js-marquee'))
                track.classList.add('js-marquee'); // disables the CSS fallback animation
            const key = track.getAttribute('data-marquee-key');
            const s = trackState(key);
            const half = track.scrollWidth / 2;   // list is rendered twice
            if (!(half > 0)) continue;
            const wrap = track.parentElement;
            const paused = wrap && (wrap.matches(':hover') || wrap.classList.contains('is-touch-paused'));
            if (s.offset >= half) s.offset %= half;  // list got shorter after a refresh
            if (!paused) {
                if (s.hold > 0) {
                    s.hold -= dt;
                } else {
                    const speed = key.endsWith('-out') ? SPEED_PX_PER_SEC.out : SPEED_PX_PER_SEC.in;
                    s.offset += speed * dt;
                    if (s.offset >= half) {
                        s.offset -= half;
                        s.hold = LOOP_HOLD_SEC;
                    }
                }
            }
            // OUT strips move right (as in V2), IN strips move left.
            const x = key.endsWith('-out') ? (s.offset - half) : -s.offset;
            track.style.transform = 'translate3d(' + x.toFixed(1) + 'px,0,0)';
        }
    }
    if (users > 0)
        rafId = requestAnimationFrame(tick);
}

function onTouch(e) {
    const wrap = e.target && e.target.closest ? e.target.closest('.att-disp-page .str_wrap') : null;
    if (!wrap) return;
    wrap.classList.add('is-touch-paused');
    clearTimeout(wrap.__marqueeTouchTimer);
    wrap.__marqueeTouchTimer = setTimeout(() => wrap.classList.remove('is-touch-paused'), 4000);
}

// Idempotent per component instance: call once from OnAfterRenderAsync(firstRender).
export function start() {
    users++;
    if (users === 1) {
        document.addEventListener('touchstart', onTouch, { passive: true });
        lastTs = 0;
    }
    if (!rafId)
        rafId = requestAnimationFrame(tick);
}

export function stop() {
    users = Math.max(0, users - 1);
    if (users === 0) {
        if (rafId) cancelAnimationFrame(rafId);
        rafId = 0;
        lastTs = 0;
        state.clear();
        document.removeEventListener('touchstart', onTouch);
    }
}
