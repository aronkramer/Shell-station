$(() => {
    $('.sku').on('blur', function () {
        const sku = $(this).val();
        const tr = $(this).parent().parent();
        getIdAndSku(sku, item => {
            if (item) {
                tr.find('td:eq(0) input').val(item.Id);
                tr.find('td:eq(1) input').val(item.SKU);
                tr.find('td:eq(1) label').attr('hidden', '');
                tr.find('td:eq(1) label').removeClass('error');
                $('.submit-btn').prop('disabled', $('.error').size() > 0);

            }
            else {
                tr.find('td:eq(0) input').val(0);
                tr.find('td:eq(1) label').removeAttr("hidden");
                tr.find('td:eq(1) label').addClass('error');
                $('.submit-btn').prop('disabled', $('.error').size() > 0);

            }
            console.log(item);
            
        });
    });
    function getIdAndSku(sku, func) {
        $.get('/production/GetItemId', { sku }, result => {
            func(result);
        });
    }
    
});