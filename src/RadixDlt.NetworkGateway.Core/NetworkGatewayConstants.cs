namespace RadixDlt.NetworkGateway.Core;

public static class NetworkGatewayConstants
{
    public static class Database
    {
        public const string MigrationsConnectionStringName = "NetworkGatewayMigrations";
        public const string ReadOnlyConnectionStringName = "NetworkGatewayReadOnly";
        public const string ReadWriteConnectionStringName = "NetworkGatewayReadWrite";
    }

    public static class Transaction
    {
        public const int IdentifierByteLength = 32;
        public const int CompressedPublicKeyBytesLength = 33;
    }
}
