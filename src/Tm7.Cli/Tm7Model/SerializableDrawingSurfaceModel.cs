using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "DrawingSurfaceModel", IsReference = true, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableDrawingSurfaceModel : SerializableTaggable
{
    // The public API stays as Dictionary<Guid, object> so consumers (renderer, commands, tests)
    // are unaffected. DCS serialization is redirected to private list fields via
    // [IgnoreDataMember] + [DataMember] + [OnSerializing]/[OnDeserialized] callbacks.
    // This is required because DCS's reflection-based reader on NativeAOT cannot
    // instantiate the internal System.Runtime.Serialization.KeyValue<Guid,object>
    // closed generic that backs CollectionDataContract for IDictionary<,>.
    [IgnoreDataMember]
    public Dictionary<Guid, object> Borders { get; set; } = new();

    [DataMember(Name = "Borders")]
    private SerializableGuidObjectKvpList _bordersList = new();

    [DataMember(Name = "Header")]
    public string Header { get; private set; }

    [IgnoreDataMember]
    public Dictionary<Guid, object> Lines { get; set; } = new();

    [DataMember(Name = "Lines")]
    private SerializableGuidObjectKvpList _linesList = new();

    [DataMember(Name = "Zoom")]
    public double Zoom { get; private set; }

    public SerializableDrawingSurfaceModel(Guid guid, string typeId, string genericTypeId,
        IEnumerable<SerializableDisplayAttribute> properties,
        IEnumerable<SerializableBorder> borders,
        IEnumerable<SerializableLine> lines,
        double zoom, string header)
        : base(guid, typeId, genericTypeId, properties)
    {
        Borders = borders.ToDictionary(b => b.Guid, b => (object)b);
        Lines = lines.ToDictionary(l => l.Guid, l => (object)l);
        Zoom = zoom;
        Header = header;
    }

    [OnSerializing]
    private void OnSerializingDictionaries(StreamingContext context)
    {
        _bordersList = Borders is null
            ? new SerializableGuidObjectKvpList()
            : new SerializableGuidObjectKvpList(
                Borders.Select(kvp => new SerializableGuidObjectKvp { Key = kvp.Key, Value = kvp.Value }));
        _linesList = Lines is null
            ? new SerializableGuidObjectKvpList()
            : new SerializableGuidObjectKvpList(
                Lines.Select(kvp => new SerializableGuidObjectKvp { Key = kvp.Key, Value = kvp.Value }));
    }

    [OnDeserialized]
    private void OnDeserializedDictionaries(StreamingContext context)
    {
        Borders = _bordersList is null
            ? new Dictionary<Guid, object>()
            : _bordersList.Where(e => e.Value is not null).ToDictionary(e => e.Key, e => e.Value!);
        Lines = _linesList is null
            ? new Dictionary<Guid, object>()
            : _linesList.Where(e => e.Value is not null).ToDictionary(e => e.Key, e => e.Value!);
    }
}
