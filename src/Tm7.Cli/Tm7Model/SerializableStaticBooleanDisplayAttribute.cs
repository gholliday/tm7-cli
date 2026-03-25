using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.StaticBooleanDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableStaticBooleanDisplayAttribute : SerializableBooleanDisplayAttribute, IStaticDisplayAttribute
{
    public SerializableStaticBooleanDisplayAttribute(string displayName, string name, bool attributeValue)
        : base(displayName, name, attributeValue)
    {
    }
}
