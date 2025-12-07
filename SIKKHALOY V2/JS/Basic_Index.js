// --- Persistent sidebar visibility guardian (adds watch) ---
(function(){
 function enforce(){
 var b=document.body; if(!b) return; if(!b.classList.contains('lock-sidebar')) return;
 if(b.classList.contains('hide-sidedrawer')){ b.classList.remove('hide-sidedrawer'); }
 var sd=document.getElementById('sidedrawer');
 if(sd && (sd.style.transform==='' || sd.style.transform==='translate(0px)')){
 sd.style.transform='translate(250px)';
 }
 }
 setInterval(enforce,400); // lightweight interval
 if(typeof MutationObserver!=='undefined'){
 new MutationObserver(enforce).observe(document.documentElement,{attributes:true,subtree:true,attributeFilter:['class','style']});
 }
 if(typeof Sys!=='undefined'&&Sys.Application){ Sys.Application.add_load(enforce); }
})();

// --- Force lock sidebar early ONLY if explicitly requested via window.forceLockSidebar
(function(){
 var b=document.body;
 if(!b) return;
 // Only lock if explicitly requested
 if(window.forceLockSidebar){
 b.classList.add('lock-sidebar');
 b.classList.remove('hide-sidedrawer');
 var sd=document.getElementById('sidedrawer');
 if(sd){ sd.style.transform='translate(250px)'; sd.style.visibility='visible'; }
 // Re-assert after partial postbacks
 if(typeof Sys!=='undefined' && Sys.Application){
 Sys.Application.add_load(function(){
 b.classList.add('lock-sidebar');
 b.classList.remove('hide-sidedrawer');
 var sd=document.getElementById('sidedrawer');
 if(sd){ sd.style.transform='translate(250px)'; sd.style.visibility='visible'; }
 });
 }
 document.addEventListener('DOMContentLoaded',function(){
 b.classList.add('lock-sidebar');
 b.classList.remove('hide-sidedrawer');
 });
 }
})();

jQuery(function ($) {
 var $bodyEl = $('body'),
 $sidedrawerEl = $('#sidedrawer');

 // =====================================================================
 // Show Sidedrawer (mobile overlay)
 // =====================================================================
 function showSidedrawer() {
 // Only check if forceLockSidebar is explicitly true
 if (window.forceLockSidebar === true) {
 $bodyEl.removeClass('hide-sidedrawer');
 return;
 }
 var options = {
 onclose: function () {
 $sidedrawerEl.removeClass('active').appendTo(document.body);
 }
 };
 var $overlayEl = $(mui.overlay('on', options));
 $sidedrawerEl.appendTo($overlayEl);
 setTimeout(function () { $sidedrawerEl.addClass('active'); },20);
 }

 // =====================================================================
 // Hide / Toggle Sidedrawer (desktop)
 // =====================================================================
 function hideSidedrawer() {
 // Only check if forceLockSidebar is explicitly true
 if (window.forceLockSidebar === true) {
 console.warn('Sidebar toggle prevented (page protection active)');
 $bodyEl.removeClass('hide-sidedrawer');
 return;
 }
 $bodyEl.toggleClass('hide-sidedrawer');
 }

 $('.js-show-sidedrawer').on('click', function (e) {
 e.preventDefault();
 showSidedrawer();
 return false;
 });
 
 $('.js-hide-sidedrawer').on('click', function (e) {
 e.preventDefault();
 hideSidedrawer();
 return false;
 });

 // =====================================================================
 // Accordion animation for menu groups
 // =====================================================================
 var $titleEls = $('strong', $sidedrawerEl);
 $titleEls.next().hide();
 $titleEls.on('click', function () {
 $(this).next().slideToggle(200);
 });

 // =====================================================================
 // Page-level lock support (only if explicitly set to true)
 // =====================================================================
 if (window.forceLockSidebar === true) {
 // Make sure it is visible immediately
 $bodyEl.removeClass('hide-sidedrawer');
 // Neutralise toggles completely
 $('.js-show-sidedrawer, .js-hide-sidedrawer, .sidedrawer-toggle').off('click').on('click', function (e) {
 e.preventDefault();
 console.warn('Sidebar toggle blocked (page protection active)');
 return false;
 });
 // ASP.NET UpdatePanel / full postback support
 if (typeof Sys !== 'undefined' && Sys.Application) {
 Sys.Application.add_load(function () {
 $bodyEl.removeClass('hide-sidedrawer');
 });
 }
 }
});
