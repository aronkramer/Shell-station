var app = new Vue({
    el: '#app',
    mounted: function () {
        this.getMarkerCats();
        this.getTheDataTables();
    },
    data: {
        markerCatergories: [],
        bodyStyles: [],
        sleeves: [],
        departments: [],
        sizes: []
        
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
                    //r.DefaltMarker.CreatedOn = moment(r.DefaltMarker.CreatedOn).format('MM/DD/YYYY');
                    r.Markers = r.Markers.map(m => {
                        //made the size text based on the defalt marker
                        m.SizeText = this.markerSizeText(m.MarkerDetails);
                        //for editing a place to copy and a place to have a editing
                        m.Copy = null;
                        m.Edit = false;
                        //new marker object set to null will check if its null before opening the form
                        m.NewMarker = null;

                        return m;
                    });
                    r.DetailsOpened = false;

                    return r;

                });
            });
        },
        edit: function (markerCatIndex, MarkerIndex) {
            var marker = this.markerCatergories[markerCatIndex].Markers[MarkerIndex];
            if (!marker.Edit) {
                this.editCancelMarker(true, markerCatIndex, MarkerIndex);
            }
        },
        cancel: function (markerCatIndex, MarkerIndex) {
            this.editCancelMarker(false, markerCatIndex, MarkerIndex);
        },
        editCancelMarker: function (edit, markerCatIndex, MarkerIndex) {
            var marker = this.markerCatergories[markerCatIndex].Markers[MarkerIndex];
            
            marker.Edit = edit;
            if (edit) {
                marker.Copy = jQuery.extend(true, {}, marker);
            }
            else {
                marker.Length = marker.Copy.Length;
                marker.PercentWaste = marker.Copy.PercentWaste;
            }
            this.markerCatergories[markerCatIndex].Markers.splice(MarkerIndex, 1, marker);
        },
        updateMarker: function (markerCatIndex, MarkerIndex) {
            var marker = this.markerCatergories[markerCatIndex].Markers[MarkerIndex];
            this.updateMarkerInDb({ Id: marker.Id, Length: marker.Length, PercentWaste: marker.PercentWaste },
                () => {
                    marker.Edit = false;
                    this.markerCatergories[markerCatIndex].Markers.splice(MarkerIndex, 1, marker);

                });
        },
        deleteMarker: function (markerCatIndex, MarkerIndex) {
            if (confirm("Your sure you want to delete this?!")) {
                var marker = this.markerCatergories[markerCatIndex].Markers[MarkerIndex];
                this.updateMarkerInDb({ Id: marker.Id, Deleted: true }, () => {
                    this.markerCatergories[markerCatIndex].Markers.splice(MarkerIndex, 1);

                });
            }
        },
        updateMarkerInDb: function (marker, func) {
            $.post('/marker/UpdateMarker', { marker }, () => { if (func) func(); });
        },
        delteMarkerCat: function (index) {
            if (confirm("Your sure you want to delete this?!")) {
                var markerCat = this.markerCatergories[index];
                this.updateMarkerCat({ Id: markerCat.Id, Deleted: true }, () => {
                    this.markerCatergories.splice(index, 1);

                });
            }
        },
        makeMarkerDefalt: function (index,markerId) {
            var markerCat = this.markerCatergories[index];
            this.updateMarkerCat({ Id: markerCat.Id, DefaltMarkerId: markerId }, () => {
                markerCat.DefaltMarkerId = markerId;
                markerCat.Marker = this.markerCatergories[index].Markers.find(m => m.Id === markerId);
                this.markerCatergories.splice(index, 1, markerCat);

            });
        },
        updateMarkerCat: function (markerCategory, func) {
            $.post('/marker/UpdateMarkerCat', { markerCategory }, () => { if (func) func(); });
        },
        openMarkerForm: function (index) {
            var markerCat = this.markerCatergories[index];
            markerCat.NewMarker = { Id: null, Length: null, PercentWaste: null, MarkerDetails: [{ SizeId: null, AmountPerLayer: null, Name: null }] };
            this.markerCatergories.splice(index, 1, markerCat);
        },
        getTheDataTables: function (func) {
            $.get('/production/GetAtributteListsForFilter', result => {

                this.bodyStyles  = result.bodyStyles;
                this.sleeves     = result.sleeves;
                this.departments = result.departments;
                this.sizes       = result.sizes;
                console.log(result);
                if (func) func();
            });
        },
        


    },
    filters: {
        percentage: function (value) {
            return value ? (value * 100).toFixed(2) + '%' : '0%';
        },
        toShortDateString: function (value) {
            return moment(value).format('MM/DD/YYYY');
        }
    }
});