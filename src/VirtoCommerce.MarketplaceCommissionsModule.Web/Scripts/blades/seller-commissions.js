angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.sellerCommissionsController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.marketplaceCommissionsModule.webApi', 'platformWebApp.settings', 'virtoCommerce.marketplaceModule.approvalPolicyTranslationService',
        function ($scope, bladeNavigationService, commissionsApi, settings, approvalPolicyTranslationService) {
            var blade = $scope.blade;
            blade.headIcon = 'fas fa-usd';
            blade.isLoading = true;

            function initializeBlade(data) {
                blade.currentEntity = angular.copy(data);
                blade.originalEntity = data;

                commissionsApi.searchFees({ type: 'static', skip: 0, take: 1000 }, function (data) {
                    blade.staticFees = data.results ? data.results : [];

                    blade.isLoading = false;
                });

            }

            blade.refresh = function (parentRefresh) {
                if (parentRefresh) {
                    blade.parentBlade.refresh(true);
                }

                if (blade.sellerId) {
                    commissionsApi.getSellerCommission({ sellerId: blade.sellerId }, (data) => {
                        initializeBlade(data);
                    });
                }
            };

            blade.toolbarCommands = [
                {
                    name: "platform.commands.save", icon: 'fas fa-save',
                    executeMethod: saveChanges,
                    canExecuteMethod: function () {
                        return blade.canSave();
                    }
                },
                {
                    name: "platform.commands.reset", icon: 'fa fa-undo',
                    executeMethod: function () {
                        angular.copy(blade.originalEntity, blade.currentEntity);
                    },
                    canExecuteMethod: isDirty
                }
            ];

            blade.onClose = function (closeCallback) {
                bladeNavigationService.showConfirmationIfNeeded(isDirty(), blade.canSave(), blade, saveChanges, closeCallback, "marketplace.dialogs.seller-save.title", "marketplace.dialogs.seller-save.message");
            };

            blade.openStaticCommissions = function () {
                var newBlade = {
                    id: 'fees-static',
                    title: 'marketplace.blades.fee-static-list.title',
                    controller: 'virtoCommerce.marketplaceCommissionsModule.feeStaticListController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceCommissions)/Scripts/blades/fee-static-list.tpl.html',
                    sortDefault: true,
                    onClose: function (closeCallback) {
                        commissionsApi.searchFees({ type: 'static', skip: 0, take: 1000 }, function (data) {
                            blade.staticFees = data.results ? data.results : [];
                        });
                        closeCallback();
                    }
                };
                bladeNavigationService.showBlade(newBlade, $scope.blade);
            };

            $scope.setForm = function (form) {
                $scope.formScope = form;
            };

            //$scope.$on("new-notification-event", function (event, notification) {
            //    if (notification.notifyType === "SellerCommissionUpdatedDomainEvent"
            //        && blade.currentEntity.id === notification.sellerId) {
            //        marketplaceApi.getFeeById({ id: notification.commissionFeeId }, function (data) {
            //            blade.currentEntity.commissionFee = data;
            //            blade.refresh(true);
            //        });
            //    }
            //});

            function isDirty() {
                return !angular.equals(blade.currentEntity, blade.originalEntity);
            }

            blade.canSave = function () {
                return isDirty() && $scope.formScope && $scope.formScope.$valid;
            }

            function saveChanges() {
                blade.isLoading = true;

                commissionsApi.updateSellerCommission({
                    sellerId: blade.sellerId,
                    commissionFeeId: blade.currentEntity.id
                }, function () {
                    blade.refresh(true);
                });
            }

            blade.refresh();
        }
    ]);
