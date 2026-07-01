using System.Reflection;
using Tm7.Cli.Model;

namespace Tm7.Cli;

/// <summary>
/// Provides high-level file I/O for loading and saving .tm7 threat model files.
/// </summary>
public static class Tm7File
{
    private const string DefaultTemplateResourceName = "Tm7.Cli.Resources.DefaultTemplate.tm7";

    /// <summary>
    /// Loads a .tm7 threat model from the specified file path.
    /// </summary>
    /// <param name="path">Absolute or relative path to the .tm7 file.</param>
    /// <returns>The deserialized threat model data.</returns>
    public static SerializableModelData Load(string path)
    {
        using var fs = File.OpenRead(path);
        return Tm7XmlSerializer.Deserialize(fs);
    }

    /// <summary>
    /// Loads the default template (Azure Threat Model Template KB) bundled with the tool.
    /// Used by <c>tm7 new</c> and <c>tm7 import dot</c> when no <c>--template</c> is supplied.
    /// </summary>
    public static SerializableModelData LoadDefaultTemplate()
    {
        var asm = typeof(Tm7File).Assembly;
        using var stream = asm.GetManifestResourceStream(DefaultTemplateResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{DefaultTemplateResourceName}' not found in assembly '{asm.FullName}'.");
        return Tm7XmlSerializer.Deserialize(stream);
    }

    /// <summary>
    /// Saves a threat model to the specified file path in .tm7 XML format.
    /// </summary>
    /// <param name="model">The threat model data to serialize.</param>
    /// <param name="path">Absolute or relative path for the output file.</param>
    public static void Save(SerializableModelData model, string path)
    {
        using var fs = File.Create(path);
        Tm7XmlSerializer.Serialize(fs, model);
    }
}
