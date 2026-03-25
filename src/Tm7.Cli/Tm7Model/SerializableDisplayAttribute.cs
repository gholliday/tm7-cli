using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

public interface IStaticDisplayAttribute { }
public interface ICustomDisplayAttribute { }

[DataContract(Name = "DisplayAttribute", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public abstract class SerializableDisplayAttribute
{
    [DataMember(Name = "DisplayName")]
    public string DisplayName { get; private set; }

    [DataMember(Name = "Name")]
    public string Name { get; private set; }

    [DataMember(Name = "Value")]
    public object Value { get; private set; }

    public SerializableDisplayAttribute(string displayName, string name, object value)
    {
        DisplayName = displayName;
        Name = name;
        Value = value;
    }
}
