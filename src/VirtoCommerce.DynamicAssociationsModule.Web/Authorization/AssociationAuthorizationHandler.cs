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
    public class AssociationAuthorizationHandler : PermissionAuthorizationHandlerBase<AssociationAuthorizationRequirement>
    {
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;
        private readonly IStoreService _storeService;
        private readonly ICategoryService _categoryService;

        public AssociationAuthorizationHandler(
            IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
            IStoreService storeService,
            ICategoryService categoryService)
        {
            _jsonOptions = jsonOptions.Value;
            _storeService = storeService;
            _categoryService = categoryService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AssociationAuthorizationRequirement requirement)
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

                    if (context.Resource is Association[] associations)
                    {
                        var storeIds = associations.Select(x => x.StoreId).Distinct().ToArray();
                        var stores = await _storeService.GetNoCloneAsync(storeIds);
                        var catalogIds = stores.Select(x => x.Catalog);

                        if (catalogIds.All(x => allowedCatalogIds.Contains(x)))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is AssociationSearchCriteria associationSearchCriteria)
                    {
                        var storeIds = associationSearchCriteria.StoreIds?.Distinct().ToArray() ?? Array.Empty<string>();
                        var stores = await _storeService.GetNoCloneAsync(storeIds);
                        var availableStores = stores.Where(x => allowedCatalogIds.Contains(x.Catalog));

                        associationSearchCriteria.StoreIds = availableStores.Select(x => x.Id).ToArray();

                        context.Succeed(requirement);

                    }
                    else if (context.Resource is Association association)
                    {
                        var storeId = association.StoreId;
                        var store = await _storeService.GetNoCloneAsync(storeId);

                        if (allowedCatalogIds.Contains(store.Catalog))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is AssociationEvaluationContext evaluationContext)
                    {
                        var storeId = evaluationContext.StoreId;
                        var store = await _storeService.GetNoCloneAsync(storeId);

                        if (allowedCatalogIds.Contains(store.Catalog))
                        {
                            context.Succeed(requirement);
                        }
                    }
                    else if (context.Resource is AssociationConditionEvaluationRequest evaluationRequest)
                    {
                        var catalogId = evaluationRequest.CatalogId;
                        var categoryIds = evaluationRequest.CategoryIds;

                        if (!categoryIds.IsNullOrEmpty())
                        {
                            var categories = await _categoryService.GetAsync(categoryIds, $"{CategoryResponseGroup.WithOutlines}");

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
