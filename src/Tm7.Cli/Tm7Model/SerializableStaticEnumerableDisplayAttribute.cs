using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.StaticEnumerableDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableStaticEnumerableDisplayAttribute : SerializableEnumerableDisplayAttribute, IStaticDisplayAttribute
{
    public SerializableStaticEnumerableDisplayAttribute(string displayName, string name, Enum attributeValue)
        : base(displayName, name, attributeValue)
    {
    }
}
