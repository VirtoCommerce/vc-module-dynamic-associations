using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Caching
{
    public class AssociationCacheRegion : CancellableCacheRegion<AssociationCacheRegion>
    {
        public static IChangeToken CreateChangeToken(string[] associationIds)
        {
            if (associationIds == null)
            {
                throw new ArgumentNullException(nameof(associationIds));
            }

            var changeTokens = new List<IChangeToken> { CreateChangeToken() };

            changeTokens.AddRange(
                associationIds
                    .Select(associationId => CreateChangeTokenForKey(associationId))
                );

            return new CompositeChangeToken(changeTokens);
        }

        public static IChangeToken CreateChangeToken(Association[] associations)
        {
            if (associations == null)
            {
                throw new ArgumentNullException(nameof(associations));
            }

            return CreateChangeToken(associations.Select(x => x.Id).ToArray());
        }

        public static void ExpireEntity(Association association)
        {
            if (association == null)
            {
                throw new ArgumentNullException(nameof(association));
            }

            ExpireTokenForKey(association.Id);
        }
    }
}
