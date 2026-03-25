using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "ThreatCategory", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableThreatCategory : SerializableExtendable
{
    [DataMember(Name = "Id")]
    public string Id { get; private set; }

    [DataMember(Name = "LongDescription")]
    public string LongDescription { get; private set; }

    [DataMember(Name = "Name")]
    public string Name { get; private set; }

    [DataMember(Name = "ShortDescription")]
    public string ShortDescription { get; private set; }

    public SerializableThreatCategory(bool isExtendable, string name, string id,
        string shortDescription, string longDescription)
        : base(isExtendable)
    {
        Name = name;
        Id = id;
        ShortDescription = shortDescription;
        LongDescription = longDescription;
    }
}
