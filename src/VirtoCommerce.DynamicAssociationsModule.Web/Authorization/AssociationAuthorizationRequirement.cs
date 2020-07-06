using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.DynamicAssociationsModule.Web.Authorization
{
    public class AssociationAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public AssociationAuthorizationRequirement(string permission)
            : base(permission)
        {
        }
    }
}
