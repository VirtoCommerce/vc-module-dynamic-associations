using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.DynamicAssociationsModule.Core
{
	public static class ModuleConstants
	{
		public static class Security
		{
			public static class Permissions
			{
				public const string Access = "virtoCommerceDynamicAssociationsModule:access";
				public const string Create = "virtoCommerceDynamicAssociationsModule:create";
				public const string Read = "virtoCommerceDynamicAssociationsModule:read";
				public const string Update = "virtoCommerceDynamicAssociationsModule:update";
				public const string Delete = "virtoCommerceDynamicAssociationsModule:delete";

				public static string[] AllPermissions { get; } = { Read, Create, Access, Update, Delete };
			}
		}

		public static class Settings
		{
			public static class General
			{
				public static SettingDescriptor VirtoCommerceDynamicAssociationsModuleEnabled { get; } = new SettingDescriptor
				{
					Name = "VirtoCommerceDynamicAssociationsModule.VirtoCommerceDynamicAssociationsModuleEnabled",
					GroupName = "VirtoCommerceDynamicAssociationsModule|General",
					ValueType = SettingValueType.Boolean,
					DefaultValue = false
				};

				public static SettingDescriptor VirtoCommerceDynamicAssociationsModulePassword { get; } = new SettingDescriptor
				{
					Name = "VirtoCommerceDynamicAssociationsModule.VirtoCommerceDynamicAssociationsModulePassword",
					GroupName = "VirtoCommerceDynamicAssociationsModule|Advanced",
					ValueType = SettingValueType.SecureString,
					DefaultValue = "qwerty"
				};

				public static IEnumerable<SettingDescriptor> AllSettings
				{
					get
					{
						yield return VirtoCommerceDynamicAssociationsModuleEnabled;
						yield return VirtoCommerceDynamicAssociationsModulePassword;
					}
				}
			}

			public static IEnumerable<SettingDescriptor> AllSettings
			{
				get
				{
					return General.AllSettings;
				}
			}
		}
	}
}
