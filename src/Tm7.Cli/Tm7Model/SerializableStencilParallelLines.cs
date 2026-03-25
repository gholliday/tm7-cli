using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.StencilParallelLines, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableStencilParallelLines : SerializableStencilRectangle
{
    public SerializableStencilParallelLines(Guid guid, string typeId, string genericTypeId,
        IEnumerable<SerializableDisplayAttribute> properties,
        int x, int y, int width, int height,
        double strokeThickness, string strokeDashArray)
        : base(guid, typeId, genericTypeId, properties, x, y, width, height, strokeThickness, strokeDashArray)
    {
    }
}
