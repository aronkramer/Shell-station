var app = new Vue({
    el: '#pp-app',
    mounted: function () {
        this.getSkus();
        this.getProdCatsList();
        this.getTheDataTables();
        this.addSkuRows(1);
        //this.makeTheItems();
    },
    data: {
        hello: 'hi',
        items: [],
        rowIncremntNumber: 1,
        errors: [],
        plannedProduction: {
            id: null, productionCat: { Id: null, Name: '' }, productionCatYear: null, copy: {}
        },
        existingItems:[],
        vaildPlannedProduction: false,
        productionCat: { Id: null, Name:'' },
        productionCatYear: null,
        productionWizard: {
            marker: { Name: null },
            material: { Name: null },
            colors: [],
            sizes: [],
            items: [],
            errors:[]
        },
        validationLists: {
            skus: null,
            prodCats: null,
            materials:null,
            colors: null,
            sizes: null,
            markers: null
        },
        formVaild: false
    },
    methods: {
        validateWizard: function () {
            var wizard = this.productionWizard;
            var errors = [];
            //marker
            if (wizard.marker.Name) {
                wizard.marker.Name = wizard.marker.Name.toUpperCase();
            }
            if (!this.validationLists.markers.some(i => i.Name === wizard.marker.Name)) {
                errors.push(`Marker:${wizard.marker.Name} was not found`);
            }
            else {
                const list = JSON.parse(JSON.stringify(this.validationLists.markers));
                wizard.marker = list.filter(function (elment) {
                    return elment.Name === wizard.marker.Name;
                })[0];
            }
            //material
            if (wizard.material.Name) {
                wizard.material.Name = wizard.material.Name.toUpperCase();
            }
            if (!this.validationLists.materials.some(i => i.Name === wizard.material.Name)) {
                errors.push(`Material:${wizard.material.Name} was not found`);
            }
            else {
                const list = JSON.parse(JSON.stringify(this.validationLists.materials));
                wizard.material = list.filter(function (elment) {
                    return elment.Name === wizard.material.Name;
                })[0];
            }
            //colors
            wizard.colors.filter(element => {
                if (element.Name) {
                    element.Name = element.Name.toUpperCase();
                }
                if (!this.validationLists.colors.some(i => i.Name === element.Name)) {
                    errors.push(`Color:${element.Name} was not found`);
                }
                else {
                    const list = JSON.parse(JSON.stringify(this.validationLists.colors));
                    element.Id = list.filter(function (item) {
                        return item.Name === element.Name;
                    })[0].Id;
                }
            });
            //sizes
            wizard.sizes.filter(element => {
                if (element.Name) {
                    element.Name = element.Name.toUpperCase();
                }
                if (!this.validationLists.sizes.some(i => i.Name === element.Name)) {
                    errors.push(`size:${element.Name} was not found`);
                }
                else {
                    const list = JSON.parse(JSON.stringify(this.validationLists.sizes));
                    element.Id = list.filter(function (item) {
                        return item.Name === element.Name;
                    })[0].Id;
                }
            });
            
            if (!wizard.items.some(i => i.Quantity > 0)) {
                errors.push('sorry you need a quantity to submit');
            }
            wizard.errors = errors;
            this.formVaild = errors.length === 0;
            this.productionWizard = wizard;
        },
        validatePlannedProd: function () {
            console.log('validating......');
            var errors = [];
            var items = this.items;
            let prodCat = this.plannedProduction.productionCat;
            if (!this.plannedProduction.productionCatYear || this.plannedProduction.productionCatYear.length !== 4) {
                errors.push('Sorry its not a vaild year');
            }
            if (!prodCat.Name) {
                errors.push('Please enter a production catergory');
            }
            else if (!this.validationLists.prodCats.some(i => i.Name === prodCat.Name.toUpperCase())) {
                errors.push(`Production Catergory:${prodCat.Name} was not found`);
            }
            else {
                prodCat.Name = prodCat.Name.toUpperCase();
                const list = JSON.parse(JSON.stringify(this.validationLists.prodCats));
                prodCat = list.filter(function (elment) {
                    return elment.Name === prodCat.Name;
                })[0];
            }
            items.filter(element => {
                if (!element.SKU) {
                    errors.push(`Please enter a sku or delete`);
                }
                else if (!this.validationLists.skus.some(i => i.SKU === element.SKU.toUpperCase())) {
                    errors.push(`Sku :${element.SKU} was not found`);
                }
                else {
                    element.SKU = element.SKU.toUpperCase();
                    var allskus = JSON.parse(JSON.stringify(this.validationLists.skus));
                    element.Id = allskus.filter(function (item) {
                        return item.SKU === element.SKU;
                    })[0].Id;
                }
            });
            if (!items.some(i => i.Quantity > 0)) {
                errors.push('sorry you need a quantity to submit');
            }
            this.errors = errors;
            this.items = items;
            this.plannedProduction.productionCat = prodCat;
            this.vaildPlannedProduction = errors.length < 1;
        },
        prodCatIdSet: function () {
            const list = JSON.parse(JSON.stringify(this.validationLists.prodCats));
            var prodCat = this.productionCat;
            prodCat = list.filter(function (elment) {
                return elment.Name === prodCat.Name;
            })[0];
            this.productionCat = prodCat;
        },
        addSkuRows: function (number) {
            //App.alert({
            //    container: $('#alert_container').val(), // alerts parent container 
            //    place: 'append', // append or prepent in container 
            //    type: 'success', // alert's type 
            //    message: 'Test alert', // alert's message
            //    close: true, // make alert closable
            //    reset: false, // close all previouse alerts first 
            //    focus: true, // auto scroll to the alert after shown 
            //    closeInSeconds: 10000, // auto close after defined seconds 
            //    icon: 'fa fa-check' // put icon class before the message 
            //    });
            for (var i = 0; i < number; i++)
                this.items.push({ Id: null, SKU: '', Quantity: null });
            //this.validatePlannedProd();
        },
        getSkus: function () {
            $.get("/production/GetSKUsList", result => {
                this.validationLists.skus = result;
            });
        },
        getIdOfSku: function (index) {
            var allskus = JSON.parse(JSON.stringify(this.validationLists.skus));
            var thisItem = this.items[index];
            if (allskus.some(i => i.SKU === thisItem.SKU)) {
                thisItem.Id = allskus.filter(function (item) {
                    return item.SKU === thisItem.SKU;
                })[0].Id;
            }   else {
                thisItem.Id = null;
            }
            this.items[index] = thisItem;
        },
        removeSkuLine: function (index) {
            //swal({
            //    title: "Are you sure?",
            //    text: "Your will not be able to recover this imaginary file!",
            //    type: "warning",
            //    showCancelButton: true,
            //    confirmButtonClass: "btn-danger",
            //    confirmButtonText: "Yes, delete it!",
            //    closeOnConfirm: false
            //},
            //     () => {
            //        swal("Deleted!", "Your imaginary file has been deleted.", "success");
            //        this.items.splice(index, 1);
            //    });
            this.items.splice(index, 1);
        },
        checkIfExsits: function () {
            var pp = this.plannedProduction;
            var existingitems = this.existingItems;
            if (pp.productionCat.Id && pp.productionCatYear) {
                $.post("/production/GetPlannedProduction", {
                    plannedProduction: { ProductionCatergoryId: pp.productionCat.Id, ProductionCatYear: pp.productionCatYear }
                },
                    function (result) {
                        if (result.Id !== pp.id) {
                            swal({
                                title: "This season has been planned already!",
                                text: "Do you want to add to it?",
                                type: "success",
                                showCancelButton: true,
                                confirmButtonClass: "btn-success",
                                confirmButtonText: "Yes",
                                cancelButtonText: "No",
                                closeOnConfirm: false,
                                closeOnCancel: false
                            },

                                function (isConfirm) {
                                    window.onkeydown = null;
                                    window.onfocus = null;
                                    if (isConfirm) {

                                        pp.id = result.Id;

                                        app.existingItems = result.Items.map(function (item) {
                                            item.Edit = false;
                                            return item;

                                        });
                                        swal("Pulling up the data", "The data is loading", "success");

                                        //this.$refs.productionCatInput.focus();
                                    }
                                    else {
                                        pp.productionCat.Name = null;
                                        pp.productionCat.Id = null;
                                        app.existingItems = [];

                                        swal("Cancelled", "You can try another season", "error");
                                        app.validatePlannedProd();
                                    }
                                });
                            //this.plannedProduction = pp;
                            //this.validatePlannedProd();
                        }
                    }.bind(this)
                );
            }
        },
        getProdCatsList: function () {
            $.get("/production/GetProductionCats", result => {
                this.validationLists.prodCats = result;
            });
        },
        getTheDataTables: function (func) {
            $.get('/production/GetValidatoinListsWithIds', result => {
                this.validationLists.materials = result.material;
                this.validationLists.colors = result.colors;
                this.validationLists.sizes = result.sizes;
                this.validationLists.markers = result.markers;
                console.log(result);
                if (func) func();
                
            });
        },
        addColor: function () {
            var color = { Name: '', Id: null };
            var sizes = this.productionWizard.sizes;
            var items = [];
            sizes.forEach(function (size) {
                items.push({ size, color, Quantity: null });
            });
            this.productionWizard.items.push.apply(this.productionWizard.items, items);
            this.productionWizard.colors.push(color);
            this.validateWizard();
        },
        removeSize: function (index) {
            var size = this.productionWizard.sizes[index];
            var items = this.productionWizard.items.filter(i => i.size !== size);
            this.productionWizard.sizes.splice(index, 1);
            this.productionWizard.items = items;
            this.validateWizard();
        },
        addSize: function () {
            var size = { Name: '', Id: null };
            var colors = this.productionWizard.colors;
            var items = [];

            colors.forEach(function (color) {
                items.push({ size, color, Quantity: null });
            });
            
            this.productionWizard.items.push.apply(this.productionWizard.items,items);
            this.productionWizard.sizes.push(size);
            this.validateWizard();
        },
        removeColor: function (index) {
            var color = this.productionWizard.colors[index];
            var items = this.productionWizard.items.filter(i => i.color !== color);
            this.productionWizard.colors.splice(index, 1);
            this.productionWizard.items = items;
            this.validateWizard();
        },
        addTheGrid: function () {
            var colors = [{ Name: '', Id: null }];
            var sizes = [{ Name: 'S', Id: null }, { Name: 'M', Id: null }, { Name: 'L', Id: null }, { Name: 'EL', Id: null }];
            var items = [];
            sizes.forEach(function (size) {
                colors.forEach(function (color) {
                    items.push({ size, color, Quantity: null });
                });
            });
            this.productionWizard.items = items;
            this.productionWizard.colors = colors;
            this.productionWizard.sizes = sizes;
        },
        makeTheItems: function () {
            var colors = this.productionWizard.colors;
            var sizes = this.productionWizard.sizes;
            var items=[];
            sizes.forEach(function (size) {
                colors.forEach(function (color) {
                    items.push({ size, color, Quantity: null });
                });
            });
            this.productionWizard.items = items;
        },
        submitWizard: function () {
            var wizard = this.productionWizard;
            var stuff = wizard.items.filter(i => i.Quantity > 0).map(function (item) {
                return {
                    ColorId: item.color.Id,
                    SizeId: item.size.Id,
                    MaterialId: wizard.material.Id,
                    Quantity: item.Quantity

                };
            });
            console.log(stuff);
            var json = JSON.parse(JSON.stringify(stuff));
            $.post('/production/GetSkusFromWizard', { items: json, MarkerId: wizard.marker.Id }, result => {
                console.log(result);
                this.items.push.apply(this.items,result);
            });
        },
        submitPlanedProduction: function () {
            var plannedProduction = { id: this.plannedProduction.id, ProductionCatergoryId: this.plannedProduction.productionCat.Id, ProductionCatYear: this.plannedProduction.productionCatYear };
            var items = this.items.filter(i => i.Quantity && i.Id).map(i => {
                return {
                    ItemId: i.Id,
                    Quantity: i.Quantity
                };
            });
            if (plannedProduction.ProductionCatergoryId && items.length)
                $.post('/production/SubmitPlannedProduction', { plannedProduction, items }, () => window.location = '/production/PlannedProduction');

        }
    }

});