using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.ListDisplayAttribute, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableListDisplayAttribute : SerializableDisplayAttribute
{
    [DataMember(Name = "SelectedIndex")]
    public int SelectedIndex { get; private set; }

    public SerializableListDisplayAttribute(string displayName, string name, List<string> attributeValue, int selectedIndex)
        : base(displayName, name, attributeValue)
    {
        SelectedIndex = selectedIndex;
    }
}
