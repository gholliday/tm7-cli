using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "ElementType", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableElementType : SerializableExtendable
{
    [DataMember(Name = "Attributes")]
    public List<SerializableKnowledgeBaseAttribute> Attributes { get; private set; }

    [DataMember(Name = "AvailableToBaseModels")]
    public SerializableAvailableToBaseModels AvailableToBaseModels { get; private set; }

    [DataMember(Name = "Behavior")]
    public string Behavior { get; private set; }

    [DataMember(Name = "Description")]
    public string Description { get; private set; }

    [DataMember(Name = "Hidden")]
    public bool Hidden { get; private set; }

    [DataMember(Name = "Id")]
    public string Id { get; private set; }

    [DataMember(Name = "ImageLocation")]
    public string ImageLocation { get; set; }

    [DataMember(Name = "ImageSource")]
    public string ImageSource { get; private set; }

    [DataMember(Name = "ImageStream")]
    public string ImageStream { get; private set; }

    [DataMember(Name = "Name")]
    public string Name { get; private set; }

    [DataMember(Name = "ParentId")]
    public string ParentId { get; private set; }

    [DataMember(Name = "Representation")]
    public ElementVisualRepresentation Representation { get; private set; }

    [DataMember(Name = "Shape")]
    public string Shape { get; private set; }

    [DataMember(Name = "StencilConstraints")]
    public List<SerializableStencilConstraint> StencilConstraints { get; private set; }

    [DataMember(Name = "StrokeDashArray", EmitDefaultValue = false)]
    public string StrokeDashArray { get; private set; }

    [DataMember(Name = "StrokeThickness", EmitDefaultValue = false)]
    public double StrokeThickness { get; private set; }

    public SerializableElementType(bool isExtendable, string name, string id, string description,
        string parentId, string imageSource, string imageStream, bool hidden,
        ElementVisualRepresentation representation, double strokeWidth, string strokeDashArray,
        string imageLocation,
        IEnumerable<SerializableKnowledgeBaseAttribute> attributes,
        SerializableAvailableToBaseModels availableToBaseModels,
        IEnumerable<SerializableStencilConstraint> serializableStencilConstraints)
        : base(isExtendable)
    {
        Name = name;
        Id = id;
        Description = description;
        ParentId = parentId;
        ImageSource = imageSource;
        ImageStream = imageStream;
        Hidden = hidden;
        Representation = representation;
        StrokeThickness = strokeWidth;
        StrokeDashArray = strokeDashArray;
        ImageLocation = imageLocation;
        Attributes = attributes.ToList();
        AvailableToBaseModels = availableToBaseModels;
        StencilConstraints = serializableStencilConstraints.ToList();
    }
}
