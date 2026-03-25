using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "Profile", Namespace = "")]
public class SerializableProfile
{
    [DataMember(Name = "PromptedKb", EmitDefaultValue = true, Order = 1)]
    public SerializableKbVersion PromptedKb { get; private set; }

    public SerializableProfile()
    {
        PromptedKb = new SerializableKbVersion();
    }
}
