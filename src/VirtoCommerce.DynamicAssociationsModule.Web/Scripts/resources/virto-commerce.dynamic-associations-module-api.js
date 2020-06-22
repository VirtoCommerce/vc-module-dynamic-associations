angular.module('virtoCommerce.dynamicAssociationsModule')
    .factory('virtoCommerce.dynamicAssociationsModule.webApi', ['$resource', function ($resource) {
        return $resource('api/VirtoCommerceDynamicAssociationsModule');
}]);
