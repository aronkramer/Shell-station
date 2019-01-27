var app = new Vue({
    el: '#pp-app',
    mounted: function () {
        this.addSkuRows(1);
        this.getSkus();
        this.getProdCatsList();
        this.getTheDataTables();
        this.makeTheItems();
    },
    data: {
        hello: 'hi',
        items: [],
        rowIncremntNumber: 1,
        skus: null,
        prodCats: null,
        productionCat: null,
        productionCatYear: null,
        productionWizard: {
            marker: null,
            material: null,
            colors: [{ Name: 'black' }, { Name: 'white' }, { Name: 'red' }, { Name: 'offwhite'.toUpperCase() }],
            sizes: [{ Name: 's' }, { Name: 'm' }, { Name: 'l' }, { Name: 'xl' }],
            items: [ ]
        },
        validationLists: {
            skus: null,
            prodCats: null,
            materials:null,
            colors: null,
            sizes: null,
            markers: null
            }
    },
    methods: {
        addSkuRows: function (number) {
            for (var i = 0; i < number; i++)
                this.items.push({ Id: null , SKU: '', Quantity:null  });
        },
        getSkus: function () {
            $.get("/production/GetSKUsList", result => {
                this.validationLists.skus = result;
            });
        },
        getIdOfSku: function (index) {
            var allskus = JSON.parse(JSON.stringify(this.validationLists.skus));
            var thisItem = this.items[index];
            thisItem.Id = allskus.filter(function (item) {
                return item.SKU === thisItem.SKU;
            })[0].Id;
            this.items[index] = thisItem;
        },
        removeSkuLine: function (index) {
            this.items.splice(index, 1);
        },
        getProdCatsList: function () {
            $.get("/production/GetProductionCats", result => {
                this.validationLists.prodCats = result;
            });
        },
        getTheDataTables: function (func) {
            $.get('/production/GetValidatoinLists', result => {
                this.validationLists.materials = result.material;
                this.validationLists.colors = result.colors;
                this.validationLists.sizes = result.sizes;
                this.validationLists.markers = result.markers;
                console.log(result);
                if (func) func();
                
            });
        },
        addColor: function(){
            var color = { Name: '' };
            var sizes = this.productionWizard.sizes;
            
            sizes.forEach(function (Size) {
                var items = [];
                items.push({ Size, color, Quantity: 0 });
            });
            this.productionWizard.items = items;
            this.productionWizard.colors.push(color);
        },
        addSize: function () {
            var color = { Name: '' };
            var sizes = this.productionWizard.sizes;

            sizes.forEach(function (Size) {
                var items = [];
                items.push({ Size, color, Quantity: 0 });
            });
            this.productionWizard.items = items;
            this.productionWizard.colors.push(color);
        },
        makeTheItems: function () {
            var colors = this.productionWizard.colors;
            var sizes = this.productionWizard.sizes;
            var items=[];
            sizes.forEach(function (Size) {
                colors.forEach(function (Color) {
                    items.push({ Size, Color, Quantity: 0 });
                });
            });
            this.productionWizard.items = items;
        }
    }

});