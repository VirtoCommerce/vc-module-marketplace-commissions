using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.MarketplaceCommissionsModule.Core;

public static class ModuleConstants
{
    public static class Security
    {
        public static class Permissions
        {
            public const string Access = "MarketplaceCommissionsModule:access";
            public const string Create = "MarketplaceCommissionsModule:create";
            public const string Read = "MarketplaceCommissionsModule:read";
            public const string Update = "MarketplaceCommissionsModule:update";
            public const string Delete = "MarketplaceCommissionsModule:delete";

            public static string[] AllPermissions { get; } =
            {
                Access,
                Create,
                Read,
                Update,
                Delete,
            };
        }
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor MarketplaceCommissionsModuleEnabled { get; } = new()
            {
                Name = "MarketplaceCommissionsModule.MarketplaceCommissionsModuleEnabled",
                GroupName = "MarketplaceCommissionsModule|General",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
            };

            public static SettingDescriptor MarketplaceCommissionsModulePassword { get; } = new()
            {
                Name = "MarketplaceCommissionsModule.MarketplaceCommissionsModulePassword",
                GroupName = "MarketplaceCommissionsModule|Advanced",
                ValueType = SettingValueType.SecureString,
                DefaultValue = "qwerty",
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return MarketplaceCommissionsModuleEnabled;
                    yield return MarketplaceCommissionsModulePassword;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings
        {
            get
            {
                return General.AllGeneralSettings;
            }
        }
    }
}
