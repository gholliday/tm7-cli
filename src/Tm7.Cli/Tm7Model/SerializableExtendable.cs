using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "Extendable", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableExtendable
{
    [DataMember(Name = "IsExtension")]
    public bool IsExtension { get; private set; }

    protected SerializableExtendable(bool isExtension)
    {
        IsExtension = isExtension;
    }
}
