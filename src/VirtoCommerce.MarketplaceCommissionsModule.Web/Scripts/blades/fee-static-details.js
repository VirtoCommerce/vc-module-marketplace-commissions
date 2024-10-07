angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.feeStaticDetailsController', ['$q', '$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.marketplaceCommissionsModule.webApi',
        function ($q, $scope, bladeNavigationService, dialogService, commissionsApi) {
            var blade = $scope.blade;
            blade.isLoading = false;

            function initializeBlade(data) {
                blade.currentEntity = !blade.isNew ? angular.copy(data) : { type: 'static', calculationType: 'percent' };
                blade.originalEntity = data;
                blade.command = {
                   feeDetails: blade.currentEntity
                };
                blade.title = blade.currentEntity.name;
                blade.isLoading = false;
            }

            blade.refresh = function (parentRefresh) {
                if (parentRefresh) {
                    blade.parentBlade.refresh(true);
                }

                initializeBlade(blade.currentEntity);
            };

            $scope.setForm = function (form) { $scope.formScope = form; };

            if (!blade.isNew) {
                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save", icon: 'fas fa-save',
                        executeMethod: function () {
                            $scope.saveChanges();
                        },
                        canExecuteMethod: canSave
                    },
                    {
                        name: "platform.commands.reset", icon: 'fa fa-undo',
                        executeMethod: function () {
                            angular.copy(blade.originalEntity, blade.currentEntity);
                        },
                        canExecuteMethod: isDirty
                    },
                    {
                        name: "platform.commands.delete", icon: 'fas fa-trash-alt',
                        executeMethod: function () {
                            $scope.delete();
                        },
                        canExecuteMethod: function() {
                            return !blade.originalEntity.isDefault;
                        }
                    }
                ];

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "marketplace-commissions.dialogs.fee-static-save.title", "marketplace-commissions.dialogs.fee-static-save.message");
                };

                function isDirty() {
                    return !angular.equals(blade.currentEntity, blade.originalEntity);
                }

                function canSave() {
                    return isDirty() && $scope.formScope && $scope.formScope.$valid;
                }
            }

            $scope.saveChanges = function () {
                blade.isLoading = true;

                if (blade.isNew) {
                    commissionsApi.createFee(blade.command, function () {
                        $scope.bladeClose();
                        blade.refresh(true);
                    });
                } else {
                    commissionsApi.updateFee(blade.command, function () {
                        blade.refresh(true);
                    });
                }
            }

            $scope.delete = function () {
                var dialog = {
                    id: "confirmDeleteStaticFee",
                    title: "marketplace-commissions.dialogs.fee-static-delete.title",
                    message: "marketplace-commissions.dialogs.fee-static-delete.message",
                    callback: function (remove) {
                        if (remove) {
                            blade.isLoading = true;
                            commissionsApi.deleteFee({ ids: [blade.currentEntity.id] }, function () {
                                $scope.bladeClose();
                                blade.parentBlade.refresh();
                            });
                        }
                    }
                };
                dialogService.showConfirmationDialog(dialog);
            };

            blade.calculationTypes = [
                "Percent",
                "Fixed"
            ];

            blade.refresh();
        }
    ]);
