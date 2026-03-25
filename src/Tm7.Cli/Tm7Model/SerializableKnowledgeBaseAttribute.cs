using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "Attribute", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableKnowledgeBaseAttribute : SerializableExtendable
{
    [DataMember(Name = "AttributeValues")]
    public SerializableAttributeValues AttributeValues { get; private set; }

    [DataMember(Name = "DisplayName")]
    public string DisplayName { get; private set; }

    [DataMember(Name = "Inheritance")]
    public AttributeInheritance Inheritance { get; private set; }

    [DataMember(Name = "Mode")]
    public AttributeMode Mode { get; private set; }

    [DataMember(Name = "Name")]
    public string Name { get; private set; }

    [DataMember(Name = "Type")]
    public AttributeType Type { get; private set; }

    public SerializableKnowledgeBaseAttribute(bool isExtendable, string name, string displayName,
        AttributeMode mode, AttributeType type, AttributeInheritance inheritance,
        SerializableAttributeValues attributeValues)
        : base(isExtendable)
    {
        Name = name;
        DisplayName = displayName;
        Mode = mode;
        Type = type;
        Inheritance = inheritance;
        AttributeValues = attributeValues;
    }
}
