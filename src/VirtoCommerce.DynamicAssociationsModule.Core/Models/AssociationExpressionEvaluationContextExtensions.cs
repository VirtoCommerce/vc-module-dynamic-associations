using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model
{
    public static class AssociationExpressionEvaluationContextExtensions
    {

        public static bool AreItemsInCategory(this AssociationExpressionEvaluationContext context, string[] categoryIds)
        {
            var result = context.Products
                .InCategories(categoryIds)
                .Any();

            return result;
        }

        public static bool AreItemPropertyValuesEqual(this AssociationExpressionEvaluationContext context, Dictionary<string, string[]> propertyValues)
        {
            var result = context.Products.WithPropertyValues(propertyValues).Any();

            return result;
        }

        public static IEnumerable<CatalogProduct> InCategories(this IEnumerable<CatalogProduct> products, string[] categoryIds)
        {
            categoryIds = categoryIds.Where(x => x != null).ToArray();

            return categoryIds.Any() ? products.Where(x => ProductInCategories(x, categoryIds)) : products;
        }

        public static bool ProductInCategories(this CatalogProduct product, ICollection<string> categoryIds)
        {
            var result = categoryIds.Contains(product.CategoryId, StringComparer.OrdinalIgnoreCase);

            if (!result && !product.Outlines.IsNullOrEmpty())
            {
                result = product.Outlines.Any(x => x.Items.Select(x => x.Id).Intersect(categoryIds, StringComparer.OrdinalIgnoreCase).Any());
            }

            return result;
        }

        public static bool ProductInProducts(this CatalogProduct product, IEnumerable<string> productIds)
        {
            return productIds.Contains(product.Id, StringComparer.OrdinalIgnoreCase);
        }

        public static IEnumerable<CatalogProduct> WithPropertyValues(this IEnumerable<CatalogProduct> products, Dictionary<string, string[]> propertyValues)
        {
            var productArray = products as CatalogProduct[] ?? products.ToArray();

            return propertyValues.Any() ? productArray.Where(x => x.ProductHasPropertyValues(propertyValues)) : productArray;
        }

        public static bool ProductHasPropertyValues(this CatalogProduct product, Dictionary<string, string[]> propertyValues)
        {
            return propertyValues.Where(x => x.Key != null).All(kvp =>
            {
                // return true if no specific property values were selected, treating it as all properties
                if (kvp.Value.IsNullOrEmpty())
                    return true;

                var productProperty = product.Properties.FirstOrDefault(x => x.Name.EqualsIgnoreCase(kvp.Key));
                if (productProperty == null)
                    return false;

                var productPropertyValues = productProperty.Values.Where(x => x.Value != null).Select(x => x.Value.ToString()).Distinct().ToList();
                return kvp.Value.Intersect(productPropertyValues, StringComparer.OrdinalIgnoreCase).Any();
            });
        }

    }
}
