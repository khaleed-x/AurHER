
(function () {
    'use strict';

    const COLORS = {
        primary: '#00BFFF',
        primaryDark: '#0099CC',
        primaryLight: 'rgba(0, 191, 255, 0.12)',
        success: '#10B981',
        successLight: 'rgba(16, 185, 129, 0.12)',
        warning: '#F59E0B',
        danger: '#EF4444',
        purple: '#6366F1',
        border: '#E2E8F0',
        text: '#94A3B8',
        textDark: '#0F172A',
    };

    // ── Utility: get canvas context ──
    function getCtx(id) {
        var canvas = document.getElementById(id);
        if (!canvas) return null;
        canvas.width = canvas.offsetWidth * window.devicePixelRatio;
        canvas.height = canvas.offsetHeight * window.devicePixelRatio;
        var ctx = canvas.getContext('2d');
        ctx.scale(window.devicePixelRatio, window.devicePixelRatio);
        return { ctx: ctx, w: canvas.offsetWidth, h: canvas.offsetHeight };
    }

    // ── Line Chart (Revenue) ──
    function drawLineChart(id, labels, datasets) {
        var c = getCtx(id);
        if (!c) return;
        var ctx = c.ctx, W = c.w, H = c.h;
        var padL = 52, padR = 24, padT = 20, padB = 44;
        var chartW = W - padL - padR;
        var chartH = H - padT - padB;

        ctx.clearRect(0, 0, W, H);

        // Get all values
        var allVals = [];
        datasets.forEach(function (ds) { allVals = allVals.concat(ds.data); });
        var maxVal = Math.max.apply(null, allVals) * 1.15;
        var minVal = 0;

        // Grid lines
        var gridCount = 5;
        ctx.strokeStyle = COLORS.border;
        ctx.lineWidth = 1;
        for (var i = 0; i <= gridCount; i++) {
            var y = padT + (chartH / gridCount) * i;
            ctx.beginPath();
            ctx.moveTo(padL, y);
            ctx.lineTo(padL + chartW, y);
            ctx.stroke();

            // Y labels
            var val = maxVal - (maxVal / gridCount) * i;
            ctx.fillStyle = COLORS.text;
            ctx.font = '11px DM Sans, sans-serif';
            ctx.textAlign = 'right';
            ctx.fillText(formatK(val), padL - 6, y + 4);
        }

        // X labels
        var step = chartW / (labels.length - 1);
        labels.forEach(function (label, i) {
            var x = padL + step * i;
            ctx.fillStyle = COLORS.text;
            ctx.font = '11px DM Sans, sans-serif';
            ctx.textAlign = 'center';
            ctx.fillText(label, x, H - padB + 18);
        });

        // Draw datasets
        datasets.forEach(function (ds) {
            var points = ds.data.map(function (val, i) {
                return {
                    x: padL + step * i,
                    y: padT + chartH - ((val - minVal) / (maxVal - minVal)) * chartH
                };
            });

            // Fill area
            ctx.beginPath();
            ctx.moveTo(points[0].x, points[0].y);
            for (var i = 1; i < points.length; i++) {
                var cp1x = points[i - 1].x + step * 0.4;
                var cp2x = points[i].x - step * 0.4;
                ctx.bezierCurveTo(cp1x, points[i - 1].y, cp2x, points[i].y, points[i].x, points[i].y);
            }
            ctx.lineTo(points[points.length - 1].x, padT + chartH);
            ctx.lineTo(points[0].x, padT + chartH);
            ctx.closePath();
            ctx.fillStyle = ds.fillColor;
            ctx.fill();

            // Line
            ctx.beginPath();
            ctx.moveTo(points[0].x, points[0].y);
            for (var j = 1; j < points.length; j++) {
                var c1x = points[j - 1].x + step * 0.4;
                var c2x = points[j].x - step * 0.4;
                ctx.bezierCurveTo(c1x, points[j - 1].y, c2x, points[j].y, points[j].x, points[j].y);
            }
            ctx.strokeStyle = ds.color;
            ctx.lineWidth = 2.5;
            ctx.stroke();

            // Dots
            points.forEach(function (pt) {
                ctx.beginPath();
                ctx.arc(pt.x, pt.y, 4, 0, Math.PI * 2);
                ctx.fillStyle = 'white';
                ctx.fill();
                ctx.strokeStyle = ds.color;
                ctx.lineWidth = 2;
                ctx.stroke();
            });
        });
    }

    // ── Donut Chart (Order Status) ──
    function drawDonutChart(id, data, colors) {
        var c = getCtx(id);
        if (!c) return;
        var ctx = c.ctx, W = c.w, H = c.h;
        var cx = W / 2, cy = H / 2;
        var outerR = Math.min(W, H) / 2 - 16;
        var innerR = outerR * 0.62;
        var total = data.reduce(function (a, b) { return a + b.value; }, 0);

        ctx.clearRect(0, 0, W, H);

        if (total === 0) {
            ctx.fillStyle = COLORS.border;
            ctx.beginPath();
            ctx.arc(cx, cy, outerR, 0, Math.PI * 2);
            ctx.arc(cx, cy, innerR, 0, Math.PI * 2, true);
            ctx.fill();
            return;
        }

        var startAngle = -Math.PI / 2;
        var gap = 0.03;

        data.forEach(function (item) {
            if (item.value === 0) return;
            var sliceAngle = (item.value / total) * (Math.PI * 2) - gap;

            ctx.beginPath();
            ctx.moveTo(cx, cy);
            ctx.arc(cx, cy, outerR, startAngle, startAngle + sliceAngle);
            ctx.arc(cx, cy, innerR, startAngle + sliceAngle, startAngle, true);
            ctx.closePath();
            ctx.fillStyle = item.color;
            ctx.fill();

            startAngle += sliceAngle + gap;
        });

        // Center text
        ctx.fillStyle = COLORS.textDark;
        ctx.font = 'bold 22px Syne, sans-serif';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        ctx.fillText(total, cx, cy - 8);
        ctx.font = '11px DM Sans, sans-serif';
        ctx.fillStyle = COLORS.text;
        ctx.fillText('Total Orders', cx, cy + 12);
    }

    // ── Bar Chart ──
    function drawBarChart(id, labels, values, color) {
        var c = getCtx(id);
        if (!c) return;
        var ctx = c.ctx, W = c.w, H = c.h;
        var padL = 16, padR = 16, padT = 16, padB = 36;
        var chartW = W - padL - padR;
        var chartH = H - padT - padB;
        var maxVal = Math.max.apply(null, values) * 1.2 || 1;
        var barWidth = (chartW / labels.length) * 0.55;
        var barSpacing = chartW / labels.length;

        ctx.clearRect(0, 0, W, H);

        values.forEach(function (val, i) {
            var barH = (val / maxVal) * chartH;
            var x = padL + barSpacing * i + (barSpacing - barWidth) / 2;
            var y = padT + chartH - barH;

            // Bar background
            ctx.fillStyle = 'rgba(0, 191, 255, 0.08)';
            ctx.beginPath();
            ctx.roundRect(x, padT, barWidth, chartH, 4);
            ctx.fill();

            // Bar fill
            var gradient = ctx.createLinearGradient(x, y, x, y + barH);
            gradient.addColorStop(0, color);
            gradient.addColorStop(1, COLORS.primaryLight);
            ctx.fillStyle = gradient;
            ctx.beginPath();
            ctx.roundRect(x, y, barWidth, barH, 4);
            ctx.fill();

            // Label
            ctx.fillStyle = COLORS.text;
            ctx.font = '10px DM Sans, sans-serif';
            ctx.textAlign = 'center';
            ctx.fillText(labels[i], x + barWidth / 2, H - padB + 16);
        });
    }

    // ── Helper: Format large numbers ──
    function formatK(val) {
        if (val >= 1000000) return (val / 1000000).toFixed(1) + 'M';
        if (val >= 1000) return (val / 1000).toFixed(0) + 'K';
        return Math.round(val).toString();
    }

    // ── Initialize Charts ──
    function initCharts() {
        // Revenue Line Chart
        var revenueCanvas = document.getElementById('revenueChart');
        if (revenueCanvas) {
            var months = JSON.parse(revenueCanvas.getAttribute('data-labels') || '[]');
            var revenue = JSON.parse(revenueCanvas.getAttribute('data-revenue') || '[]');
            var orders = JSON.parse(revenueCanvas.getAttribute('data-orders') || '[]');

            drawLineChart('revenueChart', months, [
                {
                    data: revenue,
                    color: COLORS.primary,
                    fillColor: COLORS.primaryLight
                },
                {
                    data: orders,
                    color: COLORS.success,
                    fillColor: COLORS.successLight
                }
            ]);
        }

        // Order Status Donut
        var donutCanvas = document.getElementById('orderDonut');
        if (donutCanvas) {
            var pending = parseInt(donutCanvas.getAttribute('data-pending') || '0');
            var processing = parseInt(donutCanvas.getAttribute('data-processing') || '0');
            var shipped = parseInt(donutCanvas.getAttribute('data-shipped') || '0');
            var delivered = parseInt(donutCanvas.getAttribute('data-delivered') || '0');
            var cancelled = parseInt(donutCanvas.getAttribute('data-cancelled') || '0');

            drawDonutChart('orderDonut', [
                { value: pending, color: COLORS.warning },
                { value: processing, color: COLORS.primary },
                { value: shipped, color: COLORS.purple },
                { value: delivered, color: COLORS.success },
                { value: cancelled, color: COLORS.danger }
            ]);
        }

        // Daily Orders Bar Chart
        var barCanvas = document.getElementById('dailyOrdersChart');
        if (barCanvas) {
            var barLabels = JSON.parse(barCanvas.getAttribute('data-labels') || '[]');
            var barValues = JSON.parse(barCanvas.getAttribute('data-values') || '[]');
            drawBarChart('dailyOrdersChart', barLabels, barValues, COLORS.primary);
        }
    }

    // Redraw on resize
    var resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(initCharts, 200);
    });

    // Init on load
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initCharts);
    } else {
        initCharts();
    }

    window.AdminCharts = { init: initCharts };

})();
