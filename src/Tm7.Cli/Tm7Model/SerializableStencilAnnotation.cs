using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.StencilAnnotation, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableStencilAnnotation : SerializableStencilRectangle
{
    public SerializableStencilAnnotation(Guid guid, string typeId, string genericTypeId,
        List<SerializableDisplayAttribute> properties,
        int x, int y, int width, int height,
        double strokeThickness, string strokeDashArray)
        : base(guid, typeId, genericTypeId, properties, x, y, width, height, strokeThickness, strokeDashArray)
    {
    }
}
