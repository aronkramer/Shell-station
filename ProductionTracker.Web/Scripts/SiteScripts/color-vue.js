var app = new Vue({
    el: '#app',
    mounted: function () {
        this.getColors();
    },
    data: {
        formColors: [{ Id: null, Name: null }],
        listOfAllColors: []
    },
    methods: {
        addRowtoColorForm: function () {
            this.formColors.push({ Id: null, Name: null })
        },
        submitColors: function () {
            $.post("/item/AddColors", { colors: this.formColors }, () => {
                if (func) {
                    func()
                }
            })
        },
        getColors: function (func) {
            var x = $.get("/item/GetColors", result => {
                this.listOfAllColors = result.map(r => {
                    r.Edit = false;
                    r.NameCopy = null;
                    return r;
                });
            })
            console.log(x.responseJSON);
        },
        updateExistingColor: function (index) {

            var color = this.listOfAllColors[index];
            //{ color: { Id: color.Id, Name: color.Name } }
            $.post('/item/EditColors', { ColorId: color.Id, ColorName: color.Name }, () => {
                this.listOfAllColors[index].NameCopy = listOfAllColors.Name;
            });

        },
        deleteAnExistingItem: function (index) {
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
                    var row = this.listOfAllColors[index];
                    $.post('/item/DeleteColors', { ColorId: row.Id }, () => {
                        this.listOfAllColors.splice(index,1);
                    });
                });
           
        },
    }
})