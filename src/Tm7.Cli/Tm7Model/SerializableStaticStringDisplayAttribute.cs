using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.StaticStringDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableStaticStringDisplayAttribute : SerializableStringDisplayAttribute, IStaticDisplayAttribute
{
    public SerializableStaticStringDisplayAttribute(string displayName, string name, string attributeValue)
        : base(displayName, name, attributeValue)
    {
    }
}
