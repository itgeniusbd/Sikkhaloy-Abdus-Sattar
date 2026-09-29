<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeviceDisplay.aspx.cs" Inherits="EDUCATION.COM.Attendances.Online_Display.DeviceDisplay" %>



<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head runat="server">

    <title>Sikkhaloy - device display</title>

    <meta name="viewport" content="width=device-width, initial-scale=1.0" />

    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css" />

    <link href="/CSS/bootstrap/bootstrap.css" rel="stylesheet" />

    <link href="CSS/device-display.css?v=3.18.1" rel="stylesheet" />

</head>

<body>

    <form id="form1" runat="server">

        <div id="schedule-filter-bar" class="schedule-filter-bar"></div>

        <div class="display-shell">

            <!-- Per-schedule summaries (side column) -->

            <aside class="summary-column">

                <section class="summary-group">

                    <div class="group-header">STUDENT</div>

                    <div class="schedule-cards">

                        <asp:Repeater ID="StudentScheduleSummaryRepeater" runat="server" DataSourceID="StudentScheduleSummarySQL">

                            <ItemTemplate>

                                <div class="schedule-card tone-<%# Container.ItemIndex % 4 %>" data-schedule-id="<%# Eval("ScheduleID") %>">
                                    <div class="schedule-head">
                                        <span class="schedule-title"><%# Eval("ScheduleName") %></span>
                                        <span class="schedule-time"><%# Eval("TimeRange") %></span>
                                        <span class="schedule-users"><i class="fa fa-users"></i> <%# Eval("Total_User") %></span>
                                    </div>
                                    <div class="schedule-stats compact">
                                        <span class="chip">In <b><%# Eval("Current_IN") %></b></span>
                                        <span class="chip">Out <b><%# Eval("Total_Out") %></b></span>
                                        <span class="chip pre">Pre <b><%# Eval("Total_Present") %></b></span>
                                        <span class="chip late">Late <b><%# Eval("Total_Late") %></b></span>
                                        <span class="chip late-abs">L.Abs <b><%# Eval("Total_Late_Absent") %></b></span>
                                        <span class="chip abs">Abs <b><%# Eval("Total_Absent") %></b></span>
                                        <span class="chip leave" title="ছুটি">Leave <b><%# Eval("Total_Leave") %></b></span>
                                    </div>
                                </div>

                            </ItemTemplate>

                        </asp:Repeater>

                        <%-- Live SelectCommand is assigned in DeviceDisplay.aspx.cs --%>
                        <asp:SqlDataSource ID="StudentScheduleSummarySQL" runat="server"

                            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                            OnSelecting="DisplaySql_Selecting" OnSelected="DisplaySql_Selected"

                            SelectCommand="SELECT 1 AS ok WHERE 1 = 0">
                            <SelectParameters>

                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                <asp:Parameter Name="EducationYearID" Type="Int32" DefaultValue="0" />

                            </SelectParameters>

                        </asp:SqlDataSource>

                    </div>

                </section>



                <section class="summary-group">

                    <div class="group-header employee">TEACHER / STAFF</div>

                    <div class="schedule-cards">

                        <asp:Repeater ID="EmployeeScheduleSummaryRepeater" runat="server" DataSourceID="EmployeeScheduleSummarySQL">

                            <ItemTemplate>

                                <div class="schedule-card tone-<%# Container.ItemIndex % 4 %>" data-schedule-id="<%# Eval("ScheduleID") %>">
                                    <div class="schedule-head">
                                        <span class="schedule-title"><%# Eval("ScheduleName") %></span>
                                        <span class="schedule-time"><%# Eval("TimeRange") %></span>
                                        <span class="schedule-users"><i class="fa fa-users"></i> <%# Eval("Total_User") %></span>
                                    </div>
                                    <div class="schedule-stats compact">
                                        <span class="chip">In <b><%# Eval("Current_IN") %></b></span>
                                        <span class="chip">Out <b><%# Eval("Total_Out") %></b></span>
                                        <span class="chip pre">Pre <b><%# Eval("Total_Present") %></b></span>
                                        <span class="chip late">Late <b><%# Eval("Total_Late") %></b></span>
                                        <span class="chip late-abs">L.Abs <b><%# Eval("Total_Late_Absent") %></b></span>
                                        <span class="chip abs">Abs <b><%# Eval("Total_Absent") %></b></span>
                                        <span class="chip leave" title="ছুটি">Leave <b><%# Eval("Total_Leave") %></b></span>
                                    </div>
                                </div>

                            </ItemTemplate>

                        </asp:Repeater>

                        <asp:SqlDataSource ID="EmployeeScheduleSummarySQL" runat="server"

                            ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                            OnSelecting="DisplaySql_Selecting" OnSelected="DisplaySql_Selected"

                            SelectCommand="SELECT 1 AS ok WHERE 1 = 0">
                            <SelectParameters>

                                <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />

                            </SelectParameters>

                        </asp:SqlDataSource>

                    </div>

                </section>

            </aside>



            <!-- Attendance logs -->

            <main class="logs-column">

                <section class="log-section">

                    <div class="section-bar">STUDENT ATTENDANCE</div>

                    <div class="log-body">

                        <div class="slide-in str_wrap">

                            <asp:Repeater ID="StudentEntryLog" runat="server" DataSourceID="Student_Entry_LogSQL">

                                <ItemTemplate>

                                    <div class="info-block" data-schedule-id="<%# Eval("ScheduleID") %>">

                                        <div class="card">

                                            <span class="schedule-badge"><%# Eval("ScheduleName") %></span>

                                            <div class="name-title">

                                                <i class="fa fa-user-o" aria-hidden="true"></i>

                                                <%# Eval("StudentsName") %>

                                            </div>

                                            <img class="card-img-top" src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%# Eval("StudentID") %>&amp;cache=1" alt="" />

                                            <span class="notify-badge z-depth-2 <%# Eval("Attendance") %>"><%# Eval("Attendance") %></span>

                                            <div class="entry-date">

                                                <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                <%# Eval("EntryTime") %>

                                            </div>

                                        </div>

                                    </div>

                                </ItemTemplate>

                            </asp:Repeater>

                            <asp:SqlDataSource ID="Student_Entry_LogSQL" runat="server"

                                ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                                OnSelecting="DisplaySql_Selecting" OnSelected="DisplaySql_Selected"

                                SelectCommand="SELECT 1 AS ok WHERE 1 = 0">
                                <SelectParameters>

                                    <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                    <asp:Parameter Name="EducationYearID" Type="Int32" DefaultValue="0" />

                                </SelectParameters>

                            </asp:SqlDataSource>

                        </div>



                        <div class="slide-out str_wrap">

                            <asp:Repeater ID="StudentExitLog" runat="server" DataSourceID="Student_Exit_LogSQL">

                                <ItemTemplate>

                                    <div class="info-block" data-schedule-id="<%# Eval("ScheduleID") %>">

                                        <div class="card">

                                            <span class="schedule-badge"><%# Eval("ScheduleName") %></span>

                                            <div class="name-title">

                                                <i class="fa fa-user-o" aria-hidden="true"></i>

                                                <%# Eval("StudentsName") %>

                                            </div>

                                            <img class="card-img-top" src="/Handeler/Student_Id_Based_Photo.ashx?StudentID=<%# Eval("StudentID") %>&amp;cache=1" alt="" />

                                            <span class="notify-badge z-depth-2 <%# Eval("Attendance") %>"><%# Eval("Attendance") %></span>

                                            <div class="entry-date">

                                                <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                <%# Eval("EntryTime") %>

                                            </div>

                                            <div class="exit-date">

                                                <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                <%# Eval("ExitTime") %>

                                            </div>

                                        </div>

                                    </div>

                                </ItemTemplate>

                            </asp:Repeater>

                            <asp:SqlDataSource ID="Student_Exit_LogSQL" runat="server"

                                ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                                OnSelecting="DisplaySql_Selecting" OnSelected="DisplaySql_Selected"

                                SelectCommand="SELECT 1 AS ok WHERE 1 = 0">
                                <SelectParameters>

                                    <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />
                                    <asp:Parameter Name="EducationYearID" Type="Int32" DefaultValue="0" />

                                </SelectParameters>

                            </asp:SqlDataSource>

                        </div>

                    </div>

                </section>



                <section class="log-section">

                    <div class="section-bar employee">TEACHER / STAFF ATTENDANCE</div>

                    <div class="log-body">

                        <div class="slide-in str_wrap">

                            <asp:Repeater ID="EmployeeEntryLog" runat="server" DataSourceID="EmployeeEntryLogSQL">

                                <ItemTemplate>

                                    <div class="info-block" data-schedule-id="<%# Eval("ScheduleID") %>">

                                        <div class="card">

                                            <span class="schedule-badge"><%# Eval("ScheduleName") %></span>

                                            <div class="name-title">

                                                <i class="fa fa-user-o" aria-hidden="true"></i>

                                                <%# Eval("Name") %>

                                            </div>

                                            <img class="card-img-top" src="/Handeler/Employee_Image.ashx?Img=<%# Eval("EmployeeID") %>&amp;cache=1" alt="" />

                                            <span class="notify-badge z-depth-2 <%# Eval("AttendanceStatus") %>"><%# Eval("AttendanceStatus") %></span>

                                            <div class="entry-date">

                                                <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                <%# Eval("EntryTime") %>

                                            </div>

                                        </div>

                                    </div>

                                </ItemTemplate>

                            </asp:Repeater>

                            <asp:SqlDataSource ID="EmployeeEntryLogSQL" runat="server"

                                ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                                OnSelecting="DisplaySql_Selecting" OnSelected="DisplaySql_Selected"

                                SelectCommand="SELECT 1 AS ok WHERE 1 = 0">
                                <SelectParameters>

                                    <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />

                                </SelectParameters>

                            </asp:SqlDataSource>

                        </div>



                        <div class="slide-out str_wrap">

                            <asp:Repeater ID="EmployeeExitLog" runat="server" DataSourceID="EmployeeExitLogSQL">

                                <ItemTemplate>

                                    <div class="info-block" data-schedule-id="<%# Eval("ScheduleID") %>">

                                        <div class="card">

                                            <span class="schedule-badge"><%# Eval("ScheduleName") %></span>

                                            <div class="name-title">

                                                <i class="fa fa-user-o" aria-hidden="true"></i>

                                                <%# Eval("Name") %>

                                            </div>

                                            <img class="card-img-top" src="/Handeler/Employee_Image.ashx?Img=<%# Eval("EmployeeID") %>&amp;cache=1" alt="" />

                                            <span class="notify-badge z-depth-2 <%# Eval("AttendanceStatus") %>"><%# Eval("AttendanceStatus") %></span>

                                            <div class="entry-date">

                                                <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                <%# Eval("EntryTime") %>

                                            </div>

                                            <div class="exit-date">

                                                <i class="fa fa-clock-o" aria-hidden="true"></i>

                                                <%# Eval("ExitTime") %>

                                            </div>

                                        </div>

                                    </div>

                                </ItemTemplate>

                            </asp:Repeater>

                            <asp:SqlDataSource ID="EmployeeExitLogSQL" runat="server"

                                ConnectionString="<%$ ConnectionStrings:EducationConnectionString %>"

                                OnSelecting="DisplaySql_Selecting" OnSelected="DisplaySql_Selected"

                                SelectCommand="SELECT 1 AS ok WHERE 1 = 0">
                                <SelectParameters>

                                    <asp:QueryStringParameter Name="SchoolID" QueryStringField="SchoolID" />

                                </SelectParameters>

                            </asp:SqlDataSource>

                        </div>

                    </div>

                </section>

            </main>

        </div>

    </form>



    <script type="text/javascript" src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>

    <script src="js/jquery.liMarquee.js"></script>

    <script>

        (function () {
            var isEmbed = /(?:^|[?&])embed=1(?:&|$)/.test(window.location.search);
            var isLowPower = /(?:^|[?&])lowPower=1(?:&|$)/.test(window.location.search);
            var scrollMatch = window.location.search.match(/[?&]scroll=(\d+)/i);
            var delayMatch = window.location.search.match(/[?&]delay=(\d+)/i);
            var marqueeScroll = scrollMatch ? parseInt(scrollMatch[1], 10) : (isLowPower ? 8 : 18);
            var marqueeDelay = delayMatch ? parseInt(delayMatch[1], 10) : (isLowPower ? 100 : 40);
            if (!marqueeScroll || marqueeScroll < 4) marqueeScroll = isLowPower ? 8 : 18;
            if (isNaN(marqueeDelay) || marqueeDelay < 0) marqueeDelay = isLowPower ? 100 : 40;
            var refreshBusy = false;
            var refreshQueued = false;
            if (isEmbed) {
                document.body.classList.add('embed-mode');
            }
            if (isLowPower) {
                document.body.classList.add('low-power-mode');
            }

            function scheduleFilterStorageKey() {
                var match = window.location.search.match(/[?&]SchoolID=(\d+)/i);
                return 'sikkhaloyDisplayScheduleFilter_' + (match ? match[1] : '0');
            }

            window.getDisplaySchedules = function () {
                var map = {};
                $('.summary-column .schedule-card[data-schedule-id]').each(function () {
                    var id = String($(this).attr('data-schedule-id') || '');
                    if (!id || id === '0') return;
                    var name = $.trim($(this).find('.schedule-title').first().text());
                    if (!name) name = 'Schedule ' + id;
                    if (!map[id]) map[id] = { id: parseInt(id, 10), name: name };
                });
                return Object.keys(map).map(function (k) { return map[k]; })
                    .sort(function (a, b) { return a.id - b.id; });
            };

            window.getScheduleFilterActiveIds = function () {
                var raw = localStorage.getItem(scheduleFilterStorageKey());
                if (!raw) return null;
                try {
                    var parsed = JSON.parse(raw);
                    return Array.isArray(parsed) ? parsed : null;
                } catch (e) {
                    return null;
                }
            };

            window.setScheduleFilter = function (activeIds) {
                if (!Array.isArray(activeIds)) activeIds = [];
                if (!isEmbed) {
                    localStorage.setItem(scheduleFilterStorageKey(), JSON.stringify(activeIds));
                }
                applyScheduleFilter(activeIds);
                clearTimeout(window.__marqueeRefreshTimer);
                window.__marqueeRefreshTimer = setTimeout(function () {
                    initMarquee();
                    scheduleFitEmbedLayout();
                }, 400);
            };

            function applyScheduleFilter(activeIds) {
                if (!Array.isArray(activeIds) || !activeIds.length) {
                    $('.summary-column .schedule-card[data-schedule-id], .info-block[data-schedule-id]')
                        .removeClass('filter-hidden');
                    resizeScheduleCards();
                    return;
                }

                var activeSet = {};
                activeIds.forEach(function (id) { activeSet[String(id)] = true; });

                $('.summary-column .schedule-card[data-schedule-id], .info-block[data-schedule-id]').each(function () {
                    var id = String($(this).attr('data-schedule-id') || '');
                    var visible = !!activeSet[id];
                    $(this).toggleClass('filter-hidden', !visible);
                });

                resizeScheduleCards();
            }

            function destroyAllMarquees() {
                $('.slide-in, .slide-out').each(function () {
                    var $el = $(this);
                    while ($el.find('.str_move').length) {
                        $el.liMarquee('destroy');
                    }
                });
            }

            function centerMarqueeSlides() {
                $('.slide-in, .slide-out').each(function () {
                    var $slide = $(this);
                    var slideH = $slide.innerHeight();
                    if (!slideH) return;

                    $slide.find('.str_wrap').css('height', slideH + 'px');

                    $slide.find('.str_move, .str_move_clone').each(function () {
                        var $move = $(this);
                        var moveH = $move.outerHeight();
                        $move.css('top', Math.max(0, (slideH - moveH) / 2) + 'px');
                    });
                });
            }

            function fitEmbedSliderLayout() {
                if (!$('body').hasClass('embed-mode'))
                    return;

                var shellH = $('.display-shell').innerHeight();
                if (!shellH)
                    return;

                if (shellH < 520) {
                    $('body').addClass('embed-compact');
                } else {
                    $('body').removeClass('embed-compact');
                }

                var sectionCount = Math.max(1, $('.logs-column .log-section').length);
                var sectionBarH = $('.logs-column .section-bar').first().outerHeight(true) || 24;
                var sectionGap = 6;
                var logBodyPad = $('body').hasClass('embed-compact') ? 4 : 6;
                var slideGap = $('body').hasClass('embed-compact') ? 2 : 3;
                var sectionH = (shellH - sectionGap * (sectionCount - 1)) / sectionCount;
                var fallbackSlideH = Math.max(32, (sectionH - sectionBarH - logBodyPad * 2 - slideGap) / 2);

                $('.slide-in, .slide-out').each(function () {
                    var $slide = $(this);
                    var slideH = $slide.innerHeight() || fallbackSlideH;
                    var cardH = Math.max(32, Math.floor(slideH - 4));
                    var cardW = Math.max(30, Math.round(cardH * 0.88));
                    $slide[0].style.setProperty('--slide-card-h', cardH + 'px');
                    $slide[0].style.setProperty('--slide-card-w', cardW + 'px');
                });

                centerMarqueeSlides();
            }

            var fitLayoutTimer = null;
            function scheduleFitEmbedLayout() {
                clearTimeout(fitLayoutTimer);
                fitLayoutTimer = setTimeout(function () {
                    fitEmbedSliderLayout();
                }, 250);
            }

            function initMarquee() {
                destroyAllMarquees();

                var marqueeOptionsIn = {
                    direction: 'left',
                    loop: -1,
                    scrolldelay: marqueeDelay,
                    scrollamount: marqueeScroll,
                    circular: true,
                    hoverStop: false,
                    drag: false
                };

                var marqueeOptionsOut = {
                    direction: 'right',
                    loop: -1,
                    scrolldelay: marqueeDelay,
                    scrollamount: marqueeScroll,
                    circular: true,
                    hoverStop: false,
                    drag: false
                };

                $('.slide-in').liMarquee(marqueeOptionsIn);
                $('.slide-out').liMarquee(marqueeOptionsOut);

                setTimeout(function () {
                    fitEmbedSliderLayout();
                    centerMarqueeSlides();
                }, 250);
            }

            function initScheduleFilterUI() {
                if ($('body').hasClass('embed-mode')) {
                    // Embed display: WPF host owns schedule filter state.
                    return;
                }

                var schedules = window.getDisplaySchedules();
                if (!schedules.length) return;

                var stored = window.getScheduleFilterActiveIds();
                var activeSet = stored ? {} : null;
                if (stored) {
                    stored.forEach(function (id) { activeSet[String(id)] = true; });
                }

                var $bar = $('#schedule-filter-bar');
                $bar.empty();
                schedules.forEach(function (s) {
                    var checked = !activeSet || activeSet[String(s.id)];
                    var $label = $('<label class="schedule-filter-item"></label>');
                    var $cb = $('<input type="checkbox" />')
                        .attr('data-schedule-id', s.id)
                        .prop('checked', checked);
                    $label.append($cb).append(document.createTextNode(' ' + s.name));
                    $bar.append($label);
                });

                $bar.off('change.scheduleFilter').on('change.scheduleFilter', 'input', function () {
                    var ids = [];
                    $bar.find('input:checked').each(function () {
                        ids.push(parseInt($(this).attr('data-schedule-id'), 10));
                    });
                    window.setScheduleFilter(ids);
                });

                if (stored) {
                    applyScheduleFilter(stored);
                } else {
                    applyScheduleFilter(schedules.map(function (s) { return s.id; }));
                }
            }

            window.resizeScheduleCards = resizeScheduleCards;
            window.initMarquee = initMarquee;
            window.initScheduleFilterUI = initScheduleFilterUI;
            window.fitEmbedSliderLayout = fitEmbedSliderLayout;
            window.scheduleFitEmbedLayout = scheduleFitEmbedLayout;

            window.setLowPowerMode = function (enabled) {
                isLowPower = !!enabled;
                document.body.toggleClass('low-power-mode', isLowPower);
                marqueeScroll = isLowPower ? 8 : 18;
                marqueeDelay = isLowPower ? 100 : 40;
                initMarquee();
            };

            window.requestEmbedDisplayRefresh = function () {
                if (!isEmbed || refreshBusy) {
                    refreshQueued = true;
                    return;
                }

                refreshBusy = true;
                refreshQueued = false;

                var url = window.location.href.split('#')[0];

                fetch(url, { cache: 'no-store', credentials: 'same-origin' })
                    .then(function (response) { return response.text(); })
                    .then(function (html) {
                        var parser = new DOMParser();
                        var doc = parser.parseFromString(html, 'text/html');
                        var newShell = doc.querySelector('.display-shell');
                        var oldShell = document.querySelector('.display-shell');
                        if (!newShell || !oldShell) return;

                        oldShell.innerHTML = newShell.innerHTML;
                        initScheduleFilterUI();
                        var schedules = window.getDisplaySchedules();
                        $('.info-block[data-schedule-id]').each(function () {
                            var id = parseInt($(this).attr('data-schedule-id'), 10);
                            if (id && !schedules.some(function (s) { return s.id === id; })) {
                                schedules.push({ id: id, name: 'Schedule ' + id });
                            }
                        });
                        if (schedules.length) {
                            applyScheduleFilter(schedules.map(function (s) { return s.id; }));
                        }
                        initMarquee();
                        scheduleFitEmbedLayout();
                    })
                    .catch(function () { })
                    .then(function () {
                        refreshBusy = false;
                        if (refreshQueued) {
                            refreshQueued = false;
                            window.requestEmbedDisplayRefresh();
                        }
                    });
            };

            window.refreshEmbedDisplayData = window.requestEmbedDisplayRefresh;

            function resizeScheduleCards() {
                $('.summary-column .schedule-cards').each(function () {
                    var $wrap = $(this);
                    var count = $wrap.children('.schedule-card:not(.filter-hidden)').length;
                    $wrap.removeClass('sched-count-1 sched-count-2 sched-count-3 sched-count-many');
                    if (count === 1) $wrap.addClass('sched-count-1');
                    else if (count === 2) $wrap.addClass('sched-count-2');
                    else if (count === 3) $wrap.addClass('sched-count-3');
                    else if (count > 3) $wrap.addClass('sched-count-many');
                });
            }

            $(function () {
                initScheduleFilterUI();
                if (isEmbed) {
                    var schedules = window.getDisplaySchedules();
                    if (schedules.length) {
                        applyScheduleFilter(schedules.map(function (s) { return s.id; }));
                    }
                }
                initMarquee();
                scheduleFitEmbedLayout();
                $(window).on('resize.fitEmbedLayout', scheduleFitEmbedLayout);
                if (window.ResizeObserver) {
                    var shellEl = document.querySelector('.display-shell');
                    if (shellEl) {
                        new ResizeObserver(scheduleFitEmbedLayout).observe(shellEl);
                    }
                }
                document.addEventListener('visibilitychange', function () {
                    if (document.hidden) {
                        destroyAllMarquees();
                    } else {
                        initMarquee();
                    }
                });
                if (!isEmbed) {
                    setInterval(function () {
                        window.location.reload();
                    }, 120000);
                }
            });
        })();

    </script>

</body>

</html>

