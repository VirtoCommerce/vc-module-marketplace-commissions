angular.module('virtoCommerce.marketplaceCommissionsModule')
    .controller('virtoCommerce.marketplaceCommissionsModule.feeDynamicDetailsController', ['$q', '$scope', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'virtoCommerce.marketplaceCommissionsModule.webApi', 'virtoCommerce.marketplaceModule.webApi', 'virtoCommerce.coreModule.common.dynamicExpressionService', 'virtoCommerce.marketplaceModule.catalogProducts', 'virtoCommerce.marketplaceModule.catalogCategories',
        function ($q, $scope, bladeNavigationService, dialogService, commissionsApi, marketplaceApi, dynamicExpressionService, catalogProducts, catalogCategories) {
            var blade = $scope.blade;
            blade.isLoading = false;

            function initializeBlade(data) {
                if (data.expressionTree && data.expressionTree.children) {
                    data.expressionTree.children.forEach(x => extendElementBlock(x));
                }

                blade.currentEntity = angular.copy(data);
                blade.originalEntity = data;
                blade.command = {
                    feeDetails: blade.currentEntity
                };
                blade.isLoading = false;
            }

            blade.refresh = function (parentRefresh) {
                if (blade.isNew) {
                    commissionsApi.getNewFee(function (data) {
                        initializeBlade(data);
                    });
                }
                else {
                    commissionsApi.getFeeById({ id: blade.currentEntity.id }, function (data) {
                        initializeBlade(data);
                    });
                }

                if (parentRefresh) {
                    blade.parentBlade.refresh(true);
                }
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
                        canExecuteMethod: function () {
                            return !blade.isNew;
                        }
                    }
                ];

                blade.onClose = function (closeCallback) {
                    bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, $scope.saveChanges, closeCallback, "marketplace.dialogs.fee-dynamic-save.title", "marketplace.dialogs.fee-dynamic-save.message");
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

                if (blade.currentEntity.expressionTree) {
                    blade.currentEntity.expressionTree.availableChildren = undefined;
                    if (blade.currentEntity.expressionTree.children) {
                        blade.currentEntity.expressionTree.children.forEach(x => stripOffUiInformation(x));
                    }
                }

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
                    id: "confirmDeleteDynamicFee",
                    title: "marketplace-commissions.dialogs.fee-dynamic-delete.title",
                    message: "marketplace-commissions.dialogs.fee-dynamic-delete.message",
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

            $scope.validatePriorityAsync = function (value) {
                if (value > 0) {
                    if (Number.isInteger(value)) {
                        $scope.errorData = null;
                        return $q.resolve();
                    }
                    else {
                        $scope.errorData = 'PriorityNotInteger';
                        return $q.reject();
                    }
                } else {
                    $scope.errorData = 'PriorityNotPositive';
                    return $q.reject();
                }
            };

            blade.calculationTypes = [
                "percent",
                "fixed"
            ];

            // dynamic conditions
            function extendElementBlock(expressionBlock) {
                var retVal = dynamicExpressionService.expressions[expressionBlock.id];
                if (!retVal) {
                    retVal = { displayName: 'unknown element: ' + expressionBlock.id };
                }

                angular.extend(expressionBlock, retVal);

                if (expressionBlock.availableChildren) {
                    expressionBlock.availableChildren.forEach(x => extendElementBlock(x));
                }

                if (!expressionBlock.children) {
                    expressionBlock.children = [];
                }
                expressionBlock.children.forEach(x => fillChildrenBlock(x));

                return expressionBlock;
            };

            function fillChildrenBlock(expressionBlock) {
                var retVal = dynamicExpressionService.expressions[expressionBlock.id];
                if (!retVal) {
                    retVal = { displayName: 'unknown element: ' + expressionBlock.id };
                }

                angular.extend(expressionBlock, retVal);

                if (expressionBlock.id === 'VcmpConditionCategoryIs') {
                    if (expressionBlock.includingCategories.length > 0) {
                        catalogCategories.search({ withHidden: true, objectIds: expressionBlock.includingCategories }, function (data) {
                            fillIncludingCategoriesBlock(data.items);
                            blade.originalEntity = angular.copy(blade.currentEntity);
                        });
                    }

                    if (expressionBlock.excludingCategories.length > 0) {
                        catalogCategories.search({ withHidden: true, objectIds: expressionBlock.excludingCategories }, function (data) {
                            fillExcludingCategoriesBlock(data.items);
                            blade.originalEntity = angular.copy(blade.currentEntity);
                        });
                    }

                    if (expressionBlock.excludingProducts.length > 0) {
                        catalogProducts.search({ withHidden: true, objectIds: expressionBlock.excludingProducts }, function (data) {
                            fillExcludingProductsBlock(data.items);
                            blade.originalEntity = angular.copy(blade.currentEntity);
                        });
                    }
                }

                if (expressionBlock.id === 'VcmpConditionProductIs' && expressionBlock.includingProducts.length > 0) {
                    expressionBlock.productIds = expressionBlock.includingProducts;
                    catalogProducts.search({ withHidden: true, objectIds: expressionBlock.productIds }, function (data) {
                        fillProductsBlock(data.items);
                        blade.originalEntity = angular.copy(blade.currentEntity);
                    });
                }

                if (expressionBlock.id === 'VcmpConditionVendorIs' && expressionBlock.vendorIds.length > 0) {
                    marketplaceApi.searchSellers({ withHidden: true, objectIds: expressionBlock.vendorIds }, function (data) {
                        fillVendorsBlock(data.results);
                        blade.originalEntity = angular.copy(blade.currentEntity);
                    });
                }

                return expressionBlock;
            };

            function fillIncludingCategoriesBlock(filteredCategories) {
                if (filteredCategories) {
                    let commonConditionBlock = blade.currentEntity.expressionTree.children.filter(x => x.id === 'BlockCommissionCondition').find(o => true);
                    let categoryIsConditionBlock = commonConditionBlock && commonConditionBlock.children
                        ? commonConditionBlock.children.filter(x => x.id === 'VcmpConditionCategoryIs').find(o => true)
                        : undefined;
                    if (categoryIsConditionBlock && filteredCategories.length > 0) {
                        categoryIsConditionBlock.categoryId = filteredCategories[0].id;
                        categoryIsConditionBlock.categoryName = filteredCategories[0].name;
                    }
                }
            };

            function fillExcludingCategoriesBlock(filteredCategories) {
                if (filteredCategories) {
                    let commonConditionBlock = blade.currentEntity.expressionTree.children.filter(x => x.id === 'BlockCommissionCondition').find(o => true);
                    let categoryIsConditionBlock = commonConditionBlock && commonConditionBlock.children
                        ? commonConditionBlock.children.filter(x => x.id === 'VcmpConditionCategoryIs').find(o => true)
                        : undefined;
                    if (categoryIsConditionBlock) {
                        filteredCategories.forEach(x => {
                            categoryIsConditionBlock.children.push({
                                id: 'ConditionCategoryIsNot',
                                categoryId: x.id,
                                categoryName: x.name,
                                templateURL: 'ConditionCategoryIsNot.html'
                            })
                        });
                    }
                }
            };

            function fillExcludingProductsBlock(filteredProducts) {
                if (filteredProducts) {
                    let commonConditionBlock = blade.currentEntity.expressionTree.children.filter(x => x.id === 'BlockCommissionCondition').find(o => true);
                    let categoryIsConditionBlock = commonConditionBlock && commonConditionBlock.children
                        ? commonConditionBlock.children.filter(x => x.id === 'VcmpConditionCategoryIs').find(o => true)
                        : undefined;
                    if (categoryIsConditionBlock) {
                        categoryIsConditionBlock.children.push({
                            id: 'ConditionProductIsNot',
                            productIds: filteredProducts.map(x => x.id),
                            productNames: filteredProducts.map(x => x.name),
                            productCodes: filteredProducts.map(x => x.code),
                            templateURL: 'ConditionProductIsNot.html'
                        });
                    }
                }
            };

            function fillProductsBlock(filteredProducts) {
                if (filteredProducts) {
                    let commonConditionBlock = blade.currentEntity.expressionTree.children.filter(x => x.id === 'BlockCommissionCondition').find(o => true);
                    let productIsConditionBlock = commonConditionBlock && commonConditionBlock.children
                        ? commonConditionBlock.children.filter(x => x.id === 'VcmpConditionProductIs').find(o => true)
                        : undefined;
                    if (productIsConditionBlock) {
                        productIsConditionBlock.productNames = filteredProducts.map(x => x.name);
                        productIsConditionBlock.productCodes = filteredProducts.map(x => x.code);
                    }
                }
            };

            function fillVendorsBlock(filteredVendors) {
                if (filteredVendors) {
                    let vendorsCommonConditionBlock = blade.currentEntity.expressionTree.children.filter(x => x.id === 'VendorCommissionCondition').find(o => true);
                    let vendorsIsConditionBlock = vendorsCommonConditionBlock && vendorsCommonConditionBlock.children
                        ? vendorsCommonConditionBlock.children.filter(x => x.id === 'VcmpConditionVendorIs').find(o => true)
                        : undefined;
                    if (vendorsIsConditionBlock) {
                        vendorsIsConditionBlock.vendorNames = filteredVendors.map(x => x.name);
                    }
                }
            };

            function stripOffUiInformation(expressionElement) {
                expressionElement.availableChildren = undefined;
                expressionElement.displayName = undefined;
                expressionElement.getValidationError = undefined;
                expressionElement.groupName = undefined;
                expressionElement.newChildLabel = undefined;
                expressionElement.templateURL = undefined;

                let conditionCategoryElements = expressionElement.children.filter(x => x.id === 'VcmpConditionCategoryIs');
                conditionCategoryElements.forEach(x => {
                    x.includingCategories = [];
                    x.includingCategories.push(x.categoryId);
                    let excludingCategories = x.children.filter(y => y.id === 'ConditionCategoryIsNot' && y.categoryId);
                    x.excludingCategories = excludingCategories.map(c => c.categoryId);
                    let excludingProducts = x.children.filter(y => y.id === 'ConditionProductIsNot' && y.productIds);
                    let productIdsArr = excludingProducts.map(p => p.productIds);
                    x.excludingProducts = [].concat(...productIdsArr);
                    x.children = undefined;
                    x.availableChildren = undefined;
                    x.categoryId = undefined;
                    x.categoryName = undefined;
                });

                var conditionProductsElements = expressionElement.children.filter(x => x.id === 'VcmpConditionProductIs');
                conditionProductsElements.forEach(x => {
                    if (x.productIds) {
                        x.includingProducts = x.productIds;
                    }
                    if (x.productId) {
                        x.includingProducts.push(x.productId);
                    }
                    x.children = undefined;
                    x.availableChildren = undefined;
                });
            };

            blade.refresh();

            // condition wizards
            $scope.openProductSelectWizard = function (parentElement) {
                var productsBlade = {
                    id: "CatalogEntries",
                    title: "marketplace.blades.catalog-entries.title-product",
                    controller: 'virtoCommerce.marketplaceModule.catalogEntriesController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceVendor)/Scripts/blades/catalog-entry-list.tpl.html',
                    breadcrumbs: [],
                    promotion: parentElement
                };

                bladeNavigationService.showBlade(productsBlade, $scope.blade);
            };

            $scope.openCategorySelectWizard = function (parentElement) {
                var selectedListEntries = [];
                var categoryBlade = {
                    id: "CatalogCategorySelect",
                    title: "marketplace.blades.catalog-items-select.title-category",
                    controller: 'virtoCommerce.marketplaceModule.catalogItemSelectController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceVendor)/Scripts/blades/catalog-items-select.tpl.html',
                    breadcrumbs: [],
                    toolbarCommands: [
                        {
                            name: "platform.commands.pick-selected", icon: 'fas fa-plus',
                            executeMethod: function (blade) {
                                parentElement.categoryId = selectedListEntries[0].id;
                                parentElement.categoryName = selectedListEntries[0].name;
                                bladeNavigationService.closeBlade(blade);
                            },
                            canExecuteMethod: function () {
                                return selectedListEntries.length == 1;
                            }
                        }]
                };

                categoryBlade.options = {
                    showCheckingMultiple: false,
                    allowCheckingItem: false,
                    allowCheckingCategory: true,
                    withProducts: false,
                    checkItemFn: function (listItem, isSelected) {
                        if (listItem.type != 'category') {
                            categoryBlade.error = 'Must select Category';
                            listItem.selected = undefined;
                        } else {
                            if (isSelected) {
                                if (_.all(selectedListEntries, function (x) { return x.id != listItem.id; })) {
                                    selectedListEntries.push(listItem);
                                }
                            }
                            else {
                                selectedListEntries = _.reject(selectedListEntries, function (x) { return x.id == listItem.id; });
                            }
                            categoryBlade.error = undefined;
                        }
                    }
                };

                marketplaceApi.getSettings((settings) => {
                    categoryBlade.catalogId = settings.masterCatalogId;
                    bladeNavigationService.showBlade(categoryBlade, $scope.blade);
                });
            };

            $scope.openVendorSelectWizard = function (parentElement) {
                var vendorsBlade = {
                    id: "VendorsSelect",
                    title: "marketplace.blades.seller-list.title",
                    controller: 'virtoCommerce.marketplaceModule.sellerListController',
                    template: 'Modules/$(VirtoCommerce.MarketplaceVendor)/Scripts/blades/seller-list.tpl.html',
                    breadcrumbs: [],
                    customizeToolbar: true,
                    selectedVendorIds: parentElement.vendorIds,
                    toolbarCommands: [
                        {
                            name: "platform.commands.pick-selected", icon: 'fas fa-plus',
                            executeMethod: function (blade) {
                                parentElement.vendorIds = blade.selectedVendors.map(x => x.id);
                                parentElement.vendorNames = blade.selectedVendors.map(x => x.name);
                                bladeNavigationService.closeBlade(blade);
                            },
                            canExecuteMethod: function (blade) {
                                return blade.selectedVendors && blade.selectedVendors.length > 0;
                            }
                        }]
                };

                bladeNavigationService.showBlade(vendorsBlade, $scope.blade);
            };
        }
    ]);
