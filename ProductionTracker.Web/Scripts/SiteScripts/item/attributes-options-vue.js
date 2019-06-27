var appnew = new Vue({
    el: '#app-other-edit-options',
    mounted: function () {
        console.log('kdjsuhfklj');
    },
    data: {
        test:'hello world'
    },
    methods: {
        addRowtoColorForm: function () {
            this.formColors.push({ Id: null, Name: null })
        },
    
    }
})