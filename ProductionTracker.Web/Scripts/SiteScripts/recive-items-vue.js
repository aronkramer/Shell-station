const app = new Vue({
    el: '#app',
    mounted: function () {
        this.getLotNumbers(() => {
            $('.lot-numbers-select').select2({
                data: this.lotNumbers,
                placeholder: "Select lot numbers",
            });
            this.select2Loaded = true;
        });
    },
    data: {
        lotNumbers: [],
        select2Loaded: false,
        items: [],
        fillBtnText: ''
    },
    methods: {
        getLotNumbers: function (func) {
            $.get('/production/GetLotNumbers', result => {
                this.lotNumbers = result;
                func();
            });
        },
        submitLotNumbers: function () {
            var cuttingInstructionIds =  $('.lot-numbers-select').select2('data').map(function (e) {
                return e.id;
            });
            console.log(cuttingInstructionIds);
            $.post('/production/GetItemsFromLotNumbers', { cuttingInstructionIds }, result => {
                this.items = result;
                this.fillBtnText = 'Fill All';
            });
        },
        fillAll: function () {
            if (this.fillBtnText === 'Fill All') {
                this.items = this.items.map(p => {
                    p.ItemsRecived = p.QuantityOrdered - p.QuantityReceived;
                    return p;
                });
                this.fillBtnText = 'Reset';
            }
            else {
                this.items = this.items.map(p => {
                    p.ItemsRecived = null;
                    return p;
                });
                this.fillBtnText = 'Fill All';
            }
        },
    }
})