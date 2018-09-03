$(() => {
    $("#btn-row-adder").on('click', AddRow);
    let x = 1;
    function AddRow() {
        $("#color-form-table").append(`<tr>
                   <td>
                        <input type="text" placeholder="Id" name="colors[${x}].Id" class="form-control" />
                    </td>
                    <td>
                        <input type="text" placeholder="Color" name="colors[${x}].Color1" class="form-control" />
                    </td>
                </tr>`)
        x++;
    };
});