angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.orderFeeWidgetController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.widget.blade;

            $scope.openBlade = function () {
                var orderFeesBlade = {
                    id: 'orderFees',
                    title: 'marketplace-commissions.blades.order-fee-list.title',
                    controller: 'virtoCommerce.marketplaceCommissionsModule.orderFeeListController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/blades/order-fee-list.tpl.html'
                };

                if (blade.customerOrder.items) {
                    orderFeesBlade.lineItems = angular.copy(blade.customerOrder.items);
                }

                bladeNavigationService.showBlade(orderFeesBlade, blade);
            };

        }]);
