var appnew = new Vue({
    el: '#app-other-edit-options',
    mounted: function () {
        console.log(true)
        this.getSleeves();
    },
    data: {
        formColors: [{ Id: null, Name: null }],
        listOfAllSleeves: []
    },
    methods: {
        getSleeves: function () {
            var x = $.get("/item/GetSleeves", result => {
                this.listOfAllSleeves = result.map(r => {
                    
                    return r;
                });
            })
            
            console.log(x.responseJSON)
           
        } 
    }
})