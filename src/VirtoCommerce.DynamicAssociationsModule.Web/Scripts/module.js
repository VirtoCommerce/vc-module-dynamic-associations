// Call this to register your module to main application
var moduleName = "virtoCommerce.dynamicAssociationsModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider', '$urlRouterProvider',
        function ($stateProvider, $urlRouterProvider) {
            $stateProvider
                .state('workspace.virtoCommerceDynamicAssociationsModuleState', {
                    url: '/virtoCommerce.dynamicAssociationsModule',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        '$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'virtoCommerce.dynamicAssociationsModule.helloWorldController',
                                template: 'Modules/$(virtoCommerce.dynamicAssociationsModule)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state', 'virtoCommerce.marketingModule.marketingMenuItemService',
        function (mainMenuService, widgetService, $state, marketingMenuItemService) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/virtoCommerce.dynamicAssociationsModule',
                icon: 'fa fa-cube',
                title: 'VirtoCommerce.DynamicAssociationsModule',
                priority: 100,
                action: function () { $state.go('workspace.virtoCommerceDynamicAssociationsModuleState'); },
                permission: 'virtoCommerceDynamicAssociationsModule:access'
            };
            mainMenuService.addMenuItem(menuItem);

            marketingMenuItemService.register({
                id: '3',
                name: 'Dynamic product associations',
                entityName: 'dynamicProductAssociations',
                icon: 'fa fa-a',
                controller: 'virtoCommerce.dynamicAssociationsModule.dynamicAssociationsListController',
                template: 'Modules/$(virtoCommerce.dynamicAssociationsModule)/Scripts/blades/dynamicAssociations-list.tpl.html'
            });
        }
    ]);
