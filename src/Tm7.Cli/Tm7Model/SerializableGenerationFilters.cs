using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "GenerationFilters", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableGenerationFilters
{
    [DataMember(Name = "Exclude")]
    public string Exclude { get; private set; }

    [DataMember(Name = "Include")]
    public string Include { get; private set; }

    public SerializableGenerationFilters(string include, string exclude)
    {
        Include = include;
        Exclude = exclude;
    }
}
