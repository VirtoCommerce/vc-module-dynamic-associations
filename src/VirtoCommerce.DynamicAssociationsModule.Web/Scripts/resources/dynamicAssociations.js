angular.module('virtoCommerce.dynamicAssociationsModule')
    .factory('virtoCommerce.dynamicAssociationsModule.dynamicAssociations', ['$resource', function ($resource) {
        return $resource('api/dynamicAssociations/:id', null, {
            search: { url: 'api/dynamicAssociations/search', method: 'POST' },
            save: { url: 'api/dynamicAssociations/', method: 'POST', isArray: true },
            remove: { url: 'api/dynamicAssociations/', method: 'DELETE' },
            new: { url: 'api/dynamicAssociations/new', method: 'GET' },
            preview: { url: 'api/dynamicAssociations/preview', method: 'POST' }
        });
    }]);
