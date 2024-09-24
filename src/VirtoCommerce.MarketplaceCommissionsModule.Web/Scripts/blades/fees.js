angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.feesController', ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
        $scope.selectedNodeId = null;

        function initializeBlade() {
            var entities = [
                { name: 'marketplace-commissions.blades.fees.menu.static.title', entityName: 'static', icon: 'fa fa-usd' },
                { name: 'marketplace-commissions.blades.fees.menu.dynamic.title', entityName: 'dynamic', icon: 'fa fa-usd' }
            ];
            $scope.blade.currentEntities = entities;
            $scope.blade.isLoading = false;

            $scope.blade.openBlade(entities[0]);
        };

        $scope.blade.openBlade = function (data) {
            $scope.selectedNodeId = data.entityName;

            var newBlade = {
                id: 'fees-' + data.entityName,
                title: 'marketplace-commissions.blades.fee-' + data.entityName + '-list.title',
                controller: 'virtoCommerce.marketplaceCommissionsModule.fee' + data.entityName.charAt(0).toUpperCase() + data.entityName.slice(1) + 'ListController',
                template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/blades/fee-' + data.entityName + '-list.tpl.html',
                sortDefault: true
            };
            bladeNavigationService.showBlade(newBlade, $scope.blade);
        }

        $scope.blade.headIcon = 'fa fa-usd';

        initializeBlade();
    }]);
