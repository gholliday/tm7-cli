using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "ThreatMetaData", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableThreatMetaData
{
    [DataMember(Name = "IsPriorityUsed", Order = 1)]
    public bool IsPriorityUsed { get; set; }

    [DataMember(Name = "IsStatusUsed", Order = 2)]
    public bool IsStatusUsed { get; set; }

    [DataMember(Name = "PropertiesMetaData", Order = 3)]
    public List<SerializableThreatMetaDatum> PropertiesMetaData { get; set; }

    public SerializableThreatMetaData() { }

    public SerializableThreatMetaData(bool isPriorityUsed, bool isStatusUsed,
        List<SerializableThreatMetaDatum> propertiesMetaData)
    {
        IsPriorityUsed = isPriorityUsed;
        IsStatusUsed = isStatusUsed;
        PropertiesMetaData = propertiesMetaData;
    }
}
