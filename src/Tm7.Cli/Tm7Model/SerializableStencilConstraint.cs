using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "StencilConstraint", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.KnowledgeBase")]
public class SerializableStencilConstraint : SerializableExtendable
{
    [DataMember(Name = "SelectedStencilConnection")]
    public string SelectedStencilConnection { get; private set; }

    [DataMember(Name = "SelectedStencilType")]
    public string SelectedStencilType { get; private set; }

    public SerializableStencilConstraint(bool isExtendable, string selectedStencilType, string selectedStencilConnection)
        : base(isExtendable)
    {
        SelectedStencilType = selectedStencilType;
        SelectedStencilConnection = selectedStencilConnection;
    }
}
