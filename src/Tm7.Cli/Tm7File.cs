using Tm7.Cli.Model;

namespace Tm7.Cli;

/// <summary>
/// Provides high-level file I/O for loading and saving .tm7 threat model files.
/// </summary>
public static class Tm7File
{
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
