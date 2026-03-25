using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "KnowledgeBase", IsReference = true, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableKnowledgeBase
{
    [DataMember(Name = "GenericElements")]
    public List<SerializableElementType> GenericElements { get; private set; }

    [DataMember(Name = "Manifest")]
    public SerializableManifest Manifest { get; private set; }

    [DataMember(Name = "StandardElements")]
    public List<SerializableElementType> StandardElements { get; private set; }

    [DataMember(Name = "ThreatCategories")]
    public List<SerializableThreatCategory> ThreatCategories { get; private set; }

    [DataMember(Name = "ThreatMetaData")]
    public SerializableThreatMetaData ThreatMetaData { get; private set; }

    [DataMember(Name = "ThreatTypes")]
    public List<SerializableThreatType> ThreatTypes { get; private set; }

    public SerializableKnowledgeBase(SerializableManifest manifest,
        SerializableThreatMetaData threatMetaData,
        IEnumerable<SerializableElementType> genericElements,
        IEnumerable<SerializableElementType> standardElements,
        IEnumerable<SerializableThreatCategory> threatCategories,
        IEnumerable<SerializableThreatType> threatTypes)
    {
        Manifest = manifest;
        ThreatMetaData = threatMetaData;
        GenericElements = genericElements.ToList();
        StandardElements = standardElements.ToList();
        ThreatCategories = threatCategories.ToList();
        ThreatTypes = threatTypes.ToList();
    }
}
