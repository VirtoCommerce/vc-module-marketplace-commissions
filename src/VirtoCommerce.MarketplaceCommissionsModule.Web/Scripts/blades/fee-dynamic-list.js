angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.feeDynamicListController', ['$scope', 'platformWebApp.bladeUtils', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.marketplaceCommissionsModule.webApi', 'platformWebApp.uiGridHelper', 'platformWebApp.ui-grid.extension',
        function ($scope, bladeUtils, bladeNavigationService, dialogService, commissionsApi, uiGridHelper, gridOptionExtension) {
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

                    $scope.pageSettings.totalItems = data.totalCount;

                    $scope.listEntries = data.results ? data.results : [];
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
                },
                {
                    name: "platform.commands.delete",
                    icon: 'fa fa-trash-o',
                    executeMethod: function () { deleteList($scope.gridApi.selection.getSelectedRows()); },
                    canExecuteMethod: function () {
                        return $scope.listEntries && $scope.listEntries.length > 0 && $scope.gridApi && $scope.gridApi.selection.getSelectedRows().some(o => true);
                    }
                }
            ];

            var setSelectedNode = function (listItem) {
                $scope.selectedNodeId = listItem.id;
            };

            $scope.selectNode = function (listItem) {
                setSelectedNode(listItem);

                $scope.showDetails(listItem);
            }

            $scope.showDetails = function (listItem, isNew) {
                var newBlade = {
                    id: isNew ? 'feeDynamicAdd' : 'feeDynamicDetails',
                    isNew: isNew,
                    currentEntity: listItem,
                    title: isNew ? 'marketplace-commissions.blades.fee-dynamic-add.title' : listItem.name,
                    subtitle: isNew ? 'marketplace-commissions.blades.fee-dynamic-add.subtitle' : 'marketplace.blades.fee-dynamic-details.subtitle',
                    controller: 'virtoCommerce.marketplaceCommissionsModule.feeDynamicDetailsController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/blades/fee-dynamic-details.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            }

            $scope.edit = function (item) {
                $scope.showDetails(item, false);
            };

            $scope.delete = function (item) {
                deleteList([item], true);
            };

            function deleteList(selection, single) {
                var dialog = {
                    id: "confirmDeleteDynamicFee",
                    title: single ? "marketplace-commissions.dialogs.fee-dynamic-delete.title" : "marketplace-commissions.dialogs.fee-dynamic-list-delete.title",
                    message: single ? "marketplace-commissions.dialogs.fee-dynamic-delete.message" : "marketplace-commissions.dialogs.fee-dynamic-list-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            commissionsApi.deleteFee({ ids: selection.map(item => item.id) }, function () {
                                blade.refresh();
                            });
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            }

            $scope.setGridOptions = function (gridId, gridOptions) {
                $scope.gridOptions = gridOptions;
                gridOptionExtension.tryExtendGridOptions(gridId, gridOptions);

                gridOptions.onRegisterApi = function (gridApi) {
                    $scope.gridApi = gridApi;
                    gridApi.core.on.sortChanged($scope, function () {
                        if (!blade.isLoading) {
                            if (blade.sortDefault) {
                                blade.sortDefault = false;
                            }
                            blade.refresh();
                        }
                    });
                };

                bladeUtils.initializePagination($scope);
            };

            function getSearchCriteria() {
                var searchCriteria = {
                    type: 'dynamic',
                    sort: blade.sortDefault || uiGridHelper.getSortExpression($scope) === '' ? 'priority:asc' : uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount
                };
                return searchCriteria;
            }
        }
    ]);
