using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "ThreatModel", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableModelData
{
    [DataMember(Name = "DrawingSurfaceList", IsRequired = true, Order = 1)]
    public List<SerializableDrawingSurfaceModel> DrawingSurfaceList { get; private set; }

    [DataMember(Name = "MetaInformation", Order = 2)]
    public SerializableMetaInformation MetaInformation { get; private set; }

    [DataMember(Name = "Notes", Order = 4)]
    public List<SerializableNote> Notes { get; private set; }

    [DataMember(Name = "ThreatInstances", Order = 5)]
    public Dictionary<string, SerializableThreat> AllThreatsDictionary { get; private set; }

    [DataMember(Name = "ThreatGenerationEnabled", Order = 6)]
    public bool? ThreatGenerationEnabled { get; private set; }

    [DataMember(Name = "Validations", Order = 7)]
    public List<SerializableValidation> Validations { get; private set; }

    [DataMember(Name = "Version", Order = 8)]
    public string Version { get; private set; }

    [DataMember(Name = "KnowledgeBase", Order = 9)]
    public SerializableKnowledgeBase KnowledgeBase { get; private set; }

    [DataMember(Name = "Profile", Order = 10)]
    public SerializableProfile Profile { get; private set; }

    public SerializableModelData(
        IEnumerable<SerializableDrawingSurfaceModel> drawingSurfaceList,
        SerializableMetaInformation metaInformation,
        IEnumerable<SerializableNote> notes,
        Dictionary<string, SerializableThreat> allThreatsDictionary,
        bool? threatGenerationEnabled,
        IEnumerable<SerializableValidation> validations,
        string version,
        SerializableKnowledgeBase knowledgeBase,
        SerializableProfile profile)
    {
        DrawingSurfaceList = drawingSurfaceList.ToList();
        MetaInformation = metaInformation;
        Notes = notes.ToList();
        AllThreatsDictionary = allThreatsDictionary;
        ThreatGenerationEnabled = threatGenerationEnabled;
        Validations = validations.ToList();
        Version = version ?? "4.3";
        KnowledgeBase = knowledgeBase;
        Profile = profile;
    }

}
