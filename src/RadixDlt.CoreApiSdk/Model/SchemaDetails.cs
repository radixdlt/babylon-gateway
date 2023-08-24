namespace RadixDlt.CoreApiSdk.Model;

public record struct SchemaDetails(string SchemaHash, long TypeIndex, LocalTypeIndex.KindEnum SborTypeKind);