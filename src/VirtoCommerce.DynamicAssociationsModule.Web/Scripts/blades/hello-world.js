angular.module('virtoCommerce.dynamicAssociationsModule')
    .controller('virtoCommerce.dynamicAssociationsModule.helloWorldController', ['$scope', 'virtoCommerce.dynamicAssociationsModule.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'VirtoCommerce.DynamicAssociationsModule';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'virtoCommerce.dynamicAssociationsModule.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
