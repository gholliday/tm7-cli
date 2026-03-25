using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "DrawingSurfaceModel", IsReference = true, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableDrawingSurfaceModel : SerializableTaggable
{
    [DataMember(Name = "Borders")]
    public Dictionary<Guid, object> Borders { get; private set; }

    [DataMember(Name = "Header")]
    public string Header { get; private set; }

    [DataMember(Name = "Lines")]
    public Dictionary<Guid, object> Lines { get; private set; }

    [DataMember(Name = "Zoom", EmitDefaultValue = false)]
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
}
