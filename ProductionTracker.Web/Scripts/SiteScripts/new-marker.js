$(() => {
    $(document).ready(function () {
        $('#submit-btn-div').hide()
    });
    
    $('.departemnt-ckb').on('change', function () { diableEnableCheckboxes('departemnt-ckb') });
    $('.style-ckb').on('change', function () { diableEnableCheckboxes('style-ckb') });
    $('.sleeve-ckb').on('change', function () { diableEnableCheckboxes('sleeve-ckb') });

    $('.marker-form-input').on('change', function () {
        if (checkIfValuesAreChecked() && $('.marker-name').val()) {
            $('#submit-btn-div').show()
        }
        else {
            $('#submit-btn-div').hide()
        }
    })
    $('#marker-submit-btn').on('click', function () { console.log('clicked the submit button'); })

    $('.chosseThisSizeA').on('change', function () {
        var thisElement = $(this);
        var sizeId = thisElement.data('sizeId');
        console.log(sizeId);
        $(`#s-a-${sizeId} :input`).prop("disabled", thisElement.checked);
    })

    function checkIfValuesAreChecked() {
        return ($(`.departemnt-ckb`).filter(':checked').length == 1) && ($(`.style-ckb`).filter(':checked').length == 1) && ($(`.sleeve-ckb`).filter(':checked').length == 1)
    }

    function diableEnableCheckboxes(checkboxClass) {
        if ($(`.${checkboxClass}`).filter(':checked').length == 1)
            $(`.${checkboxClass}:not(:checked)`).attr('disabled', 'disabled');
        else
            $(`.${checkboxClass}`).removeAttr('disabled');
    }
    
})