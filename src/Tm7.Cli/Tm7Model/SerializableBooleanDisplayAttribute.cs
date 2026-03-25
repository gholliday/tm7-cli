using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.BooleanDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableBooleanDisplayAttribute : SerializableDisplayAttribute
{
    public SerializableBooleanDisplayAttribute(string displayName, string name, bool attributeValue)
        : base(displayName, name, attributeValue)
    {
    }
}
