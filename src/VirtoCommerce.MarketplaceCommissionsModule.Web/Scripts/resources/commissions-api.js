angular.module('virtoCommerce.marketplaceCommissionsModule')
    .factory('virtoCommerce.marketplaceCommissionsModule.webApi', ['$resource', function ($resource) {
        return $resource('api/vcmp', null, {
            //commissions
            createFee: { method: 'POST', url: 'api/vcmp/commissions' },
            updateFee: { method: 'PUT', url: 'api/vcmp/commissions' },
            searchFees: { method: 'POST', url: 'api/vcmp/commissions/search' },
            deleteFee: { method: 'DELETE', url: 'api/vcmp/commissions' },
            getFeeById: { method: 'GET', url: 'api/vcmp/commissions/:id' },
            getNewFee: { method: 'GET', url: 'api/vcmp/commissions/new' },
            //seller commissions
            getSellerCommission: { method: 'GET', url: 'api/vcmp/seller/:sellerId/commissions' },
            updateSellerCommission: { method: 'POST', url: 'api/vcmp/seller/commissions' }
        });
    }]);
