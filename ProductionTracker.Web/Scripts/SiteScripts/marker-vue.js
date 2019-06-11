var app = new Vue({
    el: '#app',
    mounted: function () {
        this.getMarkerCats();
    },
    data: {
        markerCatergories:[]
    },
    methods: {
        markerSizeText: function (markerDetails) {
            let text ='';
            for (let md in markerDetails) {
                text += `'${markerDetails[md].Size.Name}' -  ${markerDetails[md].AmountPerLayer}`; 
                if (md < markerDetails.length - 1) text += ' , ';
            }
            return text;
        },
        getMarkerCats: function () {
            $.get("/marker/GetMarkeCatsWithMarkers", result => {
                this.markerCatergories = result.map(r => {
                    r.DefaltMarker.SizeText = this.markerSizeText(r.DefaltMarker.MarkerDetails);
                    r.DefaltMarker.CreatedOn = moment(r.DefaltMarker.CreatedOn).format('MM/DD/YYYY');
                    r.Markers = r.Markers.map(m => {
                        m.SizeText = this.markerSizeText(m.MarkerDetails);
                        m.CreatedOn = moment(m.CreatedOn).format('MM/DD/YYYY');
                        return m;
                    });
                    r.DetailsOpened = false;

                    return r;

                });
            });
        }


    }
});