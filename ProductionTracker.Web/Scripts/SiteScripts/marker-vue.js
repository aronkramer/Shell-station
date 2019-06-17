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
                    //r.DefaltMarker.CreatedOn = moment(r.DefaltMarker.CreatedOn).format('MM/DD/YYYY');
                    r.Markers = r.Markers.map(m => {
                        m.SizeText = this.markerSizeText(m.MarkerDetails);
                        //m.CreatedOn = moment(m.CreatedOn).format('MM/DD/YYYY');
                        m.Copy = null;
                        m.Edit = false;
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
        }
        


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