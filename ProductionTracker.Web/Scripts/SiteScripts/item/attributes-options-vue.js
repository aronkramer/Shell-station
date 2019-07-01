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
        listOfAllMaterial: []
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
                    return r;
                });
            });
        },
        getMaterial: function () {
            $.get("/item/GetMaterial", result => {
                this.listOfAllMaterial = result.map(r => {
                    return r;
                });
            });
        }
    }
})