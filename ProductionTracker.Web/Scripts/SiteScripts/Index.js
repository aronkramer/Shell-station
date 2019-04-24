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
        orderedUsers: [],
        filterLists: {
            Departments: null,
            Materials: null,
            BodyStyles: null,
            Sleeves: null,
            Sizes: null,
            Colors: null,
            //markers: null
        },
        itemSearch: '',
        filterApliedList:[]
    },
    methods: {
        loadSkus: function (isInCuttingTicket) {
            $('.byItems').block({
                message: '<img src="/Metronic/theme/assets/global/img/loading-spinner-grey.gif" align="middle" style="width:150px;"/>',
                
                overlayCSS: { backgroundColor: '#dcd8d8' },
                css: {
                    border: 'none',
                    padding: '15px',
                    backgroundColor: 'none',
                    '-webkit-border-radius': '10px',
                    '-moz-border-radius': '10px',
                    opacity: .5,
                    color: 'none'  }
            }); 
            this.tableHeaders = ['Id', 'SKU', 'Last Production Date', 'Items In Production', 'actions'];
            $.get("/home/GetAllItemsWithDetails", { isInCuttingTicket}, SKU => {
                this.itemsInProduction = SKU;
                this.orderArray();
                $('.byItems').unblock();
                this.getTheDataTables();
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
        },
        getTheDataTables: function (func) {
            $.get('/production/GetAtributteListsForFilter', result => {
                this.filterLists.Materials =   result.material;
                this.filterLists.Colors =      result.colors;
                this.filterLists.Sizes =       result.sizes;
                //this.filterListS.markers = result.markers;
                this.filterLists.BodyStyles =  result.bodyStyles;
                this.filterLists.Sleeves =     result.sleeves;
                this.filterLists.Departments = result.departments;
                console.log(result);
                if (func) func();

            });
        },
        clearFillAllFilter: function (fill) {
            for (k in this.filterLists) {
                this.checkClearBoxesAtribute(k, fill);
            }
        },
        checkClearBoxesAtribute: function (list, check) {
            //this.filterLists[list].forEach((r,x) => {
            //    r.Selected = check;
            //});
            //for (k of this.filterLists[list]) {
            //    k.Selected = check;
            //}
            this.filterLists[list] = this.filterLists[list].map(r => {
                r.Selected = check; return r;
            });
        },
        atributeCountMsg: function (list) {
            if (this.filterLists[list]) {
                var theList = jQuery.extend(true, [], this.filterLists[list].filter(r => r.Selected));
                this.filterLists[list] = jQuery.extend(true, [], this.filterLists[list]);
                if (!theList) {
                    return 'None Selected';
                }
                else if (theList.length === this.filterLists[list].length) {
                    return 'All Selected';
                }
                else {
                    return `${theList.length} selected`;
                }
            }
            else {
                return null;
            }
        },
        applyFilter: function () {
            this.filterApliedList = this.itemsInProduction.filter(element => {
                return this.filterdLists.Departments.map(x => x.Id).includes(element.DepartmentId) &&
                    this.filterdLists.Materials.map(x => x.Id).includes(element.MaterialId)  &&
                    this.filterdLists.BodyStyles.map(x => x.Id).includes(element.BodyStyleId) &&
                    this.filterdLists.Sleeves.map(x => x.Id).includes(element.SleeveId) &&
                    this.filterdLists.Sizes.map(x => x.Id).includes(element.SizeId) &&
                    this.filterdLists.Colors.map(x => x.Id).includes(element.ColorId);
            });
        },
        resetFilters: function () {
            this.filterApliedList = [];
            this.clearFillAllFilter(false);
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
        },
        filterdLists: function () {
            let ret = {};
            
            for (k in this.filterLists) {
                    ret[k] = this.filterLists[k] ? this.filterLists[k].filter(x => x.Selected) : null;
            }
            return ret;
            },
        countMsg: function () {
            let ret = {};
            for (k in this.filterdLists) {
                ret[k] = this.filterdLists[k] ?
                    this.filterdLists[k].length === 0 ? "None selected" :
                        this.filterdLists[k].length === this.filterLists[k].length ? "All selected" :
                            `${this.filterdLists[k].length} selected` : null;
            }
            return ret;
        },
        orderedArray: function () {
            if (this.searchFileredItems.length && this.itemsInProductionSortKey) {
                var sortKey = this.itemsInProductionSortKey;
                var minus = this.itemsInProductionDescSortKey;

                return this.searchFileredItems.sort( (a, b) => {
                    if (minus)
                        return a[sortKey] > b[sortKey] ? -1 : a[sortKey] < b[sortKey] ? 1 : 0;
                    else
                        return a[sortKey] < b[sortKey] ? -1 : a[sortKey] > b[sortKey] ? 1 : 0;
                });
            }
            else
                return this.searchFileredItems;
        },
        searchFileredItems: function () {
            if (this.filterApliedList.length) {
                return this.filterApliedList.filter(element => {
                    return element.SKU.match(this.itemSearch.toUpperCase()) || element.Id.toString().match(this.itemSearch.toUpperCase());
                });
            }
            else if (this.itemsInProduction.length) {
                return this.itemsInProduction.filter(element => {
                    return element.SKU.match(this.itemSearch.toUpperCase()) || element.Id.toString().match(this.itemSearch.toUpperCase());
                });
            } else {
                return [];
            }
        },
        applyBtnDisable: function () {
            if (this.filterdLists) {
                return !Object.values(this.filterdLists).every(x => x ? x.length > 0 : false);
            }
            else return true;
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