new Vue({
    el: '#app',
    data: {
        markerSizes: [],
        depId: 0
    },
    methods: {
        markerSubmitBtn: function () {
            this.depId = +$('.departemnt-ckb').val();
            $.get("/home/GetSizesOfDepartment", { depId: this.depId }, sizes => {
                this.markerSizes = sizes;
            });
        },
        chosseThisSizeA: function (event) {
            var sizeId = event.target.id;
            //$('#theTextfield').prop('disabled', this.checked);
            $(`#s-a-${sizeId} :input`).prop("disabled", this.checked);
        }
    },

});