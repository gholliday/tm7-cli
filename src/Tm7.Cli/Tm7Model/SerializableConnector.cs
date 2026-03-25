using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = SerializableItemTypes.Connector, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableConnector : SerializableLine
{
    public SerializableConnector(Guid guid, string typeId, string genericTypeId,
        IEnumerable<SerializableDisplayAttribute> properties,
        Guid targetGuid, Guid sourceGuid,
        StencilConnectionPort portTarget, StencilConnectionPort portSource,
        int sourceX, int sourceY, int targetX, int targetY,
        int handleX, int handleY,
        double strokeThickness, string strokeDashArray)
        : base(guid, typeId, genericTypeId, properties,
               targetGuid, sourceGuid, portTarget, portSource,
               sourceX, sourceY, targetX, targetY,
               handleX, handleY, strokeThickness, strokeDashArray)
    {
    }
}
