using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "Manifest", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableManifest
{
    [DataMember(Name = "Author")]
    public string Author { get; private set; }

    [DataMember(Name = "Id")]
    public Guid Id { get; private set; }

    [DataMember(Name = "Name")]
    public string Name { get; private set; }

    [DataMember(Name = "Version")]
    public string Version { get; private set; }

    public SerializableManifest(string name, Guid id, string version, string author)
    {
        Name = name;
        Id = id;
        Version = version;
        Author = author;
    }
}
