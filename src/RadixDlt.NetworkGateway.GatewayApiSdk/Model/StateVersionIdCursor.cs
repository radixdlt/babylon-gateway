using System.Runtime.Serialization;

namespace RadixDlt.NetworkGateway.GatewayApiSdk.Model;

[DataContract]
public sealed record StateVersionIdCursor(long StateVersion, long Id)
{
    [DataMember(Name = "sv", EmitDefaultValue = false)]
    public long StateVersion { get; set; } = StateVersion;

    [DataMember(Name = "id", EmitDefaultValue = false)]
    public long Id { get; set; } = Id;

    public static StateVersionIdCursor FromCursorString(string cursorString)
    {
        return Serializations.FromBase64JsonOrDefault<StateVersionIdCursor>(cursorString);
    }

    public string ToCursorString()
    {
        return Serializations.AsBase64Json(this);
    }
}
