using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "ThreatType", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableThreatType : SerializableExtendable
{
    [DataMember(Name = "Category")]
    public string Category { get; private set; }

    [DataMember(Name = "Description")]
    public string Description { get; private set; }

    [DataMember(Name = "GenerationFilters")]
    public SerializableGenerationFilters GenerationFilters { get; private set; }

    [DataMember(Name = "Id")]
    public string Id { get; private set; }

    [DataMember(Name = "PropertiesMetaData")]
    public List<SerializableThreatMetaDatum> PropertiesMetaData { get; private set; }

    [DataMember(Name = "RelatedCategory")]
    public string RelatedCategory { get; private set; }

    [DataMember(Name = "ShortTitle")]
    public string ShortTitle { get; private set; }

    public SerializableThreatType(bool isExtendable, string id, string shortTitle,
        string category, string relatedCategory, string description,
        SerializableGenerationFilters generationFilters,
        List<SerializableThreatMetaDatum> metaData)
        : base(isExtendable)
    {
        Id = id;
        ShortTitle = shortTitle;
        Category = category;
        RelatedCategory = relatedCategory;
        Description = description;
        GenerationFilters = generationFilters;
        PropertiesMetaData = metaData;
    }
}
