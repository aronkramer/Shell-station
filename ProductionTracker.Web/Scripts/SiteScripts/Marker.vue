<template>
    <div  v-for="marker in production.Markers" class="form-group row">
        <label class="col-md-2 col-form-label" for="MarkerName">Marker Name:</label>
        <input type="text" class="col-md-1" name="MarkerName" v-model="marker.Name" />
        <label class="col-md-1 col-form-label" for="Size">Size/s:</label>
        <div class="col-md-5">
            <template v-for="size in marker.Sizes">
                <input type="text" class="col-md-2" name="Size" v-model="size.SizeId" />
            </template>
        </div>
        <label class="col-md-2 col-form-label" for="Lot">Lot Number:</label>
        <input type="text" class="col-md-1" name="Lot" v-model="marker.LotNumber" />
        <div v-if="marker.Sizes.length > 1">
            <label class="col-md-3"></label>

            <label class="col-md-1 col-form-label" for="Size">Layers:</label>
            <div class="col-md-5">
                <template v-for="size in marker.Sizes">

                    <input type="text" class="col-md-2" name="Size" v-model="size.AmountPerLayer" />
                </template>
            </div>
        </div>
        <div class="row" v-if="marker.ColorMaterials.length > 0">
            <table class="table table-bordered table-striped table-responsive">
                <tr>
                    <th>Color</th>
                    <th>Matereal</th>
                    <th>Layers</th>
                    <th>Pakaging</th>
                </tr>
                <template v-for="colMat in marker.ColorMaterials">
                    <tr>
                        <td><input type="text" v-model="colMat.Color" /></td>
                        <td><input type="text" v-model="colMat.Material" /></td>
                        <td><input type="text" v-model="colMat.Layers" /></td>
                        <td><input type="text" /></td>
                    </tr>
                </template>
            </table>
        </div>
</template>

<script>
    export default {
        data() {
            return {
                production: { "Name": "MUREX 12/5/2018", "Markers": [{ "Name": "C101", "Size": "", "Sizes": [{ "SizeId": 7, "AmountPerLayer": 1 }, { "SizeId": 8, "AmountPerLayer": 3 }, { "SizeId": 9, "AmountPerLayer": 5 }, { "SizeId": 10, "AmountPerLayer": 0 }, { "SizeId": 11, "AmountPerLayer": 5 }], "ColorMaterials": [{ "Color": "WHITE", "Material": "MODAL", "Layers": 50 }], "LotNumber": 0 }, { "Name": "C101", "Size": "", "Sizes": [{ "SizeId": 7, "AmountPerLayer": 0 }, { "SizeId": 8, "AmountPerLayer": 0 }, { "SizeId": 9, "AmountPerLayer": 0 }, { "SizeId": 10, "AmountPerLayer": 5 }, { "SizeId": 11, "AmountPerLayer": 5 }], "ColorMaterials": [{ "Color": "WHITE", "Material": "MODAL", "Layers": 100 }], "LotNumber": 0 }, { "Name": "B202", "Size": "", "Sizes": [{ "SizeId": 7, "AmountPerLayer": 6 }], "ColorMaterials": [{ "Color": "WHITE", "Material": "MODAL", "Layers": 100 }], "LotNumber": 0 }] }
            }
        }
</script>