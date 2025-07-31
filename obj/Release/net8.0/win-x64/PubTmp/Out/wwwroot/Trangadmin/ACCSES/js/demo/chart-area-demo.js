// Set new default font family and font color to mimic Bootstrap's default styling
Chart.defaults.global.defaultFontFamily = 'Nunito', '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#858796';

function number_format(number, decimals, dec_point, thousands_sep) {
    number = (number + '').replace(',', '').replace(' ', '');
    var n = !isFinite(+number) ? 0 : +number,
        prec = !isFinite(+decimals) ? 0 : Math.abs(decimals),
        sep = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep,
        dec = (typeof dec_point === 'undefined') ? '.' : dec_point,
        s = '',
        toFixedFix = function (n, prec) {
            var k = Math.pow(10, prec);
            return '' + Math.round(n * k) / k;
        };
    s = (prec ? toFixedFix(n, prec) : '' + Math.round(n)).split('.');
    if (s[0].length > 3) {
        s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
    }
    if ((s[1] || '').length < prec) {
        s[1] = s[1] || '';
        s[1] += new Array(prec - s[1].length + 1).join('0');
    }
    return s.join(dec);
}

// Area Chart Example
var ctx = document.getElementById("myAreaChart").getContext('2d');
var lineChart = new Chart(ctx, {
    type: 'line', // Chọn loại biểu đồ là đường
    data: {
        labels: @Html.Raw(Json.Serialize(ViewBag.YearLabels)), // Dữ liệu cho trục X (Năm)
        datasets: [{
            label: 'Số Dự Án', // Tên của dataset
            data: @Html.Raw(Json.Serialize(ViewBag.ProjectCountByYear)), // Dữ liệu cho trục Y (Số dự án)
            borderColor: 'rgba(75, 192, 192, 1)', // Màu của đường
            backgroundColor: 'rgba(75, 192, 192, 0.2)', // Màu nền của các điểm
            fill: true, // Điền màu phía dưới đường
            tension: 0.1 // Mức độ cong của đường
        }]
    },
    options: {
        responsive: true,
        scales: {
            y: {
                beginAtZero: true // Đảm bảo trục Y bắt đầu từ 0
            }
        }
    }
});
