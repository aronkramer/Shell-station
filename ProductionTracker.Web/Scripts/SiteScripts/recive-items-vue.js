const app = new Vue({
    el: '#app',
    mounted: function () {
        //$("#app").block({
        //    message: '<h4>Processing....</h4>',
        //    css: { border: '3px solid #a00' }
        //});
        this.getLotNumbers(() => {
            $('.lot-numbers-select').select2({
                data: this.lotNumbers,
                placeholder: "Select lot numbers",
                
            });
            //$("#app").unblock();
            this.select2Loaded = true;
        });
        this.recivedDate = this.getDateInputFormat();
    },
    data: {
        lotNumbers: [],
        select2Loaded: false,
        items: [],
        fillBtnText: '',
        recivedDate: ''
    },
    methods: {
        getLotNumbers: function (func) {
            $.get('/production/GetLotNumbers', result => {
                this.lotNumbers = result;
                func();
            });
        },
        submitLotNumbers: function () {
            var cuttingInstructionIds = $('.lot-numbers-select').select2('data').map(function (e) {
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
        submitRecivedItems: function () {
            $("#app").block({
                message: '<h4>Processing....</h4>',
                css: { border: '3px solid #a00' }
            });
            var items = this.items.map(p => {
                return {
                    ItemId: p.ItemId,
                    CuttingInstuctionId: p.CuttingInstructionId,
                    Date: this.recivedDate,
                    Quantity: p.ItemsRecived,
                    OrderedId: p.Id

                };
            });
            $.post("/home/AddRecivedItems", { items }, () => { $('.lot-numbers-select').val(null).trigger('change'); this.items = []; $("#app").unblock(); });


        },
        getDateInputFormat: function () {
            var date = new Date();
            day = fixDigit(date.getDate());
            month = fixDigit(date.getMonth() + 1);
            year = date.getFullYear();
            return [year, month, day].join('-');
        },
    },
    computed: {
        checkVal: function () {
            var temp = this.items;
            ////var tooMuch = temp.some(function (i) {
            //    return i.ItemsRecived > (i.Ordered - i.Received);
            //});
            var hasValues = temp.some(function (i) {
                return i.ItemsRecived > 0;
            });
            //console.log('no values:' + !hasValues + 'too many items:' + tooMuch);
            return !hasValues;
        }
    },
});
function fixDigit(val) {
    return val.toString().length === 1 ? "0" + val : val;
}