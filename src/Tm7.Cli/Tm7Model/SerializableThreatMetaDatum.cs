using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "ThreatMetaDatum", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableThreatMetaDatum
{
    [DataMember(Name = "Name", IsRequired = true, Order = 1)]
    public string Name { get; set; }

    [DataMember(Name = "Label", IsRequired = true, Order = 2)]
    public string Label { get; set; }

    [DataMember(Name = "HideFromUI", IsRequired = true, Order = 3)]
    public bool HideFromUI { get; set; }

    [DataMember(Name = "Values", IsRequired = false, Order = 4)]
    public List<string> Values { get; set; }

    [DataMember(Name = "Id", Order = 5)]
    public string Id { get; set; }

    [DataMember(Name = "AttributeType", Order = 6)]
    public int AttributeType { get; set; }

    public SerializableThreatMetaDatum() { }

    public SerializableThreatMetaDatum(string name, string label, bool hideFromUI,
        List<string> values, int attributeType)
    {
        Name = name;
        Label = label;
        HideFromUI = hideFromUI;
        Values = values;
        AttributeType = attributeType;
    }

    public SerializableThreatMetaDatum(string name, string label, bool hideFromUI,
        List<string> values, string id, int attributeType)
    {
        Name = name;
        Label = label;
        HideFromUI = hideFromUI;
        Values = values;
        Id = id;
        AttributeType = attributeType;
    }
}
