using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

// User-defined key/value pair types that replace DataContractSerializer's internal
// System.Runtime.Serialization.KeyValue<K,V> when serializing the dictionary-shaped
// members of the threat model. The internal KeyValue<,> type cannot be statically
// instantiated for arbitrary closed generics under NativeAOT, which causes
// "missing native code or metadata" failures at runtime. By substituting our own
// item types we keep the wire format identical while making the type graph
// fully analyzable by the AOT compiler.

[DataContract(Name = "KeyValueOfguidanyType",
    Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
public class SerializableGuidObjectKvp
{
    [DataMember(Name = "Key", Order = 0)]
    public Guid Key { get; set; }

    [DataMember(Name = "Value", Order = 1)]
    public object? Value { get; set; }
}

[CollectionDataContract(Name = "ArrayOfKeyValueOfguidanyType",
    Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays",
    ItemName = "KeyValueOfguidanyType")]
public class SerializableGuidObjectKvpList : List<SerializableGuidObjectKvp>
{
    public SerializableGuidObjectKvpList() { }
    public SerializableGuidObjectKvpList(IEnumerable<SerializableGuidObjectKvp> items) : base(items) { }
}

[DataContract(Name = "KeyValueOfstringThreatpc_P0_PhOB",
    Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
public class SerializableStringThreatKvp
{
    [DataMember(Name = "Key", Order = 0)]
    public string? Key { get; set; }

    [DataMember(Name = "Value", Order = 1)]
    public SerializableThreat? Value { get; set; }
}

[CollectionDataContract(Name = "ArrayOfKeyValueOfstringThreatpc_P0_PhOB",
    Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays",
    ItemName = "KeyValueOfstringThreatpc_P0_PhOB")]
public class SerializableStringThreatKvpList : List<SerializableStringThreatKvp>
{
    public SerializableStringThreatKvpList() { }
    public SerializableStringThreatKvpList(IEnumerable<SerializableStringThreatKvp> items) : base(items) { }
}

[DataContract(Name = "KeyValueOfstringstring",
    Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
public class SerializableStringStringKvp
{
    [DataMember(Name = "Key", Order = 0)]
    public string? Key { get; set; }

    [DataMember(Name = "Value", Order = 1)]
    public string? Value { get; set; }
}

[CollectionDataContract(Name = "ArrayOfKeyValueOfstringstring",
    Namespace = "http://schemas.microsoft.com/2003/10/Serialization/Arrays",
    ItemName = "KeyValueOfstringstring")]
public class SerializableStringStringKvpList : List<SerializableStringStringKvp>
{
    public SerializableStringStringKvpList() { }
    public SerializableStringStringKvpList(IEnumerable<SerializableStringStringKvp> items) : base(items) { }
}
