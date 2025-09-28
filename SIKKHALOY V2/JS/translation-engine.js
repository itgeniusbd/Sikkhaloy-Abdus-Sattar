// Bengali Translation Engine for Sikkhaloy Education Management System
// Handles dynamic bilingual menu translation

var isMenuBengali = false;

function initializeLanguageToggle() {
    // Wait for TreeView to fully render
    setTimeout(function() {
        var savedLanguage = localStorage.getItem('menuLanguage');
        if (savedLanguage === 'bengali') {
            isMenuBengali = true;
            translateMenuToBengali();
            $('#languageToggle').html('<i class="fa fa-language"></i> English');
        } else {
            isMenuBengali = false;
            $('#languageToggle').html('<i class="fa fa-language"></i> ?????');
        }
        
        setupTreeViewObserver();
    }, 500);
    
    // Language toggle button click handler
    $('#languageToggle').click(function() {
        var $btn = $(this);
        $btn.html('<i class="fa fa-spinner fa-spin"></i> Processing...').prop('disabled', true);
        
        setTimeout(function() {
            try {
                if (isMenuBengali) {
                    // Switch to English
                    translateMenuToEnglish();
                    $btn.html('<i class="fa fa-language"></i> ?????');
                    isMenuBengali = false;
                    localStorage.setItem('menuLanguage', 'english');
                } else {
                    // Switch to Bengali
                    isMenuBengali = true;
                    localStorage.setItem('menuLanguage', 'bengali');
                    $btn.html('<i class="fa fa-language"></i> English');
                    
                    if ($('#LinkTreeView').is(':visible')) {
                        forceTranslateAll();
                    } else {
                        emergencyReset();
                        setTimeout(forceTranslateAll, 200);
                    }
                }
            } catch (error) {
                console.error('Translation error:', error);
                emergencyReset();
            }
            
            $btn.prop('disabled', false);
        }, 100);
    });
    
    // Double-click emergency reset
    $('#languageToggle').dblclick(function() {
        emergencyReset();
        $(this).html('<i class="fa fa-language"></i> ?????');
        isMenuBengali = false;
        localStorage.setItem('menuLanguage', 'english');
    });
}

function translateMenuToBengali() {
    console.log('Starting Bengali translation...');
    
    // Method 1: Handle anchor tags and spans
    $('#LinkTreeView').find('a, span').each(function() {
        var $element = $(this);
        var text = $element.text().trim();
        
        if (!text || $element.data('translated') === 'yes') return;
        
        if (menuTranslations[text]) {
            $element.data('original-text', text);
            $element.text(menuTranslations[text]);
            $element.data('translated', 'yes');
            console.log('Translated: ' + text + ' -> ' + menuTranslations[text]);
        }
    });
    
    // Method 2: Handle table cells
    $('#LinkTreeView').find('td').each(function() {
        var $element = $(this);
        
        if ($element.data('translated') === 'yes') return;
        
        if ($element.children().length === 0) {
            var text = $element.text().trim();
            
            if (text && menuTranslations[text]) {
                $element.data('original-text', text);
                $element.text(menuTranslations[text]);
                $element.data('translated', 'yes');
                console.log('Translated (TD): ' + text + ' -> ' + menuTranslations[text]);
            }
        }
    });
    
    // Method 3: Handle other elements
    $('#LinkTreeView').find('*').each(function() {
        var $element = $(this);
        
        if ($element.data('translated') === 'yes' || $element.children().length > 0) return;
        
        var text = $element.text().trim();
        
        if (text && menuTranslations[text]) {
            $element.data('original-text', text);
            $element.text(menuTranslations[text]);
            $element.data('translated', 'yes');
            console.log('Translated (Other): ' + text + ' -> ' + menuTranslations[text]);
        }
    });
    
    console.log('Bengali translation completed');
}

function translateMenuToEnglish() {
    console.log('Starting English translation...');
    
    $('#LinkTreeView').find('*').each(function() {
        var $element = $(this);
        var originalText = $element.data('original-text');
        
        if (originalText && $element.data('translated') === 'yes') {
            if ($element.children().length === 0) {
                $element.text(originalText);
            } else {
                $element.contents().filter(function() {
                    return this.nodeType === 3;
                }).each(function() {
                    $(this).replaceWith(originalText);
                });
            }
            
            $element.removeData('translated');
            $element.removeData('original-text');
            console.log('Restored: ' + originalText);
        }
    });
    
    console.log('English translation completed');
}

function forceTranslateAll() {
    if (isMenuBengali) {
        console.log('Force translating to Bengali...');
        
        setTimeout(function() {
            translateMenuToBengali();
        }, 50);
        
        setTimeout(function() {
            translateMenuToBengali();
        }, 200);
        
        setTimeout(function() {
            translateMenuToBengali();
        }, 500);
    } else {
        translateMenuToEnglish();
    }
}

function emergencyReset() {
    console.log('Emergency reset - restoring menu visibility');
    $('#LinkTreeView').find('*').each(function() {
        var $element = $(this);
        $element.show();
        $element.removeData('translated');
        $element.removeData('original-text');
    });
    
    setTimeout(function() {
        $('#LinkTreeView').show();
    }, 100);
}

function setupTreeViewObserver() {
    if (window.MutationObserver) {
        var observer = new MutationObserver(function(mutations) {
            var shouldTranslate = false;
            
            mutations.forEach(function(mutation) {
                if (mutation.type === 'childList' && mutation.addedNodes.length > 0) {
                    for (var i = 0; i < mutation.addedNodes.length; i++) {
                        if (mutation.addedNodes[i].nodeType === 1 || mutation.addedNodes[i].nodeType === 3) {
                            shouldTranslate = true;
                            break;
                        }
                    }
                }
            });
            
            if (shouldTranslate && isMenuBengali) {
                setTimeout(function() {
                    translateMenuToBengali();
                }, 50);
            }
        });
        
        var treeView = document.getElementById('LinkTreeView');
        if (treeView) {
            observer.observe(treeView, {
                childList: true,
                subtree: true,
                characterData: true
            });
        }
    }
}