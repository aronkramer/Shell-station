new Vue({
    el: '#edit-options',
    mounted: function () {
        this.getSleeves();
        this.getBodyStyle();
        this.getMaterial();
    },
    data: {
        formColors: [{ Id: null, Name: null }],
        listOfAllSleeves: [],
        listOfAllBodyStyle: [],
        listOfAllMaterial: [],
        listOfAtributes: {
            bodysStyles:[],
            matarials:[],
            Sleeves: []
        }
    },
    methods: {
        getSleeves: function () {
            $.get("/item/GetSleeves", result => {
                this.listOfAllSleeves = result.map(r => {
                    r.Edit = false;
                    r.NameCopy = null;
                    return r;
                });
            })

        },
        getBodyStyle: function () {
            $.get("/item/GetBodyStyle", result => {
                this.listOfAllBodyStyle = result.map(r => {
                    r.Edit = false;
                    r.NameCopy = null;
                    return r;
                });
            });
        },
        getMaterial: function () {
            $.get("/item/GetMaterial", result => {
                this.listOfAllMaterial = result.map(r => {
                    r.Edit = false;
                    r.NameCopy = null;
                    return r;
                });
            });
        },
        updateExistingSleeve: function (index) {
            var item = this.listOfAllSleeves[index];
            $.post("/item/UpdateSleeve", { sleeve: { Id: item.Id, Name: item.Name } });
        },
        updateExistingBodyStyle: function (index) {
            var item = this.listOfAllBodyStyle[index];
            $.post("/item/UpdateBodyStyle", { bodyStyle: { Id: item.Id, Name: item.Name } });
        },
        updateExistingmaterial: function (index) {
            var item = this.listOfAllMaterial[index];
            $.post("/item/UpdateMaterial", { material: { Id: item.Id, Name: item.Name } });
        },
        edit: function (item) {
            if (!item.Edit) {
                item.Edit = !item.Edit;
                item.Copy = jQuery.extend(true, {}, item);
            }
            
        },
        deleteAnExistingItem: function (i, type, index) {
            swal({
                title: "Are you sure you want to delete this?",
                text: "Your will not be able to recover!",
                type: "warning",
                showCancelButton: true,
                confirmButtonClass: "btn-danger",
                confirmButtonText: "Yes, delete it!",
                showLoaderOnConfirm: true,
                closeOnConfirm: true
            },
                () => {
                    $.post(`/item/deleteItem`, { id: i.Id, type }, () => {
                        var listType = this[`listOfAll${type}`];
                        listType.splice(index, 1);
                    })
                }
            );
        },
        addRow: function (type) {
            var listType = this[`listOfAll${type}`];
            listType.push({ Name: "", Edit: true,  });
        },
        removeRow: function (type) {
            var listType = this[`listOfAll${type}`];
            listType.pop();
        },
        cancel: function (object) {
            if (object.Id) {
                object.Edit = false;
                object.Name = object.NameCopy;
            }
        },
        addNewItem: function (object) {
            var item = this[`listOfAll${object}`];
            console.log(item.Name);
        }
    }
})