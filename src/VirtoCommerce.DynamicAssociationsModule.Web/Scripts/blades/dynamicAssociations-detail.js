angular.module('virtoCommerce.dynamicAssociationsModule')
    .controller('virtoCommerce.dynamicAssociationsModule.dynamicAssociationDetailController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.dynamicAssociationsModule.dynamicAssociations', 'virtoCommerce.storeModule.stores', function ($scope, bladeNavigationService, associations, stores) {
        var blade = $scope.blade;
        var formScope;

        $scope.setForm = (form) => { formScope = form; };

        $scope.BlockMatchingRules = 'BlockMatchingRules';
        $scope.BlockResultingRules = 'BlockResultingRules';
        $scope.BlockOutputTuning = 'BlockOutputTuning';
        $scope.ConditionPropertyValues = 'ConditionPropertyValues';
        $scope.ConditionProductCategory = 'ConditionProductCategory';

        $scope.productsToMatchCount = 0;
        $scope.productsToDisplayCount = 0;

        blade.currentEntity = {};

        blade.updatePermission = 'dynamic-association:update';
        blade.isMatchingRulesExist = false;
        blade.isResultingRulesExist = false;

        blade.refresh = function (parentRefresh) {
            if (blade.isNew) {
                associations.new({},
                    data => {
                        blade.currentEntity = data;
                        initializeBlade(blade.currentEntity);
                    });
            } else {
                associations.get({ id: blade.currentEntityId }, (data) => {
                    initializeBlade(data);

                    if (parentRefresh) {
                        blade.parentBlade.refresh();
                    }

                    if (blade.currentEntity.storeId) {
                        //Need to pre filter catalog-category selector
                        stores.get({ id: blade.currentEntity.storeId }, response => {
                            blade.currentEntity.catalogId = blade.origEntity.catalogId = response.catalog;
                        });
                    }
                });
            }
        };

        function initializeBlade(data) {
            if (!blade.isNew) {
                blade.title = data.name;
            }
            blade.currentEntity = angular.copy(data);
            blade.origEntity = data;

            $scope.checkExistingRules();
            $scope.GetMatchingProductsCount();
            blade.isLoading = false;
        }

        $scope.checkExistingRules = function () {
            const matchingRules = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockMatchingRules);
            const matchingCondition = $scope.getCondition(matchingRules, $scope.ConditionProductCategory);
            blade.isMatchingRulesExist = matchingCondition.catalogId && matchingCondition.categoryIds.length > 0;

            const resultingRules = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockResultingRules);
            const resultingCondition = $scope.getCondition(resultingRules, $scope.ConditionProductCategory);
            blade.isResultingRulesExist = resultingCondition.catalogId && resultingCondition.categoryIds.length > 0;
        };

        $scope.isDirty = () => {
            return !angular.equals(blade.currentEntity, blade.origEntity) && blade.hasUpdatePermission();
        };

        $scope.canSave = () => {
            return $scope.isDirty() && formScope && formScope.$valid && blade.currentEntity.storeId && blade.currentEntity.associationType && blade.currentEntity.priority !== undefined && blade.isMatchingRulesExist && blade.isResultingRulesExist;
        };

        $scope.outputTuning = function () {
            const rulesBlock = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockResultingRules);
            const categoryCondition = $scope.getCondition(rulesBlock, $scope.ConditionProductCategory);
            const newBlade = {
                id: "outputTuning",
                title: "dynamicAssociations.blades.dynamicAssociation-outputTuning.title",
                subtitle: 'dynamicAssociations.blades.dynamicAssociation-outputTuning.subtitle',
                controller: 'virtoCommerce.dynamicAssociationsModule.outputTuningController',
                template: 'Modules/$(VirtoCommerce.dynamicAssociationsModule)/Scripts/blades/outputTuning.tpl.html',
                categoryIds: categoryCondition.categoryIds,
                originalEntity: blade.currentEntity,
                onSelected: function (newSortingRules) {
                    let sortingRules = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockOutputTuning);
                    if (sortingRules) {
                        sortingRules.sort = newSortingRules.sort;
                        sortingRules.outputLimit = newSortingRules.outputLimit;
                    }
                }
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.mainParameters = function () {
            const parametersBlade = {
                id: "mainParameters",
                title: "dynamicAssociations.blades.dynamicAssociation-parameters.title",
                subtitle: 'dynamicAssociations.blades.dynamicAssociation-parameters.subtitle',
                controller: 'virtoCommerce.dynamicAssociationsModule.dynamicAssociationParametersController',
                template: 'Modules/$(VirtoCommerce.dynamicAssociationsModule)/Scripts/blades/mainParameters.tpl.html',
                originalEntity: blade.currentEntity,
                onSelected: function (entity) {
                    if (blade.currentEntity.catalogId !== entity.catalogId) {
                        $scope.clearConditionChildren(entity, $scope.BlockMatchingRules);
                        $scope.clearConditionChildren(entity, $scope.BlockResultingRules);

                        blade.currentEntity = entity;

                        $scope.checkExistingRules();
                        $scope.GetMatchingProductsCount();
                    } else {
                        blade.currentEntity = entity;
                    }
                }
            };
            bladeNavigationService.showBlade(parametersBlade, blade);
        };

        $scope.clearConditionChildren = function (entity, blockId) {
            const ruleBlock = _.find(entity.expressionTree.children, x => x.id === blockId);
            if (ruleBlock.children) {
                ruleBlock.children = [];
            }
        };

        $scope.cancelChanges = function () {
            angular.copy(blade.origEntity, blade.currentEntity);
            $scope.bladeClose();
        };

        $scope.saveChanges = function () {
            blade.isLoading = true;

            associations.save({}, [blade.currentEntity], (data) => {
                if (data && data.length > 0) {
                    blade.isNew = undefined;
                    blade.currentEntityId = data[0].id;
                    initializeBlade(data[0]);
                    initializeToolbar();
                    blade.refresh(true);
                } else {
                    bladeNavigationService.setError('Error while saving association rule', blade);
                }
            });
        };

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded($scope.isDirty(), $scope.canSave(), blade, $scope.saveChanges, closeCallback, "catalog.dialogs.catalog-save.title", "catalog.dialogs.catalog-save.message");
        };

        function initializeToolbar() {
            if (!blade.isNew) {
                blade.toolbarCommands = [
                    {
                        name: "platform.commands.save", icon: 'fa fa-save',
                        executeMethod: function () {
                            $scope.saveChanges();
                        },
                        canExecuteMethod: $scope.canSave,
                        permission: blade.updatePermission
                    },
                    {
                        name: "platform.commands.reset", icon: 'fa fa-undo',
                        executeMethod: function () {
                            blade.refresh(false);
                        },
                        canExecuteMethod: $scope.isDirty,
                        permission: blade.updatePermission
                    }
                ];
            }
        }
        // for ui label - UI 3 of 5 properties filled
        $scope.$watch('blade.currentEntity', (data) => {
            if (data) {
                $scope.totalPropertiesCount = 5;
                $scope.filledPropertiesCount = 0;

                $scope.filledPropertiesCount += blade.currentEntity.startDate ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.endDate ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.storeId ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.associationType ? 1 : 0;
                $scope.filledPropertiesCount += blade.currentEntity.priority !== undefined ? 1 : 0;
            }
        }, true);

        $scope.getCondition = function (rulesBlock, conditionName) {
            const conditionProductCategory = {
                id: $scope.ConditionProductCategory,
                catalogId: blade.currentEntity.catalogId,
                categoryIds: []
            };
            const conditionPropertyValues = {
                id: $scope.ConditionPropertyValues,
                properties: []
            };

            let categoryCondition = _.find(rulesBlock.children, x => x.id === conditionName);
            if (!categoryCondition) {
                switch (conditionName) {
                    case $scope.ConditionProductCategory:
                        rulesBlock.children.push(conditionProductCategory);
                        break;

                    case $scope.ConditionPropertyValues:
                        rulesBlock.children.push(conditionPropertyValues);
                        break;
                }
            }
            categoryCondition = _.find(rulesBlock.children, x => x.id === conditionName);
            return categoryCondition;
        };

        $scope.createProductFilter = function (rulesBlockName) {
            const rulesBlock = _.find(blade.currentEntity.expressionTree.children, x => x.id === rulesBlockName);
            let categoryCondition = $scope.getCondition(rulesBlock, $scope.ConditionProductCategory);
            let propertyCondition = $scope.getCondition(rulesBlock, $scope.ConditionPropertyValues);

            var ruleCreationBlade = {
                id: "createDynamicAssociationRule",
                controller: 'virtoCommerce.dynamicAssociationsModule.ruleCreationController',
                title: `Create rule for ${rulesBlockName === $scope.BlockMatchingRules ? 'matching' : 'display'} products`,
                template: 'Modules/$(virtoCommerce.dynamicAssociationsModule)/Scripts/blades/rule-creation.tpl.html',
                categoryIds: angular.copy(categoryCondition.categoryIds),
                editedProperties: angular.copy(propertyCondition.properties),
                catalogId: angular.copy(blade.currentEntity.catalogId),
                onSelected: function (selectedCategoryIds, editedProperties) {
                    propertyCondition.properties = editedProperties;
                    categoryCondition.categoryIds = selectedCategoryIds;
                    categoryCondition.catalogId = blade.currentEntity.catalogId;
                    $scope.checkExistingRules();
                    $scope.GetMatchingProductsCount();
                }
            };
            bladeNavigationService.showBlade(ruleCreationBlade, blade);
        };
        // Receive counts of products
        $scope.GetMatchingProductsCount = function () {
            const matchingRules = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockMatchingRules);

            let query = $scope.prepareQuery(matchingRules);
            associations.preview(query, data => {
                $scope.productsToMatchCount = data.totalCount;
            });

            const resultingRules = _.find(blade.currentEntity.expressionTree.children, x => x.id === $scope.BlockResultingRules);
            query = $scope.prepareQuery(resultingRules);
            associations.preview(query, data => {
                $scope.productsToDisplayCount = data.totalCount;
            });
        };

        $scope.prepareQuery = function (rule) {
            const properties = $scope.getCondition(rule, $scope.ConditionPropertyValues).properties;

            let categoryIds = [];
            let catalogId = blade.currentEntity.catalogId;
            const categoryCondition = $scope.getCondition(rule, $scope.ConditionProductCategory);

            if (categoryCondition.categoryIds) {
                categoryIds = categoryCondition.categoryIds;
            }
            if (categoryCondition.catalogId) {
                catalogId = categoryCondition.catalogId;
            }

            let propertyValues = {};
            _.each(properties,
                property => {
                    propertyValues[property.name] = property.values.map(x => x.value);
                });
            const dataQuery = {
                catalogId: catalogId,
                categoryIds: categoryIds,
                propertyValues: propertyValues,
                skip: 0,
                take: 0
            };
            return dataQuery;
        };

        initializeToolbar();
        blade.refresh(false);
    }]);
