$(() => {
    console.log("dksjafh");
    $('.sku').on('change', function () {
        const tr = $(this).parent().parent();
        const val = $(this).val();
        if (val) {
            tr.removeClass('backround-red');
        } else {
            tr.addClass('backround-red');
        }
    })
});