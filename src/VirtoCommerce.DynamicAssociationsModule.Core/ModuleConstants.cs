using System.Diagnostics.CodeAnalysis;

namespace VirtoCommerce.DynamicAssociationsModule.Core
{
    [ExcludeFromCodeCoverage]
    public class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Read = "dynamic-association:read";
                public const string Create = "dynamic-association:create";
                public const string Update = "dynamic-association:update";
                public const string Access = "dynamic-association:access";
                public const string Delete = "dynamic-association:delete";

                public static string[] AllPermissions = { Read, Create, Update, Access, Delete };
            }
        }
    }
}
