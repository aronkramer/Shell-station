$(() => {
    $(document).ready(function () {
        $('#department-dropdown').multiselect({
            buttonClass: 'btn btn-primary',
            numberDisplayed: 5,
            optionClass: 'department-dropdown-li',
            onChange: function (option, checked, select) {
                disableEnableDropdown('#department-dropdown', '#bodystyle-dropdown');
                disableButtonUntilDropdownsSelected();
            }
        });
        $('#bodystyle-dropdown').multiselect({
            buttonClass: 'btn btn-success',
            numberDisplayed: 5,
            optionClass: 'bodystyle-dropdown',
            onChange: function (option, checked, select) {
                disableEnableDropdown('#bodystyle-dropdown', '#fabric-dropdown');
                disableButtonUntilDropdownsSelected();
                limitAmountOfSelections('bodystyle-dropdown', 1);
            }
        });
        $('#fabric-dropdown').multiselect({
            buttonClass: 'btn btn-success',
            numberDisplayed: 5,
            optionClass: function (element) {
                var value = $(element).val();

                if (value % 2 == 0) {
                    return 'even';
                }
                else {
                    return 'odd';
                }
            },
            onChange: function (option, checked, select) {
                disableButtonUntilDropdownsSelected();
                disableEnableDropdown('#fabric-dropdown', '#sleaves-dropdown');
                //limitAmountOfSelections('#fabric-dropdown', 2);
            }
        });
        $('#sleaves-dropdown').multiselect({
            buttonClass: 'btn btn-success',
            numberDisplayed: 5,
            onChange: function (option, checked, select) {
                disableButtonUntilDropdownsSelected();
                disableEnableDropdown('#sleaves-dropdown', '#color-dropdown');
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
                disableButtonUntilDropdownsSelected();
                //limitAmountOfSelections('#fabric-dropdown', 2);
            }
        });
        disableEnableDropdown('#department-dropdown', '#bodystyle-dropdown');
        disableEnableDropdown('#bodystyle-dropdown', '#fabric-dropdown');
        disableEnableDropdown('#fabric-dropdown', '#sleaves-dropdown');
        disableEnableDropdown('#sleaves-dropdown', '#color-dropdown');
        disableButtonUntilDropdownsSelected();
    });
    console.log('hello')
    function disableEnableDropdown(ddSelected, ddenabling) {
        if (amountSelected(ddSelected)) {
            $(ddenabling).multiselect('enable');
        }
        else {
            $(ddenabling).multiselect('disable');
        }
    };
    function disableButtonUntilDropdownsSelected() {
        const dd1 = amountSelected('#department-dropdown');
        const dd2 = amountSelected('#bodystyle-dropdown');
        const dd3 = amountSelected('#fabric-dropdown');
        const dd4 = amountSelected('#sleaves-dropdown');
        const dd5 = amountSelected('#color-dropdown');
        $('#btn-submit').prop('disabled', !(dd1 && dd2 && dd3 && dd4 && dd5) );
    }
    function amountSelected(dropdown) {
        return $(`${dropdown} option:selected`).size();
    }
    function limitAmountOfSelections(dropdown, amount) {
        var selectedOptions = $(`#${dropdown} option:selected`);
        console.log(selectedOptions);
        if (selectedOptions.length >= amount) {
            // Disable all other checkboxes.
            var nonSelectedOptions = $(`#${dropdown} option`).filter(function () {
                return !$(this).is(':selected');
            });

            nonSelectedOptions.each(function () {
                var input = $(`input[value="' + $(this).val() + '"] li.${dropdown}`);
                input.prop('disabled', true);
                input.parent('li').addClass('disabled');
            });
        }
        else {
            // Enable all checkboxes.
            $(`#${dropdown} option`).each(function () {
                var input = $(`input[value="' + $(this).val() + '"] li.${dropdown}`);
                input.prop('disabled', false);
                input.parent('li').addClass('disabled');
            });
        }
    }
})