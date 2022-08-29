namespace RadixDlt.NetworkGateway.PostgresIntegration;

public static class PostgresIntegrationConstants
{
    public static class Configuration
    {
        public const string MigrationsConnectionStringName = "NetworkGatewayMigrations";
        public const string ReadOnlyConnectionStringName = "NetworkGatewayReadOnly";
        public const string ReadWriteConnectionStringName = "NetworkGatewayReadWrite";
    }
}
