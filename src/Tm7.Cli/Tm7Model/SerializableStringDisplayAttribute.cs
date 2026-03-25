using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.StringDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableStringDisplayAttribute : SerializableDisplayAttribute
{
    public SerializableStringDisplayAttribute(string displayName, string name, string attributeValue)
        : base(displayName, name, attributeValue)
    {
    }
}
