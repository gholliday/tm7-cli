using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "Taggable", IsReference = true, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model.Abstracts")]
public abstract class SerializableTaggable
{
    [DataMember(Name = "GenericTypeId")]
    public string GenericTypeId { get; private set; }

    [DataMember(Name = "Guid")]
    public Guid Guid { get; private set; }

    [DataMember(Name = "Properties")]
    public List<object> Properties { get; private set; }

    [DataMember(Name = "TypeId")]
    public string TypeId { get; private set; }

    public SerializableTaggable(Guid guid, string typeId, string genericTypeId, IEnumerable<SerializableDisplayAttribute> properties)
    {
        Guid = guid;
        TypeId = typeId;
        GenericTypeId = genericTypeId;
        Properties = new List<object>(properties);
    }
}
