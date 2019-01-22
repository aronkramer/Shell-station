var app = new Vue({
    el: '#pp-app',
    mounted: function () {
        this.addSkuRows(1);
        this.getSkus();
        this.getProdCatsList();
        this.getTheDataTables();
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
            colors: [{ x: 'black' }, { x: 'white' }, { x: 'red' }, { x: 'offwhite'.toUpperCase() }],
            sizes: [{ x: 's' }, { x: 'm' }, { x: 'l' }, { x: 'xl'.toUpperCase() }],
            quatititys: [
                [{ x: 100 }, { x: 5000 }, { x: 3000 }, { x: 200 }],
                [{ x: 50 }, { x: 65 }, { x: 200 }, { x: 500 }],
                [{ x: null }, { x: null }, { x: null }, { x: null }],
                [{ x: null }, { x: null }, { x: null }, { x: null }]
            ]
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
            var allskus = JSON.parse(JSON.stringify(this.skus));
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
    }

});