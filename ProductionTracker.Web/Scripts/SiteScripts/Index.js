new Vue({
    el: '#app',
    mounted: function () {
        $(function () {
            $('#myTab a:first').tab('show')
        });
        this.pageHeader = 'Items In Production';
        this.tableHeaders = ['Id', 'SKU', 'Last Production Date', 'Items Not Received', 'Percentage Filled','actions'];
        this.loadSkus();
        this.getActivitysByItem(1);

    },
    data: {
        info: [],
        tableHeaders: [],
        pageHeader: 'Hi',
        itemActivty: []

    },
    methods: {
        loadSkus: function () {
            $.get("/home/GetAllItemWithDetails", SKU => {
                this.info = SKU;
            });
        },
        details: function (event) {
            //var id = event.target.id;
            //this.getActivitysByItem(id);
           
            //this.getActivitysByItem(id);
            //setInterval(5000);
            //this.loadSkus();
            $("#detail-modal").modal();
        },
        getActivitysByItem: function (id) {
            $.get("/home/GetAllActivityOfAItem", { Id: id }, function (activity) {

                this.itemActivty = Object.assign([], activity);
                console.log(activity);
                console.log(this.itemActivty);
            });
        },

    },
});
    