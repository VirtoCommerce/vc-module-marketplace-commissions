angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.feeStaticListController', ['$scope', 'platformWebApp.bladeUtils', 'platformWebApp.bladeNavigationService', 'virtoCommerce.marketplaceCommissionsModule.webApi', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        function ($scope, bladeUtils, bladeNavigationService, commissionsApi, uiGridHelper, gridOptionExtension) {
            $scope.uiGridConstants = uiGridHelper.uiGridConstants;

            var blade = $scope.blade;
            blade.headIcon = 'fas fa-usd';

            blade.refresh = function () {
                blade.isLoading = true;

                var searchCriteria = getSearchCriteria();

                if (blade.searchCriteria) {
                    angular.extend(searchCriteria, blade.searchCriteria);
                }

                commissionsApi.searchFees(searchCriteria, function (data) {
                    blade.isLoading = false;

                    $scope.listEntries = [];
                    blade.selectedItem = undefined;

                    if (data.results) {
                        $scope.listEntries = data.results;
                        if ($scope.selectedNodeId) {
                            blade.selectedItem = $scope.listEntries.filter(x => x.id === $scope.selectedNodeId).find(o => true);
                        }
                    }

                    if (blade.childBlade && blade.selectedItem) {
                        blade.childBlade.currentEntity = blade.selectedItem;
                        blade.childBlade.refresh();
                    }
                });
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.refresh", icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () {
                        return true;
                    }
                },
                {
                    name: "platform.commands.add", icon: 'fas fa-plus',
                    executeMethod: function () {
                        $scope.showDetails(undefined, true);
                    },
                    canExecuteMethod: function () {
                        return true;
                    }
                }
            ];

            var setSelectedNode = function (listItem) {
                $scope.selectedNodeId = listItem.id;
                blade.selectedItem = listItem;
            };

            $scope.selectNode = function (listItem) {
                setSelectedNode(listItem);

                $scope.showDetails(listItem);
            }

            $scope.showDetails = function (listItem, isNew) {
                var newBlade = {
                    id: isNew ? 'feeStaticAdd' : 'feeStaticDetails',
                    isNew: isNew,
                    currentEntity: listItem,
                    title: isNew ? 'marketplace-commissions.blades.fee-static-add.title' : listItem.name,
                    subtitle: isNew ? 'marketplace-commissions.blades.fee-static-add.subtitle' : 'marketplace.blades.fee-static-details.subtitle',
                    controller: 'virtoCommerce.marketplaceCommissionsModule.feeStaticDetailsController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/blades/fee-static-details.tpl.html'
                };
                blade.childBlade = newBlade;
                bladeNavigationService.showBlade(newBlade, blade);
            }

            function getSearchCriteria() {
                var searchCriteria = {
                    type: 'static',
                    sort: 'isDefault:desc'
                };
                return searchCriteria;
            }

            blade.refresh();
        }
    ]);
