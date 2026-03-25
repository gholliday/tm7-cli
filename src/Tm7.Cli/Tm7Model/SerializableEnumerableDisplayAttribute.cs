using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.EnumerableDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableEnumerableDisplayAttribute : SerializableDisplayAttribute
{
    public SerializableEnumerableDisplayAttribute(string displayName, string name, Enum attributeValue)
        : base(displayName, name, attributeValue)
    {
    }
}
