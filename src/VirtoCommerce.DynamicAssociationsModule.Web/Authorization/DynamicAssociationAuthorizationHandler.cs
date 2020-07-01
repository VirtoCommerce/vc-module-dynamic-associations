using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Core.Services;
using VirtoCommerce.CatalogModule.Data.Authorization;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.DynamicAssociationsModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.DynamicAssociationsModule.Web.Authorization
{
    public class DynamicAssociationAuthorizationHandler : PermissionAuthorizationHandlerBase<DynamicAssociationAuthorizationRequirement>
    {
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;
        private readonly IStoreService _storeService;
        private readonly ICategoryService _categoryService;

        public DynamicAssociationAuthorizationHandler(
            IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
            IStoreService storeService,
            ICategoryService categoryService)
        {
            _jsonOptions = jsonOptions.Value;
            _storeService = storeService;
            _categoryService = categoryService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DynamicAssociationAuthorizationRequirement requirement)
        {
            await base.HandleRequirementAsync(context, requirement);

            if (!context.HasSucceeded)
            {
                var userPermission = context.User.FindPermission(requirement.Permission, _jsonOptions.SerializerSettings);

                if (userPermission != null)
                {
                    var allowedCatalogIds = userPermission
                        .AssignedScopes
                        .OfType<SelectedCatalogScope>()
                        .Select(x => x.CatalogId)
                        .Distinct()
                        .ToArray();

                    if (context.Resource is DynamicAssociation[] dynamicAssociations)
                    {
                        var storeIds = dynamicAssociations.Select(x => x.StoreId).Distinct();
                        var stores = await _storeService.GetByIdsAsync(storeIds.ToArray());
                        var catalogIds = stores.Select(x => x.Catalog);

                        if (catalogIds.All(x => allowedCatalogIds.Contains(x)))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is DynamicAssociationSearchCriteria dynamicAssociationSearchCriteria)
                    {
                        var storeIds = dynamicAssociationSearchCriteria.StoreIds?.Distinct() ?? Array.Empty<string>();
                        var stores = await _storeService.GetByIdsAsync(storeIds.ToArray());
                        var availableStores = stores.Where(x => allowedCatalogIds.Contains(x.Catalog));

                        dynamicAssociationSearchCriteria.StoreIds = availableStores.Select(x => x.Id).ToArray();

                        context.Succeed(requirement);

                    }
                    else if (context.Resource is DynamicAssociation dynamicAssociation)
                    {
                        var storeId = dynamicAssociation.StoreId;
                        var store = await _storeService.GetByIdAsync(storeId);

                        if (allowedCatalogIds.Contains(store.Catalog))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is DynamicAssociationEvaluationContext evaluationContext)
                    {
                        var storeId = evaluationContext.StoreId;
                        var store = await _storeService.GetByIdAsync(storeId);

                        if (allowedCatalogIds.Contains(store.Catalog))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is DynamicAssociationConditionEvaluationRequest evaluationRequest)
                    {
                        var catalogId = evaluationRequest.CatalogId;
                        var categoryIds = evaluationRequest.CategoryIds;

                        if (!categoryIds.IsNullOrEmpty())
                        {
                            var categories = await _categoryService.GetByIdsAsync(categoryIds.ToArray(), $"{CategoryResponseGroup.WithOutlines}");

                            if ((catalogId == null || allowedCatalogIds.Any(x => x.EqualsInvariant(catalogId))) && categories.All(x => IsCategoryLocatedInCatalogs(x, allowedCatalogIds)))
                            {
                                context.Succeed(requirement);
                            }
                        }
                    }
                }
            }
        }

        protected virtual bool IsCategoryLocatedInCatalogs(Category category, string[] catalogIds)
        {
            return category
                .Outlines
                .SelectMany(x => x.Items.Select(y => y.Id))
                .Intersect(catalogIds, StringComparer.OrdinalIgnoreCase)
                .Any();
        }
    }
}
