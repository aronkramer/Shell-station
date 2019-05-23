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
        productionItems: {
            items: [],
            filterLists:{
            Departments: null,
            Materials: null,
            BodyStyles: null,
            Sleeves: null,
            Sizes: null,
            Colors: null
            },
            itemSearch: '',
            sortKey: '',
            descSortKey: true,
            displayBody: {
                expandTool: 'expand',
                display: 'none'
            }
        },
        seasonItems: {
            season: {
                PlannedProductionId: 0,
                Name: ''
            },
            items: [],
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
            sortKey: '',
            descSortKey: true,
            displayBody: {
                expandTool: 'expand',
                display: 'none'
            },
            details: [],
            currentItem: null
        },
        details: {
            typeOfDetails:'',
            itemDefalt: '',
            moreDetails:[]
        },
        tableHeaders: [],
        pageHeader: 'Hi',
        itemActivty: [],
        itemActivtyActive: false,
        currentItem: '',
        currentProduction: '',
        productions: [],
        isSkus: false,
        productionDetails: [],
        isProd: false,
        backButton: false,
        recivedItems: [],
        recivedItemProduction: '',
        fillBtnText: '',
        plannedProductions:[],
        detailMonths: {
            selected: 3,
            options: [
                { value: 3, name: '3 months' },
                { value: 6, name: '6 months' },
                { value: 12, name: '12 months' },
                { value: null, name: 'All' }
            ]
        },
        //filterLists: {
        //    Departments: null,
        //    Materials: null,
        //    BodyStyles: null,
        //    Sleeves: null,
        //    Sizes: null,
        //    Colors: null,
        //    //markers: null
        //},
        //filterListsSeason: {
        //    Departments: null,
        //    Materials: null,
        //    BodyStyles: null,
        //    Sleeves: null,
        //    Sizes: null,
        //    Colors: null,
        //    //markers: null
        //},
        //itemsInProduction: [],
        //itemSearch: '',
        //displayBody: {
        //    expandTool: 'expand',
        //    display: 'none'
        //},
        //itemsInProductionSortKey: '',
        //itemsInProductionDescSortKey: true,
        //orderedUsers: [],
    },
    methods: {
        loadSkus: function () {
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
            $.get("/home/GetAllItemsWithDetails", SKU => {
                this.productionItems.items = SKU;
                //this.itemsInProduction = SKU;
                $('.byItems').unblock();
                this.getTheDataTables();
            });
        },
        loadSeasonItems: function (plannedProdId,func) {
            $('.bySeason').block({
                message: '<img src="/Metronic/theme/assets/global/img/loading-spinner-grey.gif" align="middle" style="width:150px;"/>',

                overlayCSS: { backgroundColor: '#dcd8d8' },
                css: {
                    border: 'none',
                    padding: '15px',
                    backgroundColor: 'none',
                    '-webkit-border-radius': '10px',
                    '-moz-border-radius': '10px',
                    opacity: .5,
                    color: 'none'
                }
            });
            $.get("/home/GetItemsofSeason", { plannedProdId }, result => {
                this.seasonItems.items = result.Items;
                this.seasonItems.season = result.Season;
                if (func) func();
                $('.bySeason').unblock();
                console.log(result);
            });
        },
        seasonTab: function () {
            if (this.seasonItems.items.length < 1) {
                this.loadSeasonItems(null, () => this.getPlannedProds());
                
            }
        },
        changedSeason: function () {
            this.loadSeasonItems(this.seasonItems.season.PlannedProductionId);
        },
        changedDetailsSeason: function (index) {
            var itemId;
            var currentList; 
            var currentDetails;
            var ppId;
            
            if (this.details.typeOfDetails === 'seasonDefalt') {
                itemId = this.seasonItems.currentItem.Id;
                currentList = this.seasonItems.details;
            }
            else if (this.details.typeOfDetails === 'moreDetails') {
                itemId = this.details.itemDefalt.item.Id;
                currentList = this.details.moreDetails;
            }
            currentDetails = currentList[index];
            ppId = currentDetails.season.PlannedProductionId;
            this.getSeasonItemActivity(itemId, ppId, function (result) {
                currentList.splice(index, 1, { season: result.season, activity: result.activity });
            }.bind(this));
        },
        detailsForItem: function (event) {
            //this.detailHeaders = ['Transaction Type', 'Date', 'Quantity'];
            //this.currentProduction = '';
            var id = event.target.id;
            var months = this.detailMonths.selected;
            this.getItemActivity(id, months, function (result) {
                this.details.typeOfDetails = 'defaltItem';
                this.details.itemDefalt = result;
                console.log(this.itemActivty);
                $("#item-detail-modal").modal();
            }.bind(this));

        },
        monthsChange: function () {
            $(".modal-content").block({
                message: '<h4>Processing....</h4>',
                css: { border: '3px solid #a00' }
            });
            var id = this.details.itemDefalt.item.Id;
            var months = this.detailMonths.selected;
            this.getItemActivity(id, months, function (result) {
                this.details.itemDefalt.activity = result.activity;
                $(".modal-content").unblock();
            }.bind(this));
        },
        itemTab: function () {
            //if (this.itemsInProduction.length < 1) {
            if (this.productionItems.items.length < 1) {
                this.loadSkus(true);
            }
        },
        getActivitysByItem: function (id,func) {
            $.get("/home/GetAllActivityOfAItem", { Id: id}, result => {
                func(result);
            });
        },
        getItemActivity: function (id, months, func) {
            $.get("/home/GetItemActivity", { Id: id, months }, result => {
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
            this.detailHeaders = ['Sku', 'Lot Number', 'Quantity Ordered', 'Quantity Recived', 'Percent Filled'];
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
        getTheDataTables: function (func) {
            $.get('/production/GetAtributteListsForFilter', result => {
                this.productionItems.filterLists.Materials = result.material;
                this.productionItems.filterLists.Colors = result.colors;
                this.productionItems.filterLists.Sizes = result.sizes;
                this.productionItems.filterLists.BodyStyles = result.bodyStyles;
                this.productionItems.filterLists.Sleeves = result.sleeves;
                this.productionItems.filterLists.Departments = result.departments;

                this.seasonItems.filterLists.Materials = result.material;
                this.seasonItems.filterLists.Colors = result.colors;
                this.seasonItems.filterLists.Sizes = result.sizes;
                this.seasonItems.filterLists.BodyStyles = result.bodyStyles;
                this.seasonItems.filterLists.Sleeves = result.sleeves;
                this.seasonItems.filterLists.Departments = result.departments;
                console.log(result);
                if (func) func();

            });
        },
        getPlannedProds: function (func) {
            $.get('/production/GetValidatoinLists', result => {
                this.plannedProductions = result.plannedProductions;
                if (func) func();
            });
        },
        clearFillAllFilter: function (fill) {
            for (k in this.productionItems.filterLists) {
                this.checkClearBoxesAtribute(k, fill);
            }
        },
        clearFillAllFilterSeasons: function (fill) {
            for (k in this.seasonItems.filterLists) {
                this.checkClearBoxesAtributeSeason(k, fill);
            }
        },
        checkClearBoxesAtribute: function (list, check) {
            this.productionItems.filterLists[list] = this.productionItems.filterLists[list].map(r => {
                r.Selected = check; return r;
            });
        },
        checkClearBoxesAtributeSeason: function (list, check) {
            this.seasonItems.filterLists[list] = this.seasonItems.filterLists[list].map(r => {
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
        resetFilters: function () {
            this.clearFillAllFilter(false);
        },
        resetFiltersSeasons: function () {
            this.clearFillAllFilterSeasons(false);
        },
        openClosePortlet: function () {
            this.productionItems.displayBody.expandTool = this.productionItems.displayBody.expandTool === 'expand' ? 'collapse' : 'expand';
            this.productionItems.displayBody.display = this.productionItems.displayBody.display === 'none' ? 'block' : 'none';

        },
        openClosePortletSeason: function () {
            this.seasonItems.displayBody.expandTool = this.seasonItems.displayBody.expandTool === 'expand' ? 'collapse' : 'expand';
            this.seasonItems.displayBody.display = this.seasonItems.displayBody.display === 'none' ? 'block' : 'none';

        },
        detailsForSeasonItem: function (event) {
            var itemId = event.target.id;
            var ppId = this.seasonItems.season.PlannedProductionId;
            this.getSeasonItemActivity(itemId, ppId, function (result) {
                this.seasonItems.details = [{ season: result.season, activity: result.activity }];
                this.seasonItems.currentItem = result.item;
                this.details.typeOfDetails = 'seasonDefalt';
                $("#item-detail-modal").modal();

            }.bind(this));

        },
        getSeasonItemActivity: function (itemId,ppId,func) {
            $.get("/home/GetSeasonItemActivity", { itemId, ppId }, result => {
                func(result);
            });
        },
        getDetailforSeasonItem: function (itemId) {
            var item = this.seasonItems.items.find(e => {
                return e.Id === itemId;
            });
            item.DetailsOpened = !item.DetailsOpened;
            if (item.DetailsOpened) {
                var ppId = this.seasonItems.season.PlannedProductionId;
                this.getSeasonItemActivity(itemId, ppId, function (result) {
                    item.Details = result;
                }.bind(this));
            }
        },
        moreDetails: function (id) {
            if (this.details.typeOfDetails === 'seasonDefalt') {
                this.getPlannedProds(() => {
                    $.get("/home/getmoredetails", { id }, result => {
                        this.seasonItems.details = result;

                    });
                });
            }
            else if (this.details.typeOfDetails === 'defaltItem') {

            this.getPlannedProds(() => { 
                $.get("/home/getmoredetails", { id }, result => {
                    this.details.moreDetails = result;
                    this.details.typeOfDetails = 'moreDetails';

                });
            });
            }
        }


//old ways of doing thing
        //detailsForItem: function (event) {
        //    this.detailHeaders = ['Transaction Type', 'Date', 'Quantity'];
        //    this.currentProduction = '';
        //    var id = event.target.id;
        //    this.getActivitysByItem(id, function (result) {
        //        this.itemActivty = result.activity;
        //        this.currentItem = result.item;
        //        console.log(this.itemActivty);
        //        this.isSkus = true;
        //        this.isProd = false;
        //        $("#detail-modal").modal();
        //    }.bind(this));
            
        //},
        //orderArray: function () {
        //    if (this.itemsInProduction.length && this.itemsInProductionSortKey ) {
        //    var array = this.itemsInProduction;
        //    var sortKey = this.itemsInProductionSortKey;
        //        var minus = this.itemsInProductionDescSortKey;
                
        //        this.orderedUsers = array.sort(function (a, b) {
        //        if (minus)
        //            return a[sortKey] > b[sortKey] ? -1 : a[sortKey] < b[sortKey] ? 1 : 0;
        //        else
        //            return a[sortKey] < b[sortKey] ? -1 : a[sortKey] > b[sortKey] ? 1 : 0;
        //    });
        //    }
        //    else
        //        this.orderedUsers = this.itemsInProduction;
        //},
        //getTheDataTables: function (func) {
        //    $.get('/production/GetAtributteListsForFilter', result => {
        //        this.filterLists.Materials = result.material;
        //        this.filterLists.Colors = result.colors;
        //        this.filterLists.Sizes = result.sizes;
        //        this.filterLists.BodyStyles = result.bodyStyles;
        //        this.filterLists.Sleeves = result.sleeves;
        //        this.filterLists.Departments = result.departments;

        //        this.filterListsSeason.Materials = result.material;
        //        this.filterListsSeason.Colors = result.colors;
        //        this.filterListsSeason.Sizes = result.sizes;
        //        this.filterListsSeason.BodyStyles = result.bodyStyles;
        //        this.filterListsSeason.Sleeves = result.sleeves;
        //        this.filterListsSeason.Departments = result.departments;
        //        console.log(result);
        //        if (func) func();

        //    });
        //},
        //applyFilter: function () {
        //    this.filterApliedList = this.itemsInProduction.filter(element => {
        //        return this.filterdLists.Departments.map(x => x.Id).includes(element.DepartmentId) &&
        //            this.filterdLists.Materials.map(x => x.Id).includes(element.MaterialId)  &&
        //            this.filterdLists.BodyStyles.map(x => x.Id).includes(element.BodyStyleId) &&
        //            this.filterdLists.Sleeves.map(x => x.Id).includes(element.SleeveId) &&
        //            this.filterdLists.Sizes.map(x => x.Id).includes(element.SizeId) &&
        //            this.filterdLists.Colors.map(x => x.Id).includes(element.ColorId);
        //    });
        //},
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
        //Items Production
        filterdLists: function () {
            let ret = {};

            for (k in this.productionItems.filterLists) {
                ret[k] = this.productionItems.filterLists[k] ? this.productionItems.filterLists[k].filter(x => x.Selected).length ? this.productionItems.filterLists[k].filter(x => x.Selected) : this.productionItems.filterLists[k] : [];
            }
            return ret;
        },
        countMsg: function () {
            let filteredItems = {};
            for (k in this.productionItems.filterLists) {
                filteredItems[k] = this.productionItems.filterLists[k] ? this.productionItems.filterLists[k].filter(x => x.Selected) : null;
            }
            let ret = {};
            for (k in filteredItems) {
                ret[k] = filteredItems[k] ?
                    filteredItems[k].length === 0 ? "" :
                        filteredItems[k].length === this.productionItems.filterLists[k].length ? "All selected" : filteredItems[k].length <= 5 ? filteredItems[k].map(x => x.Name).join(', ') :
                            `${filteredItems[k].length} selected` : null;
            }
            return ret;
        },
        orderedArray: function () {
            if (this.searchFileredItems.length && this.productionItems.sortKey) {
                var sortKey = this.productionItems.sortKey;
                var minus = this.productionItems.descSortKey;

                return this.searchFileredItems.sort((a, b) => {
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
            if (this.filterApliedList) {
               
                return this.filterApliedList.length ? this.filterApliedList.filter(element => {
                    return element.SKU.match(this.productionItems.itemSearch.toUpperCase()) || element.Id.toString().match(this.productionItems.itemSearch.toUpperCase()); 
                })
                    : [];
                
            }
            else if (this.productionItems.items.length) {
                return this.productionItems.items.filter(element => {
                    return element.SKU.match(this.productionItems.itemSearch.toUpperCase()) || element.Id.toString().match(this.productionItems.itemSearch.toUpperCase());
                });
            } else {
                return [];
            }
        },
        filterApliedList: function () {
            return this.productionItems.items.filter(element => {
                return this.filterdLists.Departments.map(x => x.Id).includes(element.DepartmentId) &&
                   this.filterdLists.Materials.map(x => x.Id).includes(element.MaterialId) &&
                   this.filterdLists.BodyStyles.map(x => x.Id).includes(element.BodyStyleId) &&
                   this.filterdLists.Sleeves.map(x => x.Id).includes(element.SleeveId) &&
                   this.filterdLists.Sizes.map(x => x.Id).includes(element.SizeId) &&
                   this.filterdLists.Colors.map(x => x.Id).includes(element.ColorId);
           });
        },
        //items in season
        filterdListsSeason: function () {
            let ret = {};

            for (k in this.seasonItems.filterLists) {
                ret[k] = this.seasonItems.filterLists[k] ? this.seasonItems.filterLists[k].filter(x => x.Selected).length ? this.seasonItems.filterLists[k].filter(x => x.Selected) : this.seasonItems.filterLists[k] : [];
            }
            return ret;
            },
        countMsgSeason: function () {
            let filteredItems = {};
            for (k in this.seasonItems.filterLists) {
                filteredItems[k] = this.seasonItems.filterLists[k] ? this.seasonItems.filterLists[k].filter(x => x.Selected) : null;
            }
            let ret = {};
            for (k in filteredItems) {
                ret[k] = filteredItems[k] ?
                    filteredItems[k].length === 0 ? "" :
                        filteredItems[k].length === this.seasonItems.filterLists[k].length ? "All selected" : filteredItems[k].length <= 5 ? filteredItems[k].map(x => x.Name).join(', ') :
                            `${filteredItems[k].length} selected` : null;
            }
            return ret;
        },
        orderedArraySeason: function () {
            if (this.searchFileredItemsSeason.length && this.seasonItems.sortKey) {
                var sortKey = this.seasonItems.sortKey;
                var minus = this.seasonItems.descSortKey;

                return this.searchFileredItemsSeason.sort((a, b) => {
                    if (minus)
                        return a[sortKey] > b[sortKey] ? -1 : a[sortKey] < b[sortKey] ? 1 : 0;
                    else
                        return a[sortKey] < b[sortKey] ? -1 : a[sortKey] > b[sortKey] ? 1 : 0;
                });
            }
            else
                return this.searchFileredItemsSeason;
        },
        searchFileredItemsSeason: function () {
            if (this.filterApliedListSeason) {

                return this.filterApliedListSeason.length ? this.filterApliedListSeason.filter(element => {
                    return element.SKU.match(this.seasonItems.itemSearch.toUpperCase()) || element.Id.toString().match(this.seasonItems.itemSearch.toUpperCase());
                })
                    : [];

            }
            else if (this.seasonItems.items.length) {
                return this.seasonItems.items.filter(element => {
                    return element.SKU.match(this.seasonItems.itemSearch.toUpperCase()) || element.Id.toString().match(this.seasonItems.itemSearch.toUpperCase());
                });
            } else {
                return [];
            }
        },
        filterApliedListSeason: function () {
            return this.seasonItems.items.filter(element => {
                return this.filterdListsSeason.Departments.map(x => x.Id).includes(element.DepartmentId) &&
                    this.filterdListsSeason.Materials.map(x => x.Id).includes(element.MaterialId) &&
                    this.filterdListsSeason.BodyStyles.map(x => x.Id).includes(element.BodyStyleId) &&
                    this.filterdListsSeason.Sleeves.map(x => x.Id).includes(element.SleeveId) &&
                    this.filterdListsSeason.Sizes.map(x => x.Id).includes(element.SizeId) &&
                    this.filterdListsSeason.Colors.map(x => x.Id).includes(element.ColorId);
            });
        },
        applyBtnDisable: function () {
            if (this.filterdLists) {
                return !Object.values(this.filterdLists).every(x => x ? x.length > 0 : false);
            }
            else return false;
        },
        selectedMonth: function () {
            var detailMonths = jQuery.extend(true, {}, this.detailMonths);
            return detailMonths.options.find(a => a.value === detailMonths.selected);
            
        },
        detailsArray: function () {
            if (this.details.typeOfDetails === 'seasonDefalt') {
                return this.seasonItems.details;
            }
            else if (this.details.typeOfDetails === 'moreDetails') {
                return this.details.moreDetails;
            }
            else {
                return null;
            }
        }
        //orderedArray: function () {
        //    if (this.searchFileredItems.length && this.itemsInProductionSortKey) {
        //        var sortKey = this.itemsInProductionSortKey;
        //        var minus = this.itemsInProductionDescSortKey;

        //        return this.searchFileredItems.sort((a, b) => {
        //            if (minus)
        //                return a[sortKey] > b[sortKey] ? -1 : a[sortKey] < b[sortKey] ? 1 : 0;
        //            else
        //                return a[sortKey] < b[sortKey] ? -1 : a[sortKey] > b[sortKey] ? 1 : 0;
        //        });
        //    }
        //    else
        //        return this.searchFileredItems;
        //},
        
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