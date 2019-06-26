var app = new Vue({
    el: '#app',
    mounted: function () {
        this.getMarkerCats();
        this.getTheDataTables();
    },
    data: {
        markerCatergories: [],
        newMarkerCat: null,
        bodyStyles: [],
        sleeves: [],
        departments: [],
        sizes: [],
    },
    methods: {
        markerSizeText: function (markerDetails) {
            let text = '';
            for (let md in markerDetails) {
                text += `'${markerDetails[md].Size.Name}' -  ${markerDetails[md].AmountPerLayer}`;
                if (md < markerDetails.length - 1) text += ' , ';
            }
            return text;
        },
        getMarkerCats: function () {
            $.get("/marker/GetMarkeCatsWithMarkers", result => {
                this.markerCatergories = result.map(r => {
                    return this.markerCatFormat(r);
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
        makeMarkerDefalt: function (index, markerId) {
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
            if (index !== null) {
                var markerCat = this.markerCatergories[index];
                markerCat.NewMarker = { Id: null, Length: null, PercentWaste: null, MarkerDetails: [{}], MakeDefalt: false };
                this.markerCatergories.splice(index, 1, markerCat);
            }
            else {
                this.$set(this.newMarkerCat, 'NewMarker',
                    { Id: null, Length: null, PercentWaste: null, MarkerDetails: [{}], MakeDefalt: true }
                );
            }
        },
        getTheDataTables: function (func) {
            $.get('/production/GetAtributteListsForFilter', result => {
                this.bodyStyles = result.bodyStyles;
                this.sleeves = result.sleeves;
                this.departments = result.departments;
                this.sizes = result.sizes;
                console.log(result);
                if (func) func();
            });
        },
        addSize: function (index) {
            if (index === null) {
                this.newMarkerCat.NewMarker.MarkerDetails.push({});
            }
            else {
                this.markerCatergories[index].NewMarker.MarkerDetails.push({});
            }
        },
        removeSize: function (index, sizeIndex) {
            if (index === null) {
                this.newMarkerCat.NewMarker.MarkerDetails.splice(sizeIndex, 1);
            }
            else {
                this.markerCatergories[index].NewMarker.MarkerDetails.splice(sizeIndex, 1);
            }
        },
        addNewMarker: function (index) {
            var marCat = this.markerCatergories[index];
            var newMar = marCat.NewMarker;
            newMar.MarkerCatId = marCat.Id;
            this.addMarkerDb(newMar);
        },
        addMarkerDb: function (marker, func) {

            $.post('marker/AddNewMarkerGetResult',
                { marker: { MarkerCatId: marker.MarkerCatId, Length: marker.Length, PercentWaste: marker.PercentWaste, MarkerDetails: marker.MarkerDetails }, makeDefalt: marker.MakeDefalt },
                result => {
                    var index = this.markerCatergories.findIndex(r => r.Id = result.Id);
                    this.markerCatergories.splice(index, 1, this.markerCatFormat(result));
                });
        },
        markerCatFormat: function (r) {
            r.DefaltMarker ? r.DefaltMarker.SizeText = this.markerSizeText(r.DefaltMarker.MarkerDetails) : r.DefaltMarker = { SizeText:'-----'};
            //r.DefaltMarker.CreatedOn = moment(r.DefaltMarker.CreatedOn).format('MM/DD/YYYY');
            r.Markers = r.Markers.map(m => {
                //made the size text based on the defalt marker
                m.SizeText = this.markerSizeText(m.MarkerDetails);
                //for editing a place to copy and a place to have a editing
                m.Copy = null;
                m.Edit = false;
                //new marker object set to null will check if its null before opening the form


                return m;
            });
            r.DetailsOpened = false;
            r.NewMarker = null;
            return r;
        },
        newMrkerCatForm: function () {
            this.newMarkerCat = {
                Id: null,
                Name: null,
                BodyStyleId: null,
                BodyStyle: {
                    "Id": null, "Name": null
                },
                DepartmentId: null,
                Department: {
                    "Id": null, "Name": null
                },
                SleeveId: null,
                Sleeve: {
                    "Id": null, "Name": null
                },
                NewMarker: null
            };
        },
        onChangeDataList: function (item, list) {
            var marCat = this.newMarkerCat;
            marCat[item].Name = marCat[item].Name.toUpperCase();
            marCat[item].Id = list.find(i => i.Name === marCat[item].Name) ? list.find(i => i.Name === marCat[item].Name).Id : null;

            //app.newMarkerCat[item] = Object.assign({}, app.newMarkerCat[item], {
            //    Id: marCat[item].Id,
            //    Name:marCat[item].Name 
            //});
            this.$set(this.newMarkerCat, item,
                {
                    Id: marCat[item].Id,
                    Name: marCat[item].Name
                }
            );
        },
        addNewMarkerCat: function (event) {
            event.target.disabled = true;
            var marCat = this.newMarkerCat;
            var markerCategory = {
                Name: marCat.Name,
                BodyStyleId: marCat.BodyStyle.Id,
                DepartmentId: marCat.Department.Id,
                SleeveId: marCat.Sleeve.Id
            };
            this.addNewMarkerCatDb(markerCategory, marCat.NewMarker);
        },
        addNewMarkerCatDb: function (markerCategory, defaltMarker) {
            $.post('/marker/AddNewMarkerCatGetMarker', { markerCategory, defaltMarker }, result => {
                this.newMarkerCat = null;
                this.markerCatergories.push(this.markerCatFormat(result));
            });
        }
        
        


    },
    filters: {
        percentage: function (value) {
            return value ? (value * 100).toFixed(2) + '%' : '0%';
        },
        toShortDateString: function (value) {
            return moment(value).format('MM/DD/YYYY');
        }

    },
    computed: {
        newMarkerSubmit: function() {
            let cats = this.markerCatergories;
            let ret = [];
            for (k in cats) {
                ret[k] = cats[k].NewMarker ? !cats[k].NewMarker.MarkerDetails.some(md => md.SizeId && md.AmountPerLayer) : true;
            }
            return ret;
        },
        newMarkerCatSubmit: function () {
            var marCat = this.newMarkerCat;
            return marCat.Name &&
                marCat.BodyStyle.Id &&
                marCat.Department.Id &&
                marCat.Sleeve.Id &&
                (marCat.NewMarker ? marCat.NewMarker.MarkerDetails.some(md => md.SizeId && md.AmountPerLayer) : true);
        }
    }
});