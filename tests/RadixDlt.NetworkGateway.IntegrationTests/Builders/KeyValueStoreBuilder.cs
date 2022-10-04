using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RadixDlt.CoreApiSdk.Model;
using RadixDlt.NetworkGateway.IntegrationTests.Data;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Newtonsoft.Json.Linq.Extensions;

namespace RadixDlt.NetworkGateway.IntegrationTests.Builders;

[DataContract]
public class DataJsonField
{
    public DataJsonField(string bytes, string type, ScryptoType typeId)
    {
        Bytes = bytes;
        Type = type;
        TypeId = typeId;
    }

    [DataMember(Name ="bytes")]
    public string Bytes { get; set; }

    [DataMember(Name ="type")]
    public string Type { get; set; }

    [DataMember(Name ="type_id")]
    public ScryptoType TypeId { get; set; }
}

[DataContract]
public class DataJsonFields
{
    public DataJsonFields()
    {
        Fields = new List<DataJsonField>();
    }

    [DataMember(Name = "fields")]
    public List<DataJsonField> Fields { get; set; }

    [DataMember(Name ="type")]
    public string Type
    {
        get { return "Struct"; }
    }
}

public class KeyValueStoreBuilder : BuilderBase<DataStruct>
{
    private readonly DataJsonFields _dataJsonFields = new();
    private readonly List<EntityId> _ownedEntities = new();

    public KeyValueStoreBuilder()
    {
    }

    public override DataStruct Build()
    {
        var strJson = JsonConvert.SerializeObject(_dataJsonFields, Formatting.Indented);

        var dataHex = Convert.ToHexString(Encoding.UTF8.GetBytes(strJson));

        var dataStruct = new DataStruct(
            structData: new SborData(dataHex, strJson),
            ownedEntities: _ownedEntities,
            referencedEntities: new List<EntityId>());

        return dataStruct;
    }

    public KeyValueStoreBuilder WithDataStructField(string bytes, string type, ScryptoType typeId)
    {
        _dataJsonFields.Fields.Add(new DataJsonField(bytes, type, typeId));

        return this;
    }

    public KeyValueStoreBuilder WithOwnedEntity(EntityType entityType, string entityAddressHex)
    {
        _ownedEntities.Add(new EntityId(entityType, entityAddressHex));

        return this;
    }
}
