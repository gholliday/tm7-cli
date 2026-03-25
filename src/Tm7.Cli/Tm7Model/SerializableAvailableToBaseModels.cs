using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[CollectionDataContract(Name = "AvailableToBaseModels", ItemName = "BaseModelId", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.ExternalStorage.OM")]
public class SerializableAvailableToBaseModels : List<string>
{
    public SerializableAvailableToBaseModels() { }

    public SerializableAvailableToBaseModels(IEnumerable<string> values) : base(values) { }
}
