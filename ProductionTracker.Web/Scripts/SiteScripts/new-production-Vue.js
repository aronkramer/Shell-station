new Vue({
    el: '#prodApp',
    //components: {
    //    Marker
    //},
    mounted: function () {
        
        //this.getProductionInProgress(result =>{
        //    this.production = result;
        //    if (this.production !== {}) {
        //        this.isProduction = true;
        //    }
        //});
    },
    data: {
        hi: 'sdjfl;jas;lkj',
        production: null,
        isProduction: false,
        file: null,
        errors: null
    },
    methods: {
        getProductionInProgress: function (func) {
            $.get("/production/GetProductionInProgress", result => {
                func(result);
            });
        },
        loadProd: function () {
            this.getProductionInProgress(result => {
                this.production = result;
                if (this.production !== {}) {
                    this.isProduction = true;
                }
                console.log(this.production);
            });
        },
        removeMarker: function (index) {
            this.production.Markers.splice(index, 1);
        },
        removeColorMat: function (event, marIndex, Index) {
            console.log(Index);
            console.log(this.production.Markers[marIndex]);
            console.log(marIndex);
            //$event.target.markerIndex
            //$event.target.index
            this.production.Markers[marIndex].ColorMaterials.splice(Index, 1);
        },
        addColorMatLine: function (event, marIndex) {
            //for (var i = 0; i < 10; i++)
                this.production.Markers[marIndex].ColorMaterials.push({});
        },
        fileUpload: function () {
            //this.file = file[0];
            var form_data = new FormData();
            form_data.append('cuttingTicket', this.file);

            $.ajax({
                type: 'POST',
                url: '/production/NewProduction',
                processData: false,
                contentType: false,
                async: true,
                cache: false,
                data: form_data,
                success:  result => {
                    this.production = result.production;
                    this.errors = result.errors;
                    this.production.Date = this.getDateInputFormat(result.production.Date.replace(/\/Date\((-?\d+)\)\//, '$1'));
                    console.log(this.production.Date);
                }
            
                
            });
        },
        addMarker: function () {
            //this.production.Markers.push({ Name: "", Size: "", Sizes: [{ SizeId: 0, AmountPerLayer: 0 }], "ColorMaterials": [{ Color: "", Material: "", Layers: 0 }], LotNumber : 0 });
             this.production.Markers.push({ Sizes: [], "ColorMaterials": []});
        },
        removeSize: function (event,marIndex,Index) {
            this.production.Markers[marIndex].Sizes.splice(Index, 1);
        },
        addSize: function (event, marIndex) {
            //for (var i = 0; i < 10; i++)
            this.production.Markers[marIndex].Sizes.push({});
        },
        getDateInputFormat: function (date) {
            date = new Date(parseInt(date));
            day = fixDigit(date.getDate());
            month = fixDigit(date.getMonth() + 1);
            year = date.getFullYear();
            return [year, month, day].join('-');
        },
        vaidateMarker: function (markerIndex) {

        }
    },
    computed: {
        productoinHide: function () {
            return this.production === null;
        }
    }

})
function fixDigit(val) {
    return val.toString().length === 1 ? "0" + val : val;
}