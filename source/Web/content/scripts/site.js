$(document).ready(function () {

    $('.date-ago').each(function (index, value) {
        var el = $(value);
        var date = moment(el.html());
        el.html(date.fromNow());
    });
});
