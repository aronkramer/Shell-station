new Vue({
    el: '#app',
    mounted: function () {
        this.getAllColors(this.rerender);
        this.getAllMatreial(this.rerender);
        

    },
    data: {
        materials: [],
        colors:[]
    },
    methods: {
        getAllColors: function (func) {
            $.get("/fabric/GetAllColors", colors => {
                this.colors = colors;
                func();
            });
        },
        getAllMatreial: function (func) {
            $.get("/fabric/GetAllMaterils", mat => {
                if (mat != null) {
                    this.materials = mat;
                }
                $('.selectpicker').selectpicker('render');
                this.matrerials = [{ id: 0, name: 'enter a new matreial' }];
                func();
            });
        },
        rerender: function () {
            $('.selectpicker').selectpicker('render');
        }
    }

})