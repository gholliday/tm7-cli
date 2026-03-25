using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "Validation", IsReference = true, Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableValidation
{
    [DataMember(Name = "ElementGuids")]
    public List<Guid> ElementGuids { get; private set; }

    [DataMember(Name = "Enabled")]
    public bool Enabled { get; private set; }

    [DataMember(Name = "Guid")]
    public Guid Guid { get; private set; }

    [DataMember(Name = "IssueGuid")]
    public Guid IssueGuid { get; private set; }

    [DataMember(Name = "Items")]
    public List<object> Items { get; private set; }

    [DataMember(Name = "Message")]
    public string Message { get; private set; }

    [DataMember(Name = "Source")]
    public string Source { get; private set; }

    [DataMember(Name = "SourceGuid")]
    public Guid SourceGuid { get; private set; }

    public SerializableValidation(Guid guid, Guid issueGuid, IEnumerable<Guid> elementGuids,
        IEnumerable<object> items, string message, string source, Guid sourceGuid, bool enabled)
    {
        Guid = guid;
        IssueGuid = issueGuid;
        ElementGuids = elementGuids.ToList();
        Items = items.ToList();
        Message = message;
        Source = source;
        SourceGuid = sourceGuid;
        Enabled = enabled;
    }
}
