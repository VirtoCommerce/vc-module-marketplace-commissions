angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.orderFeeListController', ['$scope', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        function ($scope, bladeUtils, uiGridHelper, gridOptionExtension) {
            $scope.uiGridConstants = uiGridHelper.uiGridConstants;
            var blade = $scope.blade;

            blade.refresh = function () {
                if (blade.lineItems) {
                    blade.lineItems.forEach(x => angular.extend(x, { commissionName: x.feeDetails.find(o => true) ? x.feeDetails.find(o => true).description : undefined }));
                }

                blade.isLoading = false;
            };

            $scope.setGridOptions = function (gridId, gridOptions) {
                $scope.gridOptions = gridOptions;
                gridOptionExtension.tryExtendGridOptions(gridId, gridOptions);

                gridOptions.onRegisterApi = function (gridApi) {
                    $scope.gridApi = gridApi;
                };
            };

            blade.refresh();

        }
]);
