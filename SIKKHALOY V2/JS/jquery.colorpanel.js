(function ($) {
    $.fn.ColorPanel = function (options) {
        var settings = $.extend({
            styleSheet: '#DefaultCSS',
            colors: {},
            linkClass: 'linka',
            animateContainer: false
        }, options);
        var panelDiv = this;

        $('#cpToggle').click(function (e) {
            e.preventDefault();
            $('ul', panelDiv).slideToggle();
        });

        var colors = settings.colors || null;
        if (colors) {
            $.each(colors, function (key, value) {
                var li = $("<li/>");
                var e = $("<a />", { href: value, "class": settings.linkClass, }).css('background-color', key);  // you need to quote "class" since it's a reserved keyword
                li.append(e);
                $(panelDiv).find('ul').append(li);
            });

            $('ul', panelDiv).on('click', 'a', function (e) {
                e.preventDefault();
                var CssFile = $(this).attr('href') || 'Custom.css';
                if (settings.animateContainer) {
                    $(settings.animateContainer).fadeOut(function () {
                        $(settings.styleSheet).attr('href', CssFile);
                        // And then:
                        $(this).fadeIn();
                    });
                }
                else {
                    $(settings.styleSheet).attr('href', CssFile);
                }
            });
        }
    };
}(jQuery));