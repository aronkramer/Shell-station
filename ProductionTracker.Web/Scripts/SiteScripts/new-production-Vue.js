const app = new Vue({
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
        errors: null,
        finalProduction: null,
        colors: [],
        materials: [],
        sizes: [],
        markers: [],
        formVaild: false,
        packagingOptions: [
            { text: 'BOX', value: 0 },
            { text: 'HANG', value: 1 }
        ]
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
            this.updateLotNumbers();
            if (this.production.Markers.length > 0) {
                this.vaidateMarker(0);
            }
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
            this.vaidateMarker(marIndex);
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
                success: result => {
                    this.getTheDataTables(() => {
                    this.production = result.production;
                    this.production.Markers.forEach(function (element) {
                        element.errors = [];
                    });

                    this.errors = result.errors;
                    this.production.Date = this.getDateInputFormat(result.production.Date.replace(/\/Date\((-?\d+)\)\//, '$1'));
                    console.log(this.production.Date);
                    
                    if (this.production.Markers.length > 0) {
                        this.vaidateMarker(0);
                    }
                    });

                },



            });
        },
        addMarker: function () {
            //this.production.Markers.push({ Name: "", Size: "", Sizes: [{ SizeId: 0, AmountPerLayer: 0 }], "ColorMaterials": [{ Color: "", Material: "", Layers: 0 }], LotNumber : 0 });
            var index = this.production.Markers.push({ Sizes: [], "ColorMaterials": [], "errors": [] });
            this.updateLotNumbers();
            //var lastMarkerIndex = production.Markers.length - 1;
            this.vaidateMarker(index - 1);
        },
        newProd: function () {
            $.get('/production/GetLastLotNUmber', result => {
                this.getTheDataTables();
                this.production = { Date: "", LastLotNumber: result, Markers: [], Name: "" };
                var date = new Date();
                date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
                var datestr = date.toISOString().substring(0, 10);
                this.production.Date = datestr;
                this.addMarker();
            });
        },
        removeSize: function (event, marIndex, Index) {
            this.production.Markers[marIndex].Sizes.splice(Index, 1);
            if (this.production.Markers[marIndex].Sizes.length === 1) {
                this.production.Markers[marIndex].Sizes[0].AmountPerLayer = 6;
            }
        },
        addSize: function (event, marIndex) {
            //for (var i = 0; i < 10; i++)
            if (this.production.Markers[marIndex].Sizes.length === 0)
                this.production.Markers[marIndex].Sizes.push({ AmountPerLayer: 6 });
            //else if (this.production.Markers[marIndex].Sizes.length === 1) {
            //    //this.production.Markers[marIndex].Sizes[0].AmountPerLayer = 1;
            //    this.production.Markers[marIndex].Sizes.push({});
            //}
            else
                this.production.Markers[marIndex].Sizes.push({});
            this.vaidateMarker(marIndex);

        },
        getDateInputFormat: function (date) {
            date = new Date(parseInt(date));
            day = fixDigit(date.getDate());
            month = fixDigit(date.getMonth() + 1);
            year = date.getFullYear();
            return [year, month, day].join('-');
        },
        vaidateMarker: function (markerIndex) {
            console.log("vaildate func");
            var marker = this.production.Markers[markerIndex];
            marker.errors = [];
            if (marker.Name) {
                marker.Name = marker.Name.toUpperCase();
            }
            if (!this.markers.includes(marker.Name)) {

                marker.errors.push(`Marker:${marker.Name} was not found`);
            }
            marker.Sizes.forEach(element => {
                if (element.Name) {
                    element.Name = element.Name.toUpperCase();
                }
                if (!this.sizes.some(i => i.Name === element.Name)) {
                    marker.errors.push(`Size:${element.Name} was not found`);
                }
                else {
                    var allSizes = JSON.parse(JSON.stringify(this.sizes));
                    element.SizeId = allSizes.filter(function (item) {
                        return item.Name === element.Name;
                    })[0].Id;
                    //this.sizes = allSizes;
                }
                if (!element.AmountPerLayer) {
                    marker.errors.push(`Size:${element.Name} has no layers count! Either add a number or remove`);
                }
            });
            marker.ColorMaterials.forEach(element => {
                if (element.Color) {
                    element.Color.toUpperCase();
                }
                if (element.Material) {
                    element.Material.toUpperCase();
                }
                if (!this.colors.includes(element.Color)) {
                    marker.errors.push(`Color:${element.Color} was not found`);
                }
                if (!this.materials.includes(element.Material)) {
                    marker.errors.push(`Material:${element.Material} was not found`);
                }
                if (!element.Layers) {
                    marker.errors.push(`Color ${element.Color} & Material ${element.Material} has no layers! Either add a number or remove`);
                }
                if (!this.packagingOptions.some(i => i.value === element.Packaging)) {
                    marker.errors.push(`Color ${element.Color} & Material ${element.Material} has no packaging! Either choose one or remove`);
                }
            });
            
            this.production.Markers[markerIndex] = marker;
            var hasErros = this.production.Markers.some(function (i) {
                return i.errors.length > 0;
            });
            this.formVaild = !hasErros;

        },

        editAllSizes: function (event, markerIndex) {

            this.production.Markers[markerIndex].AllSizes = false;
        },
        saveProdToItems: function () {
            //var date = new Date(parseInt(this.production.Date));
            //this.production.Date = date.toJSON();
            this.updateLotNumbers();
            $.post('/production/ConvertCTToItems', { production: this.production }, result => {
                this.finalProduction = result.prodItems;
                this.finalProduction.Date = this.getDateInputFormat(result.prodItems.Date.replace(/\/Date\((-?\d+)\)\//, '$1'));
                this.errors = result.errors;
            });
        },
        submitProduction: function () {
            
            var finalprod = {
                Date: this.finalProduction.Date,
                CuttingInstructions: this.finalProduction.CuttingInstructions.map(function (ct) {
                    return {
                        Marker: ct.Marker,
                        LotNumber: ct.LotNumber,
                        Items: ct.Items.map(i => {
                            return {
                                Id: i.Id,
                                ItemId: i.ItemId,
                                Quantity: i.Quantity,
                                Packaging: i.Packaging
                            };
                        })
                    };
                })
            };
            $.post('/production/SubmitProduction', { production: finalprod }, () => window.location = '/');
        },
        getTheDataTables: function (func) {
            $.get('/production/GetValidatoinLists', result => {
                this.materials = result.material;
                this.colors = result.colors;
                this.sizes = result.sizes;
                this.markers = result.markers;
                console.log(result);
                if(func)func();
                //Vue.nextTick(function () {
                //    console.log(app.$el.textContent);
                //}
                //);
            });
        },
        getSumOfLayersPerMarker: function (marker) {
            return marker.ColorMaterials.map(function (c) { return c.Layers; }).reduce((a, b) => parseInt(a) + parseInt(b), 0);
        },
        updateLotNumbers: function () {
            this.production.Markers.forEach((element, index) => {
                element.LotNumber = this.production.LastLotNumber + index + 1;
            });
        }

    },
    computed: {
        productoinHide: function () {
            return this.production === null;
        },
        //formValid: function() {
        //    //var temp = this.production.Markers;
        //    var hasErros = this.production.Markers.some(function (i) {
        //        return i.errors.length > 0;
        //    });
        //    return !hasErros;
        //}
    },
    watch: {
        markerHasError: function (markerIndex) {
            if (this.production.Markers[markerIndex].errors) {
                if (this.production.Markers[markerIndex].errors['marker']) {
                    return true;
                }
                else
                    return false;
            }

            else
                return false;
        }
    }

});
function fixDigit(val) {
    return val.toString().length === 1 ? "0" + val : val;
}
function markerExist(markerName, func) {
    return $.get('/production/GetMarker', { markerName }, function (result) {
        
        func(result);
    });
}
