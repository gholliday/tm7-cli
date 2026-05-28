using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "Threat", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableThreat
{
    [DataMember(Name = "ChangedBy")]
    public string ChangedBy { get; private set; }

    [DataMember(Name = "DrawingSurfaceGuid")]
    public Guid DrawingSurfaceGuid { get; private set; }

    [DataMember(Name = "FlowGuid")]
    public Guid FlowGuid { get; private set; }

    [DataMember(Name = "Id")]
    public int Id { get; private set; }

    [DataMember(Name = "InteractionKey")]
    public string InteractionKey { get; private set; }

    [DataMember(Name = "InteractionString")]
    public string InteractionString { get; private set; }

    [DataMember(Name = "ModifiedAt")]
    public DateTime ModifiedAt { get; private set; }

    [DataMember(Name = "Priority")]
    public string Priority { get; private set; }

    // Public Dictionary preserved; DCS-visible backing list bypasses internal KeyValue<,>.
    [IgnoreDataMember]
    public Dictionary<string, string> Properties { get; set; }

    [DataMember(Name = "Properties")]
    private SerializableStringStringKvpList _propertiesList = new();

    [DataMember(Name = "SourceGuid")]
    public Guid SourceGuid { get; private set; }

    [DataMember(Name = "State")]
    public ThreatState State { get; private set; }

    [DataMember(Name = "StateInformation")]
    public string StateInformation { get; private set; }

    [DataMember(Name = "TargetGuid")]
    public Guid TargetGuid { get; private set; }

    [DataMember(Name = "Title")]
    public string Title { get; private set; }

    [DataMember(Name = "TypeId")]
    public string TypeId { get; private set; }

    [DataMember(Name = "Upgraded")]
    public bool Upgraded { get; private set; }

    [DataMember(Name = "UserThreatCategory")]
    public string UserThreatCategory { get; private set; }

    [DataMember(Name = "UserThreatDescription")]
    public string UserThreatDescription { get; private set; }

    [DataMember(Name = "UserThreatShortDescription")]
    public string UserThreatShortDescription { get; private set; }

    [DataMember(Name = "Wide")]
    public bool Wide { get; private set; }

    public SerializableThreat(int id, string typeId, Guid sourceGuid, Guid targetGuid,
        Guid flowGuid, Guid drawingSurfaceGuid, ThreatState state, string interactionKey,
        string priority, bool wide, string changedBy, DateTime modifiedAt, bool upgraded,
        Dictionary<string, string> properties)
    {
        Id = id;
        TypeId = typeId;
        SourceGuid = sourceGuid;
        TargetGuid = targetGuid;
        FlowGuid = flowGuid;
        DrawingSurfaceGuid = drawingSurfaceGuid;
        State = state;
        InteractionKey = interactionKey;
        Priority = priority;
        Wide = wide;
        ChangedBy = changedBy;
        ModifiedAt = modifiedAt;
        Upgraded = upgraded;
        Properties = properties;
    }

    [OnSerializing]
    private void OnSerializingDictionaries(StreamingContext context)
    {
        _propertiesList = Properties is null
            ? new SerializableStringStringKvpList()
            : new SerializableStringStringKvpList(
                Properties.Select(kvp => new SerializableStringStringKvp { Key = kvp.Key, Value = kvp.Value }));
    }

    [OnDeserialized]
    private void OnDeserializedDictionaries(StreamingContext context)
    {
        Properties = _propertiesList is null
            ? new Dictionary<string, string>()
            : _propertiesList
                .Where(e => e.Key is not null && e.Value is not null)
                .ToDictionary(e => e.Key!, e => e.Value!);
    }
}
