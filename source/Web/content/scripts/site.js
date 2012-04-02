$(document).ready(function () {

    $('.dropdown-toggle').dropdown();
    $(".collapse").collapse()

    $('.date-ago').each(function (index, value) {
        var el = $(value);
        var date = moment(el.html());
        el.html(date.fromNow());
    });

    $('.at-on-date').each(function (index, value) {
        var el = $(value);
        var date = moment(el.html());
        el.html(date.format("MMM Do YYYY \\at h:mm:ss a"));
    });
});
