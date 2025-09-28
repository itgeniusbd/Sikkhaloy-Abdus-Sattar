// Dashboard Chart and Data Management
$(function () {
    // Initialize table total calculation
    $('#table_total').tableTotal();
    $('#SGT').text($('#table_total tr:last td:last').html());

    // Session Based Student Chart
    loadSessionChart();
    
    // Gender Chart
    loadGenderChart();
    
    // SMS Chart
    loadSMSChart();
    
    // Employee Chart
    loadEmployeeChart();
});

// Session Based Student Chart
function loadSessionChart() {
    $.ajax({
        type: "POST",
        url: "Admin.aspx/Get_Session_Student",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            var Ch_data = r.d;
            if (Ch_data[0].length > 1) {
                $('#SessionBs').show();
                var ctx = document.getElementById("myChart").getContext('2d');

                var gradientStroke = ctx.createLinearGradient(500, 0, 100, 0);
                gradientStroke.addColorStop(0, 'rgba(136, 14, 79, .5)');
                gradientStroke.addColorStop(1, 'rgba(49, 27, 146, .6)');

                var gradientFill = ctx.createLinearGradient(600, 0, 100, 0);
                gradientFill.addColorStop(0, "rgba(136, 14, 79, .5)");
                gradientFill.addColorStop(1, "rgba(49, 27, 146, .6)");

                var myChart = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: Ch_data[0],
                        datasets: [{
                            label: "Session Based Student",
                            data: Ch_data[1],
                            fill: true,
                            backgroundColor: gradientFill,
                            borderWidth: 2,
                            borderColor: gradientStroke,
                            pointBorderColor: gradientStroke,
                            pointBackgroundColor: gradientStroke,
                            pointHoverBackgroundColor: gradientStroke,
                            pointHoverBorderColor: gradientStroke,
                        }]
                    },
                    options: {
                        legend: {
                            position: "bottom"
                        },
                    }
                });
            }
        },
        failure: function (r) {
            console.log('Session chart error:', r.d);
        },
        error: function (r) {
            console.log('Session chart error:', r.d);
        }
    });
}

// Gender Chart
function loadGenderChart() {
    $.ajax({
        type: "POST",
        url: "Admin.aspx/Get_Gender",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            var Ch_data = r.d;
            if (Ch_data[0].length > 0) {
                var ctx = document.getElementById("GenderChart").getContext('2d');
                var myChart = new Chart(ctx, {
                    type: 'pie',
                    data: {
                        labels: Ch_data[0],
                        datasets: [{
                            data: Ch_data[1],
                            backgroundColor: ["#26a69a", "#00e5ff"],
                        }]
                    },
                    options: {
                        responsive: true
                    }
                });
            }
        },
        failure: function (r) {
            console.log('Gender chart error:', r.d);
        },
        error: function (r) {
            console.log('Gender chart error:', r.d);
        }
    });
}

// SMS Chart
function loadSMSChart() {
    $.ajax({
        type: "POST",
        url: "Admin.aspx/Get_SentSMS",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            var Ch_data = r.d;
            if (Ch_data[0].length > 0) {
                var ctxD = document.getElementById("doughnutChart").getContext('2d');
                var myLineChart = new Chart(ctxD, {
                    type: 'doughnut',
                    data: {
                        labels: Ch_data[0],
                        datasets: [{
                            data: Ch_data[1],
                            backgroundColor: [
                                '#4bc0c0',
                                '#36a2eb',
                                '#ffcd56',
                                '#69f0ae',
                                '#ff6384',
                                '#ff9f40',
                                'rgba(128,100,161,1)',
                                'rgba(74,172,197,1)',
                                'rgba(247,150,71,1)',
                                'rgba(127,96,132,1)',
                                'rgba(119,160,51,1)',
                                'rgba(51,85,139,1)'
                            ],
                        }]
                    },
                    options: {
                        responsive: true
                    }
                });
            }
        },
        failure: function (r) {
            console.log('SMS chart error:', r.d);
        },
        error: function (r) {
            console.log('SMS chart error:', r.d);
        }
    });
}

// Employee Chart
function loadEmployeeChart() {
    $.ajax({
        type: "POST",
        url: "Admin.aspx/Get_Employee",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (r) {
            var Ch_data = r.d;

            if (Ch_data[0].length > 0) {
                var total = 0;
                total = Ch_data[1].reduce(function (a, b) {
                    return parseInt(a, 10) + parseInt(b, 10);
                });
                $("#TEmp").text("Total " + total);

                var ctxD = document.getElementById("EmployeeChart").getContext('2d');
                var myLineChart = new Chart(ctxD, {
                    type: 'pie',
                    data: {
                        labels: Ch_data[0],
                        datasets: [{
                            data: Ch_data[1],
                            backgroundColor: ['#9575cd', '#26c6da', '#d81b60', '#64b5f6', '#69f0ae'],
                        }]
                    },
                    options: {
                        responsive: true
                    }
                });
            }
        },
        failure: function (r) {
            console.log('Employee chart error:', r.d);
        },
        error: function (r) {
            console.log('Employee chart error:', r.d);
        }
    });
}

// Chart Plugin for Data Labels
Chart.plugins.register({
    afterDatasetsDraw: function (chart) {
        var ctx = chart.ctx;

        chart.data.datasets.forEach(function (dataset, i) {
            var meta = chart.getDatasetMeta(i);
            if (!meta.hidden) {
                meta.data.forEach(function (element, index) {
                    // Draw the text in black, with the specified font
                    ctx.fillStyle = '#000';

                    var fontSize = 11;
                    var fontStyle = 'normal';
                    var fontFamily = 'tahoma';
                    ctx.font = Chart.helpers.fontString(fontSize, fontStyle, fontFamily);

                    // Just naively convert to string for now
                    var dataString = dataset.data[index].toString();

                    // Make sure alignment settings are correct
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';

                    var padding = 3;
                    var position = element.tooltipPosition();
                    ctx.fillText(dataString, position.x, position.y - (fontSize / 2) - padding);
                });
            }
        });
    }
});

// Utility Functions
function refreshCharts() {
    loadSessionChart();
    loadGenderChart();
    loadSMSChart();
    loadEmployeeChart();
}

// Export functions for external use if needed
window.DashboardCharts = {
    refresh: refreshCharts,
    loadSession: loadSessionChart,
    loadGender: loadGenderChart,
    loadSMS: loadSMSChart,
    loadEmployee: loadEmployeeChart
};