// Simple Bengali Translation System for Sikkhaloy
// Minimal and lightweight approach

$(document).ready(function() {
    // Simple translation dictionary
    var translations = {
        'Basic Settings': '???????? ??????',
        'Teacher & Staff': '?????? ? ????????',
        'Admission': '?????',
        'Student Management': '????? ???????????',
        'Attendances': '????????',
        'Accounts': '?????',
        'SMS': '??????',
        'Routines': '?????',
        'Committee': '?????',
        'School Info': '??????? ????',
        'Create Class': '????? ????',
        'Create Subject': '????? ????',
        'Student Info': '??????? ????',
        'ID Card': '???? ?????',
        'Payment Report': '??????? ???????',
        'Create Exam': '??????? ????',
        'Class Routine': '????? ?????'
    };
    
    var isBengali = localStorage.getItem('lang') === 'bn';
    
    // Simple translate function
    function translate() {
        $('#LinkTreeView a, #LinkTreeView span, #LinkTreeView td').each(function() {
            var text = $(this).text().trim();
            if (translations[text]) {
                if (!$(this).data('original')) {
                    $(this).data('original', text);
                }
                $(this).text(isBengali ? translations[text] : $(this).data('original'));
            }
        });
    }
    
    // Language toggle
    $('#languageToggle').click(function() {
        isBengali = !isBengali;
        localStorage.setItem('lang', isBengali ? 'bn' : 'en');
        $(this).html(isBengali ? '<i class="fa fa-language"></i> English' : '<i class="fa fa-language"></i> ?????');
        translate();
    });
    
    // Initialize
    setTimeout(translate, 500);
    
    // Re-translate on menu expand
    $('#LinkTreeView').on('click', function() {
        setTimeout(translate, 100);
    });
});