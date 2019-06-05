var app = new Vue({
    el: '#app',
    data: {
        formColors: [{ Id: null, Name: null }],
        test:'hello'
    },
    methods: {
        addRowtoColorForm: function () {
            this.formColors.push({ Id: null, Name: null })
        }
    }
})