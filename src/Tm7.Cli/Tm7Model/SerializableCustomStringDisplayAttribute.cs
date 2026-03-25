using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.CustomStringDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableCustomStringDisplayAttribute : SerializableStringDisplayAttribute
{
    public SerializableCustomStringDisplayAttribute(string displayName, string name, string attributeValue)
        : base(displayName, name, attributeValue)
    {
    }
}
