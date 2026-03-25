using System.Runtime.Serialization;

namespace Tm7.Cli.Model;

[DataContract(Name = "Note", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
public class SerializableNote
{
    [DataMember(Name = "AddedBy")]
    public string AddedBy { get; private set; }

    [DataMember(Name = "Date")]
    public DateTime Date { get; private set; }

    [DataMember(Name = "Id")]
    public int Id { get; private set; }

    [DataMember(Name = "Message")]
    public string Message { get; private set; }

    public SerializableNote(int id, string message, DateTime date, string addedBy)
    {
        Id = id;
        Message = message;
        Date = date;
        AddedBy = addedBy;
    }
}
