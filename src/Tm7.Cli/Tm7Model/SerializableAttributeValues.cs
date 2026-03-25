using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[CollectionDataContract(Name = "AttributeValues", ItemName = "Value", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Interfaces")]
public class SerializableAttributeValues : List<string>
{
    public SerializableAttributeValues() { }

    public SerializableAttributeValues(IEnumerable<string> values) : base(values) { }
}
