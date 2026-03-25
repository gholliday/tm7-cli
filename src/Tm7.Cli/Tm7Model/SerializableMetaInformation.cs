using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "MetaInformation", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableMetaInformation
{
    [DataMember(Name = "Assumptions")]
    public string Assumptions { get; private set; }

    [DataMember(Name = "Contributors")]
    public string Contributors { get; private set; }

    [DataMember(Name = "ExternalDependencies")]
    public string ExternalDependencies { get; private set; }

    [DataMember(Name = "HighLevelSystemDescription")]
    public string HighLevelSystemDescription { get; private set; }

    [DataMember(Name = "Owner")]
    public string Owner { get; private set; }

    [DataMember(Name = "Reviewer")]
    public string Reviewer { get; private set; }

    [DataMember(Name = "ThreatModelName")]
    public string ThreatModelName { get; private set; }

    public SerializableMetaInformation(string threatModelName, string owner, string contributors,
        string reviewer, string highLevelSystemDescription, string assumptions, string externalDependencies)
    {
        ThreatModelName = threatModelName;
        Owner = owner;
        Contributors = contributors;
        Reviewer = reviewer;
        HighLevelSystemDescription = highLevelSystemDescription;
        Assumptions = assumptions;
        ExternalDependencies = externalDependencies;
    }
}
