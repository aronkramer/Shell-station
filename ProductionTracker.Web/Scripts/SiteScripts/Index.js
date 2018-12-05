new Vue({
    el: '#app',
    mounted: function () {
        $(function () {
            $('#myTab a:first').tab('show')
        });
        this.pageHeader = 'Items In Production';
        this.tableHeaders = ['Id', 'SKU', 'Last Production Date', 'Items Not Received', 'Percentage Filled','actions'];
        this.loadSkus();
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
        backButton: false
    },
    methods: {
        loadSkus: function () {
            $.get("/home/GetAllItemsWithDetails", SKU => {
                this.itemsInProduction = SKU;
            });
        },
        detailsForItem: function (event) {
            this.detailHeaders = ['Transaction Type', 'Date', 'Quantity'];
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
        getProductionDetails: function (id) {
            $.get("/home/GetDeatilsOfACuttingInstruction", { Id: id }, result => {
                this.currentItem = '';
                this.currentProduction = result.production;
                this.productionDetails = result.details;
                console.log(this.productionDetails);
            });
        },
        skuProdDeatails: function (event) {
            this.detailHeaders = ['Transaction Type', 'Date', 'Quantity'];
            var id = event.target.id;
            this.getActivitysByItem(id, function (result) {
                this.itemActivty = result.activity.filter(a => a.ProductionId === this.currentProduction.Id);
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
        }

    },
});

//function getActivitysByItem(id) {
//    var stuff;
//    $.get("/home/GetAllActivityOfAItem", { Id: id }, result => {
//        stuff = result;
//    });
//    return stuff;
//};