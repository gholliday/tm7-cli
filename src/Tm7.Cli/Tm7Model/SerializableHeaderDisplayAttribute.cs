using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.HeaderDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableHeaderDisplayAttribute : SerializableDisplayAttribute
{
    public SerializableHeaderDisplayAttribute(string headerTitle)
        : base(headerTitle, "", null)
    {
    }
}
