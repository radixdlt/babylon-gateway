namespace RadixDlt.CoreApiSdk.Model;

public partial class PackageObjectSubstateTypeReference
{
    public override SchemaDetails GetSchemaDetails()
    {
        return new SchemaDetails(SchemaHash, LocalTypeIndex.Index, LocalTypeIndex.Kind);
    }
}