using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Services
{
    public class AssociationEvaluator : IAssociationEvaluator
    {
        private readonly IStoreService _storeService;
        private readonly IAssociationConditionSelector _associationsConditionSelector;
        private readonly IItemService _itemService;
        private readonly IAssociationConditionEvaluator _associationsConditionEvaluator;

        public AssociationEvaluator(
            IStoreService storeService,
            IAssociationConditionSelector associationsConditionSelector,
            IItemService itemService,
            IAssociationConditionEvaluator associationsConditionEvaluator
            )
        {
            _storeService = storeService;
            _associationsConditionSelector = associationsConditionSelector;
            _itemService = itemService;
            _associationsConditionEvaluator = associationsConditionEvaluator;
        }

        public async Task<string[]> EvaluateAssociationsAsync(AssociationEvaluationContext context)
        {
            if (context.ProductsToMatch.IsNullOrEmpty())
            {
                return Array.Empty<string>();
            }

            var store = await _storeService.GetByIdAsync(context.StoreId);

            var products = await _itemService.GetByIdsAsync(context.ProductsToMatch,
                $"{ItemResponseGroup.WithProperties | ItemResponseGroup.WithOutlines}", store.Catalog);

            var result = new HashSet<string>();

            foreach (var product in products)
            {

                var associationCondition = await _associationsConditionSelector.GetAssociationConditionAsync(context, product);

                if (associationCondition != null)
                {
                    var searchResult = await _associationsConditionEvaluator.EvaluateAssociationConditionAsync(associationCondition);

                    result.AddRange(searchResult);
                }
            }

            return result.ToArray();
        }
    }
}
