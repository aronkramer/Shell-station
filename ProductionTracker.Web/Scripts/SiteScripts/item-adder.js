$(() => {
    $(document).ready(function () {
        $('#department-dropdown').multiselect({
            buttonClass: 'btn btn-primary',
            numberDisplayed: 5,
            optionClass: 'department-dropdown-li',
            onChange: function (option, checked, select) {
                disableEnableDropdown('#department-dropdown', '#bodystyle-dropdown');
            }
        });
        $('#bodystyle-dropdown').multiselect({
            buttonClass: 'btn btn-success',
            numberDisplayed: 5,
            onChange: function (option, checked, select) {
                disableEnableDropdown('#bodystyle-dropdown', '#fabric-dropdown');
                //limitAmountOfSelections('#bodystyle-dropdown', 1);
            }
        });
        $('#fabric-dropdown').multiselect({
            buttonClass: 'btn btn-success',
            numberDisplayed: 5,
            onChange: function (option, checked, select) {
                //limitAmountOfSelections('#fabric-dropdown', 2);
            }
        });
        $('#sleaves-dropdown').multiselect({
            buttonClass: 'btn btn-success',
            numberDisplayed: 5,
            onChange: function (option, checked, select) {
                //limitAmountOfSelections('#fabric-dropdown', 2);
            }
        });
        $('#color-dropdown').multiselect({
            buttonClass: 'btn btn-success',
            numberDisplayed: 5,
            enableFiltering: true,
            enableCaseInsensitiveFiltering: true,
            filterBehavior: 'both',
            onChange: function (option, checked, select) {
                //limitAmountOfSelections('#fabric-dropdown', 2);
            }
        });
        disableEnableDropdown('#department-dropdown', '#bodystyle-dropdown');
        disableEnableDropdown('#bodystyle-dropdown', '#fabric-dropdown');
    });
    console.log('hello')
    function disableEnableDropdown(ddSelected, ddenabling) {
        if ($(`${ddSelected} option:selected`).size() > 0) {
            $(ddenabling).multiselect('enable');
        }
        else {
            $(ddenabling).multiselect('disable');
        }
    };
    function limitAmountOfSelections(dropdown, amount) {
        var selectedOptions = $(`${dropdown} option:selected`);

        if (selectedOptions.length >= amount) {
            // Disable all other checkboxes.
            var nonSelectedOptions = $(`${dropdown} option`).filter(function () {
                return !$(this).is(':selected');
            });

            nonSelectedOptions.each(function () {
                var input = $('input[value="' + $(this).val() + '"]');
                input.prop('disabled', true);
                input.parent('li').addClass('disabled');
            });
        }
        else {
            // Enable all checkboxes.
            $(`${dropdown} option`).each(function () {
                var input = $('input[value="' + $(this).val() + '"]');
                input.prop('disabled', false);
                input.parent('li').addClass('disabled');
            });
        }
    }
})