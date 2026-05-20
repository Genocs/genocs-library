namespace Genocs.Persistence.EFCore.MultiTenancy;

// TODO: Will be removed in the future. This class is used to store the constants related to multitenancy.
public class MultitenancyConstants
{
    public static class Root
    {
        public const string Id = "root";
        public const string Name = "Root";
        public const string EmailAddress = "admin@root.com"; // TODO: Change the email address.
    }

    public const string TenantIdName = "tenant";
}