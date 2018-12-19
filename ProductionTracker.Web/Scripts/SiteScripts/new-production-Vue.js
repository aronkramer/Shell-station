new Vue({
    el: '#prodApp',
    //components: {
    //    Marker
    //},
    mounted: function () {
        
        this.getProductionInProgress(result =>{
            this.production = result;
            if (this.production !== {}) {
                this.isProduction = true;
            }
        });
    },
    data: {
        hi: 'sdjfl;jas;lkj',
        production: null,
        isProduction: false,
        file: ''
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
            for (var i = 0; i < 10; i++) this.production.Markers[marIndex].ColorMaterials.push({});
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
                    this.production = result;
                    console.log(result);
                }
            
                
            });
        }
        

    },
    computed: {
        productoinHide: function () {
            return this.production === null;
        }
    }

})