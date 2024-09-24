// Call this to register your module to main application
var moduleName = 'virtoCommerce.marketplaceCommissionsModule';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider.state('workspace.fees',
                {
                    url: '/fees',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: ['platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var blade = {
                                id: 'fees',
                                title: 'marketplace-commissions.blades.fees.title',
                                subtitle: 'marketplace-commissions.blades.fees.subtitle',
                                controller: 'virtoCommerce.marketplaceCommissionsModule.feesController',
                                template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/blades/fees.tpl.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(blade);
                        }
                    ]
                });
        }
    ])
    .run(['$http', '$state', '$compile', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', 'virtoCommerce.coreModule.common.dynamicExpressionService',
        function ($http, $state, $compile, mainMenuService, widgetService, dynamicExpressionService) {
            // Register fees in main menu
            var menuItem = {
                path: 'browse/fees',
                icon: 'fa fa-usd',
                title: 'marketplace-commissions.main-menu.fees',
                priority: 35,
                action: function () { $state.go('workspace.fees'); },
                permission: 'operator:commissions'
            };
            mainMenuService.addMenuItem(menuItem);

            // Seller details: commissions widget
            var sellerCommissionsWidget = {
                controller: 'virtoCommerce.marketplaceCommissionsModule.sellerCommissionsWidgetController',
                template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/widgets/seller-commissions-widget.tpl.html'
            };
            widgetService.registerWidget(sellerCommissionsWidget, 'sellerDetails');

            // Order details: order commission widget
            var orderFeeWidget = {
                controller: 'virtoCommerce.marketplaceCommissionsModule.orderFeeWidgetController',
                template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/widgets/order-fee-widget.tpl.html'
            };
            widgetService.registerWidget(orderFeeWidget, 'customerOrderDetailWidgets');

            // Commissions: dynamic conditions tree
            var availableExcludings = [
                {
                    id: 'ConditionCategoryIsNot'
                },
                {
                    id: 'ConditionProductIsNot'
                }
            ];

            dynamicExpressionService.registerExpression({
                id: 'VendorCommissionCondition',
                displayName: 'Vendor ...',
                templateURL: 'VendorCommissionCondition.html',
                newChildLabel: 'Add condition'
            });

            dynamicExpressionService.registerExpression({
                id: 'BlockCommissionCondition',
                displayName: 'Commission ...',
                templateURL: 'BlockCommissionCondition.html',
                newChildLabel: 'Add condition'
            });

            dynamicExpressionService.registerExpression({
                id: 'VcmpConditionCategoryIs',
                displayName: 'Category is ...',
                templateURL: 'ConditionCategoryIs.html',
                availableChildren: availableExcludings,
                newChildLabel: 'Excluding'
            });

            dynamicExpressionService.registerExpression({
                id: 'ConditionCategoryIsNot',
                displayName: 'Excluding category ...',
                templateURL: 'ConditionCategoryIsNot.html'
            });

            dynamicExpressionService.registerExpression({
                id: 'VcmpConditionProductIs',
                displayName: 'Product is ...',
                templateURL: 'ConditionProductIs.html'
            });

            dynamicExpressionService.registerExpression({
                id: 'ConditionProductIsNot',
                displayName: 'Excuding product ...',
                templateURL: 'ConditionProductIsNot.html'
            });

            dynamicExpressionService.registerExpression({
                id: 'VcmpConditionVendorIs',
                displayName: 'Vendor is ...',
                templateURL: 'ConditionVendorIs.html'
            });

            $http.get('Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/dynamicConditions/templates.html').then(function (response) {
                $compile(response.data);
            });


        }
    ]);
