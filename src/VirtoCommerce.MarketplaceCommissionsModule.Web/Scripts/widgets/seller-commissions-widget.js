angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.sellerCommissionsWidgetController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.widget.blade;

            $scope.openBlade = function () {
                var sellerCommissionsBlade = {
                    id: 'sellerCommissions',
                    title: 'marketplace-commissions.blades.seller-commissions.title',
                    sellerId: blade.currentEntity.id,
                    controller: 'virtoCommerce.marketplaceCommissionsModule.sellerCommissionsController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/blades/seller-commissions.tpl.html'
                };

                bladeNavigationService.showBlade(sellerCommissionsBlade, blade);
            };

        }]);
