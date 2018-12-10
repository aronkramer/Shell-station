new Vue({
    el: '#app',
    mounted: function () {
        $(function () {
            $('#myTab a:first').tab('show')
        });
        this.pageHeader = 'Items In Production';
        this.tableHeaders = ['Id', 'SKU', 'Last Production Date', 'Items Not Received', 'Percentage Filled','actions'];
        this.loadSkus(true);
    },
    data: {
        detailHeaders: [],
        itemsInProduction: [],
        tableHeaders: [],
        pageHeader: 'Hi',
        itemActivty: [],
        currentItem: '',
        currentProduction:'',
        productions: [],
        isSkus: false,
        productionDetails: [],
        isProd: false,
        backButton: false,
        recivedItems: [],
        recivedItemProduction: ''
    },
    methods: {
        loadSkus: function (isInCuttingTicket) {
            $.get("/home/GetAllItemsWithDetails", { isInCuttingTicket}, SKU => {
                this.itemsInProduction = SKU;
            });
        },
        detailsForItem: function (event) {
            this.detailHeaders = ['Transaction Type', 'Date', 'Quantity'];
            this.currentProduction = '';
            var id = event.target.id;
            this.getActivitysByItem(id, function (result) {
                this.itemActivty = result.activity;
                this.currentItem = result.item;
                console.log(this.itemActivty);
                this.isSkus = true;
                this.isProd = false;
                $("#detail-modal").modal();
            }.bind(this));
            
        },
        getActivitysByItem: function (id,func) {
            $.get("/home/GetAllActivityOfAItem", { Id: id }, result => {
                func(result);
            });
        },
        productionTab: function () {
            this.getProductions();
        },
        getProductions: function () {
            $.get("/home/GetCuttingInstructionsWithInfo", result => {
                this.productions = result;
            });
        },
        detailsForProduction: function (event) {
            var id = event.target.id;
            this.getProductionDetails(id);
            this.detailHeaders = ['Sku', 'Quantity Ordered', 'Quantity Recived', 'Percent Filled'];
            this.isSkus = false;
            this.isProd = true;
            $("#detail-modal").modal();
        },
        getProductionDetails: function (id,func) {
            $.get("/home/GetDeatilsOfACuttingInstruction", { Id: id }, result => {
                this.currentItem = '';
                this.currentProduction = result.CuttingInstruction;
                this.productionDetails = result.details;
                console.log(this.productionDetails);
                if (func) {
                    func();
                }
            });
        },
        skuProdDeatails: function (event) {
            this.detailHeaders = ['Transaction Type', 'Date', 'Quantity'];
            var id = event.target.id;
            this.getActivitysByItem(id, function (result) {
                this.itemActivty = result.activity.filter(a => a.CuttingInstructionId === this.currentProduction.Id);
                this.currentItem = result.item;
                console.log(this.itemActivty);
                this.backButton = true;
                this.isSkus = true;
                this.isProd = false;
                $("#detail-modal").modal();
            }.bind(this));
        },
        backToProduction: function () {
            this.detailHeaders = ['Sku', 'Quantity Ordered', 'Quantity Recived', 'Percent Filled'];
            this.isSkus = false;
            this.isProd = true;
            this.backButton = false;
            this.currentItem = '';
        },
        recivedItemButton: function (event) {
            var id = event.target.id;
            this.getProductionDetails(id,  () => {
                var temp = this.productionDetails;
                this.recivedItems = temp.map(p => {
                    return {
                        Id: p.Id,
                        SKU: p.SKU,
                        Ordered: p.Ordered,
                        Received: p.Received,
                        PercentageFilled: p.PercentageFilled,
                        ItemsRecived: 0
                    };
                });
                this.recivedItemProduction = { Id: this.currentProduction.Id, Date: this.getDateInputFormat() };
            });
            
            this.detailHeaders = ['Sku', 'Quantity Ordered', 'Quantity Recived', 'Percent Filled', 'Recived'];

            $("#recive-items-modal").modal();
        },
        getDateInputFormat: function () {
            var date = new Date();
            day = fixDigit(date.getDate());
            month = fixDigit(date.getMonth() + 1);
            year = date.getFullYear();
            return [year, month, day].join('-');
        },
        submitRecivedItems: function () {
            var items = this.recivedItems.map(p => {
                return {
                    ItemId: p.Id,
                    CuttingInstuctionId: this.recivedItemProduction.Id,
                    Date: this.recivedItemProduction.Date,
                    Quantity: p.ItemsRecived
                };
            });
            $.post("/home/AddRecivedItems", { items }, () => { this.getProductions(); });
            $("#recive-items-modal").modal('hide');
            
        },
        itemRecivedChange: function () {
            //$('#submitRecivedItems').prop('disabled', this.checkVal());
        },
        //checkVal: function () {
        //    var temp = this.recivedItems;
        //    //var isRight = $(temp).is(function (i) { return i.ItemsRecived > (i.Ordered - i.Received); });
        //    //var isRight = $.grep(temp, function (i) { return i.ItemsRecived > (i.Ordered - i.Received); });
        //    var isRight = temp.some(function (i) {
        //        return i.ItemsRecived > (i.Ordered - i.Received);
        //    });
        //    console.log(isRight);
        //    return $(isRight).size() > 0;
        //},


    },
    computed: {
        checkVal: function () {
            var temp = this.recivedItems;
            var tooMuch = temp.some(function (i) {
                return i.ItemsRecived > (i.Ordered - i.Received);
            });
            var hasValues = temp.some(function (i) {
                return i.ItemsRecived > 0;
            });
            console.log('no values:' + !hasValues + 'too many items:' + tooMuch);
            return tooMuch || !hasValues;
        },
    }
});
function fixDigit(val) {
    return val.toString().length === 1 ? "0" + val : val;
}
//function getActivitysByItem(id) {
//    var stuff;
//    $.get("/home/GetAllActivityOfAItem", { Id: id }, result => {
//        stuff = result;
//    });
//    return stuff;
//};