namespace PermissionBasedAuthorization.Constants
{
    public static class Permissions
    {
        public const string Type = "Permissions";
        public static List<string> GeneratePermissionsForModule(string module)
        {
            return
            [
                $"{Type}.{module}.View",
                $"{Type}.{module}.Create",
                $"{Type}.{module}.Edit",
                $"{Type}.{module}.Delete",
            ];
        }

        public static List<string> GenerateAllPermissions()
        {
            List<string> allPermissions = [];
            foreach (var module in Enum.GetNames(typeof(Modules)))
                allPermissions.AddRange(GeneratePermissionsForModule(module));

            return allPermissions;
        }

        public static class Products
        {
            public const string View = $"{Type}.{nameof(Products)}.View";
            public const string Create = $"{Type}.{nameof(Products)}.Create";
            public const string Edit = $"{Type}.{nameof(Products)}.Edit";
            public const string Delete = $"{Type}.{nameof(Products)}.Delete";
        }
    }
}
