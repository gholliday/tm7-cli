using Tm7.Cli.Model;
using Tm7.Cli;
using System.Runtime.Serialization;
using System.Xml;
using Xunit;

namespace Tm7.Tests;

public class PopulatedDictionaryTests
{
    private static string GetSamplePath(string filename)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "samples", filename);
        return Path.GetFullPath(path);
    }

    private static SerializableModelData LoadTemplate() => Tm7File.Load(GetSamplePath("template.tm7"));

    private static SerializableThreat MakeThreat(int id, string title)
    {
        return new SerializableThreat(
            id: id,
            typeId: "ThreatType.GenericInformation",
            sourceGuid: Guid.NewGuid(),
            targetGuid: Guid.NewGuid(),
            flowGuid: Guid.NewGuid(),
            drawingSurfaceGuid: Guid.NewGuid(),
            state: ThreatState.NotApplicable,
            interactionKey: "key-" + id,
            priority: "High",
            wide: false,
            changedBy: "tester",
            modifiedAt: DateTime.UtcNow,
            upgraded: false,
            properties: new Dictionary<string, string>
            {
                ["Title"] = title,
                ["Description"] = "Threat description " + id,
                ["Mitigation"] = "Mitigation " + id,
            });
    }

    [Fact]
    public void Roundtrip_PopulatedThreatInstances_PreservesEntries()
    {
        var model = LoadTemplate();
        var t1 = MakeThreat(1, "Threat one");
        var t2 = MakeThreat(2, "Threat two");
        model.AllThreatsDictionary.Add("threat-1", t1);
        model.AllThreatsDictionary.Add("threat-2", t2);

        using var ms = new MemoryStream();
        Tm7XmlSerializer.Serialize(ms, model);
        ms.Position = 0;
        var reloaded = Tm7XmlSerializer.Deserialize(ms);

        Assert.Equal(2, reloaded.AllThreatsDictionary.Count);
        Assert.True(reloaded.AllThreatsDictionary.ContainsKey("threat-1"));
        Assert.True(reloaded.AllThreatsDictionary.ContainsKey("threat-2"));

        var r1 = reloaded.AllThreatsDictionary["threat-1"];
        Assert.Equal(1, r1.Id);
        Assert.Equal("key-1", r1.InteractionKey);
        Assert.Equal("High", r1.Priority);
        Assert.Equal(ThreatState.NotApplicable, r1.State);
        Assert.Equal("Threat one", r1.Properties["Title"]);
        Assert.Equal("Threat description 1", r1.Properties["Description"]);
        Assert.Equal("Mitigation 1", r1.Properties["Mitigation"]);

        var r2 = reloaded.AllThreatsDictionary["threat-2"];
        Assert.Equal(2, r2.Id);
        Assert.Equal(3, r2.Properties.Count);
    }

    [Fact]
    public void Roundtrip_PopulatedThreatProperties_PreservesEntries()
    {
        var model = LoadTemplate();
        var t = MakeThreat(42, "Detailed threat");
        t.Properties["ExtraKey"] = "ExtraValue";
        t.Properties["Empty"] = "";
        model.AllThreatsDictionary.Add("only", t);

        using var ms = new MemoryStream();
        Tm7XmlSerializer.Serialize(ms, model);
        ms.Position = 0;
        var reloaded = Tm7XmlSerializer.Deserialize(ms);

        var rt = reloaded.AllThreatsDictionary["only"];
        Assert.Equal(5, rt.Properties.Count);
        Assert.Equal("ExtraValue", rt.Properties["ExtraKey"]);
        Assert.Equal("", rt.Properties["Empty"]);
    }

    [Fact]
    public void Roundtrip_PopulatedThreats_WireFormatMatchesAutoDictionary()
    {
        // Verify our custom KVP wrapper produces the same element-name shape as the
        // built-in Dictionary<string, SerializableThreat> would under JIT-DCS. If this ever
        // diverges, files written by tm7-cli would be unreadable by the original TMT
        // tooling and by any previous build of tm7-cli that used the raw Dictionary.
        var referenceModel = new ReferenceModelWithRawDictionary
        {
            ThreatInstances = new Dictionary<string, SerializableThreat>
            {
                ["k"] = MakeThreat(7, "Reference"),
            },
        };
        var serializer = new DataContractSerializer(
            typeof(ReferenceModelWithRawDictionary),
            new[] { typeof(SerializableThreat) });
        using var refMs = new MemoryStream();
        serializer.WriteObject(refMs, referenceModel);
        var refXml = System.Text.Encoding.UTF8.GetString(refMs.ToArray());

        var ourModel = LoadTemplate();
        ourModel.AllThreatsDictionary.Clear();
        ourModel.AllThreatsDictionary.Add("k", MakeThreat(7, "Reference"));
        using var ourMs = new MemoryStream();
        Tm7XmlSerializer.Serialize(ourMs, ourModel);
        var ourXml = System.Text.Encoding.UTF8.GetString(ourMs.ToArray());

        var refItemName = ExtractKvpItemNameInside(refXml, "ThreatInstances");
        var ourItemName = ExtractKvpItemNameInside(ourXml, "ThreatInstances");
        Assert.Equal(refItemName, ourItemName);
    }

    [Fact]
    public void Roundtrip_PopulatedThreatProperties_WireFormatMatchesAutoDictionary()
    {
        // Same wire-format check for Dictionary<string, string> on SerializableThreat.Properties.
        var serializer = new DataContractSerializer(typeof(Dictionary<string, string>));
        using var refMs = new MemoryStream();
        serializer.WriteObject(refMs, new Dictionary<string, string> { ["k"] = "v" });
        var refXml = System.Text.Encoding.UTF8.GetString(refMs.ToArray());
        var refItemName = ExtractFirstKvpItemName(refXml);

        var ourModel = LoadTemplate();
        ourModel.AllThreatsDictionary.Add("only", MakeThreat(1, "T"));
        using var ourMs = new MemoryStream();
        Tm7XmlSerializer.Serialize(ourMs, ourModel);
        var ourXml = System.Text.Encoding.UTF8.GetString(ourMs.ToArray());

        // The string/string KVP element name appears verbatim inside the threat's Properties.
        // Multiple <Properties> elements exist (DisplayAttribute lists on Taggables, etc.),
        // but only the string/string dictionary contributes the KeyValueOfstringstring name.
        Assert.Contains(refItemName, ourXml);
    }

    [Fact]
    public void Roundtrip_Borders_WireFormatMatchesAutoDictionary()
    {
        // Same wire-format check for Dictionary<Guid, object> on Borders/Lines.
        var serializer = new DataContractSerializer(typeof(Dictionary<Guid, object>));
        using var refMs = new MemoryStream();
        serializer.WriteObject(refMs, new Dictionary<Guid, object> { [Guid.NewGuid()] = "x" });
        var refXml = System.Text.Encoding.UTF8.GetString(refMs.ToArray());

        var ourModel = LoadTemplate();
        using var ourMs = new MemoryStream();
        Tm7XmlSerializer.Serialize(ourMs, ourModel);
        var ourXml = System.Text.Encoding.UTF8.GetString(ourMs.ToArray());

        var refItemName = ExtractFirstKvpItemName(refXml);
        var ourItemName = ExtractKvpItemNameInside(ourXml, "Borders");
        Assert.Equal(refItemName, ourItemName);
    }

    private static string ExtractKvpItemNameInside(string xml, string parentElement)
    {
        var openIdx = xml.IndexOf("<" + parentElement, StringComparison.Ordinal);
        Assert.True(openIdx >= 0, $"Parent element <{parentElement}> not found");
        var closeIdx = xml.IndexOf("</" + parentElement + ">", openIdx, StringComparison.Ordinal);
        // Empty self-closing element has no children; require a populated case.
        Assert.True(closeIdx >= 0, $"Parent element <{parentElement}> is empty/self-closing");
        var slice = xml.Substring(openIdx, closeIdx - openIdx);
        return ExtractFirstKvpItemName(slice);
    }

    private static string ExtractFirstKvpItemName(string xml)
    {
        const string marker = "KeyValueOf";
        var idx = xml.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(idx >= 0, "No KVP item element found");
        var endIdx = xml.IndexOfAny(new[] { '>', ' ', '/' }, idx);
        return xml.Substring(idx, endIdx - idx);
    }

    [Fact]
    public void Roundtrip_MultipleAddRemoveCycles()
    {
        var model = LoadTemplate();
        var surface = model.DrawingSurfaceList[0];
        var originalBorderCount = surface.Borders.Count;

        var newGuids = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var g = Guid.NewGuid();
            newGuids.Add(g);
            surface.Borders.Add(g, new SerializableStencilEllipse(
                guid: g, typeId: "StencilEllipse", genericTypeId: "GE.P",
                properties: new List<SerializableDisplayAttribute>
                {
                    new SerializableStringDisplayAttribute("Name", "Name", $"Proc {i}")
                },
                x: 10 * i, y: 10 * i, width: 100, height: 50,
                strokeThickness: 1.0, strokeDashArray: ""));
        }

        using var ms1 = new MemoryStream();
        Tm7XmlSerializer.Serialize(ms1, model);
        ms1.Position = 0;
        var reloaded = Tm7XmlSerializer.Deserialize(ms1);
        Assert.Equal(originalBorderCount + 5, reloaded.DrawingSurfaceList[0].Borders.Count);

        // Remove 3 and save again
        for (int i = 0; i < 3; i++)
            reloaded.DrawingSurfaceList[0].Borders.Remove(newGuids[i]);

        using var ms2 = new MemoryStream();
        Tm7XmlSerializer.Serialize(ms2, reloaded);
        ms2.Position = 0;
        var reloaded2 = Tm7XmlSerializer.Deserialize(ms2);
        Assert.Equal(originalBorderCount + 2, reloaded2.DrawingSurfaceList[0].Borders.Count);
    }

    [Fact]
    public void Serialize_NullDictionaries_DoesNotThrow()
    {
        // Public setters allow null on Borders/Lines/AllThreatsDictionary; serialization
        // must not crash if a caller has nulled them out.
        var model = LoadTemplate();
        var surface = model.DrawingSurfaceList[0];
        surface.Borders = null!;
        surface.Lines = null!;
        model.AllThreatsDictionary = null!;

        using var ms = new MemoryStream();
        var ex = Record.Exception(() => Tm7XmlSerializer.Serialize(ms, model));
        Assert.Null(ex);
    }

    // Reference model class used for wire-format validation. Mirrors SerializableModelData
    // but uses the raw Dictionary<string, SerializableThreat> for the threat-instances
    // member, so DCS auto-generates its own item element name. The test compares that
    // name against the one our wrapper class emits.
    [DataContract(Name = "ThreatModel", Namespace = "http://schemas.datacontract.org/2004/07/ThreatModeling.Model")]
    public class ReferenceModelWithRawDictionary
    {
        [DataMember(Name = "ThreatInstances", Order = 5)]
        public Dictionary<string, SerializableThreat> ThreatInstances { get; set; } = new();
    }
}
