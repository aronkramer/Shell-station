var app = new Vue({
    el: '#app',
    mounted: function(){
        console.log('kjshdflkjah');
        this.getColors();
    },
    data: {
        formColors: [{ Id: null, Name: null }],
        listOfAllColors:[]
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

            //$.get("/item/getcolors", {colors: this.hello})

        },
        getColors: function (func) {
             var x = $.get("/item/GetColors", result => {
                 this.listOfAllColors = result.map(r => {
                     r.Edit = false;
                     return r;
                 });
            })
            console.log('kgjh');
            console.log(x.responseJSON);
        }
    }
})