using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.StaticListDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableStaticListDisplayAttribute : SerializableListDisplayAttribute, IStaticDisplayAttribute
{
    public SerializableStaticListDisplayAttribute(string displayName, string name, List<string> attributeValue, int selectedIndex)
        : base(displayName, name, attributeValue, selectedIndex)
    {
    }
}
