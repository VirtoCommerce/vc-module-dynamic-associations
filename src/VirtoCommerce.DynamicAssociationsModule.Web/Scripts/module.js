// Call this to register your module to main application
var moduleName = "virtoCommerce.dynamicAssociationsModule";

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(['platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state', 'virtoCommerce.marketingModule.marketingMenuItemService',
        function (mainMenuService, widgetService, $state, marketingMenuItemService) {
            marketingMenuItemService.register({
                id: '3',
                name: 'Dynamic product associations',
                entityName: 'dynamicProductAssociations',
                icon: 'fa fa-a',
                controller: 'virtoCommerce.dynamicAssociationsModule.dynamicAssociationsListController',
                template: 'Modules/$(virtoCommerce.dynamicAssociationsModule)/Scripts/blades/dynamicAssociations-list.tpl.html',
                permission: 'dynamic-association:access'
            });
        }
    ]);
