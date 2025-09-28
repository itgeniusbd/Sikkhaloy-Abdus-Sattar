// Bengali Translation System for Sikkhaloy Education Management System
// Complete bilingual support for menu navigation

$(function () {
    // Initialize nice scroll for sidebar
    $("#sidedrawer").niceScroll({
        cursorcolor: "#394C3A",
        cursorwidth: "7px",
        cursorborder: "1px solid #394C3A",
        cursorborderradius: "3px",
        emulatetouch: true
    });
    
    $("#sidedrawer").mouseover(function () {
        $("#sidedrawer").getNiceScroll().resize();
    });
    
    // Initialize animations
    new WOW().init();
    
    // Add scroll to top button
    $('body').append('<div id="toTop" class="btn btn-info d-print-none"><span class="glyphicon glyphicon-chevron-up"></span> TOP</div>');
    
    $(window).scroll(function () {
        if ($(this).scrollTop() !== 0) {
            $('#toTop').fadeIn();
        } else {
            $('#toTop').fadeOut();
        }
    });
    
    $('#toTop').click(function () {
        $("html, body").animate({ scrollTop: 0 }, 600);
        return false;
    });
    
    // Set current year
    $("#CurrentYear").text((new Date).getFullYear());
    
    // Remove skip links
    $('a[href$=_SkipLink]').each(function () {
        $(this).remove();
    });
    
    // Initialize user session
    storeUser();
    
    // Initialize language translation system
    initializeLanguageToggle();
    
    // Handle TreeView expansion/collapse events
    $('#LinkTreeView').on('click', 'a, img, span', function() {
        if (isMenuBengali) {
            setTimeout(function() {
                var visibleElements = $('#LinkTreeView').find('*').filter(':visible');
                if (visibleElements.length > 0) {
                    translateMenuToBengali();
                }
            }, 100);
            
            setTimeout(function() {
                if (isMenuBengali) {
                    translateMenuToBengali();
                }
            }, 500);
        }
    });
    
    // Periodic translation check
    if (typeof window.translationInterval !== 'undefined') {
        clearInterval(window.translationInterval);
    }
    
    window.translationInterval = setInterval(function() {
        if (isMenuBengali) {
            var hasUntranslated = false;
            var untranslatedItems = [];
            
            $('#LinkTreeView').find('a, span, td').filter(':visible').each(function() {
                var text = $(this).text().trim();
                if (text && menuTranslations[text] && !$(this).data('translated')) {
                    hasUntranslated = true;
                    untranslatedItems.push(text);
                }
            });
            
            if (hasUntranslated && untranslatedItems.length < 10) {
                console.log('Found untranslated items:', untranslatedItems);
                translateMenuToBengali();
            }
        }
    }, 2000);
    
    // Quick initial check
    var quickCheckCount = 0;
    var quickInterval = setInterval(function() {
        quickCheckCount++;
        if (quickCheckCount >= 5 || !isMenuBengali) {
            clearInterval(quickInterval);
            return;
        }
        
        if (isMenuBengali) {
            var visibleUntranslated = $('#LinkTreeView').find('a, span').filter(':visible').filter(function() {
                var text = $(this).text().trim();
                return text && menuTranslations[text] && !$(this).data('translated');
            });
            
            if (visibleUntranslated.length > 0 && visibleUntranslated.length < 5) {
                translateMenuToBengali();
            }
        }
    }, 1000);
});

// Session Management Functions
$("#content-wrapper").mouseover(function () {
    checkUser();

    if (localStorage._sid) {
        const previousId = $(".edu-session-year").val();
        const currentId = localStorage._sid;

        if (previousId !== currentId) {
            localStorage._sid = previousId;
            location.reload(true);
        }
    }
});

$(".edu-session-year").change(function () {
    var id = $(this).val();
    if (!id) return;

    const data = { id: id };
    $.ajax({
        type: "POST",
        url: '/Default.aspx/Session_Change',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(data),
        success: function (r) {
            localStorage._sid = id;
            location.reload();
        },
        error: function (e) {
            console.log(`there was an error!${e.d}`);
        }
    });
});

function checkUser() {
    if (localStorage._regId) {
        const previousId = localStorage._regId;
        const currentId = $("[id*=_redIdHidden]").val();

        if (previousId !== currentId) {
            localStorage._regId = currentId;
            location.reload(true);
        }
    }
}

function storeUser() {
    const currentUser = $("[id*=_redIdHidden]").val();
    localStorage._regId = currentUser;
}