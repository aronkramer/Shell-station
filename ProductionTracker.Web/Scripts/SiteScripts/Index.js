var app = new Vue({
    el: '#app',
    mounted: function () {
        const urlParams = new URLSearchParams(window.location.search);
        const myParam = urlParams.get('tab');
        console.log(myParam);
        this.pageHeader = 'Items In Production';
        if (myParam && myParam.toLowerCase() === 'byproductions') {
            $(function () {
                $('#myTab a[href="#byProductions"]').tab('show');
                this.productionTab();
            }.bind(this));
        }
        else {
            $(function () {
                $('#myTab a:first').tab('show');
                this.loadSkus(true);
            }.bind(this));

        } 
        
        
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
        recivedItemProduction: '',
        fillBtnText: '',
        itemsInProductionSortKey: '',
        itemsInProductionDescSortKey: true,
        orderedUsers: []
    },
    methods: {
        loadSkus: function (isInCuttingTicket) {
            $('.byItems').block({
                message: '<h4>Processing....</h4>',
                css: { border: '3px solid #a00' }
            }); 
            this.tableHeaders = ['Id', 'SKU', 'Last Production Date', 'Items In Production', 'actions'];
            $.get("/home/GetAllItemsWithDetails", { isInCuttingTicket}, SKU => {
                this.itemsInProduction = SKU;
                this.orderArray();
                $('.byItems').unblock();
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
        itemTab: function () {
            if (this.itemsInProduction.length < 1) {
                this.loadSkus(true);
            }
        },
        getActivitysByItem: function (id,func) {
            $.get("/home/GetAllActivityOfAItem", { Id: id }, result => {
                func(result);
            });
        },
        productionTab: function () {
            if (this.productions.length < 1) {
            this.getProductions();
            }
        },
        getProductions: function () {
            $('.byProductions').block({
                message: '<h4>Processing....</h4>',
                css: { border: '3px solid #a00' }
            }); 
            $.get("/home/GetCuttingInstructionsWithInfo", result => {
                this.productions = result;
                $('.byProductions').unblock();
            });
        },
        detailsForProduction: function (event) {
            var id = event.target.id;
            this.getProductionDetails(id);
            this.detailHeaders = ['Sku', 'Lot Number','Quantity Ordered', 'Quantity Recived', 'Percent Filled'];
            this.isSkus = false;
            this.isProd = true;
            $("#detail-modal").modal();
        },
        getProductionDetails: function (id,func) {
            $.get("/home/GetDeatilsOfACuttingInstruction", { Id: id }, result => {
                this.currentItem = '';
                this.currentProduction = result.Production;
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
                this.itemActivty = result.activity.filter(a => this.currentProduction.CuttingIntrustionIds.includes(a.CuttingInstructionId));
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
                        OrderedId: p.OrderedId,
                        Received: p.Received,
                        PercentageFilled: p.PercentageFilled,
                        ItemsRecived: null,
                        CuttingInstructionId: p.CuttingInstructionId
                    };
                });
                this.recivedItemProduction = { Id: this.currentProduction.Id, Date: this.getDateInputFormat() };
                $("#recive-items-modal").modal();
            });
            this.fillBtnText = 'Fill All';
            
        },
        getDateInputFormat: function () {
            var date = new Date();
            day = fixDigit(date.getDate());
            month = fixDigit(date.getMonth() + 1);
            year = date.getFullYear();
            return [year, month, day].join('-');
        },
        submitRecivedItems: function () {
            $(".modal-content").block({
                message: '<h4>Processing....</h4>',
                css: { border: '3px solid #a00' }
            });
            var items = this.recivedItems.map(p => {
                return {
                    ItemId: p.Id,
                    CuttingInstuctionId: p.CuttingInstructionId,
                    Date: this.recivedItemProduction.Date,
                    Quantity: p.ItemsRecived,
                    OrderedId: p.OrderedId

                };
            });
            $.post("/home/AddRecivedItems", { items }, () => { this.getProductions(); $(".modal-content").unblock(); $("#recive-items-modal").modal('hide'); });
            
            
        },
        itemRecivedChange: function () {
            //$('#submitRecivedItems').prop('disabled', this.checkVal());
        },
        fillAll: function () {
            if (this.fillBtnText === 'Fill All') {
                this.recivedItems = this.recivedItems.map(p => {
                    p.ItemsRecived = p.Ordered - p.Received;
                    return p;
                });
                this.fillBtnText = 'Reset';
            }
            else {
                this.recivedItems = this.recivedItems.map(p => {
                    p.ItemsRecived = null;
                    return p;
                });
                this.fillBtnText = 'Fill All';
            }
        },
        download: function (event) {
            var productionId = event.target.id;
            window.location = `/Production/DownloadCuttingInstuctions?productionId=${productionId}`;
            //$.get("/Production/DownloadCuttingInstuctions", { productionId });
        },
        printBarCodes: function (id) {
            //var productionId = event.target.id;
            //$.get("/home/BarcodesFromProduction", { id: productionId });
            window.open(`/home/BarcodesFromProduction?id=${id}`);
        },
        orderArray: function () {
            if (this.itemsInProduction.length && this.itemsInProductionSortKey ) {
            var array = this.itemsInProduction;
            var sortKey = this.itemsInProductionSortKey;
                var minus = this.itemsInProductionDescSortKey;
                
                this.orderedUsers = array.sort(function (a, b) {
                if (minus)
                    return a[sortKey] > b[sortKey] ? -1 : a[sortKey] < b[sortKey] ? 1 : 0;
                else
                    return a[sortKey] < b[sortKey] ? -1 : a[sortKey] > b[sortKey] ? 1 : 0;
            });
            }
            else
                this.orderedUsers = this.itemsInProduction;
        }

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
//function getActivitysByItem(id) {
//    var stuff;
//    $.get("/home/GetAllActivityOfAItem", { Id: id }, result => {
//        stuff = result;
//    });
//    return stuff;
//};