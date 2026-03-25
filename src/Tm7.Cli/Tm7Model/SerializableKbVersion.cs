using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[CollectionDataContract(ItemName = "Version", Namespace = "")]
public class SerializableKbVersion : List<string>
{
}
